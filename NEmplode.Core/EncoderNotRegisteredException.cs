namespace NEmplode
{
    internal class EncoderNotRegisteredException : CodecNotRegisteredException
    {
        public EncoderNotRegisteredException(string fileName)
            : base(string.Format("There is no encoder registered to handle '{0}'.", fileName))
        {
        }
    }
}