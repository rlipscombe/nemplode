using System.IO;

namespace NEmplode.Empeg
{
    public class LocalEmpegDatabaseSource : IEmpegDatabaseSource
    {
        private readonly string _path;

        public LocalEmpegDatabaseSource(string path)
        {
            _path = path;
        }

        public Stream OpenConfig()
        {
            return File.OpenRead(Path.Combine(_path, "config.ini"));
        }

        public Stream OpenTags()
        {
            return File.OpenRead(Path.Combine(_path, "tags"));
        }

        public Stream OpenDatabase()
        {
            return File.OpenRead(Path.Combine(_path, "database3"));
        }

        public Stream OpenPlaylists()
        {
            return File.OpenRead(Path.Combine(_path, "playlists"));
        }
    }
}