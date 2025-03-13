namespace Edu_plat.Model.Exams
{
	public class Choice
	{
		public int Id { get; set; }
		public string Text { get; set; }  // نص الاختيار
		public bool IsCorrect { get; set; }  // هل هذا الاختيار صحيح أم لا؟

		// 🔹 العلاقة مع السؤال
		public int QuestionId { get; set; }
		public Question Question { get; set; }
	}
}
