using System.Threading.Tasks;
using Grpc.Core;
using Vera.Grpc;
using Vera.Grpc.Shared;
using Vera.Models;
using Vera.Services;

namespace Vera.Host.Services
{
    public class UserRegisterService : Grpc.UserRegisterService.UserRegisterServiceBase
    {
        private readonly IUserRegisterService _userRegisterService;

        public UserRegisterService(IUserRegisterService userRegisterService)
        {
            _userRegisterService = userRegisterService;
        }

        public override async Task<Empty> RegisterUser(RegisterUserRequest request, ServerCallContext context)
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