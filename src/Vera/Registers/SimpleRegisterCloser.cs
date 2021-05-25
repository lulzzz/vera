using System.Threading.Tasks;
using Vera.Models;

namespace Vera.Registers
{
    public class SimpleRegisterCloser : IRegisterCloser
    {
        public Task Close(Register register)
        {
            register.Status = RegisterStatus.Closed;

            return Task.CompletedTask;
        }
    }
}
