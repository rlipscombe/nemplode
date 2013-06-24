using System.Collections.Generic;

namespace NEmplode.Empeg
{
    public interface IEmpegDatabase
    {
        IEnumerable<IEmpegItem> GetAllItems();
        IEmpegPlaylist GetRootPlaylist();
        IEmpegItem GetItem(string absolutePath);
        string Name { get; }
    }
}