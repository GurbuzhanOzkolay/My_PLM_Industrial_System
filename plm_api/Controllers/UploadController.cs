using Microsoft.AspNetCore.Mvc;

namespace plm_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;

        public UploadController(IWebHostEnvironment env)
        {
            _env = env;
        }

        // ==========================================
        // POST: api/Upload/upload-image
        // ==========================================
        [HttpPost("upload-image")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            // 1. Dosya boş mu kontrolü
            if (file == null || file.Length == 0)
                return BadRequest("Lütfen geçerli bir dosya seçin.");

            // 2. Sadece resim formatlarına izin ver
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(file.FileName).ToLower();

            if (!allowedExtensions.Contains(extension))
                return BadRequest("Sadece JPG, JPEG, PNG veya GIF formatında resim yükleyebilirsiniz.");

            // 3. Dosyanın kaydedileceği wwwroot/images klasör yolunu belirle
            string wwwRootPath = _env.WebRootPath;
            if (string.IsNullOrEmpty(wwwRootPath))
            {
                // Eğer wwwroot klasörü henüz otomatik algılanmadıysa el ile oluştur
                wwwRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            }

            string uploadsFolder = Path.Combine(wwwRootPath, "images");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // 4. Çakışma olmaması için benzersiz bir dosya adı üret (GUID)
            string uniqueFileName = Guid.NewGuid().ToString() + extension;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // 5. Dosyayı fiziksel olarak klasöre kaydet
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            // 6. Frontend'in veya Postman'in veritabanına kaydedebilmesi için URL'i geri dön
            // Örnek çıktı: /images/abc-123-xyz.png
            var dbImageUrl = $"/images/{uniqueFileName}";

            return Ok(new
            {
                Message = "Fotoğraf başarıyla yüklendi.",
                ImageUrl = dbImageUrl
            });
        }
    }
} 