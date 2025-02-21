using Edu_plat.DTO.FileRequester;
using Edu_plat.DTO.UploadFiles;
using Edu_plat.Model;
using Edu_plat.Model.Course_registeration;
using JWT;
using JWT.DATA;
using JWT.Migrations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO.Compression;
using System.Linq;
using System.Net.Mime;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace Edu_plat.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class MaterialsController : ControllerBase
	{
        private readonly ApplicationDbContext _context;
		private readonly IWebHostEnvironment _hostingEnvironment;
		private readonly UserManager<ApplicationUser> _userManager;
        public MaterialsController(ApplicationDbContext context, IWebHostEnvironment hostingEnvironment, UserManager<ApplicationUser> userManager = null)
		{
			_context = context;
			_hostingEnvironment = hostingEnvironment;
			_userManager = userManager;
		} 
		
        #region UploadFile
		[HttpPost("UploadFile/Doctors")]
		[Authorize(Roles = "Doctor")]
		public async Task<IActionResult> UploadMaterial([FromForm] UploadMatarielDto uploadMaterialDto)
		{
			// Validate the input data
			if (!ModelState.IsValid)
			{
				return Ok(new { success = false, message = "Invalid data provided." });
			}
			// Check if the file is provided (not empty)
			if (uploadMaterialDto.File == null || uploadMaterialDto.File.Length == 0)
			{
				return Ok(new { success = false, message = "No file uploaded." });
			}
			// Set maximum file size (10 MB)
			var maxFileSize = 10 * 1024 * 1024; // 10 MB
			if (uploadMaterialDto.File.Length > maxFileSize)
			{
				return Ok(new { success = false, message = "File size exceeds the maximum limit (10 MB)." });
			}
			// Allowed MIME types (ContentTypes) for PDF, Word, and PowerPoint files
			var contentType = uploadMaterialDto.File.ContentType.ToLower();
			var fileExtension = Path.GetExtension(uploadMaterialDto.File.FileName).ToLower();

			var allowedContentTypes = new Dictionary<string, string>
			 {
		       {  ".pdf", "application/pdf" },
		       { ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
		       { ".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation" }
	         };

			// Check Extension of File
			if (!allowedContentTypes.ContainsKey(fileExtension))
			{
				return BadRequest(new { success = false, message = "Only PDF, Word, and PowerPoint files are allowed." });
			}
			// Check content type for better security
			if (allowedContentTypes[fileExtension] != uploadMaterialDto.File.ContentType.ToLower())
			{
				return BadRequest(new { success = false, message = "File type does not match its extension." });
			}

			// Check if the course exists
			var course = await _context.Courses
				.Where(c => c.CourseCode == uploadMaterialDto.CourseCode)
				.FirstOrDefaultAsync();

			if (course == null)
			{
				return Ok(new { success = false, message = "Course not found." });
			}

			// Get UserId from token
			var userId = User.FindFirstValue("AppicationUserId");
			var user = await _userManager.FindByIdAsync(userId);
			if (user == null)
			{
				return Ok(new { success = false, message = "User not found." });
			}

			// Check if the user (Doctor) has enrolled in this course
			bool isDoctorEnrolled = await _context.CourseDoctors
				.AnyAsync(cd => cd.Doctor.UserId == userId && cd.CourseId == course.Id);

			if (!isDoctorEnrolled)
			{
				return Ok( new { success = false, message = "Doctor is not enrolled in the course, so file upload is forbidden." });
			}

			// Get the doctor details from CourseDoctors table
			var doctor = await _context.CourseDoctors
				.Where(cd => cd.CourseId == course.Id && cd.Doctor.UserId == userId)
				.FirstOrDefaultAsync();

			// Define upload directory inside wwwroot/Uploads/{CourseCode}
			var uploadsDirectory = Path.Combine(_hostingEnvironment.WebRootPath, "Uploads", course.CourseCode);

			// Create the directory if it does not exist
			if (!Directory.Exists(uploadsDirectory))
			{
				Directory.CreateDirectory(uploadsDirectory);
			}

			// Generate the file name (check if the file already exists in the directory)
			var fileBaseName = Path.GetFileNameWithoutExtension(uploadMaterialDto.File.FileName);
			//var fileExtension = Path.GetExtension(uploadMaterialDto.File.FileName).ToLower();

			// Check if a file with the same base name exists in the directory
			var existingFiles = Directory.GetFiles(uploadsDirectory, $"{fileBaseName}*");

			// If the file already exists, append a serial number to the name
			var uniqueFileName = existingFiles.Length > 0
				? $"{fileBaseName}_{existingFiles.Length + 1}{fileExtension}"
				: $"{fileBaseName}{fileExtension}"; // No need for a serial number if it's the first file

			// Define the final file path for saving the file
			var filePath = Path.Combine(uploadsDirectory, uniqueFileName);

			// Save the file to the specified path
			using (var stream = new FileStream(filePath, FileMode.Create))
			{
				await uploadMaterialDto.File.CopyToAsync(stream);
			}

			//Initialize file size variable
			string fileSize = "Unknown";

			// Check if file exists before calculating its size
			if (System.IO.File.Exists(filePath))
			{
				long fileSizeBytes = new FileInfo(filePath).Length;
				fileSize = (fileSizeBytes / (1024.0 * 1024.0)).ToString("F2") + " MB"; // Convert to MB
			}
			//var materialExtension=

			var material = new Material
			{
				CourseId = course.Id,  // Use the course ID from CourseCode
				CourseCode = course.CourseCode,
				DoctorId = doctor.DoctorId,   // Use the DoctorId from the CourseDoctors table
				FilePath = $"/Uploads/{course.CourseCode}/{uniqueFileName}",  // Final file path
				FileName = uniqueFileName,
				Description = string.Empty,
				UploadDate = DateTime.Now,
				TypeFile = uploadMaterialDto.Type,
				Size = fileSize

			};

			_context.Materials.Add(material);
			await _context.SaveChangesAsync();

			double fileSizeInMB = uploadMaterialDto.File.Length / (1024.0 * 1024.0);
			Console.WriteLine(fileSizeInMB);

			//file Details Dto 


			//"File details ":{
			// id :
			// path :
			// etxenstion :
			// date :
			// type :
			// courseCode :
			//}


			return Ok(new
			{
				success = true,
				message = "File uploaded successfully.",
				FileDetails = new
				{
					Id = material.Id, // Send the material ID in the response
					Path = material.FilePath,
					Size = fileSize,
					Eextension = fileExtension,
					Type = uploadMaterialDto.Type,
					CoourseCode= course.CourseCode,
                }

			});
		}
		#endregion

		#region RetriveAllFiles 
		[HttpGet("AllFiles")]
		public async Task<IActionResult> GetAllFiles()
		{
			var materials = await _context.Materials.ToListAsync();

			if (materials == null || !materials.Any())
			{
				return NotFound("No materials found.");
			}

			return Ok(materials);
		}
		#endregion

		#region Method Pass MaterialId Return FileName
		private string GetFileNameByMaterialId(int materialId)
		{

			var material = _context.Materials.FirstOrDefault(m => m.Id == materialId);
			if (material != null)
			{
				return material.FileName;
			}
			return null;
		}
		#endregion

		#region DownloadFile/{courseCode}/{materialId}
		[HttpGet("DownloadFile/{courseCode}/{materialId}")]
		public async Task<IActionResult> DownloadFile(string courseCode, int materialId)
		{
			// Check if the course exists
			var course = _context.Courses.FirstOrDefault(c => c.CourseCode == courseCode);
			if (course == null)
			{
				return NotFound(new { success = false, message = "Course not found for the given course code." });
			}

			//  Check if the material exists in the specified course
			var material = _context.Materials.FirstOrDefault(m => m.Id == materialId && m.CourseCode == courseCode);
			if (material == null)
			{
				return NotFound(new { success = false, message = "Material not found for the given material ID in the specified course." });
			}

			// Define the directory path where uploaded files are stored
			var uploadsDirectory = Path.Combine(_hostingEnvironment.WebRootPath, "Uploads", courseCode);

			//  Get the file name associated with the given material ID
			var fileName = GetFileNameByMaterialId(materialId);

			// If file name is empty or invalid, return a 404 response
			if (string.IsNullOrEmpty(fileName))
			{
				return NotFound(new { success = false, message = "File not found for the given material ID." });
			}

			// Construct the full file path
			var filePath = Path.Combine(uploadsDirectory, fileName);

			//  Check if the file exists
			if (!System.IO.File.Exists(filePath))
			{
				return NotFound(new { success = false, message = "File not found." });
			}

			// Dictionary of supported MIME types for different file formats
			var mimeTypes = new Dictionary<string, string>
	         {
	        	{ ".pdf", "application/pdf" },
		        { ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
		        { ".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation" }
	        };

			// Get the file extension to determine the correct MIME type
			var fileExtension = Path.GetExtension(filePath).ToLower();

			// Set the appropriate Content-Type or fallback to "application/octet-stream"
			var contentType = mimeTypes.ContainsKey(fileExtension) ? mimeTypes[fileExtension] : "application/octet-stream";

			// Read the file bytes and return it to the user with the correct Content-Type
			var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
			return File(fileBytes, contentType, fileName);
		}
		#endregion

    	#region DownloadFile/{materialId}
		[HttpGet("DownloadFile/{materialId}")]
		public async Task<IActionResult> DownloadFile(int materialId)
		{
			// Check if the material exists
			var material = _context.Materials.FirstOrDefault(m => m.Id == materialId);
			if (material == null)
			{
				return NotFound(new { success = false, message = "Material not found for the given material ID." });
			}

			// check exist course or not throgth materialId
			var course = _context.Courses.FirstOrDefault(c => c.CourseCode == material.CourseCode);
			if (course == null)
			{
				return NotFound(new { success = false, message = "Course not found for the given material." });
			}

			 // get fileName througth materialId
			var fileName = GetFileNameByMaterialId(materialId);
			if (string.IsNullOrEmpty(fileName))
			{
				return NotFound(new { success = false, message = "File name is not found for the given material." });
			}

			// Define the directory that contains the uploaded files based on the CourseCode
			var uploadsDirectory = Path.Combine(_hostingEnvironment.WebRootPath, "Uploads", course.CourseCode);

			// Define the full file path based on the file name
			var filePath = Path.Combine(uploadsDirectory, fileName);

			// Check if the file exists
			if (!System.IO.File.Exists(filePath))
			{
				return NotFound(new { success = false, message = "File not found." });
			}

			var mimeTypes = new Dictionary<string, string>
	        {
	     	   { ".pdf", "application/pdf" },
		       { ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
		       { ".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation" }
	        };

			var fileExtension = Path.GetExtension(fileName).ToLower();
         	var contentType = mimeTypes.ContainsKey(fileExtension) ? mimeTypes[fileExtension] : "application/octet-stream";
			var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
			return File(fileBytes, contentType, fileName);
		}
		#endregion

        #region DownloadBypath

		[HttpGet("Download-path")]
		public async Task<IActionResult> DownloadMaterialbYpATH([FromBody] FilePathDto filePath)
		{
			// Ensure file path is provided
			if (string.IsNullOrEmpty(filePath.Path))
			{
				return BadRequest(new { success = false, message = "File path is required." });
			}

			// Build the full file path on the server
			var fullFilePath = Path.Combine(_hostingEnvironment.WebRootPath, filePath.Path.TrimStart('/'));

			// Check if the file exists at the given location
			if (!System.IO.File.Exists(fullFilePath))
			{
				return NotFound(new { success = false, message = "File not found." });
			}

			// Read file content
			var fileBytes = await System.IO.File.ReadAllBytesAsync(fullFilePath);

			// Get file name and extension
			var fileName = Path.GetFileName(fullFilePath);
			var fileExtension = Path.GetExtension(filePath.Path).ToLower();

			// Define MIME types help to known type of file to open easy 
			var mimeTypes = new Dictionary<string, string>
			{
			   { ".pdf", "application/pdf" },
			   { ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
			   { ".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation" }
			};

			// Determine correct content type
			var contentType = mimeTypes.ContainsKey(fileExtension) ? mimeTypes[fileExtension] : "application/octet-stream";

			return File(fileBytes, contentType, fileName);
		}

		#endregion

		#region DownloadFiles/{materialIds}
		[HttpGet("DownloadFiles")]
		public async Task<IActionResult> DownloadFiles([FromQuery] List<int> materialIds)
		{
			// Create a temporary directory to store the files
			var tempDirectory = Path.Combine(_hostingEnvironment.WebRootPath, "TempFiles");
			if (!Directory.Exists(tempDirectory))
			{
				Directory.CreateDirectory(tempDirectory);
			}

			var zipFilePath = Path.Combine(tempDirectory, "materials.zip");

			// Create a zip file
			using (var zip = new ZipArchive(new FileStream(zipFilePath, FileMode.Create), ZipArchiveMode.Create))
			{
				foreach (var materialId in materialIds)
				{
					// Check if the material exists
					var material = _context.Materials.FirstOrDefault(m => m.Id == materialId);
					if (material == null)
					{
						continue; // Skip if material is not found
					}

					// Check if the course exists
					var course = _context.Courses.FirstOrDefault(c => c.CourseCode == material.CourseCode);
					if (course == null)
					{
						continue; // Skip if course is not found
					}

					// Get the file name
					var fileName = GetFileNameByMaterialId(materialId);
					if (string.IsNullOrEmpty(fileName))
					{
						continue; // Skip if file name is not found
					}

					// Define the full file path based on the file name
					var uploadsDirectory = Path.Combine(_hostingEnvironment.WebRootPath, "Uploads", course.CourseCode);
					var filePath = Path.Combine(uploadsDirectory, fileName);

					// Check if the file exists
					if (!System.IO.File.Exists(filePath))
					{
						continue; // Skip if the file does not exist
					}

					// Add file to the ZIP archive
					zip.CreateEntryFromFile(filePath, fileName);
				}
			}

			// Return the zip file
			var fileBytes = await System.IO.File.ReadAllBytesAsync(zipFilePath);
			//System.IO.File.Delete(zipFilePath); // Clean up the temporary file

			return File(fileBytes, "application/zip", "materials.zip");
		}
		#endregion


		//--------------------------------------------------------------------------------------------------------------------------------------------------

	  
	    #region GetDoctorMaterials
		[HttpGet("GetDoctorMaterials/Doctor")]
		[Authorize(Roles = "Doctor")]
		public async Task<IActionResult> GetDoctorMaterials()
		{
			// 🔹 Get the UserId from the token
			var userId = User.FindFirstValue("AppicationUserId");

			// 🔹 Check if the user exists
			var user = await _userManager.FindByIdAsync(userId);
			if (user == null)
			{
				return Unauthorized(new { success = false, message = "User not found." });
			}

			// 🔹 Retrieve materials uploaded by this doctor
			var doctorMaterials = await _context.Materials
				.Where(m => m.Doctor.UserId == userId)
				.Select(m => new
				{
					m.Id,
					m.FileName,
					m.FilePath,
					m.CourseCode,
					m.UploadDate,
					m.TypeFile
				})
				.ToListAsync();

			// 🔹 Check if the doctor has any uploaded materials
			if (!doctorMaterials.Any())
			{
				return Ok(new { success = true, message = "No materials found for this doctor.", materials = new List<object>() });
			}

			// 🔹 Return the materials in JSON format
			return Ok(new { success = true, materials = doctorMaterials });
		}
		#endregion

		// Not Used
		#region GetDoctorsLectures
		//[HttpGet("GetDoctorLectures/Doctor")]
		//[Authorize(Roles = "Doctor")]
		//public async Task<IActionResult> GetDoctorLectures()
		//{
		//	//  Get the UserId from the token
		//	var userId = User.FindFirstValue("AppicationUserId");

		//	//  Check if the user exists
		//	var user = await _userManager.FindByIdAsync(userId);
		//	if (user == null)
		//	{
		//		return Unauthorized(new { success = false, message = "User not found." });
		//	}

		//	// Retrieve only materials where TypeFile is "lec"
		//	var doctorLectures = await _context.Materials
		//		.Where(m => m.Doctor.UserId == userId && m.TypeFile.ToLower() == "Material")
		//		.Select(m => new
		//		{
		//			m.Id,
		//			m.FileName,
		//			m.FilePath,
		//			m.CourseCode,
		//			m.Description,
		//			m.UploadDate,
		//			m.TypeFile
		//		})
		//		.ToListAsync();

		//	//  Check if there are any lecture materials
		//	if (!doctorLectures.Any())
		//	{
		//		return Ok(new { success = true, message = "No lecture materials found for this doctor.", materials = new List<object>() });
		//	}

		//	//  Return the lectures in JSON format
		//	return Ok(new { success = true, materials = doctorLectures });
		//}
		#endregion

		// Not Used
		#region GetDoctorLabs

		//[HttpGet("GetDoctorLabs/Doctor")]
		//[Authorize(Roles = "Doctor")]
		//public async Task<IActionResult> GetDoctorLabs()
		//{
		//	// 🔹 Get the UserId from the token
		//	var userId = User.FindFirstValue("AppicationUserId");

		//	// 🔹 Check if the user exists
		//	var user = await _userManager.FindByIdAsync(userId);
		//	if (user == null)
		//	{
		//		return Unauthorized(new { success = false, message = "User not found." });
		//	}

		//	// 🔹 Retrieve only materials where TypeFile is "lec"
		//	var doctorLectures = await _context.Materials
		//		.Where(m => m.Doctor.UserId == userId && m.TypeFile.ToLower() == "Labs")
		//		.Select(m => new
		//		{
		//			m.Id,
		//			m.FileName,
		//			m.FilePath,
		//			m.CourseCode,
		//			m.Description,
		//			m.UploadDate,
		//			m.TypeFile
		//		})
		//		.ToListAsync();

		//	// 🔹 Check if there are any lecture materials
		//	if (!doctorLectures.Any())
		//	{
		//		return Ok(new { success = true, message = "No labs materials found for this doctor.", materials = new List<object>() });
		//	}

		//	// 🔹 Return the lectures in JSON format
		//	return Ok(new { success = true, materials = doctorLectures });
		//}
		#endregion

		// Not Used
		#region GetDoctorExam

		//[HttpGet("GetDoctorExamss/Doctor")]
		//[Authorize(Roles = "Doctor")]
		//public async Task<IActionResult> GetDoctorExams()
		//{
		//	// 🔹 Get the UserId from the token
		//	var userId = User.FindFirstValue("AppicationUserId");

		//	// 🔹 Check if the user exists
		//	var user = await _userManager.FindByIdAsync(userId);
		//	if (user == null)
		//	{
		//		return Unauthorized(new { success = false, message = "User not found." });
		//	}

		//	// 🔹 Retrieve only materials where TypeFile is "lec"
		//	var doctorLectures = await _context.Materials
		//		.Where(m => m.Doctor.UserId == userId && m.TypeFile.ToLower() == "Exams")
		//		.Select(m => new
		//		{
		//			m.Id,
		//			m.FileName,
		//			m.FilePath,
		//			m.CourseCode,
		//			m.Description,
		//			m.UploadDate,
		//			m.TypeFile
		//		})
		//		.ToListAsync();

		//	// 🔹 Check if there are any lecture materials
		//	if (!doctorLectures.Any())
		//	{
		//		return Ok(new { success = true, message = "No exam materials found for this doctor.", materials = new List<object>() });
		//	}

		//	// 🔹 Return the lectures in JSON format
		//	return Ok(new { success = true, materials = doctorLectures });
		//}
		#endregion


		#region GetDoctorMaterialsByTypeAndCourse

		[HttpGet("GetDoctorMaterialsByTypeAndCourse")]
		[Authorize(Roles = "Doctor")]
		public async Task<IActionResult> GetDoctorMaterialsByTypeAndCourse([FromQuery] string typeFile, [FromQuery] string courseCode)
		{
			// Get the UserId from the token
			var userId = User.FindFirstValue("AppicationUserId");

			// Check if the user exists
			var user = await _userManager.FindByIdAsync(userId);
			if (user == null)
			{
				return Unauthorized(new { success = false, message = "User not found." });
			}

			// Validate that the typeFile and courseCode are provided
			if (string.IsNullOrEmpty(typeFile) || string.IsNullOrEmpty(courseCode))
			{
				return BadRequest(new { success = false, message = "TypeFile and CourseCode are required." });
			}

			var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseCode == courseCode);
			if (course == null)
			{
				return NotFound(new { success = false, message = "Course not found." });
			}
			bool isDoctorEnrolled = await _context.CourseDoctors
				.AnyAsync(cd => cd.Doctor.UserId == userId && cd.CourseId == course.Id);

			if (!isDoctorEnrolled)
			{
				return StatusCode(403, new { success = false, message = "Doctor is not enrolled in this course." });
			}

		
			// Retrieve materials based on the provided typeFile and courseCode
			var doctorMaterials = await _context.Materials
				.Where(m => m.Doctor.UserId == userId
							&& m.TypeFile.ToLower() == typeFile.ToLower()
							&& m.CourseCode.ToLower() == courseCode.ToLower())
				.Select(m => new
				{
					m.Id,
					m.FileName,
					m.FilePath,
					m.CourseCode,
					
					UploadDateFormatted = m.UploadDate.ToString("yyyy-MM-dd HH:mm:ss"), // 2025-02-16 11:56:01m.TypeFile,
					FileExtension = Path.GetExtension(m.FileName),
					m.Size

				})
				.ToListAsync();
		
			// 🔹 Check if there are any materials for the given typeFile and courseCode
			if (!doctorMaterials.Any())
			{
				return Ok(new { success = true, message = $"No {typeFile}  found for this doctor in course {courseCode}.", materials = new List<object>() });
			}

			// Return the materials in JSON format
			return Ok(new { success = true, materials = doctorMaterials });
		
			
		}
		#endregion


		// -------------------------------------------------------------------------------------------------------------------------------------------------

		#region GetCourseMaterials/{courseCode} comment
		//[HttpGet("GetCourseMaterials/{courseCode}")]
		//[Authorize(Roles = "Doctor")]
		//public async Task<IActionResult> GetCourseMaterials(string courseCode)
		//{
		//	// 🔹 Get the UserId from the token
		//	var userId = User.FindFirstValue("AppicationUserId");

		//	// 🔹 Check if the user exists
		//	var user = await _userManager.FindByIdAsync(userId);
		//	if (user == null)
		//	{
		//		return Ok(new { success = false, message = "User not found." });
		//	}


		//	// 🔹 Check if the course exists
		//	var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseCode == courseCode);
		//	if (course == null)
		//	{
		//		return NotFound(new { success = false, message = "Course not found." });
		//	}

		//	// 🔹 Check if the doctor is enrolled in this course
		//	bool isDoctorEnrolled = await _context.CourseDoctors
		//		.AnyAsync(cd => cd.Doctor.UserId == userId && cd.CourseId == course.Id);

		//	if (!isDoctorEnrolled)
		//	{
		//		return StatusCode(403, new { success = false, message = "Doctor is not enrolled in this course." });
		//	}

		//	// 🔹 Retrieve all materials for the given course
		//	var courseMaterials = await _context.Materials
		//		.Where(m => m.CourseCode == courseCode && m.Doctor.UserId == userId)
		//		.Select(m => new
		//		{
		//			m.Id,
		//			m.FileName,
		//			m.FilePath,
		//			m.CourseCode,
		//			m.Description,
		//			m.UploadDate,
		//			m.TypeFile
		//		})
		//		.ToListAsync();

		//	// 🔹 Check if there are any materials for the course
		//	if (!courseMaterials.Any())
		//	{
		//		return Ok(new { success = true, message = "No materials found for this course.", materials = new List<object>() });
		//	}

		//	// 🔹 Return the materials in JSON format
		//	return Ok(new { success = true, materials = courseMaterials });
		//}
		#endregion

		#region GetCourseMaterials/{courseCode}LecOnlycomment
		//[HttpGet("GetCourseLectures/Doctor/{courseCode}")]
		//[Authorize(Roles = "Doctor")]
		//public async Task<IActionResult> GetCourseLectures(string courseCode)
		//{
		//	// 🔹 Get the UserId from the token
		//	var userId = User.FindFirstValue("AppicationUserId");

		//	// 🔹 Check if the user exists
		//	var user = await _userManager.FindByIdAsync(userId);
		//	if (user == null)
		//	{
		//		return Unauthorized(new { success = false, message = "User not found." });
		//	}

		//	// 🔹 Check if the course exists
		//	var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseCode == courseCode);
		//	if (course == null)
		//	{
		//		return NotFound(new { success = false, message = "Course not found." });
		//	}

		//	// 🔹 Check if the doctor is enrolled in this course
		//	bool isDoctorEnrolled = await _context.CourseDoctors
		//		.AnyAsync(cd => cd.Doctor.UserId == userId && cd.CourseId == course.Id);

		//	if (!isDoctorEnrolled)
		//	{
		//		return StatusCode(403, new { success = false, message = "Doctor is not enrolled in this course." });
		//	}

		//	// 🔹 Retrieve all lecture materials for the given course
		//	var lectureMaterials = await _context.Materials
		//		.Where(m => m.CourseCode == courseCode && m.Doctor.UserId == userId && m.TypeFile == "lec")
		//		.Select(m => new
		//		{
		//			m.Id,
		//			m.FileName,
		//			m.FilePath,
		//			m.CourseCode,
		//			m.Description,
		//			m.UploadDate,
		//			m.TypeFile
		//		})
		//		.ToListAsync();

		//	// 🔹 Check if there are any lectures for the course
		//	if (!lectureMaterials.Any())
		//	{
		//		return Ok(new { success = true, message = "No lecture materials found for this course.", materials = new List<object>() });
		//	}

		//	// 🔹 Return the lectures in JSON format
		//	return Ok(new { success = true, materials = lectureMaterials });
		//}
		#endregion


		#region GetCourseMaterials/{courseCode}LabOnly comment

		//[HttpGet("GetCourseLabs/{courseCode}")]
		//[Authorize(Roles = "Doctor")]
		//public async Task<IActionResult> GetCourseLabs(string courseCode)
		//{
		//	// 🔹 Get the UserId from the token
		//	var userId = User.FindFirstValue("AppicationUserId");

		//	// 🔹 Check if the user exists
		//	var user = await _userManager.FindByIdAsync(userId);
		//	if (user == null)
		//	{
		//		return Unauthorized(new { success = false, message = "User not found." });
		//	}

		//	// 🔹 Check if the course exists
		//	var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseCode == courseCode);
		//	if (course == null)
		//	{
		//		return NotFound(new { success = false, message = "Course not found." });
		//	}

		//	// 🔹 Check if the doctor is enrolled in this course
		//	bool isDoctorEnrolled = await _context.CourseDoctors
		//		.AnyAsync(cd => cd.Doctor.UserId == userId && cd.CourseId == course.Id);

		//	if (!isDoctorEnrolled)
		//	{
		//		return StatusCode(403, new { success = false, message = "Doctor is not enrolled in this course." });
		//	}

		//	// 🔹 Retrieve all lecture materials for the given course
		//	var lectureMaterials = await _context.Materials
		//		.Where(m => m.CourseCode == courseCode && m.Doctor.UserId == userId && m.TypeFile == "lab")
		//		.Select(m => new
		//		{
		//			m.Id,
		//			m.FileName,
		//			m.FilePath,
		//			m.CourseCode,
		//			m.Description,
		//			m.UploadDate,
		//			m.TypeFile
		//		})
		//		.ToListAsync();

		//	// 🔹 Check if there are any lectures for the course
		//	if (!lectureMaterials.Any())
		//	{
		//		return Ok(new { success = true, message = "No labs materials found for this course.", materials = new List<object>() });
		//	}

		//	// 🔹 Return the lectures in JSON format
		//	return Ok(new { success = true, materials = lectureMaterials });
		//}
		#endregion


		#region GetCourseMaterials/{courseCode}LabOnlycomment

		//[HttpGet("GetCourseExams/{courseCode}")]
		//[Authorize(Roles = "Doctor")]
		//public async Task<IActionResult> GetCourseExams(string courseCode)
		//{
		//	// 🔹 Get the UserId from the token
		//	var userId = User.FindFirstValue("AppicationUserId");

		//	// 🔹 Check if the user exists
		//	var user = await _userManager.FindByIdAsync(userId);
		//	if (user == null)
		//	{
		//		return Unauthorized(new { success = false, message = "User not found." });
		//	}

		//	// 🔹 Check if the course exists
		//	var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseCode == courseCode);
		//	if (course == null)
		//	{
		//		return NotFound(new { success = false, message = "Course not found." });
		//	}

		//	// 🔹 Check if the doctor is enrolled in this course
		//	bool isDoctorEnrolled = await _context.CourseDoctors
		//		.AnyAsync(cd => cd.Doctor.UserId == userId && cd.CourseId == course.Id);

		//	if (!isDoctorEnrolled)
		//	{
		//		return StatusCode(403, new { success = false, message = "Doctor is not enrolled in this course." });
		//	}

		//	// 🔹 Retrieve all lecture materials for the given course
		//	var lectureMaterials = await _context.Materials
		//		.Where(m => m.CourseCode == courseCode && m.Doctor.UserId == userId && m.TypeFile == "exam")
		//		.Select(m => new
		//		{
		//			m.Id,
		//			m.FileName,
		//			m.FilePath,
		//			m.CourseCode,
		//			m.Description,
		//			m.UploadDate,
		//			m.TypeFile
		//		})
		//		.ToListAsync();

		//	// 🔹 Check if there are any lectures for the course
		//	if (!lectureMaterials.Any())
		//	{
		//		return Ok(new { success = true, message = "No exams materials found for this course.", materials = new List<object>() });
		//	}

		//	// 🔹 Return the lectures in JSON format
		//	return Ok(new { success = true, materials = lectureMaterials });
		//}
		#endregion

		
		#region GetCourseMaterials/{courseCode}
		[HttpGet("GetMaterialPassCourseTypeIdOfDoctor/{courseCode}")]
		// 
		public async Task<IActionResult> GetCourseMaterials(string courseCode, [FromQuery] string doctorId, [FromQuery] string typeFile)
		{
			// 🔹 Check if the doctor exists
			var doctor = await _userManager.FindByIdAsync(doctorId);
			if (doctor == null)
			{
				return Ok(new { success = false, message = "Doctor not found." });
			}

			// 🔹 Check if the course exists
			var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseCode == courseCode);
			if (course == null)
			{
				return NotFound(new { success = false, message = "Course not found." });
			}

			// 🔹 Check if the doctor is enrolled in this course
			bool isDoctorEnrolled = await _context.CourseDoctors
				.AnyAsync(cd => cd.Doctor.UserId == doctorId && cd.CourseId == course.Id);

			if (!isDoctorEnrolled)
			{
				return StatusCode(403, new { success = false, message = "Doctor is not enrolled in this course." });
			}

			// 🔹 Retrieve materials based on courseCode, doctorId, and typeFile
			var courseMaterials = await _context.Materials
				.Where(m => m.CourseCode == courseCode
							&& m.Doctor.UserId == doctorId
							&& m.TypeFile.ToLower() == typeFile.ToLower())  // Filter by typeFile
				.Select(m => new
				{
					m.Id,
					m.FileName,
					m.FilePath,
					m.CourseCode,
					m.Description,
					m.UploadDate,
					m.TypeFile
				})
				.ToListAsync();

			// 🔹 Check if there are any materials for the course
			if (!courseMaterials.Any())
			{
				return Ok(new { success = true, message = "No materials found for this course and type.", materials = new List<object>() });
			}

			// 🔹 Return the materials in JSON format
			return Ok(new { success = true, materials = courseMaterials });
		}
		#endregion


		#region Update Files
		[HttpPut("updateFile")]
		[Authorize(Roles = "Doctor")]
		public async Task<IActionResult> UpdateMaterial([FromForm] UpdateMaterialDto updateMaterialDto)
		{
			// Validate input data
			if (!ModelState.IsValid)
			{
				return BadRequest(new { success = false, message = "Invalid data provided." });
			}

			// Check if the uploaded file exists
			if (updateMaterialDto.File == null || updateMaterialDto.File.Length == 0)
			{
				return BadRequest(new { success = false, message = "No file uploaded." });
			}

			// Validate file size (Max: 10MB)
			var maxFileSize = 10 * 1024 * 1024; // 10MB
			if (updateMaterialDto.File.Length > maxFileSize)
			{
				return BadRequest(new { success = false, message = "File size exceeds the maximum limit (10 MB)." });
			}

			// Validate file type
			var fileExtension = Path.GetExtension(updateMaterialDto.File.FileName).ToLower();
			var allowedContentTypes = new Dictionary<string, string>
	        {
		       { ".pdf", "application/pdf" },
		       { ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
		       { ".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation" }
	        };

			if (!allowedContentTypes.ContainsKey(fileExtension) ||
				allowedContentTypes[fileExtension] != updateMaterialDto.File.ContentType.ToLower())
			{
				return BadRequest(new { success = false, message = "Only PDF, Word, and PowerPoint files are allowed." });
			}

			// Get UserId from the token
			var userId = User.FindFirstValue("AppicationUserId");

			// Check if the user exists
			var user = await _userManager.FindByIdAsync(userId);
			if (user == null)
			{
				return Unauthorized(new { success = false, message = "User not found." });
			}

			// Check if the material exists
			var material = await _context.Materials.FindAsync(updateMaterialDto.MaterialId);
			if (material == null)
			{
				return NotFound(new { success = false, message = "Material not found." });
			}

			// Check if the course exists
			var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseCode == material.CourseCode);
			if (course == null)
			{
				return NotFound(new { success = false, message = "Course not found." });
			}

			// Check if the doctor is enrolled in this course
			bool isDoctorEnrolled = await _context.CourseDoctors
				.AnyAsync(cd => cd.Doctor.UserId == userId && cd.CourseId == course.Id);

			if (!isDoctorEnrolled)
			{
				return StatusCode(403, new { success = false, message = "Doctor is not enrolled in this course." });
			}

			// Prepare file storage path
			var uploadsDirectory = Path.Combine(_hostingEnvironment.WebRootPath, "Uploads", course.CourseCode);
			if (!Directory.Exists(uploadsDirectory))
			{
				Directory.CreateDirectory(uploadsDirectory);
			}

			// Generate a unique file name based on an incremented serial number
			var fileBaseName = Path.GetFileNameWithoutExtension(updateMaterialDto.File.FileName);
			var existingFiles = Directory.GetFiles(uploadsDirectory, $"{fileBaseName}*{fileExtension}");

			int maxNumber = 0;
			foreach (var existingFile in existingFiles)
			{
				var existingFileName = Path.GetFileNameWithoutExtension(existingFile);
				var match = Regex.Match(existingFileName, @$"{Regex.Escape(fileBaseName)}_(\d+)$");
				if (match.Success && int.TryParse(match.Groups[1].Value, out int num))
				{
					maxNumber = Math.Max(maxNumber, num);
				}
			}

			var uniqueFileName = maxNumber > 0
				? $"{fileBaseName}_{maxNumber + 1}{fileExtension}"
				: $"{fileBaseName}_1{fileExtension}";

			var newFilePath = Path.Combine(uploadsDirectory, uniqueFileName);

			// Save the new file
			using (var stream = new FileStream(newFilePath, FileMode.Create))
			{
				await updateMaterialDto.File.CopyToAsync(stream);
			}

			// Delete the old file if it exists
			var oldFilePath = Path.Combine(_hostingEnvironment.WebRootPath, material.FilePath.TrimStart('/'));
			if (System.IO.File.Exists(oldFilePath))
			{
				System.IO.File.Delete(oldFilePath);
			}
			// Calculate and update file size
			// Calculate and update file size as a string
			long fileSizeBytes = new FileInfo(newFilePath).Length;
	    	// Update material data in the database
			material.FileName = uniqueFileName;
			material.FilePath = $"/Uploads/{course.CourseCode}/{uniqueFileName}";
			material.Description = updateMaterialDto.Description;
			material.UploadDate = DateTime.Now;
			material.TypeFile = updateMaterialDto.Type;
			material.Size = (fileSizeBytes / (1024.0 * 1024.0)).ToString("F2") + " MB"; // Convert to MB as string
			_context.Materials.Update(material);
			await _context.SaveChangesAsync();

			return Ok(new
			{
				success = true,
				message = "File updated successfully.",
				filePath = material.FilePath
			});
		}
	

		#endregion


//--------------------------------------------------------------------------------------------------------------------------------------------------


		#region DeleteAllFilesOfCertainDoctor

		[HttpDelete("deleteAllMaterialsOfDoctor/Doctor")]
		[Authorize(Roles = "Doctor")]
		public async Task<IActionResult> DeleteAllMaterials()
		{
			var userId = User.FindFirstValue("AppicationUserId"); 

			var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
			if (doctor == null)
			{
				return NotFound(new { success = false, message = "Doctor not found." });
			}

			var materials = await _context.Materials.Where(m => m.DoctorId == doctor.DoctorId).ToListAsync();

			if (!materials.Any())
			{
				return NotFound(new { success = false, message = "No materials found for this doctor." });
			}
			foreach (var material in materials)
			{
				var filePath = Path.Combine(_hostingEnvironment.WebRootPath, material.FilePath.TrimStart('/'));
				if (System.IO.File.Exists(filePath))
				{
					System.IO.File.Delete(filePath);
				}
			}
            _context.Materials.RemoveRange(materials);
			await _context.SaveChangesAsync();

			// .net 7
			//await _context.Materials
			//      .Where(m => m.DoctorId == doctor.DoctorId)
			//.ExecuteDeleteAsync();


			return Ok(new { success = true, message = "All materials deleted successfully." });
		}
		#endregion

		#region DeleteSomeMaterialOfSpeceficDoctor
		[HttpDelete("deleteMultipleMaterialOfSpeceficDoctor")]
		[Authorize(Roles = "Doctor")]
		public async Task<IActionResult> DeleteMultipleMaterials([FromQuery] List<int> materialIds)
		{
			var userId = User.FindFirstValue("AppicationUserId");

			var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
			if (doctor == null)
			{
				return NotFound(new { success = false, message = "Doctor not found." });
			}

			var materials = await _context.Materials
				.Where(m => m.DoctorId == doctor.DoctorId && materialIds.Contains(m.Id))
				.ToListAsync();

			if (!materials.Any())
			{
				return NotFound(new { success = false, message = "Some materials were not found or do not belong to the current doctor." });
			}

			// delete files from Server
			foreach (var material in materials)
			{
				var filePath = Path.Combine(_hostingEnvironment.WebRootPath, material.FilePath.TrimStart('/'));
				if (System.IO.File.Exists(filePath))
				{
					System.IO.File.Delete(filePath);
				}
			}

			_context.Materials.RemoveRange(materials);
			await _context.SaveChangesAsync();

			return Ok(new { success = true, message = "Selected materials deleted successfully." });
		}
		#endregion

		#region DeleteOnlyMaterialIdOfDoctor
		[HttpDelete("delete/{materialId}")]
		[Authorize(Roles = "Doctor")]
		public async Task<IActionResult> DeleteMaterial(int materialId)
		{
			var userId = User.FindFirstValue("AppicationUserId");  

			var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
			if (doctor == null)
			{
				return NotFound(new { success = false, message = "Doctor not found." });
			}

			var material = await _context.Materials.FirstOrDefaultAsync(m => m.Id == materialId && m.DoctorId == doctor.DoctorId);

			if (material == null)
			{
				return NotFound(new { success = false, message = "Material not found." });
			}

			// حذف الملف من السيرفر
			var filePath = Path.Combine(_hostingEnvironment.WebRootPath, material.FilePath.TrimStart('/'));
			if (System.IO.File.Exists(filePath))
			{
				System.IO.File.Delete(filePath);
			}

			_context.Materials.Remove(material);
			await _context.SaveChangesAsync();

			return Ok(new { success = true, message = "Material deleted successfully." });
		}

		#endregion

		#region DeleteAllLecMaterialsOfDoctorFromToken

		[HttpDelete("deleteAllLec/Doctor")]
		[Authorize(Roles = "Doctor")]
		public async Task<IActionResult> DeleteAllLecMaterialsOfDoctor()
		{
			// Get the UserId from the token
			var userId = User.FindFirstValue("AppicationUserId");

			// Search for the doctor using the UserId
			var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);

			if (doctor == null)
			{
				return NotFound(new { success = false, message = "Doctor not found." });
			}

			// Get all "Lec" type materials related to the doctor
			var lecMaterials = await _context.Materials
				.Where(m => m.DoctorId == doctor.DoctorId && m.TypeFile == "Material") // Specify "Lec" material type
				.ToListAsync();

			if (!lecMaterials.Any())
			{
				return NotFound(new { success = false, message = "No Lec materials found for this doctor." });
			}

			// Delete the files from the server
			foreach (var material in lecMaterials)
			{
				var filePath = Path.Combine(_hostingEnvironment.WebRootPath, material.FilePath.TrimStart('/'));
				if (System.IO.File.Exists(filePath))
				{
					System.IO.File.Delete(filePath);
				}
			}

			// Remove materials from the database
			_context.Materials.RemoveRange(lecMaterials);
			await _context.SaveChangesAsync();

			return Ok(new { success = true, message = "All Lec materials deleted successfully." });
		}

		#endregion

        #region DeleteAllLabMaterialsOfDoctorFromToken

		[HttpDelete("deleteAllLab/Doctor")]
		[Authorize(Roles = "Doctor")]
		public async Task<IActionResult> DeleteAllLabMaterialsOfDoctor()
		{
			// Get the UserId from the token
			var userId = User.FindFirstValue("AppicationUserId");

			// Search for the doctor using the UserId
			var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);

			if (doctor == null)
			{
				return NotFound(new { success = false, message = "Doctor not found." });
			}

			// Get all "lab" type materials related to the doctor
			var labMaterials = await _context.Materials
				.Where(m => m.DoctorId == doctor.DoctorId && m.TypeFile == "Labs") // Specify "lab" material type
				.ToListAsync();

			if (!labMaterials.Any())
			{
				return NotFound(new { success = false, message = "No lab materials found for this doctor." });
			}

			// Delete the files from the server
			foreach (var material in labMaterials)
			{
				var filePath = Path.Combine(_hostingEnvironment.WebRootPath, material.FilePath.TrimStart('/'));
				if (System.IO.File.Exists(filePath))
				{
					System.IO.File.Delete(filePath);
				}
			}

			// Remove materials from the database
			_context.Materials.RemoveRange(labMaterials);
			await _context.SaveChangesAsync();

			return Ok(new { success = true, message = "All lab materials deleted successfully." });
		}

		#endregion

        #region DeleteAllExamMaterialsOfDoctorFromToken

		[HttpDelete("deleteAllExam/Doctor")]
		[Authorize(Roles = "Doctor")]
		public async Task<IActionResult> DeleteAllExamMaterialsOfDoctor()
		{
			// Get the UserId from the token
			var userId = User.FindFirstValue("AppicationUserId");

			// Search for the doctor using the UserId
			var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);

			if (doctor == null)
			{
				return NotFound(new { success = false, message = "Doctor not found." });
			}

			// Get all "exam" type materials related to the doctor
			var examMaterials = await _context.Materials
				.Where(m => m.DoctorId == doctor.DoctorId && m.TypeFile == "Exams") // Specify "exam" material type
				.ToListAsync();

			if (!examMaterials.Any())
			{
				return NotFound(new { success = false, message = "No Exam materials found for this doctor." });
			}

			// Delete the files from the server
			foreach (var material in examMaterials)
			{
				var filePath = Path.Combine(_hostingEnvironment.WebRootPath, material.FilePath.TrimStart('/'));
				if (System.IO.File.Exists(filePath))
				{
					System.IO.File.Delete(filePath);
				}
			}

			// Remove materials from the database
			_context.Materials.RemoveRange(examMaterials);
			await _context.SaveChangesAsync();

			return Ok(new { success = true, message = "All exam materials deleted successfully." });
		}

		#endregion

        #region DeleteAllMaterialsOfTypeForDoctor

		[HttpDelete("deleteMaterialsByType/Doctor")]
		[Authorize(Roles = "Doctor")]
		public async Task<IActionResult> DeleteAllMaterialsOfTypeForDoctor([FromQuery] string typeFile)
		{
			if (string.IsNullOrEmpty(typeFile))
			{
				return BadRequest(new { success = false, message = "TypeFile is required." });
			}

			// Get the UserId from the token
			var userId = User.FindFirstValue("AppicationUserId");

			// Search for the doctor using the UserId
			var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);

			if (doctor == null)
			{
				return NotFound(new { success = false, message = "Doctor not found." });
			}

			// Get all materials of the specified type related to the doctor
			var materials = await _context.Materials
				.Where(m => m.DoctorId == doctor.DoctorId && m.TypeFile == typeFile)
				.ToListAsync();

			if (!materials.Any())
			{
				return NotFound(new { success = false, message = $"No materials of type '{typeFile}' found for this doctor." });
			}

			// Delete the files from the server
			foreach (var material in materials)
			{
				var filePath = Path.Combine(_hostingEnvironment.WebRootPath, material.FilePath.TrimStart('/'));
				if (System.IO.File.Exists(filePath))
				{
					System.IO.File.Delete(filePath);
				}
			}

			// Remove materials from the database
			_context.Materials.RemoveRange(materials);
			await _context.SaveChangesAsync();

			return Ok(new { success = true, message = $"All {typeFile} materials deleted successfully." });
		}

		#endregion
 
