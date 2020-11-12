using System;
using Vera.Models;

namespace Vera.Security
{
    public interface IPasswordStrategy
    {
        /// <summary>
        /// Encrypts the given password and returns an authentication object.
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        Authentication Encrypt(string password);

        /// <summary>
        /// Verifies that the given password matches the values stored in the authentication object.
        /// </summary>
        /// <param name="password"></param>
        /// <param name="authentication"></param>
        /// <returns></returns>
        bool Verify(string password, Authentication authentication);
    }
}