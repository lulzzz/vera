using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Vera.Grpc;
using Vera.Grpc.Shared;
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

        public override async Task<CreateAccountReply> Create(CreateAccountRequest request, ServerCallContext context)
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

            var accountId = Guid.NewGuid();

            accounts.Add(new Vera.Models.Account
            {
                Id = accountId,
                Certification = request.Certification,
                Name = request.Name
            });

            company.Accounts = accounts;

            await _companyStore.Update(company);

            return new CreateAccountReply
            {
                Id = accountId.ToString()
            };
        }

        public override async Task<Empty> CreateOrUpdateConfiguration(AccountConfigurationRequest request, ServerCallContext context)
        {
            var principal = context.GetHttpContext().User;
            var company = await _companyStore.GetByName(principal.FindFirstValue(Security.ClaimTypes.CompanyName));
            var account = company.Accounts.FirstOrDefault(a => a.Id.ToString() == request.Id);

            if (account == null)
            {
                throw new RpcException(new Status(StatusCode.FailedPrecondition, $"Account {request.Id} does not exist"));
            }

            // TODO: validate the configuration

            account.Configuration = request.Fields;

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