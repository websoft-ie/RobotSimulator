using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Model.Scene;
using SharpDX;
using System;

namespace MonkeyMotionControl.Simulator
{
    public class PointManipulator3D : GroupModel3D
    {
        public SceneNode Target { get; set; }
        public float SizeScale { get; set; } = 100;
        public float Diameter { get; set; } = 15;
        public float Offset { get; set; } = 0;
        public bool IsCaptured { get; set; } = false;
        public Vector3 Position
        {
            get
            {
                return centerOffset;
            }
            set
            {
                centerOffset = value;
                OnUpdateSelfTransform();
            }
        }

        private bool enableXRayGrid;
        public bool EnableXRayGrid
        {
            get 
            { 
                return enableXRayGrid; 
            }
            set 
            { 
                enableXRayGrid = value;
                xrayEffect.IsRendering = value;
            }
        }

        private enum ManipulationType
        {
            None, TranslationX, TranslationY, TranslationZ
        }
        private ManipulationType manipulationType = ManipulationType.None;

        private Geometry3D TranslationXGeometry;
        private MeshGeometryModel3D translationX, translationY, translationZ;
        private Vector3 centerOffset = Vector3.Zero;
        private Vector3 translationVector = Vector3.Zero;
        private Matrix rotationMatrix = Matrix.Identity;
        private Matrix scaleMatrix = Matrix.Identity;
        private Matrix targetMatrix = Matrix.Identity;

        private Viewport3DX currentViewport;
        private Element3D xrayEffect;
        private Vector3 lastHitPosWS;
        private Vector3 normal;
        private Vector3 direction;
        private Vector3 currentHit;
        private Color4 currentColor;

        public PointManipulator3D()
        {
            var bd = new MeshBuilder();
            float arrowLength = 1.5f;
            bd.AddArrow(Vector3.UnitX * arrowLength, new Vector3(1.2f * arrowLength, 0, 0), 0.14, 4, 12);
            bd.AddCylinder(Vector3.Zero, Vector3.UnitX * arrowLength, 0.10, 12);
            TranslationXGeometry = bd.ToMesh();
            TranslationXGeometry.OctreeParameter.MinimumOctantSize = 0.01f;
            TranslationXGeometry.UpdateOctree();

            var rotationYMatrix = Matrix.RotationZ((float)Math.PI / 2);
            var rotationZMatrix = Matrix.RotationY(-(float)Math.PI / 2);

            translationX = new MeshGeometryModel3D() {
                Geometry = TranslationXGeometry,
                Material = DiffuseMaterials.Red,
                CullMode = SharpDX.Direct3D11.CullMode.Back,
                PostEffects = "ManipulatorXRayGrid"
            };
            translationX.SceneNode.Name = "X Axis Manipulator";

            translationY = new MeshGeometryModel3D() { 
                Geometry = TranslationXGeometry, 
                Material = DiffuseMaterials.Green, 
                CullMode = SharpDX.Direct3D11.CullMode.Back, 
                PostEffects = "ManipulatorXRayGrid"
            };
            translationY.SceneNode.Name = "Y Axis Manipulator";
            translationZ = new MeshGeometryModel3D() { 
                Geometry = TranslationXGeometry, 
                Material = DiffuseMaterials.Blue, 
                CullMode = SharpDX.Direct3D11.CullMode.Back, 
                PostEffects = "ManipulatorXRayGrid"
            };
            translationZ.SceneNode.Name = "Z Axis Manipulator";

            translationY.Transform = new System.Windows.Media.Media3D.MatrixTransform3D(rotationYMatrix.ToMatrix3D());
            translationZ.Transform = new System.Windows.Media.Media3D.MatrixTransform3D(rotationZMatrix.ToMatrix3D());

            translationX.Mouse3DDown += Translation_Mouse3DDown;
            translationY.Mouse3DDown += Translation_Mouse3DDown;
            translationZ.Mouse3DDown += Translation_Mouse3DDown;
            translationX.Mouse3DMove += Translation_Mouse3DMove;
            translationY.Mouse3DMove += Translation_Mouse3DMove;
            translationZ.Mouse3DMove += Translation_Mouse3DMove;
            translationX.Mouse3DUp += Manipulation_Mouse3DUp;
            translationY.Mouse3DUp += Manipulation_Mouse3DUp;
            translationZ.Mouse3DUp += Manipulation_Mouse3DUp;

            Children.Add(translationX);
            Children.Add(translationY);
            Children.Add(translationZ);
            
            xrayEffect = new PostEffectMeshXRayGrid()
            {
                EffectName = "ManipulatorXRayGrid",
                DimmingFactor = 0.5,
                BlendingFactor = 0.8,
                GridDensity = 4,
                GridColor = System.Windows.Media.Colors.Gray
            };
            (xrayEffect.SceneNode as NodePostEffectXRayGrid).XRayDrawingPassName = DefaultPassNames.EffectMeshDiffuseXRayGridP3;
            xrayEffect.SceneNode.Name = "ManipulatorXRayGrid";
            Children.Add(xrayEffect.SceneNode);

            SceneNode.Name = "Point Manipulator";
            EnableXRayGrid = false;
        }
        
