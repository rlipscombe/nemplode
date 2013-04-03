using System;

namespace NEmplode.Codecs
{
    internal class CodecProcessFailedException : Exception
    {
        public CodecProcessFailedException(int exitCode)
        {
        }
    }
}