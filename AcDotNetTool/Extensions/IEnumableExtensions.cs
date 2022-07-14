using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;

namespace AcDotNetTool.Extensions
{
    public static class IEnumableExtensions
    {
        /// <summary>
        /// 把列表转为DBObjectCollection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static DBObjectCollection ToDBObjectCollection<T>(this IEnumerable<T> list) where T : DBObject
        {
            var dBObjectCollection = new DBObjectCollection();
            foreach (var item in list)
            {
                dBObjectCollection.Add(item);
            }
            return dBObjectCollection;
        }
    }
}
