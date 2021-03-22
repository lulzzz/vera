using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Vera.Documents.Nodes;
using Vera.Extensions;
using Vera.Models;
using Vera.Thermal;

namespace Vera.Austria
{
    public class ThermalReceiptGenerator : IThermalReceiptGenerator
    {
        private static readonly CultureInfo Culture = CultureInfo.CreateSpecificCulture("de-DE");

        private readonly string _certificateNumber;

        public ThermalReceiptGenerator(string certificateNumber)
        {
            _certificateNumber = certificateNumber;
        }

        public IThermalNode Generate(ThermalReceiptContext context)
        {
            var nodes = new List<IThermalNode>();

            nodes.AddRange(GenerateHeader(context));
            nodes.Add(new SpacingThermalNode(1));

            nodes.AddRange(GenerateInvoiceLines(context));
            nodes.Add(new SpacingThermalNode(1));

            nodes.AddRange(GenerateTotals(context));
            nodes.Add(new SpacingThermalNode(1));

            nodes.AddRange(GenerateTaxLines(context));
            nodes.Add(new SpacingThermalNode(1));

            nodes.AddRange(GeneratePayments(context));
            nodes.Add(new SpacingThermalNode(1));

            nodes.AddRange(GenerateFooter(context));

            return new DocumentThermalNode(nodes);
        }

        private IEnumerable<IThermalNode> GenerateHeader(ThermalReceiptContext context)
        {
            var account = context.Account;
            var invoice = context.Invoice;
            var returnInvoiceNumber = invoice.Lines == null ? null : invoice.Lines.Select(line => line.CreditReference?.Number).FirstOrDefault();

            // TODO add {#partial InvoiceReceiptHeader}
            //if (!string.IsNullOrEmpty(context.HeaderImageMimeType) && context.HeaderImage != null)
            //{
            //    yield return new ImageThermalNode(context.HeaderImageMimeType, context.HeaderImage);
            //}

            yield return new SpacingThermalNode(1);

            yield return new TextThermalNode(invoice.Supplier.Name);
            yield return new TextThermalNode($"{invoice.Supplier.Address.Street} {invoice.Supplier.Address.Number}");
            yield return new TextThermalNode($"{invoice.Supplier.Address.PostalCode} {invoice.Supplier.Address.City}");
            yield return new TextThermalNode(invoice.Supplier.Address.Country);

            yield return new SpacingThermalNode(1);


            //handle different doc types
            if (!string.IsNullOrEmpty(returnInvoiceNumber))
            {
                yield return new TextThermalNode("RÜCKSCHEIN");
            }
            else
            {
                yield return new TextThermalNode("KAUFBELEG");
            }

            yield return new SpacingThermalNode(1);
            yield return new LineThermalNode();
            yield return new SpacingThermalNode(1);

            yield return new TextThermalNode($"Belegnummer {invoice.Number}");

            var invoiceType = context.Original ? "Original" : "Dublicate";
            yield return new TextThermalNode(invoiceType);

            if (!string.IsNullOrEmpty(returnInvoiceNumber))
            {
                yield return new TextThermalNode($"Referenzdokument: {returnInvoiceNumber}");
            }

            yield return new TextThermalNode($"Datum: {invoice.Date:yyyy-MM-dd}");
            yield return new TextThermalNode($"Zeit: {invoice.Date:HH:mm:ss}");

            if (invoice.Customer != null)
            {
                yield return new TextThermalNode(invoice.Customer.FirstName + " " + invoice.Customer.LastName);

                yield return new TextThermalNode($"{invoice.Customer.BillingAddress.Street} {invoice.Customer.BillingAddress.Number}");
                yield return new TextThermalNode($"{invoice.Customer.BillingAddress.PostalCode} {invoice.Customer.BillingAddress.City}");
                yield return new TextThermalNode(invoice.Customer.BillingAddress.Country);
            }

            var taxPayerNo = "Endverbraucher";

            if (!string.IsNullOrEmpty(invoice.Customer?.RegistrationNumber))
            {
                taxPayerNo = invoice.Customer.RegistrationNumber;
            }
            else if (!string.IsNullOrEmpty(invoice.Customer?.TaxRegistrationNumber))
            {
                taxPayerNo = invoice.Customer.TaxRegistrationNumber;
            }

            yield return new TextThermalNode($"MwSt-cliente: {taxPayerNo}");
        }

