using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LE_Digital_2_Blazor_Server_WebApp.Core.Models
{
    [Table("managerParent")]
    public class ManagerParent
    {
        [Key]
        public int ManagerID { get; set; }
        public string? VersionID { get; set; }
        public string? ManagerName { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? AllocatedInvestment { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? UsedInvestment { get; set; }

        public string? Status { get; set; }
    }
}