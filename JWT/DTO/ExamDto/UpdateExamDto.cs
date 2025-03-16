namespace Edu_plat.DTO.ExamDto
{
	public class UpdateExamDto
	{
		public int ExamId { get; set; } 
		public string ExamTitle { get; set; } 
		public DateTime StartTime { get; set; }  
		public int TotalMarks { get; set; }  
		public bool IsOnline { get; set; } 
		public int DurationInMin { get; set; } 
		public int QusetionsNumber { get; set; }  

		// قائمة بالأسئلة الجديدة أو المعدّلة
		public List<ChoiceDto> Questions { get; set; }
	}
}
