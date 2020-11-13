using System;
using System.Threading.Tasks;
using Vera.Models;
using Vera.Security;
using Vera.Stores;

namespace Vera.Services
{
    public class UserToCreate
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public UserType Type { get; set; }
    }

    public interface IUserRegisterService
    {
        Task<Error> Register(string companyName, UserToCreate userToCreate);
    }

    public class UserRegisterService : IUserRegisterService
    {
        private readonly ICompanyStore _companyStore;
        private readonly IUserStore _userStore;
        private readonly IPasswordStrategy _passwordStrategy;

        public UserRegisterService(
            ICompanyStore companyStore,
            IUserStore userStore,
            IPasswordStrategy passwordStrategy
        )
        {
            _companyStore = companyStore;
            _userStore = userStore;
            _passwordStrategy = passwordStrategy;
        }

        public async Task<Error> Register(string companyName, UserToCreate userToCreate)
        {
            Guid companyId;

            var existingCompany = await _companyStore.GetByName(companyName);

            if (existingCompany != null)
            {
                // TODO(kevin): handle the case where the company is created but the user is not

                var existingUser = await _userStore.GetByCompany(existingCompany.Id, userToCreate.Username);

                if (existingUser != null)
                {
                    return new Error(ErrorCode.Exists, $"User {userToCreate.Username} already exists");
                }

                companyId = existingCompany.Id;
            }
            else
            {
                var company = await _companyStore.Store(new Company
                {
                    Id = Guid.NewGuid(),
                    Name = companyName
                });

                companyId = company.Id;
            }

            await _userStore.Store(new User
            {
                Id = Guid.NewGuid(),
                Username = userToCreate.Username,
                Authentication = _passwordStrategy.Encrypt(userToCreate.Password),
                Type = userToCreate.Type,
                CompanyId = companyId
            });

            return null;
        }
    }
}