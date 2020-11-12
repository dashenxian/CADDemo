﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcDotNetTool;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;

namespace TestCADRegion
{
    public class 曲线上中间点距离
    {
        [CommandMethod("GetDist")]
        public void GetDist()
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            var pl = BaseTools.Select("选择曲线") as Polyline;
            if (pl == null)
            {
                return;
            }
            //var point1 = Class1.SelectPoint("选择第1个点");
            //var point2 = Class1.SelectPoint("选择第2个点");
            //if (point1 == null || point2 == null)
            //{
            //    return;
            //}
            //var dist = pl.GetDistAtPoint(point2.Point2dToPoint3d()) - pl.GetDistAtPoint(point1.Point2dToPoint3d());
            for (int i = 0; i < pl.NumberOfVertices; i++)
            {
                var dist = pl.GetDistAtPoint(pl.GetPoint3dAt(i));
                ed.WriteMessage("距离为:" + dist + "\n");
            }

        }
    }
}
