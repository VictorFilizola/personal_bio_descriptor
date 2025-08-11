using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LE_Digital_2_Blazor_Server_WebApp.Core.Models
{
    [Table("user")]
    public class User
    {
        [Key]
        public int UserID { get; set; }
        public string? Name { get; set; }
        public string? Login { get; set; }
        public string? Permission { get; set; }
        public string? Email { get; set; }
        public int? ThemePreference { get; set; }
    }
}