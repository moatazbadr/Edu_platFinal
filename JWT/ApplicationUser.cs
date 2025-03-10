
using Edu_plat.Model;
using Edu_plat.Model.Course_registeration;
using JWT.DATA;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace JWT
{
	public class ApplicationUser : IdentityUser 
	{
        public byte[]? profilePicture { get; set; }

		
		public Student Student { get; set; }
		public Doctor Doctor { get; set; }

      public ICollection<TodoItems> todoItems { get; set; } = new List<TodoItems>();
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        // public ICollection<Course> Courses { get; set; } =new List<Course>();

    }
}
