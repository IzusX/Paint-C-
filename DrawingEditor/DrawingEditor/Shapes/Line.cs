using System.Drawing;
using DrawingEditor.Interfaces;

namespace DrawingEditor.Shapes
{
    public class Line : Shape, ITransformable
    {
        public Line()
        {
            points = new List<Point>();
            strokeColor = Color.Black;
            strokeWidth = 1;
        }

        public override void Draw(Graphics g)
        {
            if (points.Count < 2) return;
            DrawLineBresenham(g, points[0], points[1], strokeColor, strokeWidth);
        }

        public override bool Contains(Point p)
        {
            if (points.Count < 2) return false;

            // Проверяем, находится ли точка достаточно близко к линии
            const int threshold = 5;
            Point p1 = points[0];
            Point p2 = points[1];

            // Вычисляем расстояние от точки до линии
            double numerator = Math.Abs((p2.Y - p1.Y) * p.X - (p2.X - p1.X) * p.Y + p2.X * p1.Y - p2.Y * p1.X);
            double denominator = Math.Sqrt(Math.Pow(p2.Y - p1.Y, 2) + Math.Pow(p2.X - p1.X, 2));
            
            if (denominator == 0) return false;
            
            double distance = numerator / denominator;
            return distance <= threshold;
        }

        public override void Move(int dx, int dy)
        {
            for (int i = 0; i < points.Count; i++)
            {
                points[i] = new Point(points[i].X + dx, points[i].Y + dy);
            }
        }

        public void Rotate(float angle)
        {
            // Находим центр линии
            float centerX = (float)points.Average(p => p.X);
            float centerY = (float)points.Average(p => p.Y);
            
            // Конвертируем угол в радианы
            double angleRad = angle * Math.PI / 180.0;
            double cos = Math.Cos(angleRad);
            double sin = Math.Sin(angleRad);

            // Поворачиваем каждую точку вокруг центра
            for (int i = 0; i < points.Count; i++)
            {
                float dx = points[i].X - centerX;
                float dy = points[i].Y - centerY;
                
                int newX = (int)(centerX + dx * cos - dy * sin);
                int newY = (int)(centerY + dx * sin + dy * cos);
                
                points[i] = new Point(newX, newY);
            }
        }

        public void Scale(float sx, float sy)
        {
            if (points.Count < 2) return;

            Point center = new Point(
                (int)points.Average(p => (double)p.X),
                (int)points.Average(p => (double)p.Y)
            );

            for (int i = 0; i < points.Count; i++)
            {
                points[i] = new Point(
                    center.X + (int)((points[i].X - center.X) * sx),
                    center.Y + (int)((points[i].Y - center.Y) * sy)
                );
            }
        }
    }
}
