using System;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using TagLib;
using TagLib.Id3v2;
using TagLibFile = TagLib.File;

namespace MSBuild.Media.Tasks
{
    public class CopyMediaTags : Task
    {
        [Required]
        public string SourceFile { get; set; }

        [Required]
        public string DestinationFile { get; set; }

        public override bool Execute()
        {
            using (var source = TagLibFile.Create(SourceFile))
            using (var destination = TagLibFile.Create(DestinationFile))
            {
                var sourceTag = source.Tag;
                var destinationTag = (TagLib.Id3v2.Tag)destination.GetTag(TagTypes.Id3v2, create: true);

                sourceTag.CopyTo(destinationTag, overwrite: true);

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

                var collectionGroupIdTag = PrivateFrame.Get(destinationTag, "WM/CollectionGroupID", create: true);
                collectionGroupIdTag.PrivateData = new ByteVector(releaseId.ToByteArray());

                var collectionIdTag = PrivateFrame.Get(destinationTag, "WM/CollectionID", create: true);
                collectionIdTag.PrivateData = new ByteVector(releaseId.ToByteArray());

                var contentIdTag = PrivateFrame.Get(destinationTag, "WM/ContentID", create: true);
                contentIdTag.PrivateData = new ByteVector(trackId.ToByteArray());

                destination.Save();

                return true;
            }
        }
    }
}
