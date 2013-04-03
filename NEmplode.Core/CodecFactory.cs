using System.Configuration;

namespace NEmplode
{
    internal static class CodecFactory
    {
        public static IMediaCodec CreateDecoder(string sourceFileName)
        {
            var decoderFileName = ConfigurationManager.AppSettings["DecoderPath"];
            var decoderArguments = ConfigurationManager.AppSettings["DecoderArguments"];

            return new CodecProcess(decoderFileName, decoderArguments) { ErrorDataFilter = CodecProcess.FlacErrorDataFilter };
        }

        public static IMediaCodec CreateEncoder(string destinationFileName)
        {
            var encoderFileName = ConfigurationManager.AppSettings["EncoderPath"];
            var encoderArguments = ConfigurationManager.AppSettings["EncoderArguments"];

            return new CodecProcess(encoderFileName, encoderArguments) { ErrorDataFilter = CodecProcess.LameErrorDataFilter };
        }
    }
}