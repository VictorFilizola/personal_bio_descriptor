using LE_Digital_2_Blazor_Server_WebApp.Core.Models;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LE_Digital_2_Blazor_Server_WebApp.Core.Interfaces
{
    public interface IManagerService
    {
        Task<List<ManagerParent>> GetPendingManagerTasksAsync(ClaimsPrincipal user);
        Task<ManagerParent?> GetManagerTaskDetailsAsync(int managerParentId, ClaimsPrincipal user);
        Task<List<CostCenterParent>> GetCostCentersForManagerAsync(int managerParentId);
        Task<CostCenterParent?> GetCostCenterDetailsAsync(int costCenterParentId, ClaimsPrincipal user);
        Task<List<CostCenterSub>> GetCostCenterSubAllocationsAsync(int costCenterParentId); // Gets previously saved subs
        Task<List<HistoricData>> GetHistoricDataForCostCenterAsync(string costCenterCode, int year);

        // Uses the existing CostCenterGridTemplate model
        Task<List<CostCenterGridTemplate>> GetContaGerencialTemplateAsync();

        // Saves the detailed monthly allocations from the EditCostCenterDetails page
        Task SaveCostCenterAllocationsAsync(int costCenterParentId, decimal allocatedValue, decimal newUsedValue, List<CostCenterSub> updatedSubAllocations);

        // Updates the ManagerParent status if all associated CostCenterParents are done
        Task FinishManagerTaskAsync(int managerParentId);
    }
}