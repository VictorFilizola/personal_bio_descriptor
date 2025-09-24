using LE_Digital_2_Blazor_Server_WebApp.Core.Interfaces;
using LE_Digital_2_Blazor_Server_WebApp.Core.Models;
using LE_Digital_2_Blazor_Server_WebApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace LE_Digital_2_Blazor_Server_WebApp.Infrastructure.Repositories
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

            // This performs a case-insensitive search.
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Login != null && u.Login.ToUpper() == login.ToUpper());
        }

        // NOTE: The other methods (GetUserByNameAsync, GetAllUsersAsync) were missing from this file.
        // You should move them from the other UserService.cs into this one, or decide which one to keep.
        // For now, I will add them here to ensure the project compiles.

        public async Task<User?> GetUserByNameAsync(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }
            return await _context.Users.FirstOrDefaultAsync(u => u.Name == name);
        }

        public async Task<System.Collections.Generic.List<User>> GetAllUsersAsync()
        {
            return await _context.Users.OrderBy(u => u.Name).ToListAsync();
        }
    }
}