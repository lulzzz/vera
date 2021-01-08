using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Vera.Bootstrap;
using Vera.Grpc;
using Vera.Grpc.Shared;
using Vera.Stores;
using Vera.WebApi.Security;

namespace Vera.WebApi.Services
{
    [Authorize]
    public class AccountService : Grpc.AccountService.AccountServiceBase
    {
        private readonly IAccountStore _accountStore;
        private readonly IAccountComponentFactoryCollection _accountComponentFactoryCollection;

        public AccountService(
            IAccountStore accountStore,
            IAccountComponentFactoryCollection accountComponentFactoryCollection
        )
        {
            _accountStore = accountStore;
            _accountComponentFactoryCollection = accountComponentFactoryCollection;
        }

        public override async Task<CreateAccountReply> Create(CreateAccountRequest request, ServerCallContext context)
        {
            var companyId = context.GetCompanyId();
            var accounts = await _accountStore.GetByCompany(companyId);

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

            // TODO(kevin): address details etc.
            var account = new Vera.Models.Account
            {
                Id = accountId,
                CompanyId = companyId,
                Certification = request.Certification,
                Name = request.Name
            };

            await _accountStore.Store(account);

            return new CreateAccountReply
            {
                Id = accountId.ToString()
            };
        }

        // TODO(kevin): can merge with update all of account
        public override async Task<Empty> CreateOrUpdateConfiguration(AccountConfigurationRequest request, ServerCallContext context)
        {
            var companyId = Guid.Parse(context.GetHttpContext().User.FindFirstValue(Security.ClaimTypes.CompanyId));
            var accounts = await _accountStore.GetByCompany(companyId);
            var account = accounts.FirstOrDefault(a => a.Id.ToString() == request.Id);

            if (account == null)
            {
                throw new RpcException(new Status(StatusCode.FailedPrecondition, $"Account {request.Id} does not exist"));
            }

            // TODO(kevin): this looks like a very weird pattern and feels like chicken <-> egg
            // maybe extract it to a separate component?
            var configuration = _accountComponentFactoryCollection
                .GetOrThrow(account)
                .CreateConfiguration();

            configuration.Initialize(request.Fields);

            var validationContext = new ValidationContext(configuration);

            if (!Validator.TryValidateObject(configuration, validationContext, new List<ValidationResult>()))
            {
                // TODO(kevin): throw matching exception
                throw new RpcException(new Status(StatusCode.FailedPrecondition,
                    "One or more fields did not pass validation"));
            }

            // TODO(kevin): do more extensive testing of the configuration - like checking that the public/private keys works

            account.Configuration = request.Fields;

            await _accountStore.Update(account);

            return new Empty();
        }

        public override async Task<ListAccountReply> List(Empty request, ServerCallContext context)
        {
            var companyId = Guid.Parse(context.GetHttpContext().User.FindFirstValue(Security.ClaimTypes.CompanyId));
            var accounts = await _accountStore.GetByCompany(companyId);

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