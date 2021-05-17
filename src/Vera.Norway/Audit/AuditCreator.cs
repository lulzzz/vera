using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vera.Audits;
using Vera.Dependencies;
using Vera.Extensions;
using Vera.Models;
using Vera.Stores;

namespace Vera.Norway.Audit
{
    public class AuditCreator
    {
        private const string TicketLineBasicIDSale = "SALE";
        private const string TicketLineBasicIDReturn = "RETURN";
        private const string ArticleGroupID = "OTHER";

        private readonly IReportStore _reportStore;
        private readonly IDateProvider _dateProvider;

        private readonly ICollection<AuditfileCompanyBasicsBasic> _transactionBasics;
        private readonly Auditfile _auditfile;

        public AuditCreator(IReportStore reportStore)
        {
            _dateProvider = new RealLifeDateProvider();
            _transactionBasics = new List<AuditfileCompanyBasicsBasic>();
            _auditfile = new Auditfile();
            _reportStore = reportStore;
        }

        public async Task<Auditfile> CreateAsync(AuditContext context, AuditCriteria criteria)
        {
            CreateHeader(context, criteria);
            await CreateCompanyAsync(context, criteria);

            return _auditfile;
        }

        private void CreateHeader(AuditContext context, AuditCriteria criteria)
        {
            var date = _dateProvider.Now;
            _auditfile.Header = new AuditfileHeader
            {
                CurCode = Currencycode.NOK,
                AuditfileVersion = "1.0",
                DateCreated = date,
                TimeCreated = FormatTime(date),
                FiscalYear = criteria.StartDate.Year.ToString(),
                StartDate = criteria.StartDate,
                EndDate = criteria.EndDate,
                SoftwareDesc = context.SoftwareName,
                SoftwareVersion = context.SoftwareVersion,
                SoftwareCompanyName = context.SoftwareCompanyName
            };
        }

