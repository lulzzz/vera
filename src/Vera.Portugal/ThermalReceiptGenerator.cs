using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Vera.Documents;
using Vera.Documents.Nodes;
using Vera.Models;

namespace Vera.Portugal
{
    public class ThermalReceiptGenerator : IThermalReceiptGenerator
    {
        private static readonly CultureInfo Culture = CultureInfo.CreateSpecificCulture("pt-PT");

        private readonly decimal _shareCapital;

        public ThermalReceiptGenerator(decimal shareCapital)
        {
            _shareCapital = shareCapital;
        }

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
            var account = context.Account;
            var invoice = context.Invoice;

            if (!string.IsNullOrEmpty(context.HeaderImageMimeType) && context.HeaderImage != null)
            {
                yield return new ImageThermalNode(context.HeaderImageMimeType, context.HeaderImage);
            }

            yield return new TextThermalNode(account.Name);
            yield return new TextThermalNode($"{account.Address.Street} {account.Address.Number}");
            yield return new TextThermalNode($"{account.Address.PostalCode} {account.Address.City}");
            yield return new TextThermalNode($"Capital Social: {_shareCapital:N}");
            yield return new TextThermalNode($"Contribuinte: {account.RegistrationNumber}");

            yield return new SpacingThermalNode(1);

            yield return new TextThermalNode(invoice.Supplier.Name);
            yield return new TextThermalNode($"{invoice.Supplier.Address.Street} {invoice.Supplier.Address.Number}");
            yield return new TextThermalNode($"{invoice.Supplier.Address.PostalCode} {invoice.Supplier.Address.City}");
            yield return new TextThermalNode(invoice.Supplier.Address.Country);

            yield return new SpacingThermalNode(1);

            if (context.Totals.AmountInTax > 0)
            {
                yield return new TextThermalNode("FATURA SIMPLIFICADA")
                {
                    Bold = true,
                    FontSize = FontSize.Large
                };

                yield return new TextThermalNode($"Fatura: {invoice.Number}");
            }
            else
            {
                yield return new TextThermalNode("NOTA DE CRÉDITO")
                {
                    Bold = true,
                    FontSize = FontSize.Large
                };

                yield return new TextThermalNode($"Nota de crédito: {invoice.Number}");
            }

            yield return new SpacingThermalNode(1);
            yield return new LineThermalNode();
            yield return new SpacingThermalNode(1);

            if (!string.IsNullOrEmpty(invoice.ReturnedInvoiceNumber))
            {
                yield return new TextThermalNode($"Fatura de referencia: {invoice.ReturnedInvoiceNumber}");
            }

            if (invoice.Manual)
            {
                yield return new TextThermalNode($"Cópia do documento original -FTM {invoice.Number}");
            }

            string printNumberPrefix;

            if (string.IsNullOrEmpty(invoice.ReturnedInvoiceNumber)) {
                printNumberPrefix = invoice.Prints.Count == 0 ? "ORIGINAL" : $"DUPLICADO #{invoice.Prints.Count}";
            } else {
                printNumberPrefix = $"DUPLICADO REIMPRESSAO #{invoice.Prints.Count}";
            }

            yield return new TextThermalNode($"{printNumberPrefix}");

            if (invoice.Customer != null)
            {
                yield return new TextThermalNode(invoice.Customer.FirstName + " " + invoice.Customer.LastName);
                yield return new TextThermalNode(invoice.Customer.RegistrationNumber);
                yield return new TextThermalNode(invoice.Customer.TaxRegistrationNumber);

                yield return new TextThermalNode($"{invoice.Customer.Address.Street} {invoice.Customer.Address.Number}");
                yield return new TextThermalNode($"{invoice.Customer.Address.PostalCode} {invoice.Customer.Address.City}");
                yield return new TextThermalNode(invoice.Customer.Address.Country);
            }

            yield return new TextThermalNode($"Data: {invoice.Date:yyyy-MM-dd}");
            yield return new TextThermalNode($"Hora: {invoice.Date:HH:mm:ss}");

            var taxPayerNo = "Consumidor Final";

            if (!string.IsNullOrEmpty(invoice.Customer?.RegistrationNumber))
            {
                taxPayerNo = invoice.Customer.RegistrationNumber;
            }
            else if (!string.IsNullOrEmpty(invoice.Customer?.TaxRegistrationNumber))
            {
                taxPayerNo = invoice.Customer.TaxRegistrationNumber;
            }

            yield return new TextThermalNode($"Contribuinte: {taxPayerNo}");
        }

        private IEnumerable<IThermalNode> GenerateInvoiceLines(ThermalReceiptContext context)
        {
            const string format = "{0,5}{1,32}{2,7}{3,10}";

            var sb = new StringBuilder();
            sb.AppendFormat(
                format, 
                "Qt".PadRight(5), 
                "Artigo".PadRight(32), 
                "IVA".PadLeft(7), 
                "VALOR".PadLeft(10)
            );

            yield return new TextThermalNode(sb.ToString());

            foreach (var line in context.Invoice.Lines)
            {
                sb.Clear();

                sb.AppendFormat(
                    format, 
                    line.Quantity.ToString().PadRight(5), 
                    line.Description.PadRight(32), 
                    FormatTaxRate(line.TaxRate).PadLeft(7),
                    FormatCurrency(line.AmountInTax).PadLeft(10)
                );

                yield return new TextThermalNode(sb.ToString());

                if (line.Settlements.Any())
                {
                    var settlement = FormatCurrency(line.Settlements.Sum(s => s.Amount));

                    yield return new TextThermalNode($"DESCONTOS: {settlement}");
                }

                // TODO(kevin): tax exemption
            }
        }

