using Edu_plat.DTO;
using Edu_plat.Model;
using JWT.DATA;
using JWT.DTO;
using JWT.Services;
using JWT;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Edu_plat.DTO.AdminFiles;

namespace Edu_plat.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminDashBoardController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IMailingServices _mailService;
        private readonly ApplicationDbContext _context;
        // Declare a dictionary where key is a string (email), and value is a tuple (OTP, expiration time, TemporaryUserDTO user)
        private static readonly Dictionary<string, (string Otp, DateTime ExpirationTime, TemporaryUserDTO TempUser)> _otpStore = new Dictionary<string, (string, DateTime, TemporaryUserDTO)>();

        // Declare a dictionary where key is a string (email), and value is a tuple (OTP, expiration time)
        private static readonly Dictionary<string, (string Otp, DateTime ExpirationTime)> _otpStoreFR = new Dictionary<string, (string, DateTime)>();

        #region Dependency Injection
        public AdminDashBoardController(
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration,
            RoleManager<IdentityRole> roleManager,
            IMailingServices mailService,
             ApplicationDbContext context

            )
        {
            _userManager = userManager;
            _configuration = configuration;
            _roleManager = roleManager;
            _mailService = mailService;
            _context = context;
        }
        #endregion

        #region RegisterAdmin

        //// Admin registration (for demo purposes)
        [HttpPost("RegisterAdmin")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> RegisterAdmin([FromBody] RegisterUserDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return Ok(new { success = false, message = "Model state is invalid" });
            }

            var admin = new ApplicationUser
            {
                UserName = dto.UserName,
                Email = dto.Email
            };

            var result = await _userManager.CreateAsync(admin, dto.Password);
            if (result.Succeeded)
            {
                // Assign Admin role
                var roleExist = await _roleManager.RoleExistsAsync("Admin");
                if (!roleExist)
                {
                    await _roleManager.CreateAsync(new IdentityRole("Admin"));
                }
                await _userManager.AddToRoleAsync(admin, "Admin");
                return Ok("Admin user created successfully.");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return Ok(new { success = false, message = "model state is invalid" });
        }
        #endregion

        #region RegisterDoctor 
        // Register Doctor (Only Admin can use this endpoint)
        [HttpPost("RegisterDoctor")]
        [Authorize(Roles = "Admin,SuperAdmin")] // Only Admins can register Doctors
        public async Task<IActionResult> RegisterDoctor([FromBody] RegisterDoctorDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return Ok(new { success = false, message = "Model state is invalid" });
            }

            var doctor = new ApplicationUser
            {
                UserName = dto.UserName,
                Email = dto.Email
            };
            //var existingUser = await _userManager.Users.AnyAsync(u => u.Email == dto.Email);
            //if (existingUser)
            //    return BadRequest(new { success = false, message = "Password or Email is incorrect" });

            var result = await _userManager.CreateAsync(doctor, dto.Password);
            // Save the userId for later use
            var userId = doctor.Id;
            // Step 2: Create and Save a Doctor linked to the ApplicationUser
            var DoctorObj = new Doctor
            {
                UserId = userId, // Foreign key linking to ApplicationUser
                applicationUser = doctor

            };

            //check if the doctor already exists


            _context.Set<Doctor>().Add(DoctorObj);
            _context.SaveChanges();
            if (result.Succeeded)
            {
                // Assign Doctor role
                var roleResult = await _userManager.AddToRoleAsync(doctor, "Doctor");
                if (!roleResult.Succeeded)
                {
                    return Ok(new { success = false, message = "Failed to assign Doctor role." });
                }

                return Ok(new { success = true, Message = "Doctor registered successfully." });
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return Ok(new { success = false, message = "failed" });
        }


        #endregion

        #region GetUsersByType
        [HttpGet("GetUsersByType/{type}")]
        //[Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> GetUsersByType( string type)
        {
            if (string.IsNullOrEmpty(type))
                return BadRequest(new { success = false, message = "Type is required" });

            List<ApplicationUser> users = new List<ApplicationUser>();

            switch (type.ToLower())
            {
                case "admins":
                    users = await _userManager.GetUsersInRoleAsync("Admin") as List<ApplicationUser>;
                    break;
                case "doctors":
                    users = await _userManager.GetUsersInRoleAsync("Doctor") as List<ApplicationUser>;
                    break;
                case "students":
                    users = await _userManager.GetUsersInRoleAsync("Student") as List<ApplicationUser>;
                    break;
                default:
                    return BadRequest(new { success = false, message = "Invalid type. Use 'Admin', 'Doctor', or 'Student'." });
            }


            var totalCount = users.Count;


            var pagedUsers = users  
                .ToList();

            var result = pagedUsers.Select(u => new
            {
                u.Id,
                u.UserName,
                u.Email,
                u.PhoneNumber,
                ProfilePicture = u.profilePicture != null
                                 ? $"data:image/jpeg;base64,{Convert.ToBase64String(u.profilePicture)}"
                                 : null,
                JoinedDate = u.CreatedDate.ToString("yyyy-MM-dd")
            }).ToList();

            return Ok(new
            {
                success = true,
                data = result,
               usersCount= totalCount,
                
            });
        }
        #endregion

        #region DeleteDoctor
        [HttpDelete("DeleteDoctor")]
        [Authorize(Roles = "Admin,SuperAdmin")] // Only Admin or SuperAdmin can delete doctors
        public async Task<IActionResult> DeleteDoctor([FromQuery] string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return Ok(new { success = false, message = "UserId is required" });

            // Find the user by their ID
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Ok(new { success = false, message = "User not found" });

            // Check if the user is in the "Doctor" role
            if (!await _userManager.IsInRoleAsync(user, "Doctor"))
                return Ok(new { success = false, message = "User is not a doctor" });

            // Remove the associated Doctor record from the database, if it exists
            var doctor = await _context.Set<Doctor>().FirstOrDefaultAsync(d => d.UserId == userId);
            if (doctor != null)
            {
                _context.Set<Doctor>().Remove(doctor);
            }

            // Delete the user from Identity
            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                await _context.SaveChangesAsync();
                return Ok(new { success = true, message = "Doctor deleted successfully." });
            }
            else
            {
                return BadRequest(new { success = false, message = "Deletion failed.", errors = result.Errors });
            }
        }

        #endregion

        #region DeleteAdmin
        [HttpDelete("DeleteAdmin")]
        [Authorize(Roles = "SuperAdmin")] // Only SuperAdmin can delete Admin users
        public async Task<IActionResult> DeleteAdmin([FromQuery] string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return BadRequest(new { success = false, message = "UserId is required" });

            // Find the user by their ID
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound(new { success = false, message = "User not found" });

            // Check if the user is in the "Admin" role
            if (!await _userManager.IsInRoleAsync(user, "Admin"))
                return BadRequest(new { success = false, message = "User is not an admin" });

            // Delete the user from Identity
            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                await _context.SaveChangesAsync();
                return Ok(new { success = true, message = "Admin deleted successfully." });
            }
            else
            {
                return BadRequest(new { success = false, message = "Deletion failed.", errors = result.Errors });
            }
        }
        #endregion

        #region DeleteStudent
        [HttpDelete("DeleteStudent")]
        [Authorize(Roles = "Admin,SuperAdmin")] // Only Admin or SuperAdmin can delete student users
        public async Task<IActionResult> DeleteStudent([FromQuery] string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return BadRequest(new { success = false, message = "UserId is required" });

            // Find the user by their ID
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound(new { success = false, message = "User not found" });

            // Check if the user is in the "Student" role
            if (!await _userManager.IsInRoleAsync(user, "Student"))
                return BadRequest(new { success = false, message = "User is not a student" });

            // Remove the associated Student record from the database, if it exists
            var student = await _context.Set<Student>().FirstOrDefaultAsync(s => s.UserId == userId);
            if (student != null)
            {
                _context.Set<Student>().Remove(student);
            }

            // Delete the user from Identity
            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                await _context.SaveChangesAsync();
                return Ok(new { success = true, message = "Student deleted successfully." });
            }
            else
            {
                return BadRequest(new { success = false, message = "Deletion failed.", errors = result.Errors });
            }
        }
        #endregion

        #region Get eduPlat stats
        [HttpGet]
        [Route("GetStats")]
        [Authorize(Roles ="Admin")]
        public async Task<IActionResult> getStats()
        {
            EduPlatStats stats = new EduPlatStats();
            int noOfStudents = _context.Students.Count();
            int noOfDoctors = _context.Doctors.Count();
            int noOfCourses = _context.Courses.Count();
            stats.studentsNum = noOfStudents;
            stats.doctorNum = noOfDoctors;
            stats.coursesNum = noOfCourses;


            return Ok(new { success=true ,message="fetched Successfully",stats});


        }


        #endregion



    }
}
