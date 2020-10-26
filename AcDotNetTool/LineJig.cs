using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;

namespace AcDotNetTool
{
    public class LineJig : EntityJig
    {
        public Line Line { get; private set; }
        public LineJig() : base(new Line())
        {
            Line = new Line();
        }
        protected override SamplerStatus Sampler(JigPrompts prompts)
        {
            var jppintOp = new JigPromptPointOptions();
            jppintOp.UserInputControls = UserInputControls.Accept3dCoordinates | UserInputControls.NoZeroResponseAccepted | UserInputControls.NoNegativeResponseAccepted;
            jppintOp.UseBasePoint = false;
            jppintOp.DefaultValue = new Point3d();
            jppintOp.Message = "\n指定第一个点";
            var pPointRes = prompts.AcquirePoint(jppintOp);
            if (pPointRes.Status == PromptStatus.OK)
            {
                Line.StartPoint = pPointRes.Value;
                jppintOp.Message = "\n指定下一个点";
                pPointRes = prompts.AcquirePoint(jppintOp);
                if (pPointRes.Status == PromptStatus.OK)
                {
                    Line.EndPoint = pPointRes.Value;
                    return SamplerStatus.OK;
                }
            }
            return SamplerStatus.Cancel;
        }

        protected override bool Update()
        {
            ((Line)Entity).StartPoint = Line.StartPoint;
            ((Line)Entity).EndPoint = Line.EndPoint;
            return true;
        }
    }
}
