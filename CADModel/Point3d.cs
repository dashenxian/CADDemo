namespace CADModel
{
    public struct Point3d
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public static Point3d operator +(Point3d p1, Point3d p2)
        {
            return new Point3d { X = p1.X + p2.X, Y = p1.Y + p2.Y, Z = p1.Z + p2.Z };
        }
        public static Point3d operator -(Point3d p1, Point3d p2)
        {
            return new Point3d { X = p1.X - p2.X, Y = p1.Y - p2.Y, Z = p1.Z + p2.Z };
        }
    }
}
