using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Model.Scene;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media.Media3D;

namespace MonkeyMotionControl.Simulator
{
    public enum SelectedVisualType
    {
        None, CameraManipulator, PointManipulator,
        PrismReferencePoint, TargetPoint, CameraPoint,
        BezierMarkPoint, ControlPoint, Trajectory
    }

    public struct CustomCurve
    {
        public CurveType curveShape;                       /// Shape of curve ("bezier", "segment", "arc")
        public double lenCurve;                         /// Length of curve
        public List<BezierMarkPoint3D> ptCurve;           /// Both of edge points of curve
        public List<BezierMarkPoint3D> ptAdjust;          /// Both of adjust points of curve (2 points when bezier, 1 point when arc, none when segment)
        public List<BillboardText> lblVisual3D; /// Text Visual3D object which represents the number of bezier points
        public int radius;                              /// Radius of curve when arc type
        public List<Point3D> curvePath;                 /// Point array of which curve is composed
        public List<ControlPointLine> ctrlLine;            /// Both of control lines for curve
        public List<double> velArray;                   /// Velocity array
        public List<double> accelArray;                 /// Acceleration array
    }

    public class BillboardText : BillboardTextModel3D
    {
        private Point3D _position;
        public Point3D Position
        {
            get 
            {
                return _position;
            }
            set
            {
                _position = value;
                TextBillboard.TextInfo = new TextInfo(_text, _position.ToVector3())
                {
                    Scale = 1.3f
                };
                //Geometry = TextBillboard;
            }
        }

        private string _text;
        public string Text 
        {
            get
            {
                return _text;
            }
            set
            {
                _text = value;
                TextBillboard.TextInfo.Text = _text;
                //TextBillboard.TextInfo = new TextInfo(_text, _position.ToVector3())
                //{
                //    Scale = 1.3f
                //};
                //Geometry = TextBillboard;
            }
        }
        public Color Foreground;
        public double FontSize;

        public BillboardSingleText3D TextBillboard { get; set; }

        public BillboardText(string text, Point3D position)
            : base()
        {
            _text = text;
            _position = position;
            
            TextBillboard = new BillboardSingleText3D()
            {
                FontColor = Color.White,
                FontWeight = FontWeights.Bold,
                BackgroundColor = new Color4(0.8f, 0.8f, 0.8f, 0.0f),
                Padding = new Thickness(2),
                IsDynamic = true // if change frequently
            };

            TextBillboard.TextInfo = new TextInfo(text, position.ToVector3())
            {
                Scale = 1.3f
            };

            Geometry = TextBillboard;
        }
    }

    public class ControlPointLine : LineGeometryModel3D
    {
        public Point3DCollection Points { get; set; }
        public LineGeometry3D line;

        public ControlPointLine()
            : base()
        {
            line = new LineGeometry3D();
            line.Positions = new Vector3Collection(2);
            line.Positions.Add(Vector3.Zero);
            line.Positions.Add(Vector3.Zero);
            line.Indices = new IntCollection(2);
            line.Indices.Add(0);
            line.Indices.Add(1);
            line.UpdateVertices();
            Geometry = line;
        }

        public void Update()
        {
            if (Points.Count >= 2)
            {
                line.Positions[0] = Points[0].ToVector3();
                line.Positions[1] = Points[1].ToVector3();
                line.UpdateVertices();
                Geometry.UpdateVertices();
            }
        }
    }

    public class Trajectory : MeshGeometryModel3D
    {
        private double pipeInnerDiameter = 0;
        private double pipeDiameter = 10;
        private int pipeThetaDiv = 12;

        public int number;

        public Color Color
        {
            set
            {
                Material = new PhongMaterial
                {
                    DiffuseColor = value
                };
            }
        }

        private Point3DCollection _points;
        public Point3DCollection Points
        {
            get { return _points; }
            set
            {
                _points = value;
                Update();
            }
        }

        public Trajectory(int num, PathType pathType)
            : base()
        {
            number = num;
            Name = pathType.ToString();
            SceneNode.Name = pathType.ToString();
            Points = new Point3DCollection();
            var builder = new MeshBuilder();
            builder.AddPipe(Vector3.Zero, new Vector3(2000, 0, 0), pipeInnerDiameter, pipeDiameter, pipeThetaDiv);
            Geometry = builder.ToMeshGeometry3D();
            CullMode = SharpDX.Direct3D11.CullMode.Back;
            IsThrowingShadow = false;
            Material = new PhongMaterial
            {
                DiffuseColor = Color.Red,
                AmbientColor = Color.Black,
                EmissiveColor = Color.Black,
                SpecularColor = Color.Black
            };
        }

