using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Vera.Poland.Contracts;
using Vera.Poland.Models.Requests;

namespace Vera.Poland.Commands
{
  /// <summary>
  /// Un-sets the static trailer lines by calling <see cref="SetStaticTrailerCommand"> the set static trailer command </see>
  /// with empty static trailer lines
  ///
  /// We do this every time before setting a new static trailer to avoid a static trailer buffer underflow
  /// </summary>
  [SuppressMessage("ReSharper", "InvalidXmlDocComment")]
  public class UnsetStaticTrailerCommand: IFiscalPrinterCommand
  {
    private const int StaticTrailerLinesMaxCount = 10;
    private readonly SetStaticTrailerCommand _setStaticTrailerCommand;

    public UnsetStaticTrailerCommand()
    {
      _setStaticTrailerCommand = new SetStaticTrailerCommand();
    }

    public void BuildRequest(List<byte> request)
    {
      // Acceptance conditions
      //  Closed fiscal day

      var emptyInput = new SetStaticTrailerRequest
      {
        Lines = Enumerable.Repeat("", StaticTrailerLinesMaxCount).ToList()
      };

      _setStaticTrailerCommand.BuildRequest(emptyInput, request);
    }
  }
}