using Edu_plat.Model.Exams;
using JWT;
using System.ComponentModel.DataAnnotations.Schema;

namespace Edu_plat.Model
{
	public class Doctor
	{
		// pk 
		public int DoctorId { get; set; }

		[ForeignKey("applicationUser")]
		public string? UserId { get; set; }
		public ApplicationUser? applicationUser { get; set; }
		public List<CourseDoctor> CourseDoctors { get; set; } = new List<CourseDoctor>();

		// العلاقة One-to-Many مع Material
		public List<Material> Materials { get; set; } = new List<Material>();
		// 🔹 العلاقة One-to-Many مع الامتحانات
		public List<Exam> Exams { get; set; } = new List<Exam>();
	}
}
