using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Vera.Models;
using Vera.Security;
using Vera.Stores;
using Vera.WebApi.Models;

namespace Vera.WebApi.Controllers
{
    [ApiController]
    [Route("register")]
    public class RegisterController : ControllerBase
    {
        private readonly IUserRegisterFacade _userRegisterFacade;
        public RegisterController(IUserRegisterFacade userRegisterFacade)
        {
            _userRegisterFacade = userRegisterFacade;
        }

        [HttpPost]
        public async Task<IActionResult> Index(Register model)
        {
            var result = await _userRegisterFacade.Register(
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