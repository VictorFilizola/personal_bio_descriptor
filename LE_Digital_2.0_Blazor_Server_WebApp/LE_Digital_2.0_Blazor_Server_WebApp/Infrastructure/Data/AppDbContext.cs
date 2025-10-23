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
        public DbSet<CostCenterGridTemplate> CostCenterGridTemplates { get; set; } // Use existing model name

        // DbSet for the Historic Data View
        public DbSet<HistoricData> HistoricData { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure HistoricData
            modelBuilder.Entity<HistoricData>(entity =>
            {
                entity.HasNoKey();
                entity.ToView("le_historic");
            });

            // Configure CostCenterDesignation
            modelBuilder.Entity<CostCenterDesignation>(entity =>
            {
                entity.HasKey(e => e.CostCenterID); // Specify the primary key
                entity.ToTable("costCenterDesignation");
                // Ensure Denomination maps correctly if needed
                entity.Property(e => e.Denomination).HasColumnName("denomination");
            });

            // Configure CostCenterGridTemplate (assuming table name matches class name)
            modelBuilder.Entity<CostCenterGridTemplate>(entity =>
            {
                entity.ToTable("CostCenterGridTemplate"); // Explicitly set table name
                entity.HasKey(e => e.CostCenterSubID); // Specify the primary key
                                                       // Map ContaGerencial if needed
                entity.Property(e => e.ContaGerencial).HasColumnName("contaGerencial");
            });

            // You might need to configure decimal precision for CostCenterSub/HistoricData months here too
            modelBuilder.Entity<CostCenterSub>(entity =>
            {
                entity.Property(e => e.January).HasColumnType("decimal(18, 2)");
                entity.Property(e => e.February).HasColumnType("decimal(18, 2)");
                entity.Property(e => e.March).HasColumnType("decimal(18, 2)");
                entity.Property(e => e.April).HasColumnType("decimal(18, 2)");
                entity.Property(e => e.May).HasColumnType("decimal(18, 2)");
                entity.Property(e => e.June).HasColumnType("decimal(18, 2)");
                entity.Property(e => e.July).HasColumnType("decimal(18, 2)");
                entity.Property(e => e.August).HasColumnType("decimal(18, 2)");
                entity.Property(e => e.September).HasColumnType("decimal(18, 2)");
                entity.Property(e => e.October).HasColumnType("decimal(18, 2)");
                entity.Property(e => e.November).HasColumnType("decimal(18, 2)");
                entity.Property(e => e.December).HasColumnType("decimal(18, 2)");
            });
            modelBuilder.Entity<HistoricData>(entity =>
            {
                entity.Property(e => e.January).HasColumnType("decimal(18, 2)");
                entity.Property(e => e.February).HasColumnType("decimal(18, 2)");
                entity.Property(e => e.March).HasColumnType("decimal(18, 2)");
                entity.Property(e => e.April).HasColumnType("decimal(18, 2)");
                entity.Property(e => e.May).HasColumnType("decimal(18, 2)");
                entity.Property(e => e.June).HasColumnType("decimal(18, 2)");
                entity.Property(e => e.July).HasColumnType("decimal(18, 2)");
                entity.Property(e => e.August).HasColumnType("decimal(18, 2)");
                entity.Property(e => e.September).HasColumnType("decimal(18, 2)");
                entity.Property(e => e.October).HasColumnType("decimal(18, 2)");
                entity.Property(e => e.November).HasColumnType("decimal(18, 2)");
                entity.Property(e => e.December).HasColumnType("decimal(18, 2)");
            });
        }
    }
}