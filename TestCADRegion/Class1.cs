using System;
using System.Linq;
using AcDotNetTool;
using Autodesk.AutoCAD.Runtime;

namespace TestCADRegion
{
    public class Class1
    {

        [CommandMethod("Test")]
        public void Test()
        {
            var ent1s = BaseTools.GetSelection();

            var db = DataBaseTools.ReadFile($@"C:\Users\Administrator\Desktop\房产分层分户图.dwg");
            var ent2s = DataBaseTools.GetAllEntitiesInModel(db);

            var ents = ent1s.Concat(ent2s);

            DxfTools.DxfExport(ents, $@"C:\Users\Administrator\Desktop\test-{DateTime.Now.ToString("HH-mm-ss")}.dxf");
        }
    }
}
