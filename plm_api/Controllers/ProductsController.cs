/*using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace plm_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {


        [HttpGet(Name = "GetProducts")]
        public ProductsInfo Get()
        {
            ProductsInfo product = new ProductsInfo();
            product.ProductName = "çikolata";
            product.MinStokValue= 0;
            product.Stt_Date = DateTime.Now.AddDays(30);
            product.Id = 1;
            product.Price = 20;
            return product;
        }
        [HttpPost(Name = "PostProducts")]
        public string Post(string name)
        {
            return " "+name;
        }
    }
}
*/



/*using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
namespace plm_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        // MEMORY DATABASE (geçici liste)
        private static List<ProductsInfo> products = new List<ProductsInfo>();

        // =========================
        // GET: tüm ürünleri getir
        // =========================
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(products);
        }

        // =========================
        // GET: tek ürün getir (id ile)
        // =========================
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var product = products.FirstOrDefault(x => x.Id == id);

            if (product == null)
                return NotFound("Ürün bulunamadı");

            return Ok(product);
        }

        // =========================
        // POST: ürün ekle
        // =========================
        [HttpPost]
        public IActionResult AddProduct(ProductsInfo product)
        {
            products.Add(product);
            return Ok(product);
        }

        // =========================
        // PUT: ürün güncelle
        // =========================
        [HttpPut("{id}")]
        public IActionResult UpdateProduct(int id, ProductsInfo updatedProduct)
        {
            var product = products.FirstOrDefault(x => x.Id == id);

            if (product == null)
                return NotFound("Ürün bulunamadı");

            product.ProductName = updatedProduct.ProductName;
            product.Price = updatedProduct.Price;
            product.MinStokValue = updatedProduct.MinStokValue;
            product.Stt_Date = updatedProduct.Stt_Date;

            return Ok(product);
        }

        // =========================
        // DELETE: ürün sil
        // =========================
        [HttpDelete("{id}")]
        public IActionResult DeleteProduct(int id)
        {
            var product = products.FirstOrDefault(x => x.Id == id);

            if (product == null)
                return NotFound("Ürün bulunamadı");

            products.Remove(product);

            return Ok("Ürün silindi");
        }
    }
}
*/

/*--------------------------------------------------------------
/*--------------------------------------------------------------
/*--------------------------------------------------------------
/*--------------------------------------------------------------
/*--------------------------------------------------------------
/*--------------------------------------------------------------
/*--------------------------------------------------------------
/*--------------------------------------------------------------
/*--------------------------------------------------------------
/*--------------------------------------------------------------
/*--------------------------------------------------------------
/*--------------------------------------------------------------
/*--------------------------------------------------------------
/*--------------------------------------------------------------

 */