        private async Task CreateCompanyAsync(AuditContext context, AuditCriteria criteria)
        {
            var companyAddress = context.Account.Address;

            var company = new AuditfileCompany
            {
                CompanyIdent = context.Account.RegistrationNumber,
                CompanyName = context.Account.Name,
                TaxRegIdent = context.Account.TaxRegistrationNumber,
                TaxRegistrationCountry = Countrycode.NO,
                StreetAddress =
                {
                  new AuditfileCompanyStreetAddress
                  {
                    City = companyAddress.City,
                    Country = Countrycode.NO,
                    Region = companyAddress.Region,
                    Streetname = companyAddress.Street,
                    PostalCode = companyAddress.PostalCode,
                    Number = companyAddress.Number
                  }
                },
                PostalAddress =
                {
                  new AuditfileCompanyPostalAddress
                  {
                    City = companyAddress.City,
                    Country = Countrycode.NO,
                    Region = companyAddress.Region,
                    Streetname = companyAddress.Street,
                    PostalCode = companyAddress.PostalCode,
                    Number = companyAddress.Number
                  }
                }
            };

            company.CustomersSuppliers.AddRange(
                context.Invoices
                    .Where(i => i.Customer != null && i.Customer.SystemId != null)
                    .Select(i => i.Customer)
                    .Distinct(c => c.SystemId)
                    .Select(c => 
                    {
                        var customer = new AuditfileCompanyCustomersSuppliersCustomerSupplier
                        {
                            CustSupID = c.SystemId,
                            CustSupName = $"{c.FirstName} {c.LastName}",
                            CustSupType = Custsuptype.Customer,
                            Contact = $"{c.FirstName} {c.LastName}",
                        };

                        if (!string.IsNullOrEmpty(c.TaxRegistrationNumber))
                        {
                            customer.TaxRegIdent = c.TaxRegistrationNumber;
                            customer.TaxRegistrationCountry = Countrycode.NO;
                            customer.TaxRegistrationCountrySpecified = true;
                        }

                        return customer;
                    }));

            company.Employees.AddRange(
                context.Invoices
                    .Where(e => e?.SystemId != null)
                    .Select(i => i.Employee)
                    .Distinct(e => e.SystemId)
                    .Select(e => new AuditfileCompanyEmployeesEmployee
                    {
                        FirstName = e.FirstName,
                        SurName = e.LastName,
                        EmpID = e.SystemId
                    }));

            company.Articles.AddRange(
                context.Invoices.SelectMany(i => i.Lines)
                                .Select(l => l.Product)
                                .Distinct(p => p.Code)
                                .Select(p => new AuditfileCompanyArticlesArticle
            {
                ArtGroupID = BasicsMapper.FormatPredefinedBasicID(PredefinedProductBasics.Other),
                ArtID = p.SystemId,
                ArtDesc = p.Description
            }));


            var startPeriod = criteria.StartDate;
            var endPeriod = criteria.EndDate;

            for (var month = startPeriod.Month; month <= endPeriod.Month; month++)
            {
                var start = new DateTime(startPeriod.Year, month, 1);
                var end = new DateTime(startPeriod.Year, month, DateTime.DaysInMonth(startPeriod.Year, month));

                // Period has a restriction, it it's limited from 0-999 (inclusive) so we're unable to use the actual period
                // which would be the reference to the financial period. That's why we chose to use the number of the month for the
                // period instead.
                company.Periods.Add(new AuditfileCompanyPeriodsPeriod
                {
                    PeriodNumber = month.ToString(),
                    PeriodDesc = $"{start:yyyy-MM-dd} - {end:yyyy-MM-dd}",
                    StartDatePeriod = FormatTime(start),
                    StartTimePeriod = FormatTime(start),
                    EndDatePeriod = FormatTime(end),
                    EndTimePeriod = FormatTime(end),
                    StartDatePeriodSpecified = true,
                    StartTimePeriodSpecified = true,
                    EndDatePeriodSpecified = true,
                    EndTimePeriodSpecified = true
                });
            }

            company.VatCodeDetails.AddRange(
                context.Invoices.SelectMany(i => i.Lines)
                                .Select(l => l.Taxes)
                                .Distinct(t => t.Code)
                                .Select(t => new AuditfileCompanyVatCodeDetailsVatCodeDetail
                                {
                                    VatCode = t.Code,
                                    DateOfEntry = DateTime.Now.AddDays(-1),
                                    StandardVatCode = GetTaxCode(t.Category)
                                }));

            // Physical locations (the shops)
            var invoicesBySupplier = context.Invoices
              .Where(i => i.Supplier.SystemId == criteria.SupplierSystemId).ToList();

            var location = await CreateLocationAsync(context, invoicesBySupplier);

            if (location != null)
            {
                company.Location.Add(location);
            }

            //continue HEREs
            company.Basics.AddRange(CreateBasics(context));
            company.Basics.AddRange(_transactionBasics);

            _auditfile.Company = company;
        }


        private IEnumerable<AuditfileCompanyBasicsBasic> CreateBasics(AuditContext context)
        {
            foreach (var e in context.Events.Distinct(e => e.Type))
            {
                yield return BasicsMapper.FromEvent(e);
            }

            yield return new AuditfileCompanyBasicsBasic
            {
                BasicType = BasicsMapper.FormatBasicType(BasicTypes.ArticleGroup),
                PredefinedBasicID = BasicsMapper.FormatPredefinedBasicID(PredefinedProductBasics.Other),
                BasicID = ArticleGroupID,
                BasicDesc = "Other article group"
            };

            var payments = context.Invoices
                .SelectMany(i => i.Payments)
                .Distinct(p => p.Category);

            foreach (var p in payments)
            {
                yield return BasicsMapper.FromPayment(p);
            }

            var discounts = context.Invoices
              .SelectMany(i => i.Settlements)
              .Concat(context.Invoices
                  .SelectMany(i => i.Lines)
                  .SelectMany(l => l.Settlements))
                  .Distinct(s => s.SystemId);

            foreach (var discount in discounts)
            {
                yield return BasicsMapper.FromDiscount(discount);
            }

            // TODO (kevin) can make this more complex in the future, this should be sufficient for the time being
            yield return new AuditfileCompanyBasicsBasic
            {
                BasicType = BasicsMapper.FormatBasicType(BasicTypes.TicketLine),
                BasicID = TicketLineBasicIDSale,
                BasicDesc = "Sales"
            };

            yield return new AuditfileCompanyBasicsBasic
            {
                BasicType = BasicsMapper.FormatBasicType(BasicTypes.TicketLine),
                BasicID = TicketLineBasicIDReturn,
                BasicDesc = "Returns"
            };
        }

