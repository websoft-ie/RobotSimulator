using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Model.Scene;
using SharpDX;
using System;

namespace MonkeyMotionControl.Simulator
{
    public class CameraManipulator3D : GroupModel3D
    {
        public SceneNode Target { get; set; }
        public float SizeScale { get; set; } = 100;
        public float Diameter { get; set; } = 15;
        public float Offset { get; set; } = 0;        
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

        private Vector3 lookAtTarget;
        public Vector3 LookAtTarget
        {
            get
            {
                return lookAtTarget;
            }
            set
            {
                lookAtTarget = value;
            }
        }
        
        private Vector3 pipDirection;
        public Vector3 PIPDirection
        {
            get { return pipDirection; }
            set
            {
                pipDirection = value;
                UpdatePIPDirection();
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
        
        public bool IsCaptured { get; set; } = false;

        private bool _isShowPIPOnly = false;
        public bool IsShowPIPOnly {
            get 
            {
                return _isShowPIPOnly;
            }
            set
            {
                _isShowPIPOnly = value;
                if (value)
                {
                    ShowOnlyPIPManipulator();
                }
                else
                {
                    ShowAllManipulators();
                }
            } 
        } 

        private enum ManipulationType
        {
            None, TranslationX, TranslationY, TranslationZ, TranslationPIP
        }
        private ManipulationType manipulationType = ManipulationType.None;

        private Geometry3D TranslationXGeometry;
        private Geometry3D TranslationYZGeometry;
        private Geometry3D TranslationPIPGeometry;
        private CameraXAxisManipulator3D translationX = new CameraXAxisManipulator3D();
        private CameraYAxisManipulator3D translationY = new CameraYAxisManipulator3D();
        private CameraZAxisManipulator3D translationZ = new CameraZAxisManipulator3D();
        private CameraPIPAxisManipulator3D translationPIP = new CameraPIPAxisManipulator3D();
        
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
                
        public CameraManipulator3D() 
            : base()
        {
            /// X Manipulator Arrows
            var bd = new MeshBuilder();
            float arrowLength = 2.0f;
            bd.AddCylinder(Vector3.Zero, Vector3.UnitX * arrowLength, 0.10, 12);
            bd.AddArrow(Vector3.UnitX * arrowLength, new Vector3(1.2f * arrowLength, 0, 0), 0.14, 4, 12);
            TranslationXGeometry = bd.ToMesh();
            TranslationXGeometry.OctreeParameter.MinimumOctantSize = 0.01f;
            TranslationXGeometry.UpdateOctree();

            /// YZ Manipulator Arrows
            bd = new MeshBuilder();
            arrowLength = 1.5f;
            bd.AddCylinder(Vector3.Zero, Vector3.UnitX * arrowLength, 0.10, 12);
            bd.AddArrow(Vector3.UnitX * arrowLength, new Vector3(1.2f * arrowLength, 0, 0), 0.14, 4, 12);
            TranslationYZGeometry = bd.ToMesh();
            TranslationYZGeometry.OctreeParameter.MinimumOctantSize = 0.01f;
            TranslationYZGeometry.UpdateOctree();

            /// PIP Manipulator Arrow
            bd = new MeshBuilder();
            arrowLength = 2.5f;
            bd.AddCylinder(Vector3.Zero, new Vector3(-1.2f * arrowLength, 0, 0), 0.10, 12);
            bd.AddArrow(Vector3.Zero, new Vector3(-1.2f * arrowLength - 0.2f, 0, 0), 0.14, 4, 12);
            TranslationPIPGeometry = bd.ToMesh();
            TranslationPIPGeometry.OctreeParameter.MinimumOctantSize = 0.01f;
            TranslationPIPGeometry.UpdateOctree();

            var rotationYMatrix = Matrix.RotationZ((float)Math.PI / 2);
            var rotationZMatrix = Matrix.RotationY((float)Math.PI / 2);
            
            translationX.Geometry = TranslationXGeometry;
            translationX.Material = DiffuseMaterials.Red; 
            
            translationY.Geometry = TranslationYZGeometry;
            translationY.Material = DiffuseMaterials.Green;
            
            translationZ.Geometry = TranslationYZGeometry;
            translationZ.Material = DiffuseMaterials.Blue;
            
            translationPIP.Geometry = TranslationPIPGeometry;
            translationPIP.Material = DiffuseMaterials.Yellow;
            
            translationY.Transform = new System.Windows.Media.Media3D.MatrixTransform3D(rotationYMatrix.ToMatrix3D());
            translationZ.Transform = new System.Windows.Media.Media3D.MatrixTransform3D(rotationZMatrix.ToMatrix3D());
            
            translationX.Mouse3DDown += Translation_Mouse3DDown;
            translationY.Mouse3DDown += Translation_Mouse3DDown;
            translationZ.Mouse3DDown += Translation_Mouse3DDown;
            translationPIP.Mouse3DDown += Translation_Mouse3DDown;
            translationX.Mouse3DMove += Translation_Mouse3DMove;
            translationY.Mouse3DMove += Translation_Mouse3DMove;
            translationZ.Mouse3DMove += Translation_Mouse3DMove;
            translationPIP.Mouse3DMove += Translation_Mouse3DMove;
            translationX.Mouse3DUp += Manipulation_Mouse3DUp;
            translationY.Mouse3DUp += Manipulation_Mouse3DUp;
            translationZ.Mouse3DUp += Manipulation_Mouse3DUp;
            translationPIP.Mouse3DUp += Manipulation_Mouse3DUp;

            //Children.Add(translationX);
            //Children.Add(translationY);
            //Children.Add(translationZ);
            //Children.Add(translationPIP);
            ShowAllManipulators();
            
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

            SceneNode.Name = "Camera Manipulator";
            EnableXRayGrid = false;
        }

        private void ShowAllManipulators()
        {
            if (!Children.Contains(translationX)) Children.Add(translationX);
            if (!Children.Contains(translationY)) Children.Add(translationY);
            if (!Children.Contains(translationZ)) Children.Add(translationZ);
            if (!Children.Contains(translationPIP)) Children.Add(translationPIP);
        }

        private void ShowOnlyPIPManipulator()
        {
            if (Children.Contains(translationX)) Children.Remove(translationX); // Remove X
            if (Children.Contains(translationY)) Children.Remove(translationY); // Remove Y
            if (Children.Contains(translationZ)) Children.Remove(translationZ); // Remove Z
            if (!Children.Contains(translationPIP)) Children.Add(translationPIP); // Add PIP
        }



        private void Translation_Mouse3DDown(object sender, MouseDown3DEventArgs e)
        {
            if (ReferenceEquals(Target, null)) return;

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
            else if (e.HitTestResult.ModelHit == translationPIP)
            {
                //Console.WriteLine($"Camera Manipulator Hit PIP Axis");
                manipulationType = ManipulationType.TranslationPIP;
                direction = PIPDirection;
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
            material.DiffuseColor = Color.Magenta;
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
                    case ManipulationType.TranslationPIP:
                        translationVector += Vector3.Dot(moveDir, PIPDirection) * PIPDirection;
                        break;
                }
                currentHit = hit;
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
                centerOffset = centerOffset + translationVector;
                translationVector = Vector3.Zero;
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
            //Target.Transform = new System.Windows.Media.Media3D.MatrixTransform3D(targetMatrix.ToMatrix3D());
        }

        private void UpdatePIPDirection()
        {

            Matrix toolTransformMatrix = new Matrix();
            
            foreach (var node in MonkeyMotionControl.UI.Simulator.RobotJoints[7].model.Traverse())
                if (node is MeshNode m)
                {
                    toolTransformMatrix = m.ModelMatrix;
                    break;
                }
            
            translationPIP.Transform = new System.Windows.Media.Media3D.MatrixTransform3D(
                new System.Windows.Media.Media3D.Matrix3D(
                    toolTransformMatrix.M11, toolTransformMatrix.M12, toolTransformMatrix.M13, 0,
                    toolTransformMatrix.M21, toolTransformMatrix.M22, toolTransformMatrix.M23, 0,
                    toolTransformMatrix.M31, toolTransformMatrix.M32, toolTransformMatrix.M33, 0,
                    0, 0, 0, 1));            
        }

    }

    public class CameraXAxisManipulator3D : MeshGeometryModel3D
    {
        public CameraXAxisManipulator3D()
            : base()
        {
            Name = "XAxisManipulator";
            SceneNode.Name = Name;
            CullMode = SharpDX.Direct3D11.CullMode.Back;
            PostEffects = "ManipulatorXRayGrid";
        }
    }

    public class CameraYAxisManipulator3D : MeshGeometryModel3D
    {
        public CameraYAxisManipulator3D()
            : base()
        {
            Name = "YAxisManipulator";
            SceneNode.Name = Name;
            CullMode = SharpDX.Direct3D11.CullMode.Back;
            PostEffects = "ManipulatorXRayGrid";
        }
    }

    public class CameraZAxisManipulator3D : MeshGeometryModel3D
    {
        public CameraZAxisManipulator3D()
            : base()
        {
            Name = "ZAxisManipulator";
            SceneNode.Name = Name;
            CullMode = SharpDX.Direct3D11.CullMode.Back;
            PostEffects = "ManipulatorXRayGrid";
        }
    }

    public class CameraPIPAxisManipulator3D : MeshGeometryModel3D
    {
        public CameraPIPAxisManipulator3D()
            : base()
        {
            Name = "PIPAxisManipulator";
            SceneNode.Name = Name;
            CullMode = SharpDX.Direct3D11.CullMode.Back;
            PostEffects = "ManipulatorXRayGrid";
        }
    }

}
