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
        #endregion
    }
}