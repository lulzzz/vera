using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Vera.Invoices;
using Vera.Models;
using Vera.StandardAuditFileTaxation;

namespace Vera.Portugal
{
  /// <summary>
  /// Builder to build a code according to the
  /// </summary>
  public sealed class MachineCodeBuilder
  {
    private const string DateFormat = "yyyyMMdd";

    private static readonly NumberFormatInfo DecimalFormat = new()
    {
      NumberDecimalSeparator = "."
    };

    private readonly LinkedList<string> _parts;

    public MachineCodeBuilder()
    {
      _parts = new LinkedList<string>();
    }

    /// <summary>
    /// Tax identification number of the issuer without country prefix.
    /// </summary>
    /// <param name="tin"></param>
    /// <returns></returns>
    public MachineCodeBuilder Issuer(string tin)
    {
      return Append("A", tin);
    }

    /// <summary>
    /// Tax identification number of the purchaser.
    /// </summary>
    /// <param name="tin"></param>
    /// <returns></returns>
    public MachineCodeBuilder Purchaser(string? tin)
    {

      return Append("B", string.IsNullOrEmpty(tin) ? Constants.DefaultCustomerTaxId : tin);
    }

    /// <summary>
    /// Fill in accordance with the field Country from the table of clients of SAF-T(PT).
    /// </summary>
    /// <param name="countryCode"></param>
    /// <returns></returns>
    public MachineCodeBuilder PurchaserCountry(string? countryCode)
    {
      return Append("C", string.IsNullOrEmpty(countryCode) ? Constants.UnknownLabel : countryCode);
    }

    public MachineCodeBuilder DocumentType(string type)
    {
      return Append("D", type);
    }

    public MachineCodeBuilder DocumentState(string status)
    {
      return Append("E", status);
    }

    public MachineCodeBuilder DocumentDate(DateTime date)
    {
      return Append("F", date.ToString(DateFormat));
    }

    public MachineCodeBuilder DocumentNumber(string number)
    {
      return Append("G", number);
    }

    public MachineCodeBuilder ATCUD(string atcud)
    {
      return Append("H", atcud);
    }

    public MachineCodeBuilder FiscalSpace(string taxCountryRegion)
    {
      return Append("I1", taxCountryRegion);
    }

    public MachineCodeBuilder TaxExempt(TaxTable.Entry? value)
    {
      return AppendTaxBase("I2", value);
    }

    public MachineCodeBuilder TaxReduced(TaxTable.Entry? value)
    {
      return AppendTaxBase("I3", value).AppendTaxAmount("I4", value);
    }

    public MachineCodeBuilder TaxIntermediate(TaxTable.Entry? value)
    {
      return AppendTaxBase("I5", value).AppendTaxAmount("I6", value);
    }

    public MachineCodeBuilder TaxNormal(TaxTable.Entry? value)
    {
      return AppendTaxBase("I7", value).AppendTaxAmount("I8", value);;
    }

    public MachineCodeBuilder NonTaxable(TaxTable.Entry? value)
    {
      return AppendTaxBase("L", value);
    }

    // M = stamp duty?

    /// <summary>
    /// Total VAT and Duty Stamp value – field TaxPayable from SAF-T(PT).
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public MachineCodeBuilder TaxTotal(decimal value)
    {
      return Append("N", value);
    }

    /// <summary>
    /// Total value of the document – field GrossTotal from SAF-T(PT).
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public MachineCodeBuilder Total(decimal value)
    {
      return Append("O", value);
    }

    // P = deduction at source

    /// <summary>
    /// Fill in accordance with the Decree num. 363/2010, June 23. (article 6, paragraph 3 point a).
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public MachineCodeBuilder Hash(string value)
    {
      return Append("Q", value);
    }

    /// <summary>
    /// Free text field, which can be used, for example, to indicate information related to the
    /// payment (example: IBAN or ATM reference, using the separator “;”). This field can not contain the asterisk character (*).
    /// </summary>
    /// <param name="number"></param>
    /// <returns></returns>
    public MachineCodeBuilder CertificateNumber(string number)
    {
      return Append("R", number);
    }

    public MachineCodeBuilder Other(string value)
    {
      if (value.Length > 65) throw new ArgumentOutOfRangeException(nameof(value), "Maximum length of 65 is allowed");

      return Append("S", value.Replace('*', ' '));
    }

    private MachineCodeBuilder Append(string code, string value)
    {
      _parts.AddLast($"{code}:{value}");
      return this;
    }

    private MachineCodeBuilder Append(string code, decimal value)
    {
      return Append(code, value.ToString("0.00", DecimalFormat));
    }

    private MachineCodeBuilder AppendTaxBase(string code, TaxTable.Entry? value)
    {
      return value == null ? this : Append(code, value.Base);
    }

    private MachineCodeBuilder AppendTaxAmount(string code, TaxTable.Entry? value)
    {
      return value == null ? this : Append(code, value.Value);
    }

    public override string ToString()
    {
      return string.Join('*', _parts.OrderBy(x => x));
    }
  }
}