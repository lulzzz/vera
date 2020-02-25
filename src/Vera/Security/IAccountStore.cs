using System.Threading.Tasks;

namespace Vera.Security
{
    public interface IAccountStore
    {
        Task Store(Account account);
    }
}