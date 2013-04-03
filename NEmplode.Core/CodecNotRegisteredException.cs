using System;

namespace NEmplode
{
    internal class CodecNotRegisteredException : Exception
    {
        protected CodecNotRegisteredException(string message)
            : base(message)
        {
        }
    }
}