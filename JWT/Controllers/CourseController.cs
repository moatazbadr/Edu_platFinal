using JWT.DATA;
using JWT;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Edu_plat.Model.Course_registeration;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;
using Edu_plat.DTO.Course_Registration;
using Edu_plat.Model;
using Edu_plat.Responses;

namespace Edu_plat.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CourseController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        #region Dependence Injection
        public CourseController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }
        #endregion

        #region Adding Course [Admin-only]

        [HttpPost("Add-course")]
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> AddCourse([FromBody] CourseRegisteration courseFromBody)
        {
            if (!ModelState.IsValid)
            {
                return Ok(new { success = false, message = "Error adding course" });
            }
            var course = new Course()
            {
                CourseCode = courseFromBody.CourseCode,
                CourseDescription = courseFromBody.CourseDescription,
                Course_hours = courseFromBody.Course_hours,
                Course_degree = courseFromBody.Course_degree,
                isRegistered = false,
                Course_level = courseFromBody.Course_level,
                Course_semster = courseFromBody.Course_semster,
            };
            await _context.Courses.AddAsync(course);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "course added successfully" });

        }

        #endregion

        #region Getting-all-courses [Admin-only]
        [HttpGet("Get-all-courses")]
       
        public async Task<IActionResult> GetAllCourses()
        {
            var courses = await _context.Courses
                .Include(c => c.CourseDoctors)
                .ThenInclude(cd => cd.Doctor)
                .ThenInclude(d => d.applicationUser) // To get the username from ApplicationUser
                .Select(c => new
                {
                    CourseCode = c.CourseCode,
                    Doctors = c.CourseDoctors.Select(cd => cd.Doctor.applicationUser.UserName).ToList(),
                    CourseDescription = c.CourseDescription,
                    courseHours= c.Course_hours,
                })
                .ToListAsync();

            return Ok(courses);
        }


        #endregion

        #region Getting-courses-by-semster

        [HttpGet("Courses-semster/{sem}")]
        [Authorize(Roles = "Doctor,Student")]
        
        public async Task<IActionResult> CoursesBySemster(int sem)
        {

            if (!ModelState.IsValid)
            {
                return Ok(new { success = false, message = "no semster" });
            }
            var courseBySemster = await _context.Courses.Where(x => x.Course_semster == sem && x.isRegistered == false).ToListAsync();

            var Level1_Courses = courseBySemster.Where(x => x.Course_level == 1).Select(x => new { x.CourseCode, x.CourseDescription });
            var Level2_Courses = courseBySemster.Where(x => x.Course_level == 2).Select(x => new { x.CourseCode, x.CourseDescription });
            var Level3_Courses = courseBySemster.Where(x => x.Course_level == 3).Select(x => new { x.CourseCode, x.CourseDescription });
            var Level4_Courses = courseBySemster.Where(x => x.Course_level == 4).Select(x => new { x.CourseCode, x.CourseDescription });


            var SemesterResponse = new
            {
                SmesterId = sem,
                SemesterLevels = new List<object>
                {
                    new { LevelId=1 , Level1_Courses },
                    new { LevelId=2 , Level2_Courses },
                    new { LevelId=3 , Level3_Courses },
                    new { LevelId=4 , Level4_Courses },

                }

            };


            return Ok(SemesterResponse);

        }

        #endregion

        #region Getting-courses-by-level

        [HttpGet("Courses-level/{level}")]
        public async Task<IActionResult> CoursesByLevel(int level)
        {
            int[] validLevels = { 1, 2, 3, 4 };
            var levelindex = validLevels.Where(x => x == level);
            if (!levelindex.Any())
            {
                return Ok(new { success = false, message = "Invalid Level" });
            }
            if (!ModelState.IsValid)
            {
                return Ok(new { success = false, message = "no level" });
            }
            var courseBylevel = await _context.Courses.Where(x => x.Course_level == level && x.isRegistered == false).ToListAsync();

            return Ok(courseBylevel);

        }

        #endregion

        #region Getting-Courses-by-semester & level

        [HttpGet("Courses-level/{level}/{semester}")]
        public async Task<IActionResult> CoursesBySemLevel(int level, int semester)
        {
            int[] validLevels = { 1, 2, 3, 4 };
            var levelindex = validLevels.Where(x => x == level);
            if (!levelindex.Any())
            {
                return Ok(new { success = false, message = "Invalid Level" });
            }

            if (semester != 1 || semester != 2)
            {
                return Ok(new { success = false, message = "Invalid Semester" });
            }
            
            var coursesBySemesterAndLevel = await _context.Courses
                .Where(x => x.Course_level == level && x.Course_semster == semester && x.isRegistered == false)
                .ToListAsync();

            return Ok(coursesBySemesterAndLevel);
        }


        #endregion

        #region Doctor-course-Registration [Doctor-only]
    
		[HttpPost("Add-Doctor-Course")]
		[Authorize(Roles = "Doctor")]
		public async Task<IActionResult> CouresRegister(CourseRegistrationDto courseRegistrationDto)
		{
			var userId = User.FindFirstValue("ApplicationUserId"); 
			var user = await _userManager.FindByIdAsync(userId);

			if (user == null)
			{
				return Ok(new { success = false, message = "User not found" });
			}

			var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
			if (doctor == null)
			{
				return BadRequest(new { success = false, message = "Doctor not found" });
			}

			if (courseRegistrationDto.CoursesCodes == null || !courseRegistrationDto.CoursesCodes.Any())
			{
				return BadRequest(new { success = false, message = "Course list is empty" });
			}

			List<string> successCourses = new List<string>();
			List<string> alreadyRegisteredCourses = new List<string>();
			List<string> failureCourses = new List<string>();

			foreach (string courseCode in courseRegistrationDto.CoursesCodes)
			{
				var course = await _context.Courses.FirstOrDefaultAsync(x => x.CourseCode == courseCode);

				if (course == null)
				{
					failureCourses.Add(courseCode);
					continue;
				}

				
				var existingRelation = await _context.CourseDoctors
					.AnyAsync(cd => cd.CourseId == course.Id && cd.DoctorId == doctor.DoctorId);

				if (existingRelation)
				{
					alreadyRegisteredCourses.Add(courseCode);
					continue;
				}

				var courseDoctor = new CourseDoctor
				{
					CourseId = course.Id,  
					DoctorId = doctor.DoctorId  
				};

				await _context.CourseDoctors.AddAsync(courseDoctor);
				successCourses.Add(courseCode);
			}

			await _context.SaveChangesAsync();

			return Ok(new
			{
				success = true,
				message = "Course registration process completed"	
			});
		}


        #endregion

        #region Getting-courses-assigned-to-doctor [Doctor-only] 
        [HttpGet("Get-doctor-courses")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> GetMyCourses()
        {
         
            var userId = User.FindFirstValue("ApplicationUserId");

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { success = false, message = "Invalid token or user not found." });
            }

            
            var doctor = await _context.Doctors
                .Include(d => d.CourseDoctors)
                .ThenInclude(cd => cd.Course)  
                .FirstOrDefaultAsync(d => d.UserId == userId);

            if (doctor == null)
            {
                return NotFound(new { success = false, message = "No doctor profile found for this user." });
            }

           
            var doctorCourses = doctor.CourseDoctors.Select(cd => cd.Course).ToList();
            List<string>Courses = new List<string>();
            foreach(var course in doctorCourses)
            {
                Courses.Add(course.CourseCode);
            }

           
                return Ok(Courses);
            
        }
        #endregion


        #region Deleting Courses [Doctor only]


        #region Deleting Courses [Doctor only]
        [Authorize(Roles = "Doctor")]
        [HttpDelete("Delete-Course")]
        public async Task<IActionResult> DeleteCourse([FromBody] CourseDeletion courseDeletion)
        {
            if (string.IsNullOrEmpty(courseDeletion.CourseCode))
            {
                return BadRequest(new { success = false, message = "Invalid course code." });
            }

            var userId = User.FindFirstValue("ApplicationUserId");

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { success = false, message = "Invalid token or user not found." });
            }

            var doctor = await _context.Doctors
                .Include(d => d.CourseDoctors)
                .ThenInclude(cd => cd.Course)  
                .FirstOrDefaultAsync(d => d.UserId == userId);

            if (doctor == null)
            {
                return NotFound(new { success = false, message = "Doctor profile not found." });
            }

            if (doctor.CourseDoctors == null || !doctor.CourseDoctors.Any())
            {
                return NotFound(new { success = false, message = "Doctor is not assigned to any course." });
            }

            var doctorCourse = doctor.CourseDoctors
                .FirstOrDefault(cd => cd.Course != null && cd.Course.CourseCode == courseDeletion.CourseCode);

            if (doctorCourse == null)
            {
                return NotFound(new { success = false, message = "Course not assigned to you." });
            }

            _context.CourseDoctors.Remove(doctorCourse);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Course removed successfully." });
        }


        #endregion

        #endregion

        #region Removing a Course From Database 

        [Authorize(Roles = "Admin")]
        [HttpDelete("remove-course")]
        public async Task<IActionResult> RemoveCourse(CourseRegistrationDto dto)
        {
            if (!ModelState.IsValid) { return BadRequest(); }
            List<Course> courseFailedToBeDeleted = new List<Course>();
           if (dto.CoursesCodes != null)
            {
                foreach (string CourseCode in dto.CoursesCodes)
                {
                    var course = await _context.Courses.FirstOrDefaultAsync(x => x.CourseCode == CourseCode);
                    if (course !=null)
                    _context.Courses.Remove(course);
                    await _context.SaveChangesAsync();

                }

            }
           return Ok(new { success = true,  message ="Course deleted Successfully from the database " });



        }


        #endregion


    }
}
