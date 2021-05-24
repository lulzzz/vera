using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vera.Audits;
using Vera.Extensions;
using Vera.Models;
using Vera.Portugal.Invoices;
using Vera.Portugal.Models;
using Vera.Portugal.Stores;
using Invoice = Vera.Models.Invoice;
using ProductType = Vera.Portugal.Models.ProductType;

namespace Vera.Portugal.Audits
{
    public class AuditCreator
    {
        private const string AuditFileVersion = "1.04_01";
        private const string TaxEntity = "Global";
        private const string SourceId = "2";
        private const string SelfBillingIndicator = "0";
        private const string CashVatSchemeIndicator = "0";
        private const string ThirdPartiesBillingIndicator = "0";
        private const string UnitOfMeasure = "UN";

        private readonly string _productCompanyTaxId;
        private readonly string _certificateName;
        private readonly string _certificateNumber;

        private readonly IWorkingDocumentStore _wdStore;

        public AuditCreator(string productCompanyTaxId, string certificateName, string certificateNumber, 
            IWorkingDocumentStore wdStore)
        {
            _productCompanyTaxId = productCompanyTaxId;
            _certificateName = certificateName;
            _certificateNumber = certificateNumber;
            _wdStore = wdStore;
        }

        public async Task<AuditFile> Create(AuditContext context, AuditCriteria criteria)
        {
            var taxCountryRegion = context.Account.Address.Country;

            var auditFile = CreateAuditFileModel(context, criteria);

            ApplyCustomers(context.Invoices, auditFile);
            ApplyProducts(context.Invoices, auditFile);
            ApplyTaxTable(context.Invoices, auditFile, taxCountryRegion);
            ApplyInvoices(context.Invoices, auditFile, taxCountryRegion);
            await ApplyWorkingDocumentsAsync(context.Invoices, auditFile, taxCountryRegion);

            return auditFile;
        }

        private AuditFile CreateAuditFileModel(AuditContext context, AuditCriteria criteria)
        {
            var productId = context.SoftwareVersion + "/" + _certificateName;
            var companyAddress = context.Account.Address;
            var startDate = criteria.StartDate;
            var endDate = criteria.EndDate;

            return new AuditFile
            {
                Header = new Models.Header
                {
                    AuditFileVersion = AuditFileVersion,
                    CompanyID = context.Account.RegistrationNumber,
                    TaxRegistrationNumber = context.Account.TaxRegistrationNumber,
                    TaxAccountingBasis = TaxAccountingBasis.F,
                    CompanyName = context.Account.Name,
                    BusinessName = context.SoftwareCompanyName,
                    Telephone = context.Account.Telephone,
                    Email = context.Account.Email,
                    CompanyAddress = new AddressStructurePT
                    {
                        AddressDetail = $"{companyAddress.Street} {companyAddress.Number}",
                        City = companyAddress.City,
                        PostalCode = companyAddress.PostalCode,
                        Region = companyAddress.Region,
                        Country = companyAddress.Country
                    },
                    FiscalYear = startDate.Year.ToString(),
                    StartDate = startDate,
                    EndDate = endDate,
                    CurrencyCode = context.Account.Currency,
                    DateCreated = DateTime.Today,
                    TaxEntity = TaxEntity,
                    ProductCompanyTaxID = _productCompanyTaxId,
                    SoftwareCertificateNumber = _certificateNumber,
                    ProductID = productId,
                    ProductVersion = context.SoftwareVersion
                },
                MasterFiles = new AuditFileMasterFiles(),
                SourceDocuments = new SourceDocuments
                {
                    SalesInvoices = new SourceDocumentsSalesInvoices()
                }
            };
        }

