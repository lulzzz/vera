using System;
using System.Threading.Tasks;

namespace Vera.Stores
{
    public class ChainContext
    {
        public ChainContext(Guid accountId, string bucket)
        {
            AccountId = accountId;
            Bucket = bucket;
        }

        public Guid AccountId { get; }
        public string Bucket { get; }

        public override string ToString()
        {
            return $"{nameof(AccountId)}: {AccountId}, {nameof(Bucket)}: {Bucket}";
        }
    }
    
    public interface IChainStore
    {
        Task<IChainable> Last(ChainContext context);
    }
}