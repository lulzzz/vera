using System;
using System.Diagnostics.CodeAnalysis;

namespace Vera.Poland.Protocol
{
  [SuppressMessage("ReSharper", "InconsistentNaming")]
  public class FiscalPrinterDividers
  {
    /// <summary>
    /// Divider
    /// </summary>
    public const byte Nul = 0x00;

    /// <summary>
    /// Divider
    /// </summary>
    public const byte Lf = 0x0A;

    /// <summary>
    /// Used for separating the string
    /// </summary>
    public const byte Sp = 0x20;

    /// <summary>
    /// Used as a separator for VAT payload
    ///
    /// TODO: check
    /// </summary>
    public static readonly byte[] Kd =
    {
      Convert.ToByte('K'),
      Convert.ToByte('D')
    };

    /// <summary>
    /// Used as a separator for cashier payload
    /// </summary>
    public static readonly byte J = Convert.ToByte('J');

    /// <summary>
    /// Used as a separator for date payload
    /// </summary>
    public static readonly byte I = Convert.ToByte('I');

    /// <summary>
    /// Used as a separator for date payload when setting up currency
    /// </summary>
    public static readonly byte[] Ke =
    {
      Convert.ToByte('K'),
      Convert.ToByte('E')
    };

    /// <summary>
    /// Used as a separator for payment type payloads when setting payment types
    ///
    ///
    /// Used as a separator for discount operations on transaction line
    /// </summary>
    public static readonly byte P = Convert.ToByte('P');

    /// <summary>
    /// Used as a separator for Discount, uplift or reduction to the sales line
    /// </summary>
    public static readonly byte p = Convert.ToByte('p');

    /// <summary>
    /// Used as a separator for periodical fiscal report
    /// </summary>
    public static readonly byte H = Convert.ToByte('H');

    /// <summary>
    /// Used as a separator for taxpayer name payload
    /// </summary>
    public static readonly byte h = Convert.ToByte('h');

    /// <summary>
    /// Used as a separator for pos address
    /// Used as a separator for discounts on transaction line
    /// </summary>
    public static readonly byte A = Convert.ToByte('A');

    /// <summary>
    /// Used as a separator for creating a fiscal daily report
    /// </summary>
    public static readonly byte B = Convert.ToByte('B');

    /// <summary>
    /// Used as a separator for creating a digital only fiscal daily report
    /// Used for cancelling a transaction line
    /// Used as a separator for transaction lines discount operation
    /// </summary>
    public static readonly byte c = Convert.ToByte('c');

    /// <summary>
    /// Used as a separator for creating the invoice
    /// Used as a separator for cancelling graphics loading
    /// </summary>
    public static readonly byte C = Convert.ToByte('C');

    /// <summary>
    /// Used for printing the X report
    /// </summary>
    public static readonly byte O = Convert.ToByte('O');

    /// <summary>
    /// Used for toggling signature field printout
    /// </summary>
    public static readonly byte[] Gp =
    {
      Convert.ToByte('g'),
      Convert.ToByte('P')
    };

    /// <summary>
    /// Used for staticTrailer
    /// </summary>
    public static readonly byte[] tS =
    {
      Convert.ToByte('t'),
      Convert.ToByte('S')
    };

    /// <summary>
    /// Total periodic report type: from date to date
    ///
    /// Discount, uplift or reduction to the sales line
    /// </summary>
    public static readonly byte k = Convert.ToByte('k');

    /// <summary>
    /// Total periodic report type: from number to number
    /// </summary>
    public static readonly byte j = Convert.ToByte('j');

    /// <summary>
    /// Total periodic report type: total monthly fiscal
    /// </summary>
    public static readonly byte[] ms =
    {
      Convert.ToByte('m'),
      Convert.ToByte('s')
    };

    /// <summary>
    /// Total periodic report type: entire memory
    /// </summary>
    public static readonly byte b = Convert.ToByte('b');

    /// <summary>
    /// Periodic fiscal report type: from date to date
    /// </summary>
    public static readonly byte i = Convert.ToByte('i');

    /// <summary>
    /// Periodic fiscal report type: from number to number
    /// </summary>
    public static readonly byte n = Convert.ToByte('n');

    /// <summary>
    /// Periodic fiscal report type: entire memory
    /// </summary>
    public static readonly byte m = Convert.ToByte('m');

    /// <summary>
    /// Periodic fiscal report type: entire memory
    ///
    /// Used as a separator for transaction lines discount operation
    /// </summary>
    public static readonly byte a = Convert.ToByte('a');

    /// <summary>
    /// Used for initializing the graphics loading
    /// </summary>
    public static readonly byte[] LB =
    {
      Convert.ToByte('L'),
      Convert.ToByte('B')
    };

    /// <summary>
    /// Graphics handling
    /// </summary>
    public static readonly byte OpenParenthesis = Convert.ToByte('(');

