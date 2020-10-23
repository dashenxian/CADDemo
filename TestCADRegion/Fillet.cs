using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Math;
using AcAp = Autodesk.AutoCAD.ApplicationServices.Application;

namespace TestCADRegion
{
    /// <summary>
    /// 倒角
    /// </summary>
    public class Fillet
    {
        double radius = 0.0;

        public void Test()
        {
            var doc = AcAp.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            var ed = doc.Editor;

            var pdo = new PromptDistanceOptions("\nEnter the fillet radius: ");
            pdo.DefaultValue = radius;
            pdo.AllowNegative = false;
            pdo.AllowNone = true;
            pdo.UseDefaultValue = true;
            var pdr = ed.GetDistance(pdo);
            if (pdr.Status != PromptStatus.OK) return;
            radius = pdr.Value;

            var peo = new PromptEntityOptions("\nSelect the first line: ");
            peo.SetRejectMessage("\nSelected object is not a line.");
            peo.AddAllowedClass(typeof(Line), true);
            var per = ed.GetEntity(peo);
            if (per.Status != PromptStatus.OK) return;
            var id1 = per.ObjectId;
            var pp1 = ed.Snap("_near", per.PickedPoint).TransformBy(ed.CurrentUserCoordinateSystem);

            peo.Message = "\nSelect the second line: ";
            per = ed.GetEntity(peo);
            if (per.Status != PromptStatus.OK) return;
            var id2 = per.ObjectId;
            var pp2 = ed.Snap("_near", per.PickedPoint).TransformBy(ed.CurrentUserCoordinateSystem);

            using (var tr = db.TransactionManager.StartTransaction())
            {
                // open the lines
                var line1 = (Line)tr.GetObject(id1, OpenMode.ForRead);
                var line2 = (Line)tr.GetObject(id2, OpenMode.ForRead);

                // get the intersection
                var pts = new Point3dCollection();
                line1.IntersectWith(line2, Intersect.ExtendBoth, pts, IntPtr.Zero, IntPtr.Zero);
                if (pts.Count != 1)
                {
                    ed.WriteMessage("\nSelected lines do not intersect.");
                    return;
                }
                var inters = pts[0];

                var sp1 = line1.StartPoint;
                var ep1 = line1.EndPoint;
                var sp2 = line2.StartPoint;
                var ep2 = line2.EndPoint;

                // get the farest points from intersection on the picked side of both lines
                Func<Point3d, Point3d, Point3d, Point3d> getFarest = (sp, ep, pp) =>
                {
                    var dir = inters.GetVectorTo(pp);
                    if (!inters.GetVectorTo(sp).IsCodirectionalTo(dir)) return ep;
                    if (!inters.GetVectorTo(ep).IsCodirectionalTo(dir)) return sp;
                    if (inters.DistanceTo(sp) < inters.DistanceTo(ep)) return ep;
                    return sp;
                };
                var fp1 = getFarest(sp1, ep1, pp1);
                var fp2 = getFarest(sp2, ep2, pp2);

                // if radius == 0, just trim/extend the lines
                if (radius == 0.0)
                {
                    line1.UpgradeOpen();
                    if (sp1.IsEqualTo(line1.StartPoint))
                        line1.EndPoint = inters;
                    else
                        line1.StartPoint = inters;

                    line2.UpgradeOpen();
                    if (sp2.IsEqualTo(line2.StartPoint))
                        line2.EndPoint = inters;
                    else
                        line2.StartPoint = inters;
                }

                // compute the fillet
                else
                {
                    // 2D work in the plane defined by the two lines
                    var normal = (fp1 - inters).CrossProduct(fp2 - inters);
                    var plane = new Plane(inters, normal);
                    var v1 = fp1.Convert2d(plane).GetAsVector();
                    var v2 = fp2.Convert2d(plane).GetAsVector();
                    double angle = v1.GetAngleTo(v2) / 2.0;
                    var dist = radius / Tan(angle);
                    if (v1.Length <= dist || v2.Length <= dist)
                    {
                        ed.WriteMessage("\nRadius too large to fillet the selected lines.");
                        return;
                    }

                    double hyp = radius / Sin(angle);
                    var center = new Point2d(hyp * Cos(angle + v1.Angle), hyp * Sin(angle + v1.Angle));
                    var p1 = Point2d.Origin + v1.GetNormal() * dist;
                    var p2 = Point2d.Origin + v2.GetNormal() * dist;
                    var a1 = center.GetVectorTo(p1).Angle;
                    var a2 = center.GetVectorTo(p2).Angle;

                    // back to 3D
                    Func<Point2d, Point3d> convert3d = (pt) =>
                        new Point3d(pt.X, pt.Y, 0.0).TransformBy(Matrix3d.PlaneToWorld(plane));
                    var arc = new Arc(new Point3d(center.X, center.Y, 0.0), radius, a2, a1);
                    arc.TransformBy(Matrix3d.PlaneToWorld(plane));
                    var curSpace = (BlockTableRecord)tr.GetObject(db.CurrentSpaceId, OpenMode.ForWrite);
                    curSpace.AppendEntity(arc);
                    tr.AddNewlyCreatedDBObject(arc, true);

                    line1.UpgradeOpen();
                    line1.StartPoint = convert3d(p1);
                    line1.EndPoint = fp1;
                    line2.UpgradeOpen();
                    line2.StartPoint = fp2;
                    line2.EndPoint = convert3d(p2);
                }

                tr.Commit();
            }
        }

    }
}
