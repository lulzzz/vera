using System.Threading.Tasks;
using Vera.Models;
using Vera.Registers;

namespace Vera.Sweden
{
    public class SwedenRegisterCloser : IRegisterCloser
    {
        public Task Close(Register register)
        {
            // TODO(SEBI): Integrate Infrasec Enrollment
            register.Status = RegisterStatus.Closed;

            return Task.CompletedTask;
        }
    }
}