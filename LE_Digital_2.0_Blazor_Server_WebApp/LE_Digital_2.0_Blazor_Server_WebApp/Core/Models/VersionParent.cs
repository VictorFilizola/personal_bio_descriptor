using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LE_Digital_2_Blazor_Server_WebApp.Core.Models
{
    [Table("versionParent")]
    public class VersionParent
    {
        [Key]
        public int VersionID { get; set; }
        public string? Status { get; set; }
        public string? Step { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? PlannedInvestment { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? UsedInvestment { get; set; }

        public DateTime? CreationDate { get; set; }
        public DateTime? FinishDate { get; set; }
    }
}