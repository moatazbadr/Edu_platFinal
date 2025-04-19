using Edu_plat.Model.Interfaces;
using Edu_plat.Services;
using JWT.DATA;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Edu_plat.Services
{
    public class BlacklistService : IblackListService
    {
        private readonly ApplicationDbContext _context;

        public BlacklistService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> IsBlackListedAsync(string jti)
        {
            return await _context.BlackListTokens.AnyAsync(x => x.jti == jti);
        }

        public async Task AddTokenToBlackListAsync(string jti, DateTime expiryDate)
        {
            var token = new Model.BlackListTokens
            {
                jti = jti,
                ExpiryDate = expiryDate
            };

            await _context.BlackListTokens.AddAsync(token);
            await _context.SaveChangesAsync();
        }
    }
}
