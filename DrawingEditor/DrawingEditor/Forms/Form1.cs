using System.Drawing;
using System.Windows.Forms;
using DrawingEditor.Shapes;
using DrawingEditor.Interfaces;
using System.Linq;
using System;

namespace DrawingEditor
{
    public enum ToolType
    {
        Line,
        Rectangle,
        Ellipse,
        BezierCurve,
        Polygon,
        Polyline
    }

    public enum EditMode
    {
        Draw,       // Режим рисования
        Select,     // Режим выделения
        Move,       // Режим перемещения
        Rotate,     // Режим поворота
        Scale       // Режим масштабирования
    }

    public partial class Form1 : Form
    {
        private List<Shape> shapes;
        private Shape currentShape;
        private bool isDrawing;
        private Point startPoint;
        
        // Текущие настройки рисования
        private Color currentStrokeColor = Color.Black;
        private Color currentFillColor = Color.Transparent;
        private int currentStrokeWidth = 1;
        private ToolType currentTool = ToolType.Line;

        // Добавляем элементы управления
        private ToolStrip toolStrip;
        private ColorDialog colorDialog;

        private bool isBezierDrawing = false;

        private EditMode currentMode = EditMode.Draw;
        private Shape selectedShape = null;
        private Point lastMousePos;
        private bool isDragging = false;
        private float rotationAngle = 0;
        private float scaleFactorX = 1.0f;
        private float scaleFactorY = 1.0f;

        public Form1()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            shapes = new List<Shape>();
            isDrawing = false;

            InitializeToolStrip();

            this.Paint += Form1_Paint;
            this.MouseDown += Form1_MouseDown;
            this.MouseMove += Form1_MouseMove;
            this.MouseUp += Form1_MouseUp;
        }

        private void InitializeToolStrip()
        {
            // Создаем панель инструментов
            toolStrip = new ToolStrip();
            toolStrip.Dock = DockStyle.Top;

            // Кнопки для выбора фигур
            var lineButton = new ToolStripButton("Line", null, (s, e) => currentTool = ToolType.Line);
            var rectangleButton = new ToolStripButton("Rectangle", null, (s, e) => currentTool = ToolType.Rectangle);
            var ellipseButton = new ToolStripButton("Ellipse", null, (s, e) => currentTool = ToolType.Ellipse);
            var bezierButton = new ToolStripButton("Bezier", null, (s, e) => currentTool = ToolType.BezierCurve);

            // Разделитель
            var separator1 = new ToolStripSeparator();

            // Кнопки для выбора цветов
            var strokeColorButton = new ToolStripButton("Stroke Color", null, StrokeColorButton_Click);
            var fillColorButton = new ToolStripButton("Fill Color", null, FillColorButton_Click);

            // Разделитель
            var separator2 = new ToolStripSeparator();

            // Выбор толщины линии
            var strokeWidthLabel = new ToolStripLabel("Width:");
            var strokeWidthCombo = new ToolStripComboBox();
            strokeWidthCombo.Items.AddRange(new object[] { "1", "2", "3", "4", "5" });
            strokeWidthCombo.SelectedIndex = 0;
            strokeWidthCombo.SelectedIndexChanged += (s, e) => 
            {
                if (int.TryParse(strokeWidthCombo.SelectedItem.ToString(), out int width))
                {
                    currentStrokeWidth = width;
                }
            };

            // Добавляем элементы для многоугольника
            var polygonButton = new ToolStripButton("Polygon", null, (s, e) => currentTool = ToolType.Polygon);
            var sidesLabel = new ToolStripLabel("Sides:");
            var sidesNumeric = new ToolStripComboBox();
            sidesNumeric.Name = "sidesNumeric";
            sidesNumeric.Items.AddRange(new object[] { "3", "4", "5", "6", "7", "8" });
            sidesNumeric.SelectedIndex = 0;

            // Добавляем кнопку для ломаной
            var polylineButton = new ToolStripButton("Polyline", null, (s, e) => currentTool = ToolType.Polyline);

            // Добавляем разделитель и кнопки режимов
            var separator3 = new ToolStripSeparator();
            
            var drawButton = new ToolStripButton("Draw", null, (s, e) => 
            {
                currentMode = EditMode.Draw;
                selectedShape = null;
                Refresh();
            });
            drawButton.Checked = true;

            var selectButton = new ToolStripButton("Select", null, (s, e) => 
            {
                currentMode = EditMode.Select;
                Refresh();
            });

            var moveButton = new ToolStripButton("Move", null, (s, e) => 
            {
                currentMode = EditMode.Move;
                if (selectedShape == null) currentMode = EditMode.Select;
            });

            var rotateButton = new ToolStripButton("Rotate", null, (s, e) => 
            {
                currentMode = EditMode.Rotate;
                if (selectedShape == null) currentMode = EditMode.Select;
            });

            var scaleButton = new ToolStripButton("Scale", null, (s, e) => 
            {
                currentMode = EditMode.Scale;
                if (selectedShape == null) currentMode = EditMode.Select;
            });

            var deleteButton = new ToolStripButton("Delete", null, (s, e) => 
            {
                if (selectedShape != null)
                {
                    shapes.Remove(selectedShape);
                    selectedShape = null;
                    Refresh();
                }
            });

            // Добавляем все элементы на панель
            toolStrip.Items.AddRange(new ToolStripItem[]
            {
                lineButton,
                rectangleButton,
                ellipseButton,
                bezierButton,
                polylineButton,
                separator1,
                strokeColorButton,
                fillColorButton,
                separator2,
                strokeWidthLabel,
                strokeWidthCombo,
                polygonButton,
                sidesLabel,
                sidesNumeric,
                separator3,
                drawButton,
                selectButton,
                moveButton,
                rotateButton,
                scaleButton,
                deleteButton
            });

            // Добавляем панель на форму
            this.Controls.Add(toolStrip);

            // Инициализируем диалог выбора цвета
            colorDialog = new ColorDialog();
        }

