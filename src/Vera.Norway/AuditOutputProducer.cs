//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Xml;
//using System.Xml.Serialization;

//using Vera.Norway.Extensions;

//namespace Vera.Norway
//{
//    public class AuditOutputProducer
//    {
//        private const string TicketLineBasicIDSale = "SALE";
//        private const string TicketLineBasicIDReturn = "RETURN";

//        private const string ArticleGroupID = "OTHER";

//        private readonly IDateProvider _dateProvider;
//        //private readonly IStringTranslationProvider _stringTranslationProvider;
//        private readonly ICollection<AuditfileCompanyBasicsBasic> _transactionBasics;

//        public AuditOutputProducer(IDateProvider dateProvider)
//        {
//            _dateProvider = dateProvider;
//            //_stringTranslationProvider = stringTranslationProvider;
//            _transactionBasics = new List<AuditfileCompanyBasicsBasic>();
//        }

//        //public void Produce(FinancialPeriodAudit financialPeriodAudit, Audit audit, Stream stream)
//        //{
//        //    // NOTE: see docs/norwegian-saf-t-cash-register-data---technical-description2.pdf for more information about all of the fields below

//        //    var file = new Auditfile();
//        //    file.Header = CreateHeader(audit);
//        //    file.Company = CreateCompany(audit);

//        //    var settings = new XmlWriterSettings
//        //    {
//        //        Indent = true,
//        //        Encoding = Encoding.UTF8,
//        //        CloseOutput = false
//        //    };

//        //    using (var writer = XmlWriter.Create(stream, settings))
//        //    {
//        //        var serializer = new XmlSerializer(typeof(Auditfile));
//        //        serializer.Serialize(writer, file);
//        //    }
//        //}

//        //public Task<FinancialPeriodAuditBlobInfo> GetInfo(FinancialPeriodAudit audit)
//        //{
//        //    const string type = "SAF-T Cash Register";

//        //    // Currently only support generating one file at a time
//        //    const int sequence = 1;
//        //    const int total = 1;

//        //    var organizationNumber = audit.OrganizationUnitID.ToString();
//        //    var creationTime = _dateProvider.Now.ToString("yyyyMMddHHmmss");

//        //    // Format as defined in the "Naming of the SAF-T data file"
//        //    var fileName = $"{type}_{organizationNumber}_{creationTime}_{sequence}_{total}.xml";

//        //    return new FinancialPeriodAuditBlobInfo(fileName, BlobMimeTypes.Xml).AsTask();
//        //}

//        private AuditfileHeader CreateHeader(Audit audit)
//        {
//            return new AuditfileHeader
//            {
//                CurCode = Currencycode.NOK,
//                AuditfileVersion = "1.0",
//                //DateCreated = audit.Header.CreationTime,
//                //TimeCreated = FormatTime(audit.Header.CreationTime),
//                //FiscalYear = audit.Header.SelectionCriteria.SelectionStartDate.Year.ToString(),
//                //StartDate = audit.Header.SelectionCriteria.SelectionStartDate,
//                //EndDate = audit.Header.SelectionCriteria.SelectionEndDate,
//                //SoftwareDesc = audit.Header.SoftwareName,
//                //SoftwareVersion = audit.Header.SoftwareVersion,
//                //SoftwareCompanyName = audit.Header.SoftwareCompanyName
//            };
//        }

//        private AuditfileCompany CreateCompany(Audit audit)
//        {
//            var company = new AuditfileCompany
//            {
//                CompanyIdent = audit.Header.Company.RegistrationNumber,
//                CompanyName = audit.Header.Company.Name,
//                TaxRegIdent = audit.Header.Company.TaxRegistration.Number,
//                TaxRegistrationCountry = Countrycode.NO,
//                StreetAddress =
//        {
//          new AuditfileCompanyStreetAddress
//          {
//            City = audit.Header.Company.Address.City,
//            Country = Countrycode.NO,
//            Region = audit.Header.Company.Address.Region,
//            Streetname = audit.Header.Company.Address.Street,
//            PostalCode = audit.Header.Company.Address.PostalCode,
//            Number = audit.Header.Company.Address.Number
//          }
//        },
//                PostalAddress =
//        {
//          new AuditfileCompanyPostalAddress
//          {
//            City = audit.Header.Company.Address.City,
//            Country = Countrycode.NO,
//            Region = audit.Header.Company.Address.Region,
//            Streetname = audit.Header.Company.Address.Street,
//            PostalCode = audit.Header.Company.Address.PostalCode,
//            Number = audit.Header.Company.Address.Number
//          }
//        }
//            };

