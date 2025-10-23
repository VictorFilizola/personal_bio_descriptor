using LE_Digital_2_Blazor_Server_WebApp.Core.Interfaces;
using LE_Digital_2_Blazor_Server_WebApp.Core.Models;
using LE_Digital_2_Blazor_Server_WebApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging; // Added for logging
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
        private readonly ILogger<ManagerService> _logger; // Added logger

        public ManagerService(IDbContextFactory<AppDbContext> contextFactory, ILogger<ManagerService> logger) // Inject logger
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
            // Convert int ManagerParentId to string for comparison if DB column is varchar
            string managerIdString = managerParentId.ToString();
            return await context.CostCenterParents
                .Where(cc => cc.ManagerID == managerIdString) // Ensure type match for comparison
                .OrderBy(cc => cc.CostCenterCode)
                .ToListAsync();
        }

        public async Task<CostCenterParent?> GetCostCenterDetailsAsync(int costCenterParentId, ClaimsPrincipal user)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var managerName = user.FindFirstValue("DisplayName");

            // Efficiently check ownership by including ManagerParent
            var costCenter = await context.CostCenterParents
                                    .Include(cc => cc.ManagerParent) // Requires Navigation Property
                                    .FirstOrDefaultAsync(cc => cc.CostCenterID == costCenterParentId);

            // Security Check: Does it exist and belong to the current manager?
            if (costCenter?.ManagerParent?.ManagerName == managerName)
            {
                return costCenter;
            }

            _logger.LogWarning("Attempt to access unauthorized CostCenterParent {CostCenterId} by user {ManagerName}.", costCenterParentId, managerName);
            return null; // Not found or not authorized
        }


        public async Task<List<CostCenterSub>> GetCostCenterSubAllocationsAsync(int costCenterParentId)
        {
            // This method might be used later if we want to load saved progress
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.CostCenterSubs
                .Where(sub => sub.CostCenterParentID == costCenterParentId)
                .OrderBy(sub => sub.ContaGerencial)
                .ToListAsync();
        }

        public async Task<List<HistoricData>> GetHistoricDataForCostCenterAsync(string costCenterCode, int year = 2024)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            _logger.LogInformation("Fetching historic data for CostCenterCode: {Code}, Year: {Year}", costCenterCode, year); // Added logging
            var data = await context.HistoricData
                .Where(h => h.CentroCusto == costCenterCode // Use the corrected property name 'CentroCusto'
                         && h.Year == year
                         && h.ManagingAccount != "Personnel Costs"
                         && h.ManagingAccount != "Depreciation")
                .OrderBy(h => h.ManagingAccount)
                .ToListAsync();
            _logger.LogInformation("Fetched {Count} historic data records.", data.Count); // Added logging
            return data;
        }

        public async Task<List<CostCenterGridTemplate>> GetContaGerencialTemplateAsync()
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            // Ensure DbSet name matches AppDbContext (e.g., CostCenterGridTemplates)
            return await context.CostCenterGridTemplates
                .OrderBy(t => t.ContaGerencial)
                .ToListAsync();
        }

        public async Task<bool> UpdateCostCenterAllocatedValueAsync(int costCenterParentId, decimal allocatedValue, ClaimsPrincipal user)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var managerName = user.FindFirstValue("DisplayName");

            var costCenterParent = await context.CostCenterParents
                .Include(cc => cc.ManagerParent)
                .FirstOrDefaultAsync(cc => cc.CostCenterID == costCenterParentId);

            if (costCenterParent == null || costCenterParent.ManagerParent?.ManagerName != managerName)
            {
                _logger.LogWarning("User {ManagerName} attempted to update CC {CostCenterId} they don't own or CC not found.", managerName, costCenterParentId);
                return false;
            }

            // Optional: Prevent changing if already 'Realizado'?
            // if (costCenterParent.Status == "Realizado") return false;

            costCenterParent.AllocatedValue = allocatedValue;
            // When setting allocated value, reset UsedValue and Status?
            costCenterParent.UsedValue = 0; // Resetting usage makes sense here
            costCenterParent.Status = "Pendente"; // Mark as pending again

            context.CostCenterParents.Update(costCenterParent);

            try
            {
                await context.SaveChangesAsync();
                _logger.LogInformation("Updated AllocatedValue for CostCenterParent {CostCenterId} to {Value}", costCenterParentId, allocatedValue);
                return true;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error updating AllocatedValue for CostCenterParent {CostCenterId}", costCenterParentId);
                return false;
            }
        }

        public async Task SaveCostCenterAllocationsAsync(int costCenterParentId, decimal allocatedValue, decimal newUsedValue, List<CostCenterSub> updatedSubAllocations)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                var parent = await context.CostCenterParents.FindAsync(costCenterParentId);
                if (parent == null) throw new InvalidOperationException($"Cost Center Parent with ID {costCenterParentId} not found.");

                // Update parent with values confirmed on this screen
                parent.AllocatedValue = allocatedValue; // The value set initially via pop-up
                parent.UsedValue = newUsedValue; // The actual sum from the monthly grid
                parent.Status = "Realizado";
                context.CostCenterParents.Update(parent);

                // Delete existing Sub records for this Parent
                var existingSubs = await context.CostCenterSubs
                                         .Where(s => s.CostCenterParentID == costCenterParentId)
                                         .ToListAsync();
                if (existingSubs.Any())
                {
                    context.CostCenterSubs.RemoveRange(existingSubs);
                    _logger.LogInformation("Removed {Count} existing CostCenterSub records for Parent {Id}", existingSubs.Count, costCenterParentId);
                }

                // Add the new/updated Sub records
                int managerId = int.TryParse(parent.ManagerID, out int mId) ? mId : 0; // Assuming 0 is invalid or handle nullable int?
                int versionId = int.TryParse(parent.VersionID, out int vId) ? vId : 0; // Assuming 0 is invalid or handle nullable int?

                foreach (var sub in updatedSubAllocations)
                {
                    sub.CostCenterParentID = costCenterParentId;
                    sub.ManagerID = managerId != 0 ? managerId : (int?)null;
                    sub.VersionID = versionId != 0 ? versionId : (int?)null;
                    sub.CostCenterSubID = 0; // Ensure EF Core treats as new
                    context.CostCenterSubs.Add(sub);
                }
                _logger.LogInformation("Adding {Count} new CostCenterSub records for Parent {Id}", updatedSubAllocations.Count, costCenterParentId);


                await context.SaveChangesAsync(); // Save parent update and sub inserts/deletes
                await transaction.CommitAsync();
                _logger.LogInformation("Successfully saved allocations for CostCenterParent {Id}", costCenterParentId);

            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error saving Cost Center Allocations for Parent {Id}", costCenterParentId);
                throw; // Re-throw to notify the UI
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
                    context.ManagerParents.Update(managerTask);
                    await context.SaveChangesAsync();
                    _logger.LogInformation("ManagerParent task {Id} marked as Realizado.", managerParentId);
                    // TODO: Consider sending email notifications here
                }
                else
                {
                    _logger.LogInformation("ManagerParent task {Id} not finished, not all Cost Centers are Realizado.", managerParentId);
                }
            }
        }
    }
}