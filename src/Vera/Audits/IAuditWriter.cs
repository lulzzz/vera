using System.IO;
using System.Threading.Tasks;

namespace Vera.Audits
{
    public interface IAuditWriter
    {
        Task Write(AuditContext context, AuditCriteria criteria, Stream stream);
    }
}