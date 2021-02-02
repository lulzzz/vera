using System.IO;
using System.Threading.Tasks;

namespace Vera.Audits
{
    public interface IAuditWriter
    {
        /// <summary>
        /// Should return the name of the file that is going to be written. Note that this should also include
        /// any extensions.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="sequence"></param>
        /// <param name="total"></param>
        /// <returns></returns>
        Task<string> ResolveName(AuditContext context, int sequence, int total);

        /// <summary>
        /// Write the contents of the audit to the given stream.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="criteria"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        Task Write(AuditContext context, AuditCriteria criteria, Stream stream);
    }
}