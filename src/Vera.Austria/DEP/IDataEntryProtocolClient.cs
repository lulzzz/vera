using System;
using System.Threading.Tasks;

namespace Vera.Austria.DEP
{
    public sealed class DEPException : Exception
    {
        public DEPException(State state, string description) : base($"Error while pushing to DEP ({state.ToString()}): {description}")
        {
        }
    }

    public interface IDataEntryProtocolClient
    {
        Task Push(string entry, string signedEntry);
    }

    public interface IDataEntryProtocolClientFactory
    {
        IDataEntryProtocolClient Create();
    }
}