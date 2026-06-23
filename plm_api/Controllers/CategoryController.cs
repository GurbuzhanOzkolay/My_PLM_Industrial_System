using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using plm_api.Code;
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
            var categories = _context.Categories
                .AsNoTracking()
                .Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.ParentId
                })
                .ToList();

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
        // PUT: kategori adını güncelle
        [HttpPut("update-name")]
        public IActionResult UpdateCategoryName([FromBody] CategoryUpdateNameDto updateDto)
        {
            
            if (updateDto == null)
            {
                return BadRequest("İstek verisi boş olamaz.");
            }
            var category = _context.Categories.FirstOrDefault(x => x.Id == updateDto.Id);
            if (category == null)
            {
                return NotFound(new { Message = "Güncellenmek istenen kategori bulunamadı" });
            }
            category.Name = updateDto.NewName;
            _context.SaveChanges();
            return Ok(new
            {
                Message = "Kategori adı başarıyla güncellendi.",
                CategoryId = category.Id,
                UpdatedName = category.Name
            });
        } 






    }
}
   
 

    





