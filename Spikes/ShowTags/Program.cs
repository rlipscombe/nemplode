using System;
using System.Text;
using TagLib;
using TagLib.Id3v2;

namespace ShowTags
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var file = TagLib.File.Create(args[0]))
            {
                var tag = (TagLib.Id3v2.Tag)file.GetTag(TagTypes.Id3v2, create: false);
                var collectionGroupIdFrame = PrivateFrame.Get(tag, "WM/WMCollectionGroupID", create: false);
                var privateData = collectionGroupIdFrame.PrivateData.Data;
                var collectionGroupId = new Guid(privateData);
                var collectionId = PrivateFrame.Get(tag, "WM/WMCollectionID", create: false);
                var contentId = PrivateFrame.Get(tag, "WM/WMContentID", create: false);

                var providerFrame = PrivateFrame.Get(tag, "WM/Provider", create: false);
                var providerName = Encoding.Unicode.GetString(providerFrame.PrivateData.Data);
                Console.WriteLine(tag);
            }
        }
    }
}
