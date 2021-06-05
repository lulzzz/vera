using System;
using System.Threading.Tasks;
using Vera.Models;

namespace Vera.EventLogs
{
    public interface IEventLogCreator
    {
        Task Create(EventLog log);
    }
}
