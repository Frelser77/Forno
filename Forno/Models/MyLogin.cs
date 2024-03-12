using System.ComponentModel.DataAnnotations;

namespace Forno.Models
{
    public class MyLogin
    {
        [Required]
        [Display(Name = "Username")]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
