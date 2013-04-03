using System;

namespace NEmplode
{
    internal class CodecProcessFailedException : Exception
    {
        public CodecProcessFailedException(int exitCode)
        {
        }
    }
}