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
    public static class LineExtensions
    {
        /// <summary>
        /// 计算叉乘
        /// </summary>
        /// <param name="line1"></param>
        /// <param name="line2"></param>
        /// <returns></returns>
        public static Vector3d Cross(this Line line1, Line line2)
        {
            var a = line1.ToVector3D();
            var b = line2.ToVector3D();
            return a.CrossProduct(b);
        }
        /// <summary>
        /// 转为向量
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public static Vector3d ToVector3D(this Line line)
        {
            return line.EndPoint - line.StartPoint;
        }
        /// <summary>
        /// 计算叉乘
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static double Cross(this Vector2d a, Vector2d b)
        {
            return a.X * b.Y - a.Y * b.X;
        }
    }
}
