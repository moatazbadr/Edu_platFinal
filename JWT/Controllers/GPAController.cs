using Edu_plat.DTO.GPA;
using JWT.DATA;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace Edu_plat.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class GPAController : ControllerBase
	{
		private readonly ApplicationDbContext _context;

		public GPAController(ApplicationDbContext context)
		{
			_context = context;
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
						return BadRequest("Invalid grade value.");
					}
				}

				totalPoints += Grade*1000 * grade.CreditHours;
				totalCreditHours += grade.CreditHours;
			}

			if (totalCreditHours == 0)
			{
				return BadRequest("Total credit hours cannot be zero.");
			}

			double gpa = totalPoints / (totalCreditHours *1000) ;
			return Ok(gpa);
		}
	}
}
