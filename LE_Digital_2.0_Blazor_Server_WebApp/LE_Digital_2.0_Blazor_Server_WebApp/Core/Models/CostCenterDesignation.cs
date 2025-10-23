using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore; // For Keyless attribute

namespace LE_Digital_2_Blazor_Server_WebApp.Core.Models
{
    [Table("costCenterDesignation")]
    [Keyless]
    public class CostCenterDesignation
    {
        public string? CostCenter { get; set; }
        public string? Vp { get; set; }
        public string? Responsible { get; set; }

        // Add missing properties
        public string? User { get; set; }
        public string? CostCenterName { get; set; }
    }
}