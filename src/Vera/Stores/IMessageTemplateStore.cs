using System;
using System.Threading.Tasks;
using Vera.Models;

namespace Vera.Stores
{
    public interface IMessageTemplateStore
    {
        Task Store(MessageTemplate messageTemplate);

        Task Update(MessageTemplate messageTemplate);

        Task<MessageTemplate> Get(string accountId, Guid id);
    }
}