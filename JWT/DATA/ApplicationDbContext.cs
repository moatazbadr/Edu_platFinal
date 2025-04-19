
using Edu_plat.Model;
using Edu_plat.Model.Course_registeration;
using Edu_plat.Model.Exams;
using Edu_plat.Model.OTP;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace JWT.DATA
{
	public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
	{

        public ApplicationDbContext()
        {
        }

		public ApplicationDbContext(DbContextOptions options) : base(options)
        {
            
        }
		
		

		
		protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
			modelBuilder.Entity<ApplicationUser>().HasOne(a => a.Student).WithOne(s => s.applicationUser).HasForeignKey<Student>(s => s.UserId).OnDelete(DeleteBehavior.NoAction);
			modelBuilder.Entity<ApplicationUser>().HasOne(a => a.Doctor).WithOne(s => s.applicationUser).HasForeignKey<Doctor>(s => s.UserId).OnDelete(DeleteBehavior.NoAction);

			// Doctor - Course (Many-to-Many)
			modelBuilder.Entity<CourseDoctor>()
				.HasKey(dc => new { dc.DoctorId, dc.CourseId });

			modelBuilder.Entity<CourseDoctor>()
				.HasOne(dc => dc.Doctor)
				.WithMany(d => d.CourseDoctors)
				.HasForeignKey(dc => dc.DoctorId);

			modelBuilder.Entity<CourseDoctor>()
				.HasOne(dc => dc.Course)
				.WithMany(c => c.CourseDoctors)
				.HasForeignKey(dc => dc.CourseId);

			// Doctor - Material (One-to-Many)
			modelBuilder.Entity<Material>()
				.HasOne(m => m.Doctor)
				.WithMany(d => d.Materials)
				.HasForeignKey(m => m.DoctorId)
				.OnDelete(DeleteBehavior.Cascade);

			// Course - Material (One-to-Many)
			modelBuilder.Entity<Material>()
				.HasOne(m => m.Course)
				.WithMany(c => c.Materials)
				.HasForeignKey(m => m.CourseId)
				.OnDelete(DeleteBehavior.Cascade);



			modelBuilder.Entity<Exam>()
			   .HasOne(e => e.Course)   // امتحان له كورس واحد
			   .WithMany(c => c.Exams)  // كورس له عدة امتحانات
			   .HasForeignKey(e => e.CourseId) // مفتاح أجنبي
			   .OnDelete(DeleteBehavior.Cascade); // 
			modelBuilder.Entity<Question>()
			   .HasOne(q => q.Exam)
			   .WithMany(e => e.Questions)
			   .HasForeignKey(q => q.ExamId)
			   .OnDelete(DeleteBehavior.Cascade);
			// 🔹 تحديد العلاقة بين Choice و Question
			modelBuilder.Entity<Choice>()
				.HasOne(c => c.Question)
				.WithMany(q => q.Choices)
				.HasForeignKey(c => c.QuestionId)
				.OnDelete(DeleteBehavior.Cascade);
			// 🔹 العلاقة بين Exam و Doctor
			modelBuilder.Entity<Exam>()
				.HasOne(e => e.Doctor)
				.WithMany(d => d.Exams)
				.HasForeignKey(e => e.DoctorId)
				.OnDelete(DeleteBehavior.Cascade);
			// 🔹 إعداد العلاقة Many-to-Many بين Student و Exam
			modelBuilder.Entity<ExamStudent>()
				.HasKey(es => new { es.StudentId, es.ExamId }); // مفتاح مركب


			base.OnModelCreating(modelBuilder); 
        }
        #region DbSets
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<TodoItems> TodoItems { get; set; }
        public DbSet<OtpVerification> OtpVerification { get; set; }
        public DbSet<TemporaryUser> TemporaryUsers { get; set; }
        public DbSet<Course> Courses { get; set; } 
        public DbSet<Student> Students { get; set; }
		public DbSet<Material> Materials { get; set; }
		public DbSet<Exam> Exams { get; set; }
		public DbSet<Question> Question { get; set; }
		public DbSet<Choice> Choices { get; set; }
		public DbSet<CourseDoctor> CourseDoctors { get; set; }
		public DbSet<ExamStudent> ExamStudents { get; set; }
		public DbSet<AdminFile> AdminFiles { get; set; }
        public DbSet<BlackListTokens> BlackListTokens { get; set; }
        #endregion

    }
}
