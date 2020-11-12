using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcDotNetTool;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

namespace TestCADRegion
{
    public class PLineBulge
    {
        [CommandMethod("PlBulge")]
        public void Test()
        {
            var pl = new Polyline();
            var index = 0;
            pl.AddVertexAt(index++, new Point2d(0, 0), 0, 0, 0);
            pl.AddVertexAt(index++, new Point2d(10, 0), 0.5, 0, 0);
            pl.AddVertexAt(index++, new Point2d(20, 0), 0, 0, 0);
            pl.AddVertexAt(index++, new Point2d(30, 0), -0.5, 0, 0);
            pl.AddVertexAt(index++, new Point2d(40, 0), 0, 0, 0);
            DataBaseTools.AddToModelSpace(pl);
        }

        [CommandMethod("PlBulge1")]
        public void Test1()
        {
            var pl = new Polyline();
            var index = 0;
            pl.AddVertexAt(index++, new Point2d(40, 20), 0, 0, 0);
            pl.AddVertexAt(index++, new Point2d(30, 20), -0.5, 0, 0);
            pl.AddVertexAt(index++, new Point2d(20, 20), 0, 0, 0);
            pl.AddVertexAt(index++, new Point2d(10, 20), 0.5, 0, 0);
            pl.AddVertexAt(index++, new Point2d(0, 20), 0, 0, 0);
           DataBaseTools.AddToModelSpace(pl);
        }

        [CommandMethod("PlBulge3")]
        public void Test3()
        {
            var pl = new Polyline();
            var index = 0;
            pl.AddVertexAt(index++, new Point2d(0, 40), 0, 0, 0);
            pl.AddVertexAt(index++, new Point2d(10, 40), -0.5, 0, 0);
            pl.AddVertexAt(index++, new Point2d(20, 40), 0, 0, 0);
            pl.AddVertexAt(index++, new Point2d(30, 40), 0.5, 0, 0);
            pl.AddVertexAt(index++, new Point2d(40, 40), 0, 0, 0);
           DataBaseTools.AddToModelSpace(pl);
        }

        [CommandMethod("PlBulge4")]
        public void Test4()
        {
            var pl = new Polyline();
            var index = 0;
            pl.AddVertexAt(index++, new Point2d(40, 60), 0, 0, 0);
            pl.AddVertexAt(index++, new Point2d(30, 60), 0.5, 0, 0);
            pl.AddVertexAt(index++, new Point2d(20, 60), 0, 0, 0);
            pl.AddVertexAt(index++, new Point2d(10, 60), -0.5, 0, 0);
            pl.AddVertexAt(index++, new Point2d(0, 60), 0, 0, 0);
           DataBaseTools.AddToModelSpace(pl);
        }


        [CommandMethod("PlBulge5")]
        public void Test5()
        {
            var pl = new Polyline();
            var index = 0;
            pl.AddVertexAt(index++, new Point2d(0, 0), 0.5, 0, 0);
            pl.AddVertexAt(index++, new Point2d(10, 0), 0.5, 0, 0);
            pl.AddVertexAt(index++, new Point2d(10, 10), 0.5, 0, 0);
            pl.AddVertexAt(index++, new Point2d(0, 10), 0.5, 0, 0);
            pl.AddVertexAt(index++, new Point2d(0, 0), 0, 0, 0);
           DataBaseTools.AddToModelSpace(pl);
        }

        [CommandMethod("PlBulge6")]
        public void Test6()
        {
            var pl = new Polyline();
            var index = 0;
            pl.AddVertexAt(index++, new Point2d(20, 0), 0.5, 0, 0);
            pl.AddVertexAt(index++, new Point2d(20, 10), 0.5, 0, 0);
            pl.AddVertexAt(index++, new Point2d(30, 10), 0.5, 0, 0);
            pl.AddVertexAt(index++, new Point2d(30, 0), 0.5, 0, 0);
            pl.AddVertexAt(index++, new Point2d(20, 0), 0, 0, 0);
           DataBaseTools.AddToModelSpace(pl);
        }

        [CommandMethod("PlBulge7")]
        public void Test7()
        {
            var pl = new Polyline();
            var index = 0;
            pl.AddVertexAt(index++, new Point2d(40, 0), -0.5, 0, 0);
            pl.AddVertexAt(index++, new Point2d(50, 0), -0.5, 0, 0);
            pl.AddVertexAt(index++, new Point2d(50, 10), -0.5, 0, 0);
            pl.AddVertexAt(index++, new Point2d(40, 10), -0.5, 0, 0);
            pl.AddVertexAt(index++, new Point2d(40, 0), 0, 0, 0);
           DataBaseTools.AddToModelSpace(pl);
        }
        [CommandMethod("PlBulge8")]
        public void Test8()
        {
            var pl = new Polyline();
            var index = 0;
            pl.AddVertexAt(index++, new Point2d(60, 0), -0.5, 0, 0);
            pl.AddVertexAt(index++, new Point2d(60, 10), -0.5, 0, 0);
            pl.AddVertexAt(index++, new Point2d(70, 10), -0.5, 0, 0);
            pl.AddVertexAt(index++, new Point2d(70, 0), -0.5, 0, 0);
            pl.AddVertexAt(index++, new Point2d(60, 0), 0, 0, 0);
           DataBaseTools.AddToModelSpace(pl);
        }
        [CommandMethod("Cross")]
        public void Cross()
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            var p1 = BaseTools.SelectPoint("p1");
            Vector3d vec1 = new Vector3d(p1.X, p1.Y, 0);
            var p2 = BaseTools.SelectPoint("p2");
            Vector3d vec2 = new Vector3d(p2.X, p2.Y, 0);
            var v3 = vec1.CrossProduct(vec2);
            ed.WriteMessage($"{v3.X},{v3.Y},{v3.Z}");
        }
    }
}
