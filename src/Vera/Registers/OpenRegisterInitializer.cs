using System.Threading.Tasks;
using Vera.Models;

namespace Vera.Registers
{
    public class OpenRegisterInitializer : IRegisterInitializer
    {
        public Task Initialize(Register register)
        {
            register.Status = RegisterStatus.Open;

            return Task.CompletedTask;
        }
    }
}
