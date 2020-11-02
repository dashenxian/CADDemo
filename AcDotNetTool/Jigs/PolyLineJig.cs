using System;
using System.Collections.Generic;
using System.Linq;
using AcDotNetTool.Extensions;
using ZwSoft.ZwCAD.DatabaseServices;
using ZwSoft.ZwCAD.EditorInput;
using ZwSoft.ZwCAD.Geometry;

namespace AcDotNetTool.Jigs
{
    public class PolyLineJig : EntityJig
    {
        public int NumberOfVertices => PolyLine.NumberOfVertices;
        public Polyline PolyLine { get; private set; }
        /// <summary>
        /// 采样临时点
        /// </summary>
        private Point2d TemPoint2d { get; set; }
        public PolyLineJig() : base(new Polyline())
        {
            PolyLine = new Polyline();
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

            if (NumberOfVertices > 2)
            {
                jppintOp.Keywords.Add("C", "C", "闭合(C)");
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
            var basePl = ((Polyline)Entity);
            if (PolyLine.NumberOfVertices == 0)
            {
                while (basePl.NumberOfVertices > 1)
                {
                    basePl.RemoveVertexAt(basePl.NumberOfVertices - 1);
                }
                basePl.AddOrSetVertexAt(0, new Point2d(TemPoint2d.X, TemPoint2d.Y), 0, 0, 0);
            }
            else
            {
                var start = this.PolyLine.GetPoint2dAt(PolyLine.NumberOfVertices - 1);
                basePl.AddOrSetVertexAt(0, new Point2d(start.X, start.Y), 0, 0, 0);
                basePl.AddOrSetVertexAt(1, new Point2d(TemPoint2d.X, TemPoint2d.Y), 0, 0, 0);
            }

            return true;
        }
        /// <summary>
        /// 添加顶点
        /// </summary>
        private void AddPoint()
        {
            PolyLine.AddOrSetVertexAt(PolyLine.NumberOfVertices, new Point2d(TemPoint2d.X, TemPoint2d.Y), 0, 0, 0);
            if (NumberOfVertices == 1 && PolyLine.ObjectId == new ObjectId())
            {
                PolyLine.AddToModelSpace(DataBaseTools.DocumentDatabase());
            }
        }
        /// <summary>
        /// 移除最后一个顶点
        /// </summary>
        private void RemovePoint()
        {
            PolyLine.TryRemoveVertexAt(NumberOfVertices - 1);
        }
        /// <summary>
        /// 删除
        /// </summary>
        private void DeleteFromDatabase()
        {
            if (PolyLine.ObjectId != new ObjectId())
            {
                DataBaseTools.TryRemove(PolyLine);
            }
        }

        private void U()
        {
            if (NumberOfVertices > 0)
            {
                RemovePoint();
            }
            else
            {
                DeleteFromDatabase();
            }
        }

        private void C()
        {
            PolyLine.TryClose(); 
        }

        public static Polyline DrawPolyLine()
        {
            var ed = DataBaseTools.DocumentEditor();
            var jig = new PolyLineJig();
            do
            {
                var res = (PromptPointResult)ed.Drag(jig);
                if (res.Status == PromptStatus.OK)
                {
                    jig.AddPoint();
                }
                else if (res.Status == PromptStatus.Keyword)
                {
                    var isBreak = false;
                    switch (res.StringResult)
                    {
                        case "U":
                            jig.U();
                            break;
                        case "C":
                            jig.C();
                            isBreak = true;
                            break;
                        default:
                            isBreak = true;
                            break;
                    }
                    if (isBreak)
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
            } while (true);
            return jig.PolyLine;
        }
    }
}
