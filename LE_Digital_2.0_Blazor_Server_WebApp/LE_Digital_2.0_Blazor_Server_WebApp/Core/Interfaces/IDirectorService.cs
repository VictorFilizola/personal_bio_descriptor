using LE_Digital_2_Blazor_Server_WebApp.Core.Models;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LE_Digital_2_Blazor_Server_WebApp.Core.Interfaces
{
    // Represents the manager allocation data passed from the UI
    public class ManagerAllocation
    {
        public string ManagerName { get; set; } = string.Empty;
        public decimal AllocatedInvestment { get; set; }
    }

    public interface IDirectorService
    {
        Task<List<VpParent>> GetPendingAllocationsAsync(ClaimsPrincipal user);
        Task<VpParent?> GetVpAllocationDetailsAsync(int vpId, ClaimsPrincipal user);
        Task<List<string>> GetManagersForVpAsync(string vpName);
        Task<List<CostCenterDesignation>> GetCostCentersForManagerAsync(string managerName, string vpName);
        Task CompleteStep2Async(int vpId, int versionId, string vpName, List<ManagerAllocation> allocations, IEmailService emailService, IUserService userService, IVersionService versionService);
    }
}