using System.ComponentModel.DataAnnotations;

namespace Vera.WebApi.Models
{
    public class Account
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Certification { get; set; }
    }
}