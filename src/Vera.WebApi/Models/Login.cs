namespace Vera.WebApi.Models
{
    public class Login
    {
        /// <summary>
        /// Name of the company to log on to.
        /// </summary>
        public string Company { get; set; }

        /// <summary>
        /// Username of the user to log on with.
        /// </summary>
        public string Username { get; set; }

        public string Password { get; set; }
    }
}