//            company.CustomersSuppliers.AddRange(audit.MasterFiles.Customers.Select(c =>
//            {
//                var customer = new AuditfileCompanyCustomersSuppliersCustomerSupplier
//                {
//                    CustSupID = c.SystemID,
//                    CustSupName = c.Contact.Person.FullName,
//                    CustSupType = Custsuptype.Customer,
//                    Contact = c.Contact.Person.FullName,
//                };

//                if (c.TaxRegistration.Number.IsNotNullAndNotEmpty())
//                {
//                    customer.TaxRegIdent = c.TaxRegistration.Number;
//                    customer.TaxRegistrationCountry = Countrycode.NO;
//                    customer.TaxRegistrationCountrySpecified = true;
//                }

//                return customer;
//            }));

//            company.Employees.AddRange(audit.MasterFiles.Employees.Select(e => new AuditfileCompanyEmployeesEmployee
//            {
//                FirstName = e.Contact.Person.FirstName,
//                SurName = e.Contact.Person.LastName,
//                EmpID = e.ID
//            }));

//            company.Articles.AddRange(audit.MasterFiles.Products.Select(p => new AuditfileCompanyArticlesArticle
//            {
//                ArtGroupID = BasicsMapper.FormatPredefinedBasicID(PredefinedProductBasics.Other),
//                ArtID = p.Code,
//                ArtDesc = p.Description
//            }));


//            var startPeriod = audit.MasterFiles.Periods.Min(p => p.Start);
//            var endPeriod = audit.MasterFiles.Periods.Max(p => p.End);

//            for (var month = startPeriod.Month; month <= endPeriod.Month; month++)
//            {
//                var start = new DateTime(startPeriod.Year, month, 1);
//                var end = new DateTime(startPeriod.Year, month, DateTime.DaysInMonth(startPeriod.Year, month));

//                // Period has a restriction, it it's limited from 0-999 (inclusive) so we're unable to use the actual period
//                // which would be the reference to the financial period. That's why we chose to use the number of the month for the
//                // period instead.
//                company.Periods.Add(new AuditfileCompanyPeriodsPeriod
//                {
//                    PeriodNumber = month.ToString(),
//                    PeriodDesc = $"{start:yyyy-MM-dd} - {end:yyyy-MM-dd}",
//                    StartDatePeriod = FormatTime(start),
//                    StartTimePeriod = FormatTime(start),
//                    EndDatePeriod = FormatTime(end),
//                    EndTimePeriod = FormatTime(end),
//                    StartDatePeriodSpecified = true,
//                    StartTimePeriodSpecified = true,
//                    EndDatePeriodSpecified = true,
//                    EndTimePeriodSpecified = true
//                });
//            }

//            company.VatCodeDetails.AddRange(audit.MasterFiles.TaxTable.Select(table => new AuditfileCompanyVatCodeDetailsVatCodeDetail
//            {
//                VatCode = table.Type,
//                DateOfEntry = table.Details[0].EffectiveDate ?? DateTime.Now.AddDays(-1),
//                StandardVatCode = GetTaxCode(table.Details[0].Code)
//            }));

//            // Physical locations (the shops)
//            company.Location.AddRange(audit.SourceDocuments.SalesInvoices.Sources
//              .GroupBy(i => i.Supplier.SystemID)
//              .Select(g => CreateLocation(audit, g))
//            );

//            company.Basics.AddRange(CreateBasics(audit));
//            company.Basics.AddRange(_transactionBasics);

//            return company;
//        }

//        private IEnumerable<AuditfileCompanyBasicsBasic> CreateBasics(Audit audit)
//        {
//            //foreach (var e in audit.MasterFiles.Events.Distinct(e => e.Type))
//            //{
//            //    yield return BasicsMapper.FromEvent(e, _stringTranslationProvider);
//            //}

//            yield return new AuditfileCompanyBasicsBasic
//            {
//                BasicType = BasicsMapper.FormatBasicType(BasicTypes.ArticleGroup),
//                PredefinedBasicID = BasicsMapper.FormatPredefinedBasicID(PredefinedProductBasics.Other),
//                BasicID = ArticleGroupID,
//                BasicDesc = "Other article group"
//            };

