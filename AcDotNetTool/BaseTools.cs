using Autodesk.AutoCAD.Geometry;
using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using System.Collections.Generic;
using System.Linq;

namespace AcDotNetTool
{
    /// <summary>
    /// 基础处理
    /// </summary>
    public static partial class BaseTools
    {
        /// <summary>
        /// 容差
        /// </summary>
        public static Tolerance Tolerance = new Tolerance(0.000001, 0.000001);

        #region 坐标点转换

        /// <summary>
        /// 2d点转3d点，z值为0
        /// </summary>
        /// <param name="p2d">2d点</param>
        /// <returns>转换后的3d点</returns>
        public static Point3d ToPoint3d(this Point2d p2d)
        {
            return ToPoint3d(p2d, 0);
        }
        /// <summary>
        /// 2d点转3d点
        /// </summary>
        /// <param name="p2d">2d点</param>
        /// <param name="z">z轴值</param>
        /// <returns>转换后的3d点</returns>
        public static Point3d ToPoint3d(this Point2d p2d, double z)
        {
            return new Point3d(p2d.X, p2d.Y, z);
        }
        /// <summary>
        /// 3d点转2d点
        /// </summary>
        /// <param name="point">3d点</param>
        /// <returns>转换后的2d点</returns>
        public static Point2d ToPoint2d(this Point3d point)
        {
            return new Point2d(point.X, point.Y);
        }
        /// <summary>
        /// 转换为3d范围，Z坐标为0
        /// </summary>
        /// <param name="Extents2d"></param>
        /// <returns></returns>
        public static Extents3d ToExtents3d(this Extents2d Extents2d)
        {
            return new Extents3d(Extents2d.MinPoint.ToPoint3d(), Extents2d.MaxPoint.ToPoint3d());
        }
        /// <summary>
        /// 转换为2d范围，忽略Z坐标
        /// </summary>
        /// <param name="Extents2d"></param>
        /// <returns></returns>
        public static Extents2d ToExtents2d(this Extents3d Extents3d)
        {
            return new Extents2d(Extents3d.MinPoint.ToPoint2d(), Extents3d.MaxPoint.ToPoint2d());
        }
        /// <summary>
        /// 计算两点的中点
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static Point3d GetCenterPoint(Point3d p1, Point3d p2)
        {
            return new Point3d(
                (p1.X + p2.X) / 2,
                (p1.Y + p2.Y) / 2,
                (p1.Z + p2.Z) / 2);
        }
        /// <summary>
        /// 计算两点的中点
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static Point2d GetCenterPoint(Point2d p1, Point2d p2)
        {
            return new Point2d(
                (p1.X + p2.X) / 2,
                (p1.Y + p2.Y) / 2);
        }
        #endregion

        #region ObjectIdCollection转换
        /// <summary>
        /// 转换为List
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public static List<ObjectId> ToList(this ObjectIdCollection ids)
        {
            var list = new List<ObjectId>();
            foreach (ObjectId id in ids)
            {
                list.Add(id);
            }
            return list;
        }
        /// <summary>
        /// 转换为ObjectIdCollection对象
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public static ObjectIdCollection ToObjectIdCollection(this IEnumerable<ObjectId> ids)
        {
            var list = new ObjectIdCollection();
            foreach (ObjectId id in ids)
            {
                list.Add(id);
            }
            return list;
        }
        #endregion

        /// <summary>
        /// 创建面域
        /// </summary>
        /// <param name="polyline"></param>
        /// <returns></returns>
        public static Region CreateRegion(this Polyline polyline)
        {
            if (polyline.Area < 0.0000001)
            {
                return null;
            }
            var pl = polyline.Clone() as Polyline;
            pl.Closed = true;
            var dBObjectCollection = new DBObjectCollection();
            dBObjectCollection.Add(pl);
            var reg = Region.CreateFromCurves(dBObjectCollection)[0] as Region;
            return reg;
        }

        #region 角度与弧度转换
        /// <summary>
        /// 角度转化为弧度
        /// </summary>
        /// <param name="degree">角度值</param>
        /// <returns></returns>
        public static double DegreeToAngle(Double degree)
        {
            return degree * Math.PI / 180;
        }
        /// <summary>
        /// 弧度转换角度
        /// </summary>
        /// <param name="angle">弧度制</param>
        /// <returns></returns>
        public static double AngleToDegree(Double angle)
        {
            return angle * 180 / Math.PI;
        }
        #endregion



        #region 命令行输出

        public static void WriteMessage(string msg)
        {
            var ed = Application.DocumentManager.MdiActiveDocument.Editor;
            ed.WriteMessage(msg);
        }
        #endregion


