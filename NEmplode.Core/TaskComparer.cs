using System.Collections.Generic;
using System.Threading.Tasks;

namespace NEmplode
{
    internal class TaskComparer : IComparer<Task>
    {
        public int Compare(Task x, Task y)
        {
            return x.Id.CompareTo(y.Id);
        }
    }
}