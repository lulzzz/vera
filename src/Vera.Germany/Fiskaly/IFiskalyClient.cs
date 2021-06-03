namespace Vera.Germany.Fiskaly
{
    /// <summary>
    /// The client to handle communication with fiskaly service
    /// </summary>
    public interface IFiskalyClient
    {
        /// <summary>
        /// Authenticate a client based on api key and secret
        /// </summary>
        /// <returns>Access token</returns>
        string Authenticate();

        /// <summary>
        /// Get tss by id
        /// </summary>
        /// <param name="tssId"></param>
        /// <returns>A model that contains tss related data</returns>
        GetTssModelResponse GetTss(string tssId);

        /// <summary>
        /// Creates a tss for one supplier based on <paramref name="createTssModel"/>
        /// </summary>
        /// <param name="createTssModel"></param>
        void CreateTss(CreateTssModel createTssModel);

        /// <summary>
        /// Get client by <paramref name="clientId"/> and <paramref name="tssId"/>
        /// </summary>
        /// <param name="tssId"></param>
        /// <param name="clientId"></param>
        /// <returns>A model that contains client related data</returns>
        GetClientModelResponse GetClient(string clientId, string tssId);

        /// <summary>
        /// Creates a client for one register based on <paramref name="createClientModel"/>
        /// </summary>
        /// <param name="createClientModel"></param>
        void CreateClient(CreateClientModel createClientModel);

        /// <summary>
        /// Creates a transaction for each invoice. 
        /// The transaction is set to 'ACTIVE' state and then immediately to the 'FINISHED' state, 2 round trips 
        /// to Fiskaly are required. Latest revision must be provided for the finishing step.
        /// </summary>
        /// <param name="createTransactionModel"></param>
        /// <returns>A model that contains the signature for one invoice.</returns>
        TransactionModelResponse CreateTransaction(CreateTransactionModel createTransactionModel);
    }
}
