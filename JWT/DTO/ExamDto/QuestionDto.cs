namespace Edu_plat.DTO.ExamDto
{
	public class QuestionDto
	{
		public string QuestionText { get; set; }  // نص السؤال
		public int Marks { get; set; }  // درجة السؤال

		public int TimeInMin { get; set; }
		public List<ChoiceDto> Choices { get; set; }
	}
}
