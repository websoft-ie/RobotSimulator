using HelixToolkit.Wpf.SharpDX;
using SharpDX;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Media3D;
using Color = System.Windows.Media.Color;
using Colors = System.Windows.Media.Colors;

namespace MonkeyMotionControl.Simulator
{
    /// <summary>
    /// Plot a trace in 3D space with marker, axes and bounding box.
    /// </summary>
    /// <remarks>
    /// This class utilizes the Helix Toolkit which is licensed under the MIT License.
    /// 
    /// The MIT License (MIT)
    /// Copyright(c) 2018 Helix Toolkit contributors
    /// 
    /// Permission is hereby granted, free of charge, to any person obtaining a
    /// copy of this software and associated documentation files (the
    /// "Software"), to deal in the Software without restriction, including
    /// without limitation the rights to use, copy, modify, merge, publish,
    /// distribute, sublicense, and/or sell copies of the Software, and to
    /// permit persons to whom the Software is furnished to do so, subject to
    /// the following conditions:
    /// 
    /// The above copyright notice and this permission notice shall be included
    /// in all copies or substantial portions of the Software.
    /// 
    /// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
    /// OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
    /// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
    /// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
    /// CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
    /// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
    /// SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
    /// </remarks>
    public class PathTracer
    {
        private TracerMarker3D marker;
        
        private double minDistanceSquared;

        private class PathSegment
        {
            public Vector3 Point0;
            public Vector3 Point1;
            public Color Color;
            public double Thickness;
            public PathSegment() { }
            public PathSegment(Vector3 p0, Vector3 p1, Color color, double thickness)
            {
                Point0 = p0;
                Point1 = p1;
                Color = color;
                Thickness = thickness;
            }
        }
        private LineGeometryModel3D linesModel = new LineGeometryModel3D();
        private LineBuilder lineBuilder = new LineBuilder();
        private List<PathSegment> tracePath;
        private PathSegment segment;
        private Vector3 point0;  // last point
        //private Vector3 delta0;  // (dx,dy,dz)

        /// <summary>Reference of the parent SceneNode.</summary>
        private SceneNodeGroupModel3D MainScene { get; set; }

        /// <summary>A point closer than this distance from the previous point will not be plotted.</summary>
        public double MinDistance { get; set; } = 0.1;

        /// <summary>Current Point Marker.</summary>
        public bool MarkerEnable
        {
            get { return _markerEnable; }
            set
            {
                _markerEnable = value;
                if (value)
                {
                    ShowMarker();
                }
                else
                {
                    HideMarker();
                }
            }
        }
        private bool _markerEnable = false;
        /// <summary>Trace line thickness.</summary>
        public double Thickness 
        {
            get
            {
                return _thickness;
            }
            set
            {
                _thickness = value;
                linesModel.Thickness = value;
            }
        }
        private double _thickness = 2.0;
        
        /// <summary>Trace line smoothness.</summary>
        public double Smoothness 
        {
            get
            {
                return _smoothness;
            }
            set
            {
                _smoothness = value;
                linesModel.Smoothness = value;
            }
        }
        private double _smoothness = 2.0;
        
        /// <summary>Trace line color.</summary>
        public Color Color 
        {
            get
            {
                return _color;
            }
            set
            {
                _color = value;
                linesModel.Color = value;
            }
        }
        private Color _color = Colors.Blue;

        /// <summary>Initializes a new instance of the <see cref="PathTracer"/> class.</summary>
        public PathTracer(SceneNodeGroupModel3D mainScene)
        {
            Thickness = 2.0;
            Smoothness = 2.0;
            Color = Colors.Purple;
            MainScene = mainScene;
            CreateElements();
        }

        /// <summary>Creates the tracer elements.</summary>
        public void CreateElements()
        {
            minDistanceSquared = MinDistance * MinDistance;

            if (tracePath != null)
            {
                foreach (PathSegment l in tracePath)
                    AddLine(l.Point0, l.Point1);
                
                if (tracePath.Count > 0)
                    segment = tracePath[tracePath.Count - 1];
            }

            if (!MainScene.GroupNode.Items.Contains(linesModel.SceneNode))
                MainScene.AddNode(linesModel.SceneNode);

            if (MarkerEnable)
            {
                ShowMarker();
            }
        }

        /// <summary>Clears all traces and resets variables.</summary>
        private void Clear()
        {
            tracePath = null;
            segment = null;
            lineBuilder = new LineBuilder();
            linesModel.Geometry = new LineGeometry3D();
            CreateElements();
        }

        public void ClearTrace()
        {
            if (tracePath != null)
            {
                if (tracePath.Count > 0)
                {
                    if (MainScene.GroupNode.Items.Contains(linesModel.SceneNode))
                        MainScene.GroupNode.RemoveChildNode(linesModel.SceneNode);
                }
                Clear();
            }
        }

