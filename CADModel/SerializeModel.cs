using System;
using System.Collections.Generic;
using System.Text;

namespace CADModel
{
    public class SerializeModel
    {
        /// <summary>
        /// CAD对象类型
        /// </summary>
        public CADType CadType { get; set; }
        public string CadTypeStr => CadType.ToString();
        /// <summary>
        /// 3d点序列
        /// </summary>
        public IList<Point3d> Point3ds { get; set; }
        /// <summary>
        /// 2d点序列
        /// </summary>
        public IList<Point2d> Point2ds { get; set; }
    }

    public enum CADType
    {
        Line,
        PolyLine,
    }
}
