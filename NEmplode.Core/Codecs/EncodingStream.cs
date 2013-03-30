using System.IO;

namespace NEmplode.Codecs
{
    public static class EncodingStream
    {
        public static Stream OpenWrite(string path)
        {
            var extension = Path.GetExtension(path);
            if (extension == ".flac")
                return FlacEncoder.OpenWrite(path);
            if (extension == ".mp3")
                return Mp3Encoder.OpenWrite(path);
            if (extension == ".wav")
                return WavEncoder.OpenWrite(path);

            throw new EncoderNotRegisteredException(path);
        }
    }
}
