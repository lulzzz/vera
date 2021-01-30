using System;
using System.Collections.Generic;
using System.Text;
using Vera.Models;
using Xunit;

namespace Vera.Portugal.Tests
{
  public class MachineReadableCodeGeneratorTests
  {
    [Fact]
    public void Should_generate_correct_machine_readable_code()
    {
      const string expected = "A:123456789*B:999999990*C:PT*D:FS*E:N*F:20190812*G:FS CDVF/12345*H:0*I1:PT*I7:0.65*I8:0.15*N:0.15*O:0.80*Q:qaai*R:9999";

      var invoice = new Invoice
      {
        Supplier = new Billable
        {
          TaxRegistrationNumber = "123456789"
        },
        Customer = new Customer
        {
          BillingAddress = new()
          {
            Country = "PT"
          }
        },
        Date = new DateTime(2019, 8, 12),
        Number = "FS CDVF/12345",
        Signature = new Signature
        {
          Output = Encoding.UTF8.GetBytes("qV6q+SN5dLalOD2cK3s7aBRIWv9q3fimnRAVmutDvVZtjDEzykiQr619UW/0bzTAmPERv+4yiZcOQ7FIja1pzPOgSa9CHQsFAdLjDObZQcNcVEVZPblzCnuJL279/YQERV5q5k2bSZfeQyyA6OfYzReV8NbjAZRabcMEHZhxmdw=")
        },
        Lines = new List<InvoiceLine>
        {
          new()
          {
            Gross = .8m,
            Net = .8m / 1.23m,
            Taxes = new()
            {
              Category = TaxesCategory.High,
              Code = "NOR",
              Rate = 1.23m
            }
          }
        }
      };

      var generator = new MachineReadableCodeGenerator("qaai", "9999");
      var result = generator.Generate(invoice);

      Assert.Equal(expected, result);
    }
  }
}