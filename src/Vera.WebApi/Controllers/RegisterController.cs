using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Vera.Security;
using Vera.WebApi.Models;

namespace Vera.WebApi.Controllers
{
    [ApiController]
    [Route("register")]
    public class RegisterController : ControllerBase
    {
        private readonly ICompanyStore _companyStore;
        private readonly IUserStore _userStore;
        private readonly IPasswordStrategy _passwordStrategy;

        public RegisterController(ICompanyStore companyStore, IUserStore userStore, IPasswordStrategy passwordStrategy)
        {
            _companyStore = companyStore;
            _userStore = userStore;
            _passwordStrategy = passwordStrategy;
        }

        [HttpPost]
        public async Task<IActionResult> Index(Register model)
        {
            var facade = new UserRegisterFacade(_companyStore, _userStore, _passwordStrategy);

            await facade.Register(
                model.CompanyName,
                new UserToCreate
                {
                    Username = model.Username,
                    Password = model.Password,
                    Type = UserType.Admin
                }
            );

            return Ok();
        }


    }
}