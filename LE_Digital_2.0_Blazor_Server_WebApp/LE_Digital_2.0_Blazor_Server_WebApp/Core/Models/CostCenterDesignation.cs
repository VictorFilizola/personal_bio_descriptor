using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LE_Digital_2_Blazor_Server_WebApp.Core.Models
{
    [Table("costCenterDesignation")]
    public class CostCenterDesignation
    {
        [Key]
        // Assuming IDENTITY - remove DatabaseGenerated if it's not auto-incrementing
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CostCenterID { get; set; }

        public string? CostCenter { get; set; }

        [Column("denomination")] // Map to the correct column name
        public string? Denomination { get; set; }

        public string? Responsible { get; set; } // This is the Manager's name
        public string? Vp { get; set; }
    }
}