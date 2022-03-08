using HelixToolkit.Wpf.SharpDX;
using MonkeyMotionControl.UI;
using SharpDX;
using Color = System.Windows.Media.Color;
using Colors = System.Windows.Media.Colors;

namespace MonkeyMotionControl.Simulator
{
    public class CameraViewPrism3D : LineGeometryModel3D
    {
        private LineGeometry3D lines;

        public float FrameWidth { get; set; } = 300;
        public float FrameHeight { get; set; } = 200;
        public Color PrismColor { get; set; } = Colors.Aquamarine;

        private Vector3 ptCenter;
        private Vector3 pt1, pt2, pt3, pt4, pt5;

        public CameraViewPrism3D(Vector3 position)
            : base()
        {
            ptCenter = position;
            var initialTargetDistance = 1000;

            var cameraXAxisDirection = Vector3.UnitX;
            var cameraYAxisDirection = Vector3.UnitY;
            var cameraZAxisDirection = Vector3.UnitZ;

            LineBuilder builder = new LineBuilder();

            pt1 = ptCenter + Vector3.Multiply(cameraXAxisDirection, initialTargetDistance)
                + Vector3.Multiply(cameraYAxisDirection, FrameWidth)
                + Vector3.Multiply(cameraZAxisDirection, -FrameHeight);
            builder.AddLine(ptCenter, pt1);

            pt2 = ptCenter + Vector3.Multiply(cameraXAxisDirection, initialTargetDistance)
                + Vector3.Multiply(cameraYAxisDirection, FrameWidth)
                - Vector3.Multiply(cameraZAxisDirection, -FrameHeight);
            builder.AddLine(ptCenter, pt2);

            builder.AddLine(pt1, pt2);

            pt3 = ptCenter + Vector3.Multiply(cameraXAxisDirection, initialTargetDistance)
                - Vector3.Multiply(cameraYAxisDirection, FrameWidth)
                - Vector3.Multiply(cameraZAxisDirection, -FrameHeight);
            builder.AddLine(ptCenter, pt3);

            builder.AddLine(pt2, pt3);

            pt4 = ptCenter + Vector3.Multiply(cameraXAxisDirection, initialTargetDistance)
                - Vector3.Multiply(cameraYAxisDirection, FrameWidth)
                + Vector3.Multiply(cameraZAxisDirection, -FrameHeight);
            builder.AddLine(ptCenter, pt4);

            builder.AddLine(pt3, pt4);

            builder.AddLine(pt4, pt1);

            lines = builder.ToLineGeometry3D();
            Geometry = lines;
            Color = PrismColor;
            Thickness = 3.0;
            Smoothness = 10;
            Transform = new System.Windows.Media.Media3D.TranslateTransform3D(new System.Windows.Media.Media3D.Vector3D(0, 0, 0));
            SceneNode.Name = "Camera View Prism";
        }

        public void Update(Vector3 position, float targetDistance, Vector3 xDirection, Vector3 yDirection, Vector3 zDirection, Vector3 pipDirection, ToolPlacement camMount)
        {
            if (position == null) return;

            Color = PrismColor;

            float prismLen = targetDistance;
            Vector3 direction1 = Vector3.Zero;
            Vector3 direction2 = Vector3.Zero;

            switch (camMount)
            {
                case ToolPlacement.BOTTOM:
                    direction1 = yDirection;
                    direction2 = zDirection;
                    break;
                case ToolPlacement.FRONT:
                    direction1 = yDirection;
                    direction2 = xDirection;
                    break;
                default:
                    break;
            }

            ptCenter = position;

            pt1 = ptCenter + Vector3.Multiply(pipDirection, prismLen)
                + Vector3.Multiply(direction1, FrameWidth)
                + Vector3.Multiply(direction2, -FrameHeight);
            lines.Positions[0] = ptCenter;
            lines.Positions[1] = pt1;

            pt2 = ptCenter + Vector3.Multiply(pipDirection, prismLen)
                + Vector3.Multiply(direction1, FrameWidth)
                - Vector3.Multiply(direction2, -FrameHeight);
            lines.Positions[2] = ptCenter;
            lines.Positions[3] = pt2;

            lines.Positions[4] = pt1;
            lines.Positions[5] = pt2;

            pt3 = ptCenter + Vector3.Multiply(pipDirection, prismLen)
                - Vector3.Multiply(direction1, FrameWidth)
                - Vector3.Multiply(direction2, -FrameHeight);
            lines.Positions[6] = ptCenter;
            lines.Positions[7] = pt3;

            lines.Positions[8] = pt2;
            lines.Positions[9] = pt3;

            pt4 = ptCenter + Vector3.Multiply(pipDirection, prismLen)
                - Vector3.Multiply(direction1, FrameWidth)
                + Vector3.Multiply(direction2, -FrameHeight);
            lines.Positions[10] = ptCenter;
            lines.Positions[11] = pt4;

            lines.Positions[12] = pt3;
            lines.Positions[13] = pt4;

            lines.Positions[14] = pt4;
            lines.Positions[15] = pt1;

            Geometry.UpdateVertices();
        }

    }

}
