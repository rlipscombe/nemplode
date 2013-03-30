using System;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using TagLib;
using TagLib.Id3v2;

namespace NEmplode.MSBuild.Tasks
{
    public class CopyMediaTags : Task
    {
        [Required]
        public string SourceFile { get; set; }
        public string SourceFileMimeType { get; set; }

        [Required]
        public string DestinationFile { get; set; }
        public string DestinationFileMimeType { get; set; }

        public override bool Execute()
        {
            using (var source = File.Create(SourceFile, SourceFileMimeType, ReadStyle.Average))
            using (var destination = File.Create(DestinationFile, DestinationFileMimeType, ReadStyle.Average))
            {
                var sourceTag = source.Tag;
                var destinationTag = (TagLib.Id3v2.Tag)destination.GetTag(TagTypes.Id3v2, create: true);

                sourceTag.CopyTo(destinationTag, overwrite: true);

                // Create the WM/AlbumArtist tag -- this ought to prevent the album coming apart.
                var albumArtistFrame = PrivateFrame.Get(destinationTag, "WM/AlbumArtist", create: true);
                string firstAlbumArtist = sourceTag.FirstAlbumArtist;
                if (!string.IsNullOrWhiteSpace(firstAlbumArtist))
                    albumArtistFrame.PrivateData = new ByteVector(Encoding.Unicode.GetBytes(firstAlbumArtist));

                // Copy the MusicBrainz tags; since ID3v2 doesn't support them:
                // WM/CollectionGroupID <- MusicBrainzReleaseId
                // WM/CollectionID <- MusicBrainzReleaseId
                // WM/ContentID <- MusicBrainzTrackId
                Guid releaseId = Guid.NewGuid();
                if (!string.IsNullOrWhiteSpace(sourceTag.MusicBrainzReleaseId))
                    releaseId = Guid.Parse(sourceTag.MusicBrainzReleaseId);
                Guid trackId = Guid.NewGuid();
                if (!string.IsNullOrWhiteSpace(sourceTag.MusicBrainzDiscId))
                    trackId = Guid.Parse(sourceTag.MusicBrainzTrackId);

                var collectionGroupIdFrame = PrivateFrame.Get(destinationTag, "WM/CollectionGroupID", create: true);
                collectionGroupIdFrame.PrivateData = new ByteVector(releaseId.ToByteArray());

                var collectionIdFrame = PrivateFrame.Get(destinationTag, "WM/CollectionID", create: true);
                collectionIdFrame.PrivateData = new ByteVector(releaseId.ToByteArray());

                var contentIdFrame = PrivateFrame.Get(destinationTag, "WM/ContentID", create: true);
                contentIdFrame.PrivateData = new ByteVector(trackId.ToByteArray());

                destination.Save();

                return true;
            }
        }
    }
}
