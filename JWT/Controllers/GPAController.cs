using Edu_plat.DTO.GPA;
using Edu_plat.Requests;
using JWT;
using JWT.DATA;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace Edu_plat.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GPAController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public GPAController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private readonly Dictionary<string, double> gradePoints = new Dictionary<string, double>
        {
            { "A", 4.000 },
            { "A-", 3.670 },
            { "B+", 3.330 },
            { "B", 3.000 },
            { "C+", 2.670 },
            { "C", 2.330 },
            { "D", 2.000 },
            { "F", 0.000 }
        };

        #region Calc Gpa
        [HttpPost("calculateGPA")]
        public ActionResult<double> CalculateGPA([FromBody] List<GradeInputDto> grades)
        {
            double totalPoints = 0;
            int totalCreditHours = 0;

            foreach (var grade in grades)
            {
                double Grade;
                bool isNumber = double.TryParse(grade.Grade, out Grade);

                if (!isNumber)
                {
                    if (gradePoints.ContainsKey(grade.Grade))
                    {
                        Grade = gradePoints[grade.Grade];
                    }
                    else
                    {
                        return Ok(new { success = false, message = "Invalid grade value." });
                    }
                }

                totalPoints += Grade * 1000 * grade.CreditHours;
                totalCreditHours += grade.CreditHours;
            }

            if (totalCreditHours == 0)
            {
                return Ok(new { success = false, message = "Total credit hours cannot be zero." });
            }

            double gpa = totalPoints / (totalCreditHours * 1000);
            return Ok(new {success=true ,message=gpa});
        }
        #endregion

        // Endpoint 1: Update the GPA
        #region Update GPA
        [HttpPost("UpdateGPA")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> UpdateGPA([FromBody] StudentUpdateGpa gpa)
        {

            // Get the UserId from the token
            var userId = User.FindFirstValue("ApplicationUserId");
            if (string.IsNullOrEmpty(userId))
            {
                return Ok(new { success = false, message = "Invalid User credentials" });
            }
            // Check if the user exists
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Unauthorized(new { success = false, message = "User not found." });
            }

            // Check if the student exists
            var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId);
            if (student == null)
            {
                return NotFound(new { success = false, message = "Student not found." });
            }

            // Update the GPA
            student.GPA = gpa.GPA;
            _context.Students.Update(student);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "GPA updated successfully." });
        }

        #endregion

        // Endpoint 2: Retrieve the GPA
        #region Get GPA
        [HttpGet("GetGPA")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetGPA()
        {
            // Get the UserId from the token
            var userId = User.FindFirstValue("ApplicationUserId");
            if (string.IsNullOrEmpty(userId))
            {
                return Ok(new { success = false, message = "Invalid User credentials" });
            }
            // Check if the user exists
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Unauthorized(new { success = false, message = "User not found." });
            }

            // Retrieve student data
            var student = await _context.Students
                .Where(s => s.UserId == userId)
                .Select(s => new { s.StudentId, s.GPA })
                .FirstOrDefaultAsync();

            if (student == null)
            {
                return NotFound(new { success = false, message = "Student not found." });
            }

            return Ok(new { success = true, message=student.GPA });
        }
        #endregion
    }
}