using System.ComponentModel.DataAnnotations;

namespace Edu_plat.DTO.UploadFiles
{
	public class UploadMatarielDto
	{
		[Required(ErrorMessage = "File is required.")]
		public IFormFile File { get; set; }

		[StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
	//	[Required(ErrorMessage = "Description is required.")]
	//	public string Description { get; set; } 

		[Required(ErrorMessage = "CourseCode is required.")]
		public string CourseCode { get; set; }

		[Required(ErrorMessage = "Type is required.")]
		[RegularExpression("^(Lectures|Labs|Exams)$", ErrorMessage = "Type must be either 'Lectures', 'Labs', 'Exams'.")]
		[StringLength(10, ErrorMessage = "Type cannot exceed 10 characters.")]
		public string Type { get; set; } // Material, Labs, or Exams

	}
}
