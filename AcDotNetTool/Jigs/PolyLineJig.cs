using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;

namespace AcDotNetTool.Jigs
{
    public class PolyLineJig : EntityJig
    {
        public int NumberOfVertices => Vertices.Count;
        private IList<Point2d> Vertices { get; set; }
        public Polyline PolyLine { get; private set; }
        /// <summary>
        /// 采样临时点
        /// </summary>
        private Point2d TemPoint2d { get; set; }
        public PolyLineJig() : base(new Polyline())
        {
            PolyLine = new Polyline();
            Vertices = new List<Point2d>();
        }
        protected override SamplerStatus Sampler(JigPrompts prompts)
        {
            var jppintOp = new JigPromptPointOptions();
            jppintOp.UserInputControls =
                UserInputControls.NoZeroResponseAccepted
                | UserInputControls.NoNegativeResponseAccepted
                | UserInputControls.NullResponseAccepted
                ;
            //jppintOp.UseBasePoint = false;
            //jppintOp.DefaultValue = new Point3d();
            jppintOp.Keywords.Add("U", "U", "放弃(U)");
            if (NumberOfVertices == 0)
            {
                jppintOp.Message = "\n选择起点";
            }
            else
            {
                jppintOp.Message = "\n选择下一点";
            }
            PromptPointResult pntres = prompts.AcquirePoint(jppintOp);
            PromptStatus ss = pntres.Status;
            if (pntres.Status == PromptStatus.OK)
            {
                TemPoint2d = pntres.Value.ToPoint2d();
                return SamplerStatus.OK;
            }
            return SamplerStatus.Cancel;
        }

        protected override bool Update()
        {
            ((Polyline)Entity).Reset(true, 0);
            PolyLine.Reset(true, 0);
            for (int i = 0; i < Vertices.Count; i++)
            {
                ((Polyline)Entity).AddVertexAt(i, Vertices[i], 0, 0, 0);
                PolyLine.AddVertexAt(i, Vertices[i], 0, 0, 0);
            }
            ((Polyline)base.Entity).AddVertexAt(Vertices.Count, TemPoint2d, 0, 0, 0);

            return true;
        }

        public static Polyline DrawPolyLine()
        {
            var ed = DataBaseTools.DocumentEditor();
            var jig = new PolyLineJig();
            //jig.Entity.AddToModelSpace(DataBaseTools.DocumentDatabase());
            do
            {
                var res = (PromptPointResult)ed.Drag(jig);
                if (res.Status == PromptStatus.OK)
                {
                    jig.Vertices.Add(res.Value.ToPoint2d());
                }
                else
                {
                    break;
                }
            } while (true);

            if (jig.NumberOfVertices > 1)
            {
                //DataBaseTools.Remove(jig.Entity);
                jig.PolyLine.AddToModelSpace(DataBaseTools.DocumentDatabase());
                return jig.PolyLine;
            }
            return null;
        }
    }
}
