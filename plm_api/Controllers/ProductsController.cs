using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using plm_api.Code;
using plm_api.Dtos;

namespace plm_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductsController(AppDbContext context)
        {
            _context = context;
        }

        // =========================
        // GET: tüm ürünler
        // =========================
        [HttpGet]
        public IActionResult Get()
        {
            var products = _context.Products
                .Include(x => x.Categories)
                .ToList();

            var result = products.Select(p => new
            {
                p.Id,
                p.ProductName,
                p.Price,
                p.Stt_Date,
                p.MinStokValue,
                Categories = p.Categories.Select(c => new { c.Id, c.Name })
            });

            return Ok(result);
        }

        // =========================
        // GET: tek ürün
        // =========================
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var product = _context.Products
                .Include(x => x.Categories)
                .FirstOrDefault(x => x.Id == id);

            if (product == null)
                return NotFound("Ürün bulunamadı");

            return Ok(new
            {
                product.Id,
                product.ProductName,
                product.Price,
                product.Stt_Date,
                product.MinStokValue,
                Categories = product.Categories.Select(c => new { c.Id, c.Name })
            });
        }

        // =========================
        // POST: ürün ekle
        // =========================
        [HttpPost]
        public IActionResult AddProduct(ProductCreateDto dto)
        {
            var product = new Products
            {
                ProductName = dto.ProductName,
                Price = dto.Price,
                Stt_Date = dto.Stt_Date,
                MinStokValue = dto.MinStokValue,
            };

            if (dto.CategoryIds != null && dto.CategoryIds.Count > 0)
            {
                var categories = _context.Categories
                    .Where(c => dto.CategoryIds.Contains(c.Id))
                    .ToList();

                product.Categories = categories;
            }

            _context.Products.Add(product);
            _context.SaveChanges();

            return Ok(new
            {
                product.Id,
                product.ProductName,
                product.Price,
                product.Stt_Date,
                product.MinStokValue,
                Categories = product.Categories.Select(c => new { c.Id, c.Name })
            });
        }

        // Çoklu ürün ekleme
        [HttpPost("bulk")]
        [ApiExplorerSettings(IgnoreApi = true)]//İGNORE
        public IActionResult AddProducts([FromBody] List<Products> products)
        {
            _context.Products.AddRange(products);
            _context.SaveChanges();

            return Ok(products.Count + " ürün eklendi");
        }

        // ====================================================
        // PUT: ürün güncelle - (DİNAMİK MALİYET TETİKLEYİCİLİ!)
        // ====================================================
        [HttpPut("{id}")]
        public IActionResult UpdateProduct(int id, ProductCreateDto dto)
        {
            var product = _context.Products
                .Include(x => x.Categories)
                .FirstOrDefault(x => x.Id == id);

            if (product == null)
                return NotFound("Ürün bulunamadı");

            // Fiyatın değişip değişmediğini kontrol etmek için eski fiyatı saklıyoruz
            decimal oldPrice = product.Price;

            product.ProductName = dto.ProductName;
            product.Price = dto.Price;
            product.MinStokValue = dto.MinStokValue;
            product.Stt_Date = dto.Stt_Date;
            product.ImageUrl = dto.ImageUrl;

            if (dto.CategoryIds != null)
            {
                var categories = _context.Categories
                    .Where(c => dto.CategoryIds.Contains(c.Id))
                    .ToList();

                product.Categories.Clear();
                foreach (var cat in categories)
                    product.Categories.Add(cat);
            }

            _context.SaveChanges();

            // 🔥 CANLI TETİKLEME: Eğer parçanın birim fiyatı değiştiyse, 
            // bu parçanın bağlı olduğu bir üst montaj (Parent) var mı diye bakıyoruz.
            if (oldPrice != dto.Price)
            {
                var parentRelations = _context.ProductItems
                    .Where(pi => pi.ChildProductId == id)
                    .ToList();

                // Bu parça birden fazla reçetede alt bileşen olabilir, hepsini tek tek yukarı doğru güncelliyoruz
                foreach (var relation in parentRelations)
                {
                    RecalculateParentCostRecursive(relation.ParentProductId);
                }
            }

            return Ok(new
            {
                product.Id,
                product.ProductName,
                product.Price,
                product.Stt_Date,
                product.MinStokValue,
                Categories = product.Categories.Select(c => new { c.Id, c.Name })
            });
        }

        // Ürün json kullanarak stt tarihi değişimi
        [HttpPut("update-stt-json")]
        [ApiExplorerSettings(IgnoreApi = true)]//İGNORE
        public IActionResult ProducsSttUpdateWithJson([FromBody] ProductUpdateSttDateDto updateDto)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == updateDto.ProductId);

            if (product == null)
            {
                return NotFound(new { message = "Ürün bulunamadı!" });
            }

            product.Stt_Date = updateDto.NewStt_Date;
            _context.SaveChanges();

            return Ok(new
            {
                message = "Ürünün Son Tüketim Tarihi başarıyla güncellendi.",
                productId = product.Id,
                newStt = product.Stt_Date
            });
        }

        // =========================
        // DELETE: ürün sil
        // =========================
        [HttpDelete("{id}")]
        public IActionResult DeleteProduct(int id)
        {
            var product = _context.Products.FirstOrDefault(x => x.Id == id);

            if (product == null)
                return NotFound("Ürün bulunamadı");

            // Silinmeden önce bu ürünün üst montaj bağlarını buluyoruz
            var parentRelations = _context.ProductItems
                .Where(pi => pi.ChildProductId == id)
                .ToList();

            _context.Products.Remove(product);
            _context.SaveChanges();

            // 🔥 Parça silindiği için üst montajların maliyetinden bu parçayı düşürmek üzere yeniden hesaplama tetikliyoruz
            foreach (var relation in parentRelations)
            {
                RecalculateParentCostRecursive(relation.ParentProductId);
            }

            return Ok("Ürün silindi");
        }

        // =========================================================================
        // 🔥 PRODUCTS CONTROLLER İÇİN ROLL-UP COST HESAPLAMA YARDIMCI FONKSİYONU
        // =========================================================================
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

                // Üst kademeye doğru zincirleme özyineleme devam ediyor...
                var upperRelation = _context.ProductItems.FirstOrDefault(pi => pi.ChildProductId == parentProductId);
                if (upperRelation != null)
                {
                    RecalculateParentCostRecursive(upperRelation.ParentProductId);
                }
            }
        }
        [HttpPut("BulkUpdateImageUrls")]
        [ApiExplorerSettings(IgnoreApi = true)] //ignore 
        public IActionResult BulkUpdateImageUrls([FromBody] List<BulkImageUpdateDto> updateList)
        {
            if (updateList == null || !updateList.Any())
            {
                return BadRequest("Güncellenecek veri listesi boş olamaz.");
            }

            // 1. Veritabanındaki tüm ürünleri hafızaya çekiyoruz (Hızlı eşleşme için)
            var allProducts = _context.Products.ToList();
            int updatedCount = 0;

            // 2. Gelen listedeki her bir elemanı dönüyoruz
            foreach (var item in updateList)
            {
                // Veritabanında eşleşen ürünü bul (Hem TreeId'ler hem de alt ürünlerin Id'leri için)
                var product = allProducts.FirstOrDefault(p => p.Id == item.Id);

                if (product != null)
                {
                    product.ImageUrl = item.ImageUrl;
                    updatedCount++;
                }
            }

            // 3. Değişiklikleri tek bir seferde SQL Server'a kaydediyoruz (Bulk Save)
            _context.SaveChanges();

            return Ok(new { Message = $"{updatedCount} adet ürünün görsel linki toplu olarak başarıyla güncellendi." });
        }

        [HttpPut("convert-image-urls-to-base64")]
        [ApiExplorerSettings(IgnoreApi = true)] //ignore
        public async Task<IActionResult> ConvertImageUrlsToBase64()
        {
            var products = _context.Products
                .Where(p => p.ImageUrl != null && p.ImageUrl.StartsWith("http"))
                .ToList();

            if (!products.Any())
            {
                return Ok("Base64'e çevrilecek URL bulunamadı.");
            }

            using var httpClient = new HttpClient();

            // Bazı görsel linkleri User-Agent olmadan resmi vermeyebilir.
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0");

            int successCount = 0;
            int failCount = 0;

            foreach (var product in products)
            {
                try
                {
                    var response = await httpClient.GetAsync(product.ImageUrl);

                    if (!response.IsSuccessStatusCode)
                    {
                        failCount++;
                        continue;
                    }

                    var imageBytes = await response.Content.ReadAsByteArrayAsync();
                    var base64String = Convert.ToBase64String(imageBytes);

                    var contentType = response.Content.Headers.ContentType?.MediaType;

                    if (string.IsNullOrWhiteSpace(contentType))
                    {
                        contentType = "image/png";
                    }

                    product.ImageUrl = $"data:{contentType};base64,{base64String}";

                    successCount++;
                }
                catch
                {
                    failCount++;
                }
            }

            _context.SaveChanges();

            return Ok(new
            {
                Message = "Görsel URL'leri Base64 formatına çevrildi.",
                SuccessCount = successCount,
                FailCount = failCount
            });
        } 


    }
}