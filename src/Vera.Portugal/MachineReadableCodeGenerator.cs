using Vera.Models;
using Vera.Portugal.Models;
using Vera.Signing;

namespace Vera.Portugal
{
  public class MachineReadableCodeGenerator : IMachineReadableCodeGenerator
  {
    private readonly string _shortSignature;
    private readonly string _certificateNumber;

    public MachineReadableCodeGenerator(string shortSignature, string certificateNumber)
    {
      _shortSignature = shortSignature;
      _certificateNumber = certificateNumber;
    }

    public string Generate(Invoice invoice)
    {
      var totals = invoice.Totals;
      var table = totals.Taxes;

      var builder = new MachineCodeBuilder()
        .Issuer(invoice.Supplier.TaxRegistrationNumber)
        .Purchaser(invoice.Customer.TaxRegistrationNumber)
        .PurchaserCountry(invoice.Customer.BillingAddress?.Country)
        .DocumentType(InvoiceTypeHelper.DetermineType(invoice).ToString())
        .DocumentState(InvoiceStatus.N.ToString())
        .DocumentDate(invoice.Date)
        .DocumentNumber(invoice.Number)
        .ATCUD(Constants.ATCUD)

        // TODO(kevin): will break for the two regions within PT (Azoren/Madeira)
        // How to fix..? Need more input..?
        .FiscalSpace("PT")
        .TaxExempt(table.Exempt)
        .TaxReduced(table.Low)
        .TaxIntermediate(table.Intermediate)
        .TaxNormal(table.High)
        .NonTaxable(table.Zero)
        .TaxTotal(table.Total)
        .Total(totals.Gross)
        .Hash(_shortSignature)
        .CertificateNumber(_certificateNumber);

      return builder.ToString();
    }
  }
}