//            foreach (var p in audit.SourceDocuments.Payments.Sources.Distinct(p => p.Method))
//            {
//                yield return BasicsMapper.FromPayment(p);
//            }

//            // TODO (kevin) check this and maybe move it to the audit generator
//            var discounts = audit.SourceDocuments.SalesInvoices.Sources
//              .SelectMany(i => i.Settlements.EmptyIfNull())
//              .Concat(
//                audit.SourceDocuments.SalesInvoices.Sources
//                  .SelectMany(i => i.Lines)
//                  .SelectMany(l => l.Settlements.EmptyIfNull())
//              )
//              .Distinct(s => s.SystemID)
//              .ToList();

//            foreach (var discount in discounts)
//            {
//                yield return BasicsMapper.FromDiscount(discount);
//            }

//            // TODO (kevin) can make this more complex in the future, this should be sufficient for the time being
//            yield return new AuditfileCompanyBasicsBasic
//            {
//                BasicType = BasicsMapper.FormatBasicType(BasicTypes.TicketLine),
//                BasicID = TicketLineBasicIDSale,
//                BasicDesc = "Sales"
//            };

//            yield return new AuditfileCompanyBasicsBasic
//            {
//                BasicType = BasicsMapper.FormatBasicType(BasicTypes.TicketLine),
//                BasicID = TicketLineBasicIDReturn,
//                BasicDesc = "Returns"
//            };
//        }

//        private AuditfileCompanyLocationCashregister CreateCashRegister(
//          Audit audit,
//          string terminalID,
//          IEnumerable<Invoice> invoices,
//          IEnumerable<Event> events,
//          IEnumerable<Payment> payments
//        )
//        {
//            var register = new AuditfileCompanyLocationCashregister
//            {
//                RegisterID = terminalID
//            };

//            foreach (var e in events)
//            {
//                register.Event.Add(CreateEvent(audit, e));
//            }

//            foreach (var invoice in invoices)
//            {
//                var invoicePayments = payments.Where(p => p.TransactionID == invoice.TransactionID);

//                register.Cashtransaction.Add(CreateTransaction(audit.Header.DefaultCurrencyCode, invoice, invoicePayments));
//            }

//            return register;
//        }

//        private AuditfileCompanyLocationCashregisterCashtransaction CreateTransaction(string currencyID, Invoice invoice, IEnumerable<Payment> payments)
//        {
//            var basic = BasicsMapper.FromInvoice(invoice, payments);

//            var transaction = new AuditfileCompanyLocationCashregisterCashtransaction
//            {
//                // nr == transID (allowed by docs)
//                Nr = invoice.Number,
//                TransID = invoice.Number,
//                TransType = basic.BasicID,

//                TransAmntIn = Math.Abs(invoice.Totals.Gross.RoundFor(currencyID)),
//                TransAmntEx = Math.Abs(invoice.Totals.Net.RoundFor(currencyID)),

//                AmntTp = AmountToCreditType(invoice.Totals.Net),

//                PeriodNumber = invoice.Date.Month.ToString(),
//                TransDate = invoice.Date,
//                TransTime = FormatTime(invoice.Date),

//                EmpID = invoice.SourceID,
//                CustSupID = invoice.Customer?.SystemID,

//                ReceiptNum = invoice.PrintCount.ToString(),
//                ReceiptCopyNum = invoice.PrintCount.ToString(),

//                // The digital signature based on RSA-SHA1-1024
//                Signature = invoice.Signature,

//                // The version of the private/secret key.
//                KeyVersion = invoice.SignatureKeyVersion.ToString()
//            };

//            if (invoice.Settlements != null)
//            {
//                transaction.Discount.AddRange(invoice.Settlements.Select(s => new AuditfileCompanyLocationCashregisterCashtransactionDiscount
//                {
//                    DscAmnt = Math.Abs(s.Amount.Value.RoundFor(currencyID)),
//                    DscTp = s.SystemID
//                }));
//            }

//            foreach (var p in payments)
//            {
//                transaction.Payment.Add(CreatePayment(p));
//            }

//            if (_transactionBasics.All(b => b.BasicID != basic.BasicID))
//            {
//                // Add to the basics map if it does not exist yet
//                _transactionBasics.Add(basic);
//            }

