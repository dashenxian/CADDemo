using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        /// <summary>
        /// 计算方差
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static double Variance(this IEnumerable<double> values)
        {
            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }
            var avg = values.Average();
            return values.Sum(x => Math.Pow(x - avg, 2)) / values.Count();
        }
        /// <summary>
        /// 交换两个数
        /// </summary>
        /// <param name="value1"></param>
        /// <param name="value2"></param>
        public static void SwapNumber(ref double value1, ref double value2)
        {
            (value1, value2) = (value2, value1);
        }
    }
}
