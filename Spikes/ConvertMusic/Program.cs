using System;
using System.Threading;
using NEmplode.Codecs;
using NEmplode.IO;

namespace ConvertMusic
{
    class Program
    {
        static void Main(string[] args)
        {
            string sourceFileName = @"D:\Rips\Flac\The Crystal Method\Divided By Night\01 - Divided By Night.flac";
            string destinationFileName = @"D:\Rips\Temp\The Crystal Method\Divided By Night\01 - Divided By Night.mp3";

            using (var source = DecodingStream.OpenRead(sourceFileName))
            using (var destination = EncodingStream.OpenWrite(destinationFileName))
                source.CopyTo(destination);


#if false
            var SourceFileName = @"C:\Program Files (x86)\Flac\bin\flac.exe";
            var SourceArguments = @"--silent --decode --stdout ""D:\Rips\Flac\The Crystal Method\Divided By Night\01 - Divided By Night.flac""";
            var DestinationFileName = @"C:\Program Files (x86)\LAME\lame.exe";
            var DestinationArguments = @"--silent --preset standard --id3v2-only --pad-id3v2-size 256 - ""D:\Rips\Temp\The Crystal Method\Divided By Night\01 - Divided By Night.mp3""";


            using (
                var pipeline = new Pipeline(SourceFileName, SourceArguments, DestinationFileName, DestinationArguments))
            {
                pipeline.OutputDataReceived += (sender, e) => LogOutput(e.Data);
                pipeline.ErrorDataReceived += (sender, e) => LogError(e.Data);

                pipeline.Run(CancellationToken.None);
            }
#endif
        }

#if false
        private static void LogError(string data)
        {
            Console.Error.WriteLine(data);
        }

        private static void LogOutput(string data)
        {
            Console.Out.WriteLine(data);
        }
#endif
    }
}
