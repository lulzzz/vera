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
using Vera.Grpc.Models;
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
                    $"account with name {existing.Name} already exists"
                ));
            }

            // TODO: limit the certification values to existing ones
            // _accountComponentFactoryCollection.Names

            var accountId = Guid.NewGuid();

            var account = new Vera.Models.Account
            {
                Id = accountId,
                CompanyId = companyId,
                Certification = request.Certification,
                Name = request.Name,
                Address = request.Address.Unpack(),
                Email = request.Email,
                Telephone = request.Telephone,
                RegistrationNumber = request.RegistrationNumber,
                TaxRegistrationNumber = request.TaxRegistrationNumber
            };

            await _accountStore.Store(account);

            return new CreateAccountReply
            {
                Id = accountId.ToString()
            };
        }

        public override async Task<GetAccountReply> Get(GetAccountRequest request, ServerCallContext context)
        {
            var companyId = context.GetCompanyId();
            var account = await _accountStore.Get(companyId, Guid.Parse(request.Id));

            return new GetAccountReply
            {
                Id = account.Id.ToString(),
                Name = account.Name,
                Currency = account.Currency,
                Email = account.Email,
                Telephone = account.Telephone,
                RegistrationNumber = account.RegistrationNumber,
                TaxRegistrationNumber = account.TaxRegistrationNumber,
                Address = account.Address.Pack()
            };
        }

        public override async Task<Empty> Update(UpdateAccountRequest request, ServerCallContext context)
        {
            var companyId = context.GetCompanyId();
            var account = await _accountStore.Get(companyId, Guid.Parse(request.Id));

            if (account == null)
            {
                throw new RpcException(new Status(StatusCode.FailedPrecondition, "account does not exist"));
            }

            // TODO(kevin): more validation on the new state of the account (attributes?)

            if (request.Name != null) account.Name = request.Name;

            // TODO(kevin): check if this is allowed, if so, what should happen when it does?
            if (request.RegistrationNumber != null) account.RegistrationNumber = request.RegistrationNumber;
            if (request.TaxRegistrationNumber != null) account.TaxRegistrationNumber = request.TaxRegistrationNumber;

            if (request.Currency != null) account.Currency = request.Currency;
            if (request.Email != null) account.Email = request.Email;
            if (request.Telephone != null) account.Telephone = request.Telephone;

            if (request.Address != null)
            {
                account.Address = request.Address.Unpack();
            }

            await _accountStore.Update(account);

            return new Empty();
        }

        public override async Task<Empty> CreateOrUpdateConfiguration(AccountConfigurationRequest request, ServerCallContext context)
        {
            var companyId = context.GetCompanyId();
            var accounts = await _accountStore.GetByCompany(companyId);
            var account = accounts.FirstOrDefault(a => a.Id.ToString() == request.Id);

            if (account == null)
            {
                throw new RpcException(new Status(StatusCode.FailedPrecondition, $"account {request.Id} does not exist"));
            }

            var validator = _accountComponentFactoryCollection
                .GetComponentFactory(account)
                .CreateConfigurationValidator();

            var validationResults = validator.Validate(account.Configuration, request.Fields);

            if (validationResults.Any())
            {
                throw new RpcException(new Status(
                    StatusCode.InvalidArgument,
                    string.Join(Environment.NewLine, validationResults.Select(v => v.ErrorMessage))
                ));
            }

            account.Configuration = request.Fields;

            await _accountStore.Update(account);

            return new Empty();
        }

        public override async Task<ListAccountReply> List(Empty request, ServerCallContext context)
        {
            var companyId = context.GetCompanyId();
            var accounts = await _accountStore.GetByCompany(companyId);

            var reply = new ListAccountReply();

            reply.Accounts.AddRange(accounts.Select(x => new ListAccountReply.Types.Account
            {
                Name = x.Name
            }));

            return reply;
        }
    }
}