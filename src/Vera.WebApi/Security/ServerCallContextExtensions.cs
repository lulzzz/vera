using System;
using System.Security.Claims;
using Grpc.Core;

namespace Vera.WebApi.Security
{
    public static class ServerCallContextExtensions
    {
        public static Guid GetCompanyId(this ServerCallContext context)
        {
            return Guid.Parse(context.FindFirstValue(ClaimTypes.CompanyId));
        }

        public static string GetCompanyName(this ServerCallContext context)
        {
            return context.FindFirstValue(ClaimTypes.CompanyName);
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