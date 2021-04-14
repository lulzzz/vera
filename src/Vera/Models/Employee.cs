using Newtonsoft.Json;

namespace Vera.Models
{
    public class Employee
    {
        public string SystemId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        [JsonIgnore]
        public string FullName => $"{FirstName} {LastName}";
    }
}