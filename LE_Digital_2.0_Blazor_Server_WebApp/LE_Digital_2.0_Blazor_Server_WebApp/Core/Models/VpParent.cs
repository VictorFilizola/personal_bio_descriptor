using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LE_Digital_2_Blazor_Server_WebApp.Core.Models
{
    [Table("vpParent")]
    public class VpParent
    {
        [Key]
        public int VpID { get; set; }
        public string? Status { get; set; }
        public string? VersionID { get; set; }
        public string? VpName { get; set; }
        public string? VpUser { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? TotalInvestment { get; set; }
    }
}
