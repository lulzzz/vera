using System;
using System.Linq;
using System.Threading.Tasks;
using Vera.Audit;
using Vera.Portugal.Models;
using Vera.StandardAuditFileTaxation;

namespace Vera.Portugal
{
    public class AuditTransformer : IAuditTransformer<AuditFile>
    {
        private const string UnknownLabel = "Desconhecido";
        private const string AuditFileVersion = "1.04_01";
        private const string TaxEntity = "Global";
        private const string Atcud = "0";
        private const string SourceID = "2";
        private const string SelfBillingIndicator = "0";
        private const string CashVatSchemeIndicator = "0";
        private const string ThirdPartiesBillingIndicator = "0";
        private const string UnitOfMeasure = "UN";
        private const string DefaultFiscalID = "999999990";

        public Task<AuditFile> Transform(AuditContext context, StandardAuditFileTaxation.Audit audit)
        {
            // TODO(kevin): see if it is nicer to pass the typed configuration as a parameter or part of the context instead of doing this
            var config = context.Account.GetConfiguration<Configuration>();

            var productID = context.SoftwareVersion + "/" + context.CertificateName;
            var softwareCertificateNumber = context.CertificateNumber;

            var taxCountryRegion = audit.Header.Company.StreetAddress.Country;
            var startDate = audit.Header.SelectionCriteria.SelectionStartDate;
            var endDate = audit.Header.SelectionCriteria.SelectionEndDate;

            // TODO(kevin): wtf?
            {
                startDate = new DateTime(startDate.Year, 1, 1);
                endDate = new DateTime(endDate.Year, 12, 31, 23, 59, 59);
            }

            var salesInvoices = new SourceDocumentsSalesInvoices();

            var auditFile = new AuditFile
            {
                Header = new Models.Header
                {
                    AuditFileVersion = AuditFileVersion,
                    CompanyID = audit.Header.Company.RegistrationNumber,
                    TaxRegistrationNumber = audit.Header.Company.TaxRegistration.Number,
                    TaxAccountingBasis = TaxAccountingBasis.F,
                    CompanyName = audit.Header.Company.Name,
                    BusinessName = audit.Header.SoftwareCompanyName,
                    CompanyAddress = new AddressStructurePT
                    {
                        AddressDetail = audit.Header.Company.StreetAddress?.Street + " " + audit.Header.Company.StreetAddress?.Number,
                        City = audit.Header.Company.StreetAddress?.City,
                        PostalCode = audit.Header.Company.StreetAddress?.PostalCode,
                        Region = audit.Header.Company.StreetAddress?.Region,
                        Country = audit.Header.Company.StreetAddress?.Country
                    },
                    FiscalYear = startDate.Year.ToString(),
                    StartDate = startDate,
                    EndDate = endDate,
                    CurrencyCode = audit.Header.DefaultCurrencyCode,
                    DateCreated = DateTime.Today,
                    TaxEntity = TaxEntity,
                    ProductCompanyTaxID = config.ProductCompanyTaxId,
                    SoftwareCertificateNumber = softwareCertificateNumber,
                    ProductID = productID,
                    ProductVersion = context.SoftwareVersion,
                    Telephone = audit.Header.Company.Contact.Telephone,
                    Email = audit.Header.Company.Contact.Email
                },
                MasterFiles = new AuditFileMasterFiles(),
                SourceDocuments = new SourceDocuments
                {
                    SalesInvoices = salesInvoices
                }
            };

            var anonymousAddress = new AddressStructure
            {
                AddressDetail = UnknownLabel,
                City = UnknownLabel,
                PostalCode = UnknownLabel,
                Region = UnknownLabel,
                Country = UnknownLabel
            };

            var anonymous = new Models.Customer
            {
                CustomerID = "0",
                CustomerTaxID = DefaultFiscalID,
                AccountID = UnknownLabel,
                CompanyName = "Consumidor final",
                SelfBillingIndicator = SelfBillingIndicator,
                BillingAddress = anonymousAddress,
                ShipToAddress = new[] { anonymousAddress }
            };

            auditFile.MasterFiles.Customer = audit.MasterFiles.Customers.Select(c => new Models.Customer
            {
                CustomerID = ComputeCustomerID(c.SystemID, c.RegistrationNumber),
                CustomerTaxID = string.IsNullOrEmpty(c.RegistrationNumber) ? DefaultFiscalID : c.RegistrationNumber,
                AccountID = c.BankAccount?.AccountNumber ?? UnknownLabel,
                CompanyName = string.IsNullOrEmpty(c.Name) ? "Consumidor final" : c.Name,
                Email = c.Contact.Email,
                SelfBillingIndicator = SelfBillingIndicator,
                BillingAddress = new AddressStructure
                {
                    AddressDetail = c.BillingAddress?.Street ?? UnknownLabel,
                    City = c.BillingAddress?.City ?? UnknownLabel,
                    PostalCode = c.BillingAddress?.PostalCode ?? UnknownLabel,
                    Region = c.BillingAddress?.Region ?? UnknownLabel,
                    Country = c.BillingAddress?.Country ?? UnknownLabel
                },
                ShipToAddress = new[]
              {
                new AddressStructure
                {
                    AddressDetail = c.ShipToAddress?.Street ?? UnknownLabel,
                    City = c.ShipToAddress?.City ?? UnknownLabel,
                    PostalCode = c.ShipToAddress?.PostalCode ?? UnknownLabel,
                    Region = c.ShipToAddress?.Region ?? UnknownLabel,
                    Country = c.ShipToAddress?.Country ?? UnknownLabel
                }
                }
            }).Concat(new[] { anonymous }).ToArray();

            auditFile.MasterFiles.Product = audit.MasterFiles.Products.Select(p => new Models.Product
            {
                ProductType = p.Type == ProductTypes.Goods ? ProductType.P : ProductType.S,
                ProductCode = p.Code,
                ProductNumberCode = p.Code,
                ProductDescription = p.Description
            }).ToArray();

            auditFile.MasterFiles.TaxTable = audit.MasterFiles.TaxTable.Select(t => new Models.TaxTableEntry
            {
                TaxType = TaxType.IVA,
                TaxCountryRegion = taxCountryRegion,
                TaxCode = t.Details.Single().Code,
                Description = GetTaxPercentage(t.Details.Single().Percentage) + "%",
                Item = GetTaxPercentage(t.Details.Single().Percentage),
                ItemElementName = ItemChoiceType2.TaxPercentage
            }).ToArray();

            salesInvoices.Invoice = audit.SourceDocuments.SalesInvoices.Sources.Select(invoice => new SourceDocumentsSalesInvoicesInvoice
            {
                InvoiceNo = invoice.Number,
                ATCUD = Atcud,
                DocumentStatus = new SourceDocumentsSalesInvoicesInvoiceDocumentStatus
                {
                    InvoiceStatus = InvoiceStatus.N,
                    InvoiceStatusDate = GetDateTime(invoice.Date),
                    SourceID = SourceID,
                    SourceBilling = invoice.IsManual ? SAFTPTSourceBilling.M : SAFTPTSourceBilling.P
                },

                DocumentTotals = new SourceDocumentsSalesInvoicesInvoiceDocumentTotals
                {
                    TaxPayable = Round(invoice.Lines.SelectMany(l => l.Taxes).Sum(t => t.Amount.Value), 2),
                    NetTotal = Round(invoice.Totals.Net, 2),
                    GrossTotal = Round(invoice.Totals.Gross, 2),
                    Settlement = invoice.Settlements?.Select(s => new Models.Settlement
                    {
                        SettlementAmount = Round(s.Amount.Value, 2),
                        SettlementAmountSpecified = true,
                        PaymentTerms = s.Description,
                        SettlementDate = s.Date,
                        SettlementDateSpecified = true
                    }).ToArray()
                },

                Hash = Convert.ToBase64String(invoice.Signature),
                HashControl = GetHashControl(invoice.SignatureKeyVersion, invoice),
                Period = invoice.Date.Month.ToString(),
                InvoiceDate = GetDateTime(invoice.Date),
                InvoiceType = ((InvoiceType?)invoice.Type) ?? InvoiceType.FT,

                SpecialRegimes = new SpecialRegimes
                {
                    SelfBillingIndicator = SelfBillingIndicator,
                    CashVATSchemeIndicator = CashVatSchemeIndicator,
                    ThirdPartiesBillingIndicator = ThirdPartiesBillingIndicator
                },

                SourceID = SourceID,
                SystemEntryDate = GetDateTime(invoice.Date),
                CustomerID = ComputeCustomerID(invoice.Customer.SystemID, invoice.Customer.RegistrationNumber),
                Line = invoice.Lines.Select((line, i) => MapInvoiceLine(invoice, taxCountryRegion, line, i)).ToArray()

            }).ToArray();

            salesInvoices.NumberOfEntries = audit.SourceDocuments.SalesInvoices.Sources.Count.ToString();
            salesInvoices.TotalDebit = Round(audit.SourceDocuments.SalesInvoices.TotalDebitExTax, 2);
            salesInvoices.TotalCredit = Round(audit.SourceDocuments.SalesInvoices.TotalCreditExTax, 2);

            return Task.FromResult(auditFile);
        }

