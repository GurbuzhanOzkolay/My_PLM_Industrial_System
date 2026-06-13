using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace plm_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductTreeController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductTreeController(AppDbContext context)
        {
            _context = context;
        }

        //  Category + Products Tree
        [HttpGet("tree")]
        public IActionResult GetProductTree()
        {
            var products = _context.Products
                .Include(x => x.Category)
                .ToList();

            var tree = products
                .GroupBy(x => x.Category != null ? x.Category.Name : "Kategori Yok")
                .Select(g => new
                {
                    Category = g.Key,
                    Products = g.Select(p => new
                    {
                        p.Id,
                        p.ProductName,
                        p.Price,
                        p.Stt_Date,
                        p.MinStokValue
                    })
                });

            return Ok(tree);
        }
    }
}