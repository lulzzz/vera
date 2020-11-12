using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Vera.WebApi.Controllers;
using Vera.WebApi.Models;
using Xunit;

namespace Vera.WebApi.Tests
{
    public class RegisterControllerTests
    {
        [Fact]
        public async Task Should_return_bad_request()
        {
            var mockUserRegisterFacade = new Mock<IUserRegisterFacade>();

            mockUserRegisterFacade
                .Setup(f => f.Register(It.IsAny<string>(), It.IsAny<UserToCreate>()))
                .Returns(Task.FromResult(new Error(ErrorCode.Exists, "test")));

            var controller = new RegisterController(mockUserRegisterFacade.Object);

            var result = await controller.Index(new Register
            {
                CompanyName = "New Black",
                Username = "vera",
                Password = "vera"
            });

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var error = Assert.IsType<ErrorResponse>(badRequestResult.Value);

            Assert.Equal("EXISTS", error.Code);
            Assert.Equal("test", error.Message);
        }

        [Fact]
        public async Task Should_return_ok()
        {
            var mockUserRegisterFacade = new Mock<IUserRegisterFacade>();

            mockUserRegisterFacade
                .Setup(f => f.Register(It.IsAny<string>(), It.IsAny<UserToCreate>()))
                .Returns(Task.FromResult<Error>(null));

            var controller = new RegisterController(mockUserRegisterFacade.Object);

            var result = await controller.Index(new Register
            {
                CompanyName = "New Black",
                Username = "vera",
                Password = "vera"
            });

            Assert.IsType<OkResult>(result);
        }
    }
}