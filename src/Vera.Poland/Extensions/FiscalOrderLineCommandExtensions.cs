using System;
using System.Collections.Generic;
using Vera.Extensions;
using Vera.Poland.Commands.Invoice.Contract;
using Vera.Poland.Models.Requests.Invoice;
using Vera.Poland.Models.Validation;
using Vera.Poland.Protocol;

namespace Vera.Poland.Extensions
{
  public static class FiscalOrderLineCommandExtensions
  {
    private const int MaxCommentCharacters = 53;

    /// <summary>
    /// TODO: See later when we will have to support types of paper/fonts
    ///
    /// The string length must not exceed
    /// 56 characters for wide paper and small font,
    /// 42 for wide paper and bigger font,
    /// 40 for narrow paper
    /// </summary>
    private const int MaxProductNameCharacters = 56;

    private static readonly ValidationInterval Quantity = new()
    {
      LowerBound = 0.005m,
      UpperBound = 429496.7295m
    };

    private static readonly ValidationInterval Price = new()
    {
      LowerBound = 0.01m,
      UpperBound = 429496.72m
    };

    private static readonly ValidationInterval Value = new()
    {
      LowerBound = 0.01m,
      UpperBound = 429496.72m
    };

    public static void MaybeIncludeComment<TRequest>(
      this IFiscalOrderLineInvoiceCommand<TRequest> _,
      List<byte> request,
      string comment)
      where TRequest : OrderLinePrinterRequest, new()
    {
      if (!comment.IsNullOrWhiteSpace())
      {
        var encodedComment = EncodingHelper.Encode(comment);

        request.Add(FiscalPrinterDividers.Lf);
        request.Add(FiscalPrinterDividers.Cr);
        request.Add(encodedComment);
      }
    }

    public static void ValidateComment<TRequest>(
      this IFiscalOrderLineInvoiceCommand<TRequest> _,
      string propertyName, string comment)
      where TRequest : OrderLinePrinterRequest, new()
    {
      if (propertyName.IsNullOrWhiteSpace())
      {
        throw new ArgumentNullException(nameof(propertyName));
      }

      if (comment?.Length > MaxCommentCharacters)
      {
        throw new ArgumentOutOfRangeException(
          propertyName,
          $"Must have less than {MaxCommentCharacters} characters");
      }
    }

    public static void ValidateValueInRange<TRequest>(
      this IFiscalOrderLineInvoiceCommand<TRequest> _,
      string propertyName, decimal value, ValidationInterval interval)
      where TRequest : OrderLinePrinterRequest, new()
    {
      if (propertyName.IsNullOrWhiteSpace())
      {
        throw new ArgumentNullException(nameof(propertyName));
      }

      var invalid = value < interval.LowerBound || value > interval.UpperBound;

      if (invalid)
      {
        throw new ArgumentOutOfRangeException(
          propertyName,
          $"Value must be between {interval.LowerBound} and {interval.UpperBound}");
      }
    }
    
    public static void ValidateBaseRequest<TRequest>(
      this IFiscalOrderLineInvoiceCommand<TRequest> command, OrderLinePrinterRequest request)
      where TRequest : OrderLinePrinterRequest, new()
    {
      if (request.ProductName.IsNullOrWhiteSpace())
      {
        throw new ArgumentNullException(nameof(OrderLinePrinterRequest.ProductName));
      }

      if (request.ProductName.Length > MaxProductNameCharacters)
      {
        throw new ArgumentOutOfRangeException(
          nameof(OrderLinePrinterRequest.ProductName),
          $"Must have less than {MaxProductNameCharacters} characters");
      }

      command.ValidateValueInRange(
        nameof(OrderLinePrinterRequest.Quantity), request.Quantity, Quantity);

      command.ValidateValueInRange(
        nameof(OrderLinePrinterRequest.Price), request.Price, Price);

      command.ValidateValueInRange(
        nameof(OrderLinePrinterRequest.Value), request.Value, Value);

      if (request.Vat == default)
      {
        throw new ArgumentNullException(nameof(AddTransactionLineRequest.Vat));
      }
    }
  }
}