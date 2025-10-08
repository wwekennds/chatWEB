using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.StaticFiles;

namespace Chat.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FileController : Controller
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<FileController> _logger;

        public FileController(IWebHostEnvironment environment, ILogger<FileController> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            try
            {
                _logger.LogInformation("Начало загрузки файла");

                if (file == null || file.Length == 0)
                {
                    _logger.LogWarning("Файл не выбран или пустой");
                    return BadRequest("Файл не выбран");
                }

                _logger.LogInformation($"Файл: {file.FileName}, Размер: {file.Length} bytes");

                // Ограничение размера файла (10MB)
                if (file.Length > 10 * 1024 * 1024)
                {
                    _logger.LogWarning($"Файл слишком большой: {file.Length} bytes");
                    return BadRequest("Файл слишком большой (макс. 10MB)");
                }

                // Создаем папку uploads если ее нет
                var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads");
                _logger.LogInformation($"Путь для загрузки: {uploadsPath}");

                if (!Directory.Exists(uploadsPath))
                {
                    Directory.CreateDirectory(uploadsPath);
                    _logger.LogInformation("Создана папка uploads");
                }

                // Генерируем безопасное имя файла
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                var filePath = Path.Combine(uploadsPath, fileName);

                _logger.LogInformation($"Сохраняем файл как: {fileName}");

                // Сохраняем файл
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                _logger.LogInformation($"Файл успешно сохранен: {filePath}");

                // Возвращаем информацию о файле
                var fileInfo = new
                {
                    fileName = file.FileName,
                    fileUrl = $"/uploads/{fileName}",
                    fileSize = file.Length,
                    uploadDate = DateTime.Now
                };

                _logger.LogInformation("Файл успешно загружен");
                return Ok(fileInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка загрузки файла");
                return StatusCode(500, $"Ошибка загрузки: {ex.Message}");
            }
        }

        [HttpGet("download/{fileName}")]
        public IActionResult DownloadFile(string fileName)
        {
            try
            {
                var filePath = Path.Combine(_environment.WebRootPath, "uploads", fileName);
                
                if (!System.IO.File.Exists(filePath))
                    return NotFound("Файл не найден");

                var fileBytes = System.IO.File.ReadAllBytes(filePath);
                var contentType = GetContentType(filePath);
                
                return File(fileBytes, contentType, Path.GetFileName(filePath));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка скачивания файла");
                return StatusCode(500, $"Ошибка загрузки: {ex.Message}");
            }
        }

        private string GetContentType(string path)
        {
            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(path, out string? contentType))
            {
                contentType = "application/octet-stream";
            }
            return contentType;
        }
    }
}