using System;

namespace NEmplode.Codecs
{
    public class EncoderNotRegisteredException : Exception
    {
        public EncoderNotRegisteredException(string path)
            : base(string.Format("There is no encoder registered to handle '{0}'.", path))
        {
        }
    }
}