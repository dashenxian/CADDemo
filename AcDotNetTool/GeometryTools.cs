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

namespace AcDotNetTool
{
    /// <summary>
    /// 几何相关处理
    /// </summary>
    public static class GeometryTools
    {

        #region 夹角

        /// <summary>
        /// 判断向量与x轴的夹角，x轴上方为正，下方为负
        /// </summary>
        /// <param name="startPoint">起点</param>
        /// <param name="endPoint">终点</param>
        /// <returns></returns>
        public static double GetAngleToXAxis(this Point3d startPoint, Point3d endPoint)
        {
            // 声明一个与X轴平行的向量
            Vector3d temp = new Vector3d(1, 0, 0);
            // 获取起点到终点的向量
            Vector3d VsToe = startPoint.GetVectorTo(endPoint);
            return VsToe.Y > 0 ? temp.GetAngleTo(VsToe) : -temp.GetAngleTo(VsToe);
        }

        ///// <summary>
        ///// 两点组成的线段与X轴之间夹角，逆时针方向
        ///// </summary>
        ///// <param name="basePt">起点</param>
        ///// <param name="endPt">终点</param>
        ///// <returns>角度</returns>
        //public static double Angle(Point3d startPt, Point3d endPt)
        //{
        //    Line L = new Line(startPt, endPt);
        //    return L.Angle;
        //}

        ///// <summary>
        ///// 两直线间夹角，逆时针方向
        ///// </summary>
        ///// <param name="line1">线1</param>
        ///// <param name="line2">线2</param>
        ///// <returns></returns>
        //public static double Angle(Line line1, Line line2)
        //{
        //    return Math.Abs(line1.Angle - line2.Angle);
        //}

        #endregion

        #region 距离

        /// <summary>
        /// 获取两点距离
        /// </summary>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <returns></returns>
        public static double Distance(this Point3d point1, Point3d point2)
        {
            return (Math.Sqrt(Math.Pow((point1.X - point2.X), 2) + Math.Pow((point1.Y - point2.Y), 2) + Math.Pow((point1.Z + point2.Z), 2)));
        }
        /// <summary>
        /// 点到曲线距离
        /// </summary>
        /// <param name="point">指定点</param>
        /// <param name="cur">指定线</param>
        /// <param name="bo">是否延伸</param>
        /// <returns>距离</returns>
        public static double Distance(this Point3d point, Curve cur, bool bo)
        {
            return cur.GetClosestPointTo(point, bo).DistanceTo(point);
        }
        /// <summary>
        /// 直线距离
        /// </summary>
        /// <param name="line1">直线1</param>
        /// <param name="line2">直线2</param>
        /// <returns>两线间距离</returns>
        public static double Distance(Line line1, Line line2)
        {
            if (Parallel(line1, line2))
            {
                return line1.GetClosestPointTo(line2.StartPoint, true).DistanceTo(line2.StartPoint);
            }
            else
            {
                return 0;
            }
        }
        /// <summary>
        /// 获取与给定点指定角度和距离的点
        /// </summary>
        /// <param name="basePt">基点</param>
        /// <param name="angle">角度</param>
        /// <param name="distance">距离</param>
        /// <returns>相对点</returns>
        public static Point3d RelativePoint(Point3d basePt, double angle, double distance)
        {
            double[] pt = new double[3];
            angle = angle * Math.PI / 180;
            pt[0] = basePt[0] + distance * Math.Cos(angle);
            pt[1] = basePt[1] + distance * Math.Sin(angle);
            pt[2] = basePt[2];
            Point3d point = new Point3d(pt[0], pt[1], pt[2]);
            return point;
        }
        /// <summary>
        /// 获取与给定点相对距离的点
        /// </summary>
        /// <param name="basePt">起点</param>
        /// <param name="x">相对X距离</param>
        /// <param name="y">相对Y距离</param>
        /// <returns>点</returns>
        public static Point3d RelativePoint(Point3d basePt, double x, double y, double z)
        {
            return new Point3d(basePt.X + x, basePt.Y + y, basePt.Z + z);
        }
        #endregion

        #region 中点
        /// <summary>
        /// 计算两点中点
        /// </summary>
        /// <param name="point1">第一个点</param>
        /// <param name="point2">第二个点</param>
        /// <returns></returns>
        public static Point3d MidPoint(this Point3d point1, Point3d point2)
        {
            return new Point3d((point1.X + point2.X) / 2, (point1.Y + point2.Y) / 2, (point1.Z + point2.Z) / 2);
        }
        /// <summary>
        /// 计算线段中点
        /// </summary>
        /// <param name="line">指定线段</param>
        /// <returns>中点</returns>
        public static Point3d MidPoint(Line line)
        {
            return MidPoint(line.StartPoint, line.EndPoint);
        }
        #endregion

        #region 重合
        /// <summary>
        /// 判断点是否重合
        /// </summary>
        /// <param name="point1">点1</param>
        /// <param name="point2">点2</param>
        /// <param name="Allowance">容差</param>
        /// <returns>是否重合</returns>
        public static bool Coincide(Point3d point1, Point3d point2, double allowance)
        {
            if (point1.DistanceTo(point2) < allowance)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 判断线段是否重合
        /// </summary>
        /// <param name="line1">线段1</param>
        /// <param name="line2">线段2</param>
        /// <param name="Allowance">容差</param>
        /// <returns>线段是否重合</returns>
        public static bool Coincide(Line line1, Line line2, double allowance)
        {
            LineSegment3d l1 = new LineSegment3d(line1.StartPoint, line1.EndPoint);
            LineSegment3d l2 = new LineSegment3d(line2.StartPoint, line2.EndPoint);
            Tolerance tol = new Tolerance(allowance, allowance);
            return l1.IsColinearTo(l2, tol);
        }
        /// <summary>
        /// 获取两条线段重合部分
        /// </summary>
        /// <param name="line1">线段1</param>
        /// <param name="line2">线段2</param>
        /// <returns>重合部分</returns>
        public static Line Coincide(Line line1, Line line2)
        {
            LineSegment3d l1 = new LineSegment3d(line1.StartPoint, line1.EndPoint);
            LineSegment3d l2 = new LineSegment3d(line2.StartPoint, line2.EndPoint);
            LinearEntity3d l3 = l1.Overlap(l2);
            return new Line(l3.StartPoint, l3.EndPoint);
        }
        #endregion

        #region 相交
        /// <summary>
        /// 给定延伸方式求曲线交点
        /// </summary>
        /// <param name="C1">曲线1</param>
        /// <param name="C2">曲线2</param>
        /// <param name="inter">延伸方式</param>
        /// <returns>交点集合</returns>
        public static Point3dCollection Intersect(Curve cur1, Curve cur2, Intersect inter)
        {
            Point3dCollection ptc = new Point3dCollection();
            cur1.IntersectWith(cur2, inter, ptc, 0, 0);
            return ptc;
        }
        /// <summary>
        /// 判断点是否在曲线上
        /// </summary>/// <param name="pt">指定点</param>
        /// <param name="cur">指定线</param>
        /// <param name="Allowance">容差</param>
        /// <returns>点是否在直线上</returns>
        public static bool Inside(Point3d pt, Curve cur, double allowance)
        {
            if (Distance(pt, cur, true) < allowance)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 判断三个点是否在同一直线上
        /// </summary>
        /// <param name="firstPoint">第一个点</param>
        /// <param name="secondPoint">第二个点</param>
        /// <param name="thirdPoint">第三个点</param>
        /// <returns></returns>
        public static bool IsOnOneLine(this Point3d firstPoint, Point3d secondPoint, Point3d thirdPoint)
        {
            Vector3d v21 = secondPoint.GetVectorTo(firstPoint);
            Vector3d v23 = secondPoint.GetVectorTo(thirdPoint);
            if (v21.GetAngleTo(v23) == 0 || v21.GetAngleTo(v23) == Math.PI)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region 平行
        /// <summary>
        /// 判断直线是否平行
        /// </summary>
        /// <param name="line1">直线1</param>
        /// <param name="line2">直线2</param>
        /// <returns>是否平行</returns>
        public static bool Parallel(Line line1, Line line2)
        {
            Plane P = new Plane();
            LineSegment3d l1 = new LineSegment3d(line1.StartPoint, line1.EndPoint);
            LineSegment3d l2 = new LineSegment3d(line2.StartPoint, line2.EndPoint);
            return l1.IsCoplanarWith(l2, out P);
        }
        #endregion

        #region 垂直
        /// <summary>
        /// 判断直线是否垂直
        /// </summary>
        /// <param name="line1">直线1</param>
        /// <param name="line2">直线2</param>
        /// <param name="Allowance">容差</param>
        /// <returns>两线是否垂直</returns>
        public static bool Perpendicular(Line line1, Line line2, double allowance)
        {
            LineSegment3d l1 = new LineSegment3d(line1.StartPoint, line1.EndPoint);
            LineSegment3d l2 = new LineSegment3d(line2.StartPoint, line2.EndPoint);
            Tolerance tol = new Tolerance(allowance, allowance);
            return l1.IsPerpendicularTo(l2, tol);
        }

        #endregion

        #region 布尔运算
        /// <summary>
        /// 三维实体布尔运算
        /// </summary>
        /// <param name="solid1"></param>
        /// <param name="solid2"></param>
        /// <param name="type"></param>
        public static Solid3d BooleanOper(Solid3d solid1, Solid3d solid2, BooleanOperationType type)
        {
            solid1.BooleanOperation(type, solid2);
            return solid1;
        }
        #endregion
    }
}