        private static void ApplyCustomers(IEnumerable<Invoice> invoices, AuditFile auditFile)
        {
            var anonymousAddress = new AddressStructure
            {
                AddressDetail = Constants.UnknownLabel,
                City = Constants.UnknownLabel,
                PostalCode = Constants.UnknownLabel,
                Region = Constants.UnknownLabel,
                Country = Constants.UnknownLabel
            };

            var anonymous = new Models.Customer
            {
                CustomerID = "0",
                CustomerTaxID = Constants.DefaultCustomerTaxId,
                AccountID = Constants.UnknownLabel,
                CompanyName = "Consumidor final",
                SelfBillingIndicator = SelfBillingIndicator,
                BillingAddress = anonymousAddress,
                ShipToAddress = new[] {anonymousAddress}
            };

            var customers = invoices
                .Select(i => i.Customer)
                .Where(c => c != null)
                .GroupBy(c => ComputeCustomerID(c.SystemId, c.RegistrationNumber));

            auditFile.MasterFiles.Customer = customers.Select(g =>
            {
                var c = g.First();

                return new Models.Customer
                {
                    CustomerID = g.Key,
                    CustomerTaxID = string.IsNullOrEmpty(c.RegistrationNumber)
                        ? Constants.DefaultCustomerTaxId
                        : c.RegistrationNumber,
                    AccountID = c.BankAccount?.Number ?? Constants.UnknownLabel,
                    CompanyName = string.IsNullOrEmpty(c.CompanyName) ? "Consumidor final" : c.CompanyName,
                    Email = c.Email,
                    SelfBillingIndicator = SelfBillingIndicator,
                    BillingAddress = new AddressStructure
                    {
                        AddressDetail = c.BillingAddress?.Street ?? Constants.UnknownLabel,
                        City = c.BillingAddress?.City ?? Constants.UnknownLabel,
                        PostalCode = c.BillingAddress?.PostalCode ?? Constants.UnknownLabel,
                        Region = c.BillingAddress?.Region ?? Constants.UnknownLabel,
                        Country = c.BillingAddress?.Country ?? Constants.UnknownLabel
                    },
                    ShipToAddress = new[]
                    {
                        new AddressStructure
                        {
                            AddressDetail = c.ShippingAddress?.Street ?? Constants.UnknownLabel,
                            City = c.ShippingAddress?.City ?? Constants.UnknownLabel,
                            PostalCode = c.ShippingAddress?.PostalCode ?? Constants.UnknownLabel,
                            Region = c.ShippingAddress?.Region ?? Constants.UnknownLabel,
                            Country = c.ShippingAddress?.Country ?? Constants.UnknownLabel
                        }
                    }
                };
            }).Concat(new[] {anonymous}).ToArray();
        }

        private static void ApplyProducts(IEnumerable<Invoice> invoices, AuditFile auditFile)
        {
            var products = invoices
                .SelectMany(l => l.Lines)
                .Where(l => l.Product != null)
                .Select(l => l.Product)
                .GroupBy(x => x.Code);

            auditFile.MasterFiles.Product = products.Select(g =>
            {
                var product = g.First();
                
                return new Models.Product
                {
                    ProductCode = product.Code,
                    ProductNumberCode = product.Barcode,
                    ProductType = product.Type switch
                    {
                        // TODO(kevin): missing "S" for service?
                        Vera.Models.ProductType.Goods => ProductType.P,
                        _ => ProductType.O
                    },
                    ProductDescription = product.Description
                };
            }).ToArray();
        }

        private static void ApplyTaxTable(IEnumerable<Invoice> invoices, AuditFile auditFile, string taxCountryRegion)
        {
            var taxes = invoices
                .SelectMany(i => i.Lines)
                .Select(l => l.Taxes)
                .GroupBy(t => t.Category);

            auditFile.MasterFiles.TaxTable = taxes.Select(g =>
            {
                var t = g.First();

                return new Models.TaxTableEntry()
                {
                    TaxType = TaxType.IVA,
                    TaxCountryRegion = taxCountryRegion,
                    TaxCode = GetTaxCode(t.Category),
                    Description = GetTaxPercentage(t.Rate) + "%",
                    Item = GetTaxPercentage(t.Rate),
                    ItemElementName = ItemChoiceType2.TaxPercentage
                };
            }).ToArray();
        }

