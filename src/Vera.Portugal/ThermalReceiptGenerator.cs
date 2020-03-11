using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.Azure.Cosmos.Linq;
using Vera.Documents;
using Vera.Documents.Nodes;
using Vera.Models;

namespace Vera.Portugal
{
    // TODO(kevin): header, footer and logo from account config
    public class ThermalReceiptGenerator : IThermalReceiptGenerator
    {
        private static readonly CultureInfo Culture = CultureInfo.CreateSpecificCulture("pt-PT");

        public IThermalNode Generate(Invoice model)
        {
            var nodes = new List<IThermalNode>();

            nodes.AddRange(GenerateHeader(model));

            nodes.AddRange(GenerateInvoiceLines(model));

            nodes.AddRange(GenerateTaxLines(model));

            nodes.AddRange(GenerateFooter(model));

            return new DocumentThermalNode(nodes);
        }

        private IEnumerable<IThermalNode> GenerateHeader(Invoice model)
        {
            // TODO(kevin): supplier details
            // yield return new TextThermalNode("supplier name");
            // yield return new TextThermalNode("supplier address");
            // yield return new TextThermalNode("supplier country");

            if (model.TotalAmount > 0)
            {
                // TODO(kevin): font size
                yield return new TextThermalNode("FATURA SIMPLIFICADA");
                yield return new TextThermalNode($"Fatura: {model.Number}");
            }
            else
            {
                // TODO(kevin): font size
                yield return new TextThermalNode("NOTA DE CRÉDITO");
                yield return new TextThermalNode($"Nota de crédito: {model.Number}");
            }

            // TODO(kevin): fix this piece below
            // {{if ReturnedInvoiceNumber}}
            //     Fatura de referencia: {{:ReturnedInvoiceNumber}}
            // {{/if}

            if (model.Manual)
            {
                yield return new TextThermalNode($"Cópia do documento original -FTM {model.Number}");
            }

            // TODO(kevin): need an "original" print flag
            yield return new TextThermalNode("ORIGINAL");

            // TODO(kevin): duplicate data
//             {#duplicate}
//                 {{if ReturnedOrderReprint}}
//                     DUPLICADO REIMPRESSAO
//                 {{else}}
//                     DUPLICADO
//                 {{/if}}
//                     #{#duplicatenumber}
//             {#/duplicate}

            if (model.Customer != null)
            {
                // yield return new TextThermalNode(model.Customer.Name);
                // yield return new TextThermalNode(model.Customer.RegistrationNumber);
                // yield return new TextThermalNode(model.Customer.TaxRegistrationNumber);

                // yield return new TextThermalNode($"{model.Customer.Street} {model.Customer.HouseNumber}");
                // yield return new TextThermalNode($"{model.Customer.PostalCode} {model.Customer.City}");
                // yield return new TextThermalNode(model.Customer.CountryID);
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
                sb.Append((line.Tax.Rate - 1).ToString("P", Culture));
                sb.Append(line.Gross.ToString("C", Culture));

                yield return new TextThermalNode(sb.ToString());

                if (line.Settlements.Any())
                {
                    var settlement = line.Settlements.Sum(s => s.Amount).ToString("C", Culture);

                    yield return new TextThermalNode($"DESCONTOS: {settlement}");
                }

                // TODO(kevin): tax exemption
            }
        }

        private IEnumerable<IThermalNode> GenerateTaxLines(Invoice model)
        {
            var sb = new StringBuilder();
            sb.Append("Taxa".PadRight(19));
            sb.Append("Base".PadRight(20));
            sb.Append("Q.IVA".PadLeft(6));
            sb.Append("Total".PadLeft(8));

            yield return new TextThermalNode(sb.ToString());

            foreach (var tax in model.Taxes)
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
            sb.Append(model.TotalAmount.ToString("C", Culture));
            sb.Append((model.TotalAmountInTax - model.TotalAmount).ToString("C", Culture));
            sb.Append(model.TotalAmountInTax.ToString("C", Culture));
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

        private IEnumerable<IThermalNode> GenerateFooter(Invoice model)
        {
            var prefix = model.TotalAmountInTax >= 0 ? "TOTAL FATURA" : "TROCO";

            // TODO(kevin): font size BIG
            yield return new TextThermalNode($"{prefix}: {model.TotalAmountInTax.ToString("C", Culture)}");

            yield return new TextThermalNode($"{model.Lines.Count}/{Math.Abs(model.Lines.Sum(l => l.Quantity))} articles");

            foreach (var node in GeneratePayments(model))
            {
                yield return node;
            }

            // TODO(kevin): print debit/credit customer receipt

            if (!string.IsNullOrEmpty(model.Remark))
            {
                yield return new TextThermalNode($"COMENTARIO: {model.Remark}");
            }

            yield return new TextThermalNode($"Atendido por: {model.Employee.Number}");
            yield return new TextThermalNode($"Loja: {model.TerminalId}");

            // TODO(kevin): get software version from account config
            yield return new TextThermalNode("EVA Unified Commerce version 2.0");

            // TODO(kevin): get certificate number from account config
            yield return new TextThermalNode($"certificado no /AT");
            yield return new TextThermalNode("preços unitários com iva incluido");

            yield return new QRCodeThermalNode(Convert.ToBase64String(model.Signature));

            // TODO(kevin): static footer (from config?)
            // {#partial InvoiceReceiptFooter}

            // TODO(kevin): used to print the barcode for the order here
            // <barcode data="{{:OrderBarcode}}" type="Code39" />

            // TODO(kevin): get certificate name from account config
            yield return new TextThermalNode($"LICENCIADO A: ");
        }
    }
}