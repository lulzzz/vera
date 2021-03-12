using System;

namespace Vera.Austria.ATrust.Session
{
  public sealed class ATrustSessionExpiredException : Exception
  {
    public ATrustSessionExpiredException(IATrustSession session)
      : base($"Session {session.Username} has expired, please start a new one and try again.")
    {
    }
  }
}