using System;
using Vera.Models;

namespace Vera.Host.Mapping
{
    public static class EnumExtensions
    {
        public static Grpc.Shared.TaxCategory Map(this TaxesCategory category) =>
            category switch
            {
                TaxesCategory.High => Grpc.Shared.TaxCategory.High,
                TaxesCategory.Low => Grpc.Shared.TaxCategory.Low,
                TaxesCategory.Zero => Grpc.Shared.TaxCategory.Zero,
                TaxesCategory.Exempt => Grpc.Shared.TaxCategory.Exempt,
                TaxesCategory.Intermediate => Grpc.Shared.TaxCategory.Intermediate,
                _ => throw new ArgumentOutOfRangeException()
            };

        public static Grpc.Shared.PaymentCategory Map(this PaymentCategory category) =>
            category switch
            {
                PaymentCategory.Other => Grpc.Shared.PaymentCategory.Other,
                PaymentCategory.Debit => Grpc.Shared.PaymentCategory.Debit,
                PaymentCategory.Credit => Grpc.Shared.PaymentCategory.Credit,
                PaymentCategory.Cash => Grpc.Shared.PaymentCategory.Cash,
                PaymentCategory.Change => Grpc.Shared.PaymentCategory.Change,
                PaymentCategory.Voucher => Grpc.Shared.PaymentCategory.Voucher,
                PaymentCategory.Online => Grpc.Shared.PaymentCategory.Online,
                _ => throw new ArgumentOutOfRangeException()
            };

        public static Grpc.ProductType.ProductType Map(this Vera.Models.ProductType category) =>
            category switch
            {
                Vera.Models.ProductType.GiftCard => Grpc.ProductType.ProductType.GiftCard,
                Vera.Models.ProductType.Goods => Grpc.ProductType.ProductType.Goods,
                Vera.Models.ProductType.Other => Grpc.ProductType.ProductType.Other,
                _ => throw new ArgumentOutOfRangeException()
            };
    }
}
