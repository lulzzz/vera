using System.Threading.Tasks;
using Vera.Models;

namespace Vera.Registers
{
    public class SimpleRegisterInitializer : IRegisterInitializer
    {
        public Task Initialize(RegisterInitializationContext context)
        {
            context.Register.Status = RegisterStatus.Open;

            return Task.CompletedTask;
        }
    }
}
