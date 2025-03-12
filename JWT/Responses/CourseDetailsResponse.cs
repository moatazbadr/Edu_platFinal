using Edu_plat.Model;

namespace Edu_plat.Responses
{
    public class CourseDetailsResponse
    {
        public string CourseCode { get; set; }
        public string CourseDescription { get; set; }
        public int MidTerm { get; set; }
        public int Oral { get; set; }
        public int FinalExam { get; set; }
        public int Lab { get; set; }
        public int TotalMark { get; set; }
        public int CourseCreditHours { get; set; }
        public string doctorName { get; set; }
       public int LectureCount { get; set; }
    }
}
