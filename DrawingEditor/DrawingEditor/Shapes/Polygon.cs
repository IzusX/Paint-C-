using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using DrawingEditor.Interfaces;

namespace DrawingEditor.Shapes
{
    public class Polygon : Shape, ITransformable
    {
        private int sides;
        // Храним центр и радиус отдельно для интерактивного рисования
        private Point? interactionCenter = null;
        private double interactionRadius = 0;

        // Публичные геттеры для доступа из Form1
        public int SidesCount => sides;
        public Point? InteractionCenter => interactionCenter;

        public Polygon(int numberOfSides = 3)
        {
            points = new List<Point>(); // Будут храниться только вершины
            strokeColor = Color.Black;
            fillColor = Color.Transparent;
            strokeWidth = 1;
            sides = Math.Max(3, numberOfSides);
        }

        public override void AddPoint(Point p)
        {
            if (interactionCenter == null)
            {
                interactionCenter = p; // Первая точка - центр
                // Добавляем временные точки, чтобы currentShape.Points не был пустым в Form1
                // Эти точки будут заменены на реальные вершины в FinalizeShape
                for(int i = 0; i < sides; i++) points.Add(Point.Empty);
            }
            else
            {
                // Вторая точка определяет радиус
                interactionRadius = Math.Sqrt(Math.Pow(p.X - interactionCenter.Value.X, 2) + Math.Pow(p.Y - interactionCenter.Value.Y, 2));
                // Обновляем "временные" вершины для отрисовки в MouseMove
                UpdateVerticesForDrawing(interactionCenter.Value, interactionRadius);
            }
        }

        // Обновляет точки в списке points для предварительной отрисовки
        private void UpdateVerticesForDrawing(Point center, double radius)
        {
            List<Point> tempVertices = CalculateVertices(center, radius);
            for (int i = 0; i < sides; i++)
            {
                if (i < points.Count) points[i] = tempVertices[i];
                else points.Add(tempVertices[i]);
            }
        }

        // Вызывается при MouseUp для окончательного формирования вершин
        public void FinalizeShape()
        {
            if (interactionCenter != null && interactionRadius > 0)
            {
                points.Clear();
                points.AddRange(CalculateVertices(interactionCenter.Value, interactionRadius));
            }
            else if (interactionCenter != null && points.Count == sides && points.All(pt => pt.IsEmpty))
            {
                // Если радиус так и не был задан (например, два клика в одну точку)
                // создаем маленький полигон по умолчанию вокруг центра
                points.Clear();
                points.AddRange(CalculateVertices(interactionCenter.Value, 5)); // радиус по умолчанию 5
            }
        }

        public override void UpdatePoint(int index, Point newLocation)
        {
            // При интерактивном рисовании (MouseDrag) обновляем радиус и временные вершины
            if (interactionCenter != null && index == 1) // index 1 условно для второй точки
            {
                interactionRadius = Math.Sqrt(Math.Pow(newLocation.X - interactionCenter.Value.X, 2) + Math.Pow(newLocation.Y - interactionCenter.Value.Y, 2));
                UpdateVerticesForDrawing(interactionCenter.Value, interactionRadius);
            }
        }

        public override void Draw(Graphics g)
        {
            // Рисуем, только если есть все вершины (или временные вершины)
            if (points.Count != sides || points.Any(p => p.IsEmpty && interactionCenter == null)) return;

            List<Point> pointsToDraw = points;
            if (interactionCenter != null && points.All(pt => pt.IsEmpty || !IsDrawing))
            {
                 // Если IsDrawing и точки еще не финализированы, используем временные из interactionRadius
                 // Либо если interactionRadius == 0, но центр есть (например, один клик)
                if (IsDrawing && interactionRadius > 0) {
                    pointsToDraw = CalculateVertices(interactionCenter.Value, interactionRadius > 0 ? interactionRadius : 5 );
                } else if (!IsDrawing && points.All(pt => pt.IsEmpty) && interactionCenter != null) {
                     // Случай когда фигура добавлена в shapes, но была создана одним кликом
                    pointsToDraw = CalculateVertices(interactionCenter.Value, 5 );
                } else if (IsDrawing && interactionRadius == 0 && interactionCenter !=null) {
                    // Один клик, ничего не рисуем пока, но объект создан
                    return;
                }
            }

            if (fillColor != Color.Transparent)
            {
                FillPolygon(g, pointsToDraw);
            }
            for (int i = 0; i < pointsToDraw.Count; i++)
            {
                Point start = pointsToDraw[i];
                Point end = pointsToDraw[(i + 1) % pointsToDraw.Count];
                DrawLineBresenham(g, start, end, strokeColor, strokeWidth);
            }
        }

