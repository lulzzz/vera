namespace EVA.Auditing.Sweden.Models.Responses
{
  public class InfrasecEnrollmentResponse
  {
    public IdmResponseData IdmResponse { get; set; }

    public class IdmResponseData
    {
      public string ResponseCode { get; set; }
      public string ResponseMessage { get; set; }
      public string ResponseReason { get; set; }
      public string ApplicationID { get; set; }
      public string RequestID { get; set; }
      public string RegisterID { get; set; }
      public string Action { get; set; }
      public object CCUID { get; set; }
      public string Active { get; set; }
      public string LoginCount { get; set; }
      public object LastLogin { get; set; }
      public Info Info { get; set; }
    }

    public class PartnerAuthority
    {
      public string PartnerName { get; set; }
      public string PartnerCode { get; set; }
      public string POSAuthorityCode { get; set; }
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

    public class Info
    {
      public PartnerAuthority PartnerAuthority { get; set; }
      public OrganizationChain OrganizationChain { get; set; }
      public StoreInfo StoreInfo { get; set; }
      public CompanyInfo CompanyInfo { get; set; }
      public RegisterInfo RegisterInfo { get; set; }
      public JournalLocation JournalLocation { get; set; }
      public OperationLocation OperationLocation { get; set; }
    }
  }
}