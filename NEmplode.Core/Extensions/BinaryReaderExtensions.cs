using System.IO;

namespace NEmplode.Extensions
{
    public static class BinaryReaderExtensions
    {
        public static bool EndOfStream(this BinaryReader reader)
        {
            // Note that this assumes that the stream is seekable.
            return reader.BaseStream.Position >= reader.BaseStream.Length;
        }
    }
}