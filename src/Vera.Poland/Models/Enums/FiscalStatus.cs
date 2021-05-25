using System;

namespace Vera.Poland.Models.Enums
{
  /// <summary>
  /// 1.11.4 Extended status
  /// </summary>
  [Flags]
  public enum FiscalStatus: long
  {
    None = 0,
    //
    // Byte 1
    //


    /// <summary>
    /// Header printed
    /// </summary>
    HeaderPrinted = 1 << 0,

    /// <summary>
    /// Fiscal day opened (Sale period)
    /// </summary>
    FiscalDayOpened = 1 << 1,

    /// <summary>
    /// Receipt opened
    /// </summary>
    ReceiptOpened = 1 << 2,

    /// <summary>
    /// Returnable packaging settlement opened
    /// </summary>
    ReturnablePackagingSettlementOpened = 1 << 3,

    /// <summary>
    /// Summary receipt
    /// </summary>
    SummaryReceipt = 1 << 4,

    /// <summary>
    /// Invoice opened
    /// </summary>
    InvoiceOpened = 1 << 5,

    /// <summary>
    /// Summary invoice
    /// </summary>
    SummaryInvoice = 1 << 6,

    /// <summary>
    /// All customer data printed on invoice
    /// </summary>
    AllCustomerDataPrintedOnInvoice = 1 << 7,


    //
    // Byte 2
    //

    /// <summary>
    /// Hardware data saved in Fiscal Memory
    /// </summary>
    HardwareDataSavedInFiscalMemory = 1 << 8,

    /// <summary>
    /// Producer data saved in Fiscal Memory
    /// </summary>
    ProducerDataSavedInFiscalMemory = 1 << 9,

    /// <summary>
    /// Unique number saved in Fiscal Memory
    /// </summary>
    UniqueNumberSavedInFiscalMemory = 1 << 10,

    /// <summary>
    /// Printer in fiscal mode
    /// </summary>
    PrinterInFiscalMode = 1 << 11,

    /// <summary>
    /// VAT rates array saved in Fiscal Memory
    /// </summary>
    VatRatesArraySavedInFiscalMemory = 1 << 12,

    /// <summary>
    /// Fiscal Memory almost full (less than 30 records available)
    /// </summary>
    FiscalMemoryAlmostFull = 1 << 14,

    /// <summary>
    /// Fiscal Memory full (saved 2100 daily records or 200 RAM clear records)
    /// </summary>
    FiscalMemoryFull = 1 << 15,

    //
    // Byte 3
    //

    /// <summary>
    /// Fiscal mode
    /// </summary>
    FiscalMode = 1 << 16,

    /// <summary>
    /// Printer busy
    /// </summary>
    PrinterBusy = 1 << 17,

    /// <summary>
    /// RTC error
    /// </summary>
    RtcError = 1 << 18,

    /// <summary>
    /// RAM erased
    /// </summary>
    RamErased = 1 << 19,

    /// <summary>
    /// Non fiscal document printing in progress
    /// </summary>
    NonFiscalDocumentPrintingInProgress = 1 << 20,

    /// <summary>
    /// Header defined
    /// </summary>
    HeaderDefined = 1 << 21,

    /// <summary>
    /// Periodical report printing in progress
    /// </summary>
    PeriodicalReportPrintingInProgress = 1 << 22,

    /// <summary>
    /// Buffer empty
    /// </summary>
    BufferEmpty = 1 << 23,

    //
    // Byte 4
    //

    /// <summary>
    /// Fiscal memory connected
    /// </summary>
    FiscalMemoryConnected = 1 << 25,

    /// <summary>
    /// Service jumper ON
    /// </summary>
    ServiceJumperOn = 1 << 26,

    /// <summary>
    /// Display connected
    /// </summary>
    DisplayConnected = 1 << 27,

    /// <summary>
    /// Alphanumerical display
    /// </summary>
    AlphanumericalDisplay = 1 << 28,

    /// <summary>
    /// Xon/Xoff protocol selected
    /// </summary>
    XonXOffProtocolSelected = 1 << 29,

    IsThePrinterConnectedTo230V = 1 << 30,
  }
}
