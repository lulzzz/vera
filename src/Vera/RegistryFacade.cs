using System.Collections.Generic;
using System.Threading.Tasks;
using Vera.Security;

namespace Vera
{
    public class RegistryFacade
    {
        private readonly ICompanyStore _companyStore;
        private readonly IUserStore _userStore;

        public RegistryFacade(ICompanyStore companyStore, IUserStore userStore)
        {
            _companyStore = companyStore;
            _userStore = userStore;
        }

        public async Task Register(Company company, UserToCreate userToCreate)
        {
            var existingCompany = await _companyStore.GetByName(company.Name);

            if (existingCompany != null)
            {
                // TODO(kevin): handle the case where the company is created but the user is not
                return;
            }

            var user = await _userStore.Store(userToCreate);

            var companyToCreate = new Company(company)
            {
                Users = new List<User> {user}
            };

            await _companyStore.Store(companyToCreate);
        }
    }
}