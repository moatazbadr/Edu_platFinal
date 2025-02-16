using System.ComponentModel.DataAnnotations;

namespace Edu_plat.DTO.UploadFiles
{
	public class UpdateMaterialDto
	{
		[Required(ErrorMessage = "MaterialId is required.")]
		public int MaterialId { get; set; }  // ID of the file to update

		[Required(ErrorMessage = "File is required.")]
		public IFormFile File { get; set; }  // New file to upload

	    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string Description { get; set; } // Optional new description

		[Required(ErrorMessage = "Type is required.")]
		[RegularExpression("^(Material|Labs|Exams)$", ErrorMessage = "Type must be either 'Material', 'Labs', 'Exams'.")]
		public string Type { get; set; } // File type (PDF, DOCX, etc.)
	}
}
