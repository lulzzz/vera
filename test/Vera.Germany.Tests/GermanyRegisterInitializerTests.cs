using Moq;
using Vera.Germany.Fiskaly;
using Vera.Models;
using Xunit;

namespace Vera.Germany.Tests
{
    public class GermanyRegisterInitializerTests
    {
        [Fact]
        public void Should_get_tss_and_open_register()
        {
            var fiskalyClient = new Mock<IFiskalyClient>();
            var initalizer = new GermanyRegisterInitializer(fiskalyClient.Object);

            fiskalyClient.Setup(c => c.GetTss(It.IsAny<string>()))
                .Returns(new GetTssModelResponse());

            var register = new Register { Status = RegisterStatus.Pending };
            initalizer.Initialize(register);

            Assert.Equal(RegisterStatus.Open, register.Status);
        }

        [Fact]
        public void Should_create_tss_and_open_register()
        {
            var fiskalyClient = new Mock<IFiskalyClient>();
            var initalizer = new GermanyRegisterInitializer(fiskalyClient.Object);

            fiskalyClient.SetupSequence(c => c.GetTss(It.IsAny<string>()))
                .Returns((GetTssModelResponse)null)
                .Returns(new GetTssModelResponse());

            var register = new Register { Status = RegisterStatus.Pending };

            initalizer.Initialize(register);

            fiskalyClient.Verify(c => c.CreateTss(It.IsAny<CreateTssModel>()));

            Assert.Equal(RegisterStatus.Open, register.Status);
        }

        [Fact]
        public void Should_throw_and_pending_register()
        {
            var fiskalyClient = new Mock<IFiskalyClient>();
            var initalizer = new GermanyRegisterInitializer(fiskalyClient.Object);

            fiskalyClient.Setup(c => c.GetTss(It.IsAny<string>()))
                .Throws(new FiskalyException("ex message"));

            fiskalyClient.Setup(c => c.CreateTss(It.IsAny<CreateTssModel>()))
                .Throws(new FiskalyException("ex message"));

            var register = new Register { Status = RegisterStatus.Pending };

            Assert.ThrowsAsync<FiskalyException>(() => initalizer.Initialize(register));

            Assert.Equal(RegisterStatus.Pending, register.Status);
        }

        [Fact]
        public void Should_get_client_and_open_register()
        {
            var fiskalyClient = new Mock<IFiskalyClient>();
            var initalizer = new GermanyRegisterInitializer(fiskalyClient.Object);

            fiskalyClient.Setup(c => c.GetTss(It.IsAny<string>()))
                .Returns(new GetTssModelResponse());

            fiskalyClient.Setup(c => c.GetClient(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new GetClientModelResponse());

            var register = new Register { Status = RegisterStatus.Pending };
            initalizer.Initialize(register);

            Assert.Equal(RegisterStatus.Open, register.Status);
        }

        [Fact]
        public void Should_create_client_and_open_register()
        {
            var fiskalyClient = new Mock<IFiskalyClient>();
            var initalizer = new GermanyRegisterInitializer(fiskalyClient.Object);

            fiskalyClient.Setup(c => c.GetTss(It.IsAny<string>()))
                .Returns(new GetTssModelResponse());

            fiskalyClient.Setup(c => c.GetClient(It.IsAny<string>(), It.IsAny<string>()))
                .Returns((GetClientModelResponse)null);

            var register = new Register { Status = RegisterStatus.Pending };
            initalizer.Initialize(register);

            fiskalyClient.Verify(c => c.CreateClient(It.IsAny<CreateClientModel>()));

            Assert.Equal(RegisterStatus.Open, register.Status);
        }

        [Fact]
        public void Should_throw_on_client_create_and_pending_register()
        {
            var fiskalyClient = new Mock<IFiskalyClient>();
            var initalizer = new GermanyRegisterInitializer(fiskalyClient.Object);

            fiskalyClient.Setup(c => c.GetTss(It.IsAny<string>()))
                .Returns(new GetTssModelResponse());

            fiskalyClient.Setup(c => c.GetClient(It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new FiskalyException("ex message"));

            fiskalyClient.Setup(c => c.CreateClient(It.IsAny<CreateClientModel>()))
                .Throws(new FiskalyException("ex message"));

            var register = new Register { Status = RegisterStatus.Pending };

            Assert.ThrowsAsync<FiskalyException>(() => initalizer.Initialize(register));

            Assert.Equal(RegisterStatus.Pending, register.Status);
        }
    }
}
