using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace plm_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CategoryController(AppDbContext context)
        {
            _context = context;
        }

        // GET: tüm kategoriler
        [HttpGet]
        public IActionResult Get()
        {
            var categories = _context.Categories.ToList();
            return Ok(categories);
        }

        // POST: kategori ekle
        [HttpPost]
        public IActionResult Add([FromBody] Category category)
        {
            _context.Categories.Add(category);
            _context.SaveChanges();
            return Ok(category);
        }

        // DELETE: kategori sil
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var category = _context.Categories.FirstOrDefault(x => x.Id == id);

            if (category == null)
                return NotFound("Kategori bulunamadı");

            _context.Categories.Remove(category);
            _context.SaveChanges();

            return Ok("Kategori silindi");
        }

        // TREE (opsiyonel ama çok önemli)
        [HttpGet("tree")]
        public IActionResult GetTree()
        {
            var categories = _context.Categories.ToList();

            var tree = categories
                .Where(x => x.ParentId == null)
                .Select(x => new
                {
                    x.Id,
                    x.Name,
                    Children = GetChildren(categories, x.Id)
                });

            return Ok(tree);
        }

        private List<object> GetChildren(List<Category> categories, int parentId)
        {
            return categories
                .Where(x => x.ParentId == parentId)
                .Select(x => new
                {
                    x.Id,
                    x.Name,
                    Children = GetChildren(categories, x.Id)
                })
                .Cast<object>()
                .ToList();
        }
    }
}