//            transaction.CtLine.AddRange(invoice.Lines.Select((l, i) => CreateTransactionLine(currencyID, l, i + 1, transaction)));

//            return transaction;
//        }

//        private AuditfileCompanyLocationCashregisterCashtransactionPayment CreatePayment(Payment p)
//        {
//            return new AuditfileCompanyLocationCashregisterCashtransactionPayment
//            {
//                CurCode = Currencycode.NOK,
//                PaidAmnt = Math.Abs(p.Lines.Sum(x => x.Amount.Value)),
//                PaymentType = BasicsMapper.FromPayment(p).BasicID,
//                PaymentRefID = p.Reference
//            };
//        }

//        private AuditfileCompanyLocationCashregisterCashtransactionCtLine CreateTransactionLine(string currencyID, InvoiceLine line, int sequence, AuditfileCompanyLocationCashregisterCashtransaction transaction)
//        {
//            // The ctLine element contains details data of a transaction. If, for instance, the transaction describes a sales slip, the ctLines are the slip lines.
//            // The data not described in the cash transaction element, are described in the ctLine.

//            var vat = line.Taxes.First();

//            var ctLine = new AuditfileCompanyLocationCashregisterCashtransactionCtLine
//            {
//                // Transaction number, also used as reference to the cashtransaction - nr - element.
//                // This must be a unique, sequential number within a journal. This will be the same as the number stated on the issued receipt.
//                Nr = transaction.Nr,

//                // Transaction line number. Should be a unique sequential number within a transaction.
//                LineID = sequence.ToString(),

//                // Reference to ticketline codes. Description of the code must be stated in the table 'Basics'.
//                // Correction, returns, booking, deposit, gift certificate, etc.
//                LineType = line.Amount.Value > 0 ? TicketLineBasicIDSale : TicketLineBasicIDReturn,

//                ArtID = line.ProductCode,
//                ArtGroupID = ArticleGroupID,

//                Qnt = line.Quantity,

//                LineAmntIn = Math.Abs(line.AmountInTax.Value.RoundFor(currencyID)),
//                LineAmntEx = Math.Abs(line.Amount.Value.RoundFor(currencyID)),

//                AmntTp = AmountToCreditType(line.Amount.Value),

//                LineDate = line.Date,
//                LineTime = FormatTime(line.Date),

//                Vat = new AuditfileCompanyLocationCashregisterCashtransactionCtLineVat
//                {
//                    VatCode = vat.Code,
//                    VatPerc = (vat.Rate * 100 - 100),
//                    VatAmnt = Math.Abs(vat.Amount.Value.RoundFor(currencyID)),
//                    VatBasAmnt = Math.Abs(vat.Base.RoundFor(currencyID)),
//                    VatAmntTp = AmountToCreditType(vat.Amount.Value)
//                }
//            };

//            if (line.Settlements != null)
//            {
//                ctLine.Discount.AddRange(line.Settlements.Select(s => new AuditfileCompanyLocationCashregisterCashtransactionCtLineDiscount
//                {
//                    DscAmnt = s.Amount.Value.RoundFor(currencyID),
//                    DscTp = s.SystemID // ref to the basics table
//                }));
//            }

//            return ctLine;
//        }

//        private AuditfileCompanyLocationCashregisterEvent CreateEvent(Audit audit, Event e)
//        {
//            // The event element stores activity not represented as a cash register transaction (cashtransaction) or transaction line (ctline).
//            var cashRegisterEvent = new AuditfileCompanyLocationCashregisterEvent
//            {
//                // Unique system specific identification of this event.
//                // Replacing to limit to 35 characters
//                EventID = e.SystemID.Replace("-", ""),

//                // Reference to the type of event. Description of the system specific code must be stated in the table 'Basics' (basicType 13)
//                EventType = BasicsMapper.TranslateEventToBasicID(e),

//                EventDate = e.Date,
//                EventTime = FormatTime(e.Date),

//                EventText = e.Type,

//                // Reference to the employee. Employee must be included in the table "employees".
//                EmpID = e.SourceID,
//            };

