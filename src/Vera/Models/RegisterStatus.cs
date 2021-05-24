namespace Vera.Models
{
    public enum RegisterStatus
    {
        // Additional steps required to open, no transactions allowed, yet
        Pending,

        // Ready for transactions
        Open,

        // No longer accepting transactions
        Closed
    }
}
