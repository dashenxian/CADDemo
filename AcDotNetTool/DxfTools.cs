using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;

namespace AcDotNetTool
{
    public static class DxfTools
    {
        /// <summary>
        /// 导出dxf文件
        /// </summary>
        /// <param name="entities">要导出的对象，对象必须在Database中</param>
        /// <param name="exportFileName">保存的dxf文件名</param>
        public static void DxfExport(IEnumerable<Entity> entities, string exportFileName)
        {
            DxfExport(entities, 11, exportFileName);
        }
        /// <summary>
        /// 导出dxf文件
        /// </summary>
        /// <param name="entities">要导出的对象，对象必须在Database中</param>
        /// <param name="precision">精度位数，从0到16</param>
        /// <param name="exportFileName">保存的dxf文件名</param>
        public static void DxfExport(IEnumerable<Entity> entities, int precision, string exportFileName)
        {
            if (!exportFileName.EndsWith(".dxf"))
            {
                exportFileName += ".dxf";
            }
            var objIds = new ObjectIdCollection();
            foreach (var ent in entities)
            {
                objIds.Add(ent.ObjectId);
            }

            DxfExport(objIds, precision, exportFileName);
        }
        /// <summary>
        /// 导出dxf文件
        /// </summary>
        /// <param name="entities">要导出的对象Id集合</param>
        /// <param name="exportFileName">保存的dxf文件名</param>
        public static void DxfExport(ObjectIdCollection objIds, string exportFileName)
        {
            DxfExport(objIds, 11, exportFileName);
        }
        /// <summary>
        /// 导出dxf文件
        /// </summary>
        /// <param name="objIds">要导出的对象Id集合</param>
        /// <param name="precision">精度位数，从0到16</param>
        /// <param name="exportFileName">保存的dxf文件名</param>
        public static void DxfExport(ObjectIdCollection objIds, int precision, string exportFileName)
        {
            var db = EditEntityTools.WBClone(objIds);
            db.DxfOut(exportFileName, 11, DwgVersion.Current);
        }
    }
}
