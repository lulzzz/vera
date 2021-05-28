using System;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Vera.Grpc;
using Vera.Host.Mapping;
using Vera.Host.Security;
using Vera.Stores;

namespace Vera.Host.Services
{
    [Authorize]
    public class MessageTemplateService : Grpc.MessageTemplateService.MessageTemplateServiceBase
    {
        private readonly IMessageTemplateStore _messageTemplateStore;
        private readonly IAccountStore _accountStore;

        public MessageTemplateService(IMessageTemplateStore messageTemplateStore, IAccountStore accountStore)
        {
            _messageTemplateStore = messageTemplateStore;
            _accountStore = accountStore;
        }


        public override async Task<CreateMessageTemplateReply> Create(CreateMessageTemplateRequest request,
            ServerCallContext context)
        {
            var messageTemplate = request.MessageTemplate.Unpack();
            
            await _messageTemplateStore.Store(messageTemplate);

            return new CreateMessageTemplateReply {Id = messageTemplate.Id.ToString()};
        }

        public override async Task<UpdateMessageTemplateReply> Update(UpdateMessageTemplateRequest request,
            ServerCallContext context)
        {
            var account = await context.ResolveAccount(_accountStore);

            var original = await _messageTemplateStore.Get(account.Id.ToString(), Guid.Parse(request.Id));

            if (original == null)
            {
                throw new RpcException(new Status(
                    StatusCode.InvalidArgument,
                    $"message template does not exist"
                ));
            }

            if (request.MessageTemplate.Logo != null)
            {
                original.Logo = request.MessageTemplate.Logo.ToByteArray();
            }
            
            if (request.MessageTemplate.Footer != null)
            {
                original.Footer = request.MessageTemplate.Footer.ToList();
            }

            await _messageTemplateStore.Update(original);

            return new UpdateMessageTemplateReply { Id = original.Id.ToString() };
        }

        public override async Task<GetMessageTemplateByIdReply> Get(GetMessageTemplateByIdRequest request, ServerCallContext context)
        {
            var account = await context.ResolveAccount(_accountStore);

            var found = await _messageTemplateStore.Get(account.Id.ToString(), Guid.Parse(request.Id));

            if (found == null)
            {
                return new GetMessageTemplateByIdReply();
            }

            return new GetMessageTemplateByIdReply
            {
                MessageTemplate = found.Pack()
            };
        }
    }
}