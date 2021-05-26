using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Vera.Documents.Nodes;
using Vera.Extensions;
using Vera.Models;
using Vera.Thermal;

namespace Vera.Germany
{
    public class ThermalReceiptGenerator : IThermalReceiptGenerator
    {
        private static readonly CultureInfo Culture = CultureInfo.CreateSpecificCulture("de-DE");

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

            //TODO - partial InvoiceReceiptHeader

            yield return new SpacingThermalNode(1);

            yield return new TextThermalNode(invoice.Supplier.Name);
            yield return new TextThermalNode($"{invoice.Supplier.Address.Street} {invoice.Supplier.Address.Number}");
            yield return new TextThermalNode($"{invoice.Supplier.Address.PostalCode} {invoice.Supplier.Address.City} {invoice.Supplier.Address.Country}");
            //TODO - Supplier.PhoneNumber

            yield return new SpacingThermalNode(1);

            if (!string.IsNullOrEmpty(invoice.ReturnedInvoiceNumber))
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

            if (!string.IsNullOrEmpty(invoice.ReturnedInvoiceNumber))
            {
                yield return new TextThermalNode($"Referenzdokument: {invoice.ReturnedInvoiceNumber}");
            }

            yield return new TextThermalNode($"Erstellt am: {invoice.Date:yyyy-MM-dd}");
            yield return new TextThermalNode($"Signiert am: {invoice.Date:HH:mm:ss}");

            yield return new LineThermalNode();

            if (invoice.Customer != null)
            {
                yield return new TextThermalNode($"{invoice.Customer.SystemId} {invoice.Customer.FirstName + " " + invoice.Customer.LastName}");
                yield return new TextThermalNode($"{invoice.Customer.BillingAddress.Street} {invoice.Customer.BillingAddress.Number}");
                yield return new TextThermalNode($"{invoice.Customer.BillingAddress.PostalCode} {invoice.Customer.BillingAddress.City}");
                yield return new TextThermalNode(invoice.Customer.BillingAddress.Country);
            }

            var taxPayerNo = "Endverbraucher";

            if (invoice.Customer != null && !string.IsNullOrEmpty(invoice.Customer.RegistrationNumber))
            {
                taxPayerNo = invoice.Customer.RegistrationNumber;
            }

            yield return new TextThermalNode($"MwSt-Kunde: {taxPayerNo}");
        }

        private IEnumerable<IThermalNode> GenerateInvoiceLines(ThermalReceiptContext context)
        {
            const string format = "{0,-5}{1,-24}{2,7}{3,12}";
            var invoice = context.Invoice;

            var sb = new StringBuilder();
            sb.AppendFormat(
                format,
                "St.",
                "Artikel",
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

                    yield return new TextThermalNode($"Rabatt: {settlement}");
                }
            }

            yield return new SpacingThermalNode(1);

            if (invoice.Settlements != null && invoice.Settlements.Any())
            {
                sb.Clear();

                sb.AppendFormat(
                    format,
                    "",
                    "Rabatt:"
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

            yield return new TextThermalNode($"{lines.Count}/{Math.Abs(lines.Sum(l => l.Quantity))} artikel");
        }

        private IEnumerable<IThermalNode> GenerateTaxLines(ThermalReceiptContext context)
        {
            const string format = "{0,-12}{1,-12}{2,12}{3,12}";

            var totals = context.Invoice.Totals;

            var sb = new StringBuilder();
            sb.AppendFormat(
                format,
                "MwSt",
                "Grundbetrag",
                "MwSt",
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
                    (tax.Base + tax.Value).FormatCurrency(Culture)
                );

                yield return new TextThermalNode(sb.ToString());
            }

            yield return new SpacingThermalNode(1);
            yield return new LineThermalNode();
        }

        private IEnumerable<IThermalNode> GeneratePayments(ThermalReceiptContext context)
        {
            const string format = "{0,-39}{1,9}";

            var payments = context.Invoice.Payments;
            var sb = new StringBuilder();

            yield return new TextThermalNode("Zahlungsmethoden");

            foreach (var payment in payments.Where(p => p.Category != PaymentCategory.Change))
            {
                sb.Clear();

                sb.AppendFormat(format,
                    payment.Category.ToString(),
                    payment.Amount.FormatCurrency(Culture));

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
            sb.AppendFormat(format, "Geschenkkarte", voucher);
            yield return new TextThermalNode(sb.ToString());

            sb.Clear();
            sb.AppendFormat(format, "Rückgeld", change);
            yield return new TextThermalNode(sb.ToString());

            sb.Clear();
            sb.AppendFormat(format, "Rabatt", totalDiscount);
            yield return new TextThermalNode(sb.ToString());

            sb.Clear();
            sb.AppendFormat(format, "Gezahlter Betrag", totalPayments);
            yield return new TextThermalNode(sb.ToString());

            yield return new SpacingThermalNode(1);

            //TODO- print PinReceipts and VoucherActivation

            yield return new LineThermalNode();
        }

        private IEnumerable<IThermalNode> GenerateFooter(ThermalReceiptContext context)
        {
            yield return new TextThermalNode($"Angestellter: {context.Invoice.Employee.SystemId}");
            yield return new TextThermalNode($"KassenID: {context.Invoice.RegisterId}");
            yield return new TextThermalNode(context.SoftwareVersion);
            yield return new TextThermalNode($"TSE signatur: {context.Invoice.Signature.Input}");
            yield return new TextThermalNode($"TSE - Transaktionsnr: {context.Invoice.Number}");
            yield return new TextThermalNode($"TSE - Start: {context.Invoice.Date:yyyy-MM-dd}");
            //TODO - TSE - Finish
            yield return new TextThermalNode($"TSE - Seriennummer: {context.Invoice.Signature.Version}");
            //TODO - TSE - Signaturcount
            //TODO - TSE - Zeitformat
            //TODO - TSE - Algorithmus
            //TODO - TSE - PublicKey

            var signature = Convert.ToBase64String(context.Invoice.Signature.Output);
            yield return new QRCodeThermalNode(signature);

            yield return new SpacingThermalNode(1);

            //TODO - #partial InvoiceReceiptFooter

            yield return new SpacingThermalNode(1);

            foreach (var orderReference in context.Invoice.OrderReferences)
            {
                yield return new BarcodeThermalNode(BarcodeTypes.Code39, orderReference);
            }

            //TODO - OrderID
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
