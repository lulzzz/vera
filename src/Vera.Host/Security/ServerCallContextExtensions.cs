using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Grpc.Core;
using Vera.Models;
using Vera.Stores;

namespace Vera.Host.Security
{
    public static class ServerCallContextExtensions
    {
        /// <summary>
        /// Attempts to resolve the account based on the given parameters. The company is read from the context and the
        /// account will be resolved in combination with company and the given accountId from the incoming request. If
        /// this does not resolve to a non-null account, this will throw.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="store"></param>
        /// <param name="accountId"></param>
        /// <returns></returns>
        /// <exception cref="RpcException">when the account cannot be resolved</exception>
        public static async Task<Account> ResolveAccount(this ServerCallContext context, IAccountStore store, string accountId)
        {
            var companyId = context.GetCompanyId();
            var account = await store.Get(companyId, Guid.Parse(accountId));

            return account ?? throw new RpcException(new Status(StatusCode.Unauthenticated, "unauthenticated"));
        }

        public static Guid GetCompanyId(this ServerCallContext context)
        {
            return Guid.Parse(context.FindFirstValue(ClaimTypes.CompanyId));
        }

        public static string GetUsername(this ServerCallContext context)
        {
            return context.FindFirstValue(ClaimTypes.Username);
        }

        private static string FindFirstValue(this ServerCallContext context, string claimType)
        {
            return context.GetHttpContext().User.FindFirstValue(claimType);
        }
    }
}