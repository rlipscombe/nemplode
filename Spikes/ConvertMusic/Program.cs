using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NEmplode;
using NEmplode.Tasks;
using TagLib;
using TagLib.Id3v2;
using TagLib.Ogg;
using Tag = TagLib.Id3v2.Tag;

namespace ConvertMusic
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            var rand = new Random();

            if (args.Length != 2)
            {
                Console.WriteLine("ConvertMusic source-root destination-root");
            }

            string sourceRoot = args[0];
            string destinationRoot = args[1];

            var cancellationTokenSource = new CancellationTokenSource();
            Console.CancelKeyPress += (sender, e) =>
                {
                    Console.WriteLine("^C");
                    cancellationTokenSource.Cancel();
                    e.Cancel = true;
                };

            int maxDegreeOfParallelism = Environment.ProcessorCount;

            // TODO: We need a recursive search that allows cancellation. Obviously, throwing OperationCanceledException works, but...
            var sourceFiles = Directory.EnumerateFiles(sourceRoot, "*.flac", SearchOption.AllDirectories);
            Concurrent.ForEach(sourceFiles, maxDegreeOfParallelism, cancellationTokenSource.Token, sourceFileName =>
                {
                    // TODO: This is not how to figure out the destination.
                    var destinationFileName = Path.ChangeExtension(sourceFileName, ".mp3");
                    return
                        Transcoder.ConvertAsync(sourceFileName, destinationFileName, cancellationTokenSource.Token)
                                  .ContinueWith(t =>
                                      {
                                          if (!t.IsFaulted && !t.IsCanceled)
                                              CopyTags(sourceFileName, destinationFileName);
                                      });
                });
        }

        private static void CopyTags(string sourceFileName, string destinationFileName)
        {
            using (var sourceFile = TagLib.File.Create(sourceFileName))
            using (var destinationFile = TagLib.File.Create(destinationFileName))
            {
                var sourceTag = (TagLib.Ogg.XiphComment)sourceFile.GetTag(TagTypes.Xiph, create: false);
                var destinationTag = (TagLib.Id3v2.Tag)destinationFile.GetTag(TagTypes.Id3v2, create: true);

                CopyTag(sourceTag, "TITLE", destinationTag, "TIT2");
                CopyTag(sourceTag, "ARTIST", destinationTag, "TPE1");
                CopyTag(sourceTag, "ALBUM", destinationTag, "TALB");

                CopyTrackNumberTag(sourceTag, destinationTag);
                
                // TDOR <- Year / TDRC <- yyyy-MM-dd
                var date = sourceTag.GetFirstField("DATE");
                if (!string.IsNullOrWhiteSpace(date))
                {
                    var yearFrame = TextInformationFrame.Get(destinationTag, "TDOR", create: true);
                    yearFrame.Text = new[] {date.Substring(0, 4)};
                    destinationTag.AddFrame(yearFrame);

                    var dateFrame = TextInformationFrame.Get(destinationTag, "TDRC", create: true);
                    dateFrame.Text = new[] { date };
                    destinationTag.AddFrame(dateFrame);
                }

                CopyTag(sourceTag, "ALBUMARTIST", destinationTag, "TPE2");
                CopyTag(sourceTag, "ALBUMARTISTSORT", destinationTag, "TSO2");
                CopyTag(sourceTag, "ARTISTSORT", destinationTag, "TSOP");

                // ASIN: (ASIN) [TXXX [ASIN]]
                // Barcode: (BARCODE) [TXXX [BARCODE]]
                // Catalogue Number: (CATLOGNUMBER) [TXXX [CATALOGNUMBER]]
                CopyUserTextTag(sourceTag, "ASIN", destinationTag, "ASIN");
                CopyUserTextTag(sourceTag, "BARCODE", destinationTag, "BARCODE");
                CopyUserTextTag(sourceTag, "CATALOGNUMBER", destinationTag, "CATALOGNUMBER");
                
                CopyCommentTags(sourceTag, destinationTag);

                CopyTag(sourceTag, "COMPILATION", destinationTag, "TCMP");

                CopyDiscNumberTag(sourceTag, destinationTag);

                // The next two are from lastfmplus.
                CopyTag(sourceTag, "GENRE", destinationTag, "TCON");
                CopyTag(sourceTag, "GROUPING", destinationTag, "TIT1");

                CopyTag(sourceTag, "ISRC", destinationTag, "TSRC");
                CopyTag(sourceTag, "LABEL", destinationTag, "TPUB");
                CopyTag(sourceTag, "MEDIA", destinationTag, "TMED");
                
                // Mood -> ??? (from lastfmplus)

                // MusicBrainz Release Artist Id: 89ad... (MUSICBRAINZ_ALBUMARTISTID) [TXXX [MusicBrainz Album Artist Id]]
                // MusicBrainz Release Id: 7d36... (MUSICBRAINZ_ALBUMID) [TXXX [MusicBrainz Album Id]]
                // MusicBrainz Artist Id: dbbc... (MUSICBRAINZ_ARTISTID) [TXXX [MusicBrainz Artist Id]]
                CopyMusicBrainzId(sourceTag, "MUSICBRAINZ_ALBUMARTISTID", destinationTag, "MusicBrainz Album Artist Id");
                CopyMusicBrainzId(sourceTag, "MUSICBRAINZ_ALBUMID", destinationTag, "MusicBrainz Album Id");
                CopyMusicBrainzId(sourceTag, "MUSICBRAINZ_ARTISTID", destinationTag, "MusicBrainz Artist Id");

                // MusicBrainz Recording Id: 91ab...  (MUSICBRAINZ_TRACKID) [UFID]
                CopyUniqueFileId(sourceTag, "MUSICBRAINZ_TRACKID", destinationTag, "http://musicbrainz.org");

                // Release Country: GB (RELEASECOUNTRY) [TXXX [MusicBrainz Album Release Country]]
                // Release Status: official (RELEASESTATUS) [TXXX [MusicBrainz Album Status]]
                // Release Type: compilation (RELEASETYPE) [TXXX [MusicBrainz Album Type]]
                // Script: Latn (SCRIPT) [TXXX [SCRIPT]]
                // TODO: Use the official MusicBrainz tags.
                CopyUserTextTag(sourceTag, "RELEASECOUNTRY", destinationTag, "MusicBrainz Album Release Country");
                CopyUserTextTag(sourceTag, "RELEASESTATUS", destinationTag, "MusicBrainz Album Status");
                CopyUserTextTag(sourceTag, "RELEASETYPE", destinationTag, "MusicBrainz Album Type");
                CopyUserTextTag(sourceTag, "SCRIPT", destinationTag, "SCRIPT");

                // PRIV:WM/WMCollectionGroupID <- MUSICBRAINZ_ALBUMID
                // PRIV:WM/WMCollectionID <- MUSICBRAINZ_ALBUMID
                // PRIV:WM/WMContentID <- MUSICBRAINZ_TRACKID
                CreateWindowsMediaTag(sourceTag.MusicBrainzReleaseId, destinationTag, "WM/WMCollectionGroupID");
                CreateWindowsMediaTag(sourceTag.MusicBrainzReleaseId, destinationTag, "WM/WMCollectionID");
                CreateWindowsMediaTag(sourceTag.MusicBrainzTrackId, destinationTag, "WM/WMContentID");

                destinationFile.Save();
            }
        }

        private static void CreateWindowsMediaTag(string value, Tag destinationTag, string owner)
        {
            var frame = PrivateFrame.Get(destinationTag, owner, create: true);
            var releaseId = Guid.NewGuid();
            if (!string.IsNullOrWhiteSpace(value))
                frame.PrivateData = new ByteVector(releaseId.ToByteArray());

            destinationTag.AddFrame(frame);
        }

        private static void CopyUserTextTag(XiphComment sourceTag, string sourceKey, Tag destinationTag, string destinationDescription)
        {
            var id = sourceTag.GetFirstField(sourceKey);
            if (!string.IsNullOrWhiteSpace(id))
            {
                var destinationFrame = UserTextInformationFrame.Get(destinationTag, destinationDescription, create: true);
                destinationFrame.Text = new[] { id };

                destinationTag.AddFrame(destinationFrame);
            }
        }

        private static void CopyUniqueFileId(XiphComment sourceTag, string sourceKey, Tag destinationTag, string owner)
        {
            var id = sourceTag.GetFirstField(sourceKey);
            if (!string.IsNullOrWhiteSpace(id))
            {
                var destinationFrame = UniqueFileIdentifierFrame.Get(destinationTag, owner, create: true);
                destinationFrame.Identifier = id;

                destinationTag.AddFrame(destinationFrame);
            }
        }

        private static void CopyMusicBrainzId(XiphComment sourceTag, string sourceKey, Tag destinationTag, string destinationDescription)
        {
            CopyUserTextTag(sourceTag, sourceKey, destinationTag, destinationDescription);
        }

        private static void CopyTrackNumberTag(XiphComment sourceTag, Tag destinationTag)
        {
            CopyTrackOrDiscNumberTag(sourceTag, "TRACKNUMBER", "TRACKTOTAL", "TOTALTRACKS", destinationTag, "TRCK");
        }

        private static void CopyDiscNumberTag(XiphComment sourceTag, Tag destinationTag)
        {
            CopyTrackOrDiscNumberTag(sourceTag, "DISCNUMBER", "DISCTOTAL", "TOTALDISCS", destinationTag, "TPOS");
        }

        /// <summary>
        /// Handle TRCK and TPOS tags, which are formatted as "n/N".
        /// </summary>
        /// <remarks>
        /// The source FLAC file has two standards for denoting the total count, so we look for both. The total is also optional.
        /// </remarks>
        private static void CopyTrackOrDiscNumberTag(XiphComment sourceTag, string numberKey, string totalKey, string totalKeyAlt, Tag destinationTag, string destinationKey)
        {
            var destinationFrame = TextInformationFrame.Get(destinationTag, destinationKey, create: true);
            var number = sourceTag.GetFirstField(numberKey);
            if (!string.IsNullOrWhiteSpace(number))
            {
                var total = sourceTag.GetFirstField(totalKey);
                if (string.IsNullOrWhiteSpace(total))
                    total = sourceTag.GetFirstField(totalKeyAlt);

                if (!string.IsNullOrWhiteSpace(total))
                    destinationFrame.Text = new[] { string.Format("{0}/{1}", number, total) };
                else
                    destinationFrame.Text = new[] { number };
            }

            destinationTag.AddFrame(destinationFrame);
        }

        private static void CopyCommentTags(XiphComment sourceTag, Tag destinationTag)
        {
            // MusicBrainz Picard uses COMMENT=... (type), so we need to find the last set of brackets on the line.
            var comments = sourceTag.GetField("COMMENT");
            foreach (var comment in comments)
            {
                // Find the last '('
                var pos = comment.LastIndexOf('(');
                if (pos == -1)
                    continue;
                
                // Find the ')' after that.
                var end = comment.IndexOf(')', pos);
                if (end == -1)
                    continue;

                // Get the parts.
                var description = comment.Substring(pos + 1, end - pos - 1);
                var text = comment.Substring(0, pos - 1);

                // Create a new frame.
                var commentsFrame = CommentsFrame.Get(destinationTag, description, "eng", create: true);
                commentsFrame.Text = text;
                destinationTag.AddFrame(commentsFrame);
            }
        }

        private static void CopyTag(XiphComment sourceTag, string sourceKey, Tag destinationTag, string destinationKey)
        {
            var sourceValue = sourceTag.GetField(sourceKey);
            if (sourceValue == null || sourceValue.Length == 0)
                return;

            var destinationFrame = TextInformationFrame.Get(destinationTag, destinationKey, create: true);
            destinationFrame.Text = sourceValue;
            destinationTag.AddFrame(destinationFrame);
        }
    }
}
