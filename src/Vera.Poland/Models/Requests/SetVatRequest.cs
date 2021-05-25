namespace Vera.Poland.Models.Requests
{
  public class SetVatRequest : PrinterRequest
  {
    public VatItem A { get; set; }

    public VatItem B { get; set; }

    public VatItem C { get; set; }

    public VatItem D { get; set; }

    public VatItem E { get; set; }

    public VatItem F { get; set; }

    public VatItem G { get; set; }
  }
}