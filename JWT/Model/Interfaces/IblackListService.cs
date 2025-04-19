namespace Edu_plat.Model.Interfaces
{
    public interface IblackListService
    {
        Task<bool> IsBlackListedAsync(string jti);
        Task AddTokenToBlackListAsync(string jti, DateTime expiryDate);
    }
}
