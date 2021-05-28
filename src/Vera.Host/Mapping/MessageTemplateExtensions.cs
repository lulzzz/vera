using System;
using System.Linq;
using Google.Protobuf;
using MessageTemplate = Vera.Grpc.MessageTemplate;

namespace Vera.Host.Mapping
{
    public static class MessageTemplateExtensions
    {
        public static Models.MessageTemplate Unpack(this MessageTemplate messageTemplate)
        {
            return new()
            {
                Footer = messageTemplate.Footer.ToList(),
                Logo = messageTemplate.Logo.ToByteArray(),
                AccountId = messageTemplate.AccountId
            };
        }

        public static MessageTemplate Pack(this Models.MessageTemplate messageTemplate)
        {
            var result = new MessageTemplate()
            {
                Logo = ByteString.CopyFrom(messageTemplate.Logo),
                AccountId = messageTemplate.AccountId,
                Footer =
                {
                    messageTemplate.Footer
                }
            };
            
            return result;
        }
    }
}