        private static void ApplyInvoices(IEnumerable<Invoice> invoices, AuditFile auditFile, string taxCountryRegion)
        {
            var sourceDocumentsSalesInvoices = auditFile.SourceDocuments.SalesInvoices;

            sourceDocumentsSalesInvoices.Invoice = invoices.Select(invoice =>
            {
                var totals = invoice.Totals;

                return new SourceDocumentsSalesInvoicesInvoice
                {
                    InvoiceNo = invoice.Number,
                    ATCUD = Constants.ATCUD,
                    DocumentStatus = new SourceDocumentsSalesInvoicesInvoiceDocumentStatus
                    {
                        InvoiceStatus = InvoiceStatus.N,
                        InvoiceStatusDate = GetDateTime(invoice.Date),
                        SourceID = SourceId,
                        SourceBilling = invoice.Manual ? SAFTPTSourceBilling.M : SAFTPTSourceBilling.P
                    },

                    DocumentTotals = new SourceDocumentsSalesInvoicesInvoiceDocumentTotals
                    {
                        TaxPayable = totals.Taxes.Total.Round(2),
                        NetTotal = totals.Net.Round(2),
                        GrossTotal = totals.Gross.Round(2),
                        Settlement = invoice.Settlements?.Select(s => new Models.Settlement
                        {
                            SettlementAmount = s.Amount.Round(2),
                            SettlementAmountSpecified = true,
                            PaymentTerms = s.Description,

                            // TODO(kevin): check if SAF-T file is valid without these
                            // SettlementDate = s.Date,
                            // SettlementDateSpecified = true
                        }).ToArray()
                    },

                    Hash = Convert.ToBase64String(invoice.Signature.Output),
                    HashControl = GetHashControl(invoice),

                    Period = invoice.Date.Month.ToString(),
                    InvoiceDate = GetDateTime(invoice.Date),
                    InvoiceType = InvoiceTypeHelper.DetermineType(invoice),

                    SpecialRegimes = new SpecialRegimes
                    {
                        SelfBillingIndicator = SelfBillingIndicator,
                        CashVATSchemeIndicator = CashVatSchemeIndicator,
                        ThirdPartiesBillingIndicator = ThirdPartiesBillingIndicator
                    },

                    SourceID = SourceId,
                    SystemEntryDate = GetDateTime(invoice.Date),
                    CustomerID = ComputeCustomerID(invoice.Customer?.SystemId, invoice.Customer?.RegistrationNumber),
                    Line = invoice.Lines.Select((line, i) => MapInvoiceLine(invoice, taxCountryRegion, line, i))
                        .ToArray()
                };
            }).ToArray();

            var totalDebitExTax = 0m;
            var totalCreditExTax = 0m;

            foreach (var invoice in invoices)
            {
                var totals = invoice.Totals;

                if (totals.Net > 0)
                {
                    totalDebitExTax += totals.Net;
                }
                else
                {
                    totalCreditExTax += totals.Net;
                }
            }

            sourceDocumentsSalesInvoices.NumberOfEntries = invoices.Count().ToString();
            sourceDocumentsSalesInvoices.TotalDebit = totalDebitExTax.Round(2);
            sourceDocumentsSalesInvoices.TotalCredit = totalCreditExTax.Round(2);
        }

        private async Task ApplyWorkingDocumentsAsync(ICollection<Invoice> invoices, AuditFile auditFile, string taxCountryRegion)
        {
            var auditDocuments = new List<SourceDocumentsWorkingDocumentsWorkDocument>();

            foreach (var invoice in invoices)
            {
                var workingDocuments = await _wdStore.List(invoice.Id);
                if (!workingDocuments.Any())
                {
                    continue;
                }

                auditDocuments.AddRange(workingDocuments.Select(document => new SourceDocumentsWorkingDocumentsWorkDocument
                {
                    DocumentNumber = document.Number,
                    ATCUD = Constants.ATCUD,
                    DocumentStatus = new SourceDocumentsWorkingDocumentsWorkDocumentDocumentStatus
                    {
                        WorkStatus = WorkStatus.F,
                        WorkStatusDate = GetDateTime(document.CreationTime),
                        SourceID = invoice.Employee.SystemId,
                        SourceBilling = SAFTPTSourceBilling.P
                    },
                    Hash = document.Signature.Output.ToString(),
                    HashControl = document.Signature.Version.ToString(),
                    WorkDate = GetDateTime(document.CreationTime),
                    WorkType = (WorkType.CM),
                    SourceID = invoice.Employee.SystemId,
                    SystemEntryDate = GetDateTime(document.CreationTime),
                    CustomerID = ComputeCustomerID(invoice.Customer.SystemId, invoice.Customer.RegistrationNumber),
                    Line = document.Lines.Select((l, index) => new SourceDocumentsWorkingDocumentsWorkDocumentLine
                    {
                        LineNumber = (index + 1).ToString(),
                        ProductCode = l.Product.Code,
                        ProductDescription = l.Product.Description,
                        Description = l.Description,
                        Quantity = l.Quantity,
                        UnitOfMeasure = UnitOfMeasure,
                        UnitPrice = l.UnitPrice,
                        TaxPointDate = GetDateTime(document.CreationTime),
                        Item = l.Gross.RoundAwayFromZero().Absolute(),
                        ItemElementName = l.Gross > 0 ? ItemChoiceType7.CreditAmount : ItemChoiceType7.DebitAmount,
                        Tax = new Tax
                        {
                            TaxType = TaxType.IVA,
                            TaxCountryRegion = taxCountryRegion,
                            TaxCode = GetTaxCode(l.Taxes.Category),
                            Item = GetTaxPercentage(l.Taxes.Rate),
                            ItemElementName = ItemChoiceType1.TaxPercentage
                        }
                    }).ToArray(),
                    DocumentTotals = new SourceDocumentsWorkingDocumentsWorkDocumentDocumentTotals
                    {
                        TaxPayable = document.Lines.Sum(l => l.Gross - l.Net).RoundAwayFromZero().Absolute(),
                        NetTotal = document.Lines.Sum(l => l.Net).RoundAwayFromZero().Absolute(),
                        GrossTotal = document.Lines.Sum(l => l.Gross).RoundAwayFromZero().Absolute()
                    }
                }));
            }

            auditFile.SourceDocuments.WorkingDocuments = new SourceDocumentsWorkingDocuments
            {
                WorkDocument = auditDocuments.ToArray(),
                TotalDebit = auditDocuments.Where(a => a.DocumentTotals.NetTotal > 0).Sum(a => a.DocumentTotals.NetTotal),
                TotalCredit = auditDocuments.Where(a => a.DocumentTotals.NetTotal < 0).Sum(a => a.DocumentTotals.NetTotal),
                NumberOfEntries = auditDocuments.Count().ToString()
            };
        }

