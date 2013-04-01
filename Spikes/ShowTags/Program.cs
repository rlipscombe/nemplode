using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TagLib.Id3v2;

namespace ShowTags
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            string path = args[0];
            var file = TagLib.File.Create(path);
            switch (Path.GetExtension(path))
            {
                case ".flac":
                    {
                        var flac = (TagLib.Flac.Metadata) file.GetTag(TagLib.TagTypes.FlacMetadata, create: false);
                        //flac.Tags[0].

                        // flac.Tags is a list of the tags contained. In general, it's just a XiphComment.
                        //Console.WriteLine(
                        //    string.Join(Environment.NewLine,
                        //    flac.Tags.SelectMany(x=>x.).AsEnumerable()));
                        var xiph = (TagLib.Ogg.XiphComment)file.GetTag(TagLib.TagTypes.Xiph, create: false);
                        foreach (var key in xiph.OrderBy(x=>x))
                        {
                            var values = xiph.GetField(key);
                            foreach (var value in values)
                            {
                                Console.WriteLine("{0}: '{1}'", key, value);
                            }
                        }
                    }
                    break;

                case ".mp3":
                    {
                        var tag = (TagLib.Id3v2.Tag) file.GetTag(TagLib.TagTypes.Id3v2, create: false);
                        var comparer = new FrameComparer();
                        Console.WriteLine(
                            string.Join(Environment.NewLine,
                                        tag.GetFrames()
                                           .OrderBy(f => f, comparer)
                                           .Select(AsString)));
                    }
                    break;
            }
        }

        private static string AsString(PrivateFrame privateFrame)
        {
            switch (privateFrame.Owner)
            {
                case "WM/UniqueFileIdentifier":
                case "WM/Provider":
                    return string.Format(
                        "{0}: '{1}': '{2}' (UTF-16)", privateFrame.FrameId,
                        privateFrame.Owner,
                        Encoding.Unicode.GetString(
                            privateFrame.PrivateData.Data));

                case "WM/MediaClassPrimaryID":
                case "WM/MediaClassSecondaryID":
                case "WM/CollectionGroupID":
                case "WM/CollectionID":
                case "WM/ContentID":
                case "WM/WMCollectionGroupID":
                case "WM/WMCollectionID":
                case "WM/WMContentID":
                    return string.Format("{0}: '{1}': {2} (GUID)",
                                         privateFrame.FrameId,
                                         privateFrame.Owner,
                                         new Guid(privateFrame.PrivateData.Data));

                default:
                    return string.Format("{0}: '{1}': '{2}'",
                                         privateFrame.FrameId, privateFrame.Owner,
                                         privateFrame.PrivateData);
            }
        }

        private static string AsString(UniqueFileIdentifierFrame ufif)
        {
            return string.Format("{0}: '{1}': '{2}'", ufif.FrameId, ufif.Owner, ufif.Identifier);
        }

        private static string AsString(Frame frame)
        {
            if (frame.FrameId == "COMM")
            {
                var cf = (CommentsFrame) frame;
                return string.Format("{0}: '{1}': '{2}'", cf.FrameId, cf.Description, cf.Text);
            }

            if (frame.FrameId == "PRIV")
            {
                var pf = (TagLib.Id3v2.PrivateFrame) frame;
                return AsString(pf);
            }

            if (frame.FrameId == "UFID")
            {
                var ufif = (UniqueFileIdentifierFrame) frame;
                return AsString(ufif);
            }

            if (frame.FrameId == "IPLS")
            {
                /* [0] = Encoding (1 = UTF-16)
                 * Then it's BOM,key,BOM,value, until the data runs out.
                 */

                var uf = (UnknownFrame) frame;
                return string.Format("{0}: {1}", uf.Data[0], BitConverter.ToString(uf.Data.Data));
            }

            return string.Format("{0}: '{1}'", frame.FrameId, frame.ToString());
        }

        private class FrameComparer : IComparer<Frame>
        {
            public int Compare(Frame x, Frame y)
            {
                if (x.FrameId == "PRIV" && y.FrameId == "PRIV")
                {
                    var xx = (PrivateFrame) x;
                    var yy = (PrivateFrame) y;

                    return xx.Owner.CompareTo(yy.Owner);
                }

                if (x.FrameId == "COMM" && y.FrameId == "COMM")
                {
                    var xx = (CommentsFrame)x;
                    var yy = (CommentsFrame)y;

                    return xx.Description.CompareTo(yy.Description);
                }

                if (x.FrameId == "TXXX" && y.FrameId == "TXXX")
                {
                    var xx = (UserTextInformationFrame)x;
                    var yy = (UserTextInformationFrame)y;

                    return xx.Description.CompareTo(yy.Description);
                }

                return x.FrameId.CompareTo(y.FrameId);
            }
        }
    }
}