        private async Task<AuditfileCompanyLocationCashregister> CreateCashRegisterAsync(
          AuditContext context,
          string registerId,
          IEnumerable<Invoice> invoices,
          IEnumerable<EventLog> events)
        {
            var register = new AuditfileCompanyLocationCashregister
            {
                RegisterID = registerId
            };

            foreach (var e in events)
                {
                    register.Event.Add(await CreateEventAsync(context, e));
                }

            foreach (var invoice in invoices)
            {
                register.Cashtransaction.Add(CreateTransaction(context.Account.Currency, invoice, invoice.Payments));
            }

            return register;
        }

        private AuditfileCompanyLocationCashregisterCashtransaction CreateTransaction(string currencyID, Invoice invoice, IEnumerable<Payment> payments)
        {
            var basic = BasicsMapper.FromInvoice(invoice, payments);

            var transaction = new AuditfileCompanyLocationCashregisterCashtransaction
            {
                // nr == transID (allowed by docs)
                Nr = invoice.Number,
                TransID = invoice.Number,
                TransType = basic.BasicID,

                TransAmntIn = invoice.Totals.Gross.RoundAwayFromZero().Absolute(),
                TransAmntEx = invoice.Totals.Net.RoundAwayFromZero().Absolute(),

                AmntTp = AmountToCreditType(invoice.Totals.Net),

                PeriodNumber = invoice.Date.Month.ToString(),
                TransDate = invoice.Date,
                TransTime = FormatTime(invoice.Date),

                EmpID = invoice.Employee.SystemId,
                CustSupID = invoice.Customer?.SystemId,

                // TODO we need to add print count
                //ReceiptNum = invoice.PrintCount.ToString(),
                //ReceiptCopyNum = invoice.PrintCount.ToString(),

                // The digital signature based on RSA-SHA1-1024
                Signature = invoice.Signature.Output.ToString(),

                // The version of the private/secret key.
                KeyVersion = invoice.Signature.Version.ToString()
            };

            if (invoice.Settlements != null)
            {
                transaction.Discount.AddRange(invoice.Settlements.Select(s => new AuditfileCompanyLocationCashregisterCashtransactionDiscount
                {
                    DscAmnt = s.Amount.RoundAwayFromZero().Absolute(),
                    DscTp = s.SystemId
                }));
            }

            foreach (var p in payments)
            {
                transaction.Payment.Add(CreatePayment(p));
            }

            if (_transactionBasics.All(b => b.BasicID != basic.BasicID))
            {
                // Add to the basics map if it does not exist yet
                _transactionBasics.Add(basic);
            }

            transaction.CtLine.AddRange(invoice.Lines.Select((l, i) => CreateTransactionLine(currencyID, l, i + 1, transaction)));

            return transaction;
        }

        private AuditfileCompanyLocationCashregisterCashtransactionPayment CreatePayment(Payment p)
        {
            return new AuditfileCompanyLocationCashregisterCashtransactionPayment
            {
                CurCode = Currencycode.NOK,
                PaidAmnt = Math.Abs(p.Amount),
                PaymentType = BasicsMapper.FromPayment(p).BasicID,
                PaymentRefID = p.SystemId
            };
        }

