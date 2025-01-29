using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace API.Controllers
{
     public class FilesController(DataContext context) : BaseApiController
    {
        private string UploadDirectory => Path.Combine(Directory.GetCurrentDirectory(), "uploads");
        private readonly HashSet<string> _allowedExtensions = new() { ".jpg", ".png" };

        // WERSJA NIEZABEZPIECZONA
        [HttpPost("upload")]
        public async Task<IActionResult> FileUpload(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Nie przesłano pliku.");

            if (!Directory.Exists(UploadDirectory))
                Directory.CreateDirectory(UploadDirectory);

            var filePath = Path.Combine(UploadDirectory, file.FileName);

            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            string checksum;
            using (var sha256 = SHA256.Create())
            {
                await using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                var hashBytes = await sha256.ComputeHashAsync(fileStream);
                checksum = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }

            var fileEntity = new FileEntity
            {
                FileName = file.FileName,
                FilePath = filePath,
                Checksum = checksum,
                SizeInBytes = file.Length,
                ContentType = file.ContentType,
                UploadedAt = DateTime.UtcNow,
                UploadedBy = User.Identity?.Name ?? " "
            };

            context.Files.Add(fileEntity);
            await context.SaveChangesAsync();

            return Ok(new { fileEntity.Id, fileEntity.FileName, fileEntity.Checksum });
        }

        // 🔒 WERSJA ZABEZPIECZONA
        [HttpPost("upload-secure")]
        public async Task<IActionResult> FileUploadSecure(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Nie przesłano pliku.");

            var fileExtension = Path.GetExtension(file.FileName).ToLower();
            if (!_allowedExtensions.Contains(fileExtension))
                return BadRequest("Nieprawidłowy format pliku. Dozwolone: .jpg, .png");

            if (!Directory.Exists(UploadDirectory))
                Directory.CreateDirectory(UploadDirectory);

            var filePath = Path.Combine(UploadDirectory, file.FileName);

            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            string checksum;
            using (var sha256 = SHA256.Create())
            {
                await using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                var hashBytes = await sha256.ComputeHashAsync(fileStream);
                checksum = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }

            var fileEntity = new FileEntity
            {
                FileName = file.FileName,
                FilePath = filePath,
                Checksum = checksum,
                SizeInBytes = file.Length,
                ContentType = file.ContentType,
                UploadedAt = DateTime.UtcNow,
                UploadedBy = User.Identity?.Name ?? " "
            };

            context.Files.Add(fileEntity);
            await context.SaveChangesAsync();

            return Ok(new { fileEntity.Id, fileEntity.FileName, fileEntity.Checksum });
        }
    }
}