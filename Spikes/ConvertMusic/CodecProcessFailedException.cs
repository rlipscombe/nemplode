using System;

namespace ConvertMusic
{
    internal class CodecProcessFailedException : Exception
    {
        public CodecProcessFailedException(int exitCode)
        {
        }
    }
}