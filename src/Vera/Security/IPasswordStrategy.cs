using System;

namespace Vera.Security
{
    public interface IPasswordStrategy
    {
        Authentication Encrypt(string password);
        bool Verify(string password, Authentication authentication);
    }
}