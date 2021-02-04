using Vera.Models;

namespace Vera.Host.Security
{
    public interface ISecurityTokenGenerator
    {
        string Generate(User user, Company company);
    }
}