        private IEnumerable<IThermalNode> GenerateInvoiceLines(ThermalReceiptContext context)
        {
            const string format = "{0,-5}{1,-24}{2,7}{3,12}";
            var invoice = context.Invoice;

            var sb = new StringBuilder();
            sb.AppendFormat(
                format,
                "Qt",
                "Artikkel",
                "MwSt",
                "Wert"
            );

            yield return new TextThermalNode(sb.ToString());

            foreach (var line in invoice.Lines)
            {
                sb.Clear();

                sb.AppendFormat(
                    format,
                    line.Quantity.ToString(),
                    line.Description,
                    line.Taxes.Rate.FormatTaxRate(Culture),
                    line.Gross.FormatCurrency(Culture)
                );

                yield return new TextThermalNode(sb.ToString());

                if (line.Settlements != null && line.Settlements.Any())
                {
                    var settlement = line.Settlements.Sum(s => s.Amount).FormatCurrency(Culture);

                    yield return new TextThermalNode($"Rabatter: {settlement}");
                }
            }

            yield return new SpacingThermalNode(1);

            //invoice level discounts
            if (invoice.Settlements != null && invoice.Settlements.Any())
            {
                sb.Clear();

                sb.AppendFormat(
                    format,
                    "",
                    "Rabatter:"
                );

                foreach (var settlement in invoice.Settlements)
                {
                    sb.Clear();

                    sb.AppendFormat(
                        format,
                        "",
                        settlement.Description,
                        "",
                        settlement.Amount.FormatCurrency(Culture)
                    );

                    yield return new TextThermalNode(sb.ToString());
                }
            }
        }

        private IEnumerable<IThermalNode> GenerateTotals(ThermalReceiptContext context)
        {
            var totals = context.Invoice.Totals;
            var prefix = "GESAMTSUMME";

            yield return new TextThermalNode($"{prefix}: {totals.Gross.FormatCurrency(Culture)}")
            {
                FontSize = FontSize.Large
            };

            var lines = context.Invoice.Lines;

            yield return new TextThermalNode($"{lines.Count}/{Math.Abs(lines.Sum(l => l.Quantity))} articles");
        }

        private IEnumerable<IThermalNode> GenerateTaxLines(ThermalReceiptContext context)
        {
            const string format = "{0,-12}{1,-12}{2,12}{3,12}";

            var totals = context.Invoice.Totals;

            var sb = new StringBuilder();
            sb.AppendFormat(
                format,
                "MwSt",
                "Grundlage",
                "Q.MwSt",
                "TOTAL"
            );

            yield return new TextThermalNode(sb.ToString());

            var taxes = new[]
            {
                totals.Taxes.High,
                totals.Taxes.Intermediate,
                totals.Taxes.Low,
                totals.Taxes.Zero,
                totals.Taxes.Exempt
            };

            foreach (var tax in taxes)
            {
                if (tax == null) continue;

                sb.Clear();

                sb.AppendFormat(
                    format,
                    tax.Rate.FormatTaxRate(Culture),
                    tax.Base.FormatCurrency(Culture),
                    tax.Value.FormatCurrency(Culture),
                    (tax.Value + tax.Base).FormatCurrency(Culture)
                );

                yield return new TextThermalNode(sb.ToString());
            }

            yield return new SpacingThermalNode(1);
            yield return new LineThermalNode();
        }

