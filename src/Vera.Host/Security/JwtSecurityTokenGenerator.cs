using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Vera.Models;

namespace Vera.Host.Security
{
    public class JwtSecurityTokenGenerator : ISecurityTokenGenerator
    {
        private readonly string _issuer;
        private readonly string _audience;
        private readonly SigningCredentials _credentials;

        public JwtSecurityTokenGenerator(string issuer, string audience, SecurityKey key)
        {
            _issuer = issuer;
            _audience = audience;
            _credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        }

        public string Generate(User user, Company company)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.UserId, user.Id.ToString()),
                new Claim(ClaimTypes.Username, user.Username),
                new Claim(ClaimTypes.CompanyId, company.Id.ToString())
            };

            var token = new JwtSecurityToken(
                _issuer,
                _audience,
                claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: _credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}