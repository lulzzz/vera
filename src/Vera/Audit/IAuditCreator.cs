using System.Threading.Tasks;

namespace Vera.Audit
{
    public interface IAuditCreator<out TResult>
    {
        TResult Create(AuditContext context, AuditCriteria criteria);
    }
}