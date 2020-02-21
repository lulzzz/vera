using System.Collections.Generic;
using System.Threading.Tasks;

namespace Vera.Audit
{
    public sealed class AuditCriteria
    {
        public int FiscalYear { get; set; }
        public int FiscalPeriod { get; set; }
    }

    public interface IAuditTransformer<T>
    {
        Task<T> Transform(Models.Audit audit);
    }

    public interface IAuditArchive<T>
    {
        Task<ICollection<T>> Get(AuditCriteria criteria);

        Task Archive(AuditCriteria criteria, T result);
    }
}