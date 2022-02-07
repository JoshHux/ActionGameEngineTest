using System;
using FixMath.NET;
using FlatPhysics.Shapes;

namespace FlatPhysics
{
    public enum ShapeType
    {
        Circle = 0,
        Box = 1
    }

    public sealed class FlatBody
    {
        private FVector2 _position;
        private FVector2 _linearVelocity;
        private Fix64 _rotation;
        private Fix64 _rotationalVelocity;

        private FVector2 force;

        public readonly Fix64 Mass;
        public readonly Fix64 InvMass;
        public readonly Fix64 Restitution;

        public readonly bool IsStatic;
        public readonly bool IsTrigger;

        public Fix64 Radius
        {
            get { return (this._shape as Circle).Radius; }
            set
            {
                (this._shape as Circle).Radius = value;
                this.transformUpdateRequired = true;
                this.aabbUpdateRequired = true;
            }
        }

        public Fix64 Width
        {
            get { return (this._shape as Rectangle).Width; }
            set
            {
                (this._shape as Rectangle).Width = value;
                this.CreateRectVertices();
            }
        }
        public Fix64 Height
        {
            get { return (this._shape as Rectangle).Height; }
            set
            {
                (this._shape as Rectangle).Height = value;
                this.CreateRectVertices();
            }
        }

        private FVector2[] vertices;
        public readonly int[] Triangles;
        private FVector2[] transformedVertices;
        private FlatAABB aabb;

        private bool transformUpdateRequired;
        private bool aabbUpdateRequired;

        public readonly ShapeType ShapeType;

        private Shape _shape;

        public FVector2 Position
        {
            get { return this._position; }
        }

        public Shape Shape
        {
            get { return this.Shape; }
        }

        public FVector2 LinearVelocity
        {
            get { return this._linearVelocity; }
            internal set { this._linearVelocity = value; }
        }

        private FlatBody(FVector2 position, Fix64 mass, Fix64 restitution,
            bool isStatic, bool isTrigger, Fix64 radius, Fix64 width, Fix64 height, ShapeType shapeType)
        {
            this._position = position;
            this._linearVelocity = FVector2.zero;
            this._rotation = 0;
            this._rotationalVelocity = 0;

            this.force = FVector2.zero;

            this.Mass = mass;
            this.Restitution = restitution;

            this.IsStatic = isStatic;
            this.IsTrigger = isTrigger;
            //this.Radius = radius;
            //this.Width = width;
            //this.Height = height;
            this.ShapeType = shapeType;

            if (!this.IsStatic)
            {
                this.InvMass = 1 / this.Mass;
            }
            else
            {
                this.InvMass = 0;
            }

            if (this.ShapeType is ShapeType.Box)
            {
                this._shape = new Rectangle(this.Width, this.Height);
                //this.vertices = this.CreateRectVertices();
                this.Triangles = FlatBody.CreateBoxTriangles();
                this.transformedVertices = new FVector2[this.vertices.Length];

            }
            else
            {
                this._shape = new Circle(radius);
                this.vertices = null;
                Triangles = null;
                this.transformedVertices = null;

            }

            this.transformUpdateRequired = true;
            this.aabbUpdateRequired = true;
        }

        private void CreateRectVertices()
        {
            this.transformUpdateRequired = true;
            this.aabbUpdateRequired = true;
            this.vertices = FlatBody.CreateBoxVertices(this.Width, this.Height);
        }

        private static FVector2[] CreateBoxVertices(Fix64 width, Fix64 height)
        {
            Fix64 left = -width / 2;
            Fix64 right = left + width;
            Fix64 bottom = -height / 2;
            Fix64 top = bottom + height;

            FVector2[] vertices = new FVector2[4];
            vertices[0] = new FVector2(left, top);
            vertices[1] = new FVector2(right, top);
            vertices[2] = new FVector2(right, bottom);
            vertices[3] = new FVector2(left, bottom);

            return vertices;
        }

        private static int[] CreateBoxTriangles()
        {
            int[] triangles = new int[6];
            triangles[0] = 0;
            triangles[1] = 1;
            triangles[2] = 2;
            triangles[3] = 0;
            triangles[4] = 2;
            triangles[5] = 3;
            return triangles;
        }

