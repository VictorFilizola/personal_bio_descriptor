using LE_Digital_2_Blazor_Server_WebApp.Core.Interfaces;
using LE_Digital_2_Blazor_Server_WebApp.Core.Models;
using LE_Digital_2_Blazor_Server_WebApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LE_Digital_2_Blazor_Server_WebApp.Infrastructure.Services
{
    public class ManagerService : IManagerService
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly ILogger<ManagerService> _logger;

        private async Task UpdateManagerParentUsedInvestmentAsync(string managerIdString, AppDbContext context)
        {
            if (int.TryParse(managerIdString, out int managerId))
            {
                var managerParent = await context.ManagerParents.FindAsync(managerId);
                if (managerParent != null)
                {
                    // *** FIX: Sum the UsedValue (monthly sum) from child cost centers ***
                    var totalUsedMonthly = await context.CostCenterParents
                        .Where(cc => cc.ManagerID == managerIdString)
                        .SumAsync(cc => cc.UsedValue ?? 0);

                    managerParent.UsedInvestment = totalUsedMonthly;
                    // *** END FIX ***

                    context.ManagerParents.Update(managerParent);
                    _logger.LogInformation("Updated ManagerParent {ManagerId} UsedInvestment to {Total}", managerId, totalUsedMonthly);
                }
            }
        }

        public ManagerService(IDbContextFactory<AppDbContext> contextFactory, ILogger<ManagerService> logger)
        {
            _contextFactory = contextFactory;
            _logger = logger;

        }

        public async Task<List<ManagerParent>> GetPendingManagerTasksAsync(ClaimsPrincipal user)
        {
            var managerName = user.FindFirstValue("DisplayName");
            if (string.IsNullOrEmpty(managerName)) return new List<ManagerParent>();
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.ManagerParents
                .Where(m => m.ManagerName == managerName && m.Status == "Não realizado")
                .ToListAsync();
        }

        public async Task<ManagerParent?> GetManagerTaskDetailsAsync(int managerParentId, ClaimsPrincipal user)
        {
            var managerName = user.FindFirstValue("DisplayName");
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.ManagerParents
                .FirstOrDefaultAsync(m => m.ManagerID == managerParentId && m.ManagerName == managerName);
        }

        public async Task<List<CostCenterParent>> GetCostCentersForManagerAsync(int managerParentId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            // *** FIX: Convert int to string for comparison ***
            string managerIdString = managerParentId.ToString();
            return await context.CostCenterParents
                .Where(cc => cc.ManagerID == managerIdString)
                .OrderBy(cc => cc.CostCenterCode)
                .ToListAsync();
        }

        public async Task<CostCenterParent?> GetCostCenterDetailsAsync(int costCenterParentId, ClaimsPrincipal user)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var managerName = user.FindFirstValue("DisplayName");

            var costCenter = await context.CostCenterParents
                                    .FirstOrDefaultAsync(cc => cc.CostCenterID == costCenterParentId);

            // *** FIX: Manual security check without navigation property ***
            if (costCenter == null || costCenter.ManagerID == null)
            {
                _logger.LogWarning("GetCostCenterDetails: CC {CostCenterId} not found or has no ManagerID.", costCenterParentId);
                return null; // Not found
            }

            // Check if the ManagerID (string) from CostCenterParent exists in ManagerParent (int)
            var managerParent = await context.ManagerParents
                .FirstOrDefaultAsync(mp => mp.ManagerID.ToString() == costCenter.ManagerID);

            if (managerParent != null && managerParent.ManagerName == managerName)
            {
                return costCenter; // Authorized
            }

            _logger.LogWarning("Attempt to access unauthorized CostCenterParent {CostCenterId} by user {ManagerName}.", costCenterParentId, managerName);
            return null; // Not authorized
        }


        public async Task<List<CostCenterSub>> GetCostCenterSubAllocationsAsync(int costCenterParentId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.CostCenterSubs
                .Where(sub => sub.CostCenterParentID == costCenterParentId)
                .OrderBy(sub => sub.ContaGerencial)
                .ToListAsync();
        }

        public async Task<List<HistoricData>> GetHistoricDataForCostCenterAsync(string costCenterCode, int year = 2025) // Default year is now 2025
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            _logger.LogInformation("Fetching historic data for CostCenterCode: {Code}, Year: {Year}", costCenterCode, year);

            var data = await context.HistoricData
                .Where(h => h.CentroCusto == costCenterCode
                         && h.Year == year // Using the year parameter (default 2025)
                         && h.ManagingAccount != "Personnel Costs"
                         && h.ManagingAccount != "Depreciation")
                .OrderBy(h => h.ManagingAccount)
                .ToListAsync();

            _logger.LogInformation("Fetched {Count} historic data records.", data.Count);
            return data;
        }

        public async Task<List<CostCenterGridTemplate>> GetContaGerencialTemplateAsync()
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.CostCenterGridTemplates
                .OrderBy(t => t.ContaGerencial)
                .ToListAsync();
        }

        public async Task SaveCostCenterAllocationsAsync(int costCenterParentId, decimal allocatedValue, decimal newUsedValue, List<CostCenterSub> updatedSubAllocations)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                var parent = await context.CostCenterParents.FindAsync(costCenterParentId);
                if (parent == null) throw new InvalidOperationException($"Cost Center Parent with ID {costCenterParentId} not found.");

                parent.AllocatedValue = allocatedValue;
                parent.UsedValue = newUsedValue;
                parent.Status = "Realizado";
                context.CostCenterParents.Update(parent);

                var existingSubs = await context.CostCenterSubs
                                         .Where(s => s.CostCenterParentID == costCenterParentId)
                                         .ToListAsync();
                if (existingSubs.Any())
                {
                    context.CostCenterSubs.RemoveRange(existingSubs);
                }

                int? managerId = int.TryParse(parent.ManagerID, out int mId) ? mId : (int?)null;
                int? versionId = int.TryParse(parent.VersionID, out int vId) ? vId : (int?)null;

                foreach (var sub in updatedSubAllocations)
                {
                    sub.CostCenterParentID = costCenterParentId;
                    sub.ManagerID = managerId;
                    sub.VersionID = versionId;
                    sub.CostCenterSubID = 0;
                    context.CostCenterSubs.Add(sub);
                }
                _logger.LogInformation("Adding/Updating {Count} CostCenterSub records for Parent {Id}", updatedSubAllocations.Count, costCenterParentId);

                await context.SaveChangesAsync(); // Save subs and parent changes

                // *** FIX: Add this call ***
                if (parent.ManagerID != null)
                {
                    await UpdateManagerParentUsedInvestmentAsync(parent.ManagerID, context);
                }
                await context.SaveChangesAsync(); // Save parent manager total
                                                  // *** END FIX ***

                await transaction.CommitAsync();
                _logger.LogInformation("Successfully saved allocations for CostCenterParent {Id}", costCenterParentId);

            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error saving Cost Center Allocations for Parent {Id}", costCenterParentId);
                throw;
            }
        }

        public async Task<bool> UpdateCostCenterAllocatedValueAsync(int costCenterParentId, decimal allocatedValue, ClaimsPrincipal user)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            using var transaction = await context.Database.BeginTransactionAsync(); // Add transaction
            var managerName = user.FindFirstValue("DisplayName");

            var costCenterParent = await context.CostCenterParents
                .FirstOrDefaultAsync(cc => cc.CostCenterID == costCenterParentId);

            if (costCenterParent == null || costCenterParent.ManagerID == null)
            {
                _logger.LogWarning("UpdateCostCenter: CC {CostCenterId} not found or has no ManagerID.", costCenterParentId);
                await transaction.RollbackAsync();
                return false;
            }

            var managerParent = await context.ManagerParents
                .FirstOrDefaultAsync(mp => mp.ManagerID.ToString() == costCenterParent.ManagerID);

            if (managerParent == null || managerParent.ManagerName != managerName)
            {
                _logger.LogWarning("User {ManagerName} attempted to update CC {CostCenterId} they don't own.", managerName, costCenterParentId);
                await transaction.RollbackAsync();
                return false;
            }

            costCenterParent.AllocatedValue = allocatedValue;
            costCenterParent.UsedValue = 0; // Reset UsedValue
            costCenterParent.Status = (allocatedValue == 0) ? "Realizado" : "Pendente"; // Set status

            context.CostCenterParents.Update(costCenterParent);

            try
            {
                await context.SaveChangesAsync(); // Save changes to costCenterParent

                // *** FIX: Update parent's UsedInvestment ***
                await UpdateManagerParentUsedInvestmentAsync(costCenterParent.ManagerID, context);
                await context.SaveChangesAsync(); // Save changes to managerParent
                                                  // *** END FIX ***

                await transaction.CommitAsync(); // Commit transaction
                _logger.LogInformation("Updated AllocatedValue for CostCenterParent {CostCenterId} to {Value}, Status set to {Status}", costCenterParentId, allocatedValue, costCenterParent.Status);
                return true;
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error updating AllocatedValue for CostCenterParent {CostCenterId}", costCenterParentId);
                return false;
            }
        }

        public async Task<bool> SaveZeroAllocationAsync(int costCenterParentId, ClaimsPrincipal user)
        {
            // 1. First, call the updated method to set parent value to 0 and update parent total
            var updateSuccess = await UpdateCostCenterAllocatedValueAsync(costCenterParentId, 0, user);
            if (!updateSuccess)
            {
                _logger.LogError("SaveZeroAllocation failed during parent update for CC {Id}", costCenterParentId);
                return false;
            }

            // 2. Now, create the 0-value sub-allocations
            await using var context = await _contextFactory.CreateDbContextAsync();
            using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                var parent = await context.CostCenterParents.FindAsync(costCenterParentId);
                if (parent == null) throw new InvalidOperationException("Cost Center Parent not found after update.");

                var templateAccounts = await context.CostCenterGridTemplates
                    .OrderBy(t => t.ContaGerencial)
                    .ToListAsync();

                var existingSubs = await context.CostCenterSubs
                    .Where(s => s.CostCenterParentID == costCenterParentId)
                    .ToListAsync();
                if (existingSubs.Any())
                {
                    context.CostCenterSubs.RemoveRange(existingSubs);
                }

                int? managerId = int.TryParse(parent.ManagerID, out int mId) ? mId : (int?)null;
                int? versionId = int.TryParse(parent.VersionID, out int vId) ? vId : (int?)null;

                foreach (var template in templateAccounts)
                {
                    var sub = new CostCenterSub
                    {
                        CostCenterParentID = costCenterParentId,
                        ManagerID = managerId,
                        VersionID = versionId,
                        ContaGerencial = template.ContaGerencial,
                        January = 0,
                        February = 0,
                        March = 0,
                        April = 0,
                        May = 0,
                        June = 0,
                        July = 0,
                        August = 0,
                        September = 0,
                        October = 0,
                        November = 0,
                        December = 0
                    };
                    context.CostCenterSubs.Add(sub);
                }

                await context.SaveChangesAsync();
                await transaction.CommitAsync();
                _logger.LogInformation("Successfully saved 0-value sub-allocations for CC {Id}", costCenterParentId);
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error in SaveZeroAllocationAsync while saving sub-items for CC {Id}", costCenterParentId);
                return false;
            }
        }



        public async Task FinishManagerTaskAsync(int managerParentId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var managerTask = await context.ManagerParents.FindAsync(managerParentId);
            if (managerTask != null && managerTask.Status == "Não realizado")
            {
                string managerIdString = managerParentId.ToString();
                bool allCostCentersDone = !await context.CostCenterParents
                                                .AnyAsync(cc => cc.ManagerID == managerIdString && cc.Status != "Realizado");

                if (allCostCentersDone)
                {
                    managerTask.Status = "Realizado";

                    // *** FIX: Add this call to update total ***
                    await UpdateManagerParentUsedInvestmentAsync(managerIdString, context);
                    // *** END FIX ***

                    context.ManagerParents.Update(managerTask);
                    await context.SaveChangesAsync();
                    _logger.LogInformation("ManagerParent task {Id} marked as Realizado.", managerParentId);
                }
                else
                {
                    _logger.LogInformation("ManagerParent task {Id} not finished, not all Cost Centers are Realizado.", managerParentId);
                }
            }
        }
    }
}