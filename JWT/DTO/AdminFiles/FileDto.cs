using System.ComponentModel.DataAnnotations;

namespace Edu_plat.DTO.AdminFiles
{
    public class FileDto
    {
        [Required]
        public IFormFile File { get; set; }

        [Required]
        public string FileName { get; set; } // Admin provides the custom file name
    }
}
