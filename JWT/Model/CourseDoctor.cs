using Edu_plat.Model.Course_registeration;

namespace Edu_plat.Model
{
	public class CourseDoctor
	{
		public int CourseId { get; set; }
		public Course Course { get; set; }
		public int DoctorId { get; set; }
		public Doctor Doctor { get; set; }
	}
}
