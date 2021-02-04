using System;
using System.Linq;
using AcDotNetTool;

#if ZWCAD
using ZwSoft.ZwCAD.Runtime;
#elif AutoCAD
using Autodesk.AutoCAD.Runtime;
#endif

namespace TestCADRegion
{
    public class Class1
    {

        [CommandMethod("Test")]
        public void Test()
        {
            BaseTools.WriteMessage("Test1");
            var ent1s = BaseTools.GetSelection();
        }
    }
}