        public void Update()
        {
            //Console.WriteLine($"Trajectory.Update() Points Count={Points.Count}");
            if (Points.Count > 1)
            {
                var builder = new MeshBuilder(true, false, false);
                for (int i = 0; i < Points.Count - 1; i++)
                {
                    //Console.WriteLine($"Update: Add Pipe {i} [{i}, {i + 1}]");
                    builder.AddPipe(Points[i].ToVector3(), Points[i + 1].ToVector3(), pipeInnerDiameter, pipeDiameter, pipeThetaDiv);
                }
                Geometry = builder.ToMeshGeometry3D(); //builder.ToLineGeometry3D();
                //Geometry.UpdateVertices();
                //Geometry.OctreeParameter.RecordHitPathBoundingBoxes = true;
                Geometry.OctreeParameter.MinimumOctantSize = 20f;
                Geometry.UpdateOctree();
                Transform = new TranslateTransform3D(new Vector3D(0, 0, 0));
            }
        }

    }


    #region POINT VISUAL

    public class BezierMarkPoint3D : SphereNode
    {
        //public delegate void BezierMarkPointEventHandler(object sender, EventArgs args);
        //public event BezierMarkPointEventHandler BezierMarkPointEvent;

        public int curveNum;
        public Point3D point;
        public PathType PathType;

        public BezierMarkPoint3D(int number, PathType pathType, PointType pointType, Point3D pt, Color col)
            : base(pt.ToVector3(), 15f, col)
        {            
            PathType = pathType;
            Name = pointType.ToString();
            curveNum = number;
            point = pt;
        }
        public BezierMarkPoint3D()
            : base()
        {
            new BezierMarkPoint3D(0, PathType.MOTIONPATH, PointType.POINT, new Point3D(), Color.Red);
        }
    }

    public class RobotFlangePoint3D : SphereNode
    {
        public RobotFlangePoint3D(Vector3 pt)
            : base(pt, 18f, Color.Purple)
        {
            Name = "Robot Flange Point";
        }
    }

    public class CameraPoint3D : SphereNode
    {
        private Vector3 direction;
        public Vector3 Direction
        {
            get { return direction; }
            set
            {
                direction = value;
                //TODO Calculate the UpDirection, the perpendicular to the Direction for ROLL angle
                //Console.WriteLine($"Camera Point Direction: {direction.ToString()}");
                //UpDirection = Vector3.Cross(
                //    Vector3.Cross(
                //        direction, 
                //        Vector3.UnitY
                //    ), 
                //    Vector3.UnitZ
                //);
                //Console.WriteLine($"Camera Point Up Direction: {UpDirection.ToString()}");
            }
        }

        //TODO UpDirection for Camera Perspective ROLL orientation 
        public Vector3 UpDirection { get; set; }

        public CameraPoint3D(Vector3 pt)
            : base(pt, 15f, Color.Yellow)
        {
            Name = "Camera Point";
        }

        //public Vector3 Staubli_ZOffset { get; set; } = new Vector3(0, 0, 550);

        //public Vector3 CameraOffset { get; set; } = new Vector3(0, 0, 0);

        //public new Vector3 Position
        //{
        //    get
        //    {
        //        return base.Position - CameraOffset - Staubli_ZOffset;
        //    }

        //    set
        //    {
        //        base.Position = value;
        //    }
        //}

    }

    public class ControlPoint3D : SphereNode
    {
        public ControlPoint3D(Vector3 pt)
            : base(pt, 15f, Color.Blue)
        {
            Name = "Control Point";
        }
    }

    public class TargetPoint3D : SphereNode
    {
        public TargetPoint3D(Vector3 pt)
            : base(pt, 15f, Color.Red)
        {
            Name = "Target Point";
        }
    }

    public class PrismReferencePoint3D : SphereNode
    {
        public PrismReferencePoint3D(Vector3 pt)
            : base(pt, 15f, Color.Aquamarine)
        {
            Name = "Prism Reference Point";
        }
    }

    #endregion

    public class LimitSphere3D : SphereNode
    {
        public LimitSphere3D(Point3D center, float radius)
            : base(center.ToVector3(), radius, Color.Transparent)
        {
            Name = "Robotic Arm Limits Sphere";
            RenderWireframe = true;
            WireframeColor = Color.DarkGray;
            CullMode = SharpDX.Direct3D11.CullMode.None;
        }
    }

    public class LightSphere3D : MeshGeometryModel3D
    {
        public LightSphere3D(Point3D center, float radius)
            : base()
        {
            var builder = new MeshBuilder();
            builder.AddSphere(Vector3.Zero, radius);
            Geometry = builder.ToMeshGeometry3D();
            CullMode = SharpDX.Direct3D11.CullMode.Back;
            IsThrowingShadow = true;
            Transform = new TranslateTransform3D(new Vector3D(center.X, center.Y, center.Z));
            SceneNode.Name = "Light Point";
            Material = new PhongMaterial
            {
                AmbientColor = Color.Gray,
                DiffuseColor = Color.Gray,
                EmissiveColor = Color.Yellow,
                SpecularColor = Color.Black
            };
            PostEffects = "glow[color:#FFFBA5];";
        }
    }


}
