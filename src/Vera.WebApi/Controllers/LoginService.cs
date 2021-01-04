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
    public class LoginService : Grpc.LoginService.LoginServiceBase
    {
        private readonly ICompanyStore _companyStore;
        private readonly IUserStore _userStore;
        private readonly ITokenFactory _tokenFactory;
        private readonly IPasswordStrategy _passwordStrategy;
        private readonly ISecurityTokenGenerator _securityTokenGenerator;

        public LoginService(
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

        [AllowAnonymous]
        public override async Task<TokenReply> Login(LoginRequest request, ServerCallContext context)
        {
            var company = await _companyStore.GetByName(request.CompanyName);

            if (company == null)
            {
                throw new RpcException(new Status(StatusCode.Unauthenticated, string.Empty));
            }

            var user = await _userStore.GetByCompany(company.Id, request.Username);

            if (user == null)
            {
                throw new RpcException(new Status(StatusCode.Unauthenticated, string.Empty));
            }

            if (!_passwordStrategy.Verify(request.Password, user.Authentication))
            {
                throw new RpcException(new Status(StatusCode.Unauthenticated, string.Empty));
            }

            return await Authorize(user, company);
        }

        [Authorize]
        public override async Task<TokenReply> Refresh(RefreshRequest request, ServerCallContext context)
        {
            var principal = context.GetHttpContext().User;

            var username = principal.FindFirstValue(Security.ClaimTypes.Username);
            var companyId = Guid.Parse(principal.FindFirstValue(Security.ClaimTypes.CompanyId));

            var user = await _userStore.GetByCompany(companyId, username);

            if (!string.Equals(user.RefreshToken, request.Token))
            {
                // TODO(kevin): what to return here? (token is invalid)
                throw new RpcException(new Status(StatusCode.Unauthenticated, string.Empty));
            }

            var company = await _companyStore.GetByName(principal.FindFirstValue(Security.ClaimTypes.CompanyName));

            return await Authorize(user, company);
        }

        private async Task<TokenReply> Authorize(User user, Company company)
        {
            var refreshToken = _tokenFactory.Create();

            user.RefreshToken = refreshToken;

            await _userStore.Update(user);

            return new TokenReply
            {
                Token = _securityTokenGenerator.Generate(user, company),
                RefreshToken = refreshToken
            };
        }
    }
}