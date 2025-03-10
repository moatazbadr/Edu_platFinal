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

        public ChatController(UserManager<ApplicationUser> userManager,ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

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
                    profilePicture = s.applicationUser.profilePicture
                })
                .Distinct()
                .ToList();

            return Ok(new { students });
        }

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
                    ProfilePicture=cd.Doctor.applicationUser.profilePicture
                })
                .Distinct()
                .ToList();

            return Ok(new { doctors });
        }










    }
}
