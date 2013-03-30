using NEmplode.Codecs;

namespace ConvertMusic
{
    static class Program
    {
        static void Main(string[] args)
        {
            const string sourceFileName = @"D:\Rips\Temp\01 - Divided By Night.flac";
            const string destinationFileName = @"D:\Rips\Temp\01 - Divided By Night.mp3";

            using (var source = DecodingStream.OpenRead(sourceFileName))
            using (var destination = EncodingStream.OpenWrite(destinationFileName))
                source.CopyTo(destination);
        }
    }
}
