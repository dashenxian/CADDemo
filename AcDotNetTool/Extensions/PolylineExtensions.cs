using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if ZWCAD
using ZwSoft.ZwCAD.DatabaseServices;
using ZwSoft.ZwCAD.EditorInput;
using ZwSoft.ZwCAD.Geometry;
#elif AutoCAD
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
#endif

namespace AcDotNetTool.Extensions
{
    /// <summary>
    /// Enumeration of offset side options
    /// </summary>
    public enum OffsetSide
    {
        /// <summary>
        /// Inside.
        /// </summary>
        In,
        /// <summary>
        /// Outside.
        /// </summary>
        Out,
        /// <summary>
        /// Left side.
        /// </summary>
        Left,
        /// <summary>
        /// Right side.
        /// </summary>
        Right,
        /// <summary>
        /// Both sides.
        /// </summary>
        Both
    }
    public static class PolylineExtensions
    {
        /// <summary>
        /// 添加或设置顶点，顶点不存在时添加，存在时设置
        /// </summary>
        /// <param name="polyline">多段线</param>
        /// <param name="index">序号</param>
        /// <param name="pt">坐标</param>
        /// <param name="bulge">角度</param>
        /// <param name="startWidth">起始宽度</param>
        /// <param name="endWidth">结束宽度</param>
        /// <returns></returns>
        public static Polyline AddOrSetVertexAt(this Polyline polyline,
            int index, Point2d pt, double bulge,
            double startWidth, double endWidth)
        {
            return polyline.TransactionExcute(ent =>
            {
                if (ent.NumberOfVertices > index)
                {
                    ent.SetPointAt(index, pt);
                    ent.SetBulgeAt(index, bulge);
                    ent.SetStartWidthAt(index, startWidth);
                    ent.SetEndWidthAt(index, endWidth);
                }
                else
                {
                    ent.AddVertexAt(index, pt, bulge, startWidth, endWidth);
                }
            });
        }
        /// <summary>
        /// 移除多段线指定顶点
        /// </summary>
        /// <param name="polyline">多段线</param>
        /// <param name="index">序号</param>
        /// <returns></returns>
        public static Polyline TryRemoveVertexAt(this Polyline polyline,
            int index)
        {
            return polyline.TransactionExcute(pl =>
            {
                if (pl.NumberOfVertices > 0)
                {
                    pl.RemoveVertexAt(index);
                }
            });
        }

        public static Polyline TryClose(this Polyline polyline)
        {
            return polyline.TransactionExcute(pl => pl.Closed = true);
        }

        /// <summary>
        /// Offset the source polyline to specified side(s).
        /// </summary>
        /// <param name="source">Instance to which the method applies.</param>
        /// <param name="offsetDist">Offset distance.</param>
        /// <param name="side">Offset side(s).</param>
        /// <returns>A polyline sequence resulting from the offset of the source polyline.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name ="source"/> is null.</exception>
        public static IEnumerable<Polyline> Offset(this Polyline source, double offsetDist, OffsetSide side)
        {
            Assert.IsNotNull(source, nameof(source));

            offsetDist = Math.Abs(offsetDist);
            using (var plines = new DisposableSet<Polyline>())
            {
                IEnumerable<Polyline> offsetRight = source.GetOffsetCurves(offsetDist).Cast<Polyline>();
                plines.AddRange(offsetRight);
                IEnumerable<Polyline> offsetLeft = source.GetOffsetCurves(-offsetDist).Cast<Polyline>();
                plines.AddRange(offsetLeft);
                double areaRight = offsetRight.Select(pline => pline.Area).Sum();
                double areaLeft = offsetLeft.Select(pline => pline.Area).Sum();
                switch (side)
                {
                    case OffsetSide.In:
                        return plines.RemoveRange(
                           areaRight < areaLeft ? offsetRight : offsetLeft);
                    case OffsetSide.Out:
                        return plines.RemoveRange(
                           areaRight < areaLeft ? offsetLeft : offsetRight);
                    case OffsetSide.Left:
                        return plines.RemoveRange(offsetLeft);
                    case OffsetSide.Right:
                        return plines.RemoveRange(offsetRight);
                    case OffsetSide.Both:
                        plines.Clear();
                        return offsetRight.Concat(offsetLeft);
                    default:
                        return null;
                }
            }
        }
    }
}
