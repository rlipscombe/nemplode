using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using NEmplode.Codecs;

namespace NEmplode.MSBuild.Tasks
{
    public sealed class ConvertMusic : Task
    {
        [Required]
        public string SourceFileName { get; set; }

        [Required]
        public string DestinationFileName { get; set; }

        public override bool Execute()
        {
            using (var source = DecodingStream.OpenRead(SourceFileName))
            using (var destination = EncodingStream.OpenWrite(DestinationFileName))
                source.CopyTo(destination);

            return true;
        }

    }
}