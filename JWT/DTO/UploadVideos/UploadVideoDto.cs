using System.ComponentModel.DataAnnotations;

namespace Edu_plat.DTO.UploadVideos
{
	public class UploadVideoDto
	{
			[Required(ErrorMessage = "File is required.")]
			public IFormFile Video { get; set; } // The uploaded video file

			[Required(ErrorMessage = "CourseCode is required.")]
		    [StringLength(10, ErrorMessage = "CourseCode cannot exceed 10 characters.")]
		    public string CourseCode { get; set; } // The course code where the video will be stored

		   // [Required(ErrorMessage = "Description is required.")]
		   // [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
		   //public string? Description { get; set; } // Optional: Video description

		   [Required(ErrorMessage = "Type is required.")]
		   [RegularExpression("^(Video)$", ErrorMessage = "Type must be 'Video'")]
		   [StringLength(5, ErrorMessage = "Type cannot exceed 5 characters.")]
		public string TypeFile { get; set; } 
		
	}
}
