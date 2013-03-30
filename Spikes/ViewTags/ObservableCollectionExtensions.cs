using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ViewTags
{
    internal static class ObservableCollectionExtensions
    {
        public static void AddRange<T>(this ObservableCollection<T> collection, IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                collection.Add(item);
            }
        }

        public static void Merge<T>(this ObservableCollection<T> collection, IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                if (!collection.Contains(item))
                    collection.Add(item);
            }
        }
    }
}