//            if (e.Type == EventLedgerTypes.XReport || e.Type == EventLedgerTypes.ZReport)
//            {
//                cashRegisterEvent.EventReport = new AuditfileCompanyLocationCashregisterEventEventReport
//                {
//                    ReportID = e.Number,
//                    ReportType = e.Type == EventLedgerTypes.XReport
//                    ? AuditfileCompanyLocationCashregisterEventEventReportReportType.X_Report
//                    : AuditfileCompanyLocationCashregisterEventEventReportReportType.Z_Report,
//                    CompanyIdent = audit.Header.Company.RegistrationNumber,
//                    CompanyName = audit.Header.Company.Name,
//                    ReportDate = e.Report.Date,
//                    ReportTime = FormatTime(e.Report.Date),
//                    RegisterID = e.TerminalID,
//                    ReportOpeningChangeFloat = e.Report.Change,
//                    ReportReceiptNum = e.Report.ReceiptsPrinted.ToString(),
//                    ReportOpenCashBoxNum = e.Report.CashDrawerOpenings.ToString(),
//                    ReportReceiptCopyNum = e.Report.CopyReceiptsPrinted.ToString(),
//                    ReportReceiptCopyAmnt = e.Report.TotalCopyReceiptsAmount,
//                    ReportReceiptProformaNum = "0",
//                    ReportReceiptProformaAmnt = 0,
//                    ReportReturnNum = e.Report.ReturnCount.ToString(),
//                    ReportReturnAmnt = Math.Abs(e.Report.TotalReturnsAmount.RoundFor(audit.Header.DefaultCurrencyCode)),
//                    ReportDiscountNum = e.Report.DiscountCount.ToString(),
//                    ReportDiscountAmnt = Math.Abs(e.Report.TotalDiscounts.RoundFor(audit.Header.DefaultCurrencyCode)),
//                    ReportVoidTransNum = "0",
//                    ReportVoidTransAmnt = 0,
//                    ReportReceiptDeliveryNum = "0",
//                    ReportReceiptDeliveryAmnt = 0,
//                    ReportTrainingNum = "0",
//                    ReportGrandTotalReturn = Math.Abs(e.Report.GrandTotalReturns),
//                    ReportGrandTotalSales = Math.Abs(e.Report.GrandTotal),
//                    ReportGrandTotalSalesNet = Math.Abs(e.Report.GrandTotalNet)
//                };

//                cashRegisterEvent.EventReport.ReportTotalCashSales = new AuditfileCompanyLocationCashregisterEventEventReportReportTotalCashSales
//                {
//                    TotalCashSaleAmnt = Math.Abs(cashRegisterEvent.EventReport.ReportGrandTotalSales)
//                };

//                // Only have 1 article group, because we do not have this. Total is the sum of all the payments and the count is the number of payments -tada-
//                cashRegisterEvent.EventReport.ReportArtGroups.Add(new AuditfileCompanyLocationCashregisterEventEventReportReportArtGroupsReportArtGroup
//                {
//                    ArtGroupID = ArticleGroupID,
//                    ArtGroupAmnt = Math.Abs(e.Report.GrandTotal),
//                    ArtGroupNum = e.Report.Payments.Sum(p => p.Count)
//                });

//                // Summary of payments per type
//                cashRegisterEvent.EventReport.ReportPayments.AddRange(e.Report.Payments.Select(p => new AuditfileCompanyLocationCashregisterEventEventReportReportPaymentsReportPayment
//                {
//                    PaymentType = p.Code,
//                    PaymentNum = p.Count.ToString(),
//                    PaymentAmnt = Math.Abs(p.Amount.RoundFor(audit.Header.DefaultCurrencyCode))
//                }));

//                cashRegisterEvent.EventReport.ReportCashSalesVat.AddRange(e.Report.Taxes
//                  .GroupBy(t => t.Rate)
//                  .Select(t =>
//                  {
//                      var amount = t.Sum(g => g.Amount);

//                      return new AuditfileCompanyLocationCashregisterEventEventReportReportCashSalesVatReportCashSaleVat
//                      {
//                          VatPerc = ((t.Key - 1) * 100).RoundOn(3), // TODO RoundOn check (rounds to 3)
//                          CashSaleAmnt = Math.Abs(t.Sum(g => g.Base + g.Amount)),
//                          VatAmnt = Math.Abs(amount),
//                          VatAmntTp = AmountToCreditType(amount),
//                          VatAmntTpSpecified = true
//                      };
//                  }));

