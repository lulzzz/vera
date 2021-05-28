using System;
using Vera.Printing;

namespace Vera.Host.Mapping
{
    public static class PrinterExtensions
    {
        public static Grpc.ClientAction Pack(this ClientAction action)
        {
            var result = action switch
            {
                ClientAction.Read => Grpc.ClientAction.Read,
                ClientAction.Write => Grpc.ClientAction.Write,
                ClientAction.Done => Grpc.ClientAction.Done,
                _ => throw new ArgumentOutOfRangeException(nameof(action), action, "unknown operation")
            };

            return result;
        }
    }
}