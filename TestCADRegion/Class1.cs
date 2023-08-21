using System;
using System.Linq;
using AcDotNetTool;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using AcDotNetTool.Extensions;
using AutoCAD;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;
#if ZWCAD
using ZwSoft.ZwCAD.Runtime;
using ZwSoft.ZwCAD.DatabaseServices;
using ZwSoft.ZwCAD.Geometry;
#elif AutoCAD
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.ApplicationServices;
#endif

namespace TestCADRegion
{

    public class Class1 : IExtensionApplication
    {
        [CommandMethod("Test")]
        public void Test()
        {
            for (int i = 0; i < 100; i++)
            {
                using var pl = new Polyline();
                pl.AddVertexAt(0, new Point2d(0, 0), 0, 0, 0);
                pl.AddVertexAt(1, new Point2d(10, 0), 0, 0, 0);
                pl.AddToModelSpace();
            }
        }
        [CommandMethod("Test1")]
        public void Test1()
        {
            var path = "d:/";
            object oCad = Autodesk.AutoCAD.ApplicationServices.Application.AcadApplication;
            Type tpCad = oCad.GetType();
            object oDoc = tpCad.InvokeMember("ActiveDocument", System.Reflection.BindingFlags.GetProperty, null, oCad, null);
            Type tpDoc = oDoc.GetType();
            object ass = tpDoc.InvokeMember("ActiveSelectionSet", System.Reflection.BindingFlags.GetProperty, null, oDoc, null);
            tpDoc.InvokeMember("Export", System.Reflection.BindingFlags.InvokeMethod, null, oDoc, new object[] { path + "\\abc", "DWG", ass });
            var ed = Application.DocumentManager.MdiActiveDocument.Editor;
            ed.SetImpliedSelection(new ObjectId[] { });
            //GC.Collect();
            //GC.WaitForPendingFinalizers();
        }
        
        [CommandMethod(globalName: "WH")]
        public void Wh()
        {
            Entity ent = BaseTools.Select("");
            BaseTools.WriteMessage(ent.Handle.Value + "");

        }
        public void ScaleMultiLineText(MText mtext, double scaleFactor)
        {
            // 获取多行文字的位置和大小
            Point3d location = mtext.Location;
            double height = mtext.ActualHeight;
            double width = mtext.ActualWidth;

            // 放大或缩小倍数
            double scale = scaleFactor;

            // 计算放大或缩小后的高度和宽度
            double scaledHeight = height * scale;
            double scaledWidth = width * scale;

            // 计算放大或缩小后的位置
            double deltaX = (width - scaledWidth) / 2.0;
            double deltaY = (height - scaledHeight) / 2.0;
            Point3d scaledLocation = new Point3d(location.X + deltaX, location.Y + deltaY, location.Z);

            // 执行放大或缩小操作
            using (Transaction tr = mtext.Database.TransactionManager.StartTransaction())
            {
                mtext.Location = scaledLocation;
                mtext.Height = scaledHeight;
                mtext.Width = scaledWidth;

                tr.Commit();
            }
        }
        /// <summary>
        /// 闭合区域检测
        /// </summary>
        [CommandMethod("MyFill")]
        public void MyFill()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            // 要求用户选择一个点
            PromptPointOptions ppo = new PromptPointOptions("\n请选择一个点: ");
            PromptPointResult ppr = ed.GetPoint(ppo);
            if (ppr.Status != PromptStatus.OK) return;
            Point3d pt = ppr.Value;

