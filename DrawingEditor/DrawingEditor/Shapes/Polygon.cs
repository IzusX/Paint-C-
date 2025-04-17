using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using DrawingEditor.Interfaces;

namespace DrawingEditor.Shapes
{
    public class Polygon : Shape, ITransformable
    {
        private int sides; // количество сторон
        private double radius; // радиус описанной окружности

        public Polygon(int numberOfSides = 3)
        {
            points = new List<Point>();
            strokeColor = Color.Black;
            fillColor = Color.Transparent;
            strokeWidth = 1;
            sides = Math.Max(3, numberOfSides); // минимум 3 стороны
        }

        public override void Draw(Graphics g)
        {
            if (points.Count < 2) return;

            // Вычисляем центр и радиус
            Point center = points[0];
            Point edge = points[1];
            radius = Math.Sqrt(Math.Pow(edge.X - center.X, 2) + Math.Pow(edge.Y - center.Y, 2));

            // Вычисляем все точки многоугольника
            List<Point> polygonPoints = CalculateVertices(center, radius);

            // Рисуем заливку
            if (fillColor != Color.Transparent)
            {
                FillPolygon(g, polygonPoints);
            }

            // Рисуем стороны
            for (int i = 0; i < polygonPoints.Count; i++)
            {
                Point start = polygonPoints[i];
                Point end = polygonPoints[(i + 1) % polygonPoints.Count];
                DrawLine(g, start, end);
            }
        }

        private List<Point> CalculateVertices(Point center, double radius)
        {
            List<Point> vertices = new List<Point>();
            double angle = Math.PI * 2 / sides;

            // Вычисляем угол поворота, чтобы первая вершина была сверху
            double startAngle = -Math.PI / 2;

            // Вычисляем координаты каждой вершины
            for (int i = 0; i < sides; i++)
            {
                double currentAngle = startAngle + angle * i;
                int x = (int)(center.X + radius * Math.Cos(currentAngle));
                int y = (int)(center.Y + radius * Math.Sin(currentAngle));
                vertices.Add(new Point(x, y));
            }

            return vertices;
        }

        private void DrawLine(Graphics g, Point p1, Point p2)
        {
            // Используем алгоритм Брезенхэма как в классе Line
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

        private void FillPolygon(Graphics g, List<Point> vertices)
        {
            // Находим границы многоугольника
            int minX = vertices.Min(p => p.X);
            int maxX = vertices.Max(p => p.X);
            int minY = vertices.Min(p => p.Y);
            int maxY = vertices.Max(p => p.Y);

            // Проверяем каждую точку внутри границ
            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    if (IsPointInPolygon(new Point(x, y), vertices))
                    {
                        g.FillRectangle(new SolidBrush(fillColor), x, y, 1, 1);
                    }
                }
            }
        }

        private bool IsPointInPolygon(Point p, List<Point> vertices)
        {
            bool inside = false;
            for (int i = 0, j = vertices.Count - 1; i < vertices.Count; j = i++)
            {
                if (((vertices[i].Y > p.Y) != (vertices[j].Y > p.Y)) &&
                    (p.X < (vertices[j].X - vertices[i].X) * (p.Y - vertices[i].Y) / 
                    (vertices[j].Y - vertices[i].Y) + vertices[i].X))
                {
                    inside = !inside;
                }
            }
            return inside;
        }

        // Добавим обязательные методы из абстрактного класса Shape
        public override bool Contains(Point p)
        {
            if (points.Count < 2) return false;

            Point center = points[0];
            Point edge = points[1];
            radius = Math.Sqrt(Math.Pow(edge.X - center.X, 2) + Math.Pow(edge.Y - center.Y, 2));
            List<Point> vertices = CalculateVertices(center, radius);

            return IsPointInPolygon(p, vertices);
        }

        public override void Move(int dx, int dy)
        {
            for (int i = 0; i < points.Count; i++)
            {
                points[i] = new Point(points[i].X + dx, points[i].Y + dy);
            }
        }

        // Реализация методов интерфейса ITransformable
        public void Rotate(float angle)
        {
            Point center = new Point(
                (int)points.Average(p => (double)p.X),
                (int)points.Average(p => (double)p.Y)
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
