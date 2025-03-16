namespace Edu_plat.DTO.ExamDto
{
	public class CreateExamDto
	{
		public string ExamTitle { get; set; } 
		public DateTime StartTime { get; set; }  
		public int TotalMarks { get; set; }  
		public bool IsOnline { get; set; }  
		public int? QusetionsNumber { get; set; }
		public int DurationInMin { get; set; }
		public string CourseCode { get; set; }
		public string? LocationExam { get; set; }

		public List<QuestionDto>? Questions { get; set; }
	}
}