        /// <summary>
        /// Creates a new trace.
        /// </summary>
        /// <param name="point">The (X,Y,Z) location.</param>
        /// <param name="color">The initial color.</param>
        /// <param name="thickness">The initial line thickness.</param>
        public void NewTrace(Vector3 point, Color color, double thickness = 1)
        {
            segment = new PathSegment();
            segment.Point0 = point;
            segment.Point1 = point;
            segment.Color = color;
            segment.Thickness = thickness;
            tracePath = new List<PathSegment>();
            tracePath.Add(segment);
            point0 = point;
            //delta0 = new Vector3();
            UpdateMarker(point);
        }

        /// <summary>
        /// Adds a point to the current trace with a specified color.
        /// </summary>
        /// <param name="point">The (X,Y,Z) location.</param>
        /// <param name="color">The color.</param>
        /// <param name="thickness">The line thickness (optional).</param>
        /// <seealso cref="AddPoint(double, double, double, Color, double)"/>
        public void AddPoint(Vector3 point, Color color, double thickness = -1)
        {
            if (tracePath == null)
            {
                NewTrace(point, color, (thickness > 0) ? thickness : 1);
                return;
            }

            if ((point - point0).LengthSquared() < minDistanceSquared) 
                return;  // less than min distance from last point

            if (segment.Color != color || (thickness > 0 && segment.Thickness != thickness))
            {
                segment = new PathSegment();
                if (thickness <= 0)
                    thickness = segment.Thickness;
                segment.Color = color;
                segment.Thickness = thickness;
            }

            // If line segments AB and BC have the same direction (small cross product) then remove point B.
            //bool sameDir = false;
            //var delta = new Vector3(point.X - point0.X, point.Y - point0.Y, point.Z - point0.Z);
            //delta.Normalize();  // use unit vectors (magnitude 1) for the cross product calculations
            //if (trace.Count() > 0)
            //{
            //    double xp2 = Vector3.Cross(delta, delta0).LengthSquared();
            //    sameDir = (xp2 < 0.0005);  // approx 0.001 seems to be a reasonable threshold from logging xp2 values
            //    if (!sameDir) System.Console.WriteLine(string.Format("xp2={0:F6}", xp2));
            //}

            //if (sameDir)  // extend the current line segment
            //{
            //    trace[trace.Count() - 1].Point1 = point;
            //    point0 = point;
            //    delta0 += delta;
            //}
            //else  // add a new line segment
            //{
                segment.Point0 = point0;
                segment.Point1 = point;
                tracePath.Add(segment);
                AddLine(point0, point);
                point0 = point;
                //delta0 = delta;
            //}

            UpdateMarker(point);
        }

        private void AddLine(Vector3 p0, Vector3 p1)
        {
            //System.Console.WriteLine($"AddLine({p0},{p1})");
            lineBuilder.AddLine(p0, p1);
            linesModel.Geometry = lineBuilder.ToLineGeometry3D();
            linesModel.Geometry.UpdateVertices();
        }

        private void UpdateMarker(Vector3 pos)
        {
            if (marker != null)
            {
                marker.Position = pos;
            }
        }

        private void ShowMarker()
        {
            if (marker == null)
            {
                marker = new TracerMarker3D();
            }
            if (segment != null)
            {
                marker.Position = segment.Point1;
            }
            if (!MainScene.GroupNode.Items.Contains(marker.SceneNode))
            {
                marker.Tag = new AttachedNodeViewModel(marker.SceneNode);
                MainScene.AddNode(marker.SceneNode);
            }
        }

        private void HideMarker()
        {
            if (marker != null)
            {
                if (MainScene.GroupNode.Items.Contains(marker.SceneNode))
                    MainScene.GroupNode.RemoveChildNode(marker.SceneNode);
            }
            else
            {
                marker = null;
            }
        }

        /// <summary>
        /// Adds a point to the current trace.
        /// </summary>
        /// <param name="point">The (X,Y,Z) location.</param>
        /// <seealso cref="AddPoint(Point3D, Color, double)"/>
        public void AddPoint(Vector3 point)
        {
            if (segment == null)
            {
                NewTrace(point, Colors.Black, 1);
                return;
            }

            AddPoint(point, segment.Color, segment.Thickness);
        }

        /// <summary>
        /// Adds a point to the current trace with a specified color.
        /// </summary>
        /// <param name="x">The X location.</param>
        /// <param name="y">The Y location.</param>
        /// <param name="z">The Z location.</param>
        /// <param name="color">The color.</param>
        /// <param name="thickness">The line thickness (optional).</param>
        /// <seealso cref="AddPoint(Point3D, Color, double)"/>
        public void AddPoint(float x, float y, float z, Color color, double thickness = -1)
        {
            AddPoint(new Vector3(x, y, z), color, thickness);
        }

        /// <summary>
        /// Adds a point to the current trace.
        /// </summary>
        /// <param name="x">The X location.</param>
        /// <param name="y">The Y location.</param>
        /// <param name="z">The Z location.</param>
        /// <seealso cref="AddPoint(double, double, double, Color, double)"/>
        public void AddPoint(float x, float y, float z)
        {
            if (segment == null) return;

            AddPoint(new Vector3(x, y, z), segment.Color, segment.Thickness);
        }
    }
}
