using System;
using System.Collections.Generic;
using NEmplode.Extensions;

namespace NEmplode.Empeg
{
    public abstract class EmpegItem : IEmpegItem
    {
        protected readonly int _id;
        protected readonly Dictionary<string, string> _dictionary;

        public EmpegItem(int id, Dictionary<string, string> dictionary)
        {
            _dictionary = dictionary;
            _id = id;
        }

        public int Id
        {
            get { return _id; }
        }

        public string Title
        {
            get { return _dictionary.GetValueOrDefault("title"); }
        }

        public bool IsPlaylist
        {
            get { return Type == "playlist"; }
        }

        protected string Type
        {
            get { return _dictionary.GetValueOrDefault("type"); }
        }

        public int Length
        {
            get { return Convert.ToInt32(_dictionary.GetValueOrDefault("length")); }
        }
    }
}