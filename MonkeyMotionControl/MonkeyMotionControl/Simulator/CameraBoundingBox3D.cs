using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Model.Scene;
using SharpDX;
using Rect3D = System.Windows.Media.Media3D.Rect3D;
using Vector3D = System.Windows.Media.Media3D.Vector3D;
using TranslateTransform3D = System.Windows.Media.Media3D.TranslateTransform3D;
using Quaternion = System.Windows.Media.Media3D.Quaternion;
using System.Collections.Generic;

namespace MonkeyMotionControl.Simulator
{
    public class CameraBoundingBox3D : LineGeometryModel3D
    {
        private Rect3D camRect;
        public Rect3D BoundingBox
        {
            get
            {
                return camRect;
            }
            set
            {
                camRect = value;
            }
        }
        public Vector3 Center { get; set; }
        
        private LineGeometry3D lines;

        public CameraBoundingBox3D(SceneNode cameraModel)
            : base()
        {
            //int i = 0;
            //foreach (var model in cameraModel.Items)
            //{
            //    if (i == 1)
            //    {
            //        BoundingBox = new Rect3D(
            //            model.Bounds.Center.X,
            //            model.Bounds.Center.Y,
            //            model.Bounds.Center.Z,
            //            model.Bounds.Size.X,
            //            model.Bounds.Size.Y,
            //            model.Bounds.Size.Z
            //        );
            //        break;
            //    }
            //    i++;
            //}

            int i = 0;
            foreach (var model in cameraModel.Traverse())
            {
                if (model is MaterialGeometryNode m)
                {
                    if (i == 1)
                    {
                        BoundingBox = new Rect3D(
                            model.Bounds.Center.X,
                            model.Bounds.Center.Y,
                            model.Bounds.Center.Z,
                            model.Bounds.Size.X,
                            model.Bounds.Size.Y,
                            model.Bounds.Size.Z
                        );
                        break;
                    }
                    i++;
                }
            }

            Center = new Vector3((float)(camRect.X + camRect.SizeX / 2), (float)(camRect.Y + camRect.SizeY / 2), (float)(camRect.Z + camRect.SizeZ / 2));
            camRect.X = -camRect.SizeX / 2;
            camRect.Y = -camRect.SizeY / 2;
            camRect.Z = -camRect.SizeZ / 2;

            SceneNode.Name = "Camera Bounding Box";

            var builder = new LineBuilder();
            builder.AddBox(Vector3.Zero, camRect.SizeX, camRect.SizeY, camRect.SizeZ);

            lines = builder.ToLineGeometry3D();
            Geometry = lines;

            Color = System.Windows.Media.Colors.Red;
            Thickness = 3.0;
            Smoothness = 10;

            Transform = new TranslateTransform3D(new Vector3D(0, 0, 0));
            //Transform = new TranslateTransform3D(new Vector3D(camRect.X, camRect.Y, camRect.Z));

            TranslateTransform3D trans = new TranslateTransform3D();
            trans.OffsetX = Center.X;
            trans.OffsetY = Center.Y;
            trans.OffsetZ = Center.Z;

            //Transform = trans;

        }

        public List<Vector3D> Update(double X, double Y, double Z, double RX, double RY, double RZ)
        {
            //if (cameraBoundingBox == null)
            //    return new List<Vector3D>();

            List<Vector3D> axisVec = new List<Vector3D>();
            //if (cartPos == null) return axisVec;

            TranslateTransform3D trans = new TranslateTransform3D();
            trans.OffsetX = 0;
            trans.OffsetY = 0;
            trans.OffsetZ = 0;
            Transform = trans;

            Vector3D axisZ = new Vector3D(0,0,1);
            float angleZ = (float)RZ; 
            var matrix = Transform.Value;
            matrix.Rotate(new Quaternion(axisZ, angleZ));

            System.Windows.Media.Media3D.Matrix3D m = System.Windows.Media.Media3D.Matrix3D.Identity;
            Quaternion q = new Quaternion(axisZ, angleZ);
            m.Rotate(q);
            Vector3D axisY = new Vector3D(0, 1, 0);
            axisY = m.Transform(axisY);
            var angleY = RY; 
            matrix.Rotate(new Quaternion(axisY, angleY));

            q = new Quaternion(axisY, angleY);
            m.Rotate(q);
            Vector3D axisX = new Vector3D(1, 0, 0);
            axisX = m.Transform(axisX);
            var angleX = RX; 
            matrix.Rotate(new Quaternion(axisX, angleX));

            matrix.Translate(new Vector3D(X, Y, Z));
            Transform = new System.Windows.Media.Media3D.MatrixTransform3D(matrix);

            q = new Quaternion(axisX, angleX);
            m.Rotate(q);
            axisX = m.Transform(new Vector3D(1, 0, 0));
            axisY = m.Transform(new Vector3D(0, 1, 0));
            axisZ = m.Transform(new Vector3D(0, 0, 1));

            //if (bSimulate)
            //{
            //cameraXAxis.Direction = axisX;
            //cameraYAxis.Direction = axisY;
            //cameraZAxis.Direction = axisZ;
            //cameraZAxis.Direction = Vector3D.Multiply(-1, cameraZAxis.Direction);
            //}

            axisVec.Add(axisX);
            axisVec.Add(axisY);
            axisVec.Add(axisZ);

            return axisVec;
        }

        public List<Vector3D> GetAxisVectors()
        {
            //if (cameraBoundingBox == null)
            //    return new List<Vector3D>();

            List<Vector3D> axisVec = new List<Vector3D>();
            //if (cartPos == null) return axisVec;

            System.Windows.Media.Media3D.Matrix3D m = SceneNode.ModelMatrix.ToMatrix3D();

            Vector3D axisX = m.Transform(new Vector3D(1, 0, 0));
            Vector3D axisY = m.Transform(new Vector3D(0, 1, 0));
            Vector3D axisZ = m.Transform(new Vector3D(0, 0, 1));

            axisVec.Add(axisX);
            axisVec.Add(axisY);
            axisVec.Add(axisZ);

            return axisVec;
        }

    }


}
