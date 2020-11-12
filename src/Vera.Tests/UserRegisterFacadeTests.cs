using System;
using System.Threading.Tasks;
using Moq;
using Vera.Models;
using Vera.Security;
using Vera.Stores;
using Xunit;

namespace Vera.Tests
{
    public class UserRegisterFacadeTests
    {
        [Fact]
        public async Task Should_return_error_when_user_exists()
        {
            var company = new Company
            {
                Name = "vera",
                Id = new Guid()
            };

            var userToCreate = new UserToCreate
            {
                Username = "vera"
            };

            var mockCompanyStore = new Mock<ICompanyStore>();
            mockCompanyStore
                .Setup(x => x.GetByName(company.Name))
                .Returns(Task.FromResult(company));

            var mockUserStore = new Mock<IUserStore>();
            mockUserStore
                .Setup(x => x.GetByCompany(company.Id, userToCreate.Username))
                .Returns(Task.FromResult(new User()));

            var mockPasswordStrategy = new Mock<IPasswordStrategy>();

            var facade = new UserRegisterFacade(
                mockCompanyStore.Object,
                mockUserStore.Object,
                mockPasswordStrategy.Object
            );

            var result = await facade.Register(company.Name, userToCreate);

            Assert.NotNull(result);
            Assert.Equal(ErrorCode.Exists, result.Code);
        }

        [Fact]
        public async Task Should_create_company_and_user()
        {
            var company = new Company
            {
                Name = "vera",
                Id = new Guid()
            };

            var userToCreate = new UserToCreate
            {
                Username = "vera"
            };

            var mockCompanyStore = new Mock<ICompanyStore>();
            mockCompanyStore
                .Setup(x => x.GetByName(company.Name))
                .Returns(Task.FromResult<Company>(null));

            mockCompanyStore
                .Setup(x => x.Store(It.IsAny<Company>()))
                .Returns(Task.FromResult(company));

            var mockUserStore = new Mock<IUserStore>();
            mockUserStore
                .Setup(x => x.GetByCompany(company.Id, userToCreate.Username))
                .Returns(Task.FromResult<User>(null));

            var mockPasswordStrategy = new Mock<IPasswordStrategy>();

            var facade = new UserRegisterFacade(
                mockCompanyStore.Object,
                mockUserStore.Object,
                mockPasswordStrategy.Object
            );

            await facade.Register(company.Name, userToCreate);

            mockUserStore.Verify(x => x.Store(It.Is<User>(u => u.Username == userToCreate.Username)));
            mockUserStore.VerifyNoOtherCalls();
        }
    }
}