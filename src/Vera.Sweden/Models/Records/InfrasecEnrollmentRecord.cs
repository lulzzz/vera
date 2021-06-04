namespace Vera.Sweden.Models.Records
{
  public record InfrasecEnrollmentRecord
  {
    public string OrganizationUnitName { get; init; }
    public string OrganizationUnitAddress { get; init; }
    public string OrganizationUnitCity { get; init; }
    public string OrganizationUnitZipCode { get; init; }
    public string CompanyZipcode { get; init; }
    public string CompanyCity { get; init; }
    public string CompanyName { get; init; }
    public string CompanyAddress { get; init; }
    public string CompanyRegistrationNumber { get; init; }
    public string StationName { get; init; }
    public string OrganizationUnitEmail { get; init; }
  }
}