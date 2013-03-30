using System;
using System.Threading;
using NEmplode.IO;

namespace EncodeMusic
{
    static class Program
    {
        static void Main(string[] args)
        {
            string FileName = @"C:\Program Files (x86)\Flac\bin\flac.exe";
            string Arguments = @"--silent --best --force -o ""D:\Rips\Temp\05 - Rihanna - Diamonds.flac"" ""D:\Rips\Various\Now 84 - CD1\05 - Rihanna - Diamonds.wav""";

            using (
                var process = new CapturedProcess(FileName, Arguments))
            {
                process.OutputDataReceived += (sender, e) => LogOutput(e.Data);
                process.ErrorDataReceived += (sender, e) => LogError(e.Data);

                process.Run(CancellationToken.None);
            }
        }

        private static void LogError(string data)
        {
            Console.Error.WriteLine(data);
        }

        private static void LogOutput(string data)
        {
            Console.Out.WriteLine(data);
        }
    }
}
