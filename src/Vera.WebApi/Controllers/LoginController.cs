using System;
using System.IdentityModel.Tokens.Jwt;
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
    public class LoginController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ICompanyStore _companyStore;
        private readonly IUserStore _userStore;
        private readonly IPasswordStrategy _passwordStrategy;

        public LoginController(
            IConfiguration configuration,
            ICompanyStore companyStore,
            IUserStore userStore,
            IPasswordStrategy passwordStrategy
        )
        {
            _configuration = configuration;
            _companyStore = companyStore;
            _userStore = userStore;
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

            return Ok(new
            {
                token = GenerateJwt(user)
            });
        }

        private string GenerateJwt(User user)
        {
            // TODO(kevin): https://fullstackmark.com/post/19/jwt-authentication-flow-with-refresh-tokens-in-aspnet-core-web-api

            var securityKey = new SymmetricSecurityKey(Convert.FromBase64String(_configuration["VERA:JWT:KEY"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                _configuration["VERA:JWT:ISSUER"],
                user.Username,
                null,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: credentials
            );

            // TODO(kevin): add claims?

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}