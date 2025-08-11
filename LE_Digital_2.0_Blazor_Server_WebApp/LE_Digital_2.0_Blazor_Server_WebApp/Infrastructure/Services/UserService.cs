using LE_Digital_2_Blazor_Server_WebApp.Core.Interfaces;
using LE_Digital_2_Blazor_Server_WebApp.Core.Models;
using LE_Digital_2_Blazor_Server_WebApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace LE_Digital_2_Blazor_Server_WebApp.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;

        public UserService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetUserByLoginAsync(string login)
        {
            if (string.IsNullOrEmpty(login))
            {
                return null;
            }

            // This performs a case-insensitive search for the user by their login name.
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Login != null && u.Login.ToUpper() == login.ToUpper());
        }
    }
}