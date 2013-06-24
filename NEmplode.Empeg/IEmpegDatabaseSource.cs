using System.IO;

namespace NEmplode.Empeg
{
    public interface IEmpegDatabaseSource
    {
        Stream OpenConfig();
        Stream OpenTags();
        Stream OpenDatabase();
        Stream OpenPlaylists();
    }
}