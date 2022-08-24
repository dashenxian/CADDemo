namespace CADModel
{
    public struct Point2d
    {
        public double X { get; set; }
        public double Y { get; set; }
        public static Point2d operator +(Point2d p1, Point2d p2)
        {
            return new Point2d { X = p1.X + p2.X, Y = p1.Y + p2.Y };
        }
        public static Point2d operator -(Point2d p1, Point2d p2)
        {
            return new Point2d { X = p1.X - p2.X, Y = p1.Y - p2.Y };
        }
	}
}
