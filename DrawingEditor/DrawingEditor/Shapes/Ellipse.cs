using System.Drawing;
using DrawingEditor.Interfaces;

namespace DrawingEditor.Shapes
{
    public class Ellipse : Shape, ITransformable
    {
        private Point startPoint;
        private Point endPoint;

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

            // Находим центр и радиусы эллипса
            int centerX = (startPoint.X + endPoint.X) / 2;
            int centerY = (startPoint.Y + endPoint.Y) / 2;
            int radiusX = Math.Abs(endPoint.X - startPoint.X) / 2;
            int radiusY = Math.Abs(endPoint.Y - startPoint.Y) / 2;

            // Рисуем эллипс алгоритмом средней точки
            DrawEllipse(g, centerX, centerY, radiusX, radiusY);

            // Если есть заливка
            if (fillColor != Color.Transparent)
            {
                FillEllipse(g, centerX, centerY, radiusX, radiusY);
            }
        }

        private void DrawEllipse(Graphics g, int centerX, int centerY, int radiusX, int radiusY)
        {
            int x = 0;
            int y = radiusY;
            
            // Начальные значения для алгоритма
            double d1 = (radiusY * radiusY) - (radiusX * radiusX * radiusY) + (0.25f * radiusX * radiusX);
            double dx = 2 * radiusY * radiusY * x;
            double dy = 2 * radiusX * radiusX * y;

            // Первая часть
            while (dx < dy)
            {
                PlotEllipsePoints(g, centerX, centerY, x, y);

                if (d1 < 0)
                {
                    x++;
                    dx = dx + (2 * radiusY * radiusY);
                    d1 = d1 + dx + (radiusY * radiusY);
                }
                else
                {
                    x++;
                    y--;
                    dx = dx + (2 * radiusY * radiusY);
                    dy = dy - (2 * radiusX * radiusX);
                    d1 = d1 + dx - dy + (radiusY * radiusY);
                }
            }

            // Вторая часть
            double d2 = ((radiusY * radiusY) * ((x + 0.5f) * (x + 0.5f))) +
                       ((radiusX * radiusX) * ((y - 1) * (y - 1))) -
                       (radiusX * radiusX * radiusY * radiusY);

            while (y >= 0)
            {
                PlotEllipsePoints(g, centerX, centerY, x, y);

                if (d2 > 0)
                {
                    y--;
                    dy = dy - (2 * radiusX * radiusX);
                    d2 = d2 + (radiusX * radiusX) - dy;
                }
                else
                {
                    y--;
                    x++;
                    dx = dx + (2 * radiusY * radiusY);
                    dy = dy - (2 * radiusX * radiusX);
                    d2 = d2 + dx - dy + (radiusX * radiusX);
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

        private void FillEllipse(Graphics g, int centerX, int centerY, int radiusX, int radiusY)
        {
            for (int y = -radiusY; y <= radiusY; y++)
            {
                for (int x = -radiusX; x <= radiusX; x++)
                {
                    if ((x * x * radiusY * radiusY + y * y * radiusX * radiusX) <= (radiusX * radiusX * radiusY * radiusY))
                    {
                        g.FillRectangle(new SolidBrush(fillColor), centerX + x, centerY + y, 1, 1);
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
            // Находим центр эллипса
            float centerX = (float)Points.Average(p => p.X);
            float centerY = (float)Points.Average(p => p.Y);
            
            // Конвертируем угол в радианы
            double angleRad = angle * Math.PI / 180.0;
            double cos = Math.Cos(angleRad);
            double sin = Math.Sin(angleRad);

            // Поворачиваем каждую точку вокруг центра
            for (int i = 0; i < Points.Count; i++)
            {
                float dx = Points[i].X - centerX;
                float dy = Points[i].Y - centerY;
                
                int newX = (int)(centerX + dx * cos - dy * sin);
                int newY = (int)(centerY + dx * sin + dy * cos);
                
                Points[i] = new Point(newX, newY);
            }
        }

        public void Scale(float sx, float sy)
        {
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
