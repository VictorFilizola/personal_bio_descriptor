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
        private readonly AppDbContext _context;

        public VersionService(AppDbContext context)
        {
            _context = context;
        }
        public async Task TrySetVersionStepAsync(int versionId, string newStep)
        {
            var version = await _context.VersionParents.FindAsync(versionId);
            // Only update if the current step is before the new step
            // (You might need a more robust way to compare steps if they aren't linear like Step1, Step2, Step3)
            if (version != null && version.Step != newStep && version.Step != "Step3 - Cost Center Allocation") // Example check
            {
                version.Step = newStep;
                _context.VersionParents.Update(version);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<VersionParent>> GetAllVersionsAsync()
        {
            return await _context.VersionParents.OrderByDescending(v => v.CreationDate).ToListAsync();
        }

        public async Task<VersionParent?> GetVersionByIdAsync(int versionId)
        {
            return await _context.VersionParents.FindAsync(versionId);
        }

        public async Task CreateVersionAsync(VersionParent version)
        {
            _context.VersionParents.Add(version);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateVersionAsync(VersionParent version)
        {
            _context.Entry(version).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteVersionAsync(int versionId)
        {
            var version = await _context.VersionParents.FindAsync(versionId);
            if (version != null)
            {
                _context.VersionParents.Remove(version);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<VpList>> GetVpListAsync()
        {
            return await _context.VpLists.ToListAsync();
        }

        public async Task CompleteStep1Async(int versionId, List<VpParent> allocations, IEmailService emailService, IUserService userService)
        {
            var version = await _context.VersionParents.FindAsync(versionId);
            if (version == null) return;

            version.Step = "Step2 - Manager Cost Allocation";
            _context.VersionParents.Update(version);

            foreach (var allocation in allocations)
            {
                allocation.VersionID = versionId.ToString();
                allocation.Status = "VPStep1 - VP Manager Values Allocation";
                _context.VpParents.Add(allocation);
            }

            await _context.SaveChangesAsync();

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
    }
}