using System.Threading.Tasks;

namespace Vera.Audit
{
    public interface IAuditTransformer<T>
    {
        Task<T> Transform(Models.Audit audit);
    }
}