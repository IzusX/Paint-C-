using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using DrawingEditor.Interfaces;

namespace DrawingEditor.Shapes
{
    public class RectangleShape : Shape, ITransformable
    {
        public RectangleShape()
        {
            points = new List<Point>();
            strokeColor = Color.Black;
            fillColor = Color.Transparent;
            strokeWidth = 1;
        }

        public override void AddPoint(Point p)
        {
            if (points.Count == 0)
            {
                points.Add(p);
            }
            else if (points.Count == 1)
            {
                // При добавлении второй точки вычисляем все четыре угла по часовой стрелке
                Point p1 = points[0];
                Point p2 = p;
                points.Clear();
                points.Add(new Point(p1.X, p1.Y)); // левый верхний
                points.Add(new Point(p2.X, p1.Y)); // правый верхний
                points.Add(new Point(p2.X, p2.Y)); // правый нижний
                points.Add(new Point(p1.X, p2.Y)); // левый нижний
            }
            else
            {
                // При интерактивном изменении второй точки пересчитываем все четыре угла
                Point p1 = points[0];
                Point p2 = p;
                points[0] = new Point(p1.X, p1.Y); // левый верхний
                points[1] = new Point(p2.X, p1.Y); // правый верхний
                points[2] = new Point(p2.X, p2.Y); // правый нижний
                points[3] = new Point(p1.X, p2.Y); // левый нижний
            }
        }

        public override void UpdatePoint(int index, Point p)
        {
            if (points.Count == 4)
            {
                // Для прямоугольника UpdatePoint всегда пересчитывает все четыре угла
                Point p1 = points[0];
                Point p2 = p;
                points[0] = new Point(p1.X, p1.Y); // левый верхний
                points[1] = new Point(p2.X, p1.Y); // правый верхний
                points[2] = new Point(p2.X, p2.Y); // правый нижний
                points[3] = new Point(p1.X, p2.Y); // левый нижний
            }
        }

        public override void Draw(Graphics g)
        {
            if (points.Count < 4) return;
            // Рисуем стороны прямоугольника вручную
            for (int i = 0; i < 4; i++)
            {
                DrawLineBresenham(g, points[i], points[(i + 1) % 4], strokeColor, strokeWidth);
            }
            // Ручная заливка (простая построчная)
            if (fillColor != Color.Transparent)
            {
                // Найдём границы
                int minY = points.Min(p => p.Y);
                int maxY = points.Max(p => p.Y);
                for (int y = minY; y <= maxY; y++)
                {
                    // Найти пересечения горизонтали y с рёбрами
                    List<int> xIntersections = new List<int>();
                    for (int i = 0; i < 4; i++)
                    {
                        Point p1 = points[i];
                        Point p2 = points[(i + 1) % 4];
                        if ((p1.Y <= y && p2.Y > y) || (p2.Y <= y && p1.Y > y))
                        {
                            // Линейная интерполяция X
                            int x = p1.X + (int)((float)(y - p1.Y) / (p2.Y - p1.Y) * (p2.X - p1.X));
                            xIntersections.Add(x);
                        }
                    }
                    xIntersections.Sort();
                    // Заливаем между парами пересечений
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

        public override bool Contains(Point p)
        {
            if (points.Count < 4) return false;
            // Используем стандартный алгоритм для проверки попадания точки в выпуклый четырёхугольник
            var poly = points.ToArray();
            bool inside = false;
            for (int i = 0, j = poly.Length - 1; i < poly.Length; j = i++)
            {
                if (((poly[i].Y > p.Y) != (poly[j].Y > p.Y)) &&
                    (p.X < (poly[j].X - poly[i].X) * (p.Y - poly[i].Y) / (poly[j].Y - poly[i].Y + 0.0001) + poly[i].X))
                    inside = !inside;
            }
            return inside;
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
            if (points.Count < 4) return;
            float centerX = (float)points.Average(p => p.X);
            float centerY = (float)points.Average(p => p.Y);
            double angleRad = angle * Math.PI / 180.0;
            double cos = Math.Cos(angleRad);
            double sin = Math.Sin(angleRad);
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
            if (points.Count < 4) return;
            float centerX = (float)points.Average(p => p.X);
            float centerY = (float)points.Average(p => p.Y);
            for (int i = 0; i < points.Count; i++)
            {
                float dx = points[i].X - centerX;
                float dy = points[i].Y - centerY;
                int newX = (int)(centerX + dx * sx);
                int newY = (int)(centerY + dy * sy);
                points[i] = new Point(newX, newY);
            }
        }
    }
} 