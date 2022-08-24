using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcDotNetTool.Extensions
{
    public static class NumberExtensions
    {
        /// <summary>
        /// 比较两个double数字在容差范围内相等
        /// </summary>
        /// <param name="value1"></param>
        /// <param name="value2"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public static bool Equals(this double value1, double value2, double tolerance)
        {
            return Math.Abs(value1 - value2) < tolerance;
        }
    }
}
