using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#if ZWCAD
using ZwSoft.ZwCAD.ApplicationServices;
using ZwSoft.ZwCAD.DatabaseServices;
using ZwSoft.ZwCAD.EditorInput;
using ZwSoft.ZwCAD.Geometry;
using ZwSoft.ZwCAD.Runtime;
#elif AutoCAD
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
#endif

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
        /// <summary>
        /// ToObjectIdCollection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static ObjectIdCollection ToObjectIdCollection<T>(this IEnumerable<T> list) where T : DBObject
        {
            var ids = new ObjectIdCollection();
            foreach (var item in list)
            {
                ids.Add(item.ObjectId);
            }
            return ids;
        }
        /// <summary>
        /// ToPoint3dCollection
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static Point3dCollection ToPoint3dCollection(this IEnumerable<Point3d> list)
        {
            var ids = new Point3dCollection();
            foreach (var item in list)
            {
                ids.Add(item);
            }
            return ids;
        }
        /// <summary>
        /// ToPoint2dCollection
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static Point2dCollection ToPoint2dCollection(this IEnumerable<Point2d> list)
        {
            var ids = new Point2dCollection();
            foreach (var item in list)
            {
                ids.Add(item);
            }
            return ids;
        }

    }
}
