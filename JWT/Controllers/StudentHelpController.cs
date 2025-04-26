using Edu_plat.Model;
using JWT.DATA;
using JWT;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Edu_plat.DTO.AdminFiles;

namespace Edu_plat.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class StudentHelpController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly UserManager<ApplicationUser> _userManager;

        public StudentHelpController(ApplicationDbContext context, IWebHostEnvironment hostingEnvironment, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
            _userManager = userManager;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UploadFile([FromForm] FileDto fileDto)
        {
            if (fileDto.File == null || fileDto.File.Length == 0)
                return BadRequest(new { success = false, message = "No file uploaded." });

            if (Path.GetExtension(fileDto.File.FileName).ToLower() != ".pdf")
                return BadRequest(new { success = false, message = "Only PDF files are allowed." });

            if (string.IsNullOrWhiteSpace(fileDto.FileName))
                return BadRequest(new { success = false, message = "File name cannot be empty." });

            // Ensure the filename has a .pdf extension
            string fileNameWithExtension = Path.HasExtension(fileDto.FileName)
                ? fileDto.FileName
                : $"{fileDto.FileName}.pdf";

            // Define upload directory
            var uploadsDir = Path.Combine(_hostingEnvironment.WebRootPath, "Uploads", "AdminFiles");
            if (!Directory.Exists(uploadsDir))
                Directory.CreateDirectory(uploadsDir);

            var filePath = Path.Combine(uploadsDir, fileNameWithExtension);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await fileDto.File.CopyToAsync(stream);
            }

            var fileRecord = new AdminFile
            {
                FileName = fileNameWithExtension, // Store custom filename
                FilePath = $"/Uploads/AdminFiles/{fileNameWithExtension}",
                uploadeDate = DateTime.UtcNow,
                size = $"{(fileDto.File.Length / (1024.0 * 1024.0)):F2} MB",
                type = fileDto.type
            };

            _context.AdminFiles.Add(fileRecord);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "File uploaded successfully.",
                fileDetails = new
                {
                    fileRecord.Id,
                    fileRecord.FileName,
                    fileRecord.FilePath,
                    fileRecord.uploadeDate,
                    fileRecord.size,
                    fileRecord.type
                }
            });
        }

        [HttpDelete("DeleteFileByName/{fileName}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteFileByName(string fileName)
        {
            var fileRecord = await _context.AdminFiles.FirstOrDefaultAsync(f => f.FileName == fileName);
            if (fileRecord == null)
                return NotFound(new { success = false, message = "File not found." });

            var filePath = Path.Combine(_hostingEnvironment.WebRootPath, fileRecord.FilePath.TrimStart('/'));

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            _context.AdminFiles.Remove(fileRecord);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "File deleted successfully." });
        }

        [HttpDelete("DeleteFileById/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteFileById(int id)
        {
            var fileRecord = await _context.AdminFiles.FindAsync(id);
            if (fileRecord == null)
                return NotFound(new { success = false, message = "File not found." });

            var filePath = Path.Combine(_hostingEnvironment.WebRootPath, fileRecord.FilePath.TrimStart('/'));

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            _context.AdminFiles.Remove(fileRecord);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "File deleted successfully." });
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllFiles()
        {
            var files = await _context.AdminFiles
                .Select(f => new
                {
                    f.Id,
                    f.FileName,
                    f.FilePath,
                    f.uploadeDate,
                    f.size,
                    f.type
                })
                .ToListAsync();

            if (files.Count == 0)
                return NotFound(new { success = false, message = "No files found." });

            return Ok(new { files });
        }
        [HttpGet]
        public async Task<IActionResult>GetFiles()
        {
            var files = await _context.AdminFiles
                .Select(f => new
                {
                    f.Id,
                    f.FileName,
                    f.FilePath,
                    f.uploadeDate,
                    f.size,
                    f.type
                })
                .ToListAsync();

            if (files.Count == 0)
                return NotFound(new { success = false, message = "No files found." });

            return Ok(new { success=true,message="fetch complete", files });



        }




    }
}
