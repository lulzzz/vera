using System.Threading.Tasks;

namespace Vera.Audit
{
    public interface IAuditTransformer<TResult>
    {
        TResult Transform(AuditContext context, AuditCriteria criteria, StandardAuditFileTaxation.Audit audit);
    }
}