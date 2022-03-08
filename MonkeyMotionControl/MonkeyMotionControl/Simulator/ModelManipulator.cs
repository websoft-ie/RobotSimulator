using HelixToolkit.Wpf.SharpDX;
using SharpDX;

namespace MonkeyMotionControl.Simulator
{
    public class ModelManipulator : TransformManipulator3D
    {
        private Vector3 position;
        public Vector3 Position
        {
            get
            {
                return position;
            }

            set
            {
                position = value;
                SceneNode.ModelMatrix = Matrix.Translation(value);
            }
        }

        public ModelManipulator() : base()
        {
            SceneNode.Name = "Model Manipulator";
            SizeScale = 1;
            EnableTranslation = true;
            EnableScaling = false;
            EnableRotation = true;
            EnableXRayGrid = false;

            //Transform = new Media3D.TranslateTransform3D(targetPoint.Geometry.BoundingSphere.Center.ToVector3D());
            //CenterOffset = ManipulatorPosition; //cameraPosition.Geometry.Bound.Center;
            //Target = ManipulatorTarget;
        }
    }
}