//                cashRegisterEvent.EventReport.ReportEmpPayments.AddRange(e.Report.PaymentsPerUser.Select(p => new AuditfileCompanyLocationCashregisterEventEventReportReportEmpPaymentsReportEmpPayment
//                {
//                    EmpID = p.UserID.ToString(),
//                    PaymentType = p.Description,
//                    PaymentNum = p.Count.ToString(),
//                    PaymentAmnt = Math.Abs(p.Amount)
//                }));

//                // Do not have this
//                cashRegisterEvent.EventReport.ReportCorrLines.Add(new AuditfileCompanyLocationCashregisterEventEventReportReportCorrLinesReportCorrLine
//                {
//                    CorrLineAmnt = 0,
//                    CorrLineNum = "0",
//                    CorrLineType = "Korriger"
//                });

//                // Do not have this
//                cashRegisterEvent.EventReport.ReportPriceInquiries.Add(new AuditfileCompanyLocationCashregisterEventEventReportReportPriceInquiriesReportPriceInquiry
//                {
//                    PriceInquiryAmnt = 0,
//                    PriceInquiryNum = "0",
//                    PriceInquiryGroup = ArticleGroupID
//                });

//                // Do not have this
//                cashRegisterEvent.EventReport.ReportOtherCorrs.Add(new AuditfileCompanyLocationCashregisterEventEventReportReportOtherCorrsReportOtherCorr
//                {
//                    OtherCorrAmnt = 0,
//                    OtherCorrNum = "0",
//                    OtherCorrType = "Korriger"
//                });
//            }

//            return cashRegisterEvent;
//        }

//        private AuditfileCompanyLocation CreateLocation(Audit audit, IEnumerable<Invoice> invoices)
//        {
//            // Grouped by supplier so this is legit to pick the correct supplier
//            // for the group of invoices
//            var supplier = invoices.First().Supplier;

//            var location = new AuditfileCompanyLocation
//            {
//                Name = supplier.Name,
//                StreetAddress = new AuditfileCompanyLocationStreetAddress
//                {
//                    City = supplier.Address.City,
//                    Country = Countrycode.NO,
//                    CountrySpecified = true,
//                    Number = supplier.Address.Number,
//                    Region = supplier.Address.Region,
//                    Streetname = supplier.Address.Street,
//                    PostalCode = supplier.Address.PostalCode
//                }
//            };

//            var invoicesGroupedByCashRegister = invoices.ToLookup(i => i.TerminalID);
//            var eventsGroupedByCashRegister = audit.MasterFiles.Events.ToLookup(e => e.TerminalID);

//            var terminalIDs = new HashSet<string>();
//            terminalIDs.AddRange(invoicesGroupedByCashRegister.Select(g => g.Key));
//            terminalIDs.AddRange(eventsGroupedByCashRegister.Select(g => g.Key));

//            foreach (var terminalID in terminalIDs)
//            {
//                var invoicesForTerminal = invoicesGroupedByCashRegister[terminalID].ToList();
//                var transactionIDs = invoicesForTerminal.Select(i => i.TransactionID).ToHashSet();

//                var events = eventsGroupedByCashRegister[terminalID];
//                var payments = audit.SourceDocuments.Payments.Sources.Where(p => transactionIDs.Contains(p.TransactionID));

//                location.Cashregister.Add(CreateCashRegister(audit, terminalID, invoicesForTerminal, events, payments));
//            }

//            return location;
//        }

//        private static string GetTaxCode(string taxCodeName)
//        {
//            // See docs/Standard_Tax_Codes.csv

//            if (taxCodeName == TaxCode.Intermediate.Name)
//            {
//                return "31";
//            }

//            if (taxCodeName == TaxCode.Low.Name)
//            {
//                return "33";
//            }

//            if (taxCodeName == TaxCode.Zero.Name)
//            {
//                return "5";
//            }

//            if (taxCodeName == TaxCode.Exempt.Name)
//            {
//                return "0";
//            }

//            // High
//            return "3";
//        }

//        private static DateTime FormatTime(DateTime d) => new DateTime(d.Year, d.Month, d.Day, d.Hour, d.Minute, d.Second);

//        private static Debitcredittype AmountToCreditType(decimal amount) => amount < 0 ? Debitcredittype.C : Debitcredittype.D;

//        public bool WillOutput => true;
//    }
//}