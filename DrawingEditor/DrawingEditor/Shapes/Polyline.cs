using System.Drawing;
using DrawingEditor.Interfaces;

namespace DrawingEditor.Shapes
{
    public class Polyline : Shape, ITransformable
    {
        private bool _isDrawingPolyline = false; // Флаг, что идет процесс рисования именно этой ломаной

        public bool IsDrawingPolyline
        {
            get => _isDrawingPolyline;
            set => _isDrawingPolyline = value;
        }

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
                DrawLineBresenham(g, points[i], points[i + 1], strokeColor, strokeWidth);
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