        private static SourceDocumentsSalesInvoicesInvoiceLine MapInvoiceLine(
            Invoice invoice,
            string taxCountryRegion,
            InvoiceLine line,
            int index)
        {
            var settlement = (line.Settlements?.Sum(s => s.Amount) ?? 0).Round(2);

            return new SourceDocumentsSalesInvoicesInvoiceLine
            {
                LineNumber = (index + 1).ToString(),

                ProductCode = line.Product?.Code,
                ProductDescription = line.Product?.Description,

                Quantity = Math.Abs(line.Quantity),
                UnitOfMeasure = UnitOfMeasure,

                References = line.CreditReference == null
                    ? Array.Empty<References>()
                    : new References[]
                    {
                        new()
                        {
                            Reference = line.CreditReference.Number,
                            Reason = line.CreditReference.Reason
                        }
                    },

                Description = line.Description,

                UnitPrice = line.UnitPrice.Round(4),
                Item = line.Net.Round(2),
                ItemElementName = line.Net > 0 ? ItemChoiceType4.CreditAmount : ItemChoiceType4.DebitAmount,

                SettlementAmount = settlement,
                SettlementAmountSpecified = settlement != 0,

                Tax = new Tax
                {
                    TaxType = TaxType.IVA,
                    TaxCountryRegion = taxCountryRegion,
                    TaxCode = GetTaxCode(line.Taxes.Category),
                    Item = GetTaxPercentage(line.Taxes.Rate),
                    ItemElementName = ItemChoiceType1.TaxPercentage
                },
                TaxPointDate = GetDateTime(invoice.Date),
                TaxExemptionCode = line.Taxes.ExemptionCode,
                TaxExemptionReason = line.Taxes.ExemptionReason
            };
        }

        private static string GetTaxCode(TaxesCategory category)
        {
            return category switch
            {
                TaxesCategory.High => "NOR",
                TaxesCategory.Low => "RED",
                TaxesCategory.Zero => "NA",
                TaxesCategory.Exempt => "ISE",
                TaxesCategory.Intermediate => "INT",
                _ => throw new ArgumentOutOfRangeException(nameof(category), category, null)
            };
        }

        private static int GetTaxPercentage(decimal taxRate) => (int) (100 * (taxRate - 1));

        private static string ComputeCustomerID(string systemID, string registrationNumber)
        {
            if (string.IsNullOrEmpty(systemID))
            {
                return string.IsNullOrEmpty(registrationNumber) ? "0" : $"N{registrationNumber}";
            }

            return systemID;
        }

        private static string GetHashControl(Invoice invoice)
        {
            var hashControl = invoice.Signature.Version;

            if (invoice.Manual)
            {
                var type = InvoiceTypeHelper.DetermineType(invoice);

                return $"{hashControl}-{type}M {invoice.Number}";
            }

            return hashControl.ToString();
        }

        private static DateTime GetDateTime(DateTime dt) =>
            new(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
    }
}