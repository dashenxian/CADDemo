﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AcDotNetTool.Extensions;
using System.Security.Cryptography;


#if ZWCAD
using ZwSoft.ZwCAD.ApplicationServices;
using ZwSoft.ZwCAD.DatabaseServices;
using ZwSoft.ZwCAD.EditorInput;
using ZwSoft.ZwCAD.Geometry;
using ZwSoft.ZwCAD.Runtime;
using ZwSoft.ZwCAD.BoundaryRepresentation;
#elif AutoCAD
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.BoundaryRepresentation;
#endif

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
        public static Tolerance Tolerance = new Tolerance(0.0000001, 0.0000001);

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
            return new Extents2d(
                Extents3d.MinPoint.X, Extents3d.MinPoint.Y,
                Extents3d.MaxPoint.X, Extents3d.MaxPoint.Y);
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
        /// <summary>
        /// 转为List
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IEnumerable<T> ToList<T>(this IList source)
        {
            var list = new List<T>();
            foreach (var item in source)
            {
                if (item is T)
                {
                    list.Add((T)item);
                }
            }
            return list;
        }
        /// <summary>
        /// 列表转为cad集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static T ToCollection<T, T2>(this IEnumerable<T2> source) where T : IList, new()
        {
            var list = new T();
            foreach (var item in source)
            {
                list.Add(item);
            }
            return list;
        }

        #endregion

        /// <summary>
        /// 创建面域
        /// </summary>
        /// <param name="polyline"></param>
        /// <returns></returns>
        public static Region ToRegion(this Polyline polyline)
        {
            if (polyline.Area < Tolerance.EqualPoint)
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

        /// <summary>
        /// 面域转为多段线
        /// </summary>
        /// <param name="region"></param>
        /// <returns></returns>
        public static Polyline ToPolyline(this Region region)
        {
            Polyline pl = new Polyline();
            var brep = new Brep(region);
            var edges = brep.Edges.ToList();
            var list = new List<PolylinePoint>();
            var curEdge = edges[0];
            Point3d? endPoint = curEdge.Curve.StartPoint;
            do
            {
                Point3d? startPoint = null;
                curEdge = edges.FirstOrDefault(i => endPoint == i.Curve.StartPoint);
                var isReverse = 1;
                if (curEdge != null)
                {
                    endPoint = curEdge.Curve.EndPoint;
                    startPoint = curEdge.Curve.StartPoint;
                }
                else
                {
                    curEdge = edges.FirstOrDefault(i => i != curEdge && endPoint == i.Curve.EndPoint);
                    endPoint = curEdge?.Curve.StartPoint;
                    startPoint = curEdge?.Curve.EndPoint;
                    isReverse = -1;
                }

                if (curEdge == null)
                {
                    break;
                }
                edges.Remove(curEdge);
                double bulge;
                var eCurve = (ExternalCurve3d)curEdge.Curve;
                if (eCurve.IsCircularArc)
                {
                    var arc = Arc.CreateFromGeCurve(eCurve.NativeCurve) as Arc;
                    bulge = GetBulge(arc) * isReverse;
                }
                else
                {
                    bulge = 0;
                }

                if (bulge != 0)
                {
                    if (list.Count != 0)
                    {
                        list.RemoveAt(list.Count - 1);
                    }
                    list.Add(new PolylinePoint { Point2d = startPoint.Value.ToPoint2d(), Bulge = bulge });
                }

                list.Add(new PolylinePoint { Point2d = endPoint.Value.ToPoint2d(), Bulge = bulge });

            } while (true);
            ////获取边界线
            //foreach (var edge in edges)
            //{
            //    var eCurve = (ExternalCurve3d)edge.Curve;

            //    if (eCurve.IsCircularArc || eCurve.IsLineSegment)
            //    {
            //        var curve = Polyline.CreateFromGeCurve(eCurve.NativeCurve);
            //        list.Add(curve);
            //    }
            //}
            ////获取到的边界可能不是第二条线的起点对应第一条线的终点，而是第一条线的起点对应第二条线的终点，线的顺序是反向，
            ////这里重新排序，保证边界线是顺序相接的
            //if (list.Count > 1 && list[0].StartPoint == list[1].EndPoint)
            //{
            //    list.Reverse();
            //}
            //重新绘制多段线
            foreach (var polylinePoint in list)
            {
                pl.AddVertexAt(pl.NumberOfVertices, polylinePoint.Point2d, polylinePoint.Bulge, 0, 0);
                //if (curve is Line)
                //{
                //    if (pl.NumberOfVertices == 0)
                //    {
                //        pl.AddVertexAt(pl.NumberOfVertices, curve.StartPoint.ToPoint2d(), 0, 0, 0);
                //    }
                //    pl.AddVertexAt(pl.NumberOfVertices, curve.EndPoint.ToPoint2d(), 0, 0, 0);
                //}
                //else if (curve is Arc)
                //{
                //    var arc = curve as Arc;
                //    var bulge = GetBulge(arc);
                //    if (bulge != 0)
                //    {
                //        if (pl.NumberOfVertices != 0)
                //        {
                //            pl.RemoveVertexAt(pl.NumberOfVertices - 1);
                //        }
                //        pl.AddVertexAt(pl.NumberOfVertices, curve.StartPoint.ToPoint2d(), bulge, 0, 0);
                //    }
                //    pl.AddVertexAt(pl.NumberOfVertices, curve.EndPoint.ToPoint2d(), 0, 0, 0);
                //}
            }
            return pl;
        }


        class PolylinePoint
        {
            public Point2d Point2d { get; set; }
            public double Bulge { get; set; }
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
        /// <summary>
        /// 获取角度
        /// </summary>
        /// <param name="arc"></param>
        /// <returns></returns>
        public static double GetBulge(this Arc arc)
        {
            var diffAngle = arc.EndAngle - arc.StartAngle;
            double bulge;
            if (diffAngle < 0)
            {
                diffAngle += 2 * Math.PI;
                bulge = Math.Tan(diffAngle / 4) * -1;
            }
            else
            {
                bulge = Math.Tan(diffAngle / 4);
            }

            //弧线要根据正向或反向弧计算bulge
            bulge *= arc.Normal.Z;
            return bulge;
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
            Document doc = Application.DocumentManager.MdiActiveDocument;
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
        public static IEnumerable<Entity> Selects(string word)
        {
            return Selects(word, Array.Empty<FilterType>());
        }
        /// <summary>
        /// 提示用户选择实体
        /// </summary>
        /// <param name="tps">类型过滤枚举类</param>
        /// <returns></returns>
        public static IEnumerable<Entity> Selects(string word, FilterType[] tps)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            var EntityCollection = new List<Entity>();

            PromptSelectionResult ents;
            var opt = new PromptSelectionOptions();
            opt.MessageForAdding = word;
            if (tps == null || !tps.Any())
            {

                ents = ed.GetSelection(opt);
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
                ents = ed.GetSelection(opt, filter);
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

        /// <summary>
        /// 是否为逆时针
        /// </summary>
        /// <param name="Vertices"></param>
        /// <returns></returns>
        public static bool IsCCW(this Polyline pl)
        {
            var points = new List<Point3d>();
            for (int i = 0; i < pl.NumberOfVertices; i++)
            {
                points.Add(pl.GetPoint3dAt(i));
            }
            return IsCCW(points);
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

            var inReg = inLine.ToRegion();
            var outReg = outLine.ToRegion();

            var inRegArea = inReg.Area;
            outReg.BooleanOperation(BooleanOperationType.BoolIntersect, inReg);

            return Math.Abs(outReg.Area - inRegArea) < tolerance;
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
            ray.UnitDir = new Vector3d(1, 0, 0);
            Point3dCollection points = new Point3dCollection();
            ray.IntersectWith(outL, Intersect.OnBothOperands, points, 0, 0);
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
            if (outExtent.MaxPoint.X + tolerance > inExtent.MaxPoint.X
                && outExtent.MaxPoint.Y + tolerance > inExtent.MaxPoint.Y
                && outExtent.MinPoint.X - tolerance < inExtent.MinPoint.X
                && outExtent.MinPoint.Y - tolerance < inExtent.MinPoint.Y
            )
            {
                return true;
            }

            return false;
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
            inLine.IntersectWith(outLine, Intersect.OnBothOperands, points, 0, 0);
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

        /// <summary>
        /// 修改曲线长度
        /// </summary>
        /// <param name="arc">曲线</param>
        /// <param name="startPoint">新的起点</param>
        /// <param name="endPoint">新的终点</param>
        public static void EditArcEndPoint(this Arc arc, Point3d p1, Point3d p2)
        {
            var startPoint = p1;
            var endPoint = p2;
            var line1 = new Line(arc.StartPoint, startPoint);
            var line2 = new Line(arc.EndPoint, endPoint);
            var points = new Point3dCollection();
            line1.IntersectWith(line2, Intersect.OnBothOperands, points, IntPtr.Zero, IntPtr.Zero);
            if (points.Count != 0)
            {
                startPoint = p2;
                endPoint = p1;
            }

            var stargAngle = Vector3d.XAxis.GetAngleTo(startPoint - arc.Center, arc.Normal);
            var endAngle = Vector3d.XAxis.GetAngleTo(endPoint - arc.Center, arc.Normal);
            arc.StartAngle = stargAngle;
            arc.EndAngle = endAngle;
            //Point3d midPoint = arc.GetPointAtParameter((arc.StartParam + arc.EndParam) / 2.0);
            //CircularArc3d circArc = new CircularArc3d(arc.StartPoint, midPoint, endPoint);
            //var angle = Vector3d.XAxis.GetAngleTo(arc.StartPoint - circArc.Center, circArc.Normal);
            //if (!arc.IsWriteEnabled)
            //    arc.UpgradeOpen();
            //arc.Center = circArc.Center;
            //arc.Normal = circArc.Normal;
            //arc.Radius = circArc.Radius;
            //arc.StartAngle = circArc.StartAngle + angle;
            //arc.EndAngle = circArc.EndAngle + angle;
        }

        /// <summary>
        /// 判断两根线是否平行
        /// </summary>
        /// <param name="line1"></param>
        /// <param name="line2"></param>
        /// <returns></returns>
        public static bool Parallel(this Line line1, Line line2)
        {
            return line1.Delta.IsParallelTo(line2.Delta);
        }

        #region 不均匀偏移多段线
        /// <summary>
        /// 不均匀偏移多段线
        /// </summary>
        /// <param name="pl"></param>
        /// <param name="widths"></param>
        public static void Offset(this Polyline pl, IEnumerable<double> widths)
        {
            //是否逆时针
            var iscw = pl.IsCCW();
            DBObjectCollection objs = new DBObjectCollection();
            pl.Explode(objs);
            var widthList = widths.ToList();
            if (widthList.Count != objs.Count)
            {
                throw new ArgumentException("偏移宽度列表数量不等于多段线数量");
            }

            var lineDirection = iscw ? -1 : 1;
            var lines = new List<Curve>();
            for (int i = 0; i < objs.Count; i++)
            {
                var obj = objs[i];
                var preIndex = i - 1 < 0 ? objs.Count : i - 1;
                var preDist = widthList[preIndex];
                var dist = widthList[i];
                var line = obj as Curve;
                if (line == null)
                {
                    continue;
                }
                var direction = lineDirection;
                if (line is Arc)
                {
                    var pl1 = new Polyline();
                    pl1.AddVertexAt(0, line.StartPoint.ToPoint2d(), 0, 0, 0);
                    pl1.AddVertexAt(1, line.EndPoint.ToPoint2d(), 0, 0, 0);
                    pl1.JoinEntity(line);
                    direction = BaseTools.IsInside(pl, pl1) ? 1 : -1;
                }
                else if (line is Line)
                {
                    var last = lines.LastOrDefault() as Line;
                    if (last != null && ((Line)line).Parallel(last) && preDist == dist || line.StartPoint == line.EndPoint)
                    {
                        continue;
                    }
                    else if (last != null && ((Line)line).Parallel(last))
                    {
                        var newLine = line.Offset(dist * direction)[0];
                        lines.AddRange(new[] { new Line(last.EndPoint, newLine.StartPoint) });
                    }
                }
                lines.AddRange(line.Offset(dist * direction));
                //index++;
            }

            if (lines.Count == 0)
            {
                BaseTools.WriteMessage("没有找到偏移的多段线");
                return;
            }
            TrimCurve(lines);
            ExtendIntersect(lines, iscw);
            var newPl = new Polyline();
            newPl.JoinEntities(lines.ToArray());
            var region = Region.CreateFromCurves(lines.ToDBObjectCollection());
            throw new NotImplementedException("返回数据还没实现");
        }

        /// <summary>
        /// 修剪线段到足够短，避免相交线段超出
        /// </summary>
        /// <param name="lines"></param>
        private static void TrimCurve(List<Curve> lines)
        {
            for (int i = 0; i < lines.Count(); i++)
            {
                if (lines[i] is Arc)
                {
                    var length = ((Arc)lines[i]).Length;
                    var startPoint = lines[i].GetPointAtDist(length / 2 - length / 100);
                    var endPoint = lines[i].GetPointAtDist(length / 2 + length / 100);
                    var points = new Point3dCollection() { startPoint, endPoint };
                    lines[i] = lines[i].GetSplitCurves(points)[1] as Curve;
                }
                else
                {
                    var length = lines[i].EndParam - lines[i].StartParam;
                    var startPoint = lines[i].GetPointAtDist(length / 2 - length / 100);
                    var endPoint = lines[i].GetPointAtDist(length / 2 + length / 100);
                    lines[i].StartPoint = startPoint;
                    lines[i].EndPoint = endPoint;
                }
            }
        }

        /// <summary>
        /// 延长线段相交
        /// </summary>
        /// <param name="lines"></param>
        /// <param name="iscw">是否逆时针</param>
        private static void ExtendIntersect(List<Curve> lines, bool iscw)
        {
            var intersectPoints = new List<Point3d>();
            for (int i = 0; i < lines.Count; i++)
            {
                var nextIndex = i + 1 == lines.Count ? 0 : i + 1;
                var nextPoints = new Point3dCollection();
                lines[i].IntersectWith(lines[nextIndex], Intersect.ExtendBoth, nextPoints, IntPtr.Zero, IntPtr.Zero);
                //prePoints.Count == 0 ||
                if (nextPoints.Count == 0)
                {
                    throw new System.Exception("缺少交点");
                }
                var nextPoint = nextPoints[0];
                if (nextPoints.Count > 1)
                {
                    var length = lines[i].EndParam - lines[i].StartParam;
                    var middlePoint = lines[i].GetPointAtDist(length / 2);
                    var nextLength = lines[nextIndex].EndParam - lines[nextIndex].StartParam;
                    var nextMiddlePoint = lines[nextIndex].GetPointAtDist(nextLength / 2);
                    foreach (Point3d point3d in nextPoints)
                    {
                        if (nextPoint.X < Math.Max(nextMiddlePoint.X, middlePoint.X)
                            && nextPoint.X > Math.Min(nextMiddlePoint.X, middlePoint.X)
                            && nextPoint.Y < Math.Max(nextMiddlePoint.Y, middlePoint.Y)
                            && nextPoint.Y > Math.Min(nextMiddlePoint.Y, middlePoint.Y)
                           )
                        {
                            break;
                        }
                        else
                        {
                            nextPoint = point3d;
                        }
                    }
                }

                intersectPoints.Add(nextPoint);

            }
            for (int i = 0; i < lines.Count; i++)
            {
                var preIndex = i - 1 < 0 ? lines.Count - 1 : i - 1;
                var prePoint = intersectPoints[preIndex];
                var nextPoint = intersectPoints[i];
                if (lines[i] is Arc)
                {
                    BaseTools.EditArcEndPoint((Arc)lines[i], prePoint, nextPoint);
                }
                else
                {
                    lines[i].Extend(true, prePoint);
                    lines[i].Extend(false, nextPoint);
                }
            }
        }
        #endregion

        #region 分割曲线

        /// <summary>
        /// 分割曲线
        /// </summary>
        /// <param name="source">被分割的曲线</param>
        /// <param name="splitCurve">分割的曲线</param>
        /// <param name="maxSplitNumber">分割后的区域数量，当被分割曲线和分割曲线有超过2个交点时，可以被分割成超过2个区域</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static IEnumerable<Polyline> GetSplitCurves(this Polyline source, Curve splitCurve, int maxSplitNumber = 2)
        {
            var points = new Point3dCollection();
            source.IntersectWith(splitCurve, Intersect.OnBothOperands, points, IntPtr.Zero, IntPtr.Zero);
            if (points.Count < 2)
            {
                throw new ArgumentException("分割曲线与原曲线的交点少于2个，无法分割");
            }

            if (!source.Closed)
            {
                source = source.Clone() as Polyline;
                source.Closed = true;
            }
            //分割点的顺序必须与起点方向一致，否则分割数量为1
            var splitPoints = points.ToList<Point3d>().OrderBy(i => source.GetDistAtPoint(i)).Take(maxSplitNumber).ToCollection<Point3dCollection, Point3d>();

            var splitCurves = source.GetSplitCurves(splitPoints);
            var pls = splitCurves.ToList<Polyline>();
            foreach (var pl in pls)
            {
                if (pl.GetBulgeAt(pl.NumberOfVertices - 1) != 0)
                {
                    pl.SetBulgeAt(pl.NumberOfVertices - 1, 0);
                }
                pl.Closed = true;
            }
            return pls;
        }

        /// <summary>
        /// 分割带有环岛的区域多段线，所有环岛应该在最外层多段线内
        /// </summary>
        /// <param name="sources">被分割区域多段线集合</param>
        /// <param name="splitCurve">分割曲线</param>
        /// <param name="maxSplitNumber">分割后的区域数量，当被分割曲线和分割曲线有超过2个交点时，可以被分割成超过2个区域</param>
        /// <returns></returns>
        public static IEnumerable<IEnumerable<Polyline>> GetSplitCurves(this IEnumerable<Polyline> sources, Curve splitCurve,
            int maxSplitNumber = 2)
        {
            if (sources == null || sources.Count() == 0)
            {
                throw new ArgumentNullException("被分割区域不能为空");
            }
            sources = sources.Select(i =>
            {
                if (i.Closed == false)
                {
                    i = i.Clone() as Polyline;
                    i.Closed = true;
                }
                return i;
            }).OrderByDescending(i => i.Area);
            var outLine = sources.First();
            //分割最外层线
            var splitOutLines = GetSplitCurves(outLine, splitCurve, maxSplitNumber);

            var resultTemp = splitOutLines.Select(i => new SplitCurvesTemp { OutLine = i }).ToList();
            //如果有环岛，则要分割环岛，并把环岛分到对应分割后的外边线中
            if (sources.Count() > 1)
            {
                var inLines = sources.Skip(1).ToList();
                foreach (var inLine in inLines)
                {
                    var points = new Point3dCollection();
                    inLine.IntersectWith(splitCurve, Intersect.OnBothOperands, points, IntPtr.Zero, IntPtr.Zero);
                    //环岛需要分割，分割后在分别分配
                    if (points.Count > 1)
                    {
                        var splitInLines = GetSplitCurves(inLine, splitCurve, points.Count);
                        foreach (var splitInLine in splitInLines)
                        {
                            foreach (var splitOutLine in resultTemp)
                            {
                                //如果分割后的环岛在外围线内则合并区域
                                if (IsInside(splitOutLine.OutLine, splitInLine))
                                {
                                    var inRegion = splitInLine.ToRegion();
                                    var outRegion = splitOutLine.OutLine.ToRegion();
                                    outRegion.BooleanOperation(BooleanOperationType.BoolSubtract, inRegion);
                                    splitOutLine.OutLine = outRegion.ToPolyline();
                                    outRegion.ColorIndex = 1;
                                    DataBaseTools.AddIn(outRegion);
                                    splitOutLine.OutLine.ColorIndex = 2;
                                    DataBaseTools.AddIn(splitOutLine.OutLine);
                                    break;
                                }
                            }
                        }
                    }
                    //环岛不需要分割，直接分配
                    else
                    {
                        foreach (var splitOutLine in resultTemp)
                        {
                            if (IsInside(splitOutLine.OutLine, inLine))
                            {
                                splitOutLine.InLines.Add(inLine);
                                break;
                            }
                        }
                    }
                }

            }

            var result = resultTemp.Select(i =>
            {
                var ls = new List<Polyline> { i.OutLine };
                ls.AddRange(i.InLines);
                return ls;
            });
            return result;
        }

        #region 中间类型
        /// <summary>
        /// 分割带有环岛的区域多段线临时类型
        /// </summary>
        class SplitCurvesTemp
        {
            public Polyline OutLine { get; set; }
            public IList<Polyline> InLines { get; set; } = new List<Polyline>();
        }
        #endregion

        #endregion
    }
}