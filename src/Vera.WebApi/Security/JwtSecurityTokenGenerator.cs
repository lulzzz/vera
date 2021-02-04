using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Vera.Models;

namespace Vera.WebApi.Security
{
    public class JwtSecurityTokenGenerator : ISecurityTokenGenerator
    {
        private readonly IConfiguration _configuration;

        public JwtSecurityTokenGenerator(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string Generate(User user, Company company)
        {
            var securityKey = new SymmetricSecurityKey(Convert.FromBase64String(_configuration["VERA:JWT:KEY"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]{
                new Claim(ClaimTypes.Id, user.Id.ToString()),
                new Claim(ClaimTypes.Username, user.Username),
                new Claim(ClaimTypes.CompanyId, company.Id.ToString())
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