    /// <summary>
    /// Used for initializing the graphics loading
    /// </summary>
    public static readonly byte[] LD =
    {
      Convert.ToByte('L'),
      Convert.ToByte('D')
    };

    /// <summary>
    /// Used for readout commands
    /// </summary>
    public static readonly byte[] LT =
    {
      Convert.ToByte('L'),
      Convert.ToByte('T')
    };

    /// <summary>
    /// Used as a separator for cancelling graphics loading
    /// </summary>
    public static readonly byte L = Convert.ToByte('L');

    /// <summary>
    /// Used as a separator for graphic printout
    /// </summary>
    public static readonly byte Zero = EncodingHelper.Encode(0)[0];

    /// <summary>
    /// Used as a separator for deleting a graphic
    /// </summary>
    public static readonly byte E = Convert.ToByte('E');

    /// <summary>
    /// Used as a separator for getting a graphic checksum
    /// </summary>
    public static readonly byte R = Convert.ToByte('R');

    /// <summary>
    /// Used as a separator for opening a cash drawer
    /// </summary>
    public static readonly byte X = Convert.ToByte('X');

    /// <summary>
    /// Used as a separator for online state readout
    /// </summary>
    public static readonly byte x = Convert.ToByte('x');

    /// <summary>
    /// Used as a separator for transaction lines
    ///
    /// 1.3.2 Transaction line
    /// </summary>
    public const byte Cr = 0x0D;

    /// <summary>
    /// Used as a separator for transaction lines
    ///
    /// 1.3.2 Transaction line
    /// </summary>
    public static readonly byte D = Convert.ToByte('D');

    /// <summary>
    /// Used as a separator for discount, uplift or reduction of transaction lines
    ///
    /// 1.3.5 Discount, uplift or reduction to the sales line
    /// </summary>
    public static readonly byte d = Convert.ToByte('d');

    /// <summary>
    /// Used as a separator for transaction lines
    ///
    /// 1.3.2 Transaction line
    /// </summary>
    public static readonly byte Star = Convert.ToByte('*');

    /// <summary>
    /// Used as a separator for cancelling  transaction lines
    ///
    /// 1.3.4 Cancellation of the sales transaction line
    /// </summary>
    public static readonly byte Minus = Convert.ToByte('-');

    /// <summary>
    /// Used as a separator for discounts on transaction line
    ///
    /// 1.3.5 Discount, uplift or reduction to the sales line
    /// </summary>
    public static readonly byte U = Convert.ToByte('U');

    /// <summary>
    /// Used as a separator for partial sum
    /// </summary>
    public static readonly byte Q = Convert.ToByte('Q');

    /// <summary>
    /// Used as a separator for the sum of the transaction
    ///
    /// 1.3.11 Sum of transaction
    /// </summary>
    public static readonly byte T = Convert.ToByte('T');

    /// <summary>
    /// Used as a separator for the percentage discount or uplift to the partial sum
    ///
    /// 1.3.7 Percentage discount or uplift to the partial sum
    /// </summary>
    public static readonly byte F = Convert.ToByte('F');

    /// <summary>
    /// Used as a separator for the discount or uplift amount to the partial sum
    ///
    /// 1.3.8 Discount or uplift amount to the partial sum
    /// </summary>
    public static readonly byte f = Convert.ToByte('f');

    /// <summary>
    /// Used as a separator for discount to the one tax rate
    ///
    /// 1.3.10 Percentage/amount discount/uplift to total of the one tax rate
    /// </summary>
    public static readonly byte V = Convert.ToByte('V');

    /// <summary>
    /// Used as a separator for discount to the one tax rate
    ///
    /// 1.3.10 Percentage/amount discount/uplift to total of the one tax rate
    /// </summary>
    public static readonly byte v = Convert.ToByte('v');

    /// <summary>
    /// Used as a separator for total periodic report when report type is total monthly report
    ///
    /// 1.4.3 Total periodic report
    /// </summary>
    public static readonly byte s = Convert.ToByte('s');

    /// <summary>
    /// Used as a separator for the end of a predefined printout
    ///
    /// 1.6.4 The end of the predefined printout
    /// </summary>
    public static readonly byte N = Convert.ToByte('N');

    /// <summary>
    /// Used as a separator for the end of a predefined printout
    ///
    /// 1.6.4 The end of the predefined printout
    /// </summary>
    public static readonly byte Z = Convert.ToByte('Z');

    /// <summary>
    /// Used as a separator for toggling euronip
    /// </summary>
    public static readonly byte g = Convert.ToByte('g');

    /// <summary>
    /// Used as a separator for setting the line display
    ///
    /// 4.10.8 Line display
    /// </summary>
    public static readonly byte G = Convert.ToByte('G');
  }
}