        private AuditfileCompanyLocationCashregisterCashtransactionCtLine CreateTransactionLine(string currencyID, InvoiceLine line, int sequence, AuditfileCompanyLocationCashregisterCashtransaction transaction)
        {
            // The ctLine element contains details data of a transaction. If, for instance, the transaction describes a sales slip, the ctLines are the slip lines.
            // The data not described in the cash transaction element, are described in the ctLine.

            var vat = line.Taxes;

            var ctLine = new AuditfileCompanyLocationCashregisterCashtransactionCtLine
            {
                // Transaction number, also used as reference to the cashtransaction - nr - element.
                // This must be a unique, sequential number within a journal. This will be the same as the number stated on the issued receipt.
                Nr = transaction.Nr,

                // Transaction line number. Should be a unique sequential number within a transaction.
                LineID = sequence.ToString(),

                // Reference to ticketline codes. Description of the code must be stated in the table 'Basics'.
                // Correction, returns, booking, deposit, gift certificate, etc.
                LineType = line.Quantity > 0 ? TicketLineBasicIDSale : TicketLineBasicIDReturn,

                ArtID = line.Product.Code,
                ArtGroupID = ArticleGroupID,

                Qnt = line.Quantity,

                LineAmntIn = line.Gross.RoundAwayFromZero().Absolute(),
                LineAmntEx = line.Net.RoundAwayFromZero().Absolute(),

                AmntTp = AmountToCreditType(line.Net),

                LineDate = transaction.TransDate,
                LineTime = FormatTime(transaction.TransDate),

                Vat = new AuditfileCompanyLocationCashregisterCashtransactionCtLineVat
                {
                    VatCode = vat.Code,
                    VatPerc = (vat.Rate * 100 - 100),
                    VatAmnt = (line.Net / vat.Rate).RoundAwayFromZero().Absolute(),
                    VatBasAmnt = (line.UnitPrice / vat.Rate).RoundAwayFromZero().Absolute(),
                    VatAmntTp = AmountToCreditType((line.Net / vat.Rate))
                }
            };

            if (line.Settlements != null)
            {
                ctLine.Discount.AddRange(line.Settlements.Select(s => new AuditfileCompanyLocationCashregisterCashtransactionCtLineDiscount
                {
                    DscAmnt = s.Amount.RoundAwayFromZero(),
                    DscTp = s.SystemId // ref to the basics table
                }));
            }

            return ctLine;
        }

