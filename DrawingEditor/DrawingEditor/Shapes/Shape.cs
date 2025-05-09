using System.Drawing;

namespace DrawingEditor.Shapes
{
    public abstract class Shape
    {
        protected List<Point> points;
        protected Color strokeColor;
        protected Color fillColor;
        protected int strokeWidth;
        protected bool isDrawing;

        public Shape()
        {
            points = new List<Point>();
            strokeColor = Color.Black;
            fillColor = Color.Transparent;
            strokeWidth = 1;
        }

        // Свойства для работы с цветами и толщиной линии
        public Color StrokeColor
        {
            get => strokeColor;
            set
            {
                strokeColor = value;
            }
        }

        public Color FillColor
        {
            get => fillColor;
            set
            {
                fillColor = value;
            }
        }

        public int StrokeWidth
        {
            get => strokeWidth;
            set
            {
                strokeWidth = value > 0 ? value : 1;
            }
        }

        // Добавляем публичное свойство
        public bool IsDrawing
        {
            get => isDrawing;
            set => isDrawing = value;
        }

        public IList<Point> Points => points;

        // Общий метод для добавления точек
        public virtual void AddPoint(Point point)
        {
            points.Add(point);
        }

        // Абстрактные методы, которые должны реализовать все фигуры
        public abstract void Draw(Graphics g);
        public abstract bool Contains(Point p);
        public abstract void Move(int dx, int dy);

        public virtual void UpdatePoint(int index, Point newLocation)
        {
            if (index >= 0 && index < points.Count)
            {
                points[index] = newLocation;
            }
        }

        // Ручная отрисовка линии (алгоритм Брезенхема)
        protected void DrawLineBresenham(Graphics g, Point p1, Point p2, Color color, int width)
        {
            int x1 = p1.X, y1 = p1.Y, x2 = p2.X, y2 = p2.Y;
            int dx = Math.Abs(x2 - x1);
            int dy = Math.Abs(y2 - y1);
            int sx = x1 < x2 ? 1 : -1;
            int sy = y1 < y2 ? 1 : -1;
            int err = dx - dy;

            while (true)
            {
                for (int w = -width / 2; w <= width / 2; w++)
                {
                    for (int h = -width / 2; h <= width / 2; h++)
                    {
                        g.FillRectangle(new SolidBrush(color), x1 + w, y1 + h, 1, 1);
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
    }
}
