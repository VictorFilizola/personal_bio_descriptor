using LE_Digital_2_Blazor_Server_WebApp.Core.Interfaces;
using LE_Digital_2_Blazor_Server_WebApp.Core.Models;
using LE_Digital_2_Blazor_Server_WebApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LE_Digital_2_Blazor_Server_WebApp.Infrastructure.Repositories
{
    public class UserService : IUserService
    {
        // *** CHANGE THIS: Inject the factory ***
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        // *** CHANGE THIS: Update constructor ***
        public UserService(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<User?> GetUserByLoginAsync(string login)
        {
            if (string.IsNullOrEmpty(login))
            {
                return null;
            }
            // *** CHANGE THIS: Create context instance ***
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Users
                .FirstOrDefaultAsync(u => u.Login != null && u.Login.ToUpper() == login.ToUpper());
        }

        public async Task<User?> GetUserByNameAsync(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }
            // *** CHANGE THIS: Create context instance ***
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Users.FirstOrDefaultAsync(u => u.Name == name);
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            // *** CHANGE THIS: Create context instance ***
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Users.OrderBy(u => u.Name).ToListAsync();
        }
    }
}