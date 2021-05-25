using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Vera.Extensions;
using Vera.Poland.Contracts;
using Vera.Poland.Models.Requests;
using Vera.Poland.Protocol;

namespace Vera.Poland.Commands
{
  /// <summary>
  /// See 1.7.2 Sets cashier and terminal ID for more detailed information
  ///
  /// Format
  ///  ESC MFB J<cashier_terminal_ID> ESC MFE
  ///
  /// Arguments
  /// • <cashier_terminal_ID> - format: ‘nnnkkkkkkkkkkkkkkkkkkkkk’
  /// where:
  /// nnn – terminal number(3 chars),
  /// kkk… - cashier ID(max 22 chars). The parameter can’t be zero or only spaces.After clearing RAM memory the parameter will be changed to „#001 Serwisant”
  /// 
  /// </summary>
  [SuppressMessage("ReSharper", "InvalidXmlDocComment")]
  public class SetCashierCommand : IFiscalPrinterCommand<SetCashierRequest>
  {

    public const int TerminalNumberRequiredLength = 3;
    private const int CashierIdentifierMaxLength = 22;

    public void Validate(SetCashierRequest input)
    {
      if (input.TerminalNumber.IsNullOrWhiteSpace())
      {
        throw new ArgumentNullException(nameof(SetCashierRequest.TerminalNumber));
      }

      if (input.CashierIdentifier.IsNullOrWhiteSpace())
      {
        throw new ArgumentNullException(nameof(SetCashierRequest.CashierIdentifier));
      }

      if (input.TerminalNumber.Length != TerminalNumberRequiredLength)
      {
        throw new ArgumentOutOfRangeException(
          nameof(SetCashierRequest.TerminalNumber),
          $"Must have exactly {TerminalNumberRequiredLength} characters");
      }

      if (input.CashierIdentifier.Length > CashierIdentifierMaxLength)
      {
        throw new ArgumentOutOfRangeException(
          nameof(SetCashierRequest.CashierIdentifier),
          $"Must have less than {CashierIdentifierMaxLength} characters");
      }
    }

    public void BuildRequest(SetCashierRequest input, List<byte> request)
    {
      byte[] GetEncodedCashierInformation()
      {
        var cashierData = $"{input.TerminalNumber}{input.CashierIdentifier}";

        return EncodingHelper.Encode(cashierData);
      }

      request.Add(FiscalPrinterCommands.Esc);
      request.Add(FiscalPrinterCommands.Mfb);
      request.Add(FiscalPrinterDividers.J);

      request.AddRange(GetEncodedCashierInformation());

      request.Add(FiscalPrinterCommands.Esc);
      request.Add(FiscalPrinterCommands.Mfe);

      // Acceptance conditions
      //   • Neutral
      //   • Not initialized

    }
  }
}