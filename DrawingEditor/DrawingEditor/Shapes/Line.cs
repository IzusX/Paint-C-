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
            DrawLine(g, points[0], points[1]);
        }

        private void DrawLine(Graphics g, Point p1, Point p2)
        {
            int x1 = p1.X, y1 = p1.Y, x2 = p2.X, y2 = p2.Y;
            int dx = Math.Abs(x2 - x1);
            int dy = Math.Abs(y2 - y1);
            int sx = x1 < x2 ? 1 : -1;
            int sy = y1 < y2 ? 1 : -1;
            int err = dx - dy;

            while (true)
            {
                for (int w = -strokeWidth / 2; w <= strokeWidth / 2; w++)
                {
                    for (int h = -strokeWidth / 2; h <= strokeWidth / 2; h++)
                    {
                        g.FillRectangle(new SolidBrush(strokeColor), x1 + w, y1 + h, 1, 1);
                    }
                }

                if (x1 == x2 && y1 == y2) break;
                int e2 = 2 * err;
                if (e2 > -dy)
                {
                    err -= dy;
                    x1 += sx;
                }
                if (e2 < dx)
                {
                    err += dx;
                    y1 += sy;
                }
            }
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
