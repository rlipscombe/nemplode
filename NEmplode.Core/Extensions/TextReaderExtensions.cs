using System.Collections.Generic;
using System.IO;

namespace NEmplode.Extensions
{
    public static class TextReaderExtensions
    {
        public static IEnumerable<string> EnumerateAllLines(this TextReader reader)
        {
            string line;
            while ((line = reader.ReadLine()) != null)
                yield return line;
        }
    }
}