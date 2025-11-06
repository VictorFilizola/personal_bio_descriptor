using LE_Digital_2_Blazor_Server_WebApp.Core.Interfaces;
using LE_Digital_2_Blazor_Server_WebApp.Core.Models;
using LE_Digital_2_Blazor_Server_WebApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LE_Digital_2_Blazor_Server_WebApp.Infrastructure.Services
{
    public class VersionService : IVersionService
    {
        // *** CHANGE THIS: Inject the factory ***
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        // *** CHANGE THIS: Update constructor ***
        public VersionService(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<List<VersionParent>> GetAllVersionsAsync()
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.VersionParents.OrderByDescending(v => v.CreationDate).ToListAsync();
        }

        public async Task<VersionParent?> GetVersionByIdAsync(int versionId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.VersionParents.FindAsync(versionId);
        }

        public async Task CreateVersionAsync(VersionParent version)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            context.VersionParents.Add(version);
            await context.SaveChangesAsync();
        }

        public async Task UpdateVersionAsync(VersionParent version)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            context.Entry(version).State = EntityState.Modified;
            await context.SaveChangesAsync();
        }

        public async Task DeleteVersionAsync(int versionId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var version = await context.VersionParents.FindAsync(versionId);
            if (version != null)
            {
                context.VersionParents.Remove(version);
                await context.SaveChangesAsync();
            }
        }

        public async Task<List<VpList>> GetVpListAsync()
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.VpLists.ToListAsync();
        }

        public async Task CompleteStep1Async(int versionId, List<VpParent> allocations, IEmailService emailService, IUserService userService)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            // Wrap in transaction for safety
            using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                var version = await context.VersionParents.FindAsync(versionId);
                if (version == null) return;

                version.Step = "Step2 - Manager Cost Allocation";
                context.VersionParents.Update(version);

                foreach (var allocation in allocations)
                {
                    allocation.VersionID = versionId.ToString();
                    allocation.Status = "VPStep1 - VP Manager Values Allocation";
                    context.VpParents.Add(allocation);
                }

                await context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Send emails after successful commit
                foreach (var allocation in allocations)
                {
                    var user = await userService.GetUserByNameAsync(allocation.VpName ?? "");
                    if (user?.Email != null)
                    {
                        string toEmail = "victor.filizola_teixeira@bbraun.com";
                        string subject = $"Action Required: Budget Allocation for {allocation.VpName}";
                        string body = $"Dear {allocation.VpName},\n\nA new budget has been allocated to you in the LE Digital system. Please log in to proceed with the manager cost allocation.";
                        await emailService.SendEmailAsync(toEmail, subject, body);
                    }
                }
            }
            catch
            {
                await transaction.RollbackAsync();
                throw; // Rethrow or handle error
            }
        }

        public async Task TrySetVersionStepAsync(int versionId, string newStep)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var version = await context.VersionParents.FindAsync(versionId);
            if (version != null && version.Step != newStep && version.Step != "Step3 - Cost Center Allocation")
            {
                version.Step = newStep;
                context.VersionParents.Update(version);
                await context.SaveChangesAsync();
            }
        }

        public async Task<List<ManagerParent>> GetManagerParentsAsync(int versionId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            string versionIdString = versionId.ToString();
            return await context.ManagerParents
                .Where(m => m.VersionID == versionIdString)
                .ToListAsync();
        }

        public async Task<List<CostCenterParent>> GetCostCenterParentsAsync(int versionId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            string versionIdString = versionId.ToString();
            return await context.CostCenterParents
                .Where(c => c.VersionID == versionIdString)
                .ToListAsync();
        }

        public async Task<List<CostCenterSubDetail>> GetCostCenterSubDetailsAsync(int versionId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            // This LINQ query replicates your SQL INNER JOIN
            return await context.CostCenterSubs
                .Where(ccs => ccs.VersionID == versionId)
                .Join(context.CostCenterParents,
                    ccs => ccs.CostCenterParentID,
                    ccp => ccp.CostCenterID,
                    (ccs, ccp) => new CostCenterSubDetail
                    {
                        CostCenterSubID = ccs.CostCenterSubID,
                        CostCenterParentID = ccs.CostCenterParentID,
                        ManagerID = ccs.ManagerID,
                        VersionID = ccs.VersionID,
                        ContaGerencial = ccs.ContaGerencial,
                        January = ccs.January,
                        February = ccs.February,
                        March = ccs.March,
                        April = ccs.April,
                        May = ccs.May,
                        June = ccs.June,
                        July = ccs.July,
                        August = ccs.August,
                        September = ccs.September,
                        October = ccs.October,
                        November = ccs.November,
                        December = ccs.December,
                        CostCenterCode = ccp.CostCenterCode,
                        CostCenterName = ccp.CostCenterName
                    })
                .OrderBy(c => c.CostCenterCode)
                .ThenBy(c => c.ContaGerencial)
                .ToListAsync();
        }
    }
}