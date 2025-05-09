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

            // Определяем, замкнута ли фигура для заливки и отрисовки
            bool isClosed = false;
            if (points.Count > 2) // Нужно хотя бы 3 точки для замкнутой фигуры
            {
                Point firstPoint = points[0];
                Point lastPoint = points[points.Count - 1];
                double distance = Math.Sqrt(Math.Pow(lastPoint.X - firstPoint.X, 2) + Math.Pow(lastPoint.Y - firstPoint.Y, 2));
                const int closingThreshold = 10; 
                if (distance <= closingThreshold) 
                {
                    isClosed = true;
                }
            }

            // Рисуем заливку, если фигура замкнута и есть цвет заливки
            if (isClosed && fillColor != Color.Transparent && points.Count >= 3)
            {
                FillPolyline(g, points);
            }

            // Рисуем линии между точками
            for (int i = 0; i < points.Count - 1; i++)
            {
                DrawLineBresenham(g, points[i], points[i + 1], strokeColor, strokeWidth);
            }

            // Рисуем замыкающий сегмент, если фигура замкнута
            if (isClosed)
            {
                DrawLineBresenham(g, points[points.Count - 1], points[0], strokeColor, strokeWidth);
            }

            // Рисуем точки вершин при активном рисовании
            if (IsDrawing && IsDrawingPolyline) // Добавил IsDrawingPolyline для точности
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

        private void FillPolyline(Graphics g, List<Point> vertices)
        {
            if (vertices.Count < 3) return;
            
            // Копируем вершины, чтобы при необходимости добавить замыкающую точку для корректной заливки,
            // не изменяя оригинальный список точек ломаной (если она не была идеально замкнута)
            List<Point> fillVertices = new List<Point>(vertices);
            // Если первая и последняя точки не совпадают точно, но близки (isClosed было true)
            // для корректной заливки многоугольника лучше, чтобы они совпадали.
            // Однако, алгоритм scanline должен справиться и без этого, если рёбра правильно обрабатываются.
            // Для простоты оставим как есть, алгоритм должен найти пересечения.

            int minY = fillVertices.Min(p => p.Y);
            int maxY = fillVertices.Max(p => p.Y);

            for (int y = minY; y <= maxY; y++)
            {
                List<int> xIntersections = new List<int>();
                for (int i = 0; i < fillVertices.Count; i++)
                {
                    Point p1 = fillVertices[i];
                    Point p2 = fillVertices[(i + 1) % fillVertices.Count]; // Замыкаем для обхода ребер

                    if ((p1.Y <= y && p2.Y > y) || (p2.Y <= y && p1.Y > y)) // Ребро пересекает текущую строку y
                    {
                        if (p1.Y == p2.Y) continue; // Горизонтальное ребро на уровне y, пропускаем (или обрабатываем особо)
                        // Вычисляем точку пересечения x
                        double x = p1.X + (double)(y - p1.Y) / (p2.Y - p1.Y) * (p2.X - p1.X);
                        xIntersections.Add((int)Math.Round(x));
                    }
                }
                xIntersections.Sort();

                for (int i = 0; i + 1 < xIntersections.Count; i += 2)
                {
                    for (int x = xIntersections[i]; x <= xIntersections[i + 1]; x++)
                    {
                        g.FillRectangle(new SolidBrush(fillColor), x, y, 1, 1);
                    }
                }
            }
        }
    }
} 