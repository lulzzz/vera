using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vera.Security;

namespace Vera
{
    public class UserToCreate
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public UserType Type { get; set; }
    }

    public class UserRegisterFacade
    {
        private readonly ICompanyStore _companyStore;
        private readonly IUserStore _userStore;
        private readonly IPasswordStrategy _passwordStrategy;

        public UserRegisterFacade(
            ICompanyStore companyStore,
            IUserStore userStore,
            IPasswordStrategy passwordStrategy
        )
        {
            _companyStore = companyStore;
            _userStore = userStore;
            _passwordStrategy = passwordStrategy;
        }

        public async Task Register(string companyName, UserToCreate userToCreate)
        {
            Guid companyId;

            var existingCompany = await _companyStore.GetByName(companyName);

            if (existingCompany != null)
            {
                // TODO(kevin): handle the case where the company is created but the user is not

                var existingUser = await _userStore.GetByCompany(existingCompany.Id, userToCreate.Username);

                if (existingUser != null)
                {
                    // TODO(kevin): return error because username is already in use
                    return;
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
        }
    }
}