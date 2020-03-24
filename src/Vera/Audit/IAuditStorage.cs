using System.IO;
using System.Threading.Tasks;

namespace Vera.Audit
{
    public interface IAuditStorage
    {
        Task Store(Stream s);
    }
}