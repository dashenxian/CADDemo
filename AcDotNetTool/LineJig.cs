using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;

namespace AcDotNetTool
{
    public class LineJig : EntityJig
    {
        public Line Line { get; private set; }
        private int count;
        public LineJig() : base(new Line())
        {
            Line = new Line();
            count = 0;
        }
        public LineJig(Point3d startPoint) : base(new Line())
        {
            Line = new Line();
            Line.StartPoint = startPoint;
            count = 1;
        }
        protected override SamplerStatus Sampler(JigPrompts prompts)
        {
            var jppintOp = new JigPromptPointOptions();
            jppintOp.UserInputControls = UserInputControls.Accept3dCoordinates | UserInputControls.NoZeroResponseAccepted | UserInputControls.NoNegativeResponseAccepted;
            jppintOp.UseBasePoint = false;
            jppintOp.DefaultValue = new Point3d();
            if (count == 0)
            {
                jppintOp.Message = "\n选择起点";
                PromptPointResult pntres = prompts.AcquirePoint(jppintOp);
                PromptStatus ss = pntres.Status;
                if (pntres.Status == PromptStatus.OK)
                {
                    Line.StartPoint = pntres.Value;
                    Line.EndPoint = pntres.Value;
                    return SamplerStatus.OK;
                }
            }
            if (count == 1)
            {
                jppintOp.Message = "\n选择下一点";
                PromptPointResult pntres = prompts.AcquirePoint(jppintOp);
                if (pntres.Status == PromptStatus.OK)
                {
                    Line.EndPoint = pntres.Value;
                    return SamplerStatus.OK;
                }
            }
            return SamplerStatus.Cancel;
        }

        public void SetCounter(int i)
        {
            count = i;
        }

        protected override bool Update()
        {
            ((Line)Entity).StartPoint = Line.StartPoint;
            ((Line)Entity).EndPoint = Line.EndPoint;
            return true;
        }

        public static Line DrawLine()
        {
            var ed = DataBaseTools.DocumentEditor();
            var lineJig = new LineJig();
            var res = ed.Drag(lineJig);
            if (res.Status == PromptStatus.OK)
            {
                lineJig.SetCounter(1);
                res = ed.Drag(lineJig);
                if (res.Status == PromptStatus.OK)
                {
                    lineJig.Line.AddToModelSpace(DataBaseTools.DocumentDatabase());
                    return lineJig.Line;
                }
            }
            return null;
        }

        public static IEnumerable<Line> DrawLineContinue()
        {
            var lines = new List<Line>();
            var line = DrawLine();
            while (line != null)
            {
                lines.Add(line);
                var ed = DataBaseTools.DocumentEditor();
                var lineJig = new LineJig(line.EndPoint);
                var res = ed.Drag(lineJig);
                if (res.Status == PromptStatus.OK)
                {
                    lineJig.Line.AddToModelSpace(DataBaseTools.DocumentDatabase());
                    line = lineJig.Line;
                }
                else
                {
                    break;
                }
            }

            return lines;
        }
    }
}
