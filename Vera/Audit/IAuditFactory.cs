using System.Threading.Tasks;

namespace Vera.Audit
{
    public interface IAuditFactory<T>
    {
        Task<IAuditArchive<T>> CreateAuditArchive();
        Task<IAuditTransformer<T>> CreateAuditTransformer();
    }
}