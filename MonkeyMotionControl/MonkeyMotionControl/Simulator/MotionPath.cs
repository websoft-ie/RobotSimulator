using HelixToolkit.Wpf.SharpDX;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace MonkeyMotionControl.Simulator
{
    public enum PathType
    {

        MOTIONPATH,
        TARGETPATH
    }

    public enum CurveType
    {
        BEZIER,
        ARC,
        LINE
    }

    public enum PointType
    {
        POINT,
        CONTROL
    }

    public class MotionPath
    {

        #region Proporties

        private UI.Simulator parentWindow { get; set; }
        private Viewport3DX viewport3d { get; set; }
        public List<double[]> ptArray = new List<double[]>();
        private PathType pathType;
        public List<CustomCurve> Paths = new List<CustomCurve>();
        public List<CustomCurve> PathsSync = new List<CustomCurve>();

        private Color trajectoryColor = Color.Yellow;
        private Color bezierpointColor = Color.Orange;
        private Color controlpointColor = Color.MediumBlue;
        private System.Windows.Media.Color controllineColor = System.Windows.Media.Colors.MediumBlue;
        private Color selectedColor = Color.Red;

        /// Motion Path Data
        private List<Trajectory> lineBezier = new List<Trajectory>();           /// list of curves (list number is curve number)
        public int nSelected = -1;                                              /// number of selected curve
        public Point3D ptSelected = new Point3D();                              /// position of selected point
        public bool bPointSelected = false;                                          /// true when selected point
        SelectedVisualType selectedVisual = SelectedVisualType.None;            /// string of the selected curve("point" or "control")
        public bool bCurveSelected = false;                                     /// true when selected curve
        public int nCurveSelected = -1;                                         /// number of the selected curve
        public bool jointChanged = false;                                       /// true when changed the location of bezier or control point
        public int nPoint = -1;
        public bool linkHandle = true;

        public SceneNodeGroupModel3D MainScene { get; set; }



        public int CurveCount
        {
            get { return Paths.Count; }
        }

        #endregion

        #region Constructor

        public MotionPath()
        {

        }

        public MotionPath(UI.Simulator _parentwindow, PathType _pathType, Color _trajectoryColor)
        {
            parentWindow = _parentwindow;
            viewport3d = parentWindow.Viewport;
            MainScene = parentWindow.MainScene;
            pathType = _pathType;
            trajectoryColor = _trajectoryColor;
        }

        #endregion

        public double GetTotLength()
        {
            double tempTot = 0;
            for (int i = 0; i < Paths.Count; i++)
                tempTot += Paths[i].lenCurve;
            return tempTot;
        }

        public void AddPathPoint(int curveId, double[] p, CurveType curveType, PointType pointType = PointType.POINT)
        {
            CustomCurve curve;
            Color mainColor = new Color();
            if (pointType == PointType.POINT)
                mainColor = bezierpointColor;
            else if (pointType == PointType.CONTROL)
                mainColor = controlpointColor;

            BezierMarkPoint3D ptVis3d = new BezierMarkPoint3D(curveId, pathType, pointType,
                new Point3D(p[0], p[1], p[2]), mainColor);
            ptVis3d.Transform = new TranslateTransform3D(new Vector3D(p[0], p[1], p[2]));

            int text_offset = 30;
            var text_position = new Point3D(p[0], p[1], p[2] + (text_offset * 2));
            var text_string = string.Format("{0}", ptArray.Count);
            var textVis3d = new BillboardText(text_string, text_position);

            if (CurveCount != 0)
            {
                if ((CurveCount != 1) || (curveId != 0))
                {
                    if (pointType == PointType.POINT)
                    {
                        curve = new CustomCurve
                        {
                            curveShape = curveType,
                            ptCurve = new List<BezierMarkPoint3D>(),
                            lblVisual3D = new List<BillboardText>()
                        };
                        curve.ptCurve.Add(ptVis3d);
                        curve.lblVisual3D.Add(textVis3d);
                        Paths.Add(curve);
                    }
                    else /// if (pointType == PointType.CONTROL)
                    {
                        curve = Paths[curveId];
                        if (ReferenceEquals(curve.ptAdjust, null))
                        {
                            curve.ptAdjust = new List<BezierMarkPoint3D>();
                        }
                        curve.ptAdjust.Add(ptVis3d);
                        Paths[curveId] = curve;
                    }
                }
                else
                {
                    if (pointType == PointType.POINT)
                    {
                        Paths[0].ptCurve.Add(ptVis3d);
                        Paths[0].lblVisual3D.Add(textVis3d);
                    }
                    else
                    {
                        curve = Paths[0];
                        if (ReferenceEquals(curve.ptAdjust, null))
                        {
                            curve.ptAdjust = new List<BezierMarkPoint3D>();
                        }
                        curve.ptAdjust.Add(ptVis3d);
                        Paths[0] = curve;
                    }
                }
            }
            else
            {
                curve = new CustomCurve
                {
                    curveShape = curveType,
                    ptCurve = new List<BezierMarkPoint3D>(),
                    lblVisual3D = new List<BillboardText>()
                };
                curve.ptCurve.Add(ptVis3d);
                curve.lblVisual3D.Add(textVis3d);
                Paths.Add(curve);
            }

            if (parentWindow.IsControlPointsEnabled || pointType == PointType.POINT)
                MainScene.AddNode(ptVis3d);
            if (parentWindow.IsBezierLabelEnabled && pointType == PointType.POINT)
                MainScene.AddNode(textVis3d.SceneNode);
        }

        public void MakeBezier(int curveId, bool redraw)
        {
            double[] temp = { 0, 0, 0 };
            int i = 0;
            if (curveId == 0) i = 1;
            int j = 0;
            if (curveId == 1) j = 1;

            temp[0] = Paths[curveId + i - 1].ptCurve[j].point.X +
                (int)(Paths[curveId].ptCurve[i].point.X - Paths[curveId + i - 1].ptCurve[j].point.X) * .25;
            temp[1] = Paths[curveId + i - 1].ptCurve[j].point.Y +
                (int)(Paths[curveId].ptCurve[i].point.Y - Paths[curveId + i - 1].ptCurve[j].point.Y) * .25;
            temp[2] = Paths[curveId + i - 1].ptCurve[j].point.Z +
                (int)(Paths[curveId].ptCurve[i].point.Z - Paths[curveId + i - 1].ptCurve[j].point.Z) * .25;
            if (redraw)
            {
                if (!bPointSelected)  // not the case when delete point
                {
                    if (Paths[curveId].curveShape == CurveType.BEZIER)
                    {
                        MainScene.RemoveNode(Paths[curveId].ptAdjust[0]);
                        MainScene.RemoveNode(Paths[curveId].ptAdjust[1]);
                        MainScene.RemoveNode(Paths[curveId].ctrlLine[0].SceneNode);
                        MainScene.RemoveNode(Paths[curveId].ctrlLine[1].SceneNode);
                    }
                    else if (Paths[curveId].curveShape == CurveType.ARC)
                        MainScene.RemoveNode(Paths[curveId].ptAdjust[0]);
                    MainScene.RemoveNode(lineBezier[curveId].SceneNode);
                }
                CustomCurve curve = Paths[curveId];
                curve.curveShape = CurveType.BEZIER;
                Paths[curveId] = curve;
                Paths[curveId].ptAdjust[0].Transform = new TranslateTransform3D(new Vector3D(temp[0], temp[1], temp[2]));
                Paths[curveId].ptAdjust[0].point = new Point3D(temp[0], temp[1], temp[2]);
                MainScene.AddNode(Paths[curveId].ptAdjust[0]);
            }
            else
                AddPathPoint(curveId, temp, CurveType.BEZIER, PointType.CONTROL);

            temp[0] = Paths[curveId + i - 1].ptCurve[j].point.X +
                (int)(Paths[curveId].ptCurve[i].point.X - Paths[curveId + i - 1].ptCurve[j].point.X) * .75;
            temp[1] = Paths[curveId + i - 1].ptCurve[j].point.Y +
                (int)(Paths[curveId].ptCurve[i].point.Y - Paths[curveId + i - 1].ptCurve[j].point.Y) * .75;
            temp[2] = Paths[curveId + i - 1].ptCurve[j].point.Z +
                (int)(Paths[curveId].ptCurve[i].point.Z - Paths[curveId + i - 1].ptCurve[j].point.Z) * .75;
            if (redraw)
            {
                Paths[curveId].ptAdjust[1].Transform = new TranslateTransform3D(new Vector3D(temp[0], temp[1], temp[2]));
                Paths[curveId].ptAdjust[1].point = new Point3D(temp[0], temp[1], temp[2]);
                MainScene.AddNode(Paths[curveId].ptAdjust[1]);

            }
            else
                AddPathPoint(curveId, temp, CurveType.BEZIER, PointType.CONTROL);
            DrawBezierCurveSegment(curveId, redraw);
            DrawControlCurveSegment(curveId, redraw);
        }

        public void MakeLine(int curveId, bool redraw)
        {
            if (redraw)
            {
                if (Paths[curveId].curveShape == CurveType.BEZIER)
                {
                    MainScene.RemoveNode(Paths[curveId].ptAdjust[0]);
                    MainScene.RemoveNode(Paths[curveId].ptAdjust[1]);
                    MainScene.RemoveNode(Paths[curveId].ctrlLine[0].SceneNode);
                    MainScene.RemoveNode(Paths[curveId].ctrlLine[1].SceneNode);
                }
                else if (Paths[curveId].curveShape == CurveType.ARC)
                    MainScene.RemoveNode(Paths[curveId].ptAdjust[0]);
                viewport3d.Items.Remove(lineBezier[curveId]);
                lineBezier[curveId] = null;
            }

            CustomCurve curve = Paths[curveId];
            curve.curveShape = CurveType.LINE;
            curve.curvePath = new List<Point3D>();
            Paths[curveId] = curve;
            if (redraw)
                lineBezier[curveId] = new Trajectory(curveId, pathType);
            else
                lineBezier.Add(new Trajectory(curveId, pathType));
            Point3D item = new Point3D();
            float t = 0f;
            int i = 0, j = 0;
            if (curveId == 0)
                i = 1;
            if (curveId == 1)
                j = 1;
            float dist = (float)CustomSpline.GetDistance(Paths[curveId].ptCurve[i].point, Paths[curveId + i - 1].ptCurve[j].point);
            float step = CustomSpline.spacing / dist;
            while (true)
            {
                if (t > 1f)
                {
                    break;
                }
                item = CustomSpline.GetSegmentPoint(Paths, curveId, t);
                Paths[curveId].curvePath.Add(item);
                t += step;
            }
            CustomCurve cur = Paths[curveId];
            cur.lenCurve = dist;
            Paths[curveId] = cur;
            for (i = 0; i < Paths[curveId].curvePath.Count - 1; i++)
            {
                lineBezier[curveId].Color = trajectoryColor;
                item = Paths[curveId].curvePath[i];
                lineBezier[curveId].Points.Add(item);
                item = Paths[curveId].curvePath[i + 1];
                lineBezier[curveId].Points.Add(item);
            }
            lineBezier[curveId].Update();
            viewport3d.Items.Add(lineBezier[curveId]);
        }

        public void MakeArc(int index, bool redraw)
        {

        }
        public void DrawBezierCurveSegment(int curveId, bool redraw)
        {
            CustomCurve curve = Paths[curveId];
            curve.curvePath = new List<Point3D>();
            Paths[curveId] = curve;
            if (redraw)
            {
                lineBezier[curveId] = new Trajectory(curveId, pathType);
            }
            else
            {
                lineBezier.Add(new Trajectory(curveId, pathType));
            }
            Point3D item = new Point3D();
            List<Point3D> pts = CustomSpline.GetBezierPoint(ref Paths, curveId);
            CustomCurve cur = Paths[curveId];
            cur.curvePath = pts;
            Paths[curveId] = cur;
            int num2 = 0;
            while (true)
            {
                if (num2 >= (Paths[curveId].curvePath.Count - 1))
                {
                    lineBezier[curveId].Update();
                    viewport3d.Items.Add(lineBezier[curveId]);
                    Console.WriteLine("LineBezier: {0}", lineBezier[curveId].Points.Count);
                    return;
                }
                lineBezier[curveId].Color = trajectoryColor;
                item = Paths[curveId].curvePath[num2];
                lineBezier[curveId].Points.Add(item);
                item = Paths[curveId].curvePath[num2 + 1];
                lineBezier[curveId].Points.Add(item);
                num2++;
            }
        }

        public void DrawControlCurveSegment(int curveId, bool redraw)
        {
            if (Paths[curveId].ptAdjust.Count == 2)
            {
                if (redraw)
                {
                    int num = 0;
                    while (true)
                    {
                        if (num >= 2)
                        {
                            break;
                        }
                        if (Paths[curveId].ctrlLine != null && Paths[curveId].ctrlLine[num] != null)
                        {
                            //if (viewport3d.Children.Contains(Paths[curveId].ctrlLine[num]))
                            MainScene.RemoveNode(Paths[curveId].ctrlLine[num].SceneNode);
                            Paths[curveId].ctrlLine[num] = null;
                        }
                        num++;
                    }
                }
                CustomCurve curve = new CustomCurve();
                curve = Paths[curveId];
                curve.ctrlLine = new List<ControlPointLine>();
                ControlPointLine item = new ControlPointLine
                {
                    Color = controllineColor,
                    Thickness = 2.0
                };
                item.Points = new System.Windows.Media.Media3D.Point3DCollection();
                item.Points.Add(Paths[curveId].ptAdjust[0].Position.ToPoint3D());
                if (curveId == 0)
                {
                    item.Points.Add(Paths[curveId].ptCurve[0].point);
                }
                else if (curveId == 1)
                {
                    item.Points.Add(Paths[curveId - 1].ptCurve[1].point);
                }
                else
                {
                    item.Points.Add(Paths[curveId - 1].ptCurve[0].point);
                }
                item.Update();
                curve.ctrlLine.Add(item);
                item = null;
                item = new ControlPointLine
                {
                    Color = controllineColor,
                    Thickness = 2.0
                };
                item.Points = new System.Windows.Media.Media3D.Point3DCollection();
                item.Points.Add(Paths[curveId].ptAdjust[1].Position.ToPoint3D());
                if (curveId == 0)
                {
                    item.Points.Add(Paths[curveId].ptCurve[1].point);
                }
                else
                {
                    item.Points.Add(Paths[curveId].ptCurve[0].point);
                }
                item.Update();
                curve.ctrlLine.Add(item);
                //if (showHandle)
                //{
                MainScene.AddNode(curve.ctrlLine[0].SceneNode);
                MainScene.AddNode(curve.ctrlLine[1].SceneNode);
                //}
                Paths[curveId] = curve;
            }
            if (Paths[curveId].curveShape == CurveType.ARC)
            {
                lineBezier[curveId] = new Trajectory(curveId, pathType);
                CustomCurve curve = new CustomCurve();
                List<Point3D> arcPoints = new List<Point3D>();
                arcPoints = CustomSpline.GetArcPoints(ref Paths, curveId, true);
                curve = Paths[curveId];
                curve.curvePath = arcPoints;
                Paths[curveId] = curve;

                Point3D pt = new Point3D();
                for (int i = 0; i < Paths[curveId].curvePath.Count - 1; i++)
                {
                    lineBezier[curveId].Color = trajectoryColor;
                    pt = Paths[curveId].curvePath[i];
                    lineBezier[curveId].Points.Add(pt);
                    pt = Paths[curveId].curvePath[i + 1];
                    lineBezier[curveId].Points.Add(pt);
                }
                lineBezier[curveId].Update();
                viewport3d.Items.Add(lineBezier[curveId]);
            }
        }

        public void onUpdateGraphics(Vector3 pt)
        {

            if (selectedVisual == SelectedVisualType.BezierMarkPoint)
            {
                Paths[nSelected].ptCurve[nPoint].point = pt.ToPoint3D();
                Paths[nSelected].ptCurve[nPoint].Transform = new TranslateTransform3D(new Vector3D(pt.X, pt.Y, pt.Z));

                Point3D lblPos = new Point3D();
                lblPos.X = pt.X + 30;
                lblPos.Y = pt.Y + 30;
                lblPos.Z = pt.Z + 60;
                Paths[nSelected].lblVisual3D[nPoint].Position = lblPos;
            }

            if (CurveCount == 1 && Paths[0].ptCurve.Count < 2)
                return;

            viewport3d.Items.Remove(lineBezier[nSelected]);
            lineBezier[nSelected] = null;
            Paths[nSelected].curvePath.Clear();
            if (Paths[nSelected].curveShape == CurveType.BEZIER)
            {
                if (nSelected == 0)
                {
                    MainScene.RemoveNode(Paths[nSelected].ctrlLine[nPoint].SceneNode);
                    Paths[nSelected].ctrlLine[nPoint] = null;
                }
                else if (selectedVisual == SelectedVisualType.BezierMarkPoint)
                {
                    MainScene.RemoveNode(Paths[nSelected].ctrlLine[1].SceneNode);
                    Paths[nSelected].ctrlLine[1] = null;
                }
                else
                {
                    MainScene.RemoveNode(Paths[nSelected].ctrlLine[nPoint].SceneNode);
                    Paths[nSelected].ctrlLine[nPoint] = null;
                }
            }

            if (nSelected == 0)
            {
                if ((Paths[nSelected].curveShape == CurveType.BEZIER) && ((nPoint == 1) && (CurveCount > (nSelected + 1))))
                {
                    if ((selectedVisual == SelectedVisualType.BezierMarkPoint) || linkHandle)
                    {
                        if (Paths[nSelected + 1].curveShape == CurveType.BEZIER)
                        {
                            MainScene.RemoveNode(Paths[nSelected + 1].ctrlLine[0].SceneNode);
                            Paths[nSelected + 1].ctrlLine[0] = null;
                        }
                        viewport3d.Items.Remove(lineBezier[nSelected + 1]);
                        lineBezier[nSelected + 1] = null;
                        Paths[nSelected + 1].curvePath.Clear();
                    }
                    if (linkHandle && (Paths[nSelected + 1].curveShape == CurveType.BEZIER))
                    {
                        double[] numArray = new double[] { (2.0 * Paths[nSelected].ptCurve[1].point.X) - Paths[nSelected].ptAdjust[1].point.X, (2.0 * Paths[nSelected].ptCurve[1].point.Y) - Paths[nSelected].ptAdjust[1].point.Y, (2.0 * Paths[nSelected].ptCurve[1].point.Z) - Paths[nSelected].ptAdjust[1].point.Z };
                        Paths[nSelected + 1].ptAdjust[0].Transform = new TranslateTransform3D(new Vector3D(numArray[0], numArray[1], numArray[2]));
                        Paths[nSelected + 1].ptAdjust[0].point = new Point3D(numArray[0], numArray[1], numArray[2]);
                    }
                    if (Paths[nSelected + 1].curveShape == CurveType.ARC && selectedVisual == SelectedVisualType.BezierMarkPoint)
                    {
                        Point3D pp = CustomSpline.getCircleCenter(Paths, nSelected + 1);
                        Paths[nSelected + 1].ptAdjust[0].Transform = new TranslateTransform3D(new Vector3D(pp.X, pp.Y, pp.Z));
                        Paths[nSelected + 1].ptAdjust[0].point = pp;
                    }
                }
                else if (Paths[nSelected].curveShape == CurveType.LINE && nPoint == 1 && (CurveCount > (nSelected + 1)))
                {
                    viewport3d.Items.Remove(lineBezier[nSelected + 1]);
                    lineBezier[nSelected + 1] = null;
                    Paths[nSelected + 1].curvePath.Clear();
                    if (Paths[nSelected + 1].curveShape == CurveType.ARC)
                    {
                        Point3D pp = CustomSpline.getCircleCenter(Paths, nSelected + 1);
                        Paths[nSelected + 1].ptAdjust[0].Transform = new TranslateTransform3D(new Vector3D(pp.X, pp.Y, pp.Z));
                        Paths[nSelected + 1].ptAdjust[0].point = pp;
                    }
                }
                else if (Paths[nSelected].curveShape == CurveType.ARC && selectedVisual == SelectedVisualType.BezierMarkPoint)
                {
                    Point3D pp = CustomSpline.getCircleCenter(Paths, 0);
                    Paths[0].ptAdjust[0].Transform = new TranslateTransform3D(new Vector3D(pp.X, pp.Y, pp.Z));
                    Paths[0].ptAdjust[0].point = pp;

                    if (nPoint == 1 && CurveCount > nSelected + 1)
                    {
                        viewport3d.Items.Remove(lineBezier[nSelected + 1]);
                        lineBezier[nSelected + 1] = null;
                        Paths[nSelected + 1].curvePath.Clear();
                        if (Paths[nSelected + 1].curveShape == CurveType.BEZIER)
                        {
                            MainScene.RemoveNode(Paths[nSelected + 1].ctrlLine[0].SceneNode);
                            Paths[nSelected + 1].ctrlLine[0] = null;
                        }
                        else if (Paths[nSelected + 1].curveShape == CurveType.ARC)
                        {
                            pp = CustomSpline.getCircleCenter(Paths, 1);
                            Paths[1].ptAdjust[1].Transform = new TranslateTransform3D(new Vector3D(pp.X, pp.Y, pp.Z));
                            Paths[1].ptAdjust[1].point = pp;
                        }
                    }
                }
            }
            else
            {
                if (Paths[nSelected].curveShape == CurveType.BEZIER)
                {
                    if ((nPoint == 1) || (selectedVisual == SelectedVisualType.BezierMarkPoint))
                    {
                        if (CurveCount > (nSelected + 1))
                        {
                            if ((selectedVisual == SelectedVisualType.BezierMarkPoint) || linkHandle)
                            {
                                if (Paths[nSelected + 1].curveShape == CurveType.BEZIER)
                                {
                                    MainScene.RemoveNode(Paths[nSelected + 1].ctrlLine[0].SceneNode);
                                    Paths[nSelected + 1].ctrlLine[0] = null;
                                    if (linkHandle)
                                    {
                                        double[] numArray2 = new double[] { (2.0 * Paths[nSelected].ptCurve[0].point.X) - Paths[nSelected].ptAdjust[1].point.X,
                                            (2.0 * Paths[nSelected].ptCurve[0].point.Y) - Paths[nSelected].ptAdjust[1].point.Y,
                                            (2.0 * Paths[nSelected].ptCurve[0].point.Z) - Paths[nSelected].ptAdjust[1].point.Z };
                                        Paths[nSelected + 1].ptAdjust[0].Transform = new TranslateTransform3D(new Vector3D(numArray2[0], numArray2[1], numArray2[2]));
                                        Paths[nSelected + 1].ptAdjust[0].point = new Point3D(numArray2[0], numArray2[1], numArray2[2]);
                                    }
                                }
                                viewport3d.Items.Remove(lineBezier[nSelected + 1]);
                                lineBezier[nSelected + 1] = null;
                                Paths[nSelected + 1].curvePath.Clear();
                            }
                            if (selectedVisual == SelectedVisualType.BezierMarkPoint && Paths[nSelected + 1].curveShape == CurveType.ARC)
                            {
                                Point3D pp = CustomSpline.getCircleCenter(Paths, nSelected + 1);
                                Paths[nSelected + 1].ptAdjust[0].Transform = new TranslateTransform3D(new Vector3D(pp.X, pp.Y, pp.Z));
                                Paths[nSelected + 1].ptAdjust[0].point = pp;
                            }
                        }
                    }
                    if ((nPoint == 0) && (selectedVisual == SelectedVisualType.ControlPoint) && (Paths[nSelected - 1].curveShape == CurveType.BEZIER) && linkHandle)
                    {
                        MainScene.RemoveNode(Paths[nSelected - 1].ctrlLine[1].SceneNode);
                        Paths[nSelected - 1].ctrlLine[1] = null;
                        viewport3d.Items.Remove(lineBezier[nSelected - 1]);
                        lineBezier[nSelected - 1] = null;
                        Paths[nSelected - 1].curvePath.Clear();
                        double[] numArray3 = new double[3];
                        if (nSelected == 1)
                        {
                            numArray3[0] = (2.0 * Paths[nSelected - 1].ptCurve[1].point.X) - Paths[nSelected].ptAdjust[0].point.X;
                            numArray3[1] = (2.0 * Paths[nSelected - 1].ptCurve[1].point.Y) - Paths[nSelected].ptAdjust[0].point.Y;
                            numArray3[2] = (2.0 * Paths[nSelected - 1].ptCurve[1].point.Z) - Paths[nSelected].ptAdjust[0].point.Z;
                        }
                        else
                        {
                            numArray3[0] = (2.0 * Paths[nSelected - 1].ptCurve[0].point.X) - Paths[nSelected].ptAdjust[0].point.X;
                            numArray3[1] = (2.0 * Paths[nSelected - 1].ptCurve[0].point.Y) - Paths[nSelected].ptAdjust[0].point.Y;
                            numArray3[2] = (2.0 * Paths[nSelected - 1].ptCurve[0].point.Z) - Paths[nSelected].ptAdjust[0].point.Z;
                        }
                        Paths[nSelected - 1].ptAdjust[1].Transform = new TranslateTransform3D(new Vector3D(numArray3[0], numArray3[1], numArray3[2]));
                        Paths[nSelected - 1].ptAdjust[1].point = new Point3D(numArray3[0], numArray3[1], numArray3[2]);
                    }
                }
                else if (Paths[nSelected].curveShape == CurveType.LINE)
                {
                    if (CurveCount > (nSelected + 1))
                    {
                        viewport3d.Items.Remove(lineBezier[nSelected + 1]);
                        lineBezier[nSelected + 1] = null;
                        Paths[nSelected + 1].curvePath.Clear();
                        if (Paths[nSelected + 1].curveShape == CurveType.BEZIER)
                        {
                            MainScene.RemoveNode(Paths[nSelected + 1].ctrlLine[0].SceneNode);
                            Paths[nSelected + 1].ctrlLine[0] = null;
                        }
                        if (Paths[nSelected + 1].curveShape == CurveType.ARC)
                        {
                            Point3D pp = CustomSpline.getCircleCenter(Paths, nSelected + 1);
                            Paths[nSelected + 1].ptAdjust[0].Transform = new TranslateTransform3D(new Vector3D(pp.X, pp.Y, pp.Z));
                            Paths[nSelected + 1].ptAdjust[0].point = pp;
                        }
                    }

                }
                else if (Paths[nSelected].curveShape == CurveType.ARC)
                {
                    if (selectedVisual == SelectedVisualType.BezierMarkPoint)
                    {
                        Point3D pp = CustomSpline.getCircleCenter(Paths, nSelected);
                        Paths[nSelected].ptAdjust[0].Transform = new TranslateTransform3D(new Vector3D(pp.X, pp.Y, pp.Z));
                        Paths[nSelected].ptAdjust[0].point = pp;

                        if (CurveCount > (nSelected + 1))
                        {
                            viewport3d.Items.Remove(lineBezier[nSelected + 1]);
                            lineBezier[nSelected + 1] = null;
                            Paths[nSelected + 1].curvePath.Clear();
                            if (Paths[nSelected + 1].curveShape == CurveType.BEZIER)
                            {
                                MainScene.RemoveNode(Paths[nSelected + 1].ctrlLine[0].SceneNode);
                                Paths[nSelected + 1].ctrlLine[0] = null;
                            }
                            if (Paths[nSelected + 1].curveShape == CurveType.ARC)
                            {
                                pp = CustomSpline.getCircleCenter(Paths, nSelected + 1);
                                Paths[nSelected + 1].ptAdjust[0].Transform = new TranslateTransform3D(new Vector3D(pp.X, pp.Y, pp.Z));
                                Paths[nSelected + 1].ptAdjust[0].point = pp;
                            }
                        }
                    }
                }
            }

            redraw();
            //updateTotDistance();
        }
        public void redraw()
        {
            for (int i = 0; i < CurveCount; i++)
            {
                CurveType shape = Paths[i].curveShape;
                if (Paths[i].curvePath.Count == 0)
                {

                    if (shape == CurveType.BEZIER)
                    {
                        DrawBezierCurveSegment(i, true);
                        for (int num2 = 0; num2 < 2; num2++)
                        {
                            if (Paths[i].ctrlLine[num2] == null)
                            {
                                DrawControlCurveSegment(i, true);
                            }
                        }
                    }
                    if (shape == CurveType.ARC)
                        MakeArc(i, true);
                    if (shape == CurveType.LINE)
                        MakeLine(i, true);

                }
            }
        }

        #region Events
        private void BezierMarkPoint_TransformChanged(object sender, TransformArgs e)
        {
            BezierMarkPoint3D tt = (BezierMarkPoint3D)sender;
            Console.WriteLine("Position : {0}", tt.Position);
            onUpdateGraphics(tt.Position);
        }

        public void OnMouseDown(object modelhit)
        {
            try
            {
                if (modelhit is BezierMarkPoint3D)
                {
                    var bpt = (BezierMarkPoint3D)modelhit;
                    if (bpt.Name is "POINT")
                        selectedVisual = SelectedVisualType.BezierMarkPoint; // SelectedVisualType.BezierMarkPoint;
                    else
                        selectedVisual = SelectedVisualType.ControlPoint;

                    bPointSelected = true;
                    bpt.TransformChanged += BezierMarkPoint_TransformChanged;

                    ptSelected = bpt.point;    ///Bezier Point
                    nSelected = bpt.curveNum;  ///Bezier Point

                                               /// Point Data
                    if (bpt.Name is "POINT")
                    {
                        for (int ii = 0; ii < Paths[nSelected].ptCurve.Count; ii++)
                        {
                            if (!(ptSelected == Paths[nSelected].ptCurve[ii].point))
                                continue;
                            nPoint = ii;
                            break;
                        }
                    }
                    else
                    {
                        for (int ii = 0; ii < Paths[nSelected].ptAdjust.Count; ii++)
                        {
                            if (!(ptSelected == Paths[nSelected].ptAdjust[ii].point))
                                continue;
                            nPoint = ii;
                            break;
                        }
                    }

                    /// Add Point Manipulator
                    PointManipulator3D man = new PointManipulator3D();
                    man.Position = bpt.Position;
                    man.Target = bpt;
                    MainScene.AddNode(man.SceneNode);
                }
                else if (modelhit is ControlPoint3D)
                {

                }
                else if (modelhit is Trajectory j)
                {
                    selectedVisual = SelectedVisualType.Trajectory;
                    bPointSelected = false;
                    bCurveSelected = true;
                    nCurveSelected = j.number;
                    lineBezier[nCurveSelected].Color = selectedColor;
                    parentWindow.btn_Target_AddArc.IsEnabled = true;
                    parentWindow.btn_Target_AddDefault.IsEnabled = true;
                }

            }
            catch (Exception e)
            {
                //ErrorLog($"TARGET OnMouseDown Error: {e.Message}");
            }
        }

        #endregion
    }
}
