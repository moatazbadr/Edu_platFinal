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
using Edu_plat.Requests;
using Microsoft.VisualBasic;

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

            var alreadyCourse =  _context.Courses.Where(c => c.CourseCode == courseFromBody.CourseCode);

            if (alreadyCourse.Any()) { 
            return Ok(new { success=false ,message="Course with that code already exists"});
            
            }

            if (courseFromBody.Course_hours == 3 &&courseFromBody.has_Lab==false)
            {
                int sum = 0;
                
                sum += courseFromBody.MidTerm;
                sum += courseFromBody.Oral;
                if (sum != 45)
                {
                    return Ok(new { success = false, message = "grades are not adding up" });
                }
                if (courseFromBody.TotalMark != 150)
                {
                    return Ok(new { success = false, message = "grades are not adding up" });
                }

                if (courseFromBody.FinalExam != 105)
                {
                    return Ok(new { success = false, message = "grades are not adding up" });

                }
            }

            if (courseFromBody.Course_hours == 3 && courseFromBody.has_Lab == true)
            {
                int sum = 0;

                sum += courseFromBody.MidTerm;
                sum += courseFromBody.Oral;
                sum += courseFromBody.Lab;
                if (sum != 60)
                {
                    return Ok(new { success = false, message = "grades are not adding up" });
                }
                if (courseFromBody.FinalExam != 90)
                {
                    return Ok(new { success = false, message = "grades are not adding up" });

                }
                if (courseFromBody.TotalMark != 150)
                {
                    return Ok(new { success = false, message = "grades are not adding up" });

                }

            }


            var course = new Course()
            {
                CourseCode = courseFromBody.CourseCode,
                CourseDescription = courseFromBody.CourseDescription,
                Course_hours = courseFromBody.Course_hours,
                
                has_Lab = courseFromBody.has_Lab,
                
                TotalMark = courseFromBody.TotalMark,
                FinalExam = courseFromBody.FinalExam,
                Oral = courseFromBody.Oral,
                Lab = courseFromBody.Lab,
                MidTerm = courseFromBody.MidTerm,

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
            if (!_context.Courses.Any())
            {
                return BadRequest("No courses available");
            }
            var courses = await _context.Courses
                .Include(c => c.CourseDoctors)
                .ThenInclude(cd => cd.Doctor)
                .ThenInclude(d => d.applicationUser) // To get the username from ApplicationUser
                .Select(c => new
                {
                    CourseCode = c.CourseCode,
                     Doctors = c.CourseDoctors.Select(cd => cd.Doctor.applicationUser.UserName).ToList(),
                    CourseDescription = c.CourseDescription,
                    courseHours = c.Course_hours,
                    
                    c.has_Lab,
                    c.FinalExam,
                    c.Oral,
                    c.Lab,
                    c.TotalMark,
                    c.MidTerm,
                    c.Course_level,
                    c.Course_semster,
                    
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

            //if (semester != 1 || semester != 2)
            //{
            //    return Ok(new { success = false, message = "Invalid Semester" });
            //}
            
            var coursesBySemesterAndLevel = await _context.Courses
                .Where(x => x.Course_level == level && x.Course_semster == semester && x.isRegistered == false).Select(x=>new
                {
                    x.CourseCode,
                    
                    x.MidTerm,
                    x.FinalExam,
                    x.Oral,
                    x.TotalMark,
                    x.has_Lab,
                    
                    
                })
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
            if (string.IsNullOrEmpty(userId))
            {
                return Ok(new { success = false, message = "Unable to find user" });
            }

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
            List<string> fullCourses = new List<string>();
            List<string> failureCourses = new List<string>();

            foreach (string courseCode in courseRegistrationDto.CoursesCodes)
            {
                var course = await _context.Courses.FirstOrDefaultAsync(x => x.CourseCode == courseCode);

                if (course == null)
                {
                    failureCourses.Add(courseCode);
                    continue;
                }

                // Count how many doctors are already registered for this course
                int registeredDoctors = await _context.CourseDoctors
                    .CountAsync(cd => cd.CourseId == course.Id);

                if (registeredDoctors >= 2)
                {
                    fullCourses.Add(courseCode); // Course is full
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
            if (fullCourses.Any())
            {
                return Ok(new { success = false, message = "this course is already registered by two doctors" });
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "Course registration process completed",
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


            var doctorCourses = doctor.CourseDoctors.Select(cd => cd.Course)
                .Select(cd => new { 
                    cd.CourseCode,
                    cd.has_Lab
                }).ToList();
                
            //List<string>Courses = new List<string>();
            //foreach(var course in doctorCourses)
            //{
            //    Courses.Add(course.CourseCode);
            //}

           
                return Ok( doctorCourses );
            
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

        [HttpDelete("remove-course")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveCourse(CourseDeletion dto)
        {
            if (!ModelState.IsValid) { return BadRequest(); }
          if (string.IsNullOrEmpty(dto.CourseCode))
            {
                return Ok(new { success = false, message = "Course Code not Entered"});
            }

            var courseRequired = await _context.Courses.FirstOrDefaultAsync(c => c.CourseCode.Equals(dto.CourseCode));
            if (courseRequired==null) {
                return Ok(new { success = false, message = "couldn't delete the course" });
            }
            _context.Courses.Remove(courseRequired);
            await _context.SaveChangesAsync();
           return Ok(new { success = true,  message ="Course deleted Successfully from the database " });



        }


        #endregion

        #region getting course details [For student] 
        [HttpGet("Details/{CourseCode}/{DoctorId}")]
      //  [Authorize(Roles ="Doctor,Student")]
        public async Task<IActionResult> getCourseDetails(string CourseCode,string DoctorId)
        {
            if (!ModelState.IsValid) {
                return Ok(new { success = false, message = "Invalid Information" });
            }

            if (string.IsNullOrEmpty(CourseCode))
            {
                return Ok(new { success = false, message = "Invaild Course code " });
            }

            var CourseRequired = await _context.Courses.Include(c=>c.CourseDoctors).Include(cm=>cm.Materials).
                FirstOrDefaultAsync(c=>c.CourseCode == CourseCode) 
                ;
            if (CourseRequired == null)
            {
                return Ok(new { success = false, message = "no courses have been found" });
            }

           var Doctor = await _userManager.FindByIdAsync(DoctorId);
            if (Doctor == null)
            {
                return Ok(new { success = false, message = "no doctor found " });

            }
            var LectureCount = CourseRequired.Materials.Where(cm => cm.TypeFile == "Lectures").ToList().Count();
            CourseDetailsResponse course = new CourseDetailsResponse()
            {
                CourseCode = CourseRequired.CourseCode,
                CourseCreditHours = CourseRequired.Course_hours,
                TotalMark = CourseRequired.TotalMark,
                FinalExam = CourseRequired.FinalExam,
                Lab = CourseRequired.Lab,
                MidTerm = CourseRequired.MidTerm,
                Oral = CourseRequired.Oral,
                CourseDescription = CourseRequired.CourseDescription,
                // doctorCount = CourseRequired.CourseDoctors.Count(),
                LectureCount = LectureCount,
                doctorName = Doctor.UserName
            };


            return Ok(
                new
                {
                    success = true,
                    message = "fetched successfully",
                    courseDetails = new
                    {
                        course.CourseCode,
                        course.doctorName,
                        course.CourseCreditHours,
                        course.CourseDescription,
                        course.LectureCount,

                        Grading =
                                new
                                {
                                
                                 MidTerm= course.MidTerm,
                                 Oral= course.Oral,
                                 TotalMark = course.TotalMark,
                                 Lab = course.Lab,
                                 FinalExam = course.FinalExam
                                
                                }
                    }
                }
                );
        }


        #endregion

        #region Getting Course Doctor Details
        [HttpGet("Details/{CourseCode}")]
        public async Task<IActionResult> GetDetails(string CourseCode)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Invalid Information" });

            if (string.IsNullOrEmpty(CourseCode))
                return BadRequest(new { success = false, message = "Invalid course code" });

            var courseRequired = await _context.Courses
                                        .Include(c => c.CourseDoctors)
                                        .ThenInclude(cd => cd.Doctor)  // Load doctor details in one query
                                        .FirstOrDefaultAsync(c => c.CourseCode == CourseCode);

            if (courseRequired == null)
                return NotFound(new { success = false, message = "No course with that code was found" });

            // Fetch user details for doctors in one query instead of looping
            var doctorUserIds = courseRequired.CourseDoctors
                                              .Select(cd => cd.Doctor.UserId)
                                              .Where(userId => !string.IsNullOrEmpty(userId))
                                              .ToList();

            var doctorUsers = await _userManager.Users
                                               .Where(u => doctorUserIds.Contains(u.Id))
                                               .Select(
                                         u => new DoctorDetailsResponse
                                               {
                                                   DoctorId = u.Id,
                                                   Name = u.UserName
                                               }).ToListAsync();

            return Ok(new { success = true,message="fetched Successfully", courseDoctors = doctorUsers });
        }



        #endregion

        #region Getting course Details
        [HttpGet]
        [Route("ExamDetails/{coursecode:required}")]
        public async Task<IActionResult>getDetailsForExam(string coursecode)
        {
            if (string.IsNullOrEmpty(coursecode))
            {
                return Ok(new { success = false, message = "course code cannot be empty" });
            }

            //where returns a list 
            var courseRequired = await _context.Courses.FirstOrDefaultAsync(x => x.CourseCode==coursecode);
            if (courseRequired == null)
            {
                return Ok(new { success = false, message = "No course with this code has been found" });
            }
            courseForExamDetails examDetails = new courseForExamDetails()
            {
                courseCode = courseRequired.CourseCode,
                level = courseRequired.Course_level,
                program = "Computer science",
                semester = courseRequired.Course_semster,
                time = courseRequired.Course_hours,
                title = courseRequired.CourseDescription,
                totalMarks = courseRequired.FinalExam
            };

            if(examDetails != null)
            {
                return Ok(new { success = true, message = "fetched successfully", examDetails });
            }

            return Ok(new { success = false, message = "error in getting exam details" });


        }


        #endregion

        #region updatecourseDetails
        [HttpPut]
        [Route("update/{courseCode:required}")]

        [Authorize(Roles ="Admin")]
        public async Task<IActionResult> updatecourseDetails(string courseCode, CourseRegisteration newCourse)
        {
            if (string.IsNullOrEmpty(courseCode))
            {
                return Ok(new { success = false, message = "course code cannot be empty" });
            }
            var requiredCourse = await _context.Courses.FirstOrDefaultAsync(x => x.CourseCode == courseCode);
            if (requiredCourse == null)
            {
                return Ok(new { success = false, message = "couldn't find the course" });
            }

            if(newCourse==null || string.IsNullOrEmpty(newCourse.CourseCode))
            {
                return Ok(new { success = false, message = "cannot update course missing details" });
            }
            //Grading validation

            if (newCourse.Course_hours == 3 && newCourse.has_Lab == false)
            {
                int sum = 0;

                sum += newCourse.MidTerm;
                sum += newCourse.Oral;
                if (sum != 45)
                {
                    return Ok(new { success = false, message = "grades are not adding up" });
                }
                if (newCourse.TotalMark != 150)
                {
                    return Ok(new { success = false, message = "grades are not adding up" });
                }

                if (newCourse.FinalExam != 105)
                {
                    return Ok(new { success = false, message = "grades are not adding up" });

                }
            }

            if (newCourse.Course_hours == 3 && newCourse.has_Lab == true)
            {
                int sum = 0;

                sum += newCourse.MidTerm;
                sum += newCourse.Oral;
                sum += newCourse.Lab;
                if (sum != 60)
                {
                    return Ok(new { success = false, message = "grades are not adding up" });
                }
                if (newCourse.FinalExam != 90)
                {
                    return Ok(new { success = false, message = "grades are not adding up" });

                }
                if (newCourse.TotalMark != 150)
                {
                    return Ok(new { success = false, message = "grades are not adding up" });

                }

            }
            //mapping
            requiredCourse.MidTerm = newCourse.MidTerm;
            requiredCourse.CourseDescription = newCourse.CourseDescription;
            requiredCourse.Course_level = newCourse.Course_level;
            requiredCourse.Course_hours = newCourse.Course_hours;
            requiredCourse.has_Lab = newCourse.has_Lab;
            requiredCourse.Oral = newCourse.Oral;

            return Ok(new { success = false, message = "updated sucessfully" });



        }

        #endregion
    }
}
