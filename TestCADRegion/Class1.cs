using System;
using System.Linq;
using System.Threading;
using AcDotNetTool;
using System.Collections.Generic;
using System.IO;
using AcDotNetTool.Extensions;
#if ZWCAD
using ZwSoft.ZwCAD.Runtime;
using ZwSoft.ZwCAD.DatabaseServices;
using ZwSoft.ZwCAD.Geometry;
using System.Threading;
#elif AutoCAD
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.ApplicationServices;
#endif

namespace TestCADRegion
{
    public class Class1
    {

        [CommandMethod("Test")]
        public void Test()
        {
            try
            {
                //var c1 = BaseTools.Select("选择多段线") as Curve;
                //TestJoin();
                //Offset();
                var line = new Line(new Point3d(14984.7850763984, 74983.6502236016, 0),
                    new Point3d(14984.7850763984, 74983.6526230616, 0));

                var prePoint = new Point3d(14984.7850763984, 74983.5914236016, 0);
                var nextPoint = new Point3d(14984.7850763984, 74983.7114236016, 0);
                line.StartPoint = prePoint;
                line.EndPoint = nextPoint;
                DataBaseTools.AddIn(line);
            }
            catch (System.Exception e)
            {
                BaseTools.WriteMessage(e.Message + e.StackTrace);
            }

            //GC.Collect();
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
 

        private static void Test1()
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
