using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Vera.Models;
using Vera.Security;
using Vera.Services;
using Vera.Stores;
using Vera.WebApi.Models;

namespace Vera.WebApi.Controllers
{
    [ApiController]
    [Route("register")]
    public class RegisterController : ControllerBase
    {
        private readonly IUserRegisterService _userRegisterService;

        public RegisterController(IUserRegisterService userRegisterService)
        {
            _userRegisterService = userRegisterService;
        }

        [HttpPost]
        public async Task<IActionResult> Index(Register model)
        {
            var result = await _userRegisterService.Register(
                model.CompanyName,
                new UserToCreate
                {
                    Username = model.Username,
                    Password = model.Password,
                    Type = UserType.Admin
                }
            );

            if (result != null)
            {
                return BadRequest(new ErrorResponse(result));
            }

            return Ok();
        }


    }
}