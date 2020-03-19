using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Vera.Documents;
using Vera.Documents.Nodes;
using Vera.Invoices;
using Vera.Models;

namespace Vera.Portugal
{
    // TODO(kevin): header, footer and logo from account config
    public class ThermalReceiptGenerator : IThermalReceiptGenerator
    {
        private static readonly CultureInfo Culture = CultureInfo.CreateSpecificCulture("pt-PT");

        private readonly IInvoiceTotalCalculator _invoiceTotalCalculator;

        public ThermalReceiptGenerator(IInvoiceTotalCalculator invoiceTotalCalculator)
        {
            _invoiceTotalCalculator = invoiceTotalCalculator;
        }

        public IThermalNode Generate(ThermalReceiptContext context, Invoice model)
        {
            var totals = _invoiceTotalCalculator.Calculate(model);

            var nodes = new List<IThermalNode>();
            
            nodes.AddRange(GenerateHeader(model, totals, context.Account));
            nodes.Add(new SpacingThermalNode(1));

            nodes.AddRange(GenerateInvoiceLines(model));
            nodes.Add(new SpacingThermalNode(1));

            nodes.AddRange(GenerateTaxLines(totals));
            nodes.Add(new SpacingThermalNode(1));

            nodes.AddRange(GenerateFooter(model, totals));

            return new DocumentThermalNode(nodes);
        }

        private IEnumerable<IThermalNode> GenerateHeader(Invoice model, Totals totals, Account account)
        {
            // TODO(kevin): static receipt header (logo only so far)

            yield return new TextThermalNode(account.Name);
            yield return new TextThermalNode($"{account.Address.Street} {account.Address.Number}");
            yield return new TextThermalNode($"{account.Address.PostalCode} {account.Address.City}");
            // TODO(kevin): missing data for Capital Social: 258.501,00 (check what this ist)
            yield return new TextThermalNode($"Contribuinte: {account.RegistrationNumber}");

            yield return new SpacingThermalNode(1);

            yield return new TextThermalNode(model.Supplier.Name);
            yield return new TextThermalNode($"{model.Supplier.Address.Street} {model.Supplier.Address.Number}");
            yield return new TextThermalNode($"{model.Supplier.Address.PostalCode} {model.Supplier.Address.City}");
            yield return new TextThermalNode(model.Supplier.Address.Country);

            yield return new SpacingThermalNode(1);

            if (totals.AmountInTax > 0)
            {
                yield return new TextThermalNode("FATURA SIMPLIFICADA")
                {
                    Bold = true,
                    FontSize = FontSize.Large
                };

                yield return new TextThermalNode($"Fatura: {model.Number}");
            }
            else
            {
                yield return new TextThermalNode("NOTA DE CRÉDITO")
                {
                    Bold = true,
                    FontSize = FontSize.Large
                };

                yield return new TextThermalNode($"Nota de crédito: {model.Number}");
            }

            yield return new SpacingThermalNode(1);
            yield return new LineThermalNode();
            yield return new SpacingThermalNode(1);

            if (!string.IsNullOrEmpty(model.ReturnedInvoiceNumber))
            {
                yield return new TextThermalNode($"Fatura de referencia: {model.ReturnedInvoiceNumber}");
            }

            if (model.Manual)
            {
                yield return new TextThermalNode($"Cópia do documento original -FTM {model.Number}");
            }

            string printNumberPrefix;

            if (string.IsNullOrEmpty(model.ReturnedInvoiceNumber)) {
                printNumberPrefix = model.Prints.Count == 0 ? "ORIGINAL" : $"DUPLICADO #{model.Prints.Count}";
            } else {
                printNumberPrefix = $"DUPLICADO REIMPRESSAO #{model.Prints.Count}";
            }

            yield return new TextThermalNode($"{printNumberPrefix}");

            if (model.Customer != null)
            {
                yield return new TextThermalNode(model.Customer.FirstName + " " + model.Customer.LastName);
                yield return new TextThermalNode(model.Customer.RegistrationNumber);
                yield return new TextThermalNode(model.Customer.TaxRegistrationNumber);

                yield return new TextThermalNode($"{model.Customer.Address.Street} {model.Customer.Address.Number}");
                yield return new TextThermalNode($"{model.Customer.Address.PostalCode} {model.Customer.Address.City}");
                yield return new TextThermalNode(model.Customer.Address.Country);
            }

            yield return new TextThermalNode($"Data: {model.Date:yyyy-MM-dd}");
            yield return new TextThermalNode($"Hora: {model.Date:HH:mm:ss}");

            var taxPayerNo = "Consumidor Final";

            if (!string.IsNullOrEmpty(model.Customer?.RegistrationNumber))
            {
                taxPayerNo = model.Customer.RegistrationNumber;
            }
            else if (!string.IsNullOrEmpty(model.Customer?.TaxRegistrationNumber))
            {
                taxPayerNo = model.Customer.TaxRegistrationNumber;
            }

            yield return new TextThermalNode($"Contribuinte: {taxPayerNo}");
        }

