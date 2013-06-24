using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using NEmplode.Extensions;

namespace NEmplode.Empeg
{
    public class EmpegDatabaseReader
    {
        private readonly IEmpegDatabaseSource _source;

        public EmpegDatabaseReader(IEmpegDatabaseSource source)
        {
            _source = source;
        }

        public IEmpegDatabase ReadDatabase()
        {
            var encoding = Encoding.UTF8;

            string[] tags = ReadTags(encoding);

            // TODO: Consider using an EmpegDatabaseBuilder? In the sense of a mutable -> immutable builder, not in the sense of the rebuilding step.
            ConfigFile config;
            using (var configStream = _source.OpenConfig())
            using (var configReader = new StreamReader(configStream))
            {
                config = ConfigFile.Load(configReader);
            }

            EmpegDatabase database = new EmpegDatabase(config);

            using (var databaseStream = _source.OpenDatabase())
            using (var databaseReader = new BinaryReader(databaseStream))
            {
                int id = 0x0;

                while (!databaseReader.EndOfStream())
                {
                    var dictionary = ReadTagsForItem(tags, encoding, databaseReader);
                    if (dictionary.Count != 0)
                    {
                        string type = dictionary["type"];
                        if (type == "playlist")
                        {
                            // TODO: I don't like the fact that the database has to be passed here; it means that the database is being passed around, but it's not finished.
                            // TODO: On the other hand, I don't *think* I'm a fan of wrapping the entities returned...
                            var item = new EmpegPlaylist(database, id, dictionary);
                            database.Add(id, item);
                        }
                        else if (type == "tune")
                        {
                            var item = new EmpegTune(id, dictionary);
                            database.Add(id, item);
                        }
                    }

                    id += 0x10;
                }
            }

            var playlists = new Dictionary<int, int[]>();
            using (var playlistsStream = _source.OpenPlaylists())
            using (var playlistsReader = new BinaryReader(playlistsStream))
            {
                foreach (var item in database.Items.Values)
                {
                    if (item.IsPlaylist)
                    {
                        int length = item.Length;
                        int childCount = length / sizeof(Int32);
                        var children = new List<int>(childCount);
                        for (int i = 0; i < childCount; ++i)
                        {
                            int childId = playlistsReader.ReadInt32();
                            if (database.Items.ContainsKey(childId))
                                children.Add(childId);
                            else
                                Trace.TraceWarning("Playlist 0x{0:X}:{1} contains child 0x{2:X} which doesn't exist.", item.Id, item.Title, childId);
                        }

                        playlists[item.Id] = children.ToArray();
                    }
                }
            }

            database.SetPlaylists(playlists);

            return database;
        }

        private string[] ReadTags(Encoding encoding)
        {
            string[] tags;
            using (var tagsStream = _source.OpenTags())
            using (var tagsReader = new StreamReader(tagsStream, encoding))
            {
                tags = tagsReader.EnumerateAllLines()
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .ToArray();
            }
            return tags;
        }

        private static Dictionary<string, string> ReadTagsForItem(string[] tags, Encoding encoding, BinaryReader databaseReader)
        {
            var dictionary = new Dictionary<string, string>();
            for (;;)
            {
                var tagIndex = databaseReader.ReadByte();
                if (tagIndex == 0xFF)
                    break;

                var valueLength = databaseReader.ReadByte();
                var valueBytes = databaseReader.ReadBytes(valueLength);
                var value = encoding.GetString(valueBytes);

                var tagName = tags[tagIndex];

                dictionary.Add(tagName, value);
            }
            return dictionary;
        }
    }

    public class ConfigFile
    {
        private readonly List<ConfigFileSection> _sections;

        private ConfigFile(List<ConfigFileSection> sections)
        {
            _sections = sections;
        }

        public static ConfigFile Load(StreamReader configReader)
        {
            List<ConfigFileSection> sections = new List<ConfigFileSection>();
            
            ConfigFileSection section = null;

            string line;
            while ((line = configReader.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                if (line[0] == '[')
                {
                    if (section != null)
                        sections.Add(section);

                    var sectionName = line.Substring(1, line.Length - 2);
                    section = new ConfigFileSection(sectionName);
                }
                else
                {
                    var pos = line.IndexOf('=');
                    if (pos != -1)
                    {
                        var key = line.Substring(0, pos);
                        var value = line.Substring(pos + 1);

                        section.Add(key, value);
                    }
                }
            }

            return new ConfigFile(sections);
        }

        public ConfigFileSection this[string sectionName]
        {
            get { return _sections.Single(x=>x.Name == sectionName); }
        }
    }

    public class ConfigFileSection
    {
        private readonly IDictionary<string, string> _values = new Dictionary<string, string>();
        public string Name { get; set; }

        public ConfigFileSection(string name)
        {
            Name = name;
        }

        public void Add(string key, string value)
        {
            _values.Add(key, value);
       }

        public string this[string key]
        {
            get { return _values[key]; }
        }
    }
}