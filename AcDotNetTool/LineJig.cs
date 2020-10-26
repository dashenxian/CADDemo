using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using System.Linq;

namespace AcDotNetTool
{
    public class LineJig : EntityJig
    {
        public Line Line { get; private set; }
        private int count;
        public LineJig() : base(new Line())
        {
            Line = base.Entity as Line;
            count = 0;
        }
        public LineJig(Point3d startPoint) : base(new Line())
        {
            Line = base.Entity as Line;
            Line.StartPoint = startPoint;
            count = 1;
        }
        protected override SamplerStatus Sampler(JigPrompts prompts)
        {
            var jppintOp = new JigPromptPointOptions();
            jppintOp.UserInputControls =
                UserInputControls.Accept3dCoordinates
                | UserInputControls.NoZeroResponseAccepted
                | UserInputControls.NoNegativeResponseAccepted
                | UserInputControls.NullResponseAccepted
                ;
            //jppintOp.UseBasePoint = false;
            //jppintOp.DefaultValue = new Point3d();
            jppintOp.Keywords.Add("U", "U", "放弃(U)");
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
            //((Line)Entity).StartPoint = Line.StartPoint;
            //((Line)Entity).EndPoint = Line.EndPoint;
            return true;
        }
        /// <summary>
        /// 绘制直线
        /// </summary>
        /// <returns></returns>
        public static (PromptResult result, Line line) DrawLine()
        {
            var ed = DataBaseTools.DocumentEditor();
            var lineJig = new LineJig();
            var res = ed.Drag(lineJig);
            Line line = null;
            if (res.Status == PromptStatus.OK)
            {
                lineJig.SetCounter(1);
                res = ed.Drag(lineJig);
                if (res.Status == PromptStatus.OK)
                {
                    lineJig.Line.AddToModelSpace(DataBaseTools.DocumentDatabase());
                    line = lineJig.Line;
                }
            }
            return (res, line);
        }
        /// <summary>
        /// 绘制直线
        /// </summary>
        /// <param name="startPoint3d">指定起点</param>
        /// <returns></returns>
        public static (PromptResult result, Line line) DrawLine(Point3d startPoint3d)
        {
            var ed = DataBaseTools.DocumentEditor();
            var lineJig = new LineJig(startPoint3d);
            //lineJig.Line.LineWeight = LineWeight.LineWeight200;
            //lineJig.Line.Color = Color.FromColor();
            var res = ed.Drag(lineJig);
            Line line = null;
            if (res.Status == PromptStatus.OK)
            {
                lineJig.Line.AddToModelSpace(DataBaseTools.DocumentDatabase());
                line = lineJig.Line;
            }
            return (res, line);
        }
        /// <summary>
        /// 连续绘制直线
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<Line> DrawLineContinue()
        {
            var lines = new List<Line>();
            var res = DrawLine();
            if (res.result.Status != PromptStatus.OK)
            {
                return lines;
            }
            lines.Add(res.line);
            var nextStartPoint = res.line.EndPoint;
            while (true)
            {
                res = DrawLine(nextStartPoint);
                if (res.result.Status == PromptStatus.OK)
                {
                    lines.Add(res.line);
                    nextStartPoint = res.line.EndPoint;
                }
                else if (res.result.Status == PromptStatus.Keyword && res.result.StringResult == "U")
                {
                    var last = lines.LastOrDefault();
                    if (last != null)
                    {
                        lines.Remove(last);
                        DataBaseTools.Remove(last);
                        nextStartPoint = last.StartPoint;
                    }
                    else
                    {
                        break;
                    }
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
