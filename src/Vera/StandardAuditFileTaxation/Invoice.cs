using System;
using System.Collections.Generic;

namespace Vera.StandardAuditFileTaxation
{
  public sealed class Invoice : SourceDocument
  {
    public Invoice() { }

    public Invoice(Invoice other)
    {
      Number = other.Number;
      Customer = other.Customer;
      Supplier = other.Supplier;
      Date = other.Date;
      ShipFrom = other.ShipFrom;
      ShipTo = other.ShipTo;
      ReceiptNumbers = other.ReceiptNumbers;
      TerminalID = other.TerminalID;
      IsManual = other.IsManual;

      if (other.Lines != null)
      {
        Lines = new List<InvoiceLine>(other.Lines);
      }

      if (other.Prints != null)
      {
        Prints = new List<InvoicePrint>(other.Prints);
      }
    }

    /// <summary>
    /// Number of the invoice, complaint with the active certification.
    /// </summary>
    public string Number { get; set; }

    /// <summary>
    /// Type of the invoice. This is defined by the certification implementation.
    /// </summary>
    public string Type { get; set; }

    /// <summary>
    /// Optional entity that is attached to the invoice. Usually the customer that purchased the goods.
    /// </summary>
    public Billable Customer { get; set; }

    /// <summary>
    /// Entity that sold/created the invoice. (store)
    /// </summary>
    public Billable Supplier { get; set; }

    /// <summary>
    /// Timestamp of the invoicing.
    /// </summary>
    public DateTime Date { get; set; }

    public ShippingPoint ShipFrom { get; set; }

    public ShippingPoint ShipTo { get; set; }

    /// <summary>
    /// Details of person/application that entered the transaction.
    /// </summary>
    public string SourceID { get; set; }

    /// <summary>
    /// The number(s) of the receipt(s) on this consolidated invoice record. Can be a single number, range or a list.
    /// </summary>
    public string[] ReceiptNumbers { get; set; }

    public ICollection<InvoiceLine> Lines { get; set; }

    /// <summary>
    /// Unique reference to the POS/terminal that handled the invoice. Definition of this field is the combination of the user and the organization
    /// that created the invoice/transaction.
    /// </summary>
    public string TerminalID { get; set; }

    /// <summary>
    /// Signature to the previous invoice (can be null if it is the first invoice).
    /// Note: this is not a SAF-T field.
    /// </summary>
    public string PreviousSignature { get; set; }

    public string RawSignature { get; set; }

    /// <summary>
    /// Signature of the invoice. Required for, for example SAF-T 1.0 and NF525 to function correctly because the signature
    /// of an invoice is required on the ticket/invoice.
    /// </summary>
    public byte[] Signature { get; set; }

    /// <summary>
    /// Version of the key that was used to generate the signature.
    /// </summary>
    public int SignatureKeyVersion { get; set; }

    public ICollection<InvoicePrint> Prints { get; } = new List<InvoicePrint>();

    /// <summary>
    /// Indicates if this is a manually created invoice.
    /// </summary>
    public bool IsManual { get; set; }
  }

  public sealed class InvoicePrint
  {
    public int Sequence { get; set; }

    public DateTime Date { get; set; }

    /// <summary>
    /// Indicates if the print succeeded yay or nay.
    /// </summary>
    public bool Success { get; set; }
  }
}