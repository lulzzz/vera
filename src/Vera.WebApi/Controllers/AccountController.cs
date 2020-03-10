using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vera.Security;

namespace Vera.WebApi.Controllers
{
    [ApiController]
    [Route("account")]
    [Authorize]
    public class AccountController : ControllerBase
    {
        private readonly ICompanyStore _companyStore;

        public AccountController(ICompanyStore companyStore)
        {
            _companyStore = companyStore;
         }

        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> Create(Models.Account model)
        {
            var companyId = User.FindFirstValue(Security.ClaimTypes.CompanyId);
            var company = await _companyStore.Get(Guid.Parse(companyId));

            var accounts = company.Accounts ?? new List<Account>();
            
            var existing = accounts.FirstOrDefault(a => a.Name.Equals(model.Name, StringComparison.OrdinalIgnoreCase));

            if (existing != null)
            {
                // TODO: account with name exists already
                return BadRequest();
            }

            accounts.Add(new Account
            {
                Id = Guid.NewGuid(),
                Certification = model.Certification,
                Name = model.Name
            });

            company.Accounts = accounts;

            await _companyStore.Update(company);

            return Ok();
        }
    }
}