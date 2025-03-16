using System.ComponentModel.DataAnnotations;

namespace Edu_plat.DTO.ExamDto
{
	public class ExamSubmissionDto
	{
		[Required]
		public int ExamId { get; set; }
		[Required]
		public int Score { get; set; }
	}
}
