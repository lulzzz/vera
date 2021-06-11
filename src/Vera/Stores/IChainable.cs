using System.Threading.Tasks;
using Vera.Models;

namespace Vera.Stores
{
    public interface IChainable
    {
        /// <summary>
        /// Append a new chainable to this chainable.
        /// </summary>
        /// <param name="signature"></param>
        /// <returns></returns>
        Task Append(Signature signature, decimal cumulatedValue = 0);
        
        /// <summary>
        /// Sequence that will follow this chainable.
        /// </summary>
        int NextSequence { get; }
        
        /// <summary>
        /// Signature of this chainable. Can be null if this is the first
        /// chainable in the link.
        /// </summary>
        Signature? Signature { get; }

        /// <summary>
        /// The Cumulated Value of the entire chain
        /// </summary>
        decimal CumulatedValue { get; }
    }
}
