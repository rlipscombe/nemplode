using System;
using System.IO;
using System.Text;

namespace ShowMediaCollection
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            var wmp = new WMPLib.WindowsMediaPlayer();
            var mediaCollection = wmp.mediaCollection;
            var playlist = mediaCollection.getAll();

            var path = DateTime.UtcNow.ToString("yyyyMMddTHHmmss") + ".txt";
            using (var stream = File.Create(path))
            using (var writer = new StreamWriter(stream, Encoding.UTF8))
            {
                var count = playlist.count;
                for (int i = 0; i < count; ++i)
                {
                    if (i % 100 == 0)
                        Console.WriteLine("{0} / {1}: {2}%", i, count, 100 * i / count);

                    var mediaItem = playlist.Item[i];
                    for (int j = 0; j < mediaItem.attributeCount; ++j)
                    {
                        var attributeName = mediaItem.getAttributeName(j);
                        var attributeValue = mediaItem.getItemInfo(attributeName);

                        writer.WriteLine("{0}: {1}", attributeName, attributeValue);
                    }
                }
            }
        }
    }
}