        private static SourceDocumentsSalesInvoicesInvoiceLine MapInvoiceLine(Invoice invoice, string taxCountryRegion, InvoiceLine line, int index)
        {
            var settlement = Round((line.Settlements?.Sum(s => s.Amount.Value) ?? 0), 2);
            var taxDetails = line.Taxes.SingleOrDefault() ?? throw new InvalidOperationException("Expected just one tax detail entry.");

            return new SourceDocumentsSalesInvoicesInvoiceLine
            {
                LineNumber = (index + 1).ToString(),

                ProductCode = line.ProductCode,
                ProductDescription = line.Description,

                Quantity = Math.Abs(line.Quantity),
                UnitOfMeasure = UnitOfMeasure,

                References = line.CreditReferences.Select(c => new References
                {
                    Reason = c.Reason,
                    Reference = c.Reference
                }).ToArray(),

                Description = line.Description,

                UnitPrice = Round(line.UnitPrice, 4),
                Item = Round(line.Amount.Value, 2),
                ItemElementName = line.Amount.Value > 0 ? ItemChoiceType4.CreditAmount : ItemChoiceType4.DebitAmount,

                SettlementAmount = settlement,
                SettlementAmountSpecified = settlement != 0,

                Tax = new Tax
                {
                    TaxType = TaxType.IVA,
                    TaxCountryRegion = taxCountryRegion,
                    TaxCode = taxDetails.Code,
                    Item = GetTaxPercentage(taxDetails.Rate),
                    ItemElementName = ItemChoiceType1.TaxPercentage
                },
                TaxPointDate = GetDateTime(invoice.Date),
                TaxExemptionReason = line.Taxes.FirstOrDefault()?.ExemptionReason,
                TaxExemptionCode = line.Taxes.FirstOrDefault()?.ExemptionCode
            };
        }

        private static int GetTaxPercentage(decimal taxRate) => (int)(100 * (taxRate - 1));

        private static string ComputeCustomerID(string systemID, string registrationNumber)
        {
            if (string.IsNullOrEmpty(systemID))
            {
                return string.IsNullOrEmpty(registrationNumber) ? "0" : $"N{registrationNumber}";
            }

            return systemID;
        }

        private static string GetHashControl(int hashControl, Invoice invoice)
        {
            if (invoice.IsManual)
            {
                return $"{hashControl}-{invoice.Type ?? 0}M {invoice.Number}";
            }

            return hashControl.ToString();
        }

        private static DateTime GetDateTime(DateTime dt) => new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
        private static decimal Round(decimal d, int decimals) => Math.Round(Math.Abs(d), decimals);
    }
}