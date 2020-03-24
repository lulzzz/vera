using System.Threading.Tasks;

namespace Vera.Audit
{
    public interface IAuditTransformer<TResult>
    {
        Task<TResult> Transform(AuditContext context, StandardAuditFileTaxation.Audit audit);
    }
}