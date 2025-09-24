using LE_Digital_2_Blazor_Server_WebApp.Core.Interfaces;
using LE_Digital_2_Blazor_Server_WebApp.Core.Models;
using LE_Digital_2_Blazor_Server_WebApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
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
            if (string.IsNullOrEmpty(login)) return null;
            return await _context.Users.FirstOrDefaultAsync(u => u.Login != null && u.Login.ToUpper() == login.ToUpper());
        }

        public async Task<User?> GetUserByNameAsync(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;
            return await _context.Users.FirstOrDefaultAsync(u => u.Name == name);
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _context.Users.OrderBy(u => u.Name).ToListAsync();
        }
    }
}