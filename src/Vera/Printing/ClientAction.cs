namespace Vera.Printing
{
    public enum ClientAction
    {
        /// <summary>
        /// Expecting a read from the client.
        /// </summary>
        Read,
        
        /// <summary>
        /// Client should forward payload to the printer.
        /// </summary>
        Write,
        
        /// <summary>
        /// Done with the operations, can close call.
        /// </summary>
        Done
    }
}