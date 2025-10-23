using LE_Digital_2_Blazor_Server_WebApp.Core.Interfaces;
using LE_Digital_2_Blazor_Server_WebApp.Core.Models;
using LE_Digital_2_Blazor_Server_WebApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LE_Digital_2_Blazor_Server_WebApp.Infrastructure.Services
{
    public class ManagerService : IManagerService
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public ManagerService(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<List<ManagerParent>> GetPendingManagerTasksAsync(ClaimsPrincipal user)
        {
            var managerName = user.FindFirstValue("DisplayName");
            if (string.IsNullOrEmpty(managerName))
            {
                return new List<ManagerParent>();
            }

            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.ManagerParents
                .Where(m => m.ManagerName == managerName && m.Status == "Não realizado")
                .ToListAsync();
        }

        public async Task<ManagerParent?> GetManagerTaskDetailsAsync(int managerParentId, ClaimsPrincipal user)
        {
            var managerName = user.FindFirstValue("DisplayName");
            await using var context = await _contextFactory.CreateDbContextAsync();

            // Security check: Ensure the user only gets their own tasks
            return await context.ManagerParents
                .FirstOrDefaultAsync(m => m.ManagerID == managerParentId && m.ManagerName == managerName);
        }

        public async Task<List<CostCenterParent>> GetCostCentersForManagerAsync(int managerParentId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            // Find all cost centers linked to this ManagerParent task
            return await context.CostCenterParents
                .Where(cc => cc.ManagerID == managerParentId.ToString())
                .OrderBy(cc => cc.CostCenterCode)
                .ToListAsync();
        }
    }
}