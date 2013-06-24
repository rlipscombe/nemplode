using System;
using System.Linq;
using System.Net;
using NEmplode.Empeg;

namespace NEmplode.Tool
{
    static class Program
    {
        static void Main(string[] args)
        {
            //var source = new LocalEmpegDatabaseSource(@"test_data\crowley");
            var source = new HijackEmpegDatabaseSource(IPAddress.Parse("10.0.0.25"));
            //var source = new LocalEmpegDatabaseSource(@"test_data\toothgnip");
            var databaseReader = new EmpegDatabaseReader(source);
            var database = databaseReader.ReadDatabase();
            var databaseName = database.Name;

            for (; ; )
            {
                var currentLocation = "\\";
                var prompt = string.Format("{0}:{1}> ", databaseName, currentLocation);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write(prompt);
                Console.ForegroundColor = ConsoleColor.White;
                var input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input))
                    continue;

                if (input == "exit" || input == "quit")
                    break;

                // TODO: Proper lexing.
                var parts = input.Split(' ');
                switch (parts[0])
                {
                    case "ls":
                    case "dir":
                        {
                            var currentItem = (IEmpegPlaylist) database.GetItem(currentLocation);
                            var children = currentItem.GetChildren();

                            foreach (var child in children)
                            {
                                Console.WriteLine(child);
                            }
                        }
                        break;

                    case "cd":
                        {
                            var currentItem = (IEmpegPlaylist) database.GetItem(currentLocation);
                            var children = currentItem.GetChildren();
                            children.SingleOrDefault();
                        }
                        break;

                    default:
                        Console.WriteLine("Unrecognised '{0}'", parts[0]);
                        break;
                }
            }
        }

        private static void Recurse(IEmpegItem item, int indent)
        {
            if (item == null)
                throw new ArgumentNullException("item");

            Console.WriteLine("{0}{1}", "".PadLeft(indent), item);

            if (item.IsPlaylist)
            {
                var playlist = (IEmpegPlaylist)item;
                foreach (var child in playlist.GetChildren())
                {
                    Recurse(child, indent + 2);
                }
            }
        }
    }
}
