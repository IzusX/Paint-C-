using System.Drawing;
using DrawingEditor.Interfaces;

namespace DrawingEditor.Shapes
{
    public class Polyline : Shape, ITransformable
    {
        public Polyline()
        {
            points = new List<Point>();
            strokeColor = Color.Black;
            fillColor = Color.Transparent;
            strokeWidth = 1;
        }

        public override void Draw(Graphics g)
        {
            if (points.Count < 2) return;

            // Рисуем линии между точками
            for (int i = 0; i < points.Count - 1; i++)
            {
                DrawLine(g, points[i], points[i + 1]);
            }

            // Рисуем точки вершин при активном рисовании
            if (IsDrawing)
            {
                foreach (var point in points)
                {
                    DrawDot(g, point, 3, Color.Blue);
                }
            }
        }

        private void DrawDot(Graphics g, Point center, int radius, Color color)
        {
            g.FillEllipse(new SolidBrush(color),
                center.X - radius, center.Y - radius,
                radius * 2, radius * 2);
        }

        private void DrawLine(Graphics g, Point p1, Point p2)
        {
            // Используем алгоритм Брезенхэма для рисования линии
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
            // Проверяем, находится ли точка рядом с любым сегментом ломаной
            for (int i = 0; i < points.Count - 1; i++)
            {
                if (IsPointNearLine(p, points[i], points[i + 1]))
                    return true;
            }
            return false;
        }

        private bool IsPointNearLine(Point p, Point lineStart, Point lineEnd)
        {
            const int threshold = 5; // Расстояние, в пределах которого точка считается близкой к линии
            
            double distance = Math.Abs(
                (lineEnd.Y - lineStart.Y) * p.X -
                (lineEnd.X - lineStart.X) * p.Y +
                lineEnd.X * lineStart.Y -
                lineEnd.Y * lineStart.X
            ) / Math.Sqrt(
                Math.Pow(lineEnd.Y - lineStart.Y, 2) +
                Math.Pow(lineEnd.X - lineStart.X, 2)
            );

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
            if (points.Count == 0) return;

            Point center = new Point(
                (int)points.Average(p => p.X),
                (int)points.Average(p => p.Y)
            );

            double radians = angle * Math.PI / 180;
            for (int i = 0; i < points.Count; i++)
            {
                int dx = points[i].X - center.X;
                int dy = points[i].Y - center.Y;

                points[i] = new Point(
                    center.X + (int)(dx * Math.Cos(radians) - dy * Math.Sin(radians)),
                    center.Y + (int)(dx * Math.Sin(radians) + dy * Math.Cos(radians))
                );
            }
        }

        public void Scale(float sx, float sy)
        {
            if (points.Count == 0) return;

            Point center = new Point(
                (int)points.Average(p => p.X),
                (int)points.Average(p => p.Y)
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