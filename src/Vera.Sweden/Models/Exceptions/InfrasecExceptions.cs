using System;

namespace Vera.Sweden.Models.Exceptions
{
    public class InfrasecRegisterIdNullOrWhitespace : ArgumentException
    {
        public InfrasecRegisterIdNullOrWhitespace(string message)
            : base(message)
        {
        }
    }
    
    public class RegisterWithThisIdAlreadyEnrolled : Exception
    {
        public RegisterWithThisIdAlreadyEnrolled(string message)
            : base(message)
        {
        }
    }
    
    public class EnrollmentFailed : Exception
    {
        public EnrollmentFailed(string message)
            : base(message)
        {
        }
    }
}