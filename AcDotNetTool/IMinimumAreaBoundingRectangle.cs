using System.Collections.Generic;
using System.Linq;


#if ZWCAD
using ZwSoft.ZwCAD.ApplicationServices;
using ZwSoft.ZwCAD.DatabaseServices;
using ZwSoft.ZwCAD.EditorInput;
using ZwSoft.ZwCAD.Geometry;
using ZwSoft.ZwCAD.Runtime;
using Colors = ZwSoft.ZwCAD.Colors;
#elif AutoCAD
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Colors = Autodesk.AutoCAD.Colors;
#endif

namespace AcDotNetTool
{
    /// <summary>
    /// 最小面积外接矩形算法接口
    /// </summary>
    public interface IMinimumAreaBoundingRectangle
    {
        /// <summary>
        /// 绘制计算过程中产生的外框到界面中
        /// </summary>
        bool ShowProcessMBR { get; set; }
        /// <summary>
        /// 获取图形凸包
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        IEnumerable<Point2d> GetConvexHull(IEnumerable<Point2d> points);
        /// <summary>
        /// 获取最小面积外接矩形
        /// </summary>
        /// <param name="point2ds"></param>
        /// <returns></returns>
        Polyline GetMinimumAreaBoundingRectangle(IEnumerable<Point2d> point2ds);
    }
    public static class MinimumAreaBoundingRectangleExtension
    {
        /// <summary>
        /// 获取最小外接矩形
        /// </summary>
        /// <param name="minimumAreaBoundingRectangle"></param>
        /// <param name="curve"></param>
        /// <returns></returns>
        public static Polyline GetMinimumAreaBoundingRectangle(this IMinimumAreaBoundingRectangle minimumAreaBoundingRectangle, Curve curve)
        {
            var point2ds = new List<Point2d>();
            var points = new Point3dCollection();
            curve.GetStretchPoints(points);
            foreach (Point3d item in points)
            {
                point2ds.Add(item.ToPoint2d());
            }
            var explodeDBObjectCollection = new DBObjectCollection();
            curve.Explode(explodeDBObjectCollection);
            var arcs = explodeDBObjectCollection.ToList<DBObject>().OfType<Arc>();
            foreach (var arc in arcs)
            {
                point2ds.AddRange(GetArcMbrPoint(arc).ToPoint2d());
            }
            return minimumAreaBoundingRectangle.GetMinimumAreaBoundingRectangle(point2ds);
        }

        private static IEnumerable<Point3d> GetArcMbrPoint(Arc arc)
        {
            var points = new List<Point3d>();
            var pointLeft = new Point3d(arc.Center.X - arc.Radius, arc.Center.Y, 0);
            if (arc.IsOnCurve(pointLeft))
            {
                points.Add(pointLeft);
            }

            var pointRight = new Point3d(arc.Center.X + arc.Radius, arc.Center.Y, 0);
            if (arc.IsOnCurve(pointRight))
            {
                points.Add(pointRight);
            }

            var pointTop = new Point3d(arc.Center.X, arc.Center.Y + arc.Radius, 0);
            if (arc.IsOnCurve(pointTop))
            {
                points.Add(pointTop);
            }

            var pointBottom = new Point3d(arc.Center.X, arc.Center.Y - arc.Radius, 0);
            if (arc.IsOnCurve(pointBottom))
            {
                points.Add(pointBottom);
            }

            points.Add(arc.StartPoint);
            points.Add(arc.EndPoint);
            return points;
        }

        public static IMinimumAreaBoundingRectangle GetIMinimumAreaBoundingRectangle()
        {
            return new MinimumAreaBoundingRectangle();
        }
    }
}