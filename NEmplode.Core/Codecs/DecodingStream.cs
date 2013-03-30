using System.IO;

namespace NEmplode.Codecs
{
    public static class DecodingStream
    {
        public static Stream OpenRead(string path)
        {
            var extension = Path.GetExtension(path);
            if (extension == ".flac")
                return FlacDecoder.OpenRead(path);
            if (extension == ".mp3")
                return Mp3Decoder.OpenRead(path);
            if (extension == ".wav")
                return WavDecoder.OpenRead(path);

            throw new DecoderNotRegisteredException(path);
        }
    }
}