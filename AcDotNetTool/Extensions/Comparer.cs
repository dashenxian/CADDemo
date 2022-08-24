using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcDotNetTool.Extensions
{
    public class Comparer<T> : IComparer<T>
    {
        public Comparer(Func<T, T, int> compareFunc)
        {
            CompareFunc = compareFunc;
        }

        public Func<T, T, int> CompareFunc { get; }

        public int Compare(T x, T y)
        {
            return CompareFunc(x, y);
        }
    }
}
