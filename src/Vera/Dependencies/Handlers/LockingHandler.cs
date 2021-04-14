using System;
using System.Threading.Tasks;
using Vera.Concurrency;

namespace Vera.Dependencies.Handlers
{
    public class LockingHandler<T> : HandlerChain<T>
    {
        private readonly IHandlerChain<T> _handler;
        private readonly ILocker _locker;
        private readonly IBucketGenerator<T> _bucketGenerator;

        public LockingHandler(IHandlerChain<T> handler, ILocker locker,
            IBucketGenerator<T> bucketGenerator)
        {
            _handler = handler;
            _locker = locker;
            _bucketGenerator = bucketGenerator;
        }

        public override async Task Handle(T entity)
        {
            var bucket = _bucketGenerator.Generate(entity);

            // Lock on the unique sequence of the report so no other report can enter
            // the bucket to ensure the sequence stays in-order
            await using (await _locker.Lock(bucket, TimeSpan.FromSeconds(15)))
            {
                // Delegate to inner handler to handle something that needs to happen within the lock
                await _handler.Handle(entity);
            }

            // Continue with the outer handler(s)
            await base.Handle(entity);
        }
    }
}
