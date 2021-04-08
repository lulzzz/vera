using System;
using Vera.Models;

namespace Vera.Grpc.Models
{
    public static class EnumExtensions
    {
        public static Shared.TaxCategory Map(this TaxesCategory category) =>
            category switch
            {
                TaxesCategory.High => Shared.TaxCategory.High,
                TaxesCategory.Low => Shared.TaxCategory.Low,
                TaxesCategory.Zero => Shared.TaxCategory.Zero,
                TaxesCategory.Exempt => Shared.TaxCategory.Exempt,
                TaxesCategory.Intermediate => Shared.TaxCategory.Intermediate,
                _ => throw new ArgumentOutOfRangeException()
            };

        public static Shared.PaymentCategory Map(this PaymentCategory category) =>
            category switch
            {
                PaymentCategory.Other => Shared.PaymentCategory.Other,
                PaymentCategory.Debit => Shared.PaymentCategory.Debit,
                PaymentCategory.Credit => Shared.PaymentCategory.Credit,
                PaymentCategory.Cash => Shared.PaymentCategory.Cash,
                PaymentCategory.Change => Shared.PaymentCategory.Change,
                PaymentCategory.Voucher => Shared.PaymentCategory.Voucher,
                PaymentCategory.Online => Shared.PaymentCategory.Online,
                _ => throw new ArgumentOutOfRangeException()
            };

        public static ProductType.ProductType Map(this Vera.Models.ProductType category) =>
            category switch
            {
                Vera.Models.ProductType.GiftCard => ProductType.ProductType.GiftCard,
                Vera.Models.ProductType.Goods => ProductType.ProductType.Goods,
                Vera.Models.ProductType.Other => ProductType.ProductType.Other,
                _ => throw new ArgumentOutOfRangeException()
            };
    }
}
