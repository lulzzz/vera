using Vera.Models;
using Vera.Security;

namespace Vera.WebApi.Security
{
    public interface ISecurityTokenGenerator
    {
        string Generate(User user, Company company);
    }
}