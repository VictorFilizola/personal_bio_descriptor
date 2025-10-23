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

        // This is varchar(200) in your database
        public string? ManagerID { get; set; }

        public string? Status { get; set; }
        public string? CostCenterCode { get; set; }
        public string? CostCenterName { get; set; }
        public string? User { get; set; }
        public string? Vp { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? AllocatedValue { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? UsedValue { get; set; }

        // REMOVED the [ForeignKey] and ManagerParent navigation property
        // public virtual ManagerParent? ManagerParent { get; set; }
    }
}