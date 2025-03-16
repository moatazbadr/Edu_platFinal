using JWT.DATA;
using JWT;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Edu_plat.DTO.UploadVideos;
using Edu_plat.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using Edu_plat.DTO.UploadFiles;

namespace Edu_plat.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class Videos : ControllerBase
	{

		private readonly ApplicationDbContext _context;
		private readonly IWebHostEnvironment _hostingEnvironment;
		private readonly UserManager<ApplicationUser> _userManager;

		public Videos(ApplicationDbContext context, IWebHostEnvironment hostingEnvironment, UserManager<ApplicationUser> userManager = null)
		{
			_context = context;
			_hostingEnvironment = hostingEnvironment;
			_userManager = userManager;
		}

		#region UploadVideo 
		//[HttpPost("UploadVideo")]
		//[Authorize(Roles = "Doctor")]
		//public async Task<IActionResult> UploadVideo([FromForm] UploadVideoDto uploadVideoDto)
		//{
		//	// Validate the request data
		//	if (!ModelState.IsValid)
		//	{
		//		return BadRequest(new { success = false, message = "Invalid data provided." });
		//	}

		//	// Check if a video file is provided
		//	if (uploadVideoDto.Video == null || uploadVideoDto.Video.Length == 0)
		//	{
		//		return BadRequest(new { success = false, message = "No video uploaded." });
		//	}

		//	// Set maximum allowed video file size (e.g., 100MB)
		//	var maxFileSize = 100 * 1024 * 1024; // 100MB
		//	if (uploadVideoDto.Video.Length > maxFileSize)
		//	{
		//		return BadRequest(new { success = false, message = "Video size exceeds the maximum limit (100MB)." });
		//	}

		//	if (string.IsNullOrEmpty(uploadVideoDto.))

		//	//// Allowed video file extensions
		//	//var allowedExtensions = new[] { ".mp4", ".avi", ".mov", ".mkv" };
		//	//var fileExtension = Path.GetExtension(uploadVideoDto.Video.FileName).ToLower();
		//	//if (!allowedExtensions.Contains(fileExtension))
		//	//{
		//	//	return BadRequest(new { success = false, message = "Only MP4, AVI, MOV, and MKV videos are allowed." });
		//	//}
		//	// Allowed video file extensions and MIME types
		//	var allowedMimeTypes = new Dictionary<string, string>
		//	{
		//			{ ".mp4", "video/mp4" },
		//			{ ".avi", "video/x-msvideo" },
		//			{ ".mov", "video/quicktime" },
		//			{ ".mkv", "video/x-matroska" }
		//	};

		//	var fileExtension = Path.GetExtension(uploadVideoDto.Video.FileName).ToLower();
		//	var contentType = uploadVideoDto.Video.ContentType.ToLower();

		//	// Check if the file extension is allowed
		//	if (!allowedMimeTypes.ContainsKey(fileExtension))
		//	{
		//		return BadRequest(new { success = false, message = "Only MP4, AVI, MOV, and MKV videos are allowed." });
		//	}

		//	// Check if the content type matches the expected MIME type
		//	if (allowedMimeTypes[fileExtension] != contentType)
		//	{
		//		return BadRequest(new { success = false, message = "Invalid video file format." });
		//	}


		//	// Check if the course exists
		//	var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseCode == uploadVideoDto.CourseCode);
		//	if (course == null)
		//	{
		//		return NotFound(new { success = false, message = "Course not found." });
		//	}

		//	// Get the doctor details from the token	
		//	var userId = User.FindFirstValue("ApplicationUserId");
		//	var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
		//	if (doctor == null)
		//	{
		//		return NotFound(new { success = false, message = "Doctor not found." });
		//	}

		//	// Check if the doctor is enrolled in the course 
		//	bool isDoctorEnrolled = await _context.CourseDoctors.AnyAsync(cd => cd.DoctorId == doctor.DoctorId && cd.CourseId == course.Id);
		//	if (!isDoctorEnrolled)
		//	{
		//		return StatusCode(403, new { success = false, message = "Doctor is not enrolled in this course." });
		//	}

		//	// Define the directory path for storing videos
		//	var videoDirectory = Path.Combine(_hostingEnvironment.WebRootPath, "Videos", course.CourseCode);
		//	if (!Directory.Exists(videoDirectory))
		//	{
		//		Directory.CreateDirectory(videoDirectory);
		//	}

		//	// Generate the video file name
		//	//var fileName = string.IsNullOrWhiteSpace(uploadVideoDto.FileName)
		//	//	? Path.GetFileName(uploadVideoDto.Video.FileName)
		//	//	: uploadVideoDto.FileName + Path.GetExtension(uploadVideoDto.Video.FileName); ;


		//	var fileName = Path.GetFileName(uploadVideoDto.Video.FileName);


		//	var filePath = Path.Combine(videoDirectory, fileName);

		//	// Save the video file to the server
		//	using (var stream = new FileStream(filePath, FileMode.Create))
		//	{
		//		await uploadVideoDto.Video.CopyToAsync(stream);
		//	}

		//	//Initialize file size variable
		//	string fileSize = "Unknown";

		//	// Check if file exists before calculating its size
		//	if (System.IO.File.Exists(filePath))
		//	{
		//		long fileSizeBytes = new FileInfo(filePath).Length;
		//		fileSize = (fileSizeBytes / (1024.0 * 1024.0)).ToString("F2") + " MB"; // Convert to MB
		//	}

		//	// Save video details in the database
		//	var videoMaterial = new Material
		//	{
		//		CourseId = course.Id, // Course ID
		//		CourseCode = course.CourseCode, // Course Code
		//		DoctorId = doctor.DoctorId, // Doctor ID
		//		FilePath = $"/Videos/{course.CourseCode}/{fileName}", // File path in the server
		//		FileName = fileName, // File name
		//		//Description = uploadVideoDto.Description, // Video description
		//		UploadDate = DateTime.Now, // Upload timestamp
		//		TypeFile = "Video",
		//		Size = fileSize

		//	};

		//	_context.Materials.Add(videoMaterial);
		//	await _context.SaveChangesAsync();

		//	return Ok(new { success = true, message = "Video uploaded successfully.", filePath = videoMaterial.FilePath, videoMaterial.Id });
		//}
		#endregion

		#region UpdateVideo
		[HttpPost("UpdateVideo")]
		[Authorize(Roles = "Doctor")]
		public async Task<IActionResult> UpdateVideo([FromForm] UpdateVideoDto updateVideoDto)
		{
			// Validate the input data
			if (!ModelState.IsValid)
			{
				return BadRequest(new { success = false, message = "Invalid data provided." });
			}

			// Ensure that the video is provided
			if (updateVideoDto.Video == null || updateVideoDto.Video.Length == 0)
			{
				return BadRequest(new { success = false, message = "No video uploaded." });
			}

			// Check file size limit (100MB)
			var maxFileSize = 100 * 1024 * 1024;
			if (updateVideoDto.Video.Length > maxFileSize)
			{
				return BadRequest(new { success = false, message = "Video size exceeds the maximum limit (100MB)." });
			}

			// Allowed video types
	   		var allowedMimeTypes = new Dictionary<string, string>
	        {
		       { ".mp4", "video/mp4" },
		       { ".avi", "video/x-msvideo" },
		       { ".mov", "video/quicktime" },
		       { ".mkv", "video/x-matroska" }
	        };

			var fileExtension = Path.GetExtension(updateVideoDto.Video.FileName).ToLower();
			var contentType = updateVideoDto.Video.ContentType.ToLower();

			if (!allowedMimeTypes.ContainsKey(fileExtension) || allowedMimeTypes[fileExtension] != contentType)
			{
				return BadRequest(new { success = false, message = "Invalid video file format." });
			}

			// Retrieve video record
			var videoMaterial = await _context.Materials.FindAsync(updateVideoDto.VideoId);
			if (videoMaterial == null)
			{
				return NotFound(new { success = false, message = "Video not found." });
			}

			// Get user ID from token
			var userId = User.FindFirstValue("ApplicationUserId");
			var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
			if (doctor == null)
			{
				return NotFound(new { success = false, message = "Doctor not found." });
			}

			// Ensure the doctor is the one who uploaded the video
			if (videoMaterial.DoctorId != doctor.DoctorId)
			{
				return StatusCode(403, new { success = false, message = "You are not authorized to update this video." });
			}

			// Get course
			var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseCode == videoMaterial.CourseCode);
			if (course == null)
			{
				return NotFound(new { success = false, message = "Course not found." });
			}

			// Check if the doctor is enrolled in the course 
			bool isDoctorEnrolled = await _context.CourseDoctors.AnyAsync(cd => cd.DoctorId == doctor.DoctorId && cd.CourseId == course.Id);
			if (!isDoctorEnrolled)
			{
				return StatusCode(403, new { success = false, message = "Doctor is not enrolled in this course." });
			}



			// Define the video directory
			var videoDirectory = Path.Combine(_hostingEnvironment.WebRootPath, "Videos", course.CourseCode);
			if (!Directory.Exists(videoDirectory))
			{
				Directory.CreateDirectory(videoDirectory);
			}

			// Generate file name
			//var fileName = string.IsNullOrWhiteSpace(updateVideoDto.FileName)
			//	? Path.GetFileName(updateVideoDto.Video.FileName)
			//	: updateVideoDto.FileName + Path.GetExtension(updateVideoDto.Video.FileName);
		
			var fileName = Path.GetFileName(updateVideoDto.Video.FileName);


			var filePath = Path.Combine(videoDirectory, fileName);

			// Delete old file
			if (System.IO.File.Exists(filePath))
			{
				System.IO.File.Delete(filePath);
			}

			// Save new file
			using (var stream = new FileStream(filePath, FileMode.Create))
			{
				await updateVideoDto.Video.CopyToAsync(stream);
			}

			// Update database
			videoMaterial.FilePath = $"/Videos/{course.CourseCode}/{fileName}";
			videoMaterial.FileName = fileName;
			videoMaterial.Description = updateVideoDto.Description;
			videoMaterial.UploadDate = DateTime.Now;

			await _context.SaveChangesAsync();

			return Ok(new { success = true, message = "Video updated successfully.", filePath = videoMaterial.FilePath });
		}
		#endregion

		#region DownloadByPath
		[HttpGet("DownloadByPath")]
		public IActionResult DownloadByPath([FromQuery] string pathVideo)
		{
			if (string.IsNullOrWhiteSpace(pathVideo))
			{
				return BadRequest(new { success = false, message = "Path video is required." });
			}

			var filePath = Path.Combine(_hostingEnvironment.WebRootPath, pathVideo.TrimStart('/'));

			if (!System.IO.File.Exists(filePath))
			{
				return NotFound(new { success = false, message = "Video not found." });
			}

			var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
			var fileName = Path.GetFileName(filePath);
			return File(fileStream, "application/octet-stream", fileName);
		}


		#endregion

		#region DownloadById&CourseCode

		[HttpGet("DownloadById")]
		public async Task<IActionResult> DownloadById([FromQuery] int videoId, [FromQuery] string courseCode)
		{
			if (videoId <= 0 || string.IsNullOrWhiteSpace(courseCode))
			{
				return BadRequest(new { success = false, message = "VideoId and CourseCode are required." });
			}

			var videoMaterial = await _context.Materials
				.FirstOrDefaultAsync(m => m.Id == videoId && m.CourseCode == courseCode);

			if (videoMaterial == null)
			{
				return NotFound(new { success = false, message = "Video not found." });
			}

			var filePath = Path.Combine(_hostingEnvironment.WebRootPath, videoMaterial.FilePath.TrimStart('/'));

			if (!System.IO.File.Exists(filePath))
			{
				return NotFound(new { success = false, message = "Video file not found on server." });
			}

			var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
			var fileName = Path.GetFileName(filePath);
			return File(fileStream, "application/octet-stream", fileName);
		}

		#endregion

		#region Delete
		[HttpDelete("DeleteAllDoctorVideos")]
		[Authorize(Roles = "Doctor")]
		public async Task<IActionResult> DeleteAllDoctorVideos()
		{
			var userId = User.FindFirstValue("ApplicationUserId");
			var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
			if (doctor == null)
			{
				return NotFound(new { success = false, message = "Doctor not found." });
			}

			var videos = await _context.Materials
				.Where(m => m.DoctorId == doctor.DoctorId && m.TypeFile == "Video")
				.ToListAsync();

			if (!videos.Any())
			{
				return NotFound(new { success = false, message = "No videos found." });
			}

			foreach (var video in videos)
			{
				if (System.IO.File.Exists(video.FilePath))
				{
					System.IO.File.Delete(video.FilePath);
				}
				_context.Materials.Remove(video);
			}
			await _context.SaveChangesAsync();

			return Ok(new { success = true, message = "All videos deleted successfully." });
		}

		[HttpDelete("DeleteAllVideosInCourse/{courseCode}")]
		[Authorize(Roles = "Doctor")]
		public async Task<IActionResult> DeleteAllVideosInCourse(string courseCode)
		{
			if (string.IsNullOrWhiteSpace(courseCode))
			{
				return BadRequest(new { success = false, message = "CourseCode is required." });
			}
			var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseCode == courseCode);
			if (course == null)
			{
				return NotFound(new { success = false, message = "Course not found." });
			}

			var videos = await _context.Materials
				.Where(m => m.CourseId == course.Id && m.TypeFile == "Video")
				.ToListAsync();

			if (!videos.Any())
			{
				return NotFound(new { success = false, message = "No videos found in this course." });
			}

			foreach (var video in videos)
			{
				if (System.IO.File.Exists(video.FilePath))
				{
					System.IO.File.Delete(video.FilePath);
				}
				_context.Materials.Remove(video);
			}
			await _context.SaveChangesAsync();

			return Ok(new { success = true, message = "All videos in course deleted successfully." });
		}

		[HttpDelete("DeleteVideoById/{videoId}")]
		[Authorize(Roles = "Doctor")]
		public async Task<IActionResult> DeleteVideoById(int videoId)
		{
			if (videoId <= 0)
			{
				return BadRequest(new { success = false, message = "VideoId is required." });
			}
			var userId = User.FindFirstValue("ApplicationUserId");
			var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
			if (doctor == null)
			{
				return NotFound(new { success = false, message = "Doctor not found." });
			}

			var video = await _context.Materials.FirstOrDefaultAsync(m => m.Id == videoId && m.TypeFile == "Video");
			if (video == null)
			{
				return NotFound(new { success = false, message = "Video not found." });
			}

			if (video.DoctorId != doctor.DoctorId)
			{
				return StatusCode(403, new { success = false, message = "You can only delete your own videos." });
			}

			if (System.IO.File.Exists(video.FilePath))
			{
				System.IO.File.Delete(video.FilePath);
			}

			_context.Materials.Remove(video);
			await _context.SaveChangesAsync();

			return Ok(new { success = true, message = "Video deleted successfully." });
		}

		[HttpDelete("DeleteMultipleVideos")]
		[Authorize(Roles = "Doctor")]
		public async Task<IActionResult> DeleteMultipleVideos([FromBody] List<int> videoIds)
		{
			if (videoIds == null || !videoIds.Any())
			{
				return BadRequest(new { success = false, message = "At least one VideoId is required." });
			}
			var userId = User.FindFirstValue("ApplicationUserId");
			var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
			if (doctor == null)
			{
				return NotFound(new { success = false, message = "Doctor not found." });
			}

			var videos = await _context.Materials
				.Where(m => videoIds.Contains(m.Id) && m.TypeFile == "Video")
				.ToListAsync();

			if (!videos.Any())
			{
				return NotFound(new { success = false, message = "No videos found." });
			}

			foreach (var video in videos)
			{
				if (video.DoctorId != doctor.DoctorId)
				{
					return StatusCode(403, new { success = false, message = "You can only delete your own videos." });
				}
				if (System.IO.File.Exists(video.FilePath))
				{
					System.IO.File.Delete(video.FilePath);
				}
				_context.Materials.Remove(video);
			}
			await _context.SaveChangesAsync();

			return Ok(new { success = true, message = "Selected videos deleted successfully." });
		}

		#endregion

		#region GetAllVideosOfSpecificDoctor
		[HttpGet("getAllVideosOfDoctor/{courseCode}/{doctorId}")]
		public async Task<IActionResult> GetAllVideosOfSpecificDoctor(string courseCode, int doctorId)
		{
			// التحقق من وجود الدكتور
			var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.DoctorId == doctorId);
			if (doctor == null)
			{
				return NotFound(new { success = false, message = "Doctor not found." });
			}

			// التحقق من وجود الكورس
			var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseCode == courseCode);
			if (course == null)
			{
				return NotFound(new { success = false, message = "Course not found." });
			}

			var videos = await _context.Materials
				.Where(m => m.DoctorId == doctorId && m.CourseCode == courseCode && m.TypeFile == "video")
				.ToListAsync();

			if (!videos.Any())
			{
				return NotFound(new { success = false, message = "No videos found for this doctor in the specified course." });
			}

			return Ok(new { success = true, message = "Videos retrieved successfully.", videos });
		}
		#endregion

		#region GetAllVideosOfCourse
		[HttpGet("getAllVideosOfCourse/{courseCode}")]
		[Authorize(Roles = "Doctor")]
		public async Task<IActionResult> GetAllVideosOfCourse(string courseCode)
		{

			var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseCode == courseCode);
			if (course == null)
			{
				return NotFound(new { success = false, message = "Course not found." });
			}

			var videos = await _context.Materials
				.Where(m => m.CourseCode == courseCode && m.TypeFile == "video") // تأكد من نوع المادة "video"
				.ToListAsync();

			if (!videos.Any())
			{
				return NotFound(new { success = false, message = "No videos found for this course." });
			}

			return Ok(new { success = true, videos });
		}

		#endregion

		#region GetDoctorVideosForCourseBasedOnType

		[HttpGet("getDoctorMaterials/{courseCode}/{userId}/{typeFile}")]
		[AllowAnonymous]
		// Token Student
		public async Task<IActionResult> GetDoctorMaterialsForCourse(string courseCode, string userId, string typeFile)
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
				.Where(m => m.CourseCode == courseCode && m.DoctorId == doctor.DoctorId && m.TypeFile == "Video")
				.ToListAsync();

			if (!materials.Any())
			{
				return NotFound(new { success = false, message = "No Videos found for this doctor in the specified course." });
			}

			return Ok(new
			{
				success = true,
				message = "Videos retrieved successfully.",
				materials = materials
			});
		}

		#endregion

	}
}
		

