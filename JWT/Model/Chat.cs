namespace Edu_plat.Model
{
	public class Chat
	{
		public int Id { get; set; }
		public string SenderId { get; set; }   
		public string ReceiverId { get; set; }  
		public DateTime CreatedDate { get; set; }
	}
}