        private IEnumerable<IThermalNode> GenerateTaxLines(ThermalReceiptContext context)
        {
            const string format = "{0,19}{1,20}{2,6}{3,8}";

            var totals = context.Totals;

            var sb = new StringBuilder();
            sb.AppendFormat(
                format,
                "Taxa".PadRight(19),
                "Base".PadRight(20),
                "Q.IVA".PadLeft(6),
                "Total".PadLeft(9)
            );

            yield return new TextThermalNode(sb.ToString());

            foreach (var tax in totals.Taxes)
            {
                sb.Clear();

                sb.AppendFormat(
                    format,
                    FormatTaxRate(tax.Rate).PadRight(19),
                    FormatCurrency(tax.Base).PadRight(20),
                    FormatCurrency(tax.Amount).PadLeft(6),
                    FormatCurrency(tax.Amount + tax.Base).PadLeft(9)
                );

                yield return new TextThermalNode(sb.ToString());
            }

            sb.Clear()
              .AppendFormat(
                  format,
                    "SUBTOTAL".PadRight(19),
                    FormatCurrency(totals.Amount).PadRight(20),
                    FormatCurrency(totals.AmountInTax - totals.Amount).PadLeft(6),
                    FormatCurrency(totals.AmountInTax).PadLeft(9)
            );

            yield return new TextThermalNode(sb.ToString());
        }

        private IEnumerable<IThermalNode> GenerateTotals(ThermalReceiptContext context)
        {
            var totals = context.Totals;
            var prefix = totals.AmountInTax >= 0 ? "TOTAL FATURA" : "TROCO";

            yield return new TextThermalNode($"{prefix}: {FormatCurrency(totals.AmountInTax)}")
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
            const string format = "{0,45}{1,9}";

            yield return new TextThermalNode("MODO DE PAGAMENTO");

            var payments = context.Invoice.Payments;
            var sb = new StringBuilder();

            foreach (var payment in payments.Where(p => p.Category != PaymentCategory.Change))
            {
                if (payment.Category == PaymentCategory.Credit || payment.Category == PaymentCategory.Debit)
                {
                    // TOOD(kevin): not always Adyen, what to do?
                    sb.AppendFormat(format, "PROCESSADO POR ADYEN", FormatCurrency(payment.Amount).PadLeft(10));

                    yield return new TextThermalNode("COPIA CLIENTE");
                }
                else
                {
                    sb.AppendFormat(
                        format, 
                        payment.Description.PadRight(45), 
                        FormatCurrency(payment.Amount).PadLeft(9)
                    );
                }

                yield return new TextThermalNode(sb.ToString());

                sb.Clear();
            }

            var change = payments.FirstOrDefault(p => p.Category == PaymentCategory.Change);

            if (change != null)
            {
                sb.AppendFormat(
                    format,
                    "Troco",
                    FormatCurrency(change.Amount).PadLeft(10)
                );

                yield return new TextThermalNode(sb.ToString());
            }

            var receipts = context.Invoice.Receipts;
            if (receipts.Count != 0)
            {
                yield return new SpacingThermalNode(1);

                // Output credit/debit receipt
                foreach (var receipt in receipts)
                {
                    foreach (var line in receipt.Lines)
                    {
                        yield return new TextThermalNode(line);
                    }

                    if (receipt.SignatureData != null)
                    {
                        yield return new ImageThermalNode(receipt.SignatureMimeType, receipt.SignatureData);
                    }
                }                

                yield return new SpacingThermalNode(1);
            }            
        }

        private IEnumerable<IThermalNode> GenerateFooter(ThermalReceiptContext context)
        {
            if (!string.IsNullOrEmpty(context.Invoice.Remark))
            {
                yield return new TextThermalNode($"COMENTARIO: {context.Invoice.Remark}");
            }

            yield return new TextThermalNode($"Atendido por: {context.Invoice.Employee.SystemID}");
            yield return new TextThermalNode($"Loja: {context.Invoice.TerminalId}");

            yield return new TextThermalNode(context.SoftwareVersion);

            yield return new TextThermalNode($"certificado no {context.CertificateNumber}/AT");
            yield return new TextThermalNode("preços unitários com iva incluido");

            yield return new QRCodeThermalNode(Convert.ToBase64String(context.Invoice.Signature));
            yield return new SpacingThermalNode(1);

            if (context.Footer != null)
            {
                foreach (var line in context.Footer)
                {
                    yield return new TextThermalNode(line);
                }
            }

            foreach (var orderReference in context.Invoice.OrderReferences)
            {
                yield return new BarcodeThermalNode("code39", orderReference);
            }

            yield return new LineThermalNode();

            yield return new TextThermalNode($"LICENCIADO A: {context.CertificateName}");
        }

        private static string FormatCurrency(decimal d) => Math.Abs(d).ToString("C", Culture);
        private static string FormatTaxRate(decimal r) => (r - 1).ToString("P0", Culture);
    }
}