using System;
using System.Threading.Tasks;
using Vera.Models;
using Vera.Registers;
using Vera.Stores;
using Vera.Sweden.InfrasecHttpClient.Contracts;
using Vera.Sweden.Models.Configs;
using Vera.Sweden.Models.Enums;
using Vera.Sweden.Models.Exceptions;
using Vera.Sweden.Models.Records;
using Vera.Sweden.RequestBuilders.Contracts;
using Vera.Sweden.Validators.Contracts;

namespace Vera.Sweden.Services
{
  public class SyncNewStationWithInfrasecService : IRegisterInitializer
  {
    private readonly SwedenConfigs _swedenConfigs;
    private readonly IInfrasecEnrollmentApiClientFactory _infrasecEnrollmentApiClientFactory;
    private readonly IInfrasecNewStationEnrollmentRequestBuilder _infrasecNewStationEnrollmentRequestBuilder; 
    private readonly IInfrasecEnrollmentResponseValidator _infrasecEnrollmentResponseValidator;
    private readonly IRegisterStore _registerStore;

    public SyncNewStationWithInfrasecService(IInfrasecEnrollmentApiClientFactory infrasecEnrollmentApiClientFactory,
      SwedenConfigs swedenConfigs, 
      IInfrasecNewStationEnrollmentRequestBuilder infrasecNewStationEnrollmentRequestBuilder, 
      IInfrasecEnrollmentResponseValidator infrasecEnrollmentResponseValidator, 
      IRegisterStore registerStore)
    {
      _infrasecEnrollmentApiClientFactory = infrasecEnrollmentApiClientFactory;
      _swedenConfigs = swedenConfigs;
      _infrasecNewStationEnrollmentRequestBuilder = infrasecNewStationEnrollmentRequestBuilder;
      _infrasecEnrollmentResponseValidator = infrasecEnrollmentResponseValidator;
      _registerStore = registerStore;
    }

    public async Task Initialize(RegisterInitializationContext context)
    {
      await EnrollNewStationWithInfrasec(context);
      
      context.Register.Status = RegisterStatus.Open;
    }

    // TODO(SEBI): LOGS
    /// <summary>
    ///   No recovery scenarios, will throw if anything goes wrong
    ///   ACTION = "NEW"
    ///   Assigns the RegisterID as the Station FiscalID
    /// </summary>
    /// <returns></returns>
    private async Task EnrollNewStationWithInfrasec(RegisterInitializationContext registerInitializationContext)
    {
      try
      {
        // _log.Info($"Syncing NEW Station: {station.Name}, ID: {station.ID}, OrganizationUnitID {station.OrganizationUnitID} with Infrasec");

        // get total swedish stations in order to compute the StationNumber (part of the register unique identity)
        var totalSwedishRegisters = await _registerStore.GetTotalRegisters(registerInitializationContext.Supplier.Id);
        
        var enrollmentData = new InfrasecEnrollmentRecord{
          OrganizationUnitName = registerInitializationContext.Supplier.Name, 
          OrganizationUnitAddress = $"{registerInitializationContext.Supplier.Address?.Street} - {registerInitializationContext.Supplier.Address?.Number}", 
          OrganizationUnitCity = registerInitializationContext.Supplier.Address?.City, 
          OrganizationUnitZipCode = registerInitializationContext.Supplier.Address?.PostalCode, 
          CompanyZipcode = registerInitializationContext.Account.Address?.PostalCode, 
          CompanyCity = registerInitializationContext.Account?.Address?.City, 
          CompanyName = registerInitializationContext.Supplier?.Name, 
          CompanyAddress = $"{registerInitializationContext.Account?.Address?.Street} - {registerInitializationContext.Account?.Address?.Number}", 
          CompanyRegistrationNumber = registerInitializationContext.Supplier?.RegistrationNumber, 
          StationName = registerInitializationContext.Register.Name, 
          OrganizationUnitEmail = registerInitializationContext.Supplier.EmailAddress
        };
        
        var requestModel = _infrasecNewStationEnrollmentRequestBuilder.BuildRequest(_swedenConfigs.MandatoryEnrollmentSettingFields, 
          enrollmentData, totalSwedishRegisters);
        var infrasecClient = _infrasecEnrollmentApiClientFactory.Create(_swedenConfigs);
        var response = await infrasecClient.SendInfrasecEnrollmentRequest(requestModel);
        var infrasecResponseModel =
          await _infrasecEnrollmentResponseValidator.HandleInfrasecResponse(response, AvailableInfrasecRequestActions.NEW.ToString(), 
            registerInitializationContext.Register.Id);
        
        if (string.IsNullOrWhiteSpace(infrasecResponseModel.IdmResponse.RegisterID))
        {
          throw new InfrasecRegisterIdNullOrWhitespace(
            $"Failed to register Station with id: {registerInitializationContext.Register.Id} with Infrasec");
        }

        // TODO(SEBI): Update register
        // TODO(SEBI): Use register.Number property for FiscalSystemID
        registerInitializationContext.Register.Data.Add("FiscalSystemId", infrasecResponseModel.IdmResponse.RegisterID);
        await _registerStore.Update(registerInitializationContext.Register);
      }
      catch (Exception ex)
      {
        // TODO(SEBI): Log
        // _log.Error($"{SwedenAuditingConstants.Identifier} - {nameof(SyncNewReceiptWithInfrasecService)} Exception: {ex.Message}", ex);
        throw;
      }
    }
  }
}