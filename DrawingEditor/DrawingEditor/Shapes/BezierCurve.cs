using System.Drawing;
using DrawingEditor.Interfaces;

namespace DrawingEditor.Shapes
{
    public class BezierCurve : Shape, ITransformable
    {
        private int dotRadius = 5;

        public BezierCurve()
        {
            points = new List<Point>();
            strokeColor = Color.Black;
            fillColor = Color.Transparent;
            strokeWidth = 1;
        }

        public Point GetLastPoint()
        {
            return points.Count > 0 ? points[^1] : Point.Empty;
        }

        public override void Draw(Graphics g)
        {
            if (points.Count < 4) return; // Минимум 5 точек (включая пустую начальную)

            // Рисуем кривую Безье
            for (int i = 1; i < points.Count - 3; i += 3)
            {
                DrawBezierSegment(g, points[i], points[i + 1], points[i + 2], points[i + 3]);
                
                // Рисуем точки управления и направляющие линии только при активном рисовании
                if (IsDrawing)
                {
                    // Рисуем опорные точки
                    DrawDot(g, points[i], dotRadius, Color.Black);      // Опорная точка
                    DrawDot(g, points[i + 3], dotRadius, Color.Black);  // Конечная точка
                    
                    // Рисуем контрольные точки
                    DrawDot(g, points[i + 1], dotRadius, Color.Blue);   // Контрольная точка 1
                    DrawDot(g, points[i + 2], dotRadius, Color.Blue);   // Контрольная точка 2
                    
                    // Рисуем направляющие линии
                    using (Pen guidePen = new Pen(Color.Gray, 1))
                    {
                        g.DrawLine(guidePen, points[i], points[i + 1]);     // Линия от опорной к первой контрольной
                        g.DrawLine(guidePen, points[i + 3], points[i + 2]); // Линия от конечной ко второй контрольной
                    }
                }
            }
        }

        private void DrawDot(Graphics g, Point center, int radius, Color color)
        {
            g.FillEllipse(new SolidBrush(color), 
                center.X - radius, center.Y - radius, 
                radius * 2, radius * 2);
        }

        private void DrawBezierSegment(Graphics g, Point p1, Point p2, Point p3, Point p4)
        {
            for (double t = 0; t <= 1; t += 0.001)
            {
                Point a = Lerp(p1, p2, t);
                Point b = Lerp(p2, p3, t);
                Point c = Lerp(p3, p4, t);
                Point ab = Lerp(a, b, t);
                Point bc = Lerp(b, c, t);
                Point abc = Lerp(ab, bc, t);

                // Рисуем основную кривую
                for (int x = -strokeWidth / 2; x <= strokeWidth / 2; x++)
                {
                    for (int y = -strokeWidth / 2; y <= strokeWidth / 2; y++)
                    {
                        g.FillRectangle(new SolidBrush(strokeColor), 
                            abc.X + x, abc.Y + y, 1, 1);
                    }
                }

                // Рисуем дополнительные точки на кривой
                if (IsDrawing)
                {
                    g.FillRectangle(new SolidBrush(Color.Black), a.X, a.Y, 1, 1);
                    g.FillRectangle(new SolidBrush(Color.Black), c.X, c.Y, 1, 1);
                }
            }
        }

        private Point Lerp(Point a, Point b, double t)
        {
            return new Point(
                Convert.ToInt32(a.X * (1 - t) + b.X * t),
                Convert.ToInt32(a.Y * (1 - t) + b.Y * t)
            );
        }

        public void UpdateControlPoints(Point newLocation)
        {
            if (points.Count >= 3) // Учитываем пустую начальную точку
            {
                int lastIndex = points.Count - 1;
                Point anchorPoint = points[lastIndex - 1]; // Опорная точка
                //points[lastIndex - 1] = newLocation; // Контрольная точка 2
                points[lastIndex] = newLocation; // Конечная точка
                // Симметрично обновляем контрольную точку 1
                points[lastIndex - 2] = new Point(
                    anchorPoint.X - (newLocation.X - anchorPoint.X),
                    anchorPoint.Y - (newLocation.Y - anchorPoint.Y)
                );
            }
        }

        // Реализация остальных методов интерфейса
        public override bool Contains(Point p)
        {
            // Упрощенная проверка - проверяем только точки управления
            foreach (var point in points)
            {
                if (Math.Abs(point.X - p.X) <= dotRadius && 
                    Math.Abs(point.Y - p.Y) <= dotRadius)
                    return true;
            }
            return false;
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
            if (points.Count == 0 || points.All(p => p.IsEmpty && points.Count > 1)) return; // если только пустая точка или вообще пусто
            
            var relevantOriginalPointsForCenter = points.Where((p, index) => !(index == 0 && p.IsEmpty && points.Count > 1)).ToList();
            if(relevantOriginalPointsForCenter.Count == 0) return;

            const int minSize = 2;
            List<Point> originalPointsFull = points.Select(p => new Point(p.X, p.Y)).ToList();
            System.Drawing.Rectangle initialBounds = GetBoundingBox();

            Point center = new Point(
                (int)relevantOriginalPointsForCenter.Average(p => p.X),
                (int)relevantOriginalPointsForCenter.Average(p => p.Y)
            );

            for (int i = 0; i < points.Count; i++)
            {
                if (points[i].IsEmpty && i == 0 && points.Count > 1) continue; 
                
                points[i] = new Point(
                    center.X + (int)((originalPointsFull[i].X - center.X) * sx),
                    center.Y + (int)((originalPointsFull[i].Y - center.Y) * sy)
                );
            }

            System.Drawing.Rectangle newBounds = GetBoundingBox();
            
            bool tryingToShrink = sx < 1.0f || sy < 1.0f;
            bool becameTooSmallWidth = (newBounds.Width < minSize && initialBounds.Width >= minSize);
            bool becameTooSmallHeight = (newBounds.Height < minSize && initialBounds.Height >= minSize);
            bool collapsed = newBounds.IsEmpty && !initialBounds.IsEmpty;

            if ((tryingToShrink && (becameTooSmallWidth || becameTooSmallHeight)) || collapsed)
            {
                for (int i = 0; i < originalPointsFull.Count; i++)
                {
                    points[i] = originalPointsFull[i];
                }
            }
        }

        public override System.Drawing.Rectangle GetBoundingBox()
        {
            // Для кривой Безье проходим по всем точкам (опорным и контрольным)
            // для более точного bounding box нужно было бы сэмплировать точки на самой кривой.
            if (points.Count == 0 || points.All(p => p.IsEmpty)) return System.Drawing.Rectangle.Empty;
            
            // Игнорируем первую пустую точку, если она есть
            var relevantPoints = points.Where((p, index) => !(index == 0 && p.IsEmpty && points.Count > 1)).ToList();
            if (relevantPoints.Count == 0) return System.Drawing.Rectangle.Empty;

            int minX = relevantPoints.Min(p => p.X);
            int minY = relevantPoints.Min(p => p.Y);
            int maxX = relevantPoints.Max(p => p.X);
            int maxY = relevantPoints.Max(p => p.Y);
            return System.Drawing.Rectangle.FromLTRB(minX, minY, maxX, maxY);
        }
    }
}
