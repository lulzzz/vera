namespace Vera.Models
{
    public enum RegisterReportType
    {
        /// <summary>
        /// Daily report that can be generated during an open period. Used to show
        /// intermediate results of the transactions within a period thus far.
        /// Also known as the X report.
        /// </summary>
        Current,

        /// <summary>
        /// Daily closing report that is generated during the closing of a period. Used
        /// to show the final results of a period.
        /// Also known as the Z report.
        /// </summary>
        EndOfDay
    }
}
