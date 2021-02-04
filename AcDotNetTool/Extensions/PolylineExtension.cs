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
    public static class PolylineExtension
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
    }
}
