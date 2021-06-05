using System.Threading.Tasks;
using Vera.Dependencies.Handlers;
using Vera.Models;

namespace Vera.Periods
{
    public interface IPeriodCloser
    {
        Task ClosePeriod(IHandlerChain<RegisterReport> handler, PeriodClosingContext closingContext);
    }
}
