using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Vera.Configuration
{
    public interface IAuditConfiguration
    {
        Option Get(string key);
        Task Set(string key, byte[] value);
        IEnumerable<Option> List();
    }

    public class Option
    {
        public string Key { get; set; }
        public byte[] Value { get; set; }
        public string Description { get; set; }
    }
}