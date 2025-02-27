using Edu_plat.Model.OTP;
using Edu_plat.Model;
using JWT.DATA;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Edu_plat.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ChatController : ControllerBase
	{
		private readonly ApplicationDbContext _context;

		public ChatController(ApplicationDbContext context)
		{
			_context = context;
		}

		#region CreateChat
		// Endpoint to create a new chat message
		[HttpPost("CreateChat")]
		
		public async Task<IActionResult> CreateChat([FromBody] ChatMessageDTO chatMessageDto)
		{
			if (chatMessageDto == null || string.IsNullOrEmpty(chatMessageDto.SenderId) ||
				string.IsNullOrEmpty(chatMessageDto.ReceiverId))
			{
				return BadRequest(new { success = false, message = "Invalid chat message data" });
			}

			// Create a new chat message
			var chat = new Chat
			{
				SenderId = chatMessageDto.SenderId,
				ReceiverId = chatMessageDto.ReceiverId,
				CreatedDate = DateTime.UtcNow
			};

			await _context.Chats.AddAsync(chat);
			await _context.SaveChangesAsync();

			return Ok(new { success = true, message = "Chat message created successfully", data = chat });
		}
		#endregion

		#region GetChatContacts
		[HttpGet("GetChatContacts")]

		public async Task<IActionResult> GetChatContacts([FromQuery] string userId)
		{
			if (string.IsNullOrEmpty(userId))
				return BadRequest(new { success = false, message = "UserId is required" });


			var contactIds = await _context.Chats
				.Where(c => c.SenderId == userId || c.ReceiverId == userId)
				.Select(c => c.SenderId == userId ? c.ReceiverId : c.SenderId)
				.Distinct()
				.ToListAsync();

			return Ok(new { success = true, contacts = contactIds });
		}
		#endregion

		#region Commnet
		//[HttpPost("CreateChat")]
		//[Authorize] // يسمح فقط للمستخدمين المسجلين بالدخول
		//public async Task<IActionResult> CreateChat([FromBody] ChatMessageDTO chatMessageDto)
		//{
		//	// التحقق من صحة البيانات المُرسلة
		//	if (chatMessageDto == null || string.IsNullOrEmpty(chatMessageDto.SenderId) ||
		//		string.IsNullOrEmpty(chatMessageDto.ReceiverId) || string.IsNullOrEmpty(chatMessageDto.Message))
		//	{
		//		return BadRequest(new { success = false, message = "Invalid chat message data" });
		//	}

		//	// التأكد من وجود المُرسل في جدول المستخدمين
		//	var sender = await _context.Users.FirstOrDefaultAsync(u => u.Id == chatMessageDto.SenderId);
		//	if (sender == null)
		//		return NotFound(new { success = false, message = "Sender not found" });

		//	// التأكد من وجود المستقبل في جدول المستخدمين
		//	var receiver = await _context.Users.FirstOrDefaultAsync(u => u.Id == chatMessageDto.ReceiverId);
		//	if (receiver == null)
		//		return NotFound(new { success = false, message = "Receiver not found" });

		//	// إنشاء رسالة الدردشة الجديدة مع تعيين تاريخ الإنشاء الحالي (UTC)
		//	var chat = new Chat
		//	{
		//		SenderId = chatMessageDto.SenderId,
		//		ReceiverId = chatMessageDto.ReceiverId,
		//		Message = chatMessageDto.Message,
		//		CreatedDate = DateTime.UtcNow
		//	};

		//	await _context.Chats.AddAsync(chat);
		//	await _context.SaveChangesAsync();

		//	return Ok(new { success = true, message = "Chat message created successfully", data = chat });
		//} 
		#endregion

		#region Comment

		//[HttpGet("GetChatContactsWithUser")]
		//[Authorize] // يسمح فقط للمستخدمين الموثقين بالوصول لهذا الـ Endpoint
		//public async Task<IActionResult> GetChatContactsWithUser([FromQuery] string userId)
		//{
		//	if (string.IsNullOrEmpty(userId))
		//		return BadRequest(new { success = false, message = "UserId is required" });

		//	// استعلام لجلب كافة المعرفات الفريدة للأشخاص الذين تحدث معهم المستخدم
		//	var contactIds = await _context.Chats
		//		.Where(c => c.SenderId == userId || c.ReceiverId == userId)
		//		.Select(c => c.SenderId == userId ? c.ReceiverId : c.SenderId)
		//		.Distinct()
		//		.ToListAsync();

		//	// ربط المعرفات بجدول المستخدمين لاسترجاع البيانات الكاملة (مثل الاسم، البريد الإلكتروني، رقم الهاتف، والصورة)
		//	var contacts = await _context.Users
		//		.Where(u => contactIds.Contains(u.Id))
		//		.Select(u => new
		//		{
		//			u.Id,
		//			u.UserName,
		//			u.Email,
		//			u.PhoneNumber,
		//			ProfilePicture = u.ProfilePicture != null
		//							 ? $"data:image/jpeg;base64,{Convert.ToBase64String(u.ProfilePicture)}"
		//							 : null
		//		})
		//		.ToListAsync();

		//	return Ok(new { success = true, contacts });
		//}

		#endregion


	}
}
