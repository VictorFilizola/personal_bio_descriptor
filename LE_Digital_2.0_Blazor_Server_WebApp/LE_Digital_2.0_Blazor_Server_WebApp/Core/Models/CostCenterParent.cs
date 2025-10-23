using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LE_Digital_2_Blazor_Server_WebApp.Core.Models
{
    [Table("costCenterParent")]
    public class CostCenterParent
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CostCenterID { get; set; }

        public string? VersionID { get; set; }

        // REVIEW TYPE: Is this string or int in your DB table?
        // If it's INT and links to ManagerParent.ManagerID, change to: public int? ManagerID { get; set; }
        public string? ManagerID { get; set; }

        public string? Status { get; set; }
        public string? CostCenterCode { get; set; }
        public string? CostCenterName { get; set; } // Should map to 'denomination' via service logic if needed, or rename here if DB changes
        public string? User { get; set; } // Populated with Manager Name by DirectorService
        public string? Vp { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? AllocatedValue { get; set; } // Set via pop-up on previous screen

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? UsedValue { get; set; } // Sum of CostCenterSub monthly values

        // --- Navigation Property Added ---
        // Links CostCenterParent back to the ManagerParent.
        // EF Core uses this for includes. Needs ManagerID to be correctly typed.
        // If ManagerID is string and ManagerParent.ManagerID is int, this FK attribute might cause issues.
        // Let's assume ManagerID is INT for this to work cleanly. If not, remove [ForeignKey] and rely on Include in service.
        [ForeignKey("ManagerID")]
        public virtual ManagerParent? ManagerParent { get; set; }
    }
}