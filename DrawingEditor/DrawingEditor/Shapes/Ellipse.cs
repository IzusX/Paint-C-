using System.Drawing;
using DrawingEditor.Interfaces;

namespace DrawingEditor.Shapes
{
    public class Ellipse : Shape, ITransformable
    {
        private Point startPoint;
        private Point endPoint;
        private float rotationAngle = 0;

        public Ellipse()
        {
            points = new List<Point>();
            strokeColor = Color.Black;
            fillColor = Color.Transparent;
            strokeWidth = 1;
        }

        public override void Draw(Graphics g)
        {
            if (points.Count < 2) return;

            startPoint = points[0];
            endPoint = points[1];

            int centerX = (startPoint.X + endPoint.X) / 2;
            int centerY = (startPoint.Y + endPoint.Y) / 2;
            int radiusX = Math.Abs(endPoint.X - startPoint.X) / 2;
            int radiusY = Math.Abs(endPoint.Y - startPoint.Y) / 2;

            // Сохраняем все точки контура эллипса
            int steps = (int)(2 * Math.PI * Math.Max(radiusX, radiusY));
            if (steps < 60) steps = 60;
            double angleRad = rotationAngle * Math.PI / 180.0;
            Point? prev = null;
            List<Point> ellipseContour = new List<Point>();
            for (int i = 0; i <= steps; i++)
            {
                double t = 2 * Math.PI * i / steps;
                double x0 = radiusX * Math.Cos(t);
                double y0 = radiusY * Math.Sin(t);
                // Поворот
                double xr = x0 * Math.Cos(angleRad) - y0 * Math.Sin(angleRad);
                double yr = x0 * Math.Sin(angleRad) + y0 * Math.Cos(angleRad);
                int x = (int)Math.Round(centerX + xr);
                int y = (int)Math.Round(centerY + yr);
                ellipseContour.Add(new Point(x, y));
                if (prev != null)
                {
                    DrawLineBresenham(g, prev.Value, new Point(x, y), strokeColor, strokeWidth);
                }
                prev = new Point(x, y);
            }

            // Заливка по новой рамке
            if (fillColor != Color.Transparent)
            {
                int minX = ellipseContour.Min(p => p.X);
                int maxX = ellipseContour.Max(p => p.X);
                int minY = ellipseContour.Min(p => p.Y);
                int maxY = ellipseContour.Max(p => p.Y);
                FillEllipse(g, centerX, centerY, radiusX, radiusY, minX, maxX, minY, maxY);
            }
        }

