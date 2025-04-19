namespace Edu_plat.Model
{
    public class BlackListTokens
    {
        public int Id { get; set; }
        public string jti { get; set; } =string.Empty;
        public DateTime ExpiryDate { get; set; }    
    }
}
