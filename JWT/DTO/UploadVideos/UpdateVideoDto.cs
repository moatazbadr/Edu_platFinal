using System.ComponentModel.DataAnnotations;

namespace Edu_plat.DTO.UploadVideos
{
	public class UpdateVideoDto
	{
		[Required(ErrorMessage = "Video Id is required.")]
		public int VideoId { get; set; } // The ID of the video to be updated

		[Required(ErrorMessage = "Video is required.")]
		
		public IFormFile Video { get; set; } // The new video file


		
		[StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
		public string? Description { get; set; } // Optional: New video description

		
		[RegularExpression("^(Video)$", ErrorMessage = "Type must be 'Video'")]
		[StringLength(5, ErrorMessage = "Type cannot exceed 5 characters.")]
		public string TypeFile { get; set; } // The updated type of video (Lec or Lab)
	}
}
