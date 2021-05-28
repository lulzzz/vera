using System;
using System.Linq;
using Vera.Grpc;
using Vera.Grpc.Shared;
using Vera.Models;

namespace Vera.Host.Mapping
{
    public static class AccountExtensions
    {
        public static Account Unpack(this CreateAccountRequest request, Guid companyId)
        {
            var accountId = Guid.NewGuid();
            var account = new Account
            {
                Id = accountId,
                CompanyId = companyId,
                Certification = request.Certification,
                Name = request.Name,
                Address = request.Address.Unpack(),
                Email = request.Email,
                Telephone = request.Telephone,
                RegistrationNumber = request.RegistrationNumber,
                TaxRegistrationNumber = request.TaxRegistrationNumber,
                TaxRates = request.TaxRates.Select(taxRate => new Models.TaxRate
                {
                    Category = taxRate.Category.Unpack(),
                    Code = taxRate.Code,
                    Rate = taxRate.Rate
                }).ToList()
            };


            return account;
        }

        public static GetAccountReply Pack(this Account account)
        {
            return new GetAccountReply
            {
                Id = account.Id.ToString(),
                Name = account.Name,
                Currency = account.Currency,
                Email = account.Email,
                Telephone = account.Telephone,
                RegistrationNumber = account.RegistrationNumber,
                TaxRegistrationNumber = account.TaxRegistrationNumber,
                Address = account.Address.Pack(),
                TaxRates =
                {
                    account.TaxRates.Select(x => new Grpc.TaxRate
                    {
                        Category = x.Category.Pack(),
                        Code = x.Code,
                        Rate = x.Rate
                    })
                }
            };
        }

        private static TaxCategory Pack(this TaxesCategory taxCategory) => taxCategory switch
        {
            TaxesCategory.Exempt => TaxCategory.Exempt,
            TaxesCategory.High => TaxCategory.High,
            TaxesCategory.Intermediate => TaxCategory.Intermediate,
            TaxesCategory.Low => TaxCategory.Low,
            TaxesCategory.Zero => TaxCategory.Zero,
            _ => throw new ArgumentOutOfRangeException()
        };

        private static TaxesCategory Unpack(this TaxCategory taxCategory) => taxCategory switch
        {
            TaxCategory.Exempt => TaxesCategory.Exempt,
            TaxCategory.High => TaxesCategory.High,
            TaxCategory.Intermediate => TaxesCategory.Intermediate,
            TaxCategory.Low => TaxesCategory.Low,
            TaxCategory.Zero => TaxesCategory.Zero,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}