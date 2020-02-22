using System.Collections.Generic;
using System.Threading.Tasks;

namespace Vera.Audit
{
    public interface IAuditArchive<T>
    {
        Task<ICollection<T>> Get(AuditCriteria criteria);

        Task Archive(AuditCriteria criteria, T result);
    }
}