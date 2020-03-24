using System.Threading.Tasks;

namespace Vera.Audit
{
    public interface IAuditTransformer<TResult, TConfig>
    {
        Task<TResult> Transform(AuditContext<TConfig> context, StandardAuditFileTaxation.Audit audit);
    }
}