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
            nodes.AddRange(GenerateInvoiceLines(model));
            nodes.AddRange(GenerateTaxLines(totals));
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

            // TODO(kevin): fix this piece below
            // {{if ReturnedInvoiceNumber}}
            //     Fatura de referencia: {{:ReturnedInvoiceNumber}}
            // {{/if}

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

            yield return new TextThermalNode($"Data: {model.Date:YYYY-MM-DD}");
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
            var sb = new StringBuilder();
            sb.Append("Qt".PadRight(5));
            sb.Append("Artigo".PadRight(34));
            sb.Append("IVA".PadLeft(5));
            sb.Append("VALOR".PadLeft(10));

            yield return new TextThermalNode(sb.ToString());

            foreach (var line in model.Lines)
            {
                sb.Clear();

                sb.Append(line.Quantity.ToString().PadRight(5));
                sb.Append(line.Description.PadRight(34).Substring(0, 34));
                sb.Append((line.TaxRate - 1).ToString("P", Culture));
                sb.Append(line.AmountInTax.ToString("C", Culture));

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
            var sb = new StringBuilder();
            sb.Append("Taxa".PadRight(19));
            sb.Append("Base".PadRight(20));
            sb.Append("Q.IVA".PadLeft(6));
            sb.Append("Total".PadLeft(8));

            yield return new TextThermalNode(sb.ToString());

            foreach (var tax in totals.Taxes)
            {
                sb.Clear();

                sb.Append((tax.Rate - 1).ToString("P", Culture).PadRight(19));
                sb.Append(tax.Base.ToString("C", Culture).PadRight(20));
                sb.Append(tax.Amount.ToString("C", Culture).PadLeft(6));
                sb.Append((tax.Amount + tax.Base).ToString("C", Culture).PadLeft(8));

                yield return new TextThermalNode(sb.ToString());
            }

            sb.Clear();

            sb.Append("SUBTOTAL".PadRight(19));
            sb.Append(totals.Amount.ToString("C", Culture));
            sb.Append((totals.AmountInTax - totals.Amount).ToString("C", Culture));
            sb.Append(totals.AmountInTax.ToString("C", Culture));
        }

        private IEnumerable<IThermalNode> GeneratePayments(Invoice model)
        {
            yield return new TextThermalNode("MODO DE PAGAMENTO");

            var sb = new StringBuilder();
            foreach (var payment in model.Payments)
            {
                // TODO(kevin): exception for PIN

                sb.Append(payment.Description.Substring(0, 46));
                sb.Append(payment.Amount.ToString("C", Culture).PadLeft(10));

                yield return new TextThermalNode(sb.ToString());

                sb.Clear();
            }

            // TODO(kevin): print "change" somewhere

            //     <grid positions="0,46">
            // {{for Payments}}
            // {{if Description == "PIN"}}
            // <row>
            //     <col>COPIA CLIENTE</col>
            //     </row>
            //     <row>
            //     <col>PROCESSADO POR ADYEN</col>
            //     <col width="10" align="right">{{:~currencyAbs(Amount, ~root.CurrencyID)}}</col>
            //     </row>
            // {{else}}
            // <row>
            //     <col>{{>Description}}</col>
            //     <col width="10" align="right">{{:~currencyAbs(Amount, ~root.CurrencyID)}}</col>
            //     </row>
            // {{/if}}
            // {{/for}}
            // </grid>
            //
            // {{if Change}}
            // <grid positions="0,46">
            //     <row>
            //     <col>Troco</col>
            //     <col width="10" align="right">{{:~currencyAbs(Change, ~root.CurrencyID)}}</col>
            //     </row>
            //     </grid>
            // {{/if}}
        }

        private IEnumerable<IThermalNode> GenerateFooter(Invoice model, Totals totals)
        {
            var prefix = totals.AmountInTax >= 0 ? "TOTAL FATURA" : "TROCO";

            yield return new TextThermalNode($"{prefix}: {totals.AmountInTax.ToString("C", Culture)}")
            {
                FontSize = FontSize.Large
            };

            yield return new TextThermalNode($"{model.Lines.Count}/{Math.Abs(model.Lines.Sum(l => l.Quantity))} articles");

            foreach (var node in GeneratePayments(model))
            {
                yield return node;
            }

            // TODO(kevin): print debit/credit (PIN) customer receipt

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

            // TODO(kevin): static footer (from config?)
            // {#partial InvoiceReceiptFooter}

            foreach (var orderReference in model.OrderReferences)
            {
                yield return new BarcodeThermalNode("code39", orderReference);
            }

            // TODO(kevin): get certificate name from account config
            yield return new TextThermalNode($"LICENCIADO A: ");
        }
    }
}