using System.ComponentModel.DataAnnotations;

namespace Edu_plat.DTO.UploadVideos
{
	public class DeleteVideoDto
	{
		[Required(ErrorMessage = "MaterialId is required.")]
		public int MaterialId { get; set; } // The ID of the video to be deleted
	}
}
