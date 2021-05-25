using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Bogus;
using FakeItEasy;
using Vera.Extensions;
using Vera.Poland.Contracts;
using Vera.Poland.Models.Enums;
using Vera.Poland.Models.Requests;
using Vera.Poland.Models.Responses;
using Vera.Poland.Protocol;
using Vera.Poland.Utils;

namespace Vera.Poland.Tests
{
  public abstract class FiscalPrinterCommandTestsBase
  {
    private readonly IFiscalResponseEmulator _emulator;

    protected readonly List<byte> CommandPayload; // any call to Device.PostData will store the command (input)
    protected Faker Faker { get; }

    private const string FakerNlLocale = "nl";

    protected FiscalPrinterCommandTestsBase()
    {
      _emulator = A.Fake<IFiscalResponseEmulator>();
      CommandPayload = new List<byte>();
      Faker = new Faker(FakerNlLocale);
    }

    protected void ResetPrinterWriteRawDataResponse()
    {
      CommandPayload.Clear();
    }

    protected static byte[] ProducePrinterLastErrorMockResponse(int errorCode)
    {
      static byte[] ProduceByteArray(int number)
      {
        var byteRepresentation = new byte[2];
        byteRepresentation[0] = (byte)(number >> 8);
        byteRepresentation[1] = (byte)(number >> 0);

        return byteRepresentation;
      }

      // We need to respond with ESC r Nul Nul [fiscal bytes] [printer mechanism bytes]
      var response = new[]
      {
        FiscalPrinterCommands.Esc,
        FiscalPrinterResponses.ResponseArgument,
        FiscalPrinterDividers.Nul, // ignoring this byte value
        FiscalPrinterDividers.Nul, // ignore this byte value
      };

      var final = response.Concat(ProduceByteArray(errorCode)).ToArray();
      return final;
    }

    protected static byte[] ProducePrinterAvailabilityResponse(FiscalStatus fiscalStatus, PrinterMechanismStatus printerMechanismStatus)
    {
      var fiscalBytes = GetBytesFor(fiscalStatus);
      var printerMechanismBytes = GetBytesFor(printerMechanismStatus);

      // We need to respond with ESC r Nul Nul [fiscal bytes] [printer mechanism bytes]

      var response = new[]
      {
        FiscalPrinterCommands.Esc,
        FiscalPrinterResponses.ResponseArgument,
        FiscalPrinterDividers.Nul, // ignoring this byte value
        FiscalPrinterDividers.Nul // ignore this byte value
      };

      var final = response.Concat(fiscalBytes.ToList()).Concat(printerMechanismBytes).ToArray();

      return final;
    }

    protected void MockSinglePrinterResponse(byte[] response)
    {
      A.CallTo(() => _emulator.PostData(A<byte[]>.Ignored))
        .WithAnyArguments()
        .ReturnsLazily((byte[] data) =>
        {
          CommandPayload.AddRange(data);
          return new ValueTask<byte[]>(response);
        }).Once();
    }

    /// <summary>
    /// Used to verify if the exact command
    /// </summary>
    /// <param name="response"></param>
    protected void MockExactPrinterResponse(byte[] response)
    {
      A.CallTo(() => _emulator.PostData(A<byte[]>.Ignored))
        .WithAnyArguments()
        .ReturnsLazily((byte[] data) =>
        {
          CommandPayload.AddRange(data);
          return new ValueTask<byte[]>(response);
        }).Once();
    }

    /// <summary>
    /// The printer will always return the same response
    /// </summary>
    /// <param name="response"></param>
    protected void MockAllPrinterResponses(byte[] response)
    {
      A.CallTo(() => _emulator.PostData(A<byte[]>.Ignored))
        .WithAnyArguments()
        .ReturnsLazily((byte[] data) =>
        {
          CommandPayload.AddRange(data);
          return new ValueTask<byte[]>(response);
        });
    }

