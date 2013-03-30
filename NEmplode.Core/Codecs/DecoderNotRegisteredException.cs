using System;

namespace NEmplode.Codecs
{
    public class DecoderNotRegisteredException : Exception
    {
        public DecoderNotRegisteredException(string path)
            : base(string.Format("There is no decoder registered to handle '{0}'.", path))
        {
        }
    }
}