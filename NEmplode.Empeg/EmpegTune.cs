using System;
using System.Collections.Generic;
using NEmplode.Extensions;

namespace NEmplode.Empeg
{
    public class EmpegTune : EmpegItem
    {
        public EmpegTune(int id, Dictionary<string, string> dictionary)
            : base(id, dictionary)
        {
        }
        
        public string Source
        {
            get { return _dictionary.GetValueOrDefault("source"); }
        }

        public int TrackNumber
        {
            get
            {
                string str = _dictionary.GetValueOrDefault("tracknr");
                if (string.IsNullOrWhiteSpace(str))
                    return 0;

                var pos = str.IndexOf('/');
                if (pos == -1)
                    return Convert.ToInt32(str);
                return Convert.ToInt32(str.Substring(0, pos));
            }
        }

        public string Artist
        {
            get { return _dictionary.GetValueOrDefault("artist"); }
        }

        public override string ToString()
        {
            return string.Format("{0:X}: {1} - {2} - {3} - {4}", Id, Source, TrackNumber, Artist, Title);
        }
    }
}