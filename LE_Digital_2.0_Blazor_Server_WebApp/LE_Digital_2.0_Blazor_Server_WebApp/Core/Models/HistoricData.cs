using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LE_Digital_2_Blazor_Server_WebApp.Core.Models
{
    [Keyless]
    public class HistoricData
    {
        [Column("centroCusto")]
        public string? CentroCusto { get; set; }

        public string? ManagingAccount { get; set; }

        [Column("year")]
        public int? Year { get; set; }

        // *** FIX: Changed all month types from decimal? to double? ***
        // This must match the data type in your SQL View (float/real)
        public double? January { get; set; }
        public double? February { get; set; }
        public double? March { get; set; }
        public double? April { get; set; }
        public double? May { get; set; }
        public double? June { get; set; }
        public double? July { get; set; }
        public double? August { get; set; }
        public double? September { get; set; }
        public double? October { get; set; }
        public double? November { get; set; }
        public double? December { get; set; }

        public string? CostCenter { get; set; }
        public string? Responsible { get; set; }
    }
}