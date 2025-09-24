using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LE_Digital_2_Blazor_Server_WebApp.Core.Models
{
    [Table("vpList")]
    public class VpList
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int VpListId { get; set; }
        public string? VpName { get; set; }
        [Column("responsable")]
        public string? Responsible { get; set; }
    }
}