        private IEnumerable<IThermalNode> GeneratePayments(ThermalReceiptContext context)
        {
            const string format = "{0,-39}{1,9}";

            yield return new TextThermalNode("Zahlungsmethoden");

            var payments = context.Invoice.Payments;
            var sb = new StringBuilder();

            foreach (var payment in payments.Where(p => p.Category != PaymentCategory.Change))
            {
                sb.Clear();

                //TODO use method instead of category when it is available
                sb.AppendFormat(format, payment.Category.ToString(), payment.Amount.FormatCurrency(Culture));

                yield return new TextThermalNode(sb.ToString());
            }

            var cash = payments.Aggregate(0m, (total, p) => 
                p.Category == PaymentCategory.Cash ? total += p.Amount : total);
            var card = payments.Aggregate(0m, (total, p) => 
                p.Category == PaymentCategory.Credit || p.Category == PaymentCategory.Debit ? total += p.Amount : total);
            var voucher = payments.Aggregate(0m, (total, p) => 
                p.Category == PaymentCategory.Voucher ? total += p.Amount : total);
            var change = payments.Aggregate(0m, (total, p) =>
                p.Category == PaymentCategory.Change ? total += p.Amount : total);
            var totalDiscount = CalculateTotalDiscount(context.Invoice);
            var totalPayments = cash + card + voucher;

            sb.Clear();
            sb.AppendFormat(format, "Barzahlung", cash);
            yield return new TextThermalNode(sb.ToString());

            sb.Clear();
            sb.AppendFormat(format, "Elektronische Zahlung", card);
            yield return new TextThermalNode(sb.ToString());

            sb.Clear();
            sb.AppendFormat(format, "Vouchers", voucher);
            yield return new TextThermalNode(sb.ToString());

            sb.Clear();
            sb.AppendFormat(format, "Resto", change);
            yield return new TextThermalNode(sb.ToString());

            sb.Clear();
            sb.AppendFormat(format, "Rabatt", totalDiscount);
            yield return new TextThermalNode(sb.ToString());

            sb.Clear();
            sb.AppendFormat(format, "Gezahlter Betrag", totalPayments);
            yield return new TextThermalNode(sb.ToString());

            yield return new SpacingThermalNode(1);

            yield return new TextThermalNode("KUNDEN KOPIEREN");
            // TODO PRINT PinReceipts, and VoucherActivation
            // < scope align = "center" font = "FontB" linespacing = "40" >
            //{ { for PinReceipts} }
            //</ scope >

            //< feed amount = "55" />


            // < placeholder name = "VoucherActivation" />
            yield return new LineThermalNode();
        }

        private IEnumerable<IThermalNode> GenerateFooter(ThermalReceiptContext context)
        {
            yield return new TextThermalNode($"Angestellter: {context.Invoice.Employee.SystemId}");
            yield return new TextThermalNode($"KassenID: {context.Invoice.TerminalId}");
            yield return new TextThermalNode(context.SoftwareVersion);

            yield return new SpacingThermalNode(1);

            yield return new TextThermalNode($"Verarbeitet fur A-Trust {_certificateNumber}");

            yield return new SpacingThermalNode(1);

            yield return new TextThermalNode("Stückpreis inkl. MwSt");

            yield return new SpacingThermalNode(1);

            var signature = Convert.ToBase64String(context.Invoice.Signature.Output);
            yield return new QRCodeThermalNode(signature);

            //check signature state?
            //if signature state
            //    yield return new TextThermalNode("Sicherheitseinrichtung Ausgefallen");
            yield return new SpacingThermalNode(1);

            // partial InvoiceReceiptFooter
            //if (context.Footer != null)
            //{
            //    foreach (var line in context.Footer)
            //    {
            //        yield return new TextThermalNode(line);
            //    }
            //}

            foreach (var orderReference in context.Invoice.OrderReferences)
            {
                yield return new BarcodeThermalNode(BarcodeTypes.Code39, orderReference);
            }
            // TODO OrderId?
        }

        private decimal CalculateTotalDiscount(Invoice invoice)
        {
            var invoiceDiscount = invoice.Settlements != null ? invoice.Settlements.Sum(s => s.Amount) : 0m;
            var lines = invoice.Lines;
            var linesDiscount = 0m;
            if (lines != null)
            {
                foreach (var line in lines)
                {
                    if (line.Settlements != null)
                    {
                        linesDiscount += line.Settlements.Sum(s => s.Amount);
                    }
                }
            }
            return invoiceDiscount + linesDiscount;
        }
    }
}
