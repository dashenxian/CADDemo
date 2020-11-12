using System;
using AcDotNetTool;
using Autodesk.AutoCAD.Runtime;

namespace TestCADRegion
{
    public class Class1
    {

        [CommandMethod("Test")]
        public void Test()
        {
            var ents = BaseTools.GetSelection();

            DxfTools.DxfExport(ents, $@"C:\Users\Administrator\Desktop\test-{DateTime.Now.ToString("HH-mm-ss")}.dxf");
        }
    }
}
