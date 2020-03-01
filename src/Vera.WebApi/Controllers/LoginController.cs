using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Vera.Security;
using Vera.WebApi.Models;

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

        public LoginController(
            IConfiguration configuration,
            ICompanyStore companyStore,
            IUserStore userStore,
            ITokenFactory tokenFactory,
            IPasswordStrategy passwordStrategy
        )
        {
            _configuration = configuration;
            _companyStore = companyStore;
            _userStore = userStore;
            _tokenFactory = tokenFactory;
            _passwordStrategy = passwordStrategy;
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
            var username = HttpContext.User.FindFirstValue(ClaimTypes.Username);
            var companyId = Guid.Parse(HttpContext.User.FindFirstValue(ClaimTypes.CompanyId));

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
                token = GenerateJwt(user),
                refreshToken
            });            
        }

        private string GenerateJwt(User user)
        {
            var securityKey = new SymmetricSecurityKey(Convert.FromBase64String(_configuration["VERA:JWT:KEY"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]{
                new Claim(ClaimTypes.Id, user.Id.ToString()),
                new Claim(ClaimTypes.CompanyId, user.CompanyId.ToString()),
                new Claim(ClaimTypes.Username, user.Username),
            };

            var token = new JwtSecurityToken(
                _configuration["VERA:JWT:ISSUER"],
                _configuration["VERA:JWT:ISSUER"],
                claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}