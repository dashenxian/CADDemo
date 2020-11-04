using System;
using System.Collections.Generic;
using System.Linq;
using AcDotNetTool;
using AcDotNetTool.Jigs;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

namespace TestCADRegion
{
    public class Class1
    {

        [CommandMethod("Test")]
        public void Test()
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            ViewTableRecord vtr = ed.GetCurrentView();

            var aa = vtr.ViewDirection;
            vtr.ViewDirection = new Vector3d(-1, -1, 1);
            ed.SetCurrentView(vtr);
            Application.DocumentManager.MdiActiveDocument.SendStringToExecute("zoom e ", false, false, false);
            //var mtext = BaseTools.Select("选择文本框") as MText;
            //if (mtext == null)
            //{
            //    return;
            //}
            //BaseTools.WriteMessage($"text:{mtext.Text}\r\ncontents:{mtext.Contents}");
        }
        [CommandMethod("IsBound")]
        public void IsBound()
        {
            var fillet = new Fillet();
            fillet.Test();
            return;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            var prePoint = SelectPoint("选择第1个点");
            var curPoint = SelectPoint("选择第2个点");
            var nextPoint = SelectPoint("选择第3个点");
            var v1 = prePoint - curPoint;
            var v2 = nextPoint - curPoint;
            var angle = v1.GetAngleTo(v2);
            ed.WriteMessage($"\n 第1个点:({prePoint.X},{prePoint.Y})");
            ed.WriteMessage($"\n 第2个点:({curPoint.X},{curPoint.Y})");
            ed.WriteMessage($"\n 第3个点:({nextPoint.X},{nextPoint.Y})");
            ed.WriteMessage("\n 角度:" + angle);
            ed.WriteMessage("\n 小于等于pi:" + (angle <= Math.PI / 2));
            ed.WriteMessage("");

            Vector2d maxV;
            Vector2d minV;
            if (v1.Angle > v2.Angle)
            {
                maxV = v1;
                minV = v2;
            }
            else
            {
                maxV = v2;
                minV = v1;
            }
            Vector2d zx = new Vector2d(minV.X, minV.Y);
            if (maxV.Angle - minV.Angle > Math.PI / 2)
            {
                zx = Rotate(zx, curPoint, -angle / 2);
            }
            else
            {
                zx = Rotate(zx, curPoint, angle / 2);
            }
            var mod = Math.Sqrt(Math.Pow(zx.X, 2) + Math.Pow(zx.Y, 2));
            var li = new Line(new Point3d(0, 0, 0), new Point3d(zx.X / mod, zx.Y / mod, 0));
            Move(li, new Point3d(0, 0, 0), curPoint.ToPoint3d());
            ToModelSpace(li);
            //Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            //Entity ent = Select("\n 选择对象");
            //if (ent == null)
            //{
            //    return;
            //}
            //ed.WriteMessage("\n Handle:" + ent.Handle);

            //var ent2 = ent.Clone() as Entity;
            //ToModelSpace(ent2);
            //ed.WriteMessage("\n 浅克隆对象:" + ent2.Handle);

            //var ent3 = ent.DeepClone() as Entity;



            //double maxX = 0;
            //double maxY = 0;
            //double minX = 0;
            //double minY = 0;
            //GetExtreme(ent as Polyline,out maxX,out minX,out maxY,out minY);

            //var bound = (ent as Polyline).Bounds;

            //Entity ent2 = Select("\n 选择对象2");
            //if ( ent2 == null)
            //{
            //    return;
            //}
            //ed.WriteMessage("\n 你选择的对象ObjectId:" + ent2.ObjectId.ToString());
            //var p1 = new DBObjectCollection();
            //p1.Add(ent);
            //var r1 = Region.CreateFromCurves(p1)[0] as Region;

            //var p2 = new DBObjectCollection();
            //p2.Add(ent2);
            //var r2 = Region.CreateFromCurves(p2)[0] as Region;

            //r1.BooleanOperation(BooleanOperationType.BoolIntersect, r2.Clone() as Region);


            //ed.WriteMessage("\n 是否包含:" + (r1.Area == r2.Area));
        }
        /// <summary>
        /// 指定基点与目标点移动实体
        /// </summary>
        /// <param name="ent">实体对象</param>
        /// <param name="BasePt">基点</param>
        /// <param name="TargetPt">目标点</param>
        public static void Move(Entity ent, Point3d basePt, Point3d targetPt)
        {
            Vector3d vec = targetPt - basePt;
            Matrix3d mt = Matrix3d.Displacement(vec);
            ent.TransformBy(mt);
        }
        /// <summary>
        /// 指定基点与旋转角度旋转实体
        /// </summary>
        /// <param name="ent">实体对象</param>
        /// <param name="basePt">基点</param>
        /// <param name="angle">旋转角度</param>
        public static Vector2d Rotate(Vector2d ent, Point2d basePt, double angle)
        {
            var mt = Matrix2d.Rotation(angle, basePt);

            return ent.TransformBy(mt);
        }

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
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
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
        /// 添加对象到模型空间
        /// </summary>
        /// <param name="ent">要添加的对象</param>
        /// <returns></returns>
        public static ObjectId ToModelSpace(Entity ent)
        {
            Database db = HostApplicationServices.WorkingDatabase;
            ObjectId entId;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = (BlockTable)trans.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord btr = (BlockTableRecord)trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
                entId = btr.AppendEntity(ent);
                trans.AddNewlyCreatedDBObject(ent, true);

                trans.Commit();
            }
            return entId;
        }
        public static bool GetExtreme(Polyline pl, out double maxX, out double minX, out double maxY, out double minY)
        {
            maxX = minX = maxY = minY = 0.0;
            if (pl == null || pl.NumberOfVertices < 1)
            {
                return false;
            }
            Point3d point = pl.GetPointAtParameter(0);
            maxX = minX = point.X;
            maxY = minY = point.Y;
            int count = pl.Closed ? pl.NumberOfVertices : pl.NumberOfVertices - 1;
            for (int i = 1; i < count; i++)
            {
                Point3d curPoint = pl.GetPointAtParameter(i);
                if (curPoint.X < minX)
                {
                    minX = curPoint.X;
                }

                if (curPoint.X > maxX)
                {
                    maxX = curPoint.X;
                }

                if (curPoint.Y < minY)
                {
                    minY = curPoint.Y;
                }

                if (curPoint.Y > maxY)
                {
                    maxY = curPoint.Y;
                }
            }
            return true;
        }
    }
}
