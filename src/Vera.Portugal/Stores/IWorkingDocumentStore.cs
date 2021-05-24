using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vera.Portugal.Models;

namespace Vera.Portugal.Stores
{
    public interface IWorkingDocumentStore
    {
        Task Store(WorkingDocument document);
        Task Delete(WorkingDocument document);
        Task<ICollection<WorkingDocument>> List(Guid invoiceId);
    }
}
