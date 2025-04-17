using DrawingEditor.Shapes;

namespace DrawingEditor.Core
{
    public enum EditorState
    {
        Draw,
        Edit,
        Transform
    }
    public class Canvas
    {
        private List<Shape> shapes;
        private EditorState currentState;
        private Shape currentShape;

        public Canvas()
        {
            shapes = new List<Shape>();
            currentState = EditorState.Draw;
        }

        public void SetState(EditorState state)
        {
            currentState = state;
        }

        public void HandleMouseDown(Point location)
        {
            switch (currentState)
            {
                case EditorState.Draw:
                    // Логика создания новой фигуры
                    break;
                case EditorState.Edit:
                    // Логика редактирования существующей фигуры
                    break;
                case EditorState.Transform:
                    // Логика трансформации фигуры
                    break;
            }
        }
    }
    
}