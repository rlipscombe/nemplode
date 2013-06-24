using System;
using System.IO;
using System.Net;

namespace NEmplode.Empeg
{
    public class HijackEmpegDatabaseSource : IEmpegDatabaseSource
    {
        private readonly Uri _baseUri;

        public HijackEmpegDatabaseSource(IPAddress address)
        {
            _baseUri = new Uri(string.Format("http://{0}", address));
        }

        public Stream OpenConfig()
        {
            return OpenFile("/empeg/var/config.ini");
        }

        public Stream OpenTags()
        {
            return OpenFile("/empeg/var/tags");
        }

        public Stream OpenDatabase()
        {
            return OpenFile("/empeg/var/database3");
        }

        public Stream OpenPlaylists()
        {
            return OpenFile("/empeg/var/playlists");
        }

        private Stream OpenFile(string relativeUri)
        {
            var uri = new Uri(_baseUri, relativeUri);

            var client = new WebClient();
            byte[] bytes = client.DownloadData(uri);
            return new MemoryStream(bytes);
        }
    }
}