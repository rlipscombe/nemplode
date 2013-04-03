namespace NEmplode
{
    internal class DecoderNotRegisteredException : CodecNotRegisteredException
    {
        public DecoderNotRegisteredException(string fileName)
            : base(string.Format("There is no decoder registered to handle '{0}'.", fileName))
        {
        }
    }
}