using System;
using System.Linq;
using AcDotNetTool;
using AcDotNetTool.Jigs;
using ZwSoft.ZwCAD.ApplicationServices;
using ZwSoft.ZwCAD.DatabaseServices;
using ZwSoft.ZwCAD.EditorInput;
using ZwSoft.ZwCAD.Geometry;
using ZwSoft.ZwCAD.Runtime;
using CADModel;
using Point2d = ZwSoft.ZwCAD.Geometry.Point2d;
using Point3d = ZwSoft.ZwCAD.Geometry.Point3d;

namespace TestCADRegion
{
    public class Class1
    {

        [CommandMethod("Test")]
        public void Test()
        {
            LayoutTest();
        }

        private static void LayoutTest()
        {
            string layoutName = "布局1";
            LayoutManager acLayoutMgr = LayoutManager.Current;
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            DocumentLock acLock = acDoc.LockDocument();
            using (Transaction acTrans = acDoc.TransactionManager.StartTransaction())
            {
                acLayoutMgr.CurrentLayout = layoutName;

                ObjectId acLayoutId = acLayoutMgr.GetLayoutId(layoutName);
                //获取布局
                var layout = acTrans.GetObject(acLayoutId, OpenMode.ForWrite) as Layout;
                BlockTableRecord acBlkTblRe0 = acTrans.GetObject(layout.BlockTableRecordId, OpenMode.ForWrite) as BlockTableRecord;
                foreach (ObjectId entOid in acBlkTblRe0)
                {
                    Entity ent = acTrans.GetObject(entOid, OpenMode.ForWrite) as Entity;
                    if (ent != null)
                        ent.Erase();
                }
                acTrans.Commit();
            }
            acLock.Dispose();
        }
    }
}
