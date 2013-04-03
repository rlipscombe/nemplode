using System.Configuration;
using System.IO;

namespace NEmplode
{
    internal static class CodecFactory
    {
        public static IMediaCodec CreateDecoder(string sourceFileName)
        {
            var extension = Path.GetExtension(sourceFileName);
            if (string.IsNullOrWhiteSpace(extension))
            {
                throw new DecoderNotRegisteredException(sourceFileName);
            }

            if (extension.ToLowerInvariant() == ".flac")
            {
                var decoderFileName = ConfigurationManager.AppSettings["DecoderPath"];
                const string decoderArguments = @"--silent --decode --stdout -";

                return new CodecProcess(decoderFileName, decoderArguments)
                    {
                        ErrorDataFilter = CodecProcess.FlacErrorDataFilter
                    };
            }

            throw new DecoderNotRegisteredException(sourceFileName);
        }

        public static IMediaCodec CreateEncoder(string destinationFileName)
        {
            var extension = Path.GetExtension(destinationFileName);
            if (string.IsNullOrWhiteSpace(extension))
            {
                throw new EncoderNotRegisteredException(destinationFileName);
            }
            
            if (extension.ToLowerInvariant() == ".mp3")
            {
                var encoderFileName = ConfigurationManager.AppSettings["EncoderPath"];

                // Now for the fun part: LAME requires an actual file to write to, in order to have somewhere to put the VBR TOC.
                return new LameEncoder(encoderFileName);
            }

            throw new EncoderNotRegisteredException(destinationFileName);
        }
    }
}