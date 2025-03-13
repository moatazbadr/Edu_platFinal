namespace Edu_plat.Model.Exams
{
	public class Question
	{
		public int Id { get; set; }
		public string QuestionText { get; set; }  // نص السؤال
		public int Marks { get; set; }  // درجة السؤال

		public int TimeInMin { get; set; }
		
		
		// 🔹 العلاقة مع الامتحان
		public int ExamId { get; set; }
		public Exam Exam { get; set; }
		public List<Choice> Choices { get; set; } = new List<Choice>();

	}
}
