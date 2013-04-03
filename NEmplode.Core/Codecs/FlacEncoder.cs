namespace NEmplode.Codecs
{
    internal class FlacEncoder : EncoderProcessWithTemporaryFile
    {
        public FlacEncoder(string encoderFileName)
            : base(encoderFileName)
        {
        }

        protected override string GetEncoderArguments(string tempFileName)
        {
            const string encoderArgumentsFormat =
                @"--silent --best -f -o ""{0}"" -";
            return string.Format(encoderArgumentsFormat, tempFileName);
        }
    }
}