        private void StrokeColorButton_Click(object sender, EventArgs e)
        {
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                currentStrokeColor = colorDialog.Color;
            }
        }

        private void FillColorButton_Click(object sender, EventArgs e)
        {
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                currentFillColor = colorDialog.Color;
            }
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            foreach (var shape in shapes)
            {
                shape.Draw(e.Graphics);
                
                // Рисуем рамку выделения для выбранной фигуры
                if (shape == selectedShape)
                {
                    var bounds = GetShapeBounds(shape);
                    using (Pen pen = new Pen(Color.Blue, 1))
                    {
                        pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                        e.Graphics.DrawRectangle(pen, bounds);
                    }
                }
            }

            if (currentShape != null)
            {
                currentShape.Draw(e.Graphics);
            }
        }

        private void CreateShape()
        {
            switch (currentTool)
            {
                case ToolType.Line:
                    currentShape = new Line();
                    break;
                case ToolType.Rectangle:
                    currentShape = new RectangleShape();
                    break;
                case ToolType.Ellipse:
                    currentShape = new Ellipse();
                    break;
                case ToolType.BezierCurve:
                    currentShape = new BezierCurve();
                    break;
                case ToolType.Polygon:
                    var sidesCombo = toolStrip.Items
                        .OfType<ToolStripComboBox>()
                        .FirstOrDefault(item => item.Name == "sidesNumeric");
                    int sides = 3; // значение по умолчанию
                    if (sidesCombo != null && sidesCombo.SelectedItem != null)
                    {
                        sides = int.Parse(sidesCombo.SelectedItem.ToString());
                    }
                    currentShape = new Polygon(sides);
                    break;
                case ToolType.Polyline:
                    currentShape = new Polyline();
                    break;
            }

            currentShape.StrokeColor = currentStrokeColor;
            currentShape.FillColor = currentFillColor;
            currentShape.StrokeWidth = currentStrokeWidth;
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            lastMousePos = e.Location;

            if (e.Button == MouseButtons.Left)
            {
                switch (currentMode)
                {
                    case EditMode.Draw:
                        isDrawing = true; // Ставим isDrawing в true для всех фигур в режиме Draw
                        CreateShape();
                        currentShape.IsDrawing = true; // Устанавливаем и для текущей фигуры

                        if (currentTool == ToolType.Polyline)
                        {
                            // Логика для Polyline остается почти такой же
                            if (!(currentShape is Polyline) || !((Polyline)currentShape).IsDrawingPolyline) // Проверяем, не рисуется ли уже
                            {
                                // Начинаем новую ломаную, если это первый клик для нее
                                currentShape.AddPoint(e.Location); // Первая точка
                                ((Polyline)currentShape).IsDrawingPolyline = true;
                            }
                            else
                            {
                                currentShape.AddPoint(e.Location); // Добавляем новую точку
                            }
                        }
                        else if (currentTool == ToolType.BezierCurve)
                        {
                            if (!isBezierDrawing)
                            {
                                // Начинаем новую кривую Безье
                                isBezierDrawing = true;
                                CreateShape();
                                currentShape.IsDrawing = true;
                                // Добавляем точки в правильном порядке:
                                //currentShape.AddPoint(Point.Empty); // Пустая точка для индексации с 1
                                currentShape.AddPoint(e.Location); // Первая опорная точка (i=1)
                                currentShape.AddPoint(e.Location); // Первая контрольная точка
                                currentShape.AddPoint(e.Location); // Вторая контрольная точка
                                //currentShape.AddPoint(e.Location); // Конечная точка
                            }
                            else
                            {
                                // Добавляем новый сегмент, начиная с опорной точки
                                currentShape.AddPoint(e.Location); // Новая опорная точка
                                currentShape.AddPoint(e.Location); // Новая контрольная точка 1
                                currentShape.AddPoint(e.Location); // Новая контрольная точка 2
                                //currentShape.AddPoint(e.Location); // Новая конечная точка
                            }
                        }
                        else if (currentTool == ToolType.Polygon)
                        {
                            currentShape.AddPoint(e.Location); // Первая точка - центр
                            // Добавляем "пустышку" для второй точки, чтобы UpdatePoint работал
                            // Эта точка будет обновляться в MouseMove
                            // currentShape.AddPoint(e.Location); 
                            // Вместо добавления второй точки здесь, она будет определяться в MouseMove
                            // и использоваться для UpdatePoint
                        }
                        else
                        {
                            // Обычное рисование для других фигур (Line, RectangleShape, Ellipse)
                            startPoint = e.Location;
                            currentShape.AddPoint(startPoint);
                            currentShape.AddPoint(startPoint); // Добавляем вторую точку для UpdatePoint
                        }
                        Refresh();
                        break;

                    case EditMode.Select:
                        selectedShape = null;
                        for (int i = shapes.Count - 1; i >= 0; i--)
                        {
                            if (shapes[i].Contains(e.Location))
                            {
                                selectedShape = shapes[i];
                                break;
                            }
                        }
                        Refresh();
                        break;

                    case EditMode.Move:
                    case EditMode.Rotate:
                    case EditMode.Scale:
                        if (selectedShape != null)
                        {
                            isDragging = true;
                        }
                        break;
                }
            }
            else if (e.Button == MouseButtons.Right)
            {
                if (currentTool == ToolType.Polyline && currentShape is Polyline polyline && polyline.IsDrawingPolyline)
                {
                    polyline.IsDrawingPolyline = false; // Завершаем рисование ломаной
                    if (currentShape.Points.Count >= 2) 
                    {
                        shapes.Add(currentShape);
                    }
                    currentShape = null;
                    isDrawing = false;
                    Refresh();
                }
                else if (currentTool == ToolType.BezierCurve && isBezierDrawing)
                {
                    // Завершаем рисование кривой Безье
                    currentShape.IsDrawing = false;
                    shapes.Add(currentShape);
                    currentShape = null;
                    isBezierDrawing = false;
                    Refresh();
                }
            }
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDrawing && currentShape != null && currentMode == EditMode.Draw)
            {
                if (currentTool == ToolType.Polygon)
                {
                    // Для полигона, если interactionCenter уже задан (после первого клика)
                    // обновляем его радиус на основе текущей позиции мыши
                    // currentShape.UpdatePoint(1, e.Location); // Передаем условный индекс 1
                    // Вместо UpdatePoint(1,...) напрямую вызываем AddPoint, 
                    // который в Polygon теперь обрабатывает вторую точку для радиуса
                    if (currentShape.Points.Count == ((Polygon)currentShape).SidesCount && ((Polygon)currentShape).InteractionCenter != null) // Убедимся, что центр уже есть
                    {
                       ((Polygon)currentShape).AddPoint(e.Location); // Это обновит interactionRadius и временные вершины
                    }
                }
                else if (currentTool == ToolType.Polyline)
                {
                    // ... (логика для Polyline)
                }
                else if (currentTool == ToolType.BezierCurve)
                {
                    if (e.Button == MouseButtons.Left)
                    {
                        ((BezierCurve)currentShape).UpdateControlPoints(e.Location);
                    }
                }
                else
                {
                    // Для других фигур (Line, RectangleShape, Ellipse)
                    currentShape.UpdatePoint(1, e.Location);
                }
                Refresh();
            }
            else if (isDragging && selectedShape != null)
            {
                int dx = e.X - lastMousePos.X;
                int dy = e.Y - lastMousePos.Y;

                switch (currentMode)
                {
                    case EditMode.Move:
                        selectedShape.Move(dx, dy);
                        break;

                    case EditMode.Rotate:
                        // Вычисляем угол поворота относительно центра фигуры
                        Point center = new Point(
                            (int)selectedShape.Points.Average(p => p.X),
                            (int)selectedShape.Points.Average(p => p.Y)
                        );
                        
                        // Вычисляем угол между предыдущей и текущей позицией мыши
                        double prevAngle = Math.Atan2(lastMousePos.Y - center.Y, lastMousePos.X - center.X);
                        double currentAngle = Math.Atan2(e.Y - center.Y, e.X - center.X);
                        float angleDelta = (float)((currentAngle - prevAngle) * 180.0 / Math.PI);
                        
                        ((ITransformable)selectedShape).Rotate(angleDelta);
                        break;

                    case EditMode.Scale:
                        // Вычисляем коэффициенты масштабирования
                        float sx = 1.0f + dx / 100.0f;
                        float sy = 1.0f + dy / 100.0f;
                        ((ITransformable)selectedShape).Scale(sx, sy);
                        break;
                }

                lastMousePos = e.Location;
                Refresh();
            }
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            if (isDrawing && currentShape != null && currentMode == EditMode.Draw)
            {
                currentShape.IsDrawing = false; // Завершаем флаг рисования для фигуры

                if (currentTool == ToolType.Polygon)
                {
                    ((Polygon)currentShape).FinalizeShape();
                    if (currentShape.Points.Count == ((Polygon)currentShape).SidesCount && !currentShape.Points.Any(p => p.IsEmpty)) 
                    {
                        shapes.Add(currentShape);
                    }
                }
                else if (currentTool == ToolType.Polyline)
                {
                    // Для полилайна завершение происходит по правой кнопке
                    // Здесь можно ничего не делать, или если нужно добавить последнюю точку, то добавить
                    // но обычно MouseUp левой кнопки не завершает полилайн.
                    return; // Возвращаемся, чтобы не сбросить currentShape и isDrawing
                }
                else if (currentTool == ToolType.BezierCurve)
                {
                    // ... (логика для BezierCurve, возможно, тоже FinalizeShape)
                }
                else
                {
                     // Для Line, RectangleShape, Ellipse
                    currentShape.UpdatePoint(1, e.Location); // Обновляем последнюю точку
                    shapes.Add(currentShape);
                }
                
                currentShape = null;
                isDrawing = false; // Сбрасываем общий флаг рисования
                Refresh();
            }
            isDragging = false; // Сбрасываем флаг перетаскивания в любом случае
        }

        private System.Drawing.Rectangle GetShapeBounds(Shape shape)
        {
            // Для эллипса ищем bounding box по всем точкам контура с учётом поворота
            if (shape is DrawingEditor.Shapes.Ellipse ellipse && ellipse.Points.Count >= 2)
            {
                var startPoint = ellipse.Points[0];
                var endPoint = ellipse.Points[1];
                int centerX = (startPoint.X + endPoint.X) / 2;
                int centerY = (startPoint.Y + endPoint.Y) / 2;
                int radiusX = Math.Abs(endPoint.X - startPoint.X) / 2;
                int radiusY = Math.Abs(endPoint.Y - startPoint.Y) / 2;
                float rotationAngle = ellipse.GetType().GetField("rotationAngle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(ellipse) is float a ? a : 0f;
                double angleRad = rotationAngle * Math.PI / 180.0;
                List<Point> ellipseContour = new List<Point>();
                int steps = (int)(2 * Math.PI * Math.Max(radiusX, radiusY));
                if (steps < 60) steps = 60;
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
                int minX = ellipseContour.Min(p => p.X);
                int maxX = ellipseContour.Max(p => p.X);
                int minY = ellipseContour.Min(p => p.Y);
                int maxY = ellipseContour.Max(p => p.Y);
                return System.Drawing.Rectangle.FromLTRB(minX - 5, minY - 5, maxX + 5, maxY + 5);
            }
            // Для остальных фигур — по Points
            int minX2 = shape.Points.Min(p => p.X);
            int minY2 = shape.Points.Min(p => p.Y);
            int maxX2 = shape.Points.Max(p => p.X);
            int maxY2 = shape.Points.Max(p => p.Y);
            return System.Drawing.Rectangle.FromLTRB(minX2 - 5, minY2 - 5, maxX2 + 5, maxY2 + 5);
        }
    }
}