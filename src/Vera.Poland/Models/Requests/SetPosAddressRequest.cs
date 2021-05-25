
namespace Vera.Poland.Models.Requests
{
  public class SetPosAddressRequest : PrinterRequest
  {
    public string Place { get; set; }

    public string TaxOffice { get; set; }

    public string Street { get; set; }

    public string PostalCode { get; set; }

    public string HouseNumber { get; set; }

    public string ApartmentNumber { get; set; }
  }
}