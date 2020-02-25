using System;
using System.Collections.Generic;
using System.Linq;

namespace Vera.Models
{
    public class Invoice
    {
        public Invoice(Invoice other)
        {
            Id = other.Id;
            SystemId = other.SystemId;
            Number = other.Number;
            Store = other.Store;
            Customer = other.Customer;
            Employee = other.Employee;
            Timestamp = other.Timestamp;
            TerminalId = other.TerminalId;
            Manual = other.Manual;
            FiscalPeriod = other.FiscalPeriod;
            FiscalYear = other.FiscalYear;
            Sequence = other.Sequence;
            RawSignature = other.RawSignature;
            Signature = other.Signature;
            Lines = new List<InvoiceLine>(other.Lines ?? Enumerable.Empty<InvoiceLine>());
            Payments = new List<Payment>(other.Payments ?? Enumerable.Empty<Payment>());
            Settlements = new List<Settlement>(other.Settlements ?? Enumerable.Empty<Settlement>());
        }

        public Invoice() { }

        public Guid Id { get; set; }
        public string SystemId { get; set; }

        public string Number { get; set; }

        public Store Store { get; set; }
        public Customer Customer { get; set; }
        public Employee Employee { get; set; }

        public DateTime Timestamp { get; set; }

        public string TerminalId { get; set; }

        public bool Manual { get; set; }

        public int FiscalPeriod { get; set; }
        public int FiscalYear { get; set; }

        public int Sequence { get; set; }
        public string RawSignature { get; set; }
        public byte[] Signature { get; set; }

        public ICollection<InvoiceLine> Lines { get; set; }
        public ICollection<Payment> Payments { get; set; }
        public ICollection<Settlement> Settlements { get; set; }
    }
}