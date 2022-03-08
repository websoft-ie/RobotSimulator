using HelixToolkit.Wpf.SharpDX;
using SharpDX;
using Vector3D = System.Windows.Media.Media3D.Vector3D;
using TranslateTransform3D = System.Windows.Media.Media3D.TranslateTransform3D;
using MatrixTransform3D = System.Windows.Media.Media3D.MatrixTransform3D;

namespace MonkeyMotionControl.Simulator
{
    public class TracerMarker3D : GroupModel3D
    {
        private MeshGeometryModel3D Marker;
        private BillboardTextModel3D TextLabel;
        private BillboardSingleText3D textGeometry;

        private Vector3 centerOffset = Vector3.Zero;
        public Vector3 Position
        {
            get
            {
                return centerOffset;
            }
            set
            {
                centerOffset = value;
                UpdateLabelText();
                OnUpdateSelfTransform();
            }
        }

        private float SizeScale = 1;
        private static int DecimalPlaces = 2;
        private string coordinateFormat = string.Format("{{0:F{0}}}, {{1:F{0}}}, {{2:F{0}}}", DecimalPlaces, DecimalPlaces, DecimalPlaces);
        private static double labelOffsetZ = 150;
        private double coneHeight = labelOffsetZ;
        private double coneBaseRadius = 0.0;
        private double coneTopRadius = (double)labelOffsetZ / 5;

        public TracerMarker3D()
            : base()
        {
            Marker = new MeshGeometryModel3D();
            Marker.Name = "TracerMarkerPointer";
            Marker.SceneNode.Name = Marker.Name;
            Marker.CullMode = SharpDX.Direct3D11.CullMode.Back;
            MeshBuilder builder = new MeshBuilder();
            builder.AddCone(Vector3.Zero, Vector3.UnitZ, coneBaseRadius, coneTopRadius, coneHeight, false, true, 100);
            Geometry3D mesh = builder.ToMesh();
            Marker.Geometry = mesh;
            Marker.Material = DiffuseMaterials.Red;
            Marker.Transform = new TranslateTransform3D(new Vector3D(0, 0, 0));
            Children.Add(Marker);

            TextLabel = new BillboardTextModel3D();
            TextLabel.Name = "TracerMarkerLabel";
            TextLabel.SceneNode.Name = Marker.Name;
            TextLabel.DepthBias = 3;
            textGeometry = new BillboardSingleText3D();
            textGeometry.TextInfo.Text = string.Format(coordinateFormat, 0.0, 0.0, 0.0);
            textGeometry.TextInfo.Origin = Vector3.Zero;
            textGeometry.FontColor = new Color4(255f, 255f, 0f, 1.0f);
            textGeometry.FontSize = 12;
            textGeometry.IsDynamic = true;
            TextLabel.Geometry = textGeometry;
            TextLabel.Transform = new TranslateTransform3D(new Vector3D(0, 0, labelOffsetZ * 1.3));
            Children.Add(TextLabel);

            Name = "TracerMarkerGroup";
            SceneNode.Name = Name;
        }

        private void OnUpdateSelfTransform()
        {
            var m = Matrix.Translation(centerOffset);
            m.M11 = m.M22 = m.M33 = SizeScale;
            Transform = new MatrixTransform3D(m.ToMatrix3D());
            //SceneNode.ModelMatrix = Matrix.Translation(-centerOffset) * scaleMatrix * rotationMatrix * Matrix.Translation(centerOffset) * Matrix.Translation(translationVector);
        }

        private void UpdateLabelText()
        {
            textGeometry.TextInfo = new TextInfo(string.Format(coordinateFormat, Position.X, Position.Y, Position.Z),Vector3.Zero);
            TextLabel.Geometry.UpdateVertices();
        }
    }

}
