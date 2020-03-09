using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Vera.Security;
using Vera.WebApi.Models;
using Vera.WebApi.Security;

namespace Vera.WebApi.Controllers
{
    [ApiController]
    [Route("login")]
    [Authorize]
    public class LoginController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ICompanyStore _companyStore;
        private readonly IUserStore _userStore;
        private readonly ITokenFactory _tokenFactory;
        private readonly IPasswordStrategy _passwordStrategy;
        private readonly ISecurityTokenGenerator _securityTokenGenerator;

        public LoginController(
            IConfiguration configuration,
            ICompanyStore companyStore,
            IUserStore userStore,
            ITokenFactory tokenFactory,
            IPasswordStrategy passwordStrategy,
            ISecurityTokenGenerator securityTokenGenerator
        )
        {
            _configuration = configuration;
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

            return await Authorize(user);
        }

        [HttpPost]
        [Route("refresh")]
        public async Task<IActionResult> Refresh(Refresh model)
        {
            var username = User.FindFirstValue(ClaimTypes.Username);
            var companyId = Guid.Parse(User.FindFirstValue(ClaimTypes.CompanyId));

            var user = await _userStore.GetByCompany(companyId, username);

            if (!string.Equals(user.RefreshToken, model.Token))
            {
                // TODO(kevin): what to return here? (token is invalid)
                return BadRequest();
            }

            return await Authorize(user);
        }

        private async Task<IActionResult> Authorize(User user)
        {
            var refreshToken = _tokenFactory.Create();

            user.RefreshToken = refreshToken;

            await _userStore.Update(user);

            return Ok(new
            {
                token = _securityTokenGenerator.Generate(user),
                refreshToken
            });            
        }
    }
}