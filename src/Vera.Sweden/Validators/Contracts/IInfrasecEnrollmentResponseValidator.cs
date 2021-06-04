using System;
using System.Net.Http;
using System.Threading.Tasks;
using EVA.Auditing.Sweden.Models.Responses;

namespace Vera.Sweden.Validators.Contracts
{
  public interface IInfrasecEnrollmentResponseValidator
  {
    Task<InfrasecEnrollmentResponse> HandleInfrasecResponse(HttpResponseMessage response, string action, Guid registerId);
  }
}