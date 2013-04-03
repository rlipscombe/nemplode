using System;

namespace NEmplode
{
    internal class CodecNotRegisteredException : Exception
    {
        public CodecNotRegisteredException(string fileName)
            : base(string.Format("There is no codec registered to handle '{0}'.", fileName))
        {
        }
    }
}