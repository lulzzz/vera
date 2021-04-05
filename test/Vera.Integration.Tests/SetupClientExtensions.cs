using Google.Protobuf.WellKnownTypes;
using System;
using System.Threading.Tasks;
using Vera.Grpc;

namespace Vera.Integration.Tests
{
    public static class SetupClientExtensions
    {
        public static async Task<GetAuditReply> GetAuditReplyAsync(this SetupClient client, string auditId)
        {
            var reply = new GetAuditReply();
            for (var i = 0; i < 10; i++)
            {
                // Wait a little before the audit is finished
                await Task.Delay(100);

                var getAuditRequest = new GetAuditRequest
                {
                    AccountId = client.AccountId,
                    AuditId = auditId
                };

                using var getAuditCall = client.Audit.GetAsync(getAuditRequest, client.AuthorizedMetadata);

                reply = await getAuditCall.ResponseAsync;

                if (!string.IsNullOrEmpty(reply.Location))
                {
                    break;
                }
            }
            return reply;
        }

        public static async Task<GetAuditReply> GenerateAuditFile(this SetupClient client)
        {
            var createAuditRequest = new CreateAuditRequest
            {
                AccountId = client.AccountId,
                SupplierSystemId = client.SupplierSystemId,
                StartDate = DateTime.UtcNow.AddDays(-1).ToTimestamp(),
                EndDate = DateTime.UtcNow.ToTimestamp()
            };
            var createAuditReply = await client.Audit.CreateAsync(createAuditRequest, client.AuthorizedMetadata);
            var getAuditReply = await client.GetAuditReplyAsync(createAuditReply.AuditId);

            return getAuditReply;
        }

    }
}
