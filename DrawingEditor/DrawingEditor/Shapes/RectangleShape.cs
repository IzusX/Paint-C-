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

        public override void Draw(Graphics g)
        {
            if (points.Count < 2) return;
            
            // Добавим реализацию метода Draw
            int x = Math.Min(points[0].X, points[1].X);
            int y = Math.Min(points[0].Y, points[1].Y);
            int width = Math.Abs(points[1].X - points[0].X);
            int height = Math.Abs(points[1].Y - points[0].Y);

            // Рисуем контур
            using (Pen pen = new Pen(strokeColor, strokeWidth))
            {
                g.DrawRectangle(pen, x, y, width, height);
            }

            // Если есть заливка
            if (fillColor != Color.Transparent)
            {
                using (Brush brush = new SolidBrush(fillColor))
                {
                    g.FillRectangle(brush, x, y, width, height);
                }
            }
        }

        public override bool Contains(Point p)
        {
            if (points.Count < 2) return false;

            int x = Math.Min(points[0].X, points[1].X);
            int y = Math.Min(points[0].Y, points[1].Y);
            int width = Math.Abs(points[1].X - points[0].X);
            int height = Math.Abs(points[1].Y - points[0].Y);

            return p.X >= x && p.X <= x + width && p.Y >= y && p.Y <= y + height;
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
            // Находим центр прямоугольника
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