        private async Task<AuditfileCompanyLocationCashregisterEvent> CreateEventAsync(AuditContext context, EventLog e)
        {
            // The event element stores activity not represented as a cash register transaction (cashtransaction) or transaction line (ctline).
            var cashRegisterEvent = new AuditfileCompanyLocationCashregisterEvent
            {
                // Unique system specific identification of this event.
                // Replacing to limit to 35 characters
                EventID = e.Id.ToString().Replace("-", ""),

                // Reference to the type of event. Description of the system specific code must be stated in the table 'Basics' (basicType 13)
                EventType = BasicsMapper.TranslateEventToBasicID(e.Type).ToString(),

                EventDate = e.Date,
                EventTime = FormatTime(e.Date),

                EventText = e.Type.ToString(),

                // Reference to the employee. Employee must be included in the table "employees".
                EmpID = e.EmployeeId,
            };

            if (e.Type == EventLogType.XReport || e.Type == EventLogType.ZReport)
            {
                var report = await _reportStore.GetByNumber(context.Account.Id, e.ReportNumber);

                cashRegisterEvent.EventReport = new AuditfileCompanyLocationCashregisterEventEventReport
                {
                    ReportID = report.Number,
                    ReportType = e.Type == EventLogType.XReport
                    ? AuditfileCompanyLocationCashregisterEventEventReportReportType.X_Report
                    : AuditfileCompanyLocationCashregisterEventEventReportReportType.Z_Report,
                    CompanyIdent = context.Account.RegistrationNumber,
                    CompanyName = context.Account.Name,
                    ReportDate = report.Date,
                    ReportTime = FormatTime(report.Date),
                    RegisterID = e.RegisterId,
                    ReportOpeningChangeFloat = report.RegisterOpeningAmount,
                    ReportOpenCashBoxNum = report.CashDrawerOpenings.ToString(),
                    //TODO(andrei) resolve these properties
                    //ReportReceiptNum = e.Report.ReceiptsPrinted.ToString(),
                    //ReportReceiptCopyNum = e.Report.CopyReceiptsPrinted.ToString(),
                    //ReportReceiptCopyAmnt = e.Report.TotalCopyReceiptsAmount,
                    ReportReceiptProformaNum = "0",
                    ReportReceiptProformaAmnt = 0,
                    ReportReturnNum = report.Return.Count.ToString(),
                    ReportReturnAmnt = Math.Abs(report.Return.Amount.RoundAwayFromZero()),
                    ReportDiscountNum = report.Discount.Count.ToString(),
                    ReportDiscountAmnt = Math.Abs(report.Discount.Amount.RoundAwayFromZero()),
                    ReportVoidTransNum = "0",
                    ReportVoidTransAmnt = 0,
                    ReportReceiptDeliveryNum = "0",
                    ReportReceiptDeliveryAmnt = 0,
                    ReportTrainingNum = "0",
                    ReportGrandTotalReturn = Math.Abs(report.Totals.Return),
                    ReportGrandTotalSales = Math.Abs(report.Totals.Gross),
                    ReportGrandTotalSalesNet = Math.Abs(report.Totals.Net)
                };

                cashRegisterEvent.EventReport.ReportTotalCashSales = new AuditfileCompanyLocationCashregisterEventEventReportReportTotalCashSales
                {
                    TotalCashSaleAmnt = Math.Abs(cashRegisterEvent.EventReport.ReportGrandTotalSales)
                };

                // Only have 1 article group, because we do not have this. Total is the sum of all the payments and the count is the number of payments -tada-
                cashRegisterEvent.EventReport.ReportArtGroups.Add(new AuditfileCompanyLocationCashregisterEventEventReportReportArtGroupsReportArtGroup
                {
                    ArtGroupID = ArticleGroupID,
                    ArtGroupAmnt = Math.Abs(report.Totals.Gross),
                    ArtGroupNum = report.Payments.Sum(p => p.Count)
                });

                // Summary of payments per type
                cashRegisterEvent.EventReport.ReportPayments.AddRange(report.Payments.Select(p => new AuditfileCompanyLocationCashregisterEventEventReportReportPaymentsReportPayment
                {
                    PaymentType = p.PaymentCategory.ToString(),
                    PaymentNum = p.Count.ToString(),
                    PaymentAmnt = Math.Abs(p.Amount.RoundAwayFromZero())
                }));

                cashRegisterEvent.EventReport.ReportCashSalesVat.AddRange(report.Taxes
                  .GroupBy(t => t.TaxRate)
                  .Select(t =>
                  {
                      var amount = t.Sum(g => g.Amount);

                      return new AuditfileCompanyLocationCashregisterEventEventReportReportCashSalesVatReportCashSaleVat
                      {
                          VatPerc = Math.Round((t.Key - 1) * 100, 3), // TODO RoundOn check (rounds to 3)
                          CashSaleAmnt = Math.Abs(t.Sum(g => g.Base + g.Amount)),
                          VatAmnt = Math.Abs(amount),
                          VatAmntTp = AmountToCreditType(amount),
                          VatAmntTpSpecified = true
                      };
                  }));

                cashRegisterEvent.EventReport.ReportEmpPayments.AddRange(report.PaymentsPerEmployee.Select(p => new AuditfileCompanyLocationCashregisterEventEventReportReportEmpPaymentsReportEmpPayment
                {
                    EmpID = p.Employee.SystemId,
                    PaymentType = p.Payment.PaymentCategory.ToString(),
                    PaymentNum = p.Payment.Count.ToString(),
                    PaymentAmnt = Math.Abs(p.Payment.Amount)
                }));

                // Do not have this
                cashRegisterEvent.EventReport.ReportCorrLines.Add(new AuditfileCompanyLocationCashregisterEventEventReportReportCorrLinesReportCorrLine
                {
                    CorrLineAmnt = 0,
                    CorrLineNum = "0",
                    CorrLineType = "Korriger"
                });

                // Do not have this
                cashRegisterEvent.EventReport.ReportPriceInquiries.Add(new AuditfileCompanyLocationCashregisterEventEventReportReportPriceInquiriesReportPriceInquiry
                {
                    PriceInquiryAmnt = 0,
                    PriceInquiryNum = "0",
                    PriceInquiryGroup = ArticleGroupID
                });

                // Do not have this
                cashRegisterEvent.EventReport.ReportOtherCorrs.Add(new AuditfileCompanyLocationCashregisterEventEventReportReportOtherCorrsReportOtherCorr
                {
                    OtherCorrAmnt = 0,
                    OtherCorrNum = "0",
                    OtherCorrType = "Korriger"
                });
            }

            return cashRegisterEvent;
        }

