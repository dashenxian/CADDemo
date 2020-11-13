using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace AcDotNetTool
{
    /// <summary>
    /// 编辑图形
    /// </summary>
    public static class EditEntityTools
    {
        /// <summary>
        /// 指定基点与目标点移动实体
        /// </summary>
        /// <param name="ent">实体对象</param>
        /// <param name="BasePt">基点</param>
        /// <param name="TargetPt">目标点</param>
        public static void Move(this Entity ent, Point3d basePt, Point3d targetPt)
        {
            Vector3d vec = targetPt - basePt;
            Matrix3d mt = Matrix3d.Displacement(vec);
            ent.TransformBy(mt);
        }
        /// <summary>
        /// 指定基点与目标点复制实体
        /// </summary>
        /// <param name="ent">实体对象</param>
        /// <param name="sourcePt">基点</param>
        /// <param name="targetPt">目标点</param>
        /// <returns>复制的实体对象</returns>
        public static Entity CopyTo(this Entity ent, Point3d sourcePt, Point3d targetPt)
        {
            Matrix3d mt = Matrix3d.Displacement(targetPt - sourcePt);
            Entity entCopy = ent.GetTransformedCopy(mt);
            return entCopy;
        }
        /// <summary>
        /// 指定基点与旋转角度旋转实体
        /// </summary>
        /// <param name="ent">实体对象</param>
        /// <param name="basePt">基点</param>
        /// <param name="angle">旋转角度</param>
        public static void Rotate(this Entity ent, Point2d basePt, double angle)
        {
            Matrix3d mt = Matrix3d.Rotation(angle, Vector3d.ZAxis, basePt.ToPoint3d());
            ent.TransformBy(mt);
        }
        /// <summary>
        /// 指定基点与旋转角度旋转实体
        /// </summary>
        /// <param name="ent">实体对象</param>
        /// <param name="basePt">基点</param>
        /// <param name="angle">旋转角度</param>
        /// <param name="Axis">旋转轴(XY平面内旋转则设为Vector3d.ZAxis)</param>
        public static void Rotate(this Entity ent, Point3d basePt, double angle, Vector3d Axis)
        {
            Matrix3d mt = Matrix3d.Rotation(angle, Axis, basePt);
            ent.TransformBy(mt);
        }
        /// <summary>
        /// 指定基点与比例缩放实体
        /// </summary>
        /// <param name="ent">实体对象</param>
        /// <param name="basePt">基点</param>
        /// <param name="scaleFactor">缩放比例</param>
        public static void Scale(this Entity ent, Point3d basePt, double scaleFactor)
        {
            Matrix3d mt = Matrix3d.Scaling(scaleFactor, basePt);
            ent.TransformBy(mt);
        }
        /// <summary>
        /// 按照参照点镜像实体
        /// </summary>
        /// <param name="ent">实体对象</param>
        /// <param name="mirrorPt1">镜像点1</param>
        /// <param name="mirrorPt2">镜像点2</param>
        public static void Mirror(this Entity ent, Point3d mirrorPt1, Point3d mirrorPt2)
        {
            Line3d mirrorLine = new Line3d(mirrorPt1, mirrorPt2);
            Matrix3d mt = Matrix3d.Mirroring(mirrorLine);
            ent.TransformBy(mt);
        }
        /// <summary>
        /// 按照指定直线镜像实体
        /// </summary>
        /// <param name="ent"></param>
        /// <param name="line"></param>
        public static void Mirror(this Entity ent, Line line)
        {
            Line3d mirrorLine = new Line3d(line.StartPoint, line.EndPoint);
            Matrix3d mt = Matrix3d.Mirroring(mirrorLine);
            ent.TransformBy(mt);
        }

        /// <summary>
        /// 获得指定偏移距的偏移对象
        /// </summary>
        /// <param name="cur">待偏移曲线</param>
        /// <param name="dis">便宜距离</param>
        /// <returns>偏移后对象集合</returns>
        public static List<Curve> Offset(this Curve cur, double dis)
        {
            List<Curve> Cl = new List<Curve>();
            try
            {
                DBObjectCollection offsetCur = cur.GetOffsetCurves(dis);
                foreach (var i in offsetCur)
                {
                    Cl.Add((Curve)i);
                }
            }
            catch
            { }
            return Cl;
        }

        /// <summary>
        /// 偏移对象到指定点
        /// </summary>
        /// <param name="cur"></param>
        /// <param name="OffsetPoint">偏移方向</param>
        /// <param name="dis">偏移距离绝对值</param>
        /// <returns></returns>
        public static List<Curve> Offset(this Curve cur, double disAbs, Point3d OffsetPoint)
        {
            List<Curve> Cl = new List<Curve>();
            var closestPoint = cur.GetClosestPointTo(OffsetPoint, false);
            //曲线上给定一点的一阶导数(斜率)
            var d1 = cur.GetFirstDerivative(closestPoint);
            //最近的点到偏移点的向量
            var v1 = closestPoint.GetVectorTo(OffsetPoint);
            //向量积垂直于d1、v1构成的平面，如果向量积的z轴数据为正，则构成坐标系，偏移方向也为正，否则为负
            var direction = d1.X * v1.Y - d1.Y * v1.X > 0 ? -1 : 1;
            if (cur is Line)//直线的方向和多段线相反
            {
                direction = 0 - direction;
            }
            var dis = disAbs * direction;
            try
            {
                DBObjectCollection offsetCur = cur.GetOffsetCurves(dis);
                foreach (var i in offsetCur)
                {
                    Cl.Add((Curve)i);
                }
            }
            catch
            { }
            return Cl;
        }
        /// <summary>
        /// 指定行数、列数、行距、列距矩形阵列实体
        /// </summary>
        /// <param name="ent">要阵列的对象</param>
        /// <param name="numRows">行数</param>
        /// <param name="numCols">列数</param>
        /// <param name="disRows">行距</param>
        /// <param name="disCols">列距</param>
        /// <returns>阵列后的对象集合</returns>
        public static Entity[] ArrayRectang(this Entity ent, int numRows, int numCols, double disRows, double disCols)
        {
            Entity[] ents = new Entity[numRows * numCols];
            int N = 0;
            for (int m = 0; m < numRows; m++)
            {
                for (int n = 0; n < numCols; n++)
                {
                    Matrix3d mt = Matrix3d.Displacement(new Vector3d(n * disCols, m * disRows, 0));
                    ents[N] = ent.GetTransformedCopy(mt);
                    N++;
                }
            }
            return ents;
        }
        /// <summary>
        /// 指定圆心、阵列数量、角度圆形阵列实体
        /// </summary>
        /// <param name="ent">要整列的实体对象</param>
        /// <param name="cenPt">阵列圆心</param>
        /// <param name="numObj">阵列数量</param>
        /// <param name="Angle">角度</param>
        /// <returns>阵列后的实体对象</returns>
        public static Entity[] ArrayPolar(this Entity ent, Point3d cenPt, int numObj, double Angle)
        {
            Entity[] ents = new Entity[numObj];
            ents[0] = ent;
            for (int i = 1; i < numObj; i++)
            {
                Matrix3d mt = Matrix3d.Rotation(Angle * i / numObj, Vector3d.ZAxis, cenPt);
                ents[i] = ent.GetTransformedCopy(mt);
            }
            return ents;
        }
        /// <summary>
        /// 设置动态块属性
        /// </summary>
        /// <param name="br">要设置属性的动态块参照</param>
        /// <param name="Properties">属性数组</param>
        public static void SetDynamicValue(BlockReference br, Property[] properties)
        {
            try
            {
                if (br.IsDynamicBlock)
                {
                    foreach (DynamicBlockReferenceProperty dbrp in br.DynamicBlockReferencePropertyCollection)
                    {
                        for (int i = 0; i < properties.Length; i++)
                        {
                            if (dbrp.PropertyName == properties[i].PropertyName)
                            {
                                dbrp.Value = properties[i].Value;
                            }
                        }
                    }
                }
            }
            catch
            { }
        }
        /// <summary>
        /// 动态块属性
        /// </summary>
        public class Property
        {
            private string propertyname;
            private double value;
            public string PropertyName
            {
                get { return propertyname; }
            }
            public double Value
            {
                get { return value; }
            }
            public Property(string PropertyName, double Value)
            {
                propertyname = PropertyName;
                value = Value;
            }
        }

        /// <summary>
        /// 改变对象的绘图次序到顶层
        /// </summary>
        /// <param name="ent"></param>
        /// <param name="db"></param>
        public static void MoveTop(this Entity ent, Database db)
        {
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                ObjectIdCollection idc = new ObjectIdCollection();
                idc.Add(ent.ObjectId);
                BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForWrite, false);
                BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace],
                    OpenMode.ForWrite, false);
                DrawOrderTable orderTable = tr.GetObject(btr.DrawOrderTableId, OpenMode.ForWrite) as
                    DrawOrderTable;
                orderTable.MoveToTop(idc);
                tr.Commit();
            }
        }


        /// <summary>
        /// 写块克隆对象
        /// </summary>
        /// <param name="idCollection">对象ObjectId集合</param>
        /// <returns>克隆后的数据库</returns>
        public static Database WBClone(ObjectIdCollection idCollection)
        {
            //获取新数据库的块表id
            Database TargetDb = new Database(true, true);
            ObjectId objectId;
            using (Transaction trans = TargetDb.TransactionManager.StartTransaction())
            {
                BlockTable bt = (BlockTable)trans.GetObject(TargetDb.BlockTableId, OpenMode.ForRead);
                BlockTableRecord btr = (BlockTableRecord)trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);
                objectId = btr.ObjectId;
                trans.Commit();
            }
            //拷贝对象
            var idsGroup = idCollection.ToList()
                .GroupBy(i => i.Database)
                .Select(i =>
                    new
                    {
                        i.Key,
                        Value = i.ToList(),
                    }).ToList();
            foreach (var idg in idsGroup)
            {
                Database db = idg.Key;
                var idc = idg.Value.ToObjectIdCollection();
                IdMapping Map = new IdMapping();
                db.WblockCloneObjects(idc, objectId, Map, DuplicateRecordCloning.Replace, false);
            }

            return TargetDb;
        }
        /// <summary>
        /// 写块克隆对象
        /// </summary>
        /// <param name="Idc">对象ObjectId集合</param>
        /// <param name="FileName">目标文件名</param>
        public static void WBClone(ObjectIdCollection idCollection, string fileName)
        {
            var TargetDb = WBClone(idCollection);
            TargetDb.SaveAs(fileName, DwgVersion.Current);
        }
    }
}
