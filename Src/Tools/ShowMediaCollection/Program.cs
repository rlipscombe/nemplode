using System;

namespace ShowMediaCollection
{
    class Program
    {
        static void Main(string[] args)
        {
            var wmp = new WMPLib.WindowsMediaPlayer();
            var mediaCollection = wmp.mediaCollection;
            var playlist = mediaCollection.getAll();
            for (int i = 0; i < playlist.count; ++i)
            {
                var media = playlist.Item[i];
                for (int j = 0; j < media.attributeCount; ++j)
                {
                    var attributeName = media.getAttributeName(j);
                    var attributeValue = media.getItemInfo(attributeName);
                    Console.WriteLine("{0}: {1}", attributeName, attributeValue);
                }

                Console.WriteLine();
            }
        }
    }
}