        private void Translation_Mouse3DDown(object sender, MouseDown3DEventArgs e)
        {
            //Console.WriteLine($"CameraManipulator MouseDown.");
            if (Target == null)
            {
                return;
            }

            if (e.HitTestResult.ModelHit == translationX)
            {
                //Console.WriteLine($"Camera Manipulator Hit X Axis");
                manipulationType = ManipulationType.TranslationX;
                direction = Vector3.UnitX;
            }
            else if (e.HitTestResult.ModelHit == translationY)
            {
                //Console.WriteLine($"Camera Manipulator Hit Y Axis");
                manipulationType = ManipulationType.TranslationY;
                direction = Vector3.UnitY;
            }
            else if (e.HitTestResult.ModelHit == translationZ)
            {
                //Console.WriteLine($"Camera Manipulator Hit Z Axis");
                manipulationType = ManipulationType.TranslationZ;
                direction = Vector3.UnitZ;
            }
            else
            {
                //Console.WriteLine($"Camera Manipulator No Hit");
                manipulationType = ManipulationType.None;
                IsCaptured = false;
                return;
            }
            var material = ((e.HitTestResult.ModelHit as MeshGeometryModel3D).Material as DiffuseMaterial);
            currentColor = material.DiffuseColor;
            material.DiffuseColor = Color.Yellow;
            currentViewport = e.Viewport;
            var cameraNormal = Vector3.Normalize(e.Viewport.Camera.CameraInternal.LookDirection);
            lastHitPosWS = e.HitTestResult.PointHit;
            var up = Vector3.Cross(cameraNormal, direction);
            normal = Vector3.Cross(up, direction);
            if (currentViewport.UnProjectOnPlane(e.Position.ToVector2(), lastHitPosWS, normal, out var hit))
            {
                currentHit = hit;
                IsCaptured = true;
            }
        }
        private void Translation_Mouse3DMove(object sender, MouseMove3DEventArgs e)
        {
            if (!IsCaptured)
            {
                return;
            }
            if (currentViewport.UnProjectOnPlane(e.Position.ToVector2(), lastHitPosWS, normal, out var hit))
            {
                var moveDir = hit - currentHit;
                currentHit = hit;
                switch (manipulationType)
                {
                    case ManipulationType.TranslationX:
                        translationVector += new Vector3(moveDir.X, 0, 0);
                        break;
                    case ManipulationType.TranslationY:
                        translationVector += new Vector3(0, moveDir.Y, 0);
                        break;
                    case ManipulationType.TranslationZ:
                        translationVector += new Vector3(0, 0, moveDir.Z);
                        break;
                }
                OnUpdateSelfTransform();
                OnUpdateTargetMatrix();
            }
        }

        private void Manipulation_Mouse3DUp(object sender, MouseUp3DEventArgs e)
        {
            if (IsCaptured)
            {
                var material = ((e.HitTestResult.ModelHit as MeshGeometryModel3D).Material as DiffuseMaterial);
                material.DiffuseColor = currentColor;
            }
            manipulationType = ManipulationType.None;
            IsCaptured = false;
        }

        private void ResetTransforms()
        {
            scaleMatrix = rotationMatrix = targetMatrix = Matrix.Identity;
            translationVector = Vector3.Zero;
            OnUpdateSelfTransform();
        }

        //private void UpdateModelTransform()
        //{
        //    SceneNode.ModelMatrix = transform.ToMatrix();
        //}

        private void OnUpdateSelfTransform()
        {
            var m = Matrix.Translation(centerOffset + translationVector);
            m.M11 = m.M22 = m.M33 = SizeScale;
            Transform = new System.Windows.Media.Media3D.MatrixTransform3D(m.ToMatrix3D());
            //SceneNode.ModelMatrix = Matrix.Translation(-centerOffset) * scaleMatrix * rotationMatrix * Matrix.Translation(centerOffset) * Matrix.Translation(translationVector);
            //Console.WriteLine($"Point Manipulator: {Position.X} {Position.Y} {Position.Z}");
        }

        private void OnUpdateTargetMatrix()
        {
            if (Target == null)
            {
                return;
            }
            //targetMatrix = Matrix.Translation(-centerOffset) * scaleMatrix * rotationMatrix * Matrix.Translation(centerOffset) * Matrix.Translation(translationVector);
            targetMatrix = Matrix.Translation(centerOffset) * Matrix.Translation(translationVector);
            Target.ModelMatrix = targetMatrix;
            Target.Update(null);
            //Target.Transform = new System.Windows.Media.Media3D.MatrixTransform3D(targetMatrix.ToMatrix3D());
        }

    }

}