        private IEnumerable<IThermalNode> GenerateInvoiceLines(Invoice model)
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

            foreach (var line in model.Lines)
            {
                sb.Clear();

                sb.AppendFormat(
                    format, 
                    line.Quantity.ToString().PadRight(5), 
                    line.Description.PadRight(32), 
                    (line.TaxRate - 1).ToString("P", Culture).PadLeft(7),
                    line.AmountInTax.ToString("C", Culture).PadLeft(10)
                );

                yield return new TextThermalNode(sb.ToString());

                if (line.Settlements.Any())
                {
                    var settlement = line.Settlements.Sum(s => s.Amount).ToString("C", Culture);

                    yield return new TextThermalNode($"DESCONTOS: {settlement}");
                }

                // TODO(kevin): tax exemption
            }
        }

        private IEnumerable<IThermalNode> GenerateTaxLines(Totals totals)
        {
            const string format = "{0,19}{1,20}{2,6}{3,8}";

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
                    (tax.Rate - 1).ToString("P", Culture).PadRight(19),
                    tax.Base.ToString("C", Culture).PadRight(20),
                    tax.Amount.ToString("C", Culture).PadLeft(6),
                    (tax.Amount + tax.Base).ToString("C", Culture).PadLeft(9)
                );

                yield return new TextThermalNode(sb.ToString());
            }

            sb.Clear();

            sb.AppendFormat(
                format,
                "SUBTOTAL".PadRight(19),
                totals.Amount.ToString("C", Culture).PadLeft(20),
                (totals.AmountInTax - totals.Amount).ToString("C", Culture).PadLeft(6),
                totals.AmountInTax.ToString("C", Culture).PadLeft(8)
            );
        }

        private IEnumerable<IThermalNode> GeneratePayments(Invoice model)
        {
            const string format = "{0,45}{1,10}";

            yield return new TextThermalNode("MODO DE PAGAMENTO");

            var sb = new StringBuilder();
            foreach (var payment in model.Payments.Where(p => p.Category != PaymentCategory.Change))
            {
                if (payment.Category == PaymentCategory.Credit || payment.Category == PaymentCategory.Debit)
                {
                    // TOOD(kevin): not always Adyen, what to do?
                    sb.AppendFormat(format, "PROCESSADO POR ADYEN", payment.Amount.ToString("C", Culture).PadLeft(10));

                    yield return new TextThermalNode("COPIA CLIENTE");
                }
                else
                {
                    sb.AppendFormat(
                        format, 
                        payment.Description.PadRight(45), 
                        payment.Amount.ToString("C", Culture).PadLeft(10)
                    );
                }

                yield return new TextThermalNode(sb.ToString());

                sb.Clear();
            }

            var change = model.Payments.FirstOrDefault(p => p.Category == PaymentCategory.Change);

            if (change != null)
            {
                sb.AppendFormat(
                    format,
                    "Troco",
                    change.Amount.ToString("C", Culture).PadLeft(10)
                );

                yield return new TextThermalNode(sb.ToString());
            }
        }

        private IEnumerable<IThermalNode> GenerateFooter(Invoice model, Totals totals)
        {
            var prefix = totals.AmountInTax >= 0 ? "TOTAL FATURA" : "TROCO";

            yield return new TextThermalNode($"{prefix}: {totals.AmountInTax.ToString("C", Culture)}")
            {
                FontSize = FontSize.Large
            };

            yield return new TextThermalNode($"{model.Lines.Count}/{Math.Abs(model.Lines.Sum(l => l.Quantity))} articles");
            
            yield return new SpacingThermalNode(1);
            yield return new LineThermalNode();

            foreach (var node in GeneratePayments(model))
            {
                yield return node;
            }

            
            if (model.Receipts.Count != 0)
            {
                yield return new SpacingThermalNode(1);

                // Output credit/debit receipt
                foreach (var receipt in model.Receipts)
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

            if (!string.IsNullOrEmpty(model.Remark))
            {
                yield return new TextThermalNode($"COMENTARIO: {model.Remark}");
            }

            yield return new TextThermalNode($"Atendido por: {model.Employee.SystemID}");
            yield return new TextThermalNode($"Loja: {model.TerminalId}");

            // TODO(kevin): get software version from account config
            yield return new TextThermalNode("EVA Unified Commerce version 2.0");

            // TODO(kevin): get certificate number from account config
            yield return new TextThermalNode($"certificado no /AT");
            yield return new TextThermalNode("preços unitários com iva incluido");

            yield return new QRCodeThermalNode(Convert.ToBase64String(model.Signature));
            yield return new SpacingThermalNode(1);

            // TODO(kevin): static footer (from config?)
            // {#partial InvoiceReceiptFooter}

            foreach (var orderReference in model.OrderReferences)
            {
                yield return new BarcodeThermalNode("code39", orderReference);
            }

            yield return new LineThermalNode();

            // TODO(kevin): get certificate name from account config
            yield return new TextThermalNode($"LICENCIADO A: ");
        }
    }
}