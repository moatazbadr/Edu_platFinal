using Azure;
using Edu_plat.DTO.ExamDto;
using Edu_plat.Model.Exams;
using JWT;
using JWT.DATA;
using JWT.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;

namespace Edu_plat.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ExamsController : ControllerBase
	{
		private readonly ApplicationDbContext _context;
		private readonly UserManager<ApplicationUser> _userManager;
		public ExamsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
		{
			_context = context;
			_userManager = userManager;
		}

		#region CreateExamComment

		//[HttpPost("CreateExam")]
		//[Authorize(Roles = "Doctor")]
		//public async Task<IActionResult> CreateExam([FromBody] CreateExamDto examDto)
		//{
		//	if (examDto == null || examDto.Questions == null || examDto.Questions.Count == 0)
		//	{
		//		return BadRequest("Exam details and questions are required.");
		//	}

		//	var userId = User.FindFirstValue("ApplicationUserId");
		//	var user = await _userManager.FindByIdAsync(userId);
		//	if (user == null)
		//	{
		//		return Ok(new { success = false, message = "User not found." });
		//	}

		//	var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
		//	if (doctor == null)
		//	{
		//		return NotFound(new { message = "Doctor profile not found." });
		//	}

		//	var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseCode == examDto.CourseCode);
		//	if (course == null)
		//	{
		//		return NotFound(new { message = "Course not found." });
		//	}

		//	// 🔹 Check if the doctor is assigned to this course
		//	var isDoctorAssigned = await _context.CourseDoctors.AnyAsync(cd => cd.DoctorId == doctor.DoctorId && cd.CourseId == course.Id);
		//	if (!isDoctorAssigned)
		//	{
		//		return Unauthorized(new { message = "Doctor is not assigned to this course." });
		//	}
		//	// 🔹 Create a new exam object
		//	var exam = new Exam
		//	{
		//		ExamTitle = examDto.ExamTitle,
		//		StartTime = examDto.StartTime,
		//		TotalMarks = examDto.TotalMarks,
		//		IsOnline = examDto.IsOnline,
		//		DurationInMin = examDto.DurationInMin,
		//		QusetionsNumber = examDto.QusetionsNumber,
		//		CourseId = course.Id,
		//		DoctorId = doctor.DoctorId,
		//		Questions = new List<Question>()
		//	};

		//	// 🔹 Add questions with choices
		//	foreach (var qDto in examDto.Questions)
		//	{
		//		if (qDto.Choices == null || qDto.Choices.Count < 2 || qDto.Choices.Count > 4)
		//		{
		//			return BadRequest("Each question must have between 2 to 4 choices.");
		//		}

		//		if (qDto.Choices.Count(c => c.IsCorrect) != 1)
		//		{
		//			return BadRequest("Each question must have exactly one correct answer.");
		//		}

		//		var question = new Question
		//		{
		//			QuestionText = qDto.QuestionText,
		//			Marks = qDto.Marks,
		//			TimeInMin = qDto.TimeInMin,
		//			Exam = exam,
		//			Choices = new List<Choice>()
		//		};

		//		foreach (var cDto in qDto.Choices)
		//		{
		//			var choice = new Choice
		//			{
		//				Text = cDto.ChoiceText,
		//				IsCorrect = cDto.IsCorrect,
		//				Question = question
		//			};
		//			question.Choices.Add(choice);
		//		}

		//		exam.Questions.Add(question);
		//	}

		//	// 🔹 Save to database
		//	_context.Exams.Add(exam);
		//	await _context.SaveChangesAsync();

		//	return Ok(new { message = "Exam created successfully.", examId = exam.Id });
		//}
		#endregion


		// POST: Create a new exam

		#region CreateExamOnline&Offline
		//[HttpPost("CreateExamOnline&Offline")]
		//[Authorize(Roles = "Doctor")]
		//public async Task<IActionResult> CreateExam([FromBody] CreateExamDto examDto)
		//{

		//	if (!ModelState.IsValid)
		//	{
		//		return BadRequest(new { succes = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
		//	}

		//	// Taken UserId From Token
		//	var userId = User.FindFirstValue("ApplicationUserId");
		//	var user = await _userManager.FindByIdAsync(userId);
		//	if (user == null)
		//	{
		//		return Ok(new { success = false, message = "User not found." });
		//	}

		//	var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
		//	if (doctor == null)
		//	{
		//		return NotFound(new { success = false, message = "Doctor profile not found." });
		//	}

		//	var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseCode.ToLower() == examDto.CourseCode.ToLower());
		//	if (course == null)
		//	{
		//		return NotFound(new { success = false, message = "Course not found." });
		//	}

		//	// Check if the doctor is assigned to this course
		//	var isDoctorAssigned = await _context.CourseDoctors.AnyAsync(cd => cd.DoctorId == doctor.DoctorId && cd.CourseId == course.Id);
		//	if (!isDoctorAssigned)
		//	{
		//		return Unauthorized(new { success = false, message = "Doctor is not assigned to this course." });
		//	}
		//	var egyptNow = DateTime.Now; // الحصول على التوقيت المحلي بشكل مباشر

		//	if (examDto.StartTime <= egyptNow)
		//	{
		//		return BadRequest(new { success = false, message = "وقت بدء الامتحان يجب أن يكون في المستقبل." });
		//	}


		//	// 🔹 Create a new exam object
		//	var exam = new Exam
		//	{
		//		ExamTitle = examDto.ExamTitle,
		//		StartTime = examDto.StartTime,
		//		TotalMarks = examDto.TotalMarks,
		//		IsOnline = examDto.IsOnline,
		//		DurationInMin = examDto.DurationInMin,
		//		QusetionsNumber = examDto.IsOnline ? examDto.QuestionsNumber : null,
		//		CourseId = course.Id,
		//		DoctorId = doctor.DoctorId,
		//		CourseCode = examDto.CourseCode,
		//		Location = examDto.IsOnline ? "Online" : examDto.LocationExam,
		//		Questions = examDto.IsOnline ? new List<Question>() : null
		//	};

		//	// check if exam Online
		//	if (examDto.IsOnline)
		//	{

		//		if (examDto.Questions == null || examDto.Questions.Count == 0 || examDto.QuestionsNumber == null)
		//		{
		//			return BadRequest(new { success = false, message = "Online exams must have at least one question." });
		//		}
		//		// check NumberOfQuestion User Enter Must Be == number of Question User Created 
		//		if (examDto.Questions.Count != examDto.QuestionsNumber)
		//		{
		//			return BadRequest($"Number of questions must be exactly {examDto.QuestionsNumber}.");
		//		}
		//		int totalTimeFromQuestions = examDto.Questions.Sum(q => q.TimeInMin);
		//		if (totalTimeFromQuestions != examDto.DurationInMin)
		//		{
		//			return BadRequest(new { succces = false, message = $"Total exam time should be exactly {examDto.DurationInMin} minutes, but got {totalTimeFromQuestions} minutes." });
		//		}

		//		int totalMarksFromQuestions = examDto.Questions.Sum(q => q.Marks);
		//		if (totalMarksFromQuestions != examDto.TotalMarks)
		//		{
		//			return BadRequest(new { succes = false, message = $"Total marks should be exactly {examDto.TotalMarks}, but got {totalMarksFromQuestions}." });
		//		}

		//		foreach (var qDto in examDto.Questions)
		//		{
		//			// Number of Choices Must be at least 2 at most 5 
		//			if (qDto.Choices == null || qDto.Choices.Count < 2 || qDto.Choices.Count > 5)
		//			{
		//				return BadRequest(new { succes = false, message = "Each question must have between 2 to 5 choices." });
		//			}
		//			// correct answer must be one answer 
		//			if (qDto.Choices.Count(c => c.IsCorrect) != 1)
		//			{
		//				return BadRequest(new { succes = false, message = "Each question must have exactly one correct answer." });
		//			}

		//			var question = new Question
		//			{
		//				QuestionText = qDto.QuestionText,
		//				Marks = qDto.Marks,
		//				TimeInMin = qDto.TimeInMin,
		//				Exam = exam,
		//				Choices = new List<Choice>()
		//			};

		//			foreach (var cDto in qDto.Choices)
		//			{
		//				var choice = new Choice
		//				{
		//					Text = cDto.ChoiceText,
		//					IsCorrect = cDto.IsCorrect,
		//					Question = question
		//				};
		//				question.Choices.Add(choice);
		//			}
		//			// exam is online => so exam.Question not be null atmost beacuse i make check above 
		//			exam.Questions.Add(question);
		//		}
		//	}
		//	else
		//	{
		//		if (string.IsNullOrWhiteSpace(examDto.LocationExam))
		//		{
		//			return BadRequest(new { succes = false, message = "Offline exams must have a location." });
		//		}
		//		if (examDto.Questions != null && examDto.Questions.Count > 0)
		//		{
		//			examDto.Questions = null;
		//			examDto.QuestionsNumber = null;
		//			return BadRequest(new { succes = false, message = "Offline exams should not have questions." });
		//		}
		//	}

		//	// Save  Examto database
		//	_context.Exams.Add(exam);
		//	await _context.SaveChangesAsync();

		//	return Ok(new { succes = true, message = "Exam created successfully.", examId = exam.Id });
		//}

		#endregion

		[HttpPost("CreateExamOnline&Offline")]
		[Authorize(Roles = "Doctor")]
		public async Task<IActionResult> CreateExam([FromBody] CreateExamDto examDto)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(new
				{
					succes = false,
					errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
				});
			}

			var userId = User.FindFirstValue("ApplicationUserId");
			var user = await _userManager.FindByIdAsync(userId);
			if (user == null)
			{
				return Ok(new { success = false, message = "User not found." });
			}

			var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
			if (doctor == null)
			{
				return NotFound(new { success = false, message = "Doctor profile not found." });
			}

			var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseCode.ToLower() == examDto.CourseCode.ToLower());
			if (course == null)
			{
				return NotFound(new { success = false, message = "Course not found." });
			}

			var isDoctorAssigned = await _context.CourseDoctors.AnyAsync(cd => cd.DoctorId == doctor.DoctorId && cd.CourseId == course.Id);
			if (!isDoctorAssigned)
			{
				return Unauthorized(new { success = false, message = "Doctor is not assigned to this course." });
			}

			var utcNow = DateTime.UtcNow;
			if (examDto.StartTime <= utcNow)
			{
				return BadRequest(new { success = false, message = "وقت بدء الامتحان يجب أن يكون في المستقبل." });
			}

			var exam = new Exam
			{
				ExamTitle = examDto.ExamTitle,
				StartTime = examDto.StartTime,
				TotalMarks = examDto.TotalMarks,
				IsOnline = examDto.IsOnline,
				DurationInMin = examDto.DurationInMin,
				QusetionsNumber = examDto.IsOnline ? examDto.QuestionsNumber : null,
				CourseId = course.Id,
				DoctorId = doctor.DoctorId,
				CourseCode = examDto.CourseCode,
				Location = examDto.IsOnline ? "Online" : examDto.LocationExam,
				Questions = examDto.IsOnline ? new List<Question>() : null
			};

			if (examDto.IsOnline)
			{
				if (examDto.Questions == null || examDto.Questions.Count == 0 || examDto.QuestionsNumber == null)
				{
					return BadRequest(new { success = false, message = "Online exams must have at least one question." });
				}
				if (examDto.Questions.Count != examDto.QuestionsNumber)
				{
					return BadRequest($"Number of questions must be exactly {examDto.QuestionsNumber}.");
				}
				if (examDto.Questions.Sum(q => q.TimeInMin) != examDto.DurationInMin)
				{
					return BadRequest(new { succces = false, message = $"Total exam time should be exactly {examDto.DurationInMin} minutes." });
				}
				if (examDto.Questions.Sum(q => q.Marks) != examDto.TotalMarks)
				{
					return BadRequest(new { succes = false, message = $"Total marks should be exactly {examDto.TotalMarks}." });
				}

				foreach (var qDto in examDto.Questions)
				{
					if (qDto.Choices == null || qDto.Choices.Count < 2 || qDto.Choices.Count > 5)
					{
						return BadRequest(new { succes = false, message = "Each question must have between 2 to 5 choices." });
					}
					if (qDto.Choices.Count(c => c.IsCorrect) != 1)
					{
						return BadRequest(new { succes = false, message = "Each question must have exactly one correct answer." });
					}

					var question = new Question
					{
						QuestionText = qDto.QuestionText,
						Marks = qDto.Marks,
						TimeInMin = qDto.TimeInMin,
						Exam = exam,
						Choices = qDto.Choices.Select(cDto => new Choice
						{
							Text = cDto.ChoiceText,
							IsCorrect = cDto.IsCorrect
						}).ToList()
					};

					exam.Questions.Add(question);
				}
			}
			else
			{
				if (string.IsNullOrWhiteSpace(examDto.LocationExam))
				{
					return BadRequest(new { succes = false, message = "Offline exams must have a location." });
				}
				if (examDto.Questions != null && examDto.Questions.Count > 0)
				{
					return BadRequest(new { succes = false, message = "Offline exams should not have questions." });
				}
			}

			_context.Exams.Add(exam);
			await _context.SaveChangesAsync();

			return Ok(new { succes = true, message = "Exam created successfully.", examId = exam.Id });
		}

		// DELETE: Delete an exam by ID (Done) cannot Delete if StartTime Exam Is Fininsh 
		#region DeleteExam
		[HttpDelete("DeleteExam/{examId}")]
		[Authorize(Roles = "Doctor")]
		public async Task<IActionResult> DeleteExam(int examId)
		{
			var userId = User.FindFirstValue("ApplicationUserId");
			var doctor = await _context.Doctors
				.AsNoTracking()
				.FirstOrDefaultAsync(d => d.UserId == userId);

			if (doctor == null)
			{
				return Unauthorized(new { message = "Doctor profile not found." });
			}
			var exam = await _context.Exams
				.Include(e => e.Questions!)
				.ThenInclude(q => q.Choices)
				.FirstOrDefaultAsync(e => e.Id == examId);

			if (exam == null)
			{
				return NotFound(new { message = "Exam not found." });
			}

			bool isDoctorAssigned = await _context.CourseDoctors
				.AnyAsync(cd => cd.DoctorId == doctor.DoctorId && cd.CourseId == exam.CourseId);

			if (!isDoctorAssigned)
			{
				return Unauthorized(new { message = "You are not authorized to delete this exam." });
			}
			if (exam.StartTime <= DateTime.UtcNow)
			{
				return BadRequest(new { success = false, message = "You cannot update an exam that has already started." });
			}


			_context.Exams.Remove(exam);
			await _context.SaveChangesAsync();

			return Ok(new { message = "Exam deleted successfully." });
		}
		#endregion

		#region GetExamToDoctor
		[HttpGet("GetExam/{examId}/{courseCode}")]
		[Authorize(Roles = "Doctor")]
		public async Task<IActionResult> GetExam(int examId, string courseCode)
		{
			if (examId <= 0 || string.IsNullOrWhiteSpace(courseCode))
			{
				return BadRequest(new { success = false, message = "Invalid exam ID or course code." });
			}

			var userId = User.FindFirstValue("ApplicationUserId");
			var user = await _userManager.FindByIdAsync(userId);
			if (user == null)
			{
				return Unauthorized(new { success = false, message = "User not found." });
			}

			var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
			if (doctor == null)
			{
				return NotFound(new { success = false, message = "Doctor profile not found." });
			}
			var course = await _context.Courses
		.FirstOrDefaultAsync(c => c.CourseCode == courseCode);
			if (course == null)
			{
				return BadRequest("Course Code Not Found");
			}
			bool isDoctorAssignedToCourse = await _context.CourseDoctors
	.AnyAsync(c => c.CourseId == course.Id && c.DoctorId == doctor.DoctorId);


			if (!isDoctorAssignedToCourse)
			{
				return Forbid();
			}


			var exam = await _context.Exams
				.Include(e => e.Questions!)
				.ThenInclude(q => q.Choices)
				.FirstOrDefaultAsync(e => e.Id == examId && e.CourseCode == courseCode);

			if (exam == null)
			{
				return NotFound(new { success = false, message = "Exam not found for this course." });
			}

			var examDetails = new
			{
				exam.Id,
				exam.ExamTitle,
				exam.StartTime,
				exam.TotalMarks,
				exam.IsOnline,
				exam.DurationInMin,
				exam.QusetionsNumber,
				exam.CourseCode,
				exam.Location,
				exam.DoctorId,
				IsFinished = exam.IsExamFinished(),
				Questions = exam.Questions?.Select(q => new
				{
					q.Id,
					q.QuestionText,
					q.Marks,
					q.TimeInMin,
					Choices = q.Choices.Select(c => new
					{
						c.Id,
						c.Text
					})
				})
			};

			return Ok(new { success = true, exam = examDetails });
		}

		#endregion


		// GET: Retrieve model answers (Question + Correct Answer)
		#region ModelAnswer
		[HttpGet("GetModelAnswer/{examId}")]
		[Authorize(Roles = "Doctor,Student")]
		public async Task<IActionResult> GetModelAnswer(int examId)
		{
			var exam = await _context.Exams
				.Include(e => e.Questions!)
				.ThenInclude(q => q.Choices)
				.FirstOrDefaultAsync(e => e.Id == examId);

			if (exam == null)
			{
				return NotFound(new { message = "Exam not found." });
			}

			if (!exam.IsOnline)
			{
				return BadRequest(new { message = "Model answers are only available for online exams." });
			}

			var modelAnswers = exam.Questions
				.Where(q => q.Choices.Any(c => c.IsCorrect))
				.Select(q => new
				{
					QuestionText = q.QuestionText,
					CorrectAnswer = q.Choices.FirstOrDefault(c => c.IsCorrect)?.Text
				})
				.ToList();

			return Ok(new { examId = exam.Id, examTitle = exam.ExamTitle, modelAnswers });
		}
		#endregion

		// PUT: Update an exam
		#region UpdateExam
		[HttpPut("UpdateExam/{examId}")]
		[Authorize(Roles = "Doctor")]
		public async Task<IActionResult> UpdateExam(int examId, [FromForm] CreateExamDto examDto)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
			}

			var userId = User.FindFirstValue("ApplicationUserId");
			var doctor = await _context.Doctors.AsNoTracking().FirstOrDefaultAsync(d => d.UserId == userId);
			if (doctor == null)
			{
				return Unauthorized(new { message = "Doctor profile not found." });
			}

			var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseCode == examDto.CourseCode);
			if (course == null)
			{
				return NotFound(new { message = "Course not found." });
			}

			// Check if the doctor is assigned to this course
			var isDoctorAssigned = await _context.CourseDoctors.AnyAsync(cd => cd.DoctorId == doctor.DoctorId && cd.CourseId == course.Id);
			if (!isDoctorAssigned)
			{
				return Unauthorized(new { message = "Doctor is not assigned to this course." });
			}

			var exam = await _context.Exams
				.Include(e => e.Questions!)
				.ThenInclude(q => q.Choices!)
				.FirstOrDefaultAsync(e => e.Id == examId);

			if (exam == null)
			{
				return NotFound(new { message = "Exam not found." });
			}

			if (exam.StartTime <= DateTime.UtcNow)
			{
				return BadRequest(new { success = false, message = "You cannot update an exam that has already started." });
			}


			exam.ExamTitle = examDto.ExamTitle;
			exam.StartTime = examDto.StartTime;
			exam.TotalMarks = examDto.TotalMarks;
			exam.IsOnline = examDto.IsOnline;
			exam.DurationInMin = examDto.DurationInMin;
			exam.QusetionsNumber = examDto.IsOnline ? examDto.QuestionsNumber : null;

			if (examDto.IsOnline)
			{

				if (examDto.Questions == null || examDto.Questions.Count == 0 || examDto.QuestionsNumber == null)
				{
					return BadRequest(new { success = false, message = "Online exams must have at least one question." });
				}
				// check NumberOfQuestion User Enter Must Be == number of Question User Created 
				if (examDto.Questions.Count != examDto.QuestionsNumber)
				{
					return BadRequest($"Number of questions must be exactly {examDto.QuestionsNumber}.");
				}
				int totalTimeFromQuestions = examDto.Questions.Sum(q => q.TimeInMin);
				if (totalTimeFromQuestions != examDto.DurationInMin)
				{
					return BadRequest(new { succces = false, message = $"Total exam time should be exactly {examDto.DurationInMin} minutes, but got {totalTimeFromQuestions} minutes." });
				}

				int totalMarksFromQuestions = examDto.Questions.Sum(q => q.Marks);
				if (totalMarksFromQuestions != examDto.TotalMarks)
				{
					return BadRequest(new { succes = false, message = $"Total marks should be exactly {examDto.TotalMarks}, but got {totalMarksFromQuestions}." });
				}


				bool hasExamBeenTaken = await _context.ExamStudents.AnyAsync(se => se.ExamId == exam.Id);
				if (hasExamBeenTaken)
				{
					return BadRequest(new { success = false, message = "You cannot modify questions for an exam that has already been taken by students." });
				}


				var existingQuestions = exam.Questions.ToList();
				foreach (var oldQuestion in existingQuestions)
				{
					var updatedQuestion = examDto.Questions.FirstOrDefault(q => q.QuestionText == oldQuestion.QuestionText);
					if (updatedQuestion == null)
					{
						_context.Question.Remove(oldQuestion);
					}
					else
					{
						oldQuestion.Marks = updatedQuestion.Marks;
						oldQuestion.TimeInMin = updatedQuestion.TimeInMin;


						var existingChoices = oldQuestion.Choices.ToList();
						foreach (var oldChoice in existingChoices)
						{
							var updatedChoice = updatedQuestion.Choices.FirstOrDefault(c => c.ChoiceText == oldChoice.Text);
							if (updatedChoice == null)
							{
								_context.Choices.Remove(oldChoice);
							}
							else
							{
								oldChoice.IsCorrect = updatedChoice.IsCorrect;
							}
						}
						var newChoices = updatedQuestion.Choices
	                                    .Where(c => !existingChoices.Any(ec => ec.Text == c.ChoiceText))
	                                    .Select(c => new Choice
	                                     {
	                                    	Text = c.ChoiceText,
		                                    IsCorrect = c.IsCorrect // لو في خصائص تانية لازم تضيفها هنا
	                                     })
	                                    .ToList();

						oldQuestion.Choices.AddRange(newChoices);


						//var newChoices = updatedQuestion.Choices.Where(c => !existingChoices.Any(ec => ec.Text == c.ChoiceText)).ToList();
						//oldQuestion.Choices.AddRange(newChoices);
					}
				}


				var newQuestions = examDto.Questions.Where(q => !existingQuestions.Any(eq => eq.QuestionText == q.QuestionText)).ToList();
				foreach (var newQuestion in newQuestions)
				{
					exam.Questions.Add(new Question
					{
						QuestionText = newQuestion.QuestionText,
						Marks = newQuestion.Marks,
						TimeInMin = newQuestion.TimeInMin,
						Exam = exam,
						Choices = newQuestion.Choices.Select(c => new Choice
						{
							Text = c.ChoiceText,
							IsCorrect = c.IsCorrect
						}).ToList()
					});
				}
			}
			else
			{
				_context.Question.RemoveRange(exam.Questions);
				exam.Questions.Clear();
			}

			await _context.SaveChangesAsync();
			return Ok(new { message = "Exam updated successfully." });
		}
		#endregion

		#region GetUserExamsComment
		//[HttpGet("GetUserExams")]
		//[Authorize(Roles = "Doctor,Student")]
		//public async Task<IActionResult> GetUserExams([FromQuery] GetUserExamsDto userexamdto)
		//{
		//	if (!ModelState.IsValid)
		//	{
		//		return BadRequest(ModelState);
		//	}

		//	var userId = User.FindFirstValue("ApplicationUserId");

		//	if (User.IsInRole("Doctor"))
		//	{
		//		var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
		//		if (doctor == null)
		//		{
		//			return NotFound(new { message = "Doctor profile not found." });
		//		}

		//		var exams = await _context.Exams
		//			.Where(e => _context.CourseDoctors.Any(cd => cd.DoctorId == doctor.DoctorId && cd.CourseId == e.CourseId ))
		//			.Select(e => new
		//			{
		//				e.Id,
		//				e.ExamTitle,
		//				e.StartTime,
		//				e.TotalMarks,
		//				e.IsOnline,
		//				e.DurationInMin,
		//				e.QusetionsNumber,
		//				e.CourseCode,
		//				e.DoctorId,
		//				e.Location,
		//				IsFinished = e.IsExamFinished()
		//			})
		//			.ToListAsync();


		//			exams = exams.Where(e => e.IsFinished == userexamdto.isFinishedExam).ToList();

		//		return Ok(exams);
		//	}
		//	else if (User.IsInRole("Student"))
		//	{
		//		var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId);
		//		if (student == null)
		//		{
		//			return NotFound(new { message = "Student profile not found." });
		//		}

		//		// Get student and Get Courses which  Register 
		//		var Getstudent = await _context.Students.Include(sc => sc.courses)
		//							  .FirstOrDefaultAsync(s => s.UserId == userId);
		//		if (Getstudent == null)
		//		{
		//			return BadRequest("Student is not in the System");
		//		}
		//		// get Courses which student Is Register 
		//		var StudentCourses = Getstudent.courses.Select(
		//			c => new
		//			{
		//				CourseCode = c.CourseCode

		//			}).ToList();

		//		var exams = await _context.Exams
		//			.Where(e => Getstudent.courses.Any(c => c.CourseCode == e.CourseCode ))
		//			.Select(e => new
		//			{
		//				e.Id,
		//				e.ExamTitle,
		//				e.StartTime,
		//				e.TotalMarks,
		//				e.IsOnline,
		//				e.DurationInMin,
		//				e.QusetionsNumber,
		//				e.CourseCode,
		//				e.Location,
		//				e.DoctorId ,
		//				IsFinished = e.IsExamFinished()
		//			})
		//			.ToListAsync();
		//		exams = exams.Where(e => e.IsFinished == userexamdto.isFinishedExam).ToList();
		//		return Ok(exams);
		//	}

		//	return Unauthorized(new { message = "User role not recognized." });
		//}
		#endregion

		#region GetUserExams
		[HttpGet("GetUserExams")]
		[Authorize(Roles = "Doctor,Student")]
		public async Task<IActionResult> GetUserExams([FromQuery] GetUserExamsDto userexamdto)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var userId = User.FindFirstValue("ApplicationUserId");

			if (User.IsInRole("Doctor"))
			{
				var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
				if (doctor == null)
				{
					return NotFound(new { message = "Doctor profile not found." });
				}

				var exams = await _context.Exams
					.Where(e => _context.CourseDoctors.Any(cd => cd.DoctorId == doctor.DoctorId && cd.CourseId == e.CourseId))
					.Select(e => new
					{
						e.Id,
						e.ExamTitle,
						e.StartTime,
						e.TotalMarks,
						e.IsOnline,
						e.DurationInMin,
						e.QusetionsNumber,
						e.CourseCode,
						e.Location,
						e.DoctorId,
						IsFinished = e.IsExamFinished()
					})
					.ToListAsync();

				exams = exams.Where(e => e.IsFinished == userexamdto.isFinishedExam).ToList();

				return Ok(exams);
			}

			else if (User.IsInRole("Student"))
			{
				var student = await _context.Students
					.Include(s => s.courses) 
					.FirstOrDefaultAsync(s => s.UserId == userId);

				if (student == null)
				{
					return NotFound(new { message = "Student profile not found." });
				}

				var studentCourseCodes = student.courses.Select(c => c.CourseCode).ToList();

				var exams = await _context.Exams
.Where(e => studentCourseCodes.Contains(e.CourseCode))
.Select(e => new
{
	e.Id,
	e.ExamTitle,
	e.StartTime,
	e.TotalMarks,
	e.IsOnline,
	e.DurationInMin,
	e.QusetionsNumber,
	e.CourseCode,
	e.Location,
	e.DoctorId,
	IsFinished = e.IsExamFinished(),

	StudentExam = _context.ExamStudents
		.Where(es => es.StudentId == student.StudentId && es.ExamId == e.Id)
		.Select(es => new
		{
			Score = (int?)es.Score,
			PercentageExam = (int?)es.precentageExam,
			IsAbsent = (bool?)es.IsAbsent
		})
		.FirstOrDefault()
})
// 
.Where(e => e.IsFinished == userexamdto.isFinishedExam)
.ToListAsync();


				var result = exams.Select(e => new
				{
					e.Id,
					e.ExamTitle,
					e.StartTime,
					e.TotalMarks,
					e.IsOnline,
					e.DurationInMin,
					e.QusetionsNumber,
					e.CourseCode,
					e.Location,
					e.DoctorId,
					e.IsFinished,

					Score = e.IsFinished ? e.StudentExam?.Score : null,
					PercentageExam = e.IsFinished ? e.StudentExam?.PercentageExam : null,
					IsAbsent = e.IsFinished ? e.StudentExam?.IsAbsent : null
				}).ToList();

				return Ok(result);
			}
           return Unauthorized(new { message = "User role not recognized." });
		}
		#endregion

        //Done 
		#region SubmitExamScore
		[HttpPost("SubmitExamScore")]
		[Authorize(Roles = "Student")]
		public async Task<IActionResult> SubmitExamScore([FromBody] ExamSubmissionDto submission)
		{
			if (submission == null || submission.ExamId <= 0 || submission.Score < 0)
			{
				return BadRequest(new { message = "Invalid data. Please provide a valid ExamId and Score." });
			}


			var userId = User.FindFirstValue("ApplicationUserId");
			var student = await _context.Students
				.Include(s => s.courses)
				.FirstOrDefaultAsync(s => s.UserId == userId);

			if (student == null)
			{
				return NotFound(new { message = "Student profile not found." });
			}

			var exam = await _context.Exams.FirstOrDefaultAsync(e => e.Id == submission.ExamId);
			if (exam == null)
			{
				return NotFound(new { message = "Exam not found." });
			}

			bool isStudentEnrolled = student.courses.Any(c => c.CourseCode == exam.CourseCode);
			if (!isStudentEnrolled)
			{
				return Unauthorized(new { message = "Student is not enrolled in this course." });
			}
			if (!exam.IsOnline)
			{
				return BadRequest(new { message = "You can only submit scores for online exams." });
			}

			var examRecord = await _context.ExamStudents
				.FirstOrDefaultAsync(es => es.StudentId == student.StudentId && es.ExamId == submission.ExamId);

			if (examRecord != null)
			{
				return BadRequest(new { message = "Score already submitted for this exam." });
			}

			DateTime calculatedEndTime = exam.StartTime.AddMinutes(exam.DurationInMin);
			if (DateTime.UtcNow <= calculatedEndTime)
			{
				return BadRequest(new { message = "Exam has not finished yet. You cannot submit your score now." });
			}

			int percentage = (int)((submission.Score / (double)exam.TotalMarks) * 100);

			var newExamRecord = new ExamStudent
			{
				StudentId = student.StudentId,
				ExamId = submission.ExamId,
				Score = submission.Score,
				IsAbsent = false,
				precentageExam = percentage
			};

			_context.ExamStudents.Add(newExamRecord);
			await _context.SaveChangesAsync();

			return Ok(new { message = "Exam score submitted successfully." });
		}

		#endregion

		// Done 
		#region GetExamResultsToAllStudnet
		[HttpGet("GetExamResults/{examId}")]
		[Authorize(Roles = "Doctor")]
		public async Task<IActionResult> GetExamResults(int examId)
		{
			if (examId <= 0)
			{
				return BadRequest(new { message = "Invalid Exam ID." });
			}

			var userId = User.FindFirstValue("ApplicationUserId");
			var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);

			if (doctor == null)
			{
				return Unauthorized(new { message = "Doctor profile not found." });
			}

			var exam = await _context.Exams.FirstOrDefaultAsync(e => e.Id == examId);
			if (exam == null)
			{
				return NotFound(new { message = "Exam not found." });
			}

			bool isDoctorAssigned = await _context.CourseDoctors
				.AnyAsync(cd => cd.DoctorId == doctor.DoctorId && cd.CourseId == exam.CourseId);

			if (!isDoctorAssigned)
			{
				return Unauthorized(new { message = "You are not authorized to view this exam's results." });
			}

			var examResults = await _context.ExamStudents
				.Where(es => es.ExamId == examId)
				.Select(es => new
				{

					es.Score,
					es.IsAbsent,
					es.precentageExam,
					StudentInfo = _context.Students
						.Where(s => s.StudentId == es.StudentId)
						.Select(s => new
						{
							s.applicationUser.Email,
							s.applicationUser.UserName
						})
						.FirstOrDefault()
				})
				.ToListAsync();

			if (examResults.Count == 0)
			{
				return NotFound(new { message = "No students have taken this exam yet." });
			}

			int totalStudents = examResults.Count;
			int passedStudents = examResults.Count(es => es.Score >= (exam.TotalMarks * 0.5));
			double successRate = (double)passedStudents / totalStudents * 100;

			return Ok(new
			{
				successRate = Math.Round(successRate, 2),
				students = examResults
			});
		}
		#endregion

		#region GetExamToStudent

		[HttpGet("GetExamForStudent/{examId}")]
		[Authorize(Roles = "Student")]
		public async Task<IActionResult> GetExamForStudent(int examId)
		{
			// Validate exam ID
			if (examId <= 0)
			{
				return BadRequest(new { success = false, message = "Invalid exam ID." });
			}

			// Get user ID from claims
			var userId = User.FindFirstValue("ApplicationUserId");
			var student = await _context.Students
				.Include(s => s.courses)
				.FirstOrDefaultAsync(s => s.UserId == userId);

			// Check if student exists
			if (student == null)
			{
				return NotFound(new { success = false, message = "Student profile not found." });
			}

			// Retrieve exam with questions and choices
			var exam = await _context.Exams
				.Include(e => e.Questions!)
				.ThenInclude(q => q.Choices)
				.FirstOrDefaultAsync(e => e.Id == examId && e.IsOnline);

			// Check if exam exists and is online
			if (exam == null)
			{
				return NotFound(new { success = false, message = "Online exam not found." });
			}

			// Verify student enrollment in course
			bool isStudentEnrolled = student.courses.Any(c => c.CourseCode == exam.CourseCode);
			if (!isStudentEnrolled)
			{
				return Unauthorized(new { success = false, message = "Student is not enrolled in this course." });
			}

			// Check if student already accessed the exam
			var examRecord = await _context.ExamStudents
				.FirstOrDefaultAsync(es => es.StudentId == student.StudentId && es.ExamId == examId);
			if (examRecord != null)
			{
				return BadRequest(new { success = false, message = "You have already accessed this exam." });
			}

			// Handle UTC time for exam timing
			var currentUtcTime = DateTime.UtcNow;
			var examEndTime = exam.StartTime.AddMinutes(exam.DurationInMin);

			// Check if exam has not started
			if (currentUtcTime < exam.StartTime)
			{
				return BadRequest(new { success = false, message = "The exam has not started yet." });
			}
			// Check if exam has ended
			if (currentUtcTime > examEndTime)
			{
				return BadRequest(new { success = false, message = "The exam has ended." });
			}

			// Create new exam record for student
			var newExamRecord = new ExamStudent
			{
				StudentId = student.StudentId,
				ExamId = examId,
				Score = 0,
				IsAbsent = false,
				precentageExam = 0
			};
			_context.ExamStudents.Add(newExamRecord);
			await _context.SaveChangesAsync();

			// Prepare exam details response
			var examDetails = new
			{
				exam.Id,
				exam.ExamTitle,
				StartTime = exam.StartTime.ToString("yyyy-MM-dd HH:mm:ss") + " UTC", // Clear format for user
				exam.TotalMarks,
				exam.IsOnline,
				exam.QusetionsNumber,
				exam.DurationInMin,
				exam.CourseCode,
				exam.Location,
				exam.DoctorId,
				Questions = exam.Questions.OrderBy(q => Guid.NewGuid()).Select(q => new
				{
					q.Id,
					q.QuestionText,
					q.Marks,
					q.TimeInMin,
					Choices = q.Choices.OrderBy(c => Guid.NewGuid()).Select(c => new
					{
						c.Id,
						c.Text
					}).ToList()
				}).ToList()
			};

			return Ok(new { success = true, exam = examDetails });
		}
		#endregion

		#region SumbitExamToStudent

		[HttpPost("SubmitExam")]
		[Authorize(Roles = "Student")]
		public async Task<IActionResult> SubmitExam([FromBody] SubmitExamDto dto)
		{
			if (dto.ExamId <= 0 || dto.Answers == null || dto.Answers.Count == 0)
			{
				return BadRequest(new { success = false, message = "Invalid data." });
			}

			var userId = User.FindFirstValue("ApplicationUserId");
			var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId);
			if (student == null)
				return NotFound(new { success = false, message = "Student not found." });

			var exam = await _context.Exams
				.Include(e => e.Questions!)
					.ThenInclude(q => q.Choices)
				.FirstOrDefaultAsync(e => e.Id == dto.ExamId && e.IsOnline);

			if (exam == null)
				return NotFound(new { success = false, message = "Exam not found." });

			var examEndTime = exam.StartTime.AddMinutes(exam.DurationInMin);
			if (DateTime.UtcNow > examEndTime)
				return BadRequest(new { success = false, message = "Time is up. Exam already ended." });

			var record = await _context.ExamStudents.FirstOrDefaultAsync(es => es.ExamId == dto.ExamId && es.StudentId == student.StudentId);
			if (record == null)
				return BadRequest(new { success = false, message = "Exam not accessed or not allowed." });

			// Prevent double submission
			if (record.Score > 0 || record.IsAbsent)
				return BadRequest(new { success = false, message = "Exam already submitted." });

			int totalScore = 0;

			foreach (var answer in dto.Answers)
			{
				var question = exam.Questions.FirstOrDefault(q => q.Id == answer.QuestionId);
				if (question == null) continue;

				var selectedChoice = question.Choices.FirstOrDefault(c => c.Id == answer.SelectedChoiceId);
				if (selectedChoice != null && selectedChoice.IsCorrect)
				{
					totalScore += question.Marks;
				}
			}

			// Save the score
			record.Score = totalScore;
			record.precentageExam = (int)((double)totalScore / exam.TotalMarks * 100);

			await _context.SaveChangesAsync();

			return Ok(new
			{
				success = true,
				message = "Exam submitted and corrected successfully.",
				score = totalScore,
				percentage = record.precentageExam
			});
		}

		#endregion


		// Done
		#region GetExamStudentComent
		//[HttpGet("GetExamStudent")]
		//[Authorize(Roles = "Student")]
		//public async Task<IActionResult> GetExamToStudent(int examId, int doctorId, string courseCode)
		//{
		//	if (examId <= 0 || doctorId <= 0 || string.IsNullOrWhiteSpace(courseCode))
		//	{
		//		return BadRequest(new { message = "ExamId, DoctorId, and CourseCode are required and must be valid." });
		//	}
		//	var userId = User.FindFirstValue("ApplicationUserId");

		//	var student = await _context.Students
		//		.Include(s => s.courses)
		//		.FirstOrDefaultAsync(s => s.UserId == userId);

		//	if (student == null)
		//	{
		//		return NotFound(new { message = "Student profile not found." });
		//	}

		//	var isStudentEnrolled = student.courses.Any(c => c.CourseCode == courseCode);
		//	if (!isStudentEnrolled)
		//	{
		//		return Unauthorized(new { message = "Student is not enrolled in this course." });

		//	}

		//	var examDetails = await _context.Exams
		//		.Where(e => e.Id == examId && e.DoctorId == doctorId && e.CourseCode == courseCode && e.IsOnline)
		//		.Select(e => new
		//		{
		//			e.Id,
		//			e.ExamTitle,
		//			StartTime = e.StartTime.ToString("yyyy-MM-dd HH:mm:ss"),
		//			e.TotalMarks,
		//			e.IsOnline,
		//			e.QusetionsNumber,
		//			e.DurationInMin,
		//			e.CourseCode,
		//			e.Location,
		//			e.DoctorId,

		//			Questions = e.Questions.OrderBy(q => Guid.NewGuid())
		//			.Select(q => new
		//			{
		//				q.Id,
		//				q.QuestionText,
		//				q.Marks,
		//				q.TimeInMin,
		//				Choices = q.Choices
		//				.OrderBy(c => Guid.NewGuid())
		//				.Select(c => new
		//				{
		//					c.Id,
		//					c.Text,
		//					c.IsCorrect
		//				}).ToList()
		//			}).ToList()
		//		})
		//		.FirstOrDefaultAsync();

		//	if (examDetails == null)
		//	{
		//		return NotFound(new { message = "Online exam not found." });
		//	}

		//	return Ok(examDetails);
		//}

		#endregion
	}
}
