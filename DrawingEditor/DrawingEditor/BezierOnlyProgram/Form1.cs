namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
        }


        private List<Point> _points = new List<Point>();

        private int _dotRadius = 5;
        private int curveBold = 1;

        private void Form1_Paint_1(object sender, PaintEventArgs e)
        {
            foreach (Point p in _points)
            {
                e.Graphics.DrawImageUnscaled(Dot("Black", _dotRadius), p.X - _dotRadius, p.Y - _dotRadius);
            }

            if (_points.Count < 4) return;

            e.Graphics.DrawImageUnscaled(BezierCurve("Blue", "Black", curveBold), 0, 0);
        }

        private Bitmap Dot(string color, int radius)
        {
            Bitmap map = new Bitmap(radius * 2, radius * 2);

            for (int x = -radius; x < radius; x++)
            {
                for (int y = -radius; y < radius; y++)
                {
                    if ((x * x + y * y) < (radius * radius))
                    {
                        map.SetPixel(x + radius, y + radius, Color.FromName(color));
                    }
                }
            }
            return map;
        }

        //private void Form1_MouseClick(object sender, MouseEventArgs e)
        //{
        //    _points.Add(e.Location);
        //    Refresh();
        //}

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            _points.Add(e.Location);
            _points.Add(e.Location);
            _points.Add(e.Location);
            Refresh();
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Point p = _points[^2];
                _points[^1] = e.Location;
                _points[^3] = new Point(p.X - (e.Location.X - p.X), p.Y - (e.Location.Y - p.Y));
                Refresh();
            }
        }

        private Bitmap BezierCurve(string curveColor, string adColor, int bold)
        {
            Bitmap map = new Bitmap(1920, 1080);
            for (int i = 1; i < _points.Count - 2; i+=3)
            {
                for (double t = 0; t <= 1; t += 0.001)
                {
                    Point a = Lerp(_points[i], _points[i + 1], t);
                    Point b = Lerp(_points[i + 1], _points[i + 2], t);
                    Point c = Lerp(_points[i + 2], _points[i + 3], t);
                    Point ab = Lerp(a, b, t);
                    Point bc = Lerp(b, c, t);
                    Point abc = Lerp(ab, bc, t);
                    for (int x = -bold; x <= bold; x++)
                    {
                        for (int y = -bold; y <= bold; y++)
                        {
                            map.SetPixel(abc.X + x, abc.Y + y, Color.FromName(curveColor));
                        }
                    }
                    map.SetPixel(a.X, a.Y, Color.FromName(adColor));
                    map.SetPixel(c.X, c.Y, Color.FromName(adColor));
                }
            }
            return map;
        }

        private Point Lerp(Point a, Point b, double t) 
        {
            return new Point(Convert.ToInt32(a.X * (1 - t) + b.X * t), Convert.ToInt32(a.Y * (1 - t) + b.Y * t));
        }
    }
}
