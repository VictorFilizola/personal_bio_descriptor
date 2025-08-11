using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LE_Digital_2_Blazor_Server_WebApp.Core.Models
{
    [Table("costCenterDesignation")]
    public class CostCenterDesignation
    {
        [Key] // Assuming costCenterID is the primary key, though not specified in schema
        public int CostCenterID { get; set; }

        [Column("costCenter")]
        public string? CostCenter { get; set; }

        [Column("denomination")]
        public string? Denomination { get; set; }

        [Column("responsible")]
        public string? Responsible { get; set; }

        [Column("vp")]
        public string? Vp { get; set; }
    }
}