using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LE_Digital_2_Blazor_Server_WebApp.Core.Models
{
    [Table("costCenterGridTem   plate")]
    public class CostCenterGridTemplate
    {
        [Key]
        public int CostCenterSubID { get; set; }
        public int? CostCenterParentID { get; set; }
        public string? ContaGerencial { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? January { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? February { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? March { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? April { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? May { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? June { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? July { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? August { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? September { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? October { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? November { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? December { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? TotalValue { get; set; }
    }
}