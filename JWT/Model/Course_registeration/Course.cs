using Edu_plat.Model.Exams;

namespace Edu_plat.Model.Course_registeration
{
    public class Course
    {
        public int Id { get; set; }
        public string CourseCode { get; set; }
        public string CourseDescription { get; set; }
        public bool isRegistered { get; set; }=false;
        public string? ApplicationUserId { get; set; }
        public int Course_level { get; set; }
        public int Course_semster { get; set; }

        //Navigational property for student many side 
        public ICollection<Student> Students { get; set; } = new List<Student>();

        public Course()
        {
            isRegistered = false;
        }

		public List<CourseDoctor> CourseDoctors { get; set; } = new List<CourseDoctor>();

		// العلاقة One-to-Many مع Material
		public List<Material> Materials { get; set; } = new List<Material>();
        public int Course_hours { get; set; }

        #region Course Grading [MidTerm,Lab,Oral,FinalExam,HasLab]

        public bool has_Lab { get; set; }
        public int MidTerm { get; set; }
        public int Oral {  get; set; }
        public int FinalExam { get; set; }
        public int Lab {  get; set; }
        public int TotalMark { get; set; }
		#endregion


	     // 🔹 العلاقة One-to-Many مع الامتحانات
		public List<Exam> Exams { get; set; } = new List<Exam>();




	}
}
