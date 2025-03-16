namespace Edu_plat.Model.Exams
{
	public class ExamStudent
	{
		public int StudentId { get; set; }
		public Student Student { get; set; }

		
		public int ExamId { get; set; }
		public Exam Exam { get; set; }

		public int Score { get; set; } // درجة الطالب في الامتحان
	   public bool IsAbsent { get; set; } = true;
	   public int precentageExam { get; set; }
	}
}
