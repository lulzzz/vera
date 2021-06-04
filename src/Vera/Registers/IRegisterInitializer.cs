using System.Threading.Tasks;
using Vera.Models;

namespace Vera.Registers
{
    public interface IRegisterInitializer
    {
        /// <summary>
        /// Initializes the register and changes its status to Open (<see cref="RegisterStatus"/>)
        /// </summary>
        Task Initialize(RegisterInitializationContext context);
    }
}
