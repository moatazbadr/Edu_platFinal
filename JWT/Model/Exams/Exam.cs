using Edu_plat.Model.Course_registeration;

namespace Edu_plat.Model.Exams
{
	public class Exam
	{
		public int Id { get; set; }
		public string ExamTitle { get; set; }  // عنوان الامتحان
		public DateTime StartTime { get; set; }  // وقت بدء الامتحان
		public int TotalMarks { get; set; }  // الدرجة الكلية للامتحان
		public bool IsOnline { get; set; }  // هل الامتحان أونلاين أم لا
		public int? QusetionsNumber { get; set; }
		public int DurationInMin { get; set; }
		public string CourseCode { get; set; }
		public string? Location { get; set; }
			public bool IsExamFinished()
			{
				var endTime = DateTime.SpecifyKind(StartTime, DateTimeKind.Utc).AddMinutes(DurationInMin);
				return DateTime.UtcNow > endTime;
			}


        public TimeSpan GetRemainingTime()
		{
			DateTime calculatedEndTime = StartTime.AddMinutes(DurationInMin);
			return calculatedEndTime - DateTime.UtcNow;
		}




		#region RelationShips

		// 🔹 العلاقة One-to-Many مع Course
		public int CourseId { get; set; }  // مفتاح أجنبي يشير إلى الكورس
		public Course Course { get; set; }  // خاصية التنقل (Navigation Property)

		// 🔹 العلاقة One-to-Many مع الأسئلة
		public List<Question>? Questions { get; set; } = new List<Question>();

		// 🔹 العلاقة مع الدكتور (امتحان واحد مرتبط بدكتور واحد)
		public int DoctorId { get; set; }
		public Doctor Doctor { get; set; }
		// 🔹 العلاقة Many-to-Many مع Student
		public List<ExamStudent> ExamStudents { get; set; } = new List<ExamStudent>(); 
		#endregion

	}
}
