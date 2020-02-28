using System.ComponentModel.DataAnnotations;

namespace Vera.WebApi.Models
{
    public class Register
    {
        [Required]
        public string CompanyName { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }
    }
}