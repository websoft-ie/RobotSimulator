using HelixToolkit.Wpf.SharpDX;
using System.Windows.Media.Media3D;

namespace MonkeyMotionControl.Simulator
{
    public class PlaneGrid3D : LineGeometryModel3D
    {

        public LineGeometry3D lines;

        public PlaneGrid3D()
        {
            SceneNode.Name = "Plane Grid";

            //lines = LineBuilder.GenerateGrid()

            var width = 10000;
            var height = 10000;
            
            var builder = new LineBuilder();
            builder.AddGrid(BoxFaces.Top, 39, 39, width, height);
            lines = builder.ToLineGeometry3D();
            Geometry = lines;

            Color = System.Windows.Media.Color.FromRgb(60, 60, 60);
            Thickness = 2.0;
            Smoothness = 10;

            Transform = new TranslateTransform3D(new Vector3D(-width / 2, -height / 2, 0));
        }

        public void Update()
        {
            //line.Positions[0] = _points[0].ToVector3();
            //line.Positions[1] = _points[1].ToVector3();
            //line.UpdateVertices();
            Geometry.UpdateVertices();
        }

    }
}
