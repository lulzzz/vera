using System;
using System.Collections.Generic;

namespace Vera.Germany.Fiskaly
{
    public class CreateTssModel
    {
        public string TssId { get; set; }
        public string Description { get; set; }
        public TssState TssState { get; set; }
    }

    public class GetTssModelResponse
    {
        public string _id { get; set; }
        public string Description { get; set; }
        public string State { get; set; }
        public Dictionary<string, string> Metadata { get; set; }
    }

    public class CreateClientModel
    {
        public string TssId { get; set; }
        public string ClientId { get; set; }
        public string SerialNumber { get; set; }
    }

    public class GetClientModelResponse
    {
        public string SerialNumber { get; set; }
        public Dictionary<string, string> Metadata { get; set; }
    }

    public class CreateTransactionModel
    {
        public string TssId { get; set; }
        public string ClientId { get; set; }
        public string TxId { get; set; }
        public List<(string taxCategory, decimal grossTotal)> Taxes { get; set; }
        public List<(bool isCash, decimal amount)> Payments { get; set; }
        public string Currency { get; set; }

        /// <summary>
        /// TODO(andrei): if 'IsFinished' update 'last_revision' metadata property
        /// store lastRevision in invoice or use 'now' everytime?
        /// </summary>
        public bool IsFinished { get; set; }
    }

    public class TransactionModelResponse
    {
        public long LatestRevision { get; set; }
        public SignatureModel Signature { get; set; }

        public class SignatureModel
        {
            public string Value { get; set; }
        }
    }

    public class FiskalyException : Exception
    {
        public FiskalyException(string message) : base(message)
        {
        }

        public FiskalyException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    public enum TssState
    {
        UNINITIALIZED,
        INITIALIZED,
        DISABLED
    }
}
