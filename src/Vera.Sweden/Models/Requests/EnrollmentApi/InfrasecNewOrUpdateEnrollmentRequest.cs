namespace Vera.Sweden.Models.Requests.EnrollmentApi
{
  public sealed class InfrasecNewOrUpdateEnrollmentRequest
  {
    public IdmRequestData IdmRequest { get; set; }

    public class PartnerAuthority
    {
      public string PartnerName { get; set; }
      public string PartnerCode { get; set; }
      public string POSAuthorityCode { get; set; }
    }

    public class OrganizationBranch
    {
      public string BranchName { get; set; }
      public string BranchCode { get; set; }
    }

    public class OrganizationChain
    {
      public string ChainName { get; set; }
      public string ChainCode { get; set; }
    }

    public class StoreInfo
    {
      public string StoreID { get; set; }
      public string StoreName { get; set; }
      public string Email { get; set; }
      public string Cellphone { get; set; }
      public string Address { get; set; }
      public string Zipcode { get; set; }
      public string City { get; set; }
    }

    public class CompanyInfo
    {
      public string OrganizationNumber { get; set; }
      public string Company { get; set; }
      public string Address { get; set; }
      public string Zipcode { get; set; }
      public string City { get; set; }
    }

    public class RegisterInfo
    {
      public string RegisterID { get; set; }
      public string RegisterMake { get; set; }
      public string RegisterModel { get; set; }
      public string LocalAlias { get; set; }
      public string CounterNumber { get; set; }
      public string Address { get; set; }
      public string Zipcode { get; set; }
      public string City { get; set; }
    }

    public class JournalLocation
    {
      public string Company { get; set; }
      public string Address { get; set; }
      public string Zipcode { get; set; }
      public string City { get; set; }
    }

    public class OperationLocation
    {
      public string Company { get; set; }
      public string Address { get; set; }
      public string Zipcode { get; set; }
      public string City { get; set; }
    }

    public class CCU
    {
      public string Enable { get; set; }
    }

    public class Swish
    {
      public string Enable { get; set; }
      public string SwishNr { get; set; }
      public string SwishType { get; set; }
    }

    public class Sparakvittot
    {
      public string Enable { get; set; }
      public string SparakvittotAccount { get; set; }
      public string SparakvittotStoreid { get; set; }
      public string SparakvittotUsername { get; set; }
      public string SparakvittotPassword { get; set; }
    }

    public class PcxService
    {
      public CCU CCU { get; set; }
      public Swish Swish { get; set; }
      public Sparakvittot Sparakvittot { get; set; }
    }

    public class EnrollData
    {
      public string Action { get; set; }
      public PartnerAuthority PartnerAuthority { get; set; }
      public OrganizationBranch OrganizationBranch { get; set; }
      public OrganizationChain OrganizationChain { get; set; }
      public StoreInfo StoreInfo { get; set; }
      public CompanyInfo CompanyInfo { get; set; }
      public RegisterInfo RegisterInfo { get; set; }
      public JournalLocation JournalLocation { get; set; }
      public OperationLocation OperationLocation { get; set; }
      public PcxService PcxService { get; set; }
    }

    public class IdmRequestData
    {
      public string ApplicationID { get; set; }
      public string RequestID { get; set; }
      public EnrollData EnrollData { get; set; }
    }
  }
}