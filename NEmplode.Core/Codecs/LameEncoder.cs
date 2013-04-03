namespace NEmplode.Codecs
{
    internal class LameEncoder : EncoderProcessWithTemporaryFile
    {
        public LameEncoder(string encoderFileName)
            : base(encoderFileName)
        {
        }

        protected override string GetEncoderArguments(string tempFileName)
        {
            const string encoderArgumentsFormat = @"--silent --preset standard --id3v2-only --pad-id3v2-size 256 - ""{0}""";
            return string.Format(encoderArgumentsFormat, tempFileName);
        }
    }
}