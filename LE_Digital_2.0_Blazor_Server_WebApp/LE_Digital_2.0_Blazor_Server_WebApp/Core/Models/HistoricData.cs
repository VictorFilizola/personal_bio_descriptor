using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore; // Needed for Keyless attribute if not using DataAnnotations Schema

namespace LE_Digital_2_Blazor_Server_WebApp.Core.Models
{
    // Maps to the le_historic view
    [Keyless] // Views don't have a primary key defined for EF Core
    public class HistoricData
    {
        // Corrected property name to match view column 'centroCusto'
        // Using [Column] attribute to ensure correct mapping
        [Column("centroCusto")]
        public string? CentroCusto { get; set; }

        public string? ManagingAccount { get; set; }

        // Changed 'Year' to match casing, although EF Core is often case-insensitive for properties vs columns
        [Column("year")]
        public int? Year { get; set; }

        // Month properties (ensure type matches DB, decimal is usually safe)
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

        // The view also selects costCenter and responsible, let's include them
        public string? CostCenter { get; set; } // This seems redundant with centroCusto? Check view logic if needed.
        public string? Responsible { get; set; }
    }
}