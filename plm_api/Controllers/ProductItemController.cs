using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using plm_api.Code;
using plm_api.Dtos;
using Spectre.Console;
using System.ComponentModel.DataAnnotations;
using System.Data;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace plm_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductItemController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductItemController(AppDbContext context)
        {
            _context = context;
        }

        // =========================================================================
        //  ARADIĞIMIZ VE GÜNCELLEDİĞİmİZ METOT TAM OLARAK BURASI! (TÜM AĞAÇLAR)
        // =========================================================================
        [HttpGet("master-trees")]
        public IActionResult GetAllMasterProductTrees()
        {
            // 1. Adım: Veritabanındaki tüm reçete bağlarını çekiyoruz (Kategori hatası düzeltildi)
            var allItems = _context.ProductItems
                .Include(pi => pi.ChildProduct)
                    .ThenInclude(p => p.Categories)
                .ToList();

            // 2. Adım: Hiyerarşinin en tepesindeki "Ana Montaj Gruplarını" (Parentleri) buluyoruz
            var parentIds = allItems.Select(pi => pi.ParentProductId).Distinct().ToList();
            var masterProducts = _context.Products
                .Where(p => parentIds.Contains(p.Id))
                .ToList();

            // 3. Adım: Her bir ana montaj için alt ağaçları ve resim yollarını eşleştiriyoruz
            var allTrees = masterProducts.Select(masterProduct => new
            {
                TreeId = masterProduct.Id,
                TreeName = masterProduct.ProductName,
                TotalPrice = masterProduct.Price, // Canlı Roll-up maliyeti
                MinStokValue = masterProduct.MinStokValue,
                TreeImageUrl = masterProduct.ImageUrl, // Üst ürün görseli

                Products = allItems
                    .Where(pi => pi.ParentProductId == masterProduct.Id)
                    .Select(pi => new
                    {
                        Id = pi.ChildProductId,
                        Name = pi.ChildProduct != null ? pi.ChildProduct.ProductName : "Bilinmeyen Ürün",
                        Price = pi.ChildProduct != null ? pi.ChildProduct.Price : 0,
                        Quantity = pi.Quantity,
                        ImageUrl = pi.ChildProduct != null ? pi.ChildProduct.ImageUrl : null, // Alt parça görseli
                        Categories = pi.ChildProduct != null
                            ? pi.ChildProduct.Categories.Select(c => c.Name).ToList()
                            : new List<string>()
                    }).ToList()
            }).ToList();

            return Ok(allTrees);
        }

        

        [HttpPost("product-items")]
        public IActionResult AddProductItems([FromBody] ProductItemsRequestDto request) 
        {
            if (request == null) return BadRequest("İstek verisi boş olamaz.");

            var parentExists = _context.Products.Any(p => p.Id == request.ParentProductId);
            var childExists = _context.Products.Any(p => p.Id == request.ChildProductId);

            if (!parentExists || !childExists)
                return NotFound("Ana ürün veya alt ürün bulunamadı.");

            var treeElement = new Code.ProductItem
            {
                ParentProductId = request.ParentProductId,
                ChildProductId = request.ChildProductId,
                Quantity = request.Quantity
            };

            _context.ProductItems.Add(treeElement);
            _context.SaveChanges();

            // Yeni parça eklendiğinde üst maliyeti tetikle
            RecalculateParentCostRecursive(request.ParentProductId);

            return Ok(new { Message = "Alt bileşen ürün ağacına başarıyla eklendi." });
        }

        [HttpGet("{id}")]
        public IActionResult GetMasterProductTree(int id)
        {
            var masterProduct = _context.Products.FirstOrDefault(p => p.Id == id);
            if (masterProduct == null) return NotFound("Ürün ağacı bulunamadı.");

            var ProductItems = _context.ProductItems
                .Include(pt => pt.ChildProduct)
                    .ThenInclude(p => p.Categories)
                .Where(pt => pt.ParentProductId == id)
                .Select(pt => new
                {
                    Id = pt.ChildProductId,
                    Name = pt.ChildProduct != null ? pt.ChildProduct.ProductName : "Bilinmeyen Ürün",
                    Price = pt.ChildProduct != null ? pt.ChildProduct.Price : 0,
                    Quantity = pt.Quantity,
                    ImageUrl = pt.ChildProduct != null ? pt.ChildProduct.ImageUrl : null, // Görsel buraya da eklendi
                    Categories = pt.ChildProduct != null ? pt.ChildProduct.Categories.Select(c => c.Name).ToList() : new List<string>()
                });

            var efQueryString = ProductItems.ToQueryString();
            System.Diagnostics.Debug.WriteLine($"\n[VS OUTPUT LOG] SQL SORGUSU:\n{efQueryString}\n");

            AnsiConsole.Write(
                new Panel(new Markup($"[cyan]{Markup.Escape(efQueryString)}[/]"))
                    .Header("[bold yellow] Ef core sorgu basma [/]")
                    .BorderColor(Color.Yellow)
            );

            Console.WriteLine(efQueryString);

            var ProductsItems = ProductItems.ToList();

            return Ok(new
            {
                Id = masterProduct.Id,
                TreeName = masterProduct.ProductName,
                TotalPrice = masterProduct.Price,
                Products = ProductItems
            });
        }

        [HttpGet("sql")]
        [ApiExplorerSettings(IgnoreApi = true)] //ignore 
        public IActionResult GetProductTreeSql(int id)
            
        {
            string masterQuery = "SELECT Id, ProductName, Price FROM Products WHERE Id = @MasterId";
            string sqlQuery = @"
        SELECT 
            pt.ChildProductId AS ProductItemId,
            p.ProductName AS ProductItemName,
            p.Price AS ProductItemPrice,
            pt.Quantity AS ItemQuantity, 
            p.ImageUrl AS ProductItemImageUrl,
            (SELECT STRING_AGG(c.Name, '; ') 
             FROM CategoryProducts cp -- 
             INNER JOIN Categories c ON cp.CategoriesId = c.Id 
             WHERE cp.ProductsId = p.Id) AS CategoryNames
        FROM ProductItems pt
        INNER JOIN Products p ON pt.ChildProductId = p.Id
        WHERE pt.ParentProductId = @MasterId"; 

            var connectionString = _context.Database.GetConnectionString();
            var ProductItemsList = new List<object>();

            int masterId = 0;
            string masterName = "";
            decimal masterPrice = 0;
            bool masterExists = false;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (SqlCommand masterCmd = new SqlCommand(masterQuery, connection))
                {
                    masterCmd.Parameters.AddWithValue("@MasterId", id);
                    using (SqlDataReader masterReader = masterCmd.ExecuteReader())
                    {
                        if (masterReader.Read())
                        {
                            masterId = masterReader.GetInt32(masterReader.GetOrdinal("Id"));
                            masterName = masterReader.GetString(masterReader.GetOrdinal("ProductName"));
                            masterPrice = masterReader.GetDecimal(masterReader.GetOrdinal("Price"));
                            masterExists = true;
                        }
                    }
                }

                if (!masterExists) return NotFound("Ürün bulunamadı.");

                using (SqlCommand subCmd = new SqlCommand(sqlQuery, connection))
                {
                    subCmd.Parameters.AddWithValue("@MasterId", id);
                    using (SqlDataReader subReader = subCmd.ExecuteReader())
                    {
                        while (subReader.Read())
                        {
                            var categoryRaw = subReader.IsDBNull(subReader.GetOrdinal("CategoryNames"))
                                ? ""
                                : subReader.GetString(subReader.GetOrdinal("CategoryNames"));

                            var categoryList = string.IsNullOrEmpty(categoryRaw)
                                ? new List<string>()
                                : categoryRaw.Split(';').Select(c => c.Trim()).ToList();

                            ProductItemsList.Add(new
                            {
                                Id = subReader.GetInt32(subReader.GetOrdinal("ProductItemId")),
                                Name = subReader.GetString(subReader.GetOrdinal("ProductItemName")),
                                Price = subReader.GetDecimal(subReader.GetOrdinal("ProductItemPrice")),
                                Quantity = subReader.GetDecimal(subReader.GetOrdinal("ItemQuantity")),
                                ImageUrl = subReader.IsDBNull(subReader.GetOrdinal("ProductItemImageUrl")) ? null : subReader.GetString(subReader.GetOrdinal("ProductItemImageUrl")),
                                Categories = categoryList
                            });
                        }
                    }
                }
            }

            return Ok(new
            {
                Id = masterId,
                TreeName = masterName,
                TotalPrice = masterPrice,
                Products = ProductItemsList
            });
        }

        [HttpPut("update-quantity")]
        public IActionResult UpdateSubProductQuantity([FromBody] UpdateQuantityRequest request)
        {
            if (request == null) return BadRequest("İstek verisi boş olamaz.");

            var treeElement = _context.ProductItems
                .FirstOrDefault(pt => pt.ParentProductId == request.ParentProductId && pt.ChildProductId == request.ChildProductId);

            if (treeElement == null)
                return NotFound("Belirtilen ana ürün ve alt ürün ilişkisi ürün ağacında bulunamadı.");

            treeElement.Quantity = request.NewQuantity;
            _context.SaveChanges();

            // Maliyetleri yukarı doğru tetikle
            RecalculateParentCostRecursive(request.ParentProductId);

            return Ok(new { Message = "Bileşen miktarı başarıyla güncellendi.", UpdatedQuantity = request.NewQuantity });
        }

        [HttpDelete("child-item")]
        public IActionResult DeleteProductItem(int parentProductId, int childProductId)
        {
            var productItem = _context.ProductItems
                .FirstOrDefault(pi => pi.ParentProductId == parentProductId && pi.ChildProductId == childProductId);

            if (productItem == null)
            {
                return NotFound("Belirtilen ana ürün ve alt ürün ilişkisi ürün ağacında bulunamadı.");
            }

            _context.ProductItems.Remove(productItem);
            _context.SaveChanges();

            // Parça silindiğinde maliyetleri yeniden hesapla
            RecalculateParentCostRecursive(parentProductId);

            return Ok(new { Message = "Alt ürün başarıyla ürün ağacından silindi." });
        }

        // 🔥 RECURSIVE MALİYET HESAPLAMA METODU
        private void RecalculateParentCostRecursive(int parentProductId)
        {
            var subComponents = _context.ProductItems
                .Include(pi => pi.ChildProduct)
                .Where(pi => pi.ParentProductId == parentProductId)
                .ToList();

            if (!subComponents.Any()) return;

            decimal calculatedTotalCost = 0;
            foreach (var item in subComponents)
            {
                if (item.ChildProduct != null)
                {
                    calculatedTotalCost += item.ChildProduct.Price * item.Quantity;
                }
            }

            var parentProduct = _context.Products.FirstOrDefault(p => p.Id == parentProductId);
            if (parentProduct != null)
            {
                parentProduct.Price = calculatedTotalCost;
                _context.SaveChanges();

                var upperRelation = _context.ProductItems.FirstOrDefault(pi => pi.ChildProductId == parentProductId);
                if (upperRelation != null)
                {
                    RecalculateParentCostRecursive(upperRelation.ParentProductId);
                }
            }
        }

       

      
    }
}