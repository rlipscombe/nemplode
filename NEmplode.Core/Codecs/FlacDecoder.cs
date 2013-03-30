using System.IO;

namespace NEmplode.Codecs
{
    public class FlacDecoder
    {
        private FlacDecoder(Stream stream)
        {
        }

        public static Stream OpenRead(string path)
        {
            Stream stream = File.OpenRead(path);
            return new FlacDecoderStream(stream);
        }
    }
}