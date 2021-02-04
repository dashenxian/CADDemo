using System;
using System.Collections.Generic;
using System.Linq;
using AcDotNetTool.Extensions;

#if ZWCAD
using ZwSoft.ZwCAD.DatabaseServices;
using ZwSoft.ZwCAD.EditorInput;
using ZwSoft.ZwCAD.Geometry;
#elif AutoCAD
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
#endif


namespace AcDotNetTool.Jigs
{
    /// <summary>
    /// 圆弧
    /// </summary>
    public class ArcJig : EntityJig
    {
        public Polyline Arc { get; set; }
        private IList<Point2d> Points { get; set; }
        private Point2d TempPoint { get; set; }
        public ArcJig() : base(new Polyline())
        {
            Arc = base.Entity as Polyline;
            Points = new List<Point2d>(3);
        }
        public ArcJig(Point2d firstPoint) : this()
        {
            Points.Add(firstPoint);
        }

        protected override SamplerStatus Sampler(JigPrompts prompts)
        {
            var jppintOp = new JigPromptPointOptions();
            jppintOp.UserInputControls =
                UserInputControls.NoZeroResponseAccepted
                | UserInputControls.NoNegativeResponseAccepted
                | UserInputControls.NullResponseAccepted
                ;
            if (Arc.NumberOfVertices == 0)
            {
                jppintOp.Message = "\n指定圆弧的起点";
            }
            else if (Arc.NumberOfVertices == 1)
            {
                jppintOp.Message = "\n指定圆弧的第二个点";
            }
            else
            {
                jppintOp.Message = "\n指定圆弧的端点";
            }
            PromptPointResult pntres = prompts.AcquirePoint(jppintOp);
            if (pntres.Status == PromptStatus.OK)
            {
                TempPoint = pntres.Value.ToPoint2d();
                return SamplerStatus.OK;
            }

            return SamplerStatus.Cancel;
        }

        protected override bool Update()
        {
            while (Arc.NumberOfVertices > 0)
            {
                Arc.TryRemoveVertexAt(Arc.NumberOfVertices - 1);
            }
            if (Points.Count < 3)
            {
                for (int i = 0; i < Points.Count; i++)
                {
                    Arc.AddOrSetVertexAt(i, Points[i], 0, 0, 0);
                }
            }
            else
            {
                var bulge = GetBulge(Points[0], Points[1], Points[2]);
                Arc.AddOrSetVertexAt(0, Points[0], bulge, 0, 0);
                Arc.AddOrSetVertexAt(1, Points[1], 0, 0, 0);
                Arc.AddOrSetVertexAt(2, Points[2], 0, 0, 0);
            }
            return true;
        }
        private double GetBulge(Point2d p1, Point2d p2, Point2d p3)
        {
            //计算圆心
            double a, b, c, d, e, f;
            a = 2 * (p2.X - p1.X);
            b = 2 * (p2.Y - p1.Y);
            c = p2.X * p2.X + p2.Y * p2.Y - p1.X * p1.X - p1.Y * p1.Y;
            d = 2 * (p3.X - p2.X);
            e = 2 * (p3.Y - p2.Y);
            f = p3.X * p3.X + p3.Y * p3.Y - p2.X * p2.X - p2.Y * p2.Y;
            double centerX = (b * f - e * c) / (b * d - e * a);
            double centerY = (d * c - a * f) / (b * d - e * a);

            Polyline pl = new Polyline();
            pl.AddVertexAt(0, new Point2d(p1.X, p1.Y), 0, 0, 0);
            pl.AddVertexAt(1, new Point2d(p2.X, p2.Y), 0, 0, 0);
            pl.AddVertexAt(2, new Point2d(p3.X, p3.Y), 0, 0, 0);
            pl.Closed = true;

            //方向判断
            bool isCloseCise = true; //CADDrawingMgrHelper.IsCloseCise(pl);

            //优劣弧判断
            Point3d xianCenter = new Point3d((p1.X + p3.X) / 2, (p1.Y + p3.Y) / 2, 0);
            pl.AddVertexAt(3, new Point2d(centerX, centerY), 0, 0, 0);
            bool isBadArc = true; //CADDrawingMgrHelper.PointInBound(xianCenter, pl, true);

            Vector2d v1 = new Vector2d(p1.X - centerX, p1.Y - centerY);
            Vector2d v2 = new Vector2d(p3.X - centerX, p3.Y - centerY);

            double angle = v1.GetAngleTo(v2);
            if (!isBadArc)
                angle = Math.PI * 2 - angle;

            double kBulge = Math.Tan(angle / 4);
            if (isCloseCise)
                kBulge = -kBulge;

            return kBulge;
        }

        private void AddPoint()
        {
            if (Points.Count < 3)
            {
                Points.Add(TempPoint);
            }
        }

        private void RemovePoint()
        {
            if (Points.Any())
            {
                Points.RemoveAt(Points.Count - 1);
            }
        }
        public static Polyline DrawArc()
        {
            var ed = DataBaseTools.DocumentEditor();
            var jig = new ArcJig();
            do
            {
                ed.Drag(jig);
                var res = (PromptPointResult)ed.Drag(jig);
                if (res.Status == PromptStatus.OK)
                {
                    jig.AddPoint();
                }
            } while (jig.Points.Count < 3);
            return jig.Arc;
        }
    }
}