        private void DrawEllipse(Graphics g, int centerX, int centerY, int radiusX, int radiusY)
        {
            // Учитываем угол поворота (rotationAngle)
            double angleRad = rotationAngle * Math.PI / 180.0;
            double cos = Math.Cos(-angleRad); // обратное вращение
            double sin = Math.Sin(-angleRad);
            int minX = centerX - radiusX;
            int maxX = centerX + radiusX;
            int minY = centerY - radiusY;
            int maxY = centerY + radiusY;
            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    // Переводим в систему координат эллипса
                    double dx = x - centerX;
                    double dy = y - centerY;
                    // Обратное вращение
                    double xr = dx * cos - dy * sin;
                    double yr = dx * sin + dy * cos;
                    // Проверяем попадание в эллипс
                    if ((xr * xr) / (radiusX * radiusX) + (yr * yr) / (radiusY * radiusY) <= 1.0)
                    {
                        g.FillRectangle(new SolidBrush(fillColor), x, y, 1, 1);
                    }
                }
            }
        }

        private void PlotEllipsePoints(Graphics g, int centerX, int centerY, int x, int y)
        {
            void DrawPoint(int px, int py)
            {
                for (int w = -strokeWidth / 2; w <= strokeWidth / 2; w++)
                {
                    for (int h = -strokeWidth / 2; h <= strokeWidth / 2; h++)
                    {
                        g.FillRectangle(new SolidBrush(strokeColor), px + w, py + h, 1, 1);
                    }
                }
            }

            DrawPoint(centerX + x, centerY + y);
            DrawPoint(centerX - x, centerY + y);
            DrawPoint(centerX + x, centerY - y);
            DrawPoint(centerX - x, centerY - y);
        }

        // Новый вариант FillEllipse с рамкой
        private void FillEllipse(Graphics g, int centerX, int centerY, int radiusX, int radiusY, int minX, int maxX, int minY, int maxY)
        {
            double angleRad = rotationAngle * Math.PI / 180.0;
            double cos = Math.Cos(-angleRad); // обратное вращение
            double sin = Math.Sin(-angleRad);
            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    double dx = x - centerX;
                    double dy = y - centerY;
                    double xr = dx * cos - dy * sin;
                    double yr = dx * sin + dy * cos;
                    if ((xr * xr) / (radiusX * radiusX) + (yr * yr) / (radiusY * radiusY) <= 1.0)
                    {
                        g.FillRectangle(new SolidBrush(fillColor), x, y, 1, 1);
                    }
                }
            }
        }

        public override bool Contains(Point p)
        {
            int centerX = (startPoint.X + endPoint.X) / 2;
            int centerY = (startPoint.Y + endPoint.Y) / 2;
            int radiusX = Math.Abs(endPoint.X - startPoint.X) / 2;
            int radiusY = Math.Abs(endPoint.Y - startPoint.Y) / 2;

            if (radiusX == 0 || radiusY == 0) return false;

            // Проверяем, находится ли точка внутри эллипса
            double normalizedX = (double)(p.X - centerX) / radiusX;
            double normalizedY = (double)(p.Y - centerY) / radiusY;
            return (normalizedX * normalizedX + normalizedY * normalizedY) <= 1.0;
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
            rotationAngle += angle;
            rotationAngle %= 360f;
        }

        public void Scale(float sx, float sy)
        {
            if (points.Count < 2) return;

            const int minSize = 2;
            Point originalStartPoint = points[0];
            Point originalEndPoint = points[1];
            System.Drawing.Rectangle initialBounds = GetBoundingBox();
            // rotationAngle не меняется при масштабировании

            int currentCenterX = (points[0].X + points[1].X) / 2;
            int currentCenterY = (points[0].Y + points[1].Y) / 2;

            points[0] = new Point(
                currentCenterX + (int)((originalStartPoint.X - currentCenterX) * sx),
                currentCenterY + (int)((originalStartPoint.Y - currentCenterY) * sy)
            );
            points[1] = new Point(
                currentCenterX + (int)((originalEndPoint.X - currentCenterX) * sx),
                currentCenterY + (int)((originalEndPoint.Y - currentCenterY) * sy)
            );

            System.Drawing.Rectangle newBounds = GetBoundingBox();

            bool tryingToShrink = sx < 1.0f || sy < 1.0f;
            bool becameTooSmallWidth = (newBounds.Width < minSize && initialBounds.Width >= minSize);
            bool becameTooSmallHeight = (newBounds.Height < minSize && initialBounds.Height >= minSize);
            // Для эллипса, points[0] и points[1] могут совпасть, что GetBoundingBox вернет как Empty, но это валидное состояние (очень маленький эллипс)
            // Поэтому collapsed проверяем чуть иначе или больше полагаемся на becameTooSmall
            bool collapsed = newBounds.IsEmpty && !initialBounds.IsEmpty && (Math.Abs(originalStartPoint.X - originalEndPoint.X) > 0 || Math.Abs(originalStartPoint.Y - originalEndPoint.Y) > 0) ;

            if ((tryingToShrink && (becameTooSmallWidth || becameTooSmallHeight)) || collapsed)
            {
                points[0] = originalStartPoint;
                points[1] = originalEndPoint;
            }
        }

        public override System.Drawing.Rectangle GetBoundingBox()
        {
            if (points.Count < 2) return System.Drawing.Rectangle.Empty;

            // Необходимо вычислить реальные точки контура с учетом поворота
            var currentStartPoint = points[0];
            var currentEndPoint = points[1];

            int centerX = (currentStartPoint.X + currentEndPoint.X) / 2;
            int centerY = (currentStartPoint.Y + currentEndPoint.Y) / 2;
            int radiusX = Math.Abs(currentEndPoint.X - currentStartPoint.X) / 2;
            int radiusY = Math.Abs(currentEndPoint.Y - currentStartPoint.Y) / 2;

            if (radiusX == 0 && radiusY == 0) return new System.Drawing.Rectangle(centerX, centerY, 0, 0);

            List<Point> ellipseContour = new List<Point>();
            int steps = (int)(Math.PI * (radiusX + radiusY)); // Упрощенное количество шагов
            if (steps < 60) steps = 60;
            double angleRad = rotationAngle * Math.PI / 180.0;
            
            for (int i = 0; i <= steps; i++)
            {
                double t = 2 * Math.PI * i / steps;
                double x0 = radiusX * Math.Cos(t);
                double y0 = radiusY * Math.Sin(t);
                
                double xr = x0 * Math.Cos(angleRad) - y0 * Math.Sin(angleRad);
                double yr = x0 * Math.Sin(angleRad) + y0 * Math.Cos(angleRad);
                
                int x = (int)Math.Round(centerX + xr);
                int y = (int)Math.Round(centerY + yr);
                ellipseContour.Add(new Point(x, y));
            }
            
            if (ellipseContour.Count == 0) return System.Drawing.Rectangle.Empty; // На случай если что-то пошло не так

            int minX = ellipseContour.Min(p => p.X);
            int minY = ellipseContour.Min(p => p.Y);
            int maxX = ellipseContour.Max(p => p.X);
            int maxY = ellipseContour.Max(p => p.Y);
            return System.Drawing.Rectangle.FromLTRB(minX, minY, maxX, maxY);
        }
    }
}
