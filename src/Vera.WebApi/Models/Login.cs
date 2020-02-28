using System.ComponentModel.DataAnnotations;

namespace Vera.WebApi.Models
{
    public class Login
    {
        /// <summary>
        /// Name of the company to log on to.
        /// </summary>
        [Required]
        public string CompanyName { get; set; }

        /// <summary>
        /// Username of the user to log on with.
        /// </summary>
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }
    }
}