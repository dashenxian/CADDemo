using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

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
            return minimumAreaBoundingRectangle.GetMinimumAreaBoundingRectangle(point2ds);
        }
    }
}