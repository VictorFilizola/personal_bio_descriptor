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
        private readonly AppDbContext _context;

        public DirectorService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<VpParent>> GetPendingAllocationsAsync(ClaimsPrincipal user)
        {
            var directorName = user.FindFirstValue("DisplayName"); // Get name from claim
            if (string.IsNullOrEmpty(directorName))
            {
                return new List<VpParent>();
            }

            return await _context.VpParents
                .Where(vp => vp.VpUser == directorName && vp.Status == "VPStep1 - VP Manager Values Allocation")
                .ToListAsync();
        }

        public async Task<VpParent?> GetVpAllocationDetailsAsync(int vpId, ClaimsPrincipal user)
        {
            var directorName = user.FindFirstValue("DisplayName");
            return await _context.VpParents
                .FirstOrDefaultAsync(vp => vp.VpID == vpId && vp.VpUser == directorName && vp.Status == "VPStep1 - VP Manager Values Allocation");
        }

        public async Task<List<string>> GetManagersForVpAsync(string vpName)
        {
            // Query distinct managers from CostCenterDesignation based on vpName
            return await _context.CostCenterDesignations
                .Where(cc => cc.Vp == vpName && !string.IsNullOrEmpty(cc.Responsible))
                .Select(cc => cc.Responsible!)
                .Distinct()
                .OrderBy(name => name)
                .ToListAsync();
        }

        public async Task<List<CostCenterDesignation>> GetCostCentersForManagerAsync(string managerName, string vpName)
        {
            return await _context.CostCenterDesignations
                .Where(cc => cc.Responsible == managerName && cc.Vp == vpName)
                .ToListAsync();
        }

        public async Task CompleteStep2Async(int vpId, int versionId, string vpName, List<ManagerAllocation> allocations, IEmailService emailService, IUserService userService, IVersionService versionService)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Update VpParent status
                var vpParent = await _context.VpParents.FindAsync(vpId);
                if (vpParent == null || vpParent.Status != "VPStep1 - VP Manager Values Allocation")
                {
                    await transaction.RollbackAsync();
                    return; // Or throw an exception
                }
                vpParent.Status = "VPStep2 - VP Manager Values Allocated";
                _context.VpParents.Update(vpParent);
                await _context.SaveChangesAsync(); // Save this change first

                var managerEmails = new List<string>();
                var directorEmail = (await userService.GetUserByNameAsync(vpParent.VpUser ?? ""))?.Email;

                // 2. Create ManagerParent and CostCenterParent records
                foreach (var allocation in allocations)
                {
                    // Create ManagerParent
                    var newManagerParent = new ManagerParent
                    {
                        VersionID = versionId.ToString(),
                        ManagerName = allocation.ManagerName,
                        AllocatedInvestment = allocation.AllocatedInvestment,
                        UsedInvestment = 0,
                        Status = "Não realizado" // Initial status for manager step
                    };
                    _context.ManagerParents.Add(newManagerParent);
                    await _context.SaveChangesAsync(); // Save to get the new ManagerID

                    // Add manager email for notification
                    var managerUser = await userService.GetUserByNameAsync(allocation.ManagerName);
                    if (managerUser?.Email != null) managerEmails.Add(managerUser.Email);

                    // Get CostCenters for this Manager/VP
                    var costCenters = await GetCostCentersForManagerAsync(allocation.ManagerName, vpName);

                    foreach (var cc in costCenters)
                    {
                        var newCostCenterParent = new CostCenterParent
                        {
                            VersionID = versionId.ToString(),
                            ManagerID = newManagerParent.ManagerID.ToString(), // Link to the newly created ManagerParent
                            Status = "Pendente", // Initial status
                            CostCenterCode = cc.CostCenter,
                            CostCenterName = cc.CostCenterName,
                            User = cc.User,
                            Vp = cc.Vp,
                            AllocatedValue = 0, // Manager hasn't allocated this yet
                            UsedValue = 0
                        };
                        _context.CostCenterParents.Add(newCostCenterParent);
                    }
                }
                await _context.SaveChangesAsync(); // Save all CostCenterParents

                // 3. Update main VersionParent step (if needed)
                await versionService.TrySetVersionStepAsync(versionId, "Step3 - Cost Center Allocation");

                await transaction.CommitAsync();

                // 4. Send Emails (after successful commit)
                // Director Confirmation
                if (!string.IsNullOrEmpty(directorEmail))
                {
                    await emailService.SendEmailAsync("victor.filizola_teixeira@bbraun.com", // directorEmail, // Send to your email for testing
                        $"Step 2 Completed for {vpName} (Version {versionId})",
                        $"You have successfully allocated the budget for {vpName} to your managers.");
                }

                // Controller Notification
                var controllers = await _context.Users.Where(u => u.Permission != null && (u.Permission.Contains("Controller") || u.Permission.Contains("Master"))).ToListAsync();
                foreach (var controller in controllers)
                {
                    await emailService.SendEmailAsync("victor.filizola_teixeira@bbraun.com", // controller.Email, // Send to your email for testing
                        $"Director Action: {vpParent.VpUser} completed Step 2 for {vpName} (Version {versionId})",
                        $"Director {vpParent.VpUser} has allocated the budget for {vpName}. Managers can now proceed.");
                }

                // Manager Notification
                foreach (var managerEmail in managerEmails.Distinct())
                {
                    await emailService.SendEmailAsync("victor.filizola_teixeira@bbraun.com", // managerEmail, // Send to your email for testing
                        $"Action Required: Budget Allocation for Cost Centers (Version {versionId})",
                        $"Dear Manager,\n\nYour Director ({vpParent.VpUser}) has allocated budget funds to you for Version {versionId}. Please log in to the LE Digital system to proceed with the Cost Center allocation.");
                }
            }
            catch
            {
                await transaction.RollbackAsync();
                // Log the error appropriately
                throw; // Re-throw the exception
            }
        }
    }
}