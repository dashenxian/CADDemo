using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcDotNetTool.Extensions;

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
    public class MinimumAreaBoundingRectangle : IMinimumAreaBoundingRectangle
    {
        public bool ShowProcessMBR { get; set; } = false;
        /// <summary>
        /// 获取最小面积外接矩形
        /// </summary>
        /// <param name="point2ds"></param>
        /// <returns></returns>
        public virtual Polyline GetMinimumAreaBoundingRectangle(IEnumerable<Point2d> point2ds)
        {
            var convexHull = GetConvexHull(point2ds).ToList();
            //外接矩形和角度
            var mabr = new MBR();
            for (int i = 0; i < convexHull.Count; i++)
            {
                var current = convexHull[i];
                var next = i == convexHull.Count() - 1 ? convexHull[0] : convexHull[i + 1];
                var v = next - current;
                var mt = Matrix2d.Rotation(-v.Angle, current);
                var points = convexHull.Select(p => p.TransformBy(mt)).ToList();
                var bound = new Extents3d();
                double minX = points[0].X, minY = points[0].Y, maxX = points[0].X, maxY = points[0].Y;

                foreach (var item in points)
                {
                    if (item.X < minX)
                    {
                        minX = item.X;
                    }
                    if (item.Y < minY)
                    {
                        minY = item.Y;
                    }
                    if (item.X > maxX)
                    {
                        maxX = item.X;
                    }
                    if (item.Y > maxY)
                    {
                        maxY = item.Y;
                    }
                }
                var currMabr = new MBR { Angle = -v.Angle, BasePoint = current, MinPoint = new Point2d(minX, minY), MaxPoint = new Point2d(maxX, maxY) };
                if (ShowProcessMBR)
                {
                    var plt = CreateBoundsLine(currMabr, false);
                    plt.Color = Colors.Color.FromColorIndex(Colors.ColorMethod.ByLayer, 1);
                    DataBaseTools.AddIn(plt);
                }

                if (i == 0 || mabr.Area > currMabr.Area)
                {
                    mabr = currMabr;
                }
            }
            var pl = CreateBoundsLine(mabr);
            return pl;
        }

        /// <summary>
        /// 获取图形凸包
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public virtual IEnumerable<Point2d> GetConvexHull(IEnumerable<Point2d> points)
        {
            if (points == null)
            {
                throw new ArgumentNullException("外边线点不能少于3个");
            }
            points = points.Distinct().ToList();
            if (points.Count() < 3)
            {
                throw new ArgumentException("外边线点不能少于3个");
            }
            //求y坐标最小的点，y坐标相同时x坐标最小的点
            var p0 = points.First();
            foreach (var item in points)
            {
                if (p0.Y > item.Y || p0.Y == item.Y && p0.X > item.X)
                {
                    p0 = item;
                }
            }
            //按角度排序
            var otherPoinsts = points.Where(i => i != p0)
                .OrderBy(i =>
                {
                    return new Vector2d(i.X - p0.X, i.Y - p0.Y);
                }, new Extensions.Comparer<Vector2d>((t1, t2) =>
                {
                    if (t1.Angle > t2.Angle || Math.Abs(t1.Angle - t2.Angle) < BaseTools.Tolerance.EqualPoint && t1.Length > t2.Length) { return 1; }
                    else if (t1.Angle < t2.Angle) { return -1; }
                    else { return 0; }
                }))
                .ToList();
            var stack = new List<Point2d>(points.Count())
            {
                p0,
                otherPoinsts[0]
            };
            for (int i = 1; i < otherPoinsts.Count; i++)
            {
                while (stack.Count >= 2)
                {
                    var k = stack.Count;
                    var v1 = otherPoinsts[i] - stack[k - 1];
                    var v2 = stack[k - 1] - stack[k - 2];
                    if (v1.Cross(v2) > 0)
                    {
                        stack.RemoveAt(k - 1);
                    }
                    else
                    {
                        break;
                    }
                }
                stack.Add(otherPoinsts[i]);
            }
            return stack;
        }
        /// <summary>
        /// 根据边界创建外接矩形
        /// </summary>
        /// <param name="mbr"></param>
        /// <param name="isRotation">是否旋转</param>
        /// <returns></returns>
        private Polyline CreateBoundsLine(MBR mbr, bool isRotation = true)
        {
            var pl = new Polyline();
            pl.AddVertexAt(0, new Point2d(mbr.MinPoint.X, mbr.MinPoint.Y), 0, 0, 0);
            pl.AddVertexAt(1, new Point2d(mbr.MaxPoint.X, mbr.MinPoint.Y), 0, 0, 0);
            pl.AddVertexAt(2, new Point2d(mbr.MaxPoint.X, mbr.MaxPoint.Y), 0, 0, 0);
            pl.AddVertexAt(3, new Point2d(mbr.MinPoint.X, mbr.MaxPoint.Y), 0, 0, 0);
            pl.Closed = true;
            if (isRotation)
            {
                var mtP = Matrix3d.Rotation(-mbr.Angle, Vector3d.ZAxis, mbr.BasePoint.ToPoint3d());
                pl.TransformBy(mtP);
            }

            return pl;
        }
        /// <summary>
        /// 保存计算最小面积外接矩形过程中的外接矩形数据
        /// </summary>
        private struct MBR
        {
            /// <summary>
            /// 旋转基点
            /// </summary>
            public Point2d BasePoint;
            /// <summary>
            /// 旋转角度
            /// </summary>
            public double Angle;
            /// <summary>
            /// 范围最小点
            /// </summary>
            public Point2d MinPoint;
            /// <summary>
            /// 范围最大点
            /// </summary>
            public Point2d MaxPoint;
            /// <summary>
            /// 面积
            /// </summary>
            public double Area { get => (MaxPoint.X - MinPoint.X) * (MaxPoint.Y - MinPoint.Y); }
        }
    }

}
