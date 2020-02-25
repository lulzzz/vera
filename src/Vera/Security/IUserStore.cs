using System;
using System.Threading.Tasks;

namespace Vera.Security
{
    public class UserToCreate
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public UserType Type { get; set; }
        public Guid Company { get; set; }
    }

    public interface IUserStore
    {
        Task Store(UserToCreate toCreate);

        Task<User> Get(string username, Guid companyId);
    }
}