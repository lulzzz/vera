using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Vera.Dependencies;
using Vera.Models;
using Vera.Stores;
using static Vera.Models.RegisterReport;

namespace Vera.Reports
{
    public interface IRegisterReportGenerator
    {
        public Task<RegisterReport> Generate(RegisterReportContext context);
    }

    public class RegisterReportGenerator : IRegisterReportGenerator
    {
        private readonly IDateProvider _dateProvider;
        private readonly IInvoiceStore _invoiceStore;
        private readonly ISupplierStore _supplierStore;
        private readonly IAccountStore _accountStore;

        public RegisterReportGenerator(
            IDateProvider dateProvider, 
            IInvoiceStore invoiceStore, 
            ISupplierStore supplierStore, 
            IAccountStore accountStore)
        {
            _dateProvider = dateProvider;
            _invoiceStore = invoiceStore;
            _supplierStore = supplierStore;
            _accountStore = accountStore;
        }

        public async Task<RegisterReport> Generate(RegisterReportContext context)
        {
            var account = await _accountStore.Get(context.CompanyId, context.AccountId); 
            if (account == null)
            {
                throw new ValidationException("Failed preconditions, account does not exist");
            }

            var supplier = await _supplierStore.Get(account.Id, context.SupplierSystemId);
            if (supplier == null)
            {
                throw new ValidationException("Failed preconditions, supplier does not exist");
            }

            var today = _dateProvider.Now.Date;
            var invoicesByRegister = await _invoiceStore.List(new Audits.AuditCriteria
            {
                AccountId = account.Id,
                StartDate = today,
                EndDate = today.AddDays(1),
                SupplierSystemId = supplier.SystemId,
                RegisterId = context.RegisterId
            });

            if (invoicesByRegister == null || !invoicesByRegister.Any())
            {
                return new RegisterReport
                {
                    Account = AccountReport.FromAccount(account),
                    SupplierId = supplier.Id,
                    Date = _dateProvider.Now,
                    ReportType = context.ReportType
                };
            }

            var paymentsReport = invoicesByRegister
                .SelectMany(i => i.Payments)
                .Where(p => p.Amount > 0)
                .GroupBy(p => p.Category)
                .Select(g => new PaymentReport
                {
                    PaymentCategory = g.Key,
                    Count = g.Count(),
                    Amount = g.Sum(p => p.Amount)
                });

            var taxesReport = invoicesByRegister
                .SelectMany(i => i.Lines)
                .Where(l => l.Quantity > 0)
                .GroupBy(l => l.Taxes.Rate)
                .Select(g => new TaxesReport
                {
                    TaxRate = g.Key,
                    TaxesCategory = g.First().Taxes.Category,
                    Amount = g.Sum(l => l.Gross - l.Net)
                });

            var productsReport = invoicesByRegister
                .SelectMany(i => i.Lines)
                .GroupBy(l => l.Product.Type)
                .Select(g => new ProductReport
                {
                    Type = g.Key,
                    Count = g.Count(),
                    Amount = g.Sum(l => l.Gross)
                });

            var employeeGrouping = invoicesByRegister
                .GroupBy(i => i.Employee)
                .Where(g => g.Any(i => i.Payments.Any()));

            var employeePaymentsReport = new List<EmployeePaymentsReport>();
            foreach (var group in employeeGrouping)
            {
                var employee = group.Key;
                var paymentTypesPerEmployee = group
                    .SelectMany(i => i.Payments)
                    .Where(p => p.Amount > 0)
                    .GroupBy(p => p.Category)
                    .Select(g => new
                    {
                        PaymentCategory = g.Key,
                        Count = g.Count(),
                        Amount = g.Sum(p => p.Amount)
                    });

                employeePaymentsReport
                    .AddRange(paymentTypesPerEmployee
                        .Select(p => new EmployeePaymentsReport
                        {
                            Employee = employee,
                            Payment = new PaymentReport
                            {
                                PaymentCategory = p.PaymentCategory,
                                Count = p.Count,
                                Amount = p.Amount
                            }
                        }));
            }

            var settlements = invoicesByRegister
                .SelectMany(i => i.Lines)
                .Where(l => l.Settlements != null)
                .SelectMany(l => l.Settlements);

            var settlementsReport = new
            {
                Count = settlements.Count(),
                Amount = settlements.Sum(s => s.Amount)
            };

            var returnInvoices = invoicesByRegister.Where(i => i.Lines.Any(l => l.CreditReference != null));
            var returnsReport = new
            {
                Count = returnInvoices.Count(),
                Amount = returnInvoices.Sum(i => i.Totals.Gross)
            };

            var totalAmountCash = invoicesByRegister
                .Where(i => i.Payments.Any(p => p.Category == PaymentCategory.Cash && p.Amount > 0))
                .Sum(i => i.Totals.Gross);

            var totalAmount = invoicesByRegister.Sum(i => i.Totals.Gross > 0 ? i.Totals.Gross : 0m);

            var totalAmountReturns = returnInvoices.Sum(i => i.Totals.Gross);

            var totalAmountNet = invoicesByRegister.Sum(i => i.Totals.Net);

            return new RegisterReport
            {
                Date = _dateProvider.Now,
                Account = AccountReport.FromAccount(account),
                SupplierId = supplier.Id,
                Payments = paymentsReport.ToList(),
                Taxes = taxesReport.ToList(),
                Products = productsReport.ToList(),
                PaymentsPerEmployee = employeePaymentsReport,
                Discount = new DiscountReport
                {
                    Count = settlementsReport.Count,
                    Amount = settlementsReport.Amount
                },
                Return = new ReturnReport
                {
                    Count = returnsReport.Count,
                    Amount = returnsReport.Amount
                },
                Totals = new TotalsReport
                {
                    Cash = totalAmountCash,
                    Gross = totalAmount,
                    Return = totalAmountReturns,
                    Net = totalAmountNet
                },
                ReportType = context.ReportType
            };
        }
    }

    public class RegisterReportContext
    {
        public Guid AccountId { get; set; }
        public Guid CompanyId { get; set; }
        public string SupplierSystemId { get; set; }
        public string RegisterId { get; set; }
        public ReportType ReportType { get; set; }
    }
}