//MSSQL Sistemine aktardıktan sonra controller kodlarımız(Artık verilerimiz kalıcı)
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace plm_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;

        // Constructor (Dependency Injection)
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
            var products = _context.Products.ToList();
            return Ok(products);
        }

        // =========================
        // GET: tek ürün
        // =========================
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var product = _context.Products.FirstOrDefault(x => x.Id == id);

            if (product == null)
                return NotFound("Ürün bulunamadı");

            return Ok(product);
        }
        //ÜRÜN İÇERİĞİ ÇEKME 
        [HttpGet("ingredients")]
        public IActionResult GetIngredients()
        {
            var result = _context.Products
                .Select(x => new ProductIngredientsDto
                {
                    ProductName = x.ProductName,
                    Ingredients = x.Ingredients
                })
                .ToList();

            return Ok(result);
        }
        //İD NUMBER A GÖRE İÇİNDEKİLER KISMINI ÇEKMEK 
        [HttpGet("{id}/ingredients")]
        public IActionResult GetIngredientsById(int id)
        {
            var product = _context.Products
                .Where(x => x.Id == id)
                .Select(x => new
                {
                    x.Id,
                    x.ProductName,
                    x.Ingredients
                })
                .FirstOrDefault();

            if (product == null)
                return NotFound("Ürün bulunamadı");

            return Ok(product);
        }

        // =========================
        // POST: ürün ekle
        // =========================

         [HttpPost]
         public IActionResult AddProduct(Products product)
         {
             _context.Products.Add(product);
             _context.SaveChanges();

             return Ok(product);
         }
        
        //Çoklu ürün ekleme 
        [HttpPost("bulk")]
        public IActionResult AddProducts([FromBody] List<Products> products)
        {
            _context.Products.AddRange(products);
            _context.SaveChanges();

            return Ok(products.Count + " ürün eklendi");
        }


        // =========================
        // PUT: ürün güncelle
        // =========================

        [HttpPut("{id}")]
        public IActionResult UpdateProduct(int id, Products updated)
        {
            var product = _context.Products.FirstOrDefault(x => x.Id == id);

            if (product == null)
                return NotFound("Ürün bulunamadı");

            product.ProductName = updated.ProductName;
            product.Price = updated.Price;
            product.MinStokValue = updated.MinStokValue;
            product.Stt_Date = updated.Stt_Date;
            
            _context.SaveChanges();

            return Ok(product);
        }
        //Ürün stt tarihini id yazarak değiş(Yalnızca stt tarihi değişir)
        [HttpPut("{id}/stt-date")]
        public IActionResult UpdateSttDate(int id, DateTime newDate)
        {
            var product = _context.Products.FirstOrDefault(x => x.Id == id);

            if (product == null)
                return NotFound("Ürün bulunamadı");

            product.Stt_Date = newDate;

            _context.SaveChanges();
            //HANGİ SORGUYLA UPDATE OLDU

            return Ok(new
            {
                Message = "Stt tarihi güncellendi",
                product.Id,
                product.ProductName,
                product.Stt_Date
            });
        }

        //Ürün json kullanarak stt tarihi değişimi 

        /*[HttpPut("{id}/stt-date-json")]
        public IActionResult UpdateSttDate(int id, [FromBody] UpdateSttDateDto dto)
        {
            var product = _context.Products.FirstOrDefault(x => x.Id == id);
            if (product == null)
                return NotFound("Ürün bulunamadı");
            product.Stt_Date = dto.Stt_Date;
            _context.SaveChanges();
            return Ok(new
            {
                Message = "STT tarihi güncellendi",
                product.Id,
                product.ProductName,
                product.Stt_Date
            });
        }
        */

        // =========================
        // DELETE: ürün sil
        // =========================
        [HttpDelete("{id}")]
        public IActionResult DeleteProduct(int id)
        {
            var product = _context.Products.FirstOrDefault(x => x.Id == id);

            if (product == null)
                return NotFound("Ürün bulunamadı");

            _context.Products.Remove(product);
            _context.SaveChanges();

            return Ok("Ürün silindi");
        }

        /*[HttpPost("category")]
        public IActionResult AddCategory(Category category)
        {
            _context.Categories.Add(category);
            _context.SaveChanges();

            return Ok(category);
        }
        */
        [HttpGet("category/root")]
        public IActionResult GetRootCategories()
        {
            var roots = _context.Categories
                .Where(x => x.ParentId == null)
                .ToList();

            return Ok(roots);
        }
        [HttpGet("category/tree")]
        public IActionResult GetTree()
        {
            var tree = _context.Categories
                .Include(x => x.Children)
                .Where(x => x.ParentId == null)
                .ToList();

            return Ok(tree);
        }
        [HttpPost("category")]
        public IActionResult AddCategory([FromBody] Category category)
        {
            _context.Categories.Add(category);
            _context.SaveChanges();
            return Ok(category);
        }

        
        // 1. METOT: - TÜM PARAMETRELER JSON'DAN GELİR
        [HttpPut("update-stt-json")]
        public IActionResult ProducsSttUpdateWithJson([FromBody] ProductUpdateSttDateDto updateDto)
        {
            // DTO'daki 'ProductId' kullanılıyor
            var product = _context.Products.FirstOrDefault(p => p.Id == updateDto.ProductId);

            if (product == null)
            {
                return NotFound(new { message = "Ürün bulunamadı!" });
            }

            // DTO'daki 'NewStt_Date' kullanılıyor
            product.Stt_Date = updateDto.NewStt_Date;
            _context.SaveChanges();

            return Ok(new
            {
                message = "Ürünün Son Tüketim Tarihi başarıyla güncellendi.",
                productId = product.Id,
                newStt = product.Stt_Date
            });
        }

        

    }
}

