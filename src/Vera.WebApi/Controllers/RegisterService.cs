using System.Threading.Tasks;
using Grpc.Core;
using Vera.Grpc;
using Vera.Grpc.Shared;
using Vera.Models;
using Vera.Services;

namespace Vera.WebApi.Controllers
{
    public class RegisterService : Grpc.RegisterService.RegisterServiceBase
    {
        private readonly IUserRegisterService _userRegisterService;

        public RegisterService(IUserRegisterService userRegisterService)
        {
            _userRegisterService = userRegisterService;
        }

        public override async Task<Empty> Register(RegisterRequest request, ServerCallContext context)
        {
            var result = await _userRegisterService.Register(
                request.CompanyName,
                new UserToCreate
                {
                    Username = request.Username,
                    Password = request.Password,
                    Type = UserType.Admin
                }
            );

            if (result != null)
            {
                throw new RpcException(new Status(StatusCode.FailedPrecondition, result.Message));
            }

            return new Empty();
        }
    }
}