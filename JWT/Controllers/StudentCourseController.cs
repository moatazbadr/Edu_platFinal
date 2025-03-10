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
using Edu_plat.Requests;
namespace Edu_plat.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class StudentCourseController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _hostingEnvironment;

        #region Dependancy Injection
        public StudentCourseController(UserManager<ApplicationUser> userManager, ApplicationDbContext context, IWebHostEnvironment hostingEnvironment)
        {
            _userManager = userManager;
            _context = context;
            _hostingEnvironment = hostingEnvironment;
        }
        #endregion

        #region Student-Registeration 

        [HttpPost]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Register(CourseRegistrationDto registrationDto)
        {


            var userId = User.FindFirstValue("ApplicationUserId");
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
            var UserId = User.FindFirstValue("ApplicationUserId");
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
                {
                    CourseCode=c.CourseCode,
                     has_Lab=c.has_Lab, 
                }).ToList();

            List<string>CoursesCodes=new List<string>();

            foreach (var code in StudentCourses)
            {
                if (!string.IsNullOrEmpty(code.CourseCode))
                {
                    CoursesCodes.Add(code.CourseCode);

                }
            }


            return Ok(StudentCourses);
    
        }




        #endregion

        #region Delete Courses For Student

        [HttpDelete]
        [Authorize(Roles ="Student")]
        public async Task<IActionResult> DeleteCourse(CourseDeletion courseDeletion)
        {
            if (courseDeletion.CourseCode == null)
            {
                return Ok(new { success = false, message = "invalid course Code" });
            }

            var userId = User.FindFirstValue("ApplicationUserId");
            
            if (userId == null) {
                return Ok(new { success = false, message = "Couldn't Find User" });
            }
            
            var user= await _userManager.FindByIdAsync(userId);

            var CourseRequired =await _context.Courses.FirstOrDefaultAsync(x=>x.CourseCode==courseDeletion.CourseCode );

            if (CourseRequired == null) {
                return Ok(new { success = false, message = "Couldn't Delete the Course" });
            }

            var student = await _context.Students.Include(s => s.courses).FirstOrDefaultAsync(s => s.UserId == userId);
            
            if (student == null) {
                return Ok(new { success = false, message = "Couldn't Delete the Course" });

            }
            
            student.courses.Remove(CourseRequired);
            _context.Update(student);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Course Deleted Successfully" });

        }

        #endregion





    }
}