using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Vera.Grpc;
using Vera.Models;
using Vera.Security;
using Vera.Stores;
using Vera.WebApi.Security;

namespace Vera.WebApi.Controllers
{
    public class TokenService : Grpc.TokenService.TokenServiceBase
    {
        private readonly IUserStore _userStore;
        private readonly ICompanyStore _companyStore;
        private readonly ITokenFactory _tokenFactory;
        private readonly IPasswordStrategy _passwordStrategy;
        private readonly ISecurityTokenGenerator _securityTokenGenerator;

        public TokenService(
            IUserStore userStore,
            ICompanyStore companyStore,
            ITokenFactory tokenFactory,
            IPasswordStrategy passwordStrategy,
            ISecurityTokenGenerator securityTokenGenerator
        )
        {
            _userStore = userStore;
            _companyStore = companyStore;
            _tokenFactory = tokenFactory;
            _passwordStrategy = passwordStrategy;
            _securityTokenGenerator = securityTokenGenerator;
        }

        [Authorize]
        public override async Task<TokenReply> Generate(TokenRequest request, ServerCallContext context)
        {
            var principal = context.GetHttpContext().User;

            var companyId = Guid.Parse(principal.FindFirstValue(Security.ClaimTypes.CompanyId));
            var existingUser = await _userStore.GetByCompany(companyId, request.Username);

            if (existingUser != null)
            {
                throw new RpcException(new Status(StatusCode.AlreadyExists, $"User with username {request.Username} already exists"));
            }

            var company = await _companyStore.GetByName(principal.FindFirstValue(Security.ClaimTypes.CompanyName));

            var auth = _passwordStrategy.Encrypt(_tokenFactory.Create());

            var user = new User
            {
                Id = Guid.NewGuid(),
                CompanyId = companyId,
                Type = UserType.Robot,
                Username = request.Username,
                Authentication = auth,
                RefreshToken = _tokenFactory.Create()
            };

            await _userStore.Store(user);

            var token = _securityTokenGenerator.Generate(user, company);

            return new TokenReply
            {
                Token = token,
                RefreshToken = user.RefreshToken
            };
        }
    }
}