            // 通过该点的射线或线段与所有实体进行相交检测
            List<Curve> curves = new List<Curve>();
            List<Point3d> points = new List<Point3d>();
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);
                foreach (ObjectId id in btr)
                {
                    var ent = tr.GetObject(id, OpenMode.ForRead) as Curve;
                    if (ent != null)
                    {
                        Point3dCollection pts = new Point3dCollection();
                        ent.IntersectWith(new Xline() { BasePoint = pt }, Intersect.OnBothOperands, pts, IntPtr.Zero, IntPtr.Zero);
                        if (pts.Count > 0)
                        {
                            curves.Add(ent.GetOffsetCurves(-0.001)[0] as Curve);
                            points.AddRange(pts.Cast<Point3d>());
                        }
                    }
                }
            }

            // 将所有相交实体分为内部和外部两部分
            List<Curve> innerCurves = new List<Curve>();
            List<Curve> outerCurves = new List<Curve>();
            foreach (Curve curve in curves)
            {
                Point3d closestPt = curve.GetClosestPointTo(pt, false);
                if (closestPt.DistanceTo(pt) <= Tolerance.Global.EqualPoint)
                {
                    innerCurves.Add(curve);
                }
                else
                {
                    outerCurves.Add(curve);
                }
            }

            // 将内部实体和相交点集合组成一个闭合多段线
            Polyline pline = new Polyline();
            foreach (Curve curve in innerCurves)
            {
                Point3dCollection pts = new Point3dCollection();
                curve.IntersectWith(new Xline() { BasePoint = pt }, Intersect.OnBothOperands, pts, IntPtr.Zero, IntPtr.Zero);
                pline.AddVertexAt(pline.NumberOfVertices, pts[0].ToPoint2d(), 0, 0, 0);
            }
            pline.Closed = true;
            if (pline.Area < 0)
            {
                pline.ReverseCurve();
            }

            // 使用AutoCAD .NET API中提供的函数来创建填充图案
            Hatch hatch = new Hatch();
            hatch.SetDatabaseDefaults();
            hatch.SetHatchPattern(HatchPatternType.PreDefined, "SOLID");
            hatch.Associative = true;
            hatch.AppendLoop(HatchLoopTypes.Default, new ObjectIdCollection { pline.ObjectId });
            hatch.EvaluateHatch(true);
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTableRecord btr = (BlockTableRecord)tr.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(db), OpenMode.ForWrite);
                btr.AppendEntity(hatch);
                tr.AddNewlyCreatedDBObject(hatch, true);
                tr.Commit();
            }
        }


        [CommandMethod("CheckForPickfirstSelection", CommandFlags.UsePickSet)]
        public static void CheckForPickfirstSelection()
        {
            // Get the current document
            Editor acDocEd = Application.DocumentManager.MdiActiveDocument.Editor;

            // Get the PickFirst selection set
            PromptSelectionResult acSSPrompt;
            acSSPrompt = acDocEd.SelectImplied();

            SelectionSet acSSet;

            // If the prompt status is OK, objects were selected before
            // the command was started
            if (acSSPrompt.Status == PromptStatus.OK)
            {
                acSSet = acSSPrompt.Value;

                Application.ShowAlertDialog("Number of objects in Pickfirst selection: " +
                                            acSSet.Count.ToString());
            }
            else
            {
                Application.ShowAlertDialog("Number of objects in Pickfirst selection: 0");
            }

            // Clear the PickFirst selection set
            ObjectId[] idarrayEmpty = new ObjectId[0];
            acDocEd.SetImpliedSelection(idarrayEmpty);

            // Request for objects to be selected in the drawing area
            acSSPrompt = acDocEd.GetSelection();

            // If the prompt status is OK, objects were selected
            if (acSSPrompt.Status == PromptStatus.OK)
            {
                acSSet = acSSPrompt.Value;

                Application.ShowAlertDialog("Number of objects selected: " +
                                            acSSet.Count.ToString());
            }
            else
            {
                Application.ShowAlertDialog("Number of objects selected: 0");
            }
        }
        /// <summary>
        /// 竖排文本
        /// </summary>
        [CommandMethod("TestText")]
        public void TestText()
        {
            // 获取活动文档对象
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            // 获取数据库对象
            Database db = doc.Database;
            // 获取当前空间
            Editor ed = doc.Editor;

            // 提示用户选择插入点
            PromptPointResult ppr = ed.GetPoint("输入插入点：");
            if (ppr.Status != PromptStatus.OK)
            {
                return;
            }
            Point3d insertPt = ppr.Value;

            // 创建一个新的文本对象
            DBText text = new DBText();
            text.Position = insertPt;
            text.Height = 2.5;
            text.TextString = "这是一个竖排的文本";
            text.VerticalMode = TextVerticalMode.TextVerticalMid;
            text.AlignmentPoint = insertPt;

            // 创建一个矩阵，将文本旋转90度
            Matrix3d matrix = Matrix3d.Rotation(Math.PI / 2, Vector3d.ZAxis, insertPt);

            // 将矩阵应用于文本对象的位置和方向属性
            text.TransformBy(matrix);

            // 将文本对象添加到模型空间
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord ms = trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                ms.AppendEntity(text);
                trans.AddNewlyCreatedDBObject(text, true);
                trans.Commit();
            }

        }

        public static Polyline OffsetPolyline(Polyline polyline, double[] distances, bool direction)
        {
            // 创建偏移后的多段线
            Polyline offsetPolyline = new Polyline();

            // 遍历多段线的每一条线段
            for (int i = 0; i < polyline.NumberOfVertices - 1; i++)
            {
                Point2d startPoint = polyline.GetPoint2dAt(i);
                Point2d endPoint = polyline.GetPoint2dAt(i + 1);

                if (polyline.GetSegmentType(i) == SegmentType.Arc)
                {
                    // 计算圆弧的半径、圆心、起始角度和终止角度
                    CircularArc2d arc = polyline.GetArcSegment2dAt(i);
                    double radius = arc.Radius;
                    Point2d center = arc.Center;
                    double startAngle = arc.StartAngle;
                    double endAngle = arc.EndAngle;

                    // 计算偏移后的圆心
                    Vector2d offsetVector = (direction ? -1 : 1) * new Vector2d(arc.Center.X - startPoint.X, arc.Center.Y - startPoint.Y).GetNormal() *
                                            distances[i + 1];
                    Point2d offsetCenter = arc.Center + offsetVector;

                    // 计算偏移后的起始角度和终止角度
                    double offsetStartAngle = startAngle;
                    double offsetEndAngle = endAngle;
                    if (direction)
                    {
                        // 向内偏移，起始角度和终止角度都减小
                        offsetStartAngle -= Math.Atan2(offsetVector.Y, offsetVector.X);
                        offsetEndAngle -= Math.Atan2(offsetVector.Y, offsetVector.X);
                    }
                    else
                    {
                        // 向外偏移，起始角度和终止角度都增大
                        offsetStartAngle += Math.Atan2(offsetVector.Y, offsetVector.X);
                        offsetEndAngle += Math.Atan2(offsetVector.Y, offsetVector.X);
                    }

                    // 创建偏移后的圆弧
                    Arc offsetArc = new Arc(offsetCenter.ToPoint3d(), radius, offsetStartAngle, offsetEndAngle);

                    // 将偏移后的圆弧添加到偏移后的多段线中
                    //offsetPolyline.Append(offsetArc);
                    var bugle = BaseTools.GetBulge(offsetArc);

                    offsetPolyline.AddVertexAt(i, offsetArc.StartPoint.ToPoint2d(), bugle, 0, 0);
                    offsetPolyline.AddVertexAt(i + 1, offsetArc.EndPoint.ToPoint2d(), 0, 0, 0);
                }
                else
                {
                    // 计算偏移向量
                    Vector2d offsetVector = (endPoint - startPoint).GetNormal() * distances[i + 1];

                    if (direction)
                    {
                        // 向   // 计算偏移后的起点和终点
                        Point2d offsetStartPoint = startPoint + offsetVector;
                        Point2d offsetEndPoint = endPoint + offsetVector;

                        // 将偏移后的线段添加到偏移后的多段线中
                        offsetPolyline.AddVertexAt(i, offsetStartPoint, 0, 0, 0);
                        offsetPolyline.AddVertexAt(i + 1, offsetEndPoint, 0, 0, 0);
                    }
                }

                // 如果多段线是闭合的，将偏移后的多段线也闭合
                if (polyline.Closed && offsetPolyline.NumberOfVertices > 2)
                {
                    offsetPolyline.AddVertexAt(offsetPolyline.NumberOfVertices, offsetPolyline.StartPoint.ToPoint2d(), 0, 0, 0);
                    offsetPolyline.Closed = true;
                }
            }
            return offsetPolyline;
        }

        [CommandMethod("TestPrint")]
        public void TestPrint()
        {
            try
            {
                var outPut = @"C:\Users\Administrator\Desktop\新建文件夹";
                Document doc = Application.DocumentManager.MdiActiveDocument;
                Database db = doc.Database;
                var layouts = new List<Layout>();
                using (Transaction trans = db.TransactionManager.StartTransaction())
                {
                    var dic = trans.GetObject(db.LayoutDictionaryId, OpenMode.ForWrite) as DBDictionary;
                    foreach (var item in dic)
                    {
                        var layout = trans.GetObject(item.Value, OpenMode.ForWrite) as Layout;
                        var acplSetVdr = PlotSettingsValidator.Current;
                        //acplSetVdr.SetPlotType(layout, PlotType.Layout);
                        acplSetVdr.RefreshLists(layout);
                        acplSetVdr.SetPlotConfigurationName(layout, "DWG To PDF.pc3", "ISO_full_bleed_A4_(210.00_x_297.00_MM)");
                        layouts.Add(layout);
                    }
                    trans.Commit();
                }
                var multiSheetsPdf = new SingleSheetPdf(outPut, layouts);
                multiSheetsPdf.Publish();
            }
            catch (System.Exception e)
            {
                BaseTools.WriteMessage(e.Message + e.StackTrace);
            }

            //GC.Collect();
        }

        [CommandMethod("ToRegionToPolyLine")]
        public void ToRegionToPolyLine()
        {
            var c1 = BaseTools.Select("请选择多段线") as Polyline;
            if (c1 == null)
            {
                BaseTools.WriteMessage("选择错误");
                return;
            }

            var pl = c1.ToRegion();
            DataBaseTools.AddIn(pl);
        }

        private void Trim()
        {
            // Get the current document and database
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;
            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Block table for read
                BlockTable acBlkTbl;
                acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId,
                    OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write
                BlockTableRecord acBlkTblRec;
                acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
                    OpenMode.ForWrite) as BlockTableRecord;

                // Create a line that starts at (4,4,0) and ends at (7,7,0)
                using (Line acLine = new Line(new Point3d(4, 4, 0),
                           new Point3d(7, 7, 0)))
                {

                    // Add the new object to the block table record and the transaction
                    acBlkTblRec.AppendEntity(acLine);
                    acTrans.AddNewlyCreatedDBObject(acLine, true);

                    // Update the display and diaplay a message box
                    acDoc.Editor.Regen();
                    Application.ShowAlertDialog("Before extend");

                    // Double the length of the line
                    acLine.EndPoint = acLine.EndPoint + acLine.Delta;
                }

                // Save the new object to the database
                acTrans.Commit();
            }
        }

        private void TestJoin()
        {
            var p1 = new Point2d(1, 1);
            var p2 = new Point2d(10, 10);
            var p21 = new Point3d(10, 10, 0);
            var l1 = new Polyline();
            l1.AddVertexAt(0, p1, 0, 0, 0);
            l1.AddVertexAt(0, p2, 0, 0, 0);
            Point3d p3 = new Point3d(15, 8, 0);
            Line l2 = new Line(p21, p3);
            Point3d p4 = new Point3d(20, 20, 0);
            Line l3 = new Line(p3, p4);
            //Entity ent = new Entity[2];
            //ent[0] = l2;
            //ent[1] = l3;
            //l1.JoinEntities(ent);//JoinEntities为多条曲线相联结，曲线之间必须是连续的，参数为Entity数组
            l1.JoinEntity(l2);//两条曲线相联结

            //var c1 = BaseTools.Select("选择多段线") as Curve;
            //var c2 = BaseTools.Select("选择多段线") as Curve;
            //if (c1 == null || c2 == null)
            //{
            //    BaseTools.WriteMessage("选择的不是曲线。");
            //    return;
            //}
            //var trans = DataBaseTools.DocumentDatabase().TransactionManager.StartTransaction();
            //var c1W = trans.GetObject(c1.ObjectId, OpenMode.ForWrite) as Curve;
            //var c2W = trans.GetObject(c2.ObjectId, OpenMode.ForWrite) as Curve;
            //c1W.JoinEntity(c2W);
            ////DataBaseTools.AddIn(c1);
            //trans?.Commit();
            //trans?.Dispose();
        }


        private static void TestLock()
        {
            var str = File.ReadAllText(@"C:\Users\Administrator\Desktop\无标题.json");
            var qys = Newtonsoft.Json.JsonConvert.DeserializeObject<List<QY>>(str);
            var doc = Application.DocumentManager.MdiActiveDocument;
            using (DocumentLock doclock = doc.LockDocument())
            {
                using (var trans = doc.TransactionManager.StartTransaction())
                {
                    foreach (var qy in qys)
                    {
                        foreach (var handle in qy.ListHandle)
                        {
                            try
                            {
                                doc.Database.GetObjectId(false, new Handle(handle), 0);
                            }
                            catch (System.Exception e)
                            {
                                Console.WriteLine(e);
                            }
                        }
                    }
                }
            }
        }
        public static void SaveDoc()
        {
            var DwgSavePath = "d:/";
            //找到对应的htwj
            Document acDoc = Application.DocumentManager.MdiActiveDocument;

            if (acDoc == null)
            {
                return;
            }
            if (!Directory.Exists(DwgSavePath))
            {
                Directory.CreateDirectory(DwgSavePath);
            }
            string path = DwgSavePath + "\\test.dwg";
            FileStream fs = null;
            acDoc.Database.SaveAs(path, DwgVersion.Current);
            try
            {
                fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                var bytes = new byte[fs.Length];
                fs.Read(bytes, 0, (int)fs.Length);
            }
            catch (System.Exception ex)
            {
                string note = string.Empty;
                return;
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                }
            }
        }

        public void Initialize()
        {
            //throw new NotImplementedException();
            //Application.DocumentManager.DocumentCreateStarted += DocumentManager_DocumentCreateStarted;
            //Application.DocumentManager.DocumentCreated += DocumentManager_DocumentCreated;
            //Application.DocumentManager.DocumentCreationCanceled += DocumentManager_DocumentCreationCanceled;
            //Application.DocumentManager.DocumentDestroyed += DocumentManager_DocumentDestroyed;
            //Application.DocumentManager.DocumentToBeDestroyed += DocumentManager_DocumentToBeDestroyed;
            //Application.DocumentManager.DocumentToBeDeactivated += DocumentManager_DocumentToBeDeactivated;
            //Application.DocumentManager.DocumentActivated += DocumentManager_DocumentActivated;
            //var login = new LoginFrom();
            //var handle = Application.MainWindow.Handle;
            //NativeWindow main = new NativeWindow();
            //main.AssignHandle(handle);
            //login.ShowDialog(main);

        }

        private void DocumentManager_DocumentActivated(object sender, DocumentCollectionEventArgs e)
        {
            BaseTools.WriteMessage("DocumentManager_DocumentActivated\r\n");
        }

        private void DocumentManager_DocumentToBeDeactivated(object sender, DocumentCollectionEventArgs e)
        {
            BaseTools.WriteMessage("DocumentManager_DocumentToBeDeactivated\r\n");
        }

        private void DocumentManager_DocumentToBeDestroyed(object sender, DocumentCollectionEventArgs e)
        {
            BaseTools.WriteMessage("DocumentManager_DocumentToBeDestroyed\r\n");
        }

        private void DocumentManager_DocumentDestroyed(object sender, DocumentDestroyedEventArgs e)
        {
            BaseTools.WriteMessage("DocumentManager_DocumentDestroyed\r\n");
        }

        private void DocumentManager_DocumentCreationCanceled(object sender, DocumentCollectionEventArgs e)
        {
            BaseTools.WriteMessage("DocumentManager_DocumentCreationCanceled\r\n");
        }

        private void DocumentManager_DocumentCreated(object sender, DocumentCollectionEventArgs e)
        {
            BaseTools.WriteMessage("DocumentManager_DocumentCreated\r\n");
        }

        private void DocumentManager_DocumentCreateStarted(object sender, DocumentCollectionEventArgs e)
        {
            BaseTools.WriteMessage("DocumentManager_DocumentCreateStarted\r\n");
        }

        public void Terminate()
        {
            //throw new NotImplementedException();
        }
    }

    public class QY
    {
        public string CADENTITY { get; set; }
        public string GID { get; set; }

        public List<int> ListHandle
        {
            get
            {
                if (string.IsNullOrEmpty(CADENTITY))
                {
                    return new List<int>();
                }

                return CADENTITY.Split(',').Select(i => int.Parse(i)).ToList();
            }
        }
    }
}
