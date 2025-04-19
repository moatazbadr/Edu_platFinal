using Edu_plat.Model.Exams;
using System.ComponentModel.DataAnnotations;

namespace Edu_plat.DTO.ExamDto
{
	public class CreateExamDto
	{
		[Required(ErrorMessage = "Exam title is required.")]
		[MaxLength(100, ErrorMessage = "Exam title must not exceed 100 characters.")]
		public string ExamTitle { get; set; }

		[Required(ErrorMessage = "Start time is required.")]
		public DateTime StartTime { get; set; }

		//[Range(1, int.MaxValue, ErrorMessage = "Total marks must be greater than 0.")]
		public int TotalMarks { get; set; }

		[Required(ErrorMessage = "Exam type (IsOnline) is required.")]
		public bool IsOnline { get; set; }

		//[Range(1, int.MaxValue, ErrorMessage = "Number of questions must be greater than 0.")]
		public int? QuestionsNumber { get; set; }

		//[Required(ErrorMessage = " DurationInMin is required.")]
		//[Range(1, int.MaxValue, ErrorMessage = "Duration must be greater than 0 minutes.")]
		public int DurationInMin { get; set; }

		[Required(ErrorMessage = "Course code is required.")]
		[MaxLength(100, ErrorMessage = "Course Code must not exceed 100 characters.")]
		public string CourseCode { get; set; }

		[MaxLength(100, ErrorMessage = "Location Exam must not exceed 100 characters.")]
		public string? LocationExam { get; set; }

		public List<QuestionDto>? Questions { get; set; }

	}
}
