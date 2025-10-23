using LE_Digital_2_Blazor_Server_WebApp.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace LE_Digital_2_Blazor_Server_WebApp.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // DbSets for your tables
        public DbSet<User> Users { get; set; }
        public DbSet<VersionParent> VersionParents { get; set; }
        public DbSet<VpParent> VpParents { get; set; }
        public DbSet<VpList> VpLists { get; set; }
        public DbSet<ManagerParent> ManagerParents { get; set; }
        public DbSet<CostCenterParent> CostCenterParents { get; set; }
        public DbSet<CostCenterSub> CostCenterSubs { get; set; }
        public DbSet<CostCenterDesignation> CostCenterDesignations { get; set; }

        // Add DbSet for the Historic Data View
        public DbSet<HistoricData> HistoricData { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure the HistoricData model to map to the 'le_historic' view
            modelBuilder.Entity<HistoricData>(entity =>
            {
                entity.HasNoKey(); // Reinforce that it's keyless
                entity.ToView("le_historic"); // Specify the view name
            });

            // You can add other specific configurations for your tables here if needed
        }
    }
}