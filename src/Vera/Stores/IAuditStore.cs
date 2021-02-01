using System;
using System.Threading.Tasks;
using Vera.Audits;
using Vera.Models;

namespace Vera.Stores
{
    public interface IAuditStore
    {
        /// <summary>
        /// Creates a new audit entry based on the given criteria.
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        Task<Audit> Create(AuditCriteria criteria);

        /// <summary>
        /// Returns the audit based on the given parameters.
        /// </summary>
        /// <param name="accountId">Id of the account that the audit was created on</param>
        /// <param name="auditId">Id of the audit itself</param>
        /// <returns></returns>
        Task<Audit> Get(Guid accountId, Guid auditId);

        /// <summary>
        /// Updates an existing audit. Note that this won't update all of the fields
        /// if mutated.
        /// </summary>
        /// <param name="audit"></param>
        /// <returns></returns>
        Task Update(Audit audit);
    }
}