using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Vera.Grpc;
using Vera.Stores;

namespace Vera.WebApi.Controllers
{
    [Authorize]
    public class AccountService : Grpc.AccountService.AccountServiceBase
    {
        private readonly ICompanyStore _companyStore;

        public AccountService(ICompanyStore companyStore)
        {
            _companyStore = companyStore;
        }

        public override async Task<Empty> Create(CreateAccountRequest request, ServerCallContext context)
        {
            var companyName = context.GetHttpContext().User.FindFirstValue(Security.ClaimTypes.CompanyName);
            var company = await _companyStore.GetByName(companyName);

            var accounts = company.Accounts ?? new List<Vera.Models.Account>();

            var existing =
                accounts.FirstOrDefault(a => a.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase));

            if (existing != null)
            {
                throw new RpcException(new Status(
                    StatusCode.InvalidArgument,
                    $"Account with name {existing.Name} already exists"
                ));
            }

            // TODO: limit the certification values to existing ones

            accounts.Add(new Vera.Models.Account
            {
                Id = Guid.NewGuid(),
                Certification = request.Certification,
                Name = request.Name
            });

            company.Accounts = accounts;

            await _companyStore.Update(company);

            return new Empty();
        }

        public override async Task<ListAccountReply> List(Empty request, ServerCallContext context)
        {
            var companyName = context.GetHttpContext().User.FindFirstValue(Security.ClaimTypes.CompanyName);
            var company = await _companyStore.GetByName(companyName);

            var accounts = company.Accounts ?? new List<Vera.Models.Account>();

            var reply = new ListAccountReply();

            reply.Accounts.AddRange(accounts.Select(x => new Account
            {
                Name = x.Name,
                Certification = x.Certification
            }));

            return reply;
        }
    }
}