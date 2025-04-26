using JWT.DATA;
using JWT.Services;
using JWT;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Edu_plat.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public ChatController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        #region getting students for a doctor
        [HttpGet("students")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> GetStudentsForDoctor()
        {
            var UserId = User.FindFirstValue("ApplicationUserId");
            if (String.IsNullOrEmpty(UserId))
            {
                return Ok(new { success = false, message = "No user was found" });
            }


            var doctor = await _context.Doctors
                .Include(d => d.CourseDoctors)
                .ThenInclude(cd => cd.Course)
                .ThenInclude(c => c.Students)
                .ThenInclude(s => s.applicationUser)
                .FirstOrDefaultAsync(d => d.UserId == UserId);

            if (doctor == null)
            {
                return NotFound(new { success = false, message = "No doctor found for this user." });
            }

            var students = doctor.CourseDoctors
                .SelectMany(cd => cd.Course.Students)
                .Where(s => s.applicationUser != null)
                .Select(s => new
                {
                    s.StudentId,
                    Name = s.applicationUser.UserName ?? "Unknown Student",
                    profilePicture = s.applicationUser.profilePicture,
                    StudentEmail = s.applicationUser.Email
                })
                .Distinct()
                .ToList();

            return Ok(new { students });
        }
        #endregion

        #region getting Doctors for a student
        [HttpGet("doctors")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetDoctorsForStudent()
        {
            var UserId = User.FindFirstValue("ApplicationUserId");

            if (string.IsNullOrEmpty(UserId))
            {
                return Ok(new { success = false, message = "No user was found" });
            }

            // Retrieve student and include registered courses and their doctors
            var student = await _context.Students
                .Include(s => s.courses) // Include courses
                    .ThenInclude(c => c.CourseDoctors) // Include CourseDoctors relation
                        .ThenInclude(cd => cd.Doctor) // Include the Doctor
                            .ThenInclude(d => d.applicationUser) // Include Doctor's user info
                .FirstOrDefaultAsync(s => s.UserId == UserId);

            if (student == null)
            {
                return NotFound(new { success = false, message = "No student found for this user." });
            }

            // Extract unique doctors teaching the student's registered courses
            var doctors = student.courses
                .SelectMany(c => c.CourseDoctors) // Flatten course-doctor mapping
                .Where(cd => cd.Doctor != null && cd.Doctor.applicationUser != null) // Ensure no null values
                .Select(cd => new
                {
                    cd.Doctor.DoctorId,
                    Name = cd.Doctor.applicationUser.UserName ?? "Unknown Doctor",
                    ProfilePicture = cd.Doctor.applicationUser.profilePicture,
                    DoctorEmail = cd.Doctor.applicationUser.Email
                })
                .Distinct()
                .ToList();

            return Ok(new { doctors });
        }

        #endregion

        [HttpGet]
        [Route("groupChat/{courseTitle}")]
        [Authorize(Roles = "Student,Doctor")]
        public async Task<IActionResult> GetGroupChat(string courseTitle)
        {
            var UserId = User.FindFirstValue("ApplicationUserId");

            if (string.IsNullOrEmpty(UserId))
            {
                return Ok(new { success = false, message = "No user was found" });
            }

            // Get the course with its students and doctors
            var course = await _context.Courses
                .Include(c => c.Students)
                    .ThenInclude(s => s.applicationUser)
                .Include(c => c.CourseDoctors)
                    .ThenInclude(cd => cd.Doctor)
                        .ThenInclude(d => d.applicationUser)
                .FirstOrDefaultAsync(c => c.CourseDescription == courseTitle);

            if (course == null)
            {
                return NotFound(new { success = false, message = "Course not found" });
            }

            // Get all students in the course
            var students = course.Students
                .Where(s => s.applicationUser != null)
                .Select(s => new
                {
                   // s.StudentId,
                    Name = s.applicationUser.UserName ?? "Unknown Student",
                    ProfilePicture = s.applicationUser.profilePicture,
                    Email = s.applicationUser.Email,
                   // Role = "Student"
                })
                .Distinct()
                .ToList();

            // Get all doctors teaching the course
            var doctors = course.CourseDoctors
                .Where(cd => cd.Doctor != null && cd.Doctor.applicationUser != null)
                .Select(cd => new
                {
                    //cd.Doctor.DoctorId,
                    Name = cd.Doctor.applicationUser.UserName ?? "Unknown Doctor",
                    ProfilePicture = cd.Doctor.applicationUser.profilePicture,
                    Email = cd.Doctor.applicationUser.Email,
                  //  Role = "Doctor"
                })
                .Distinct()
                .ToList();

            // Combine both lists
            var groupMembers = new
            {
                Doctors = doctors,
                Students = students
            };

            return Ok(new { success = true,message="fetched successfully", groupMembers });
        }







    }
}