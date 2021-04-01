using System;
using System.Linq;
using AcDotNetTool;


//#if ZWCAD
using ZwSoft.ZwCAD.Runtime;
using  ZwSoft.ZwCAD.DatabaseServices;
//#elif AutoCAD
//using Autodesk.AutoCAD.Runtime;
//using Autodesk.AutoCAD.DatabaseServices;
//#endif

namespace TestCADRegion
{
    public class Class1
    {

        [CommandMethod("Test")]
        public void Test()
        {
            
            var ent1 = BaseTools.Select("选择直线") as Line;
            var ent2 = BaseTools.Select("选择直线") as Line;
            if (ent1==null||ent2==null)
            {
                BaseTools.WriteMessage("从新选择");
                return;
            }

            BaseTools.WriteMessage(GeometryTools.Parallel(ent1, ent2)+"");
        }
    }
}
