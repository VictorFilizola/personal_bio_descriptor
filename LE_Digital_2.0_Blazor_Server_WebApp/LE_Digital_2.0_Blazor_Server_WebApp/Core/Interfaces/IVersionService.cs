using LE_Digital_2_Blazor_Server_WebApp.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LE_Digital_2_Blazor_Server_WebApp.Core.Interfaces
{
    public interface IVersionService
    {
        Task<List<VersionParent>> GetAllVersionsAsync();
        Task<VersionParent?> GetVersionByIdAsync(int versionId);
        Task CreateVersionAsync(VersionParent version);
        Task UpdateVersionAsync(VersionParent version);
        Task DeleteVersionAsync(int versionId);
        Task<List<VpList>> GetVpListAsync();
        Task CompleteStep1Async(int versionId, List<VpParent> allocations, IEmailService emailService, IUserService userService);
        Task TrySetVersionStepAsync(int versionId, string newStep); 
    }
}