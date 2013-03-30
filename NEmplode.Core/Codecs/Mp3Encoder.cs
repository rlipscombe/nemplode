using System.IO;

namespace NEmplode.Codecs
{
    public class Mp3Encoder
    {
        public static Stream OpenWrite(string path)
        {
            var stream = File.Create(path);
            return new Mp3EncoderStream(stream);
        }
    }
}