using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bogus;
using Google.Protobuf;
using Vera.Grpc;
using Xunit;

namespace Vera.Integration.Tests.Common
{
    public class MessageTemplateTests : IClassFixture<ApiWebApplicationFactory>
    {
        private readonly Faker _faker;
        private readonly Setup _setup;

        public MessageTemplateTests(ApiWebApplicationFactory fixture)
        {
            _faker = new Faker();
            _setup = fixture.CreateSetup();
        }

        [Fact]
        public async Task Should_Be_Able_To_Create_Message_Templates()
        {
            var accountContext = new AccountContext
            {
                AccountName = _faker.Company.CompanyName(),
                Certification = "123",
                SupplierSystemId = _faker.Random.AlphaNumeric(10)
            };

            var bytes = new byte[] {1};
            var footer = new List<string> {"a", "b", "c"};

            var client = await _setup.CreateClient(accountContext);

            var createReply = await client.MessageTemplateClient.CreateAsync(new CreateMessageTemplateRequest
            {
                MessageTemplate = new MessageTemplate
                {
                    Footer = { footer },
                    Logo = ByteString.CopyFrom(bytes),
                    AccountId = client.AccountId
                }
            }, client.AuthorizedMetadata);

            var id = createReply.Id;

            var getReply = await client.MessageTemplateClient.GetAsync(
                new GetMessageTemplateByIdRequest
                {
                    Id = id
                }, client.AuthorizedMetadata);

            Assert.Equal(footer, getReply.MessageTemplate.Footer.ToList());
            Assert.Equal(bytes, getReply.MessageTemplate.Logo.ToByteArray());
        }

        [Fact]
        public async Task Should_Be_Able_To_Update_Message_Templates()
        {
            var accountContext = new AccountContext
            {
                AccountName = _faker.Company.CompanyName(),
                Certification = "123",
                SupplierSystemId = _faker.Random.AlphaNumeric(10)
            };

            var bytes = new byte[] {1, 2, 3, 4, 56, 7};
            var footer = new List<string> {"a", "b", "c"};

            var client = await _setup.CreateClient(accountContext);

            var createReply = await client.MessageTemplateClient.CreateAsync(new CreateMessageTemplateRequest
            {
                MessageTemplate = new MessageTemplate
                {
                    Footer = { footer },
                    Logo = ByteString.CopyFrom(bytes),
                    AccountId = client.AccountId
                }
            }, client.AuthorizedMetadata);

            var id = createReply.Id;
            
            var getReply = await client.MessageTemplateClient.GetAsync(
                new GetMessageTemplateByIdRequest
                {
                    Id = id
                }, client.AuthorizedMetadata);

            Assert.Equal(footer, getReply.MessageTemplate.Footer.ToList());
            Assert.Equal(bytes, getReply.MessageTemplate.Logo.ToByteArray());

            bytes = new byte[] {1, 2, 3, 4};
            footer = new List<string> {"new", "footer", "lines"};

            await client.MessageTemplateClient.UpdateAsync(new UpdateMessageTemplateRequest
            {
                Id = id,
                MessageTemplate = new MessageTemplate
                {
                    Footer = { footer },
                    Logo = ByteString.CopyFrom(bytes),
                    AccountId = client.AccountId
                }
            }, client.AuthorizedMetadata);

            getReply = await client.MessageTemplateClient.GetAsync(
                new GetMessageTemplateByIdRequest
                {
                    Id = id
                }, client.AuthorizedMetadata);

            Assert.Equal(footer, getReply.MessageTemplate.Footer.ToList());
            Assert.Equal(bytes, getReply.MessageTemplate.Logo.ToByteArray());
        }
    }
}