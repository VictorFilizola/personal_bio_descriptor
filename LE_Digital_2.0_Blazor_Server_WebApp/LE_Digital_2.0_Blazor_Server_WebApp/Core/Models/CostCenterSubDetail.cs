using LE_Digital_2_Blazor_Server_WebApp.Core.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LE_Digital_2_Blazor_Server_WebApp.Core.Models
{
    // This is a DTO (Data Transfer Object) for the export
    public class CostCenterSubDetail
    {
        // From CostCenterSub
        public int CostCenterSubID { get; set; }
        public int? CostCenterParentID { get; set; }
        public int? ManagerID { get; set; }
        public int? VersionID { get; set; }
        public string? ContaGerencial { get; set; }
        public decimal? January { get; set; }
        public decimal? February { get; set; }
        public decimal? March { get; set; }
        public decimal? April { get; set; }
        public decimal? May { get; set; }
        public decimal? June { get; set; }
        public decimal? July { get; set; }
        public decimal? August { get; set; }
        public decimal? September { get; set; }
        public decimal? October { get; set; }
        public decimal? November { get; set; }
        public decimal? December { get; set; }

        // From CostCenterParent
        public string? CostCenterCode { get; set; }
        public string? CostCenterName { get; set; }
    }
}