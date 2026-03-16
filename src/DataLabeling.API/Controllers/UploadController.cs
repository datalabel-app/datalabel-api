using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO.Compression;

namespace DataLabeling.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        private readonly Cloudinary _cloudinary;

        public UploadController(IConfiguration config)
        {
            var account = new Account(
                config["Cloudinary:CloudName"],
                config["Cloudinary:ApiKey"],
                config["Cloudinary:ApiSecret"]
            );

            _cloudinary = new Cloudinary(account);
        }


        [HttpPost("image")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File không hợp lệ");

            using var stream = file.OpenReadStream();

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = "datalabeling"
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.Error != null)
                return BadRequest(uploadResult.Error.Message);

            return Ok(new
            {
                url = uploadResult.SecureUrl.ToString(),
                publicId = uploadResult.PublicId
            });
        }


        [HttpPost("images")]
        public async Task<IActionResult> UploadImages(List<IFormFile> files)
        {
            if (files == null || files.Count == 0)
                return BadRequest("Không có file nào");

            var results = new List<object>();

            foreach (var file in files)
            {
                if (file.Length == 0) continue;

                using var stream = file.OpenReadStream();

                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Folder = "datalabeling"
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                if (uploadResult.Error != null)
                    return BadRequest(uploadResult.Error.Message);

                results.Add(new
                {
                    url = uploadResult.SecureUrl.ToString(),
                    publicId = uploadResult.PublicId
                });
            }

            return Ok(results);
        }

        [HttpPost("zip")]
        public async Task<IActionResult> UploadZip(IFormFile zipFile)
        {
            if (zipFile == null || zipFile.Length == 0)
                return BadRequest("Zip file không hợp lệ");

            var results = new List<object>();
            using var zipStream = zipFile.OpenReadStream();
            using var archive = new ZipArchive(zipStream, ZipArchiveMode.Read);

            foreach (var entry in archive.Entries)
            {
                if (entry.Length == 0 || string.IsNullOrEmpty(entry.Name))
                    continue;

                var extension = Path.GetExtension(entry.Name).ToLower();

                try
                {
                    using var entryStream = entry.Open();
                    UploadResult? uploadResult = null;
                    if (extension == ".jpg" || extension == ".jpeg" || extension == ".png" || extension == ".webp")
                    {
                        var uploadParams = new ImageUploadParams
                        {
                            File = new FileDescription(entry.Name, entryStream),
                            Folder = "datalabeling"
                        };

                        uploadResult = await _cloudinary.UploadAsync(uploadParams);
                    }
                    else if (extension == ".mp4" || extension == ".mov" || extension == ".avi"
                          || extension == ".mp3" || extension == ".wav")
                    {
                        var uploadParams = new VideoUploadParams
                        {
                            File = new FileDescription(entry.Name, entryStream),
                            Folder = "datalabeling"
                        };

                        uploadResult = await _cloudinary.UploadAsync(uploadParams);
                    }
                    else continue;

                    if (uploadResult == null || uploadResult.Error != null)
                        return BadRequest(uploadResult?.Error?.Message ?? "Upload thất bại");

                    results.Add(new
                    {
                        fileName = entry.Name,
                        url = uploadResult.SecureUrl?.ToString(),
                        publicId = uploadResult.PublicId
                    });
                }
                catch (Exception ex)
                {
                    return BadRequest($"Lỗi khi upload file {entry.Name}: {ex.Message}");
                }
            }

            return Ok(results);
        }
    }
}