        #region 选择实体
        /// <summary>
        /// 选择点
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        public static Point2d SelectPoint(string word)
        {
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            var ent = ed.GetPoint(word);
            return new Point2d(ent.Value.X, ent.Value.Y);
        }

        /// <summary>
        /// 提示用户选择单个实体
        /// </summary>
        /// <param name="word">选择提示</param>
        /// <returns>实体对象</returns>
        public static Entity Select(string word)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            Entity entity = null;

            PromptEntityResult ent = ed.GetEntity(word);
            if (ent.Status == PromptStatus.OK)
            {
                using (Transaction transaction = db.TransactionManager.StartTransaction())
                {
                    entity = (Entity)transaction.GetObject(ent.ObjectId, OpenMode.ForWrite, true);
                    transaction.Commit();
                }
            }
            return entity;
        }

        /// <summary>
        /// 提示用户选择实体
        /// </summary>
        /// <param name="tps">类型过滤枚举类</param>
        /// <returns></returns>
        public static IEnumerable<Entity> GetSelection()
        {
            return GetSelection(null);
        }
        /// <summary>
        /// 提示用户选择实体
        /// </summary>
        /// <param name="tps">类型过滤枚举类</param>
        /// <returns></returns>
        public static IEnumerable<Entity> GetSelection(FilterType[] tps)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            var EntityCollection = new List<Entity>();

            PromptSelectionResult ents;
            if (tps == null || !tps.Any())
            {
                ents = ed.GetSelection();
            }
            else// 按照过滤器进行选择
            {
                // 建立选择的过滤器内容
                TypedValue[] filList = new TypedValue[tps.Length + 2];
                filList[0] = new TypedValue((int)DxfCode.Operator, "<or");
                filList[tps.Length + 1] = new TypedValue((int)DxfCode.Operator, "or>");
                for (int i = 0; i < tps.Length; i++)
                {
                    filList[i + 1] = new TypedValue((int)DxfCode.Start, tps[i].ToString());
                }// 建立过滤器
                SelectionFilter filter = new SelectionFilter(filList);
                ents = ed.GetSelection(filter);
            }

