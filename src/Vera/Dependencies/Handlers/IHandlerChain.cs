using System.Threading.Tasks;

namespace Vera.Dependencies.Handlers
{
    public interface IHandlerChain<T>
    {
        Task Handle(T entity);
        IHandlerChain<T> WithNext(IHandlerChain<T> next);
    }

    public abstract class HandlerChain<T> : IHandlerChain<T>
    {
        private IHandlerChain<T> _next;

        public virtual Task Handle(T entity)
        {
            return _next?.Handle(entity) ?? Task.CompletedTask;
        }

        public IHandlerChain<T> WithNext(IHandlerChain<T> next)
        {
            _next = next;
            return next;
        }
    }
}
