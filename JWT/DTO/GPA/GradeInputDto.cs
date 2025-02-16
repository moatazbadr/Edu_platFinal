using System.ComponentModel.DataAnnotations;

namespace Edu_plat.DTO.GPA
{
	public class GradeInputDto
	{
		[RegularExpression(@"^(A|A-|B\+|B|C\+|C|D|F|a|a-|b\+|b|c\+|c|d|f)$|^[0-9]+\.?[0-9]*$|^[0-9]*\.?[0-9]+$", ErrorMessage = "Invalid grade value. Valid values are A, A-, B+, B, C+, C, D, F. Or Numbers")]

		public string Grade { get; set; }

		public int CreditHours { get; set; }
	}
}