        private async Task<AuditfileCompanyLocation> CreateLocationAsync(AuditContext context, ICollection<Invoice> invoices)
        {
            if (!invoices.Any())
            {
                return null;
            }

            // Grouped by supplier so this is legit to pick the correct supplier for the group of invoices
            var supplier = invoices.First().Supplier;

            var location = new AuditfileCompanyLocation
            {
                Name = supplier.Name,
                StreetAddress = new AuditfileCompanyLocationStreetAddress
                {
                    City = supplier.Address.City,
                    Country = Countrycode.NO,
                    CountrySpecified = true,
                    Number = supplier.Address.Number,
                    Region = supplier.Address.Region,
                    Streetname = supplier.Address.Street,
                    PostalCode = supplier.Address.PostalCode
                }
            };

            var invoicesGroupedByCashRegister = invoices.ToLookup(i => i.RegisterId);
            var eventsGroupedByCashRegister = context.Events.ToLookup(e => e.RegisterId);

            var registerIds = new HashSet<string>();
            registerIds.AddRange(invoicesGroupedByCashRegister.Select(g => g.Key));
            registerIds.AddRange(eventsGroupedByCashRegister.Select(g => g.Key));

            foreach (var registerId in registerIds)
            {
                var invoicesForRegister = invoicesGroupedByCashRegister[registerId];
                var eventsForRegister = eventsGroupedByCashRegister[registerId];

                location.Cashregister.Add(await CreateCashRegisterAsync(context, registerId, invoicesForRegister, eventsForRegister));
            }

            return location;
        }

        private static string GetTaxCode(TaxesCategory taxesCategory)
        {
            // See docs/Standard_Tax_Codes.csv

            if (taxesCategory == TaxesCategory.Intermediate)
            {
                return "31";
            }

            if (taxesCategory == TaxesCategory.Low)
            {
                return "33";
            }

            if (taxesCategory == TaxesCategory.Zero)
            {
                return "5";
            }

            if (taxesCategory == TaxesCategory.Exempt)
            {
                return "0";
            }

            // High
            return "3";
        }


        private static DateTime FormatTime(DateTime d) => new DateTime(d.Year, d.Month, d.Day, d.Hour, d.Minute, d.Second);

        private static Debitcredittype AmountToCreditType(decimal amount) => amount < 0 ? Debitcredittype.C : Debitcredittype.D;
    }
}
