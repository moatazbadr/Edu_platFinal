using JWT.DATA;
using JWT;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Edu_plat.DTO.Course_Registration;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using static System.String;

using Microsoft.EntityFrameworkCore;
using Edu_plat.Responses;
namespace Edu_plat.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class StudentCourseController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        
        #region Dependancy Injection
        public StudentCourseController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        } 
        #endregion
        
        #region Student-Registeration 

        [HttpPost]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Register(CourseRegistrationDto registrationDto)
        {


            var userId = User.FindFirstValue("AppicationUserId");
            if (string.IsNullOrEmpty(userId))
            {
                return Ok(new { success = false, message = "Invalid Token: User not found" });
            }

            var student = await _context.Students
                .Include(s => s.courses)
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (student == null)
            {
                return Ok(new { success = false, message = "Student not found" });
            }

            if (registrationDto.CoursesCodes == null || !registrationDto.CoursesCodes.Any())
            {
                return Ok(new { success = false, message = "Invalid Registration Data" });
            }

            var coursesToRegister = await _context.Courses
                .Where(c => registrationDto.CoursesCodes.Contains(c.CourseCode))
                .ToListAsync();

            if (!coursesToRegister.Any())
            {
                return Ok(new { success = false, message = "No matching courses found" });
            }

            foreach (var course in coursesToRegister)
            {
                if (!student.courses.Any(c => c.Id == course.Id))
                {
                    student.courses.Add(course);
                }
            }

            await _context.SaveChangesAsync();
            StudentRegisterResponse  response = new StudentRegisterResponse()
            { 
                success = true,
                message = "Course registration successful"

            };


            return Ok(response);
        }

        #endregion

        #region View Courses for student 
        [HttpGet]
        [Authorize(Roles = "Student")]

        public async Task<IActionResult> MyCourses()
        {
            var UserId = User.FindFirstValue("AppicationUserId");
            StudentRegisterResponse response = new StudentRegisterResponse();
            if (string.IsNullOrEmpty(UserId))
            {
                response.success = false;
                response.message = "Invalid Token User is Not found !";
                return Ok(response);
            }
           var student = await _context.Students.Include(sc=>sc.courses)
                                       .FirstOrDefaultAsync(s=>s.UserId==UserId);
            if (student == null) { 
                response.success = false;
                response.message = "Student is not in the System";
                return Ok(response);
            }

            var StudentCourses= student.courses.Select(
                c => new
                ViewCourseDto{
                    CourseCode=c.CourseCode,
                    CourseDescription=c.CourseDescription,
                    Course_degree=c.Course_degree,
                    Course_hours = c.Course_hours,
                    Course_level=c.Course_level,
                    Course_semster = c.Course_semster
                }).ToList();

            return Ok(StudentCourses);
    
        }




        #endregion

        #region Delete Courses For Student



        #endregion



    }
}