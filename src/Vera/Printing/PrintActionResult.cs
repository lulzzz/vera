namespace Vera.Printing
{
    public class PrintActionResult
    {
        /// <summary>
        /// Next action for the client to be taken
        /// </summary>
        public ClientAction Action { get; set; }
        
        /// <summary>
        /// Next action in the chain to be executed
        /// </summary>
        public IPrintAction? NextAction { get; set; }
        
        /// <summary>
        /// Payload to be sent to the client
        /// </summary>
        public byte[]? Payload { get; set; }
    }
}