//--------------------------------------------------------------------------------------------------------------------------------------------------	

    	#region GetAllMaterialsForCourseComment

		//[HttpGet("getAllMaterials/{courseCode}")]
		//[AllowAnonymous]
		//public async Task<IActionResult> GetAllMaterialsForCourse(string courseCode)
		//{
			
		//	var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseCode == courseCode);
		//	if (course == null)
		//	{
		//		return NotFound(new { success = false, message = "Course not found." });
		//	}

		//	var materials = await _context.Materials
		//		.Where(m => m.CourseCode == courseCode)
		//		.ToListAsync();

		//	if (!materials.Any())
		//	{
		//		return NotFound(new { success = false, message = "No materials found for this course." });
		//	}

		//	return Ok(new
		//	{
		//		success = true,
		//		message = "Materials retrieved successfully for the course.",
		//		materials = materials
		//	});
		//}

		#endregion

	 	#region GetLecMaterialsForCourseComment

		//[HttpGet("getLecMaterials/{courseCode}")]
		//[AllowAnonymous]
		//public async Task<IActionResult> GetLecMaterialsForCourse(string courseCode)
		//{
			
		//	var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseCode == courseCode);
		//	if (course == null)
		//	{
		//		return NotFound(new { success = false, message = "Course not found." });
		//	}

			
		//	var lecMaterials = await _context.Materials
		//		.Where(m => m.CourseCode == courseCode && m.TypeFile == "lec")
		//		.ToListAsync();

		//	if (!lecMaterials.Any())
		//	{
		//		return NotFound(new { success = false, message = "No Lec materials found for this course." });
		//	}

		//	return Ok(new
		//	{
		//		success = true,
		//		message = "Lec materials retrieved successfully for the course.",
		//		materials = lecMaterials
		//	});
		//}

		#endregion

		#region GetLabMaterialsForCourseComment

		//[HttpGet("getLabMaterials/{courseCode}")]
		//[AllowAnonymous]
		//public async Task<IActionResult> GetLabMaterialsForCourse(string courseCode)
		//{
			
		//	var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseCode == courseCode);
		//	if (course == null)
		//	{
		//		return NotFound(new { success = false, message = "Course not found." });
		//	}

		//	var labMaterials = await _context.Materials
		//		.Where(m => m.CourseCode == courseCode && m.TypeFile == "lab")
		//		.ToListAsync();

		//	if (!labMaterials.Any())
		//	{
		//		return NotFound(new { success = false, message = "No lab materials found for this course." });
		//	}

		//	return Ok(new
		//	{
		//		success = true,
		//		message = "lab materials retrieved successfully for the course.",
		//		materials = labMaterials
		//	});
		//}

		#endregion

        #region GetExamMaterialsForCourseComment

		//[HttpGet("getExamMaterials/{courseCode}")]
		//[AllowAnonymous]
		//public async Task<IActionResult> GetExamMaterialsForCourse(string courseCode)
		//{
			
		//	var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseCode == courseCode);
		//	if (course == null)
		//	{
		//		return NotFound(new { success = false, message = "Course not found." });
		//	}

			
		//	var examMaterials = await _context.Materials
		//		.Where(m => m.CourseCode == courseCode && m.TypeFile == "exam")
		//		.ToListAsync();

		//	if (!examMaterials.Any())
		//	{
		//		return NotFound(new { success = false, message = "No exam materials found for this course." });
		//	}

		//	return Ok(new
		//	{
		//		success = true,
		//		message = "exam materials retrieved successfully for the course.",
		//		materials = examMaterials
		//	});
		//}

		#endregion

		// For Student

		#region GetDoctorMaterialsForCourse

		[HttpGet("getDoctorMaterials/{courseCode}/{userId}")]
		[AllowAnonymous]
		public async Task<IActionResult> GetDoctorMaterialsForCourse(string courseCode, string userId)
		{
			// Validate input parameters
			if (string.IsNullOrEmpty(courseCode) || string.IsNullOrEmpty(userId))
			{
				return BadRequest(new { success = false, message = "CourseCode and UserId are required." });
			}

			// Get DoctorId from UserId
			var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
			if (doctor == null)
			{
				return NotFound(new { success = false, message = "Doctor not found for the provided UserId." });
			}

			// Check if the course exists
			var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseCode == courseCode);
			if (course == null)
			{
				return NotFound(new { success = false, message = "Course not found." });
			}

			// Retrieve materials uploaded by this doctor for the given course
			var materials = await _context.Materials
				.Where(m => m.CourseCode == courseCode && m.DoctorId == doctor.DoctorId)
				.ToListAsync();

			if (!materials.Any())
			{
				return NotFound(new { success = false, message = "No materials found for this doctor in the specified course." });
			}
			
			return Ok(new
			{
				success = true,
				message = "Materials retrieved successfully.",
				materials = materials
			});
		}

		#endregion
		
		#region GetDoctorMaterialsForCourseBasedOnType

		[HttpGet("getDoctorMaterials/{courseCode}/{userId}/{typeFile}")]
		[AllowAnonymous]
		public async Task<IActionResult> GetDoctorMaterialsForCourse(string courseCode, string userId , string typeFile)
		{
			// Validate input parameters
			if (string.IsNullOrEmpty(courseCode) || string.IsNullOrEmpty(userId))
			{
				return BadRequest(new { success = false, message = "CourseCode and UserId are required." });
			}

			// Get DoctorId from UserId
			var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
			if (doctor == null)
			{
				return NotFound(new { success = false, message = "Doctor not found for the provided UserId." });
			}

			// Check if the course exists
			var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseCode == courseCode);
			if (course == null)
			{
				return NotFound(new { success = false, message = "Course not found." });
			}

			// Retrieve materials uploaded by this doctor for the given course
			var materials = await _context.Materials
				.Where(m => m.CourseCode == courseCode && m.DoctorId == doctor.DoctorId && m.TypeFile==typeFile)
				.ToListAsync();

			if (!materials.Any())
			{
				return NotFound(new { success = false, message = "No materials found for this doctor in the specified course." });
			}

			return Ok(new
			{
				success = true,
				message = "Materials retrieved successfully.",
				materials = materials
			});
		}

		#endregion

	}
}


