using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using EVA.Auditing.Sweden.Models.Responses;
using FakeItEasy;
using Vera.Models;
using Vera.Registers;
using Vera.Stores;
using Vera.Sweden.InfrasecHttpClient.Contracts;
using Vera.Sweden.Models.Configs;
using Vera.Sweden.Models.Enums;
using Vera.Sweden.Models.Exceptions;
using Vera.Sweden.Models.Records;
using Vera.Sweden.Models.Requests.EnrollmentApi;
using Vera.Sweden.RequestBuilders.Contracts;
using Vera.Sweden.Services;
using Vera.Sweden.Tests.TestHelpers;
using Vera.Sweden.Validators.Contracts;
using Xunit;

namespace Vera.Sweden.Tests.Services
{
  public class SyncNewStationWithInfrasecServiceTests
  {
    private readonly IRegisterInitializer _service;
    private readonly IInfrasecEnrollmentApiClientFactory _infrasecEnrollmentApiClientFactory;
    private readonly IInfrasecEnrollmentResponseValidator _validator;
    private readonly IInfrasecNewStationEnrollmentRequestBuilder _requestBuilder;
    private readonly IRegisterStore _registerStore;
    private readonly SwedenConfigs _swedenConfigs;

    private readonly IInfrasecClient _infrasecClient;

    public SyncNewStationWithInfrasecServiceTests()
    {
      _swedenConfigs = InfrasecModelBuilderTestHelper.BuildSwedenConfigs();
      _infrasecEnrollmentApiClientFactory = A.Fake<IInfrasecEnrollmentApiClientFactory>();
      _validator = A.Fake<IInfrasecEnrollmentResponseValidator>();
      _requestBuilder = A.Fake<IInfrasecNewStationEnrollmentRequestBuilder>();
      _registerStore = A.Fake<IRegisterStore>();
      _service = new SyncNewStationWithInfrasecService(_infrasecEnrollmentApiClientFactory, 
        _swedenConfigs, 
        _requestBuilder, 
        _validator
      ,_registerStore);

      _infrasecClient = A.Fake<IInfrasecClient>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task When_Infrasec_RegisterID_Is_Null_Or_Whitespace_Will_Throw(string infrasecRegisterId)
    {
      const int swedishStationCount = 14;

      var registerId = Guid.Parse("6AC1BFFC-78FF-4F7A-8F3D-CE2063DE8467");
      var supplierId = Guid.Parse("C8604803-10F9-46A2-94BC-9A167998207B");

      var registerInitializationContext = new RegisterInitializationContext(
        InfrasecModelBuilderTestHelper.CreateAccount(),
        InfrasecModelBuilderTestHelper.CreateSupplier(id: supplierId),
        InfrasecModelBuilderTestHelper.CreateRegister(id: registerId));
      
      var register = registerInitializationContext.Register;
      
      SetupMocks(register, swedishStationCount, infrasecRegisterId, supplierId);
      
      var exception = await Assert.ThrowsAsync<InfrasecRegisterIdNullOrWhitespace>(async () =>
      {
        await _service.Initialize(registerInitializationContext);
      });

      Assert.Contains(registerId.ToString(), exception.Message);

      A.CallTo(() => _registerStore.GetTotalRegisters(registerInitializationContext.Supplier.Id)).MustHaveHappenedOnceExactly();
      A.CallTo(() => _requestBuilder.BuildRequest(A<MandatoryEnrollmentSettingFields>.Ignored, 
        A<InfrasecEnrollmentRecord>.Ignored, swedishStationCount)).MustHaveHappenedOnceExactly();
      A.CallTo(() => _infrasecEnrollmentApiClientFactory.Create(_swedenConfigs)).MustHaveHappenedOnceExactly();
      A.CallTo(() => _infrasecClient.SendInfrasecEnrollmentRequest(A<InfrasecNewOrUpdateEnrollmentRequest>.Ignored)).MustHaveHappenedOnceExactly();
      A.CallTo(() => _validator.HandleInfrasecResponse(A<HttpResponseMessage>.Ignored,
        AvailableInfrasecRequestActions.NEW.ToString(), register.Id)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _registerStore.Update(A<Register>.Ignored)).MustNotHaveHappened();
    }

    [Fact]
    public async Task Will_Enroll_New_Station_With_Infrasec()
    {
      const int swedishStationCount = 14;

      var registerId = Guid.Parse("6AC1BFFC-78FF-4F7A-8F3D-CE2063DE8467");
      var supplierId = Guid.Parse("C8604803-10F9-46A2-94BC-9A167998207B");
      const string infrasecRegisterId = "Infrasec-Mock-Register-Unique-Identity";

      var registerInitializationContext = new RegisterInitializationContext(
        InfrasecModelBuilderTestHelper.CreateAccount(),
        InfrasecModelBuilderTestHelper.CreateSupplier(id: supplierId),
        InfrasecModelBuilderTestHelper.CreateRegister(id: registerId));
      
      var register = registerInitializationContext.Register;
      
      SetupMocks(register, swedishStationCount, infrasecRegisterId, supplierId);
      
      await _service.Initialize(registerInitializationContext);
   
      A.CallTo(() => _registerStore.GetTotalRegisters(registerInitializationContext.Supplier.Id)).MustHaveHappenedOnceExactly();
      A.CallTo(() => _requestBuilder.BuildRequest(A<MandatoryEnrollmentSettingFields>.Ignored, 
        A<InfrasecEnrollmentRecord>.Ignored, swedishStationCount)).MustHaveHappenedOnceExactly();
      A.CallTo(() => _infrasecEnrollmentApiClientFactory.Create(_swedenConfigs)).MustHaveHappenedOnceExactly();
      A.CallTo(() => _infrasecClient.SendInfrasecEnrollmentRequest(A<InfrasecNewOrUpdateEnrollmentRequest>.Ignored)).MustHaveHappenedOnceExactly();
      A.CallTo(() => _validator.HandleInfrasecResponse(A<HttpResponseMessage>.Ignored,
        AvailableInfrasecRequestActions.NEW.ToString(), register.Id)).MustHaveHappenedOnceExactly();
      A.CallTo(() => _registerStore.Update(registerInitializationContext.Register)).MustHaveHappenedOnceExactly();

      Assert.Equal(registerInitializationContext.Register.Data["FiscalSystemId"], infrasecRegisterId);
    }
    
    private void SetupMocks(Register register, int currentStationNumber, string infrasecRegisterId, Guid supplierId)
    {
      A.CallTo(() => _registerStore.GetTotalRegisters(supplierId)).Returns(currentStationNumber);
      A.CallTo(() => _infrasecClient.SendInfrasecEnrollmentRequest(A<string>.Ignored)).Returns(new HttpResponseMessage());
      A.CallTo(() => _requestBuilder.BuildRequest(A<MandatoryEnrollmentSettingFields>.Ignored, A<InfrasecEnrollmentRecord>.Ignored,
        currentStationNumber)).Returns(new InfrasecNewOrUpdateEnrollmentRequest());
      A.CallTo(() => _infrasecEnrollmentApiClientFactory.Create(A<SwedenConfigs>.Ignored)).Returns(_infrasecClient);
      A.CallTo(() => _validator.HandleInfrasecResponse(A<HttpResponseMessage>.Ignored,
        AvailableInfrasecRequestActions.NEW.ToString(), register.Id)).Returns(new InfrasecEnrollmentResponse
      {
        IdmResponse = new InfrasecEnrollmentResponse.IdmResponseData
        {
          RegisterID = infrasecRegisterId
        }
      });
    }
  }
}