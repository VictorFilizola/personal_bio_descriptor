using LE_Digital_2_Blazor_Server_WebApp.Core.Interfaces;
using LE_Digital_2_Blazor_Server_WebApp.Core.Models;
using LE_Digital_2_Blazor_Server_WebApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging; // Import ILogger
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LE_Digital_2_Blazor_Server_WebApp.Infrastructure.Services
{
    public class DirectorService : IDirectorService
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly ILogger<DirectorService> _logger; // Add logger field

        // Inject ILogger
        public DirectorService(IDbContextFactory<AppDbContext> contextFactory, ILogger<DirectorService> logger)
        {
            _contextFactory = contextFactory;
            _logger = logger; // Initialize logger
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
            await using var context = await _contextFactory.CreateDbContextAsync();
            using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
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
                    await context.SaveChangesAsync();

                    var managerUser = await userService.GetUserByNameAsync(allocation.ManagerName);
                    if (managerUser?.Email != null) managerEmails.Add(managerUser.Email);

                    var costCenters = await GetCostCentersForManagerAsync(allocation.ManagerName, vpName);

                    foreach (var cc in costCenters)
                    {
                        var newCostCenterParent = new CostCenterParent
                        {
                            VersionID = versionId.ToString(),
                            // *** FIX: Save the int ManagerID as a string ***
                            ManagerID = newManagerParent.ManagerID.ToString(),
                            Status = "Pendente",
                            CostCenterCode = cc.CostCenter,
                            CostCenterName = cc.Denomination,
                            User = allocation.ManagerName,
                            Vp = cc.Vp,
                            AllocatedValue = 0,
                            UsedValue = 0
                        };
                        context.CostCenterParents.Add(newCostCenterParent);
                    }
                }
                await context.SaveChangesAsync();

                await versionService.TrySetVersionStepAsync(versionId, "Step3 - Cost Center Allocation");

                await transaction.CommitAsync();

                // 4. Send Emails (after successful commit)
                if (!string.IsNullOrEmpty(directorEmail))
                {
                    await emailService.SendEmailAsync("victor.filizola_teixeira@bbraun.com", // directorEmail,
                       $"Step 2 Completed for {vpName} (Version {versionId})",
                       $"You have successfully allocated the budget for {vpName} to your managers.");
                }

                var controllers = await userService.GetAllUsersAsync();
                controllers = controllers.Where(u => u.Permission != null && (u.Permission.Contains("Controller") || u.Permission.Contains("Master"))).ToList();
                foreach (var controller in controllers)
                {
                    await emailService.SendEmailAsync("victor.filizola_teixeira@bbraun.com", // controller.Email,
                        $"Director Action: {vpParent.VpUser} completed Step 2 for {vpName} (Version {versionId})",
                        $"Director {vpParent.VpUser} has allocated the budget for {vpName}. Managers can now proceed.");
                }

                foreach (var managerEmail in managerEmails.Distinct())
                {
                    await emailService.SendEmailAsync("victor.filizola_teixeira@bbraun.com", // managerEmail,
                        $"Action Required: Budget Allocation for Cost Centers (Version {versionId})",
                        $"Dear Manager,\n\nYour Director ({vpParent.VpUser}) has allocated budget funds to you for Version {versionId}. Please log in to the LE Digital system to proceed with the Cost Center allocation.");
                }
            }
            catch (Exception ex)
            {
                // This line now works because _logger is injected
                _logger.LogError(ex, "Error during CompleteStep2Async for VpId {VpId}", vpId);
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}