        private List<Point> CalculateVertices(Point center, double radius)
        {
            List<Point> vertices = new List<Point>();
            double angleStep = Math.PI * 2 / sides;
            double startAngle = -Math.PI / 2; // Начать с верхней точки
            for (int i = 0; i < sides; i++)
            {
                double currentAngle = startAngle + angleStep * i;
                int x = (int)Math.Round(center.X + radius * Math.Cos(currentAngle));
                int y = (int)Math.Round(center.Y + radius * Math.Sin(currentAngle));
                vertices.Add(new Point(x, y));
            }
            return vertices;
        }

        private void FillPolygon(Graphics g, List<Point> vertices)
        {
            if (vertices.Count < 3) return;
            int minY = vertices.Min(p => p.Y);
            int maxY = vertices.Max(p => p.Y);
            for (int y = minY; y <= maxY; y++)
            {
                List<int> xIntersections = new List<int>();
                for (int i = 0; i < vertices.Count; i++)
                {
                    Point p1 = vertices[i];
                    Point p2 = vertices[(i + 1) % vertices.Count];
                    if ((p1.Y <= y && p2.Y > y) || (p2.Y <= y && p1.Y > y))
                    {
                        if (p1.Y == p2.Y) continue; 
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

        public override bool Contains(Point p)
        {
            // Используем актуальные вершины для проверки
            List<Point> currentVertices = points;
            if (interactionCenter != null && IsDrawing && interactionRadius > 0) {
                 currentVertices = CalculateVertices(interactionCenter.Value, interactionRadius);
            } else if (interactionCenter != null && points.All(pt => pt.IsEmpty)) {
                 currentVertices = CalculateVertices(interactionCenter.Value, 5);
            }

            if (currentVertices.Count != sides || currentVertices.Any(pt => pt.IsEmpty)) return false;

            bool inside = false;
            for (int i = 0, j = currentVertices.Count - 1; i < currentVertices.Count; j = i++)
            {
                if (((currentVertices[i].Y > p.Y) != (currentVertices[j].Y > p.Y)) &&
                    (p.X < (currentVertices[j].X - currentVertices[i].X) * (p.Y - currentVertices[i].Y) / (currentVertices[j].Y - currentVertices[i].Y + 0.00001) + currentVertices[i].X))
                {
                    inside = !inside;
                }
            }
            return inside;
        }

        public override void Move(int dx, int dy)
        {
            // При перемещении двигаем сохраненный центр и все вершины
            if (interactionCenter != null) {
                interactionCenter = new Point(interactionCenter.Value.X + dx, interactionCenter.Value.Y + dy);
            }
            for (int i = 0; i < points.Count; i++)
            {
                points[i] = new Point(points[i].X + dx, points[i].Y + dy);
            }
        }

        public void Rotate(float angle)
        {
            if (points.Count != sides || points.Any(p => p.IsEmpty)) return;
            Point center = new Point((int)points.Average(p => p.X), (int)points.Average(p => p.Y));
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
            // Если поворачиваем уже созданный полигон, нужно обновить и interactionCenter, если он есть
            // и если он совпадает с геометрическим центром. Для простоты пока не делаем.
        }

        public void Scale(float sx, float sy)
        {
            if (points.Count != sides || points.Any(p => p.IsEmpty)) return;
            Point center = new Point((int)points.Average(p => p.X), (int)points.Average(p => p.Y));
            for (int i = 0; i < points.Count; i++)
            {
                points[i] = new Point(
                    center.X + (int)((points[i].X - center.X) * sx),
                    center.Y + (int)((points[i].Y - center.Y) * sy)
                );
            }
            // Аналогично Rotate, нужно подумать про interactionCenter и interactionRadius при масштабировании
        }
    }
}
