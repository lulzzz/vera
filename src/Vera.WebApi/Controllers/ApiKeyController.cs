using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vera.Security;
using Vera.WebApi.Models;
using Vera.WebApi.Security;

namespace Vera.WebApi.Controllers
{
    [ApiController]
    [Route("token")]
    [Authorize]
    public class ApiKeyController : ControllerBase
    {
        private readonly IUserStore _userStore;
        private readonly ITokenFactory _tokenFactory;
        private readonly IPasswordStrategy _passwordStrategy;
        private readonly ISecurityTokenGenerator _securityTokenGenerator;

        public ApiKeyController(
            IUserStore userStore, 
            ITokenFactory tokenFactory, 
            IPasswordStrategy passwordStrategy,
            ISecurityTokenGenerator securityTokenGenerator
        )
        {
            _userStore = userStore;
            _tokenFactory = tokenFactory;
            _passwordStrategy = passwordStrategy;
            _securityTokenGenerator = securityTokenGenerator;
        }

        public async Task<IActionResult> Index(RobotUser model)
        {
            var companyId = Guid.Parse(User.FindFirstValue(ClaimTypes.CompanyId));

            var existingUser = await _userStore.GetByCompany(companyId, model.Username);

            if (existingUser != null)
            {
                // TODO(kevin): detailed error that user already exists
                return BadRequest();
            }

            var auth = _passwordStrategy.Encrypt(_tokenFactory.Create());

            var user = new User
            {
                Id = Guid.NewGuid(),
                CompanyId = companyId,
                Type = UserType.Robot,
                Username = model.Username,
                AccountId = model.AccountId,
                Authentication = auth,
                RefreshToken = _tokenFactory.Create()
            };

            await _userStore.Store(user);

            var token = _securityTokenGenerator.Generate(user);

            return Ok(new
            {
                token,
                refreshToken = user.RefreshToken
            });
        }       
    }
}