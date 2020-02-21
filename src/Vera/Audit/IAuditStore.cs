using System.Threading.Tasks;

namespace Vera.Audit
{
    public interface IAuditStore
    {
        Task Store();
    }
}