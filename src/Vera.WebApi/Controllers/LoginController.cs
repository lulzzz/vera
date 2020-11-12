using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vera.Models;
using Vera.Security;
using Vera.Stores;
using Vera.WebApi.Models;
using Vera.WebApi.Security;

namespace Vera.WebApi.Controllers
{
    [ApiController]
    [Route("login")]
    [Authorize]
    public class LoginController : ControllerBase
    {
        private readonly ICompanyStore _companyStore;
        private readonly IUserStore _userStore;
        private readonly ITokenFactory _tokenFactory;
        private readonly IPasswordStrategy _passwordStrategy;
        private readonly ISecurityTokenGenerator _securityTokenGenerator;

        public LoginController(
            ICompanyStore companyStore,
            IUserStore userStore,
            ITokenFactory tokenFactory,
            IPasswordStrategy passwordStrategy,
            ISecurityTokenGenerator securityTokenGenerator
        )
        {
            _companyStore = companyStore;
            _userStore = userStore;
            _tokenFactory = tokenFactory;
            _passwordStrategy = passwordStrategy;
            _securityTokenGenerator = securityTokenGenerator;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Index(Login model)
        {
            var company = await _companyStore.GetByName(model.CompanyName);

            if (company == null)
            {
                return Unauthorized();
            }

            var user = await _userStore.GetByCompany(company.Id, model.Username);

            if (user == null)
            {
                return Unauthorized();
            }

            if (!_passwordStrategy.Verify(model.Password, user.Authentication))
            {
                return Unauthorized();
            }

            return await Authorize(user, company);
        }

        [HttpPost]
        [Route("refresh")]
        public async Task<IActionResult> Refresh(Refresh model)
        {
            var username = User.FindFirstValue(Security.ClaimTypes.Username);
            var companyId = Guid.Parse(User.FindFirstValue(Security.ClaimTypes.CompanyId));

            var user = await _userStore.GetByCompany(companyId, username);

            if (!string.Equals(user.RefreshToken, model.Token))
            {
                // TODO(kevin): what to return here? (token is invalid)
                return BadRequest();
            }

            var company = await _companyStore.GetByName(User.FindFirstValue(Security.ClaimTypes.CompanyName));

            return await Authorize(user, company);
        }

        private async Task<IActionResult> Authorize(User user, Company company)
        {
            var refreshToken = _tokenFactory.Create();

            user.RefreshToken = refreshToken;

            await _userStore.Update(user);

            return Ok(new
            {
                token = _securityTokenGenerator.Generate(user, company),
                refreshToken
            });            
        }
    }
}