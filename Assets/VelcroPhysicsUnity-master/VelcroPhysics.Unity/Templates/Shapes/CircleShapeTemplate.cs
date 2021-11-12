using VelcroPhysics.Collision.Shapes;
using FixMath.NET;

namespace VelcroPhysics.Templates.Shapes
{
    public class CircleShapeTemplate : ShapeTemplate
    {
        public CircleShapeTemplate() : base(ShapeType.Circle)
        {
        }

        /// <summary>
        /// Get or set the position of the circle
        /// </summary>
        public FVector2 Position { get; set; }
    }
}