        public FVector2[] GetTransformedVertices()
        {
            if (this.transformUpdateRequired)
            {
                FlatTransform transform = new FlatTransform(this._position, this._rotation);

                for (int i = 0; i < this.vertices.Length; i++)
                {
                    FVector2 v = this.vertices[i];
                    this.transformedVertices[i] = FVector2.Transform(v, transform);
                }
            }

            this.transformUpdateRequired = false;
            return this.transformedVertices;
        }

        public FlatAABB GetAABB()
        {
            if (this.aabbUpdateRequired)
            {
                /* Fix64 minX = Fix64.MaxValue;
                 Fix64 minY = Fix64.MaxValue;
                 Fix64 maxX = Fix64.MinValue;
                 Fix64 maxY = Fix64.MinValue;

                 if (this.ShapeType is ShapeType.Box)
                 {
                     /*
                     FVector2[] vertices = this.GetTransformedVertices();

                     for (int i = 0; i < vertices.Length; i++)
                     {
                         FVector2 v = vertices[i];

                         if (v.x < minX) { minX = v.x; }
                         if (v.x > maxX) { maxX = v.x; }
                         if (v.y < minY) { minY = v.y; }
                         if (v.y > maxY) { maxY = v.y; }
                     }

                     /*var wid = this.Width / 2;
                     var hei = this.Height / 2;


                     minX = this.position.x - wid;
                     maxX = this.position.x + wid;

                     minY = this.position.y - hei;
                     maxY = this.position.y + hei;
                     return this._shape.GetAABB(this.position);
                 }
                 else if (this.ShapeType is ShapeType.Circle)
                 {
                     /*
                     minX = this.position.x - this.Radius;
                     minY = this.position.y - this.Radius;
                     maxX = this.position.x + this.Radius;
                     maxY = this.position.y + this.Radius;

                     return this._shape.GetAABB(this.position);
                 }
                 else
                 {
                     throw new Exception("Unknown ShapeType.");
                 }

                 this.aabb = new FlatAABB(minX, minY, maxX, maxY);*/
                this.aabb = this._shape.GetAABB(this._position);
            }


            this.aabbUpdateRequired = false;
            return this.aabb;
        }

        internal void Step(Fix64 time, FVector2 gravity, int iterations)
        {
            if (this.IsStatic)
            {
                return;
            }

            time /= (Fix64)iterations;

            // force = mass * acc
            // acc = force / mass;

            //FVector2 acceleration = this.force / this.Mass;
            //this._linearVelocity += acceleration * time;


            this._linearVelocity += gravity * time;
            this._position += this._linearVelocity * time;

            //this._rotation += this._rotationalVelocity * time;

            this.force = FVector2.zero;
            this.transformUpdateRequired = true;
            this.aabbUpdateRequired = true;
        }

        public void Move(FVector2 amount)
        {
            this._position += amount;
            this.transformUpdateRequired = true;
            this.aabbUpdateRequired = true;
        }

        public void MoveTo(FVector2 position)
        {
            this._position = position;
            this.transformUpdateRequired = true;
            this.aabbUpdateRequired = true;
        }

        public void Rotate(Fix64 amount)
        {
            this._rotation += amount;
            this.transformUpdateRequired = true;
            this.aabbUpdateRequired = true;
        }

        public void AddForce(FVector2 amount)
        {
            this.force = amount;
        }

        public static bool CreateCircleBody(Fix64 radius, Fix64 mass, FVector2 position, bool isStatic, bool isTrigger, Fix64 restitution, out FlatBody body, out string errorMessage)
        {
            body = null;
            errorMessage = string.Empty;


            restitution = Fix64.Clamp(restitution, 0, 1);

            // mass = area * depth * density

            body = new FlatBody(position, mass, restitution, isStatic, isTrigger, radius, 0, 0, ShapeType.Circle);
            return true;
        }

        public static bool CreateBoxBody(Fix64 width, Fix64 height, Fix64 mass, FVector2 position, bool isStatic, bool isTrigger, Fix64 restitution, out FlatBody body, out string errorMessage)
        {
            body = null;
            errorMessage = string.Empty;

            restitution = Fix64.Clamp(restitution, 0, 1);

            // mass = area * depth * density

            body = new FlatBody(position, mass, restitution, isStatic, isTrigger, 0, width, height, ShapeType.Box);
            return true;
        }
    }
}
