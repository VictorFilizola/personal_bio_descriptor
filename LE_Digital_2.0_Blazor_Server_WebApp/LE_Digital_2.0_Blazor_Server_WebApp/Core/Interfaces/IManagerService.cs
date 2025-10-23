using LE_Digital_2_Blazor_Server_WebApp.Core.Models;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LE_Digital_2_Blazor_Server_WebApp.Core.Interfaces
{
    public interface IManagerService
    {
        /// <summary>
        /// Gets a list of all pending manager-level tasks for the currently logged-in user.
        /// </summary>
        Task<List<ManagerParent>> GetPendingManagerTasksAsync(ClaimsPrincipal user);

        /// <summary>
        /// Gets the details for a single manager task (ManagerParent)
        /// </summary>
        Task<ManagerParent?> GetManagerTaskDetailsAsync(int managerParentId, ClaimsPrincipal user);

        /// <summary>
        /// Gets the list of cost centers (CostCenterParent) associated with a specific manager task.
        /// </summary>
        Task<List<CostCenterParent>> GetCostCentersForManagerAsync(int managerParentId);
    }
}