using System.Threading.Tasks;

namespace Vera.Audit
{
    public interface IAuditFactory<T>
    {
        IAuditArchive<T> CreateAuditArchive();
        IAuditTransformer<T> CreateAuditTransformer();
    }
}