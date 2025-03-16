using System.ComponentModel.DataAnnotations;

namespace Edu_plat.DTO.ExamDto
{
	public class GetUserExamsDto
	{
		
        [Required] 
		public bool isFinishedExam { get; set; }
	}
}