    protected void SetupAckRespondingPrinter()
    {
      ResetPrinterWriteRawDataResponse();
      var responses = new List<byte> { FiscalPrinterResponses.Ack };
      MockAllPrinterResponses(responses.ToArray());
    }

    protected static T CloneExcludingProperty<T>(T source, string excludedProperty)
      where T : class, new()
    {
      var destination = new T();
      var properties = source.GetType().GetProperties();
      foreach (var property in properties)
      {
        if (property.Name == excludedProperty) 
          continue;
        
        var value = property.GetValue(source);
        property.SetValue(destination, value);
      }

      return destination;
    }

    protected static void MaybeWriteComment(List<byte> sentCommand, string comment)
    {
      if (comment.IsNullOrWhiteSpace()) 
        return;
      
      var encodedComment = EncodingHelper.Encode(comment);
      sentCommand.AddRange(new[] { FiscalPrinterDividers.Lf, FiscalPrinterDividers.Cr });
      sentCommand.AddRange(encodedComment);
    }
    
    protected async ValueTask<TResponse> Run<TQuery, TRequest, TResponse>(TRequest request)
      where TQuery : IFiscalPrinterQuery<TRequest, TResponse>, new()
      where TRequest : PrinterRequest, new()
      where TResponse : PrinterResponse, new()
    {
      var query = new TQuery();

      query.Validate(request);

      var payload = new List<byte>();
      query.BuildRequest(request, payload);

      var printerRawResponse = await _emulator.PostData(payload.ToArray());

      var response = query.ReadResponse(printerRawResponse);

      return response;
    }

    protected async ValueTask<TResponse> Run<TQuery, TResponse>()
      where TQuery : IFiscalPrinterQuery<TResponse>, new()
      where TResponse : PrinterResponse, new()
    {
      var query = new TQuery();

      var payload = new List<byte>();
      query.BuildRequest(payload);

      var printerRawResponse = await _emulator.PostData(payload.ToArray());

      var response = query.ReadResponse(printerRawResponse);

      return response;
    }
    
    protected async ValueTask<PrinterResponse> Run<TCommand, TRequest>(TRequest request)
      where TCommand : IFiscalPrinterCommand<TRequest>, new()
      where TRequest : PrinterRequest, new()
    {

      var command = new TCommand();

      command.Validate(request);

      var payload = new List<byte>();

      command.BuildRequest(request, payload);

      var response = await _emulator.PostData(payload.ToArray());

      var status = GetCommandResultStatus(response);
      return new PrinterResponse
      {
        Success = status
      };
    }

    protected async ValueTask<PrinterResponse> Run<TCommand>()
      where TCommand : IFiscalPrinterCommand, new()
    {
      var command = new TCommand();

      var payload = new List<byte>();
      command.BuildRequest(payload);
      var response = await _emulator.PostData(payload.ToArray());

      var status = GetCommandResultStatus(response);
      return new PrinterResponse
      {
        Success = status
      };
    }

    private static bool GetCommandResultStatus(IReadOnlyList<byte> response)
    {
      if (!response.Any())
      {
        throw new InvalidOperationException("Printer command did not return a response");
      }

      var firstByte = response[0];

      var printerResponse = firstByte switch
      {
        FiscalPrinterResponses.Ack => true,
        FiscalPrinterResponses.Nak => false,
        _ => throw new InvalidOperationException($"Unknown response {response}")
      };

      return printerResponse;
    }
    
    private static IEnumerable<byte> GetBytesFor(FiscalStatus fiscalStatus)
      => EnumConverter<FiscalStatus>.GetByteArrayRepresentation(fiscalStatus);

    private static IEnumerable<byte> GetBytesFor(PrinterMechanismStatus printerMechanismStatus)
      => EnumConverter<PrinterMechanismStatus>.GetByteArrayRepresentation(printerMechanismStatus);
    
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public interface IFiscalResponseEmulator
    {
      public ValueTask<byte[]> PostData(params byte[] data);
    }
  }
}