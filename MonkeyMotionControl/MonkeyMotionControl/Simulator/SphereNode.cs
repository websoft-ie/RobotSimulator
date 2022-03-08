using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Model.Scene;
using SharpDX;
using Transform3D = System.Windows.Media.Media3D.Transform3D;

namespace MonkeyMotionControl.Simulator
{
    public class SphereNode : MeshNode
    {
        public delegate void PositionChangedEventHandler(object sender, TransformArgs args);
        public event PositionChangedEventHandler PositionChangedEvent;
        

        protected Vector3 PreviousPos { get; set; } = Vector3.Zero;

        private Transform3D _transform;
        public Transform3D Transform
        {
            get
            {
                return _transform;
            }
            set
            {
                _transform = value;
                UpdateModelTransform();
            }
        }

        public bool IsPositionChanged
        {
            get
            {
                if ((Position.X == PreviousPos.X) && (Position.Y == PreviousPos.Y) && (Position.Z == PreviousPos.Z))
                    return false;
                else
                    return true;
            }
        }

        private PointManipulator3D _manipulator;
        public PointManipulator3D Manipulator 
        {
            get 
            {
                return _manipulator;
            }
            set
            {
                _manipulator = value;
                _manipulator.Position = Position;
            }
        }

        public Vector3 Position
        {
            get
            {
                return ModelMatrix.TranslationVector;
            }
            set
            {
                translationVector = value;
                UpdateModelMatrix();
                UpdateManipulator();
            }
        }

        private float _radiusLength;
        public float Radius
        {
            get
            {
                return _radiusLength;
            }
            set
            {
                _radiusLength = value;
                scaleMatrix = Matrix.Scaling(value);
                UpdateModelMatrix();
            }
        }

        private Color _color;
        public Color Color
        {
            get
            {
                return _color;
            }
            set
            {
                _color = value;
                var material = new PhongMaterial() { DiffuseColor = value };
                Material = material;
                if (material.DiffuseColor.Alpha < 1)
                {
                    IsTransparent = true;
                }
            }
        }

        private Vector3 centerOffset = Vector3.Zero;
        protected Vector3 translationVector = Vector3.Zero;
        private Matrix rotationMatrix = Matrix.Identity;
        private Matrix scaleMatrix = Matrix.Identity;

        public SphereNode()
            : base()
        {

        }

        public SphereNode(Vector3 center, float radius, Color color)
            : base()
        {
            var builder = new MeshBuilder();
            builder.AddSphere(Vector3.Zero, radius);
            Geometry = builder.ToMeshGeometry3D();
            CullMode = SharpDX.Direct3D11.CullMode.Back;
            Color = color;
            _radiusLength = radius;
            Position = center;
            PreviousPos = center;
        }

        private void UpdateModelTransform()
        {
            if (Transform != null)
            {
                ModelMatrix = Transform.ToMatrix();
            }
        }

        protected void UpdateModelMatrix()
        {
            PreviousPos = translationVector;
            ModelMatrix = Matrix.Translation(-centerOffset) * scaleMatrix * rotationMatrix * Matrix.Translation(centerOffset) * Matrix.Translation(translationVector);
            PositionChangedEvent?.Invoke(this, new TransformArgs(ModelMatrix));
        }

        //private void OnUpdateSelfTransform()
        //{
        //    var m = Matrix.Translation(centerOffset + translationVector);
        //    m.M11 = m.M22 = m.M33 = (float)sizeScale;
        //    //ctrlGroup.Transform = new Media3D.MatrixTransform3D(m.ToMatrix3D());
        //}

        private void UpdateManipulator()
        {
            if (Manipulator == null)
            {
                return;
            }
            if (!Manipulator.IsCaptured)
            {
                //TODO FIX 

                //var m = Matrix.Translation(-centerOffset) * scaleMatrix * rotationMatrix * Matrix.Translation(centerOffset) * Matrix.Translation(translationVector);
                //Manipulator.Transform = new System.Windows.Media.Media3D.MatrixTransform3D(m.ToMatrix3D());

                //var m = Matrix.Translation(centerOffset) * Matrix.Translation(translationVector);
                //Manipulator.SceneNode.ModelMatrix = m;
                //Manipulator.SceneNode.ModelMatrix = Matrix.Translation(-centerOffset) * scaleMatrix * rotationMatrix * Matrix.Translation(centerOffset) * Matrix.Translation(translationVector);
                //Manipulator.Position = Position;
            }
        }
    }
}
