using System.Threading.Tasks;
using Vera.Models;

namespace Vera.Registers
{
    public interface IRegisterCloser
    {
        /// <summary>
        /// Closes the register and changes its status to Closed (<see cref="RegisterStatus"/>)
        /// </summary>
        Task Close(Register register);
    }
}
