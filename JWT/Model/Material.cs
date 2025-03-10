using Edu_plat.Model.Course_registeration;
using System.Text.Json.Serialization;

namespace Edu_plat.Model
{
	public class Material
	{
		public int Id { get; set; }
		public string Description { get; set; }
		public string FilePath { get; set; }
		public string FileName { get; set; }
		public string TypeFile { get; set; } // lecture or lab 
		public DateTime UploadDate { get; set; }
		public string Size { get; set; }

		public int DoctorId { get; set; }
		[JsonIgnore]
		public Doctor Doctor { get; set; }

		public string CourseCode { get; set; }
		public int CourseId { get; set; }
		[JsonIgnore]
		public Course Course { get; set; }
	}
}
