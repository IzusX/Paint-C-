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
    }
}
