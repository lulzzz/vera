using Grpc.Core;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
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
        public static async Task<Account> ResolveAccount(this ServerCallContext context, IAccountStore store)
        {
            var companyId = context.GetCompanyId();
            var accountId = context.GetAccountId();
            var account = await store.Get(companyId, accountId);

            return account ?? throw new RpcException(new Status(StatusCode.Unauthenticated, "unauthenticated"));
        }

        /// <summary>
        /// Attempts to resolve the supplier based on the given parameters.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="store"></param>
        /// <param name="supplierSystemId"></param>
        /// <returns></returns>
        /// <exception cref="RpcException">when the account cannot be resolved</exception>
        public static async Task<Supplier> ResolveSupplier(this ServerCallContext context, ISupplierStore store, string supplierSystemId)
        {
            var supplier = await store.Get(context.GetAccountId(), supplierSystemId);
            
            return supplier ?? throw new RpcException(new Status(StatusCode.NotFound, "supplier does not exist"));
        }

        public static Guid GetCompanyId(this ServerCallContext context)
        {
            return Guid.Parse(context.FindFirstValue(ClaimTypes.CompanyId));
        }

        public static Guid GetAccountId(this ServerCallContext context)
        {
            var accountId = context.RequestHeaders.GetValue(MetadataKeys.AccountId);

            return accountId != null 
                ? Guid.Parse(accountId) 
                : throw new RpcException(new Status(StatusCode.Unauthenticated, "unauthenticated"));
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