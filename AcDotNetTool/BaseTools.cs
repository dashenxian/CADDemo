using ZwSoft.ZwCAD.Geometry;
using System;
using System.Collections.Generic;
using ZwSoft.ZwCAD.ApplicationServices;
using ZwSoft.ZwCAD.DatabaseServices;
using ZwSoft.ZwCAD.EditorInput;

namespace AcDotNetTool
{
    /// <summary>
    /// 基础处理
    /// </summary>
    public static partial class BaseTools
    {
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

        #endregion
        
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
        public static IEnumerable<Entity> GetSelection(FilterType[] tps)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            Entity entity = null;
            var EntityCollection = new List<Entity>();
            PromptSelectionOptions selops = new PromptSelectionOptions();
            // 建立选择的过滤器内容
            TypedValue[] filList = new TypedValue[tps.Length + 2];
            filList[0] = new TypedValue((int)DxfCode.Operator, "<or");
            filList[tps.Length + 1] = new TypedValue((int)DxfCode.Operator, "or>");
            for (int i = 0; i < tps.Length; i++)
            {
                filList[i + 1] = new TypedValue((int)DxfCode.Start, tps[i].ToString());
            }// 建立过滤器
            SelectionFilter filter = new SelectionFilter(filList);
            // 按照过滤器进行选择
            PromptSelectionResult ents = ed.GetSelection(selops, filter);
            if (ents.Status == PromptStatus.OK)
            {
                using (Transaction transaction = db.TransactionManager.StartTransaction())
                {
                    SelectionSet SS = ents.Value;
                    foreach (ObjectId id in SS.GetObjectIds())
                    {
                        entity = (Entity)transaction.GetObject(id, OpenMode.ForWrite, true);
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
    }
}