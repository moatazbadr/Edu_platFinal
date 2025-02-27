using System.ComponentModel.DataAnnotations;

namespace Edu_plat.DTO.UploadFiles
{
	public class UpdateMaterialDto
	{
		[Required(ErrorMessage = "MaterialId is required.")]
		public int Material_Id { get; set; }  // ID of the file to update

		[Required(ErrorMessage = "File is required.")]
		public IFormFile File { get; set; }  // New file to upload

	    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
       // public string Description { get; set; } // Optional new description

		[Required(ErrorMessage = "Type is required.")]
		[RegularExpression("^(Lectures|Labs|Exams)$", ErrorMessage = "Type must be either 'Lectures', 'Labs', 'Exams'.")]
		public string Type { get; set; } 
	}
}
