using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LE_Digital_2_Blazor_Server_WebApp.Core.Models
{
    [Table("vpList")]
    public class VpList
    {
        [Key]
        public int VpListId { get; set; }
        public string? VpName { get; set; }
        public string? Responsible { get; set; } 
    }
}