using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Vera.Documents.Nodes;
using Vera.Models;
using Vera.Thermal;

namespace Vera.Norway
{
    public class ThermalReceiptGenerator : IThermalReceiptGenerator
    {
        private static readonly CultureInfo Culture = CultureInfo.CreateSpecificCulture("no-NO");

        public IThermalNode Generate(ThermalReceiptContext context)
        {
            var nodes = new List<IThermalNode>();

            nodes.AddRange(GenerateHeader(context));
            nodes.Add(new SpacingThermalNode(1));

            nodes.AddRange(GenerateInvoiceLines(context));
            nodes.Add(new SpacingThermalNode(1));

            nodes.AddRange(GenerateTaxLines(context));
            nodes.Add(new SpacingThermalNode(1));

            nodes.AddRange(GenerateTotals(context));
            nodes.Add(new SpacingThermalNode(1));

            nodes.AddRange(GeneratePayments(context));
            nodes.Add(new SpacingThermalNode(1));

            nodes.AddRange(GenerateFooter(context));

            return new DocumentThermalNode(nodes);
        }

        private IEnumerable<IThermalNode> GenerateHeader(ThermalReceiptContext context)
        {
            var invoice = context.Invoice;

            var receiptType = context.Original 
                ? string.IsNullOrEmpty(invoice.ReturnedInvoiceNumber) 
                    ? "Salgskvittering" : "Returkvittering"
                : "KOPI";
            
            yield return new TextThermalNode(receiptType);

            // TODO add {#partial InvoiceReceiptHeader}
            //yield return new SpacingThermalNode(1);

            //if (!string.IsNullOrEmpty(context.HeaderImageMimeType) && context.HeaderImage != null)
            //{
            //    yield return new ImageThermalNode(context.HeaderImageMimeType, context.HeaderImage);
            //}

            //yield return new TextThermalNode(account.Name);
            //yield return new TextThermalNode($"{account.Address.Street} {account.Address.Number}");
            //yield return new TextThermalNode($"{account.Address.PostalCode} {account.Address.City}");
            //yield return new TextThermalNode($"Capital Social: {_socialCapital:N}");
            //yield return new TextThermalNode($"Contribuinte: {account.RegistrationNumber}");

            yield return new TextThermalNode(invoice.Supplier.Name);
            yield return new TextThermalNode($"{invoice.Supplier.Address.Street} {invoice.Supplier.Address.Number}");
            yield return new TextThermalNode($"{invoice.Supplier.Address.PostalCode} {invoice.Supplier.Address.City}");
            yield return new TextThermalNode(invoice.Supplier.Address.Country);

            yield return new SpacingThermalNode(1);

            yield return new TextThermalNode("Foretaksregisteret");
            
            yield return new SpacingThermalNode(1);
            yield return new LineThermalNode();
            yield return new SpacingThermalNode(1);

            if (context.Invoice.Totals.Gross > 0)
            {
                yield return new TextThermalNode($"Kvitteringnummer: {invoice.Number}");
            }
            else
            {
                yield return new TextThermalNode($"Kreditnota: {invoice.Number}");
            }
            
            if (!string.IsNullOrEmpty(invoice.ReturnedInvoiceNumber))
            {
                yield return new TextThermalNode($"Kvitteringnummer: {invoice.ReturnedInvoiceNumber}");
            }

            //TODO use ReturnedOrderReprint or is valid to check ReturnedInvoiceNumber in this context?
            var printType = context.Original
                ? "OPPRINNELIG"
                : string.IsNullOrEmpty(invoice.ReturnedInvoiceNumber) ? "KOPI" : "DOBBELTTRYKK";
            
            yield return new TextThermalNode(printType);

            yield return new TextThermalNode($"Dato: {invoice.Date:dd-MM-yyyy}");
            yield return new TextThermalNode($"Tid: {invoice.Date:HH:mm:ss}");

            var taxPayerNo = "Sluttforbruker";
            if (!string.IsNullOrEmpty(invoice.Customer?.RegistrationNumber))
            {
                taxPayerNo = invoice.Customer.RegistrationNumber;
            }
            else if (!string.IsNullOrEmpty(invoice.Customer?.TaxRegistrationNumber))
            {
                taxPayerNo = invoice.Customer.TaxRegistrationNumber;
            }

            yield return new TextThermalNode($"Kunde: {taxPayerNo}");
        }

        private IEnumerable<IThermalNode> GenerateInvoiceLines(ThermalReceiptContext context)
        {
            const string format = "{0,-5}{1,-24}{2,7}{3,12}";

            var sb = new StringBuilder();
            sb.AppendFormat(
                format,
                "Qt",
                "Artikkel",
                "MVA",
                "VERDI"
            );

            yield return new TextThermalNode(sb.ToString());

            foreach (var line in context.Invoice.Lines)
            {
                sb.Clear();

                sb.AppendFormat(
                    format,
                    line.Quantity.ToString(),
                    line.Description,
                    FormatTaxRate(line.Taxes.Rate),
                    FormatCurrency(line.Gross)
                );

                yield return new TextThermalNode(sb.ToString());

                if (line.Settlements.Any())
                {
                    var settlement = FormatCurrency(line.Settlements.Sum(s => s.Amount));

                    yield return new TextThermalNode($"Rabatter: {settlement}");
                }
            }
        }

        private IEnumerable<IThermalNode> GenerateTaxLines(ThermalReceiptContext context)
        {
            const string format = "{0,-12}{1,-12}{2,12}{3,12}";

            var totals = context.Invoice.Totals;

            var sb = new StringBuilder();
            sb.AppendFormat(
                format,
                "MVA",
                "GRUNNLAG",
                "Q.MVA",
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
                    FormatTaxRate(tax.Rate),
                    FormatCurrency(tax.Base),
                    FormatCurrency(tax.Value),
                    FormatCurrency(tax.Value + tax.Base)
                );

                yield return new TextThermalNode(sb.ToString());
            }

            sb.Clear()
              .AppendFormat(
                  format,
                    "SUBTOTAL",
                    FormatCurrency(totals.Net),
                    FormatCurrency(totals.Gross - totals.Net),
                    FormatCurrency(totals.Gross)
            );

            yield return new TextThermalNode(sb.ToString());
        }

        private IEnumerable<IThermalNode> GenerateTotals(ThermalReceiptContext context)
        {
            var totals = context.Invoice.Totals;
            var prefix = "TOTAL";

            yield return new TextThermalNode($"{prefix}: {FormatCurrency(totals.Gross)}")
            {
                FontSize = FontSize.Large
            };

            var lines = context.Invoice.Lines;

            yield return new TextThermalNode($"{lines.Count}/{Math.Abs(lines.Sum(l => l.Quantity))} articles");

            yield return new SpacingThermalNode(1);
            yield return new LineThermalNode();
        }

        private IEnumerable<IThermalNode> GeneratePayments(ThermalReceiptContext context)
        {
            const string format = "{0,-39}{1,9}";

            yield return new TextThermalNode("BETALINGSMETODER");

            var payments = context.Invoice.Payments;
            var sb = new StringBuilder();

            foreach (var payment in payments.Where(p => p.Category != PaymentCategory.Change))
            {
                if (payment.Category == PaymentCategory.Credit || payment.Category == PaymentCategory.Debit)
                {
                    sb.AppendFormat(format, "BEHANDLET AV ADYEN", FormatCurrency(payment.Amount));
                }
                else
                {
                    sb.AppendFormat(format, payment.Description, FormatCurrency(payment.Amount));
                }

                yield return new TextThermalNode(sb.ToString());

                sb.Clear();
            }

            var change = payments.FirstOrDefault(p => p.Category == PaymentCategory.Change);

            if (change != null)
            {
                sb.AppendFormat(
                    format,
                    "Endring",
                    FormatCurrency(change.Amount)
                );

                yield return new TextThermalNode(sb.ToString());
            }

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
            yield return new TextThermalNode($"Operatør: {context.Invoice.Employee.SystemId}");
            yield return new TextThermalNode($"Kassapunkt ID: {context.Invoice.TerminalId}");

            yield return new TextThermalNode(context.SoftwareVersion);

            yield return new TextThermalNode("enhetspriser inkl.mva");

            yield return new QRCodeThermalNode(Convert.ToBase64String(context.Invoice.Signature.Output));
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

        private static string FormatCurrency(decimal d) => Math.Abs(d).ToString("C", Culture);
        private static string FormatTaxRate(decimal r) => (r - 1).ToString("P0", Culture);
    }
}
