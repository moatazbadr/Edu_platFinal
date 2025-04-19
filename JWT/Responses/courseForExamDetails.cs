using System.ComponentModel.DataAnnotations;

namespace Edu_plat.Responses
{
    public class courseForExamDetails
    {
        public string title { get; set; }
        public string courseCode { get; set; }
        public int time { get; set; }
        public int totalMarks { get; set; }
        public int semester { get; set; }
        public string program { get; set; }
        public int level { get; set; }

    }
}
