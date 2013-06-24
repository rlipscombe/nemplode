using System.Collections.Generic;
using System.Linq;
using NEmplode.Extensions;

namespace NEmplode.Empeg
{
    internal class EmpegDatabase : IEmpegDatabase
    {
        private readonly IDictionary<int, EmpegItem> _items = new Dictionary<int, EmpegItem>();
        private IDictionary<int, int[]> _playlists = new Dictionary<int, int[]>();
        private readonly ConfigFile _config;

        public EmpegDatabase(ConfigFile config)
        {
            _config = config;
        }

        internal IDictionary<int, EmpegItem> Items
        {
            get { return _items; }
        }

        public IEmpegPlaylist GetRootPlaylist()
        {
            return (IEmpegPlaylist) _items[0x100];
        }

        public IEmpegItem GetItem(string absolutePath)
        {
            // TODO: This is hideous.
            IEmpegItem current = GetRootPlaylist();
            string[] components = absolutePath.Split('\\');
            foreach (var component in components)
            {
                var playlist = (IEmpegPlaylist)current;
                var child = playlist.GetChildren().SingleOrDefault(x => x.Title == component);
                if (child == null)
                    break;
                current = child;
            }

            return current;
        }

        public string Name
        {
            get { return _config["Options"]["Name"]; }
        }

        public IEnumerable<IEmpegItem> GetAllItems()
        {
            return _items.Values;
        }

        public void Add(int id, EmpegItem item)
        {
            _items.Add(id, item);
        }

        public IEnumerable<IEmpegItem> GetChildren(int parentId)
        {
            return _playlists[parentId].Select(GetItemById);
        }

        /// <returns>The item, or null if not found.</returns>
        private IEmpegItem GetItemById(int id)
        {
            return _items.GetValueOrDefault(id);
        }

        public void SetPlaylists(Dictionary<int, int[]> playlists)
        {
            _playlists = playlists;
        }
    }
}