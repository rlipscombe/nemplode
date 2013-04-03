using System;

namespace NEmplode.Codecs
{
    internal class CodecNotRegisteredException : Exception
    {
        protected CodecNotRegisteredException(string message)
            : base(message)
        {
        }
    }
}