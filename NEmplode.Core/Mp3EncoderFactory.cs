using System.Configuration;

namespace NEmplode
{
    internal class Mp3EncoderFactory
    {
        public static CodecProcess Create()
        {
            // BUG: The encoder (LAME) needs to write to a temporary file, otherwise we get no VBR header.
            string encoderPath = ConfigurationManager.AppSettings["EncoderPath"];
            string encoderArguments = ConfigurationManager.AppSettings["EncoderArguments"];

            return new CodecProcess(encoderPath, encoderArguments)
                {
                    ErrorDataFilter = CodecProcess.LameErrorDataFilter
                };
        }
    }
}