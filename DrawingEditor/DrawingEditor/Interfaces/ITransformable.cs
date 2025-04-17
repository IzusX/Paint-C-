using System.Drawing;

namespace DrawingEditor.Interfaces
{
    public interface ITransformable
    {
        void Rotate(float angle);
        void Scale(float sx, float sy);
    }
}
