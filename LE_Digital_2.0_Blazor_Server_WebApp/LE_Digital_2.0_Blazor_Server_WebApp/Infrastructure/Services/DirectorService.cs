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
    public class DirectorService : IDirectorService
    {
        // *** CHANGE THIS: Inject the factory ***
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        // *** CHANGE THIS: Update constructor ***
        public DirectorService(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<List<VpParent>> GetPendingAllocationsAsync(ClaimsPrincipal user)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var directorName = user.FindFirstValue("DisplayName");
            if (string.IsNullOrEmpty(directorName))
            {
                return new List<VpParent>();
            }

            return await context.VpParents
                .Where(vp => vp.VpUser == directorName && vp.Status == "VPStep1 - VP Manager Values Allocation")
                .ToListAsync();
        }

        public async Task<VpParent?> GetVpAllocationDetailsAsync(int vpId, ClaimsPrincipal user)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var directorName = user.FindFirstValue("DisplayName");
            return await context.VpParents
                .FirstOrDefaultAsync(vp => vp.VpID == vpId && vp.VpUser == directorName && vp.Status == "VPStep1 - VP Manager Values Allocation");
        }

        public async Task<List<string>> GetManagersForVpAsync(string vpName)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.CostCenterDesignations
                .Where(cc => cc.Vp == vpName && !string.IsNullOrEmpty(cc.Responsible))
                .Select(cc => cc.Responsible!)
                .Distinct()
                .OrderBy(name => name)
                .ToListAsync();
        }

        public async Task<List<CostCenterDesignation>> GetCostCentersForManagerAsync(string managerName, string vpName)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.CostCenterDesignations
                .Where(cc => cc.Responsible == managerName && cc.Vp == vpName)
                .ToListAsync();
        }

        public async Task CompleteStep2Async(int vpId, int versionId, string vpName, List<ManagerAllocation> allocations, IEmailService emailService, IUserService userService, IVersionService versionService)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(); // Create context instance
            using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                // 1. Update VpParent status
                var vpParent = await context.VpParents.FindAsync(vpId);
                if (vpParent == null || vpParent.Status != "VPStep1 - VP Manager Values Allocation")
                {
                    await transaction.RollbackAsync();
                    return;
                }
                vpParent.Status = "VPStep2 - VP Manager Values Allocated";
                context.VpParents.Update(vpParent);
                await context.SaveChangesAsync();

                var managerEmails = new List<string>();
                var directorEmail = (await userService.GetUserByNameAsync(vpParent.VpUser ?? ""))?.Email;

                // 2. Create ManagerParent and CostCenterParent records
                foreach (var allocation in allocations)
                {
                    var newManagerParent = new ManagerParent
                    {
                        VersionID = versionId.ToString(),
                        ManagerName = allocation.ManagerName,
                        AllocatedInvestment = allocation.AllocatedInvestment,
                        UsedInvestment = 0,
                        Status = "Não realizado"
                    };
                    context.ManagerParents.Add(newManagerParent);
                    await context.SaveChangesAsync(); // Get ManagerID

                    var managerUser = await userService.GetUserByNameAsync(allocation.ManagerName);
                    if (managerUser?.Email != null) managerEmails.Add(managerUser.Email);

                    // Use the separate GetCostCentersForManagerAsync method
                    // Note: This requires injecting the factory into DirectorService itself OR passing context
                    // For simplicity, let's call it via this instance (which will create a new context):
                    var costCenters = await GetCostCentersForManagerAsync(allocation.ManagerName, vpName);

                    foreach (var cc in costCenters)
                    {
                        var newCostCenterParent = new CostCenterParent
                        {
                            VersionID = versionId.ToString(),
                            ManagerID = newManagerParent.ManagerID.ToString(),
                            Status = "Pendente",
                            CostCenterCode = cc.CostCenter,
                            CostCenterName = cc.Denomination, // Use Denomination from the source model
                            User = allocation.ManagerName, // Set User field to the Manager's Name
                            Vp = cc.Vp,
                            AllocatedValue = 0,
                            UsedValue = 0
                        };
                        context.CostCenterParents.Add(newCostCenterParent);
                    }
                }
                await context.SaveChangesAsync(); // Save CostCenterParents

                // 3. Update main VersionParent step
                await versionService.TrySetVersionStepAsync(versionId, "Step3 - Cost Center Allocation");

                await transaction.CommitAsync();

                // 4. Send Emails (after successful commit) - Logic remains the same
                if (!string.IsNullOrEmpty(directorEmail)) { /* ... send email ... */ }
                var controllers = await userService.GetAllUsersAsync(); // Re-fetch users outside the disposed context
                controllers = controllers.Where(u => u.Permission != null && (u.Permission.Contains("Controller") || u.Permission.Contains("Master"))).ToList();
                foreach (var controller in controllers) { /* ... send email ... */ }
                foreach (var managerEmail in managerEmails.Distinct()) { /* ... send email ... */ }

            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}