            if (ents.Status == PromptStatus.OK)
            {
                using (Transaction transaction = db.TransactionManager.StartTransaction())
                {
                    SelectionSet SS = ents.Value;
                    foreach (ObjectId id in SS.GetObjectIds())
                    {
                        var entity = (Entity)transaction.GetObject(id, OpenMode.ForWrite, true);
                        if (entity != null)
                            EntityCollection.Add(entity);
                    }
                    transaction.Commit();
                }
            }
            return EntityCollection;
        }

        /// <summary>
        /// 类型过滤枚举类
        /// </summary>
        public enum FilterType
        {
            Curve,
            Dimension,
            /// <summary>
            /// 包含Polyline2d（二维多段线）、Polyline3d（三维多段线），不包含pline多段线;
            /// </summary>
            Polyline,
            /// <summary>
            /// 可用，多段线，用pline绘制的
            /// </summary>
            Lwpolyline,
            BlockRef,
            Circle,
            Line,
            Arc,
            Text,
            MText
        }
        #endregion

        /// <summary>
        /// 是否为逆时针
        /// </summary>
        /// <param name="Vertices"></param>
        /// <returns></returns>
        public static bool IsCCW(List<Point3d> Vertices)
        {
            Point3d hip, p, prev, next;
            int hii, i;
            int nPts = Vertices.Count;

            if (nPts < 4) return false;

            hip = Vertices[0];
            hii = 0;
            for (i = 1; i < nPts; i++)
            {
                p = Vertices[i];
                if (p.Y > hip.Y)
                {
                    hip = p;
                    hii = i;
                }
            }

            int iPrev = hii - 1;
            if (iPrev < 0) iPrev = nPts - 2;
            int iNext = hii + 1;
            if (iNext >= nPts) iNext = 1;
            prev = Vertices[iPrev];
            next = Vertices[iNext];

            double prev2x = prev.X - hip.X;
            double prev2y = prev.Y - hip.Y;
            double next2x = next.X - hip.X;
            double next2y = next.Y - hip.Y;
            double disc = next2x * prev2y - next2y * prev2x;

            if (disc == 0.0)
            {
                return (prev.X > next.X);
            }
            else
            {
                return (disc > 0.0);
            }
        }

        #region 是否在范围内
        /// <summary>
        /// 判断曲线是否在另一条曲线范围内
        /// </summary>
        /// <param name="outLine">范围线</param>
        /// <param name="inLine">内部曲线</param>
        /// <returns></returns>
        public static bool IsInside(Polyline outLine, Polyline inLine)
        {
            return IsInside(outLine, inLine, Tolerance.EqualPoint);
        }
        /// <summary>
        /// 判断曲线是否在另一条曲线范围内
        /// </summary>
        /// <param name="outLine">范围线</param>
        /// <param name="inLine">内部曲线</param>
        /// <param name="tolerance">容差</param>
        /// <returns></returns>
        public static bool IsInside(Polyline outLine, Polyline inLine, double tolerance)
        {
            if (outLine.Bounds.HasValue && inLine.Bounds.HasValue)
            {
                if (!IsInside(outLine.Bounds.Value.ToExtents2d(), inLine.Bounds.Value.ToExtents2d()))
                {
                    return false;
                }
            }
            //如果内部线为直线
            if (inLine.Area == 0)
            {
                var lines = new DBObjectCollection();
                inLine.Explode(lines);
                foreach (Line line in lines)
                {
                    if (!IsInside(outLine, line))
                    {
                        return false;
                    }
                }
                return true;
            }

            //如果内部线不是直线
            if (inLine.Area > outLine.Area + tolerance)
            {
                return false;
            }

            var inReg = inLine.CreateRegion();
            var outReg = outLine.CreateRegion();

            var inRegArea = inReg.Area;
            outReg.BooleanOperation(BooleanOperationType.BoolIntersect, inReg);

            return outReg.Area == inRegArea;
        }
        /// <summary>
        /// 判断点是否在曲线范围内
        /// </summary>
        /// <param name="outLine">范围线</param>
        /// <param name="point">点</param>
        /// <returns></returns>
        public static bool IsInside(Polyline outLine, Point2d point)
        {
            return IsInside(outLine, point, Tolerance);
        }
        /// <summary>
        /// 判断点是否在曲线范围内
        /// </summary>
        /// <param name="outLine">范围线</param>
        /// <param name="point">点</param>
        /// <param name="tolerance">容差</param>
        /// <returns></returns>
        public static bool IsInside(Polyline outLine, Point2d point, Tolerance tolerance)
        {
            var outL = outLine.Clone() as Polyline;
            if (!outL.Closed)
            {
                outL.Closed = true;
            }

            var closestPoint = outL.GetClosestPointTo(point.ToPoint3d(), false);
            if (closestPoint.IsEqualTo(point.ToPoint3d(), tolerance))
            {
                return true;
            }

            var ray = new Ray();
            ray.BasePoint = point.ToPoint3d();
            ray.SecondPoint = new Point3d(point.X + 1, 0, 0);
            Point3dCollection points = new Point3dCollection();
            ray.IntersectWith(outL, Intersect.OnBothOperands, points, new IntPtr(), new IntPtr());
            if (points.Count % 2 == 1)
            {
                return true;
            }

            return false;
        }
        /// <summary>
        /// 判断边界是否在范围内
        /// </summary>
        /// <param name="outExtent">外部范围</param>
        /// <param name="inExtent">内部范围</param>
        /// <returns></returns>
        public static bool IsInside(Extents2d outExtent, Extents2d inExtent)
        {
            return IsInside(outExtent, inExtent, Tolerance.EqualPoint);
        }
        /// <summary>
        /// 判断边界是否在范围内
        /// </summary>
        /// <param name="outExtent">外部范围</param>
        /// <param name="inExtent">内部范围</param>
        /// <param name="tolerance">容差</param>
        /// <returns></returns>
        public static bool IsInside(Extents2d outExtent, Extents2d inExtent, double tolerance)
        {
            if (outExtent.MaxPoint.X + tolerance < inExtent.MaxPoint.X
                || outExtent.MaxPoint.Y + tolerance < inExtent.MaxPoint.Y
                || outExtent.MinPoint.X - tolerance > inExtent.MinPoint.X
                || outExtent.MinPoint.Y - tolerance > inExtent.MinPoint.Y
            )
            {
                return false;
            }

            return true;
        }
        /// <summary>
        /// 判断直线是否在范围内
        /// </summary>
        /// <param name="outLine">外部范围</param>
        /// <param name="inLine">直线</param>
        /// <returns></returns>
        public static bool IsInside(Polyline outLine, Line inLine)
        {
            //直线的端点、交点、交点的中点都在内部，则直线在内部
            var points = new Point3dCollection();
            inLine.IntersectWith(outLine, Intersect.OnBothOperands, points, new IntPtr(), new IntPtr());
            var list = new List<Point2d>();
            list.Add(inLine.StartPoint.ToPoint2d());
            foreach (Point3d point in points)
            {
                list.Add(point.ToPoint2d());
            }
            list.Add(inLine.EndPoint.ToPoint2d());

            for (int i = 0; i < list.Count - 1; i++)
            {
                if (!IsInside(outLine, list[i])
                    || !IsInside(outLine, GetCenterPoint(list[i], list[i + 1]))
                    || !IsInside(outLine, list[i + 1]))
                {
                    return false;
                }
            }
            return true;
        }
        #endregion
    }
}