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
            // Рисуем все точки как маркеры
            if (IsDrawing)
            {
                foreach (var point in points)
                {
                    DrawDot(g, point, dotRadius, Color.Black);
                }
            }

            if (points.Count < 4) return;

            // for (int i = 1; i < _points.Count - 3; i += 3)
            for (int i = 1; i < points.Count - 3; i += 3)
            {
                DrawBezierSegment(g, points[i], points[i + 1], points[i + 2], points[i + 3]);
                
                // Рисуем опорные и контрольные точки
                if (IsDrawing)
                {
                    // Рисуем опорные точки
                    DrawDot(g, points[i], dotRadius, Color.Black);      // Первая опорная точка
                    DrawDot(g, points[i + 3], dotRadius, Color.Black);  // Вторая опорная точка
                    
                    // Рисуем контрольные точки
                    DrawDot(g, points[i + 1], dotRadius, Color.Blue);   // Первая контрольная точка
                    DrawDot(g, points[i + 2], dotRadius, Color.Blue);   // Вторая контрольная точка
                    
                    // Рисуем направляющие линии
                    using (Pen guidePen = new Pen(Color.Gray, 1))
                    {
                        g.DrawLine(guidePen, points[i ], points[i + 1]);     // От первой опорной к первой контрольной
                        g.DrawLine(guidePen, points[i + 3], points[i + 2]); // От второй опорной ко второй контрольной
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

        private void DrawBezierSegment(Graphics g, Point p0, Point p1, Point p2, Point p3)
        {
            // Рисуем кривую Безье через 4 точки:
            // p0 - первая опорная точка
            // p1 - первая контрольная точка
            // p2 - вторая контрольная точка
            // p3 - вторая опорная точка
            for (double t = 0; t <= 1; t += 0.001)
            {
                Point a = Lerp(p0, p1, t);
                Point b = Lerp(p1, p2, t);
                Point c = Lerp(p2, p3, t);
                Point ab = Lerp(a, b, t);
                Point bc = Lerp(b, c, t);
                Point abc = Lerp(ab, bc, t);
                
                // Рисуем точку кривой
                for (int x = -strokeWidth / 2; x <= strokeWidth / 2; x++)
                {
                    for (int y = -strokeWidth / 2; y <= strokeWidth / 2; y++)
                    {
                        g.FillRectangle(new SolidBrush(strokeColor), 
                            abc.X + x, abc.Y + y, 1, 1);
                    }
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
            int numPoints = points.Count;
            bool hasDummyPoint = points.Count > 0 && points[0].IsEmpty;
            int requiredPointsForOneSegment = hasDummyPoint ? 5 : 4;

            if (numPoints < requiredPointsForOneSegment) return;

            int p0_idx, p1_idx, p2_idx, p3_idx;
            if (hasDummyPoint)
            {
                p3_idx = numPoints - 1;
                p2_idx = numPoints - 2;
                p1_idx = numPoints - 3;
                p0_idx = numPoints - 4;
            }
            else
            {
                p3_idx = numPoints - 1;
                p2_idx = numPoints - 2;
                p1_idx = numPoints - 3;
                p0_idx = numPoints - 4;
            }
            
            Point p0 = points[p0_idx];
            points[p3_idx] = newLocation; // P3 всегда текущая позиция мыши
            points[p2_idx] = newLocation; // P2 (вторая контрольная) = P3
            points[p1_idx] = p0;          // P1 (первая контрольная) = P0
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
