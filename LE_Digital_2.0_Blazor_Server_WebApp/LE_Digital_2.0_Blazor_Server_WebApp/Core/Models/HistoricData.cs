using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LE_Digital_2_Blazor_Server_WebApp.Core.Models
{
    // This class represents the le_historic view in the database.
    // Use Keyless attribute as views typically don't have a single primary key.
    [Keyless] // Important for views!
    public class HistoricData
    {
        // Property names should match view column names for simplicity
        public string? centroCusto { get; set; }
        public string? ManagingAccount { get; set; }
        public int? Year { get; set; }

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

        public string? CostCenter { get; set; }
        public string? Responsible { get; set; }
    }
}