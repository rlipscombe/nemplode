using System.Configuration;

namespace NEmplode
{
    internal class FlacDecoderFactory
    {
        public static CodecProcess Create()
        {
            string decoderPath = ConfigurationManager.AppSettings["DecoderPath"];
            string decoderArguments = ConfigurationManager.AppSettings["DecoderArguments"];

            return new CodecProcess(decoderPath, decoderArguments)
                {
                    ErrorDataFilter = CodecProcess.FlacErrorDataFilter
                };
        }
    }
}