using LE_Digital_2_Blazor_Server_WebApp.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace LE_Digital_2_Blazor_Server_WebApp.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Add a DbSet for each model class.
        // This tells EF Core that these correspond to tables in database.
        public DbSet<User> Users { get; set; }
        public DbSet<VersionParent> VersionParents { get; set; }
        public DbSet<VpParent> VpParents { get; set; }
        public DbSet<VpList> VpLists { get; set; }
        public DbSet<ManagerParent> ManagerParents { get; set; }
        public DbSet<CostCenterParent> CostCenterParents { get; set; }
        public DbSet<CostCenterSub> CostCenterSubs { get; set; }

        // These tables from from the schema are raw data/templates.
        public DbSet<CostCenterDesignation> CostCenterDesignations { get; set; }
        public DbSet<CostCenterGridTemplate> CostCenterGridTemplates { get; set; }
    }
}