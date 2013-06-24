using System.Collections.Generic;

namespace NEmplode.Empeg
{
    public class EmpegPlaylist : EmpegItem, IEmpegPlaylist
    {
        private readonly EmpegDatabase _database;

        internal EmpegPlaylist(EmpegDatabase database, int id, Dictionary<string, string> dictionary)
            : base(id, dictionary)
        {
            _database = database;
        }

        public IEnumerable<IEmpegItem> GetChildren()
        {
            return _database.GetChildren(_id);
        }

        public override string ToString()
        {
            return string.Format("{0:X}: {1}", Id, Title);
        }
    }
}