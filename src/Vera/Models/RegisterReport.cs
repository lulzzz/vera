using System;
using System.Collections.Generic;

namespace Vera.Models
{
    public class RegisterReport
    {
        //TODO add number of copies (skipped for now)
        public RegisterReport()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }

        public string RegisterId { get; set; }

        public DateTime Date { get; set; }
        public Guid SupplierId { get; set; }
        public AccountReport Account { get; set; }
        public ICollection<PaymentReport> Payments { get; set; }
        public ICollection<TaxesReport> Taxes { get; set; }
        public ICollection<ProductReport> Products { get; set; }
        public ICollection<EmployeePaymentsReport> PaymentsPerEmployee { get; set; }
        public DiscountReport Discount { get; set; }
        public ReturnReport Return { get; set; }
        public TotalsReport Totals { get; set; }

        /// <summary>
        // Sequence number of the report
        /// </summary>
        public int Sequence { get; set; }

        /// <summary>
        /// Report number that has been assigned to the report
        /// </summary>
        public string Number { get; set; }
        public Signature Signature { get; set; }
        public ReportType ReportType { get; set; }

        public class AccountReport
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public string TaxRegistrationNumber { get; set; }

            public static AccountReport FromAccount(Account account) => new AccountReport
                    {
                        Id = account.Id,
                        Name = account.Name,
                        TaxRegistrationNumber = account.TaxRegistrationNumber
                    };
        }

        public class TaxesReport
        {
            public decimal TaxRate { get; set; }
            public TaxesCategory TaxesCategory { get; set; }
            public decimal Amount { get; set; }
        }

        public class ProductReport
        {
            public ProductType Type { get; set; }
            public int Count { get; set; }
            public decimal Amount { get; set; }
        }

        public class EmployeePaymentsReport
        {
            public Employee Employee { get; set; }
            public PaymentReport Payment { get; set; }
        }

        public class PaymentReport
        {
            public PaymentCategory PaymentCategory { get; set; }
            public int Count { get; set; }
            public decimal Amount { get; set; }
        }

        public class DiscountReport
        {
            public int Count { get; set; }
            public decimal Amount { get; set; }
        }

        public class ReturnReport
        {
            public int Count { get; set; }
            public decimal Amount { get; set; }
        }

        public class TotalsReport
        {
            public decimal Cash { get; set; }
            public decimal Gross { get; set; }
            public decimal Return { get; set; }
            public decimal Net { get; set; }
        }
    }
}
