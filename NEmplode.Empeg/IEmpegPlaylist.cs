using System.Collections.Generic;

namespace NEmplode.Empeg
{
    public interface IEmpegPlaylist : IEmpegItem
    {
        IEnumerable<IEmpegItem> GetChildren();
    }
}