using HelixToolkit.Wpf.SharpDX;
using Microsoft.Win32;
using SharpDX;
using System;
using System.Collections.Generic;
using Point3D = System.Windows.Media.Media3D.Point3D;
using Vector3D = System.Windows.Media.Media3D.Vector3D;
using TranslateTransform3D = System.Windows.Media.Media3D.TranslateTransform3D;

namespace MonkeyMotionControl.Simulator
{
    public class TargetPath
    {
        private UI.Simulator parentWindow { get; set; }
        public Viewport3DX viewport3d { get; set; }
        public SceneNodeGroupModel3D MainScene { get; set; }

        private List<double[]> ptArray = new List<double[]>();
        public List<Trajectory> lineBezier = new List<Trajectory>();            /// list of curves (list number is curve number)
        public List<CustomCurve> targetPaths = new List<CustomCurve>();    /// list of curve points (list number is curve number)
        public List<CustomCurve> targetPathsSync = new List<CustomCurve>();    /// list of curve points (list number is curve number)
        public bool bCurveSelected = false;
        public int nCurveSelected = -1;
        public bool bPointSelected = false;
        private SelectedVisualType strSelected = SelectedVisualType.None;
        public int nSelected = -1;
        public int nPoint = -1;
        public Point3D ptSelected = new Point3D();

        public bool linkHandle = true;
        public bool showHandle = true;
        public int oldCount = 0;

        private Color trajectoryColor = Color.Yellow;
        private Color bezierpointColor = Color.Orange;
        private Color controlpointColor = Color.MediumBlue;
        private System.Windows.Media.Color controllineColor = System.Windows.Media.Colors.MediumBlue;
        private Color selectedColor = Color.Red;

        public TargetPath(UI.Simulator _parentwindow)
        {
            parentWindow = _parentwindow;
            viewport3d = parentWindow.Viewport;
            MainScene = parentWindow.MainScene;
        }

        public int curveCount
        {
            get { return targetPaths.Count; }
        }

        public void drawBezierCurveSegment(int index, bool redraw)
        {
            CustomCurve curve = targetPaths[index];
            curve.curvePath = new List<Point3D>();
            targetPaths[index] = curve;
            if (redraw)
            {
                lineBezier[index] = new Trajectory(index, PathType.TARGETPATH);
            }
            else
            {
                lineBezier.Add(new Trajectory(index, PathType.TARGETPATH));
            }
            Point3D item = new Point3D();
            List<Point3D> pts = CustomSpline.GetBezierPoint(ref targetPaths, index);
            CustomCurve cur = targetPaths[index];
            cur.curvePath = pts;
            targetPaths[index] = cur;
            int num2 = 0;
            while (true)
            {
                if (num2 >= (targetPaths[index].curvePath.Count - 1))
                {
                    lineBezier[index].Update();
                    viewport3d.Items.Add(lineBezier[index]);
                    return;
                }
                lineBezier[index].Color = trajectoryColor;
                item = targetPaths[index].curvePath[num2];
                lineBezier[index].Points.Add(item);
                item = targetPaths[index].curvePath[num2 + 1];
                lineBezier[index].Points.Add(item);
                num2++;
            }
        }

        public void drawControlCurveSegment(int index, bool redraw)
        {
            if (targetPaths[index].ptAdjust.Count == 2)
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
                        if (targetPaths[index].ctrlLine != null && targetPaths[index].ctrlLine[num] != null)
                        {
                            //if (viewport3d.Children.Contains(targetPaths[index].ctrlLine[num]))
                                MainScene.RemoveNode(targetPaths[index].ctrlLine[num].SceneNode);
                            targetPaths[index].ctrlLine[num] = null;
                        }
                        num++;
                    }
                }
                CustomCurve curve = new CustomCurve();
                curve = targetPaths[index];
                curve.ctrlLine = new List<ControlPointLine>();
                ControlPointLine item = new ControlPointLine
                {
                    Color = controllineColor,
                    Thickness = 2.0
                };
                item.Points = new System.Windows.Media.Media3D.Point3DCollection();
                item.Points.Add(targetPaths[index].ptAdjust[0].Position.ToPoint3D());
                if (index == 0)
                {
                    item.Points.Add(targetPaths[index].ptCurve[0].point);
                }
                else if (index == 1)
                {
                    item.Points.Add(targetPaths[index - 1].ptCurve[1].point);
                }
                else
                {
                    item.Points.Add(targetPaths[index - 1].ptCurve[0].point);
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
                item.Points.Add(targetPaths[index].ptAdjust[1].Position.ToPoint3D());
                if (index == 0)
                {
                    item.Points.Add(targetPaths[index].ptCurve[1].point);
                }
                else
                {
                    item.Points.Add(targetPaths[index].ptCurve[0].point);
                }
                item.Update();
                curve.ctrlLine.Add(item);
                //if (showHandle)
                //{
                MainScene.AddNode(curve.ctrlLine[0].SceneNode);
                MainScene.AddNode(curve.ctrlLine[1].SceneNode);
                //}
                targetPaths[index] = curve;
            }
            if (targetPaths[index].curveShape == CurveType.ARC)
            {
                lineBezier[index] = new Trajectory(index, PathType.TARGETPATH);
                CustomCurve curve = new CustomCurve();
                List<Point3D> arcPoints = new List<Point3D>();
                arcPoints = CustomSpline.GetArcPoints(ref targetPaths, index, true);
                curve = targetPaths[index];
                curve.curvePath = arcPoints;
                targetPaths[index] = curve;

                Point3D pt = new Point3D();
                for (int i = 0; i < targetPaths[index].curvePath.Count - 1; i++)
                {
                    lineBezier[index].Color = trajectoryColor;
                    pt = targetPaths[index].curvePath[i];
                    lineBezier[index].Points.Add(pt);
                    pt = targetPaths[index].curvePath[i + 1];
                    lineBezier[index].Points.Add(pt);
                }
                lineBezier[index].Update();
                viewport3d.Items.Add(lineBezier[index]);
            }
        }

        public void addBezierPoint(int id, double[] p, PointType pointType = PointType.POINT)
        {
            CustomCurve curve;
            Color mainColor = new Color();
            if (pointType == PointType.POINT)
                mainColor = bezierpointColor;
            else if (pointType == PointType.CONTROL)
                mainColor = controlpointColor;

            BezierMarkPoint3D ptVis3d = new BezierMarkPoint3D(id, PathType.TARGETPATH, pointType, 
                new Point3D(p[0], p[1], p[2]), mainColor);
            ptVis3d.Transform = new TranslateTransform3D(new Vector3D(p[0], p[1], p[2]));

            int text_offset = 30;
            var text_position = new Point3D(p[0], p[1], p[2] + (text_offset * 2));
            var text_string = string.Format("{0}", ptArray.Count);
            var textVis3d = new BillboardText(text_string, text_position);

            if (curveCount != 0)
            {
                if ((curveCount != 1) || (id != 0))
                {
                    if (pointType == PointType.POINT)
                    {
                        curve = new CustomCurve
                        {
                            curveShape = CurveType.LINE,
                            ptCurve = new List<BezierMarkPoint3D>(),
                            lblVisual3D = new List<BillboardText>()
                        };
                        curve.ptCurve.Add(ptVis3d);
                        curve.lblVisual3D.Add(textVis3d);
                        targetPaths.Add(curve);
                    }
                    else /// if (name == PointType.CONTROL)
                    {
                        curve = targetPaths[id];
                        if (ReferenceEquals(curve.ptAdjust, null))
                        {
                            curve.ptAdjust = new List<BezierMarkPoint3D>();
                        }
                        curve.ptAdjust.Add(ptVis3d);
                        targetPaths[id] = curve;
                    }
                }
                else
                {
                    if (pointType == PointType.POINT)
                    {
                        targetPaths[0].ptCurve.Add(ptVis3d);
                        targetPaths[0].lblVisual3D.Add(textVis3d);
                    }
                    else
                    {
                        curve = targetPaths[0];
                        if (ReferenceEquals(curve.ptAdjust, null))
                        {
                            curve.ptAdjust = new List<BezierMarkPoint3D>();
                        }
                        curve.ptAdjust.Add(ptVis3d);
                        targetPaths[0] = curve;
                    }
                }
            }
            else
            {
                curve = new CustomCurve
                {
                    curveShape = CurveType.LINE,
                    ptCurve = new List<BezierMarkPoint3D>(),
                    lblVisual3D = new List<BillboardText>()
                };
                curve.ptCurve.Add(ptVis3d);
                curve.lblVisual3D.Add(textVis3d);
                targetPaths.Add(curve);
            }

            if (parentWindow.IsControlPointsEnabled || pointType == PointType.POINT)
                MainScene.AddNode(ptVis3d);
            if (parentWindow.IsBezierLabelEnabled && pointType == PointType.POINT)
                MainScene.AddNode(textVis3d.SceneNode);

        }

        public void makeBezier(int index)
        {
            double[] temp = { 0, 0, 0 };
            int i = 0;
            if (index == 0) i = 1;
            int j = 0;
            if (index == 1) j = 1;

            temp[0] = targetPaths[index + i - 1].ptCurve[j].point.X +
                (int)(targetPaths[index].ptCurve[i].point.X - targetPaths[index + i - 1].ptCurve[j].point.X) * .25;
            temp[1] = targetPaths[index + i - 1].ptCurve[j].point.Y +
                (int)(targetPaths[index].ptCurve[i].point.Y - targetPaths[index + i - 1].ptCurve[j].point.Y) * .25;
            temp[2] = targetPaths[index + i - 1].ptCurve[j].point.Z +
                (int)(targetPaths[index].ptCurve[i].point.Z - targetPaths[index + i - 1].ptCurve[j].point.Z) * .25;
            if (!bPointSelected)  // not the case when delete point
            {
                if (targetPaths[index].curveShape == CurveType.BEZIER)
                {
                    MainScene.RemoveNode(targetPaths[index].ptAdjust[0]);
                    MainScene.RemoveNode(targetPaths[index].ptAdjust[1]);
                    MainScene.RemoveNode(targetPaths[index].ctrlLine[0].SceneNode);
                    MainScene.RemoveNode(targetPaths[index].ctrlLine[1].SceneNode);
                }
                else if (targetPaths[index].curveShape == CurveType.ARC)
                    MainScene.RemoveNode(targetPaths[index].ptAdjust[0]);
                viewport3d.Items.Remove(lineBezier[index]);
            }
            CustomCurve curve = targetPaths[index];
            curve.curveShape = CurveType.BEZIER;
            targetPaths[index] = curve;
            targetPaths[index].ptAdjust[0].Position = new Vector3((float)temp[0], (float)temp[1], (float)temp[2]);
            targetPaths[index].ptAdjust[0].point = new Point3D(temp[0], temp[1], temp[2]);
            //if (!viewport3d.Children.Contains(targetPaths[index].ptAdjust[0]))
            MainScene.AddNode(targetPaths[index].ptAdjust[0]);

            temp[0] = targetPaths[index + i - 1].ptCurve[j].point.X +
                (int)(targetPaths[index].ptCurve[i].point.X - targetPaths[index + i - 1].ptCurve[j].point.X) * .75;
            temp[1] = targetPaths[index + i - 1].ptCurve[j].point.Y +
                (int)(targetPaths[index].ptCurve[i].point.Y - targetPaths[index + i - 1].ptCurve[j].point.Y) * .75;
            temp[2] = targetPaths[index + i - 1].ptCurve[j].point.Z +
                (int)(targetPaths[index].ptCurve[i].point.Z - targetPaths[index + i - 1].ptCurve[j].point.Z) * .75;
            targetPaths[index].ptAdjust[1].Position = new Vector3((float)temp[0], (float)temp[1], (float)temp[2]);
            targetPaths[index].ptAdjust[1].point = new Point3D(temp[0], temp[1], temp[2]);
            //if (!viewport3d.Children.Contains(targetPaths[index].ptAdjust[1]))
                MainScene.AddNode(targetPaths[index].ptAdjust[1]);

            drawBezierCurveSegment(index, true);
            drawControlCurveSegment(index, true);
        }

        public void updateTotDistance()
        {
            double tempTot = 0;
            for (int i = 0; i < targetPaths.Count; i++)
                tempTot += targetPaths[i].lenCurve;
            parentWindow.tb_total_length_target.Text = Math.Round(tempTot,2).ToString();
        }

        public void AddLinearDefault()
        {
            if (bCurveSelected)
            {
                bCurveSelected = false;
                makeLine(nCurveSelected);
                updateTotDistance();
            }
            else
            {
                double[] pt = new double[] { parentWindow.targetPoint.Position.X, parentWindow.targetPoint.Position.Y, parentWindow.targetPoint.Position.Z };
                if (ptArray.Count > 1)
                    if (pt == ptArray[ptArray.Count - 1])
                        return;
                ptArray.Add(pt);

                if (ptArray.Count == 1 || ptArray.Count == 2)
                {
                    addBezierPoint(0, pt);
                    if (ptArray.Count == 2)
                    {
                        addBezierPoint(0, new double[] { 0, 0, 0 }, PointType.CONTROL);
                        addBezierPoint(0, new double[] { 0, 0, 0 }, PointType.CONTROL);
                    }
                }
                else
                {
                    addBezierPoint(curveCount, pt);
                    addBezierPoint(curveCount - 1, new double[] { 0, 0, 0 }, PointType.CONTROL);
                    addBezierPoint(curveCount - 1, new double[] { 0, 0, 0 }, PointType.CONTROL);
                }
                if (ptArray.Count > 1)
                {
                    makeLine(ptArray.Count - 2, false);
                    parentWindow.btn_Target_Delete.IsEnabled = true;
                    parentWindow.btn_Target_Save.IsEnabled = true;

                    double dist = Convert.ToDouble(parentWindow.tb_total_length_target.Text);
                    dist += targetPaths[ptArray.Count - 2].lenCurve;
                    parentWindow.tb_total_length_target.Text = Math.Round(dist,2).ToString();
                }
                parentWindow.btn_Target_Reset.IsEnabled = true;
            }
        }

        public void makeArc(int index, bool redraw)
        {
            if (targetPaths[index].curveShape == CurveType.BEZIER)
            {
                MainScene.RemoveNode(targetPaths[index].ptAdjust[0]);
                MainScene.RemoveNode(targetPaths[index].ptAdjust[1]);
                MainScene.RemoveNode(targetPaths[index].ctrlLine[0].SceneNode);
                MainScene.RemoveNode(targetPaths[index].ctrlLine[1].SceneNode);
            }
            else if (targetPaths[index].curveShape == CurveType.ARC && !redraw)
                return;
            if (!redraw)
            {
                viewport3d.Items.Remove(lineBezier[index]);
                lineBezier[index] = null;
            }
            lineBezier[index] = new Trajectory(index, PathType.TARGETPATH);
            Point3D item = new Point3D();
            List<Point3D> arcPoints = CustomSpline.GetArcPoints(ref targetPaths, index, redraw);
            CustomCurve curve = targetPaths[index];
            curve.curveShape = CurveType.ARC;
            curve.curvePath = arcPoints;
            if (!redraw)
            {
                Point3D center = targetPaths[index].ptAdjust[0].point;

                targetPaths[index].ptAdjust[0].Transform = new TranslateTransform3D(new Vector3D(center.X, center.Y, center.Z));
                targetPaths[index].ptAdjust[0].point = center;

            }

            targetPaths[index] = curve;
            for (int i = 0; i < targetPaths[index].curvePath.Count - 1; i++)
            {
                lineBezier[index].Color = trajectoryColor;
                item = targetPaths[index].curvePath[i];
                lineBezier[index].Points.Add(item);
                item = targetPaths[index].curvePath[i + 1];
                lineBezier[index].Points.Add(item);
            }
            viewport3d.Items.Remove(lineBezier[index]);
            if (showHandle && !bPointSelected)
                MainScene.AddNode(targetPaths[index].ptAdjust[0]);
        }

        public void drawArc(int index)
        {
            lineBezier.Add(new Trajectory(index, PathType.TARGETPATH));
            Point3D item = new Point3D();
            List<Point3D> arcPoints = CustomSpline.GetArcPoints(ref targetPaths, index, true);
            CustomCurve curve = targetPaths[index];
            curve.curveShape = CurveType.ARC;
            curve.curvePath = arcPoints;
            //if (!redraw)
            //{
            //    Point3D center = targetPaths[index].ptAdjust[0].point;

            //    targetPaths[index].ptAdjust[0].Transform = new TranslateTransform3D(new Vector3D(center.X, center.Y, center.Z));
            //    targetPaths[index].ptAdjust[0].point = center;

            //}

            targetPaths[index] = curve;
            for (int i = 0; i < targetPaths[index].curvePath.Count - 1; i++)
            {
                lineBezier[index].Color = trajectoryColor;
                item = targetPaths[index].curvePath[i];
                lineBezier[index].Points.Add(item);
                item = targetPaths[index].curvePath[i + 1];
                lineBezier[index].Points.Add(item);
            }
            viewport3d.Items.Remove(lineBezier[index]);
        }

        public void makeLine(int index, bool redraw = true)
        {
            if (redraw)
            {
                if (targetPaths[index].curveShape == CurveType.BEZIER)
                {
                    MainScene.RemoveNode(targetPaths[index].ptAdjust[0]);
                    MainScene.RemoveNode(targetPaths[index].ptAdjust[1]);
                    MainScene.RemoveNode(targetPaths[index].ctrlLine[0].SceneNode);
                    MainScene.RemoveNode(targetPaths[index].ctrlLine[1].SceneNode);
                }
                else if (targetPaths[index].curveShape == CurveType.ARC)
                    MainScene.RemoveNode(targetPaths[index].ptAdjust[0]);
                viewport3d.Items.Remove(lineBezier[index]);
                lineBezier[index] = null;
            }

            CustomCurve curve = targetPaths[index];
            curve.curveShape = CurveType.LINE;
            curve.curvePath = new List<Point3D>();
            targetPaths[index] = curve;
            if (redraw)
                lineBezier[index] = new Trajectory(index, PathType.TARGETPATH);
            else
                lineBezier.Add(new Trajectory(index, PathType.TARGETPATH));
            Point3D item = new Point3D();
            float t = 0f;
            int i = 0, j = 0;
            if (index == 0)
                i = 1;
            if (index == 1)
                j = 1;
            float dist = (float)CustomSpline.GetDistance(targetPaths[index].ptCurve[i].point, targetPaths[index + i - 1].ptCurve[j].point);
            float step = CustomSpline.spacing / dist;
            while (true)
            {
                if (t > 1f)
                {
                    break;
                }
                item = CustomSpline.GetSegmentPoint(targetPaths, index, t);
                targetPaths[index].curvePath.Add(item);
                t += step;
            }
            CustomCurve cur = targetPaths[index];
            cur.lenCurve = dist;
            targetPaths[index] = cur;
            for (i = 0; i < targetPaths[index].curvePath.Count - 1; i++)
            {
                lineBezier[index].Color = trajectoryColor;
                item = targetPaths[index].curvePath[i];
                lineBezier[index].Points.Add(item);
                item = targetPaths[index].curvePath[i + 1];
                lineBezier[index].Points.Add(item);
            }
            lineBezier[index].Update();
            viewport3d.Items.Add(lineBezier[index]);
        }

        public void drawSegment(int index)
        {
            lineBezier.Add(new Trajectory(index, PathType.TARGETPATH));
            CustomCurve curve = targetPaths[index];
            curve.curveShape = CurveType.LINE;
            curve.curvePath = new List<Point3D>();
            targetPaths[index] = curve;
            lineBezier[index] = new Trajectory(index, PathType.TARGETPATH);
            Point3D item = new Point3D();
            float t = 0f;
            int i = 0, j = 0;
            if (index == 0)
                i = 1;
            if (index == 1)
                j = 1;
            float dist = (float)CustomSpline.GetDistance(targetPaths[index].ptCurve[i].point, targetPaths[index + i - 1].ptCurve[j].point);
            float step = CustomSpline.spacing / dist;
            while (true)
            {
                if (t > 1f)
                {
                    break;
                }
                item = CustomSpline.GetSegmentPoint(targetPaths, index, t);
                targetPaths[index].curvePath.Add(item);
                t += step;
            }
            CustomCurve cur = targetPaths[index];
            cur.lenCurve = dist;
            targetPaths[index] = cur;
            for (i = 0; i < targetPaths[index].curvePath.Count - 1; i++)
            {
                lineBezier[index].Color = trajectoryColor;
                item = targetPaths[index].curvePath[i];
                lineBezier[index].Points.Add(item);
                item = targetPaths[index].curvePath[i + 1];
                lineBezier[index].Points.Add(item);
            }
            viewport3d.Items.Remove(lineBezier[index]);
        }

        public void redraw()
        {
            for (int i = 0; i < curveCount; i++)
            {
                CurveType shape = targetPaths[i].curveShape;
                if (targetPaths[i].curvePath.Count == 0)
                {

                    if (shape == CurveType.BEZIER)
                    {
                        drawBezierCurveSegment(i, true);
                        for (int num2 = 0; num2 < 2; num2++)
                        {
                            if (targetPaths[i].ctrlLine[num2] == null)
                            {
                                drawControlCurveSegment(i, true);
                            }
                        }
                    }
                    if (shape == CurveType.ARC)
                        makeArc(i, true);
                    if (shape == CurveType.LINE)
                        makeLine(i);

                }
            }
        }

        public void onUpdateGraphics(Vector3 pt)
        {

            if (strSelected == SelectedVisualType.BezierMarkPoint)
            {
                targetPaths[nSelected].ptCurve[nPoint].point = pt.ToPoint3D();
                targetPaths[nSelected].ptCurve[nPoint].Transform = new TranslateTransform3D(new Vector3D(pt.X, pt.Y, pt.Z));

                Point3D lblPos = new Point3D();
                lblPos.X = pt.X + 30;
                lblPos.Y = pt.Y + 30;
                lblPos.Z = pt.Z + 60;
                targetPaths[nSelected].lblVisual3D[nPoint].Position = lblPos;
            }

            if (curveCount == 1 && targetPaths[0].ptCurve.Count < 2)
                return;

            viewport3d.Items.Remove(lineBezier[nSelected]);
            lineBezier[nSelected] = null;
            targetPaths[nSelected].curvePath.Clear();
            if (targetPaths[nSelected].curveShape == CurveType.BEZIER)
            {
                if (nSelected == 0)
                {
                    MainScene.RemoveNode(targetPaths[nSelected].ctrlLine[nPoint].SceneNode);
                    targetPaths[nSelected].ctrlLine[nPoint] = null;
                }
                else if (strSelected == SelectedVisualType.BezierMarkPoint)
                {
                    MainScene.RemoveNode(targetPaths[nSelected].ctrlLine[1].SceneNode);
                    targetPaths[nSelected].ctrlLine[1] = null;
                }
                else
                {
                    MainScene.RemoveNode(targetPaths[nSelected].ctrlLine[nPoint].SceneNode);
                    targetPaths[nSelected].ctrlLine[nPoint] = null;
                }
            }

            if (nSelected == 0)
            {
                if ((targetPaths[nSelected].curveShape == CurveType.BEZIER) && ((nPoint == 1) && (curveCount > (nSelected + 1))))
                {
                    if ((strSelected == SelectedVisualType.BezierMarkPoint) || linkHandle)
                    {
                        if (targetPaths[nSelected + 1].curveShape == CurveType.BEZIER)
                        {
                            MainScene.RemoveNode(targetPaths[nSelected + 1].ctrlLine[0].SceneNode);
                            targetPaths[nSelected + 1].ctrlLine[0] = null;
                        }
                        viewport3d.Items.Remove(lineBezier[nSelected + 1]);
                        lineBezier[nSelected + 1] = null;
                        targetPaths[nSelected + 1].curvePath.Clear();
                    }
                    if (linkHandle && (targetPaths[nSelected + 1].curveShape == CurveType.BEZIER))
                    {
                        double[] numArray = new double[] { (2.0 * targetPaths[nSelected].ptCurve[1].point.X) - targetPaths[nSelected].ptAdjust[1].point.X, (2.0 * targetPaths[nSelected].ptCurve[1].point.Y) - targetPaths[nSelected].ptAdjust[1].point.Y, (2.0 * targetPaths[nSelected].ptCurve[1].point.Z) - targetPaths[nSelected].ptAdjust[1].point.Z };
                        targetPaths[nSelected + 1].ptAdjust[0].Transform = new TranslateTransform3D(new Vector3D(numArray[0], numArray[1], numArray[2]));
                        targetPaths[nSelected + 1].ptAdjust[0].point = new Point3D(numArray[0], numArray[1], numArray[2]);
                    }
                    if (targetPaths[nSelected + 1].curveShape == CurveType.ARC && strSelected == SelectedVisualType.BezierMarkPoint)
                    {
                        Point3D pp = CustomSpline.getCircleCenter(targetPaths, nSelected + 1);
                        targetPaths[nSelected + 1].ptAdjust[0].Transform = new TranslateTransform3D(new Vector3D(pp.X, pp.Y, pp.Z));
                        targetPaths[nSelected + 1].ptAdjust[0].point = pp;
                    }
                }
                else if (targetPaths[nSelected].curveShape == CurveType.LINE && nPoint == 1 && (curveCount > (nSelected + 1)))
                {
                    viewport3d.Items.Remove(lineBezier[nSelected + 1]);
                    lineBezier[nSelected + 1] = null;
                    targetPaths[nSelected + 1].curvePath.Clear();
                    if (targetPaths[nSelected + 1].curveShape == CurveType.ARC)
                    {
                        Point3D pp = CustomSpline.getCircleCenter(targetPaths, nSelected + 1);
                        targetPaths[nSelected + 1].ptAdjust[0].Transform = new TranslateTransform3D(new Vector3D(pp.X, pp.Y, pp.Z));
                        targetPaths[nSelected + 1].ptAdjust[0].point = pp;
                    }
                }
                else if (targetPaths[nSelected].curveShape == CurveType.ARC && strSelected == SelectedVisualType.BezierMarkPoint)
                {
                    Point3D pp = CustomSpline.getCircleCenter(targetPaths, 0);
                    targetPaths[0].ptAdjust[0].Transform = new TranslateTransform3D(new Vector3D(pp.X, pp.Y, pp.Z));
                    targetPaths[0].ptAdjust[0].point = pp;

                    if (nPoint == 1 && curveCount > nSelected + 1)
                    {
                        viewport3d.Items.Remove(lineBezier[nSelected + 1]);
                        lineBezier[nSelected + 1] = null;
                        targetPaths[nSelected + 1].curvePath.Clear();
                        if (targetPaths[nSelected + 1].curveShape == CurveType.BEZIER)
                        {
                            MainScene.RemoveNode(targetPaths[nSelected + 1].ctrlLine[0].SceneNode);
                            targetPaths[nSelected + 1].ctrlLine[0] = null;
                        }
                        else if (targetPaths[nSelected + 1].curveShape == CurveType.ARC)
                        {
                            pp = CustomSpline.getCircleCenter(targetPaths, 1);
                            targetPaths[1].ptAdjust[1].Transform = new TranslateTransform3D(new Vector3D(pp.X, pp.Y, pp.Z));
                            targetPaths[1].ptAdjust[1].point = pp;
                        }
                    }
                }
            }
            else
            {
                if (targetPaths[nSelected].curveShape == CurveType.BEZIER)
                {
                    if ((nPoint == 1) || (strSelected == SelectedVisualType.BezierMarkPoint))
                    {
                        if (curveCount > (nSelected + 1))
                        {
                            if ((strSelected == SelectedVisualType.BezierMarkPoint) || linkHandle)
                            {
                                if (targetPaths[nSelected + 1].curveShape == CurveType.BEZIER)
                                {
                                    MainScene.RemoveNode(targetPaths[nSelected + 1].ctrlLine[0].SceneNode);
                                    targetPaths[nSelected + 1].ctrlLine[0] = null;
                                    if (linkHandle)
                                    {
                                        double[] numArray2 = new double[] { (2.0 * targetPaths[nSelected].ptCurve[0].point.X) - targetPaths[nSelected].ptAdjust[1].point.X,
                                            (2.0 * targetPaths[nSelected].ptCurve[0].point.Y) - targetPaths[nSelected].ptAdjust[1].point.Y,
                                            (2.0 * targetPaths[nSelected].ptCurve[0].point.Z) - targetPaths[nSelected].ptAdjust[1].point.Z };
                                        targetPaths[nSelected + 1].ptAdjust[0].Transform = new TranslateTransform3D(new Vector3D(numArray2[0], numArray2[1], numArray2[2]));
                                        targetPaths[nSelected + 1].ptAdjust[0].point = new Point3D(numArray2[0], numArray2[1], numArray2[2]);
                                    }
                                }
                                viewport3d.Items.Remove(lineBezier[nSelected + 1]);
                                lineBezier[nSelected + 1] = null;
                                targetPaths[nSelected + 1].curvePath.Clear();
                            }
                            if (strSelected == SelectedVisualType.BezierMarkPoint && targetPaths[nSelected + 1].curveShape == CurveType.ARC)
                            {
                                Point3D pp = CustomSpline.getCircleCenter(targetPaths, nSelected + 1);
                                targetPaths[nSelected + 1].ptAdjust[0].Transform = new TranslateTransform3D(new Vector3D(pp.X, pp.Y, pp.Z));
                                targetPaths[nSelected + 1].ptAdjust[0].point = pp;
                            }
                        }
                    }
                    if ((nPoint == 0) && (strSelected == SelectedVisualType.ControlPoint) && (targetPaths[nSelected - 1].curveShape == CurveType.BEZIER) && linkHandle)
                    {
                        MainScene.RemoveNode(targetPaths[nSelected - 1].ctrlLine[1].SceneNode);
                        targetPaths[nSelected - 1].ctrlLine[1] = null;
                        viewport3d.Items.Remove(lineBezier[nSelected - 1]);
                        lineBezier[nSelected - 1] = null;
                        targetPaths[nSelected - 1].curvePath.Clear();
                        double[] numArray3 = new double[3];
                        if (nSelected == 1)
                        {
                            numArray3[0] = (2.0 * targetPaths[nSelected - 1].ptCurve[1].point.X) - targetPaths[nSelected].ptAdjust[0].point.X;
                            numArray3[1] = (2.0 * targetPaths[nSelected - 1].ptCurve[1].point.Y) - targetPaths[nSelected].ptAdjust[0].point.Y;
                            numArray3[2] = (2.0 * targetPaths[nSelected - 1].ptCurve[1].point.Z) - targetPaths[nSelected].ptAdjust[0].point.Z;
                        }
                        else
                        {
                            numArray3[0] = (2.0 * targetPaths[nSelected - 1].ptCurve[0].point.X) - targetPaths[nSelected].ptAdjust[0].point.X;
                            numArray3[1] = (2.0 * targetPaths[nSelected - 1].ptCurve[0].point.Y) - targetPaths[nSelected].ptAdjust[0].point.Y;
                            numArray3[2] = (2.0 * targetPaths[nSelected - 1].ptCurve[0].point.Z) - targetPaths[nSelected].ptAdjust[0].point.Z;
                        }
                        targetPaths[nSelected - 1].ptAdjust[1].Transform = new TranslateTransform3D(new Vector3D(numArray3[0], numArray3[1], numArray3[2]));
                        targetPaths[nSelected - 1].ptAdjust[1].point = new Point3D(numArray3[0], numArray3[1], numArray3[2]);
                    }
                }
                else if (targetPaths[nSelected].curveShape == CurveType.LINE)
                {
                    if (curveCount > (nSelected + 1))
                    {
                        viewport3d.Items.Remove(lineBezier[nSelected + 1]);
                        lineBezier[nSelected + 1] = null;
                        targetPaths[nSelected + 1].curvePath.Clear();
                        if (targetPaths[nSelected + 1].curveShape == CurveType.BEZIER)
                        {
                            MainScene.RemoveNode(targetPaths[nSelected + 1].ctrlLine[0].SceneNode);
                            targetPaths[nSelected + 1].ctrlLine[0] = null;
                        }
                        if (targetPaths[nSelected + 1].curveShape == CurveType.ARC)
                        {
                            Point3D pp = CustomSpline.getCircleCenter(targetPaths, nSelected + 1);
                            targetPaths[nSelected + 1].ptAdjust[0].Transform = new TranslateTransform3D(new Vector3D(pp.X, pp.Y, pp.Z));
                            targetPaths[nSelected + 1].ptAdjust[0].point = pp;
                        }
                    }

                }
                else if (targetPaths[nSelected].curveShape == CurveType.ARC)
                {
                    if (strSelected == SelectedVisualType.BezierMarkPoint)
                    {
                        Point3D pp = CustomSpline.getCircleCenter(targetPaths, nSelected);
                        targetPaths[nSelected].ptAdjust[0].Transform = new TranslateTransform3D(new Vector3D(pp.X, pp.Y, pp.Z));
                        targetPaths[nSelected].ptAdjust[0].point = pp;

                        if (curveCount > (nSelected + 1))
                        {
                            viewport3d.Items.Remove(lineBezier[nSelected + 1]);
                            lineBezier[nSelected + 1] = null;
                            targetPaths[nSelected + 1].curvePath.Clear();
                            if (targetPaths[nSelected + 1].curveShape == CurveType.BEZIER)
                            {
                                MainScene.RemoveNode(targetPaths[nSelected + 1].ctrlLine[0].SceneNode);
                                targetPaths[nSelected + 1].ctrlLine[0] = null;
                            }
                            if (targetPaths[nSelected + 1].curveShape == CurveType.ARC)
                            {
                                pp = CustomSpline.getCircleCenter(targetPaths, nSelected + 1);
                                targetPaths[nSelected + 1].ptAdjust[0].Transform = new TranslateTransform3D(new Vector3D(pp.X, pp.Y, pp.Z));
                                targetPaths[nSelected + 1].ptAdjust[0].point = pp;
                            }
                        }
                    }
                }
            }

            redraw();
            updateTotDistance();
        }

        public void deleteAndUpdate()
        {
            CustomCurve curve = new CustomCurve();
            BezierMarkPoint3D tempPt = new BezierMarkPoint3D();

            if (curveCount == 1)
            {
                if (targetPaths[0].ptCurve.Count == 1) reset();
                else
                {
                    viewport3d.Items.Remove(lineBezier[0]);
                    if (targetPaths[0].curveShape == CurveType.BEZIER)
                    {
                        MainScene.RemoveNode(targetPaths[0].ctrlLine[0].SceneNode);
                        MainScene.RemoveNode(targetPaths[0].ctrlLine[1].SceneNode);
                        MainScene.RemoveNode(targetPaths[0].ptAdjust[0]);
                        MainScene.RemoveNode(targetPaths[0].ptAdjust[1]);
                    }
                    else if (targetPaths[0].curveShape == CurveType.ARC)
                    {
                        MainScene.RemoveNode(targetPaths[0].ptAdjust[0]);
                    }
                    MainScene.RemoveNode(targetPaths[0].ptCurve[nPoint]);
                    MainScene.RemoveNode(targetPaths[0].lblVisual3D[nPoint].SceneNode);

                    curve.curveShape = CurveType.BEZIER;
                    curve.ptCurve = new List<BezierMarkPoint3D>();
                    curve.ptCurve.Add(targetPaths[0].ptCurve[1 - nPoint]);

                    curve.lblVisual3D = new List<BillboardText>();
                    curve.lblVisual3D.Add(targetPaths[0].lblVisual3D[1 - nPoint]);


                    targetPaths[0] = curve;

                    double[] temp = ptArray[1 - nPoint];
                    ptArray.Clear();
                    ptArray.Add(temp);

                    lineBezier.Clear();
                }
                renameLbl();
                return;
            }


            for (int i = 0; i < 2; i++)
            {
                viewport3d.Items.Remove(lineBezier[nSelected + i]);
                if (targetPaths[nSelected + i].curveShape == CurveType.BEZIER)
                {
                    MainScene.RemoveNode(targetPaths[nSelected + i].ctrlLine[0].SceneNode);
                    MainScene.RemoveNode(targetPaths[nSelected + i].ctrlLine[1].SceneNode);
                    MainScene.RemoveNode(targetPaths[nSelected + i].ptAdjust[0]);
                    MainScene.RemoveNode(targetPaths[nSelected + i].ptAdjust[1]);
                }
                else if (targetPaths[nSelected + i].curveShape == CurveType.ARC)
                {
                    MainScene.RemoveNode(targetPaths[nSelected + i].ptAdjust[0]);
                }

                if (nSelected + 1 == curveCount || (nSelected == 0 && nPoint == 0))
                    break;
            }

            if (nSelected == 0 && nPoint == 0)
            {
                tempPt = targetPaths[nSelected].ptCurve[1 - nPoint];
                var textVis3d = targetPaths[nSelected].lblVisual3D[1];
                textVis3d.Text = "1";

                MainScene.RemoveNode(targetPaths[nSelected].ptCurve[0]);
                MainScene.RemoveNode(targetPaths[nSelected].ptCurve[1]);
                MainScene.RemoveNode(targetPaths[nSelected].lblVisual3D[0].SceneNode);

                if (nPoint == 0)
                {
                    curve = targetPaths[nSelected + 1];
                    curve.ptCurve.Add(tempPt);
                    tempPt = curve.ptCurve[0];
                    curve.ptCurve[0] = curve.ptCurve[1];
                    curve.ptCurve[1] = tempPt;

                    curve.lblVisual3D.Add(textVis3d);
                    textVis3d = curve.lblVisual3D[0];
                    curve.lblVisual3D[0] = curve.lblVisual3D[1];
                    curve.lblVisual3D[1] = textVis3d;

                    MainScene.AddNode(curve.ptCurve[0]);
                    targetPaths[nSelected + 1] = curve;

                    for (int i = 0; i < curveCount - 1; i++)
                    {
                        targetPaths[i] = targetPaths[i + 1];
                        targetPaths[i].ptAdjust[0].curveNum = i;
                        targetPaths[i].ptAdjust[1].curveNum = i;
                        targetPaths[i].ptCurve[0].curveNum = i;
                        if (i == 0)
                            targetPaths[i].ptCurve[1].curveNum = i;
                        lineBezier[i] = lineBezier[i + 1];
                        lineBezier[i].number = i;
                    }

                    for (int i = 0; i < ptArray.Count - 1; i++)
                        ptArray[i] = ptArray[i + 1];
                }
            }
            else if (nSelected == curveCount - 1)
            {
                MainScene.RemoveNode(targetPaths[nSelected].ptCurve[0]);
                MainScene.RemoveNode(targetPaths[nSelected].lblVisual3D[0].SceneNode);
            }
            else
            {
                int k = 0;
                if (nSelected == 0)
                    k = 1;
                MainScene.RemoveNode(targetPaths[nSelected].ptCurve[k]);
                MainScene.RemoveNode(targetPaths[nSelected].lblVisual3D[k].SceneNode);
                curve = targetPaths[nSelected];
                curve.ptCurve[k] = targetPaths[nSelected + 1].ptCurve[0];
                curve.lblVisual3D[k] = targetPaths[nSelected + 1].lblVisual3D[0];

                targetPaths[nSelected] = curve;
                targetPaths[nSelected].ptCurve[0].curveNum = nSelected;
                makeBezier(nSelected);
                for (int i = nSelected; i < curveCount - 1; i++)
                {
                    if (i != nSelected)
                    {
                        targetPaths[i] = targetPaths[i + 1];
                        lineBezier[i] = lineBezier[i + 1];
                    }
                    targetPaths[i].ptAdjust[0].curveNum = i;
                    targetPaths[i].ptAdjust[1].curveNum = i;
                    targetPaths[i].ptCurve[0].curveNum = i;
                    if (i == 0)
                        targetPaths[i].ptCurve[1].curveNum = i;
                    lineBezier[i].number = i;
                }
                for (int i = nSelected + 1; i < ptArray.Count - 1; i++)
                    ptArray[i] = ptArray[i + 1];
            }

            renameLbl();
            ptArray.RemoveAt(ptArray.Count - 1);
            lineBezier.RemoveAt(curveCount - 1);
            targetPaths.RemoveAt(curveCount - 1);
        }

        private void renameLbl()
        {
            if (targetPaths.Count == 0) return;
            targetPaths[0].lblVisual3D[0].Text = "1";
            if (targetPaths.Count == 1 && targetPaths[0].ptCurve.Count == 1) return;
            for (int i = 0; i < curveCount; i++)
            {
                int jj = 0;
                if (i == 0)
                {
                    jj = 1;
                }
                targetPaths[i].lblVisual3D[jj].Text = string.Format("{0}", i + 2);
            }
        }

        public void removeVisual()
        {
            if (targetPaths.Count == 0)
                return;
            else if (targetPaths.Count == 1 && targetPaths[0].ptCurve.Count == 1)
            {
                MainScene.RemoveNode(targetPaths[0].ptCurve[0]);
                MainScene.RemoveNode(targetPaths[0].lblVisual3D[0].SceneNode);
                return;
            }

            for (int i = 0; i < targetPaths.Count; i++)
            {
                MainScene.RemoveNode(lineBezier[i].SceneNode);
                MainScene.RemoveNode(targetPaths[i].ptCurve[0]);
                MainScene.RemoveNode(targetPaths[i].lblVisual3D[0].SceneNode);
                if (i == 0)
                {
                    MainScene.RemoveNode(targetPaths[i].ptCurve[1]);
                    MainScene.RemoveNode(targetPaths[i].lblVisual3D[1].SceneNode);
                }
                if (targetPaths[i].curveShape == CurveType.BEZIER)
                {
                    MainScene.RemoveNode(targetPaths[i].ptAdjust[0]);
                    MainScene.RemoveNode(targetPaths[i].ptAdjust[1]);
                    MainScene.RemoveNode(targetPaths[i].ctrlLine[0].SceneNode);
                    MainScene.RemoveNode(targetPaths[i].ctrlLine[1].SceneNode);
                }
                else if (targetPaths[i].curveShape == CurveType.ARC)
                {
                    MainScene.RemoveNode(targetPaths[i].ptAdjust[0]);
                }
            }
        }

        private void addVisual()
        {
            for (int i = oldCount; i < targetPaths.Count; i++)
            {
                MainScene.AddNode(targetPaths[i].ptCurve[0]);
                MainScene.AddNode(targetPaths[i].lblVisual3D[0].SceneNode);
                if (i == 0)
                {
                    MainScene.AddNode(targetPaths[i].ptCurve[1]);
                    MainScene.AddNode(targetPaths[i].lblVisual3D[1].SceneNode);
                }
                if (oldCount > 0 && i == oldCount)
                    continue;
                if (parentWindow.IsControlPointsEnabled)
                {
                    if (targetPaths[i].curveShape == CurveType.BEZIER)
                    {
                        MainScene.AddNode(targetPaths[i].ptAdjust[0]);
                        MainScene.AddNode(targetPaths[i].ptAdjust[1]);
                    }
                    else if (targetPaths[i].curveShape == CurveType.ARC)
                    {
                        MainScene.AddNode(targetPaths[i].ptAdjust[0]);
                    }
                }
            }
        }

        public void reset()
        {
            removeVisual();
            lineBezier.Clear();
            targetPaths.Clear();
            ptArray.Clear();
        }

        public void OnMouseDown(object modelhit)
        {
            try
            {
                if (modelhit is BezierMarkPoint3D)
                {
                    var bpt = (BezierMarkPoint3D)modelhit;
                    if (bpt.Name is "POINT")
                        strSelected = SelectedVisualType.BezierMarkPoint; // SelectedVisualType.BezierMarkPoint;
                    else
                        strSelected = SelectedVisualType.ControlPoint;

                    bPointSelected = true;
                    bpt.TransformChanged += BezierMarkPoint_TransformChanged;

                    ptSelected = bpt.point;    ///Bezier Point
                    nSelected = bpt.curveNum;  ///Bezier Point

                                               /// Point Data
                    if (bpt.Name is "POINT")
                    {
                        for (int ii = 0; ii < targetPaths[nSelected].ptCurve.Count; ii++)
                        {
                            if (!(ptSelected == targetPaths[nSelected].ptCurve[ii].point))
                                continue;
                            nPoint = ii;
                            break;
                        }
                    }
                    else
                    {
                        for (int ii = 0; ii < targetPaths[nSelected].ptAdjust.Count; ii++)
                        {
                            if (!(ptSelected == targetPaths[nSelected].ptAdjust[ii].point))
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
                    strSelected = SelectedVisualType.Trajectory;
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

        private void BezierMarkPoint_TransformChanged(object sender, TransformArgs e)
        {
            BezierMarkPoint3D tt = (BezierMarkPoint3D)sender;
            onUpdateGraphics(tt.Position);
        }

        #region FILE HANDLING

        public void UpdateOpenData()
        {
            addVisual();
            //lineBezier = new List<Trajectory>();
            for (int i = oldCount; i < targetPaths.Count; i++)
            {
                if (i != 0 && i == oldCount)
                {
                    makeLine(i, false);
                    continue;
                }
                if (targetPaths[i].curveShape == CurveType.BEZIER)
                {
                    drawBezierCurveSegment(i, false);
                    drawControlCurveSegment(i, false);
                }
                else if (targetPaths[i].curveShape == CurveType.ARC)
                    drawArc(i);
                else if (targetPaths[i].curveShape == CurveType.LINE)
                    drawSegment(i);
            }
        }

        public void OpenFile()
        {
            //CustomCurve curve = new CustomCurve();
            //CustomCurve curve0 = new CustomCurve();
            //curve0.lblVisual3D = new List<BillboardTextVisual3D>();

            //OpenFileDialog openFileDialog = new OpenFileDialog();
            //openFileDialog.Filter = "Target Path File|*.xml";
            //string fileName;
            //if (openFileDialog.ShowDialog() == true)
            //{
            //    if (targetPaths.Count == 1 && targetPaths[0].ptCurve.Count == 1)
            //        reset();
            //    oldCount = targetPaths.Count;

            //    fileName = openFileDialog.FileName;

            //    System.Xml.Linq.XDocument xDoc = System.Xml.Linq.XDocument.Load(fileName);
            //    var items = xDoc.Descendants("PathData");
            //    int i = oldCount;
            //    if (oldCount > 0)
            //        i++;
            //    //ptArray = new List<double[]>();
            //    //targetPaths = new List<CustomCurve>();

            //    var ptMain = new MaterialGroup();
            //    ptMain.Children.Add(new EmissiveMaterial(new SolidColorBrush(Colors.LimeGreen)));
            //    ptMain.Children.Add(new DiffuseMaterial(new SolidColorBrush(Colors.LimeGreen)));
            //    ptMain.Children.Add(new SpecularMaterial(new SolidColorBrush(Colors.LimeGreen), 200));

            //    var ptControl = new MaterialGroup();
            //    ptControl.Children.Add(new EmissiveMaterial(new SolidColorBrush(Colors.Blue)));
            //    ptControl.Children.Add(new DiffuseMaterial(new SolidColorBrush(Colors.Blue)));
            //    ptControl.Children.Add(new SpecularMaterial(new SolidColorBrush(Colors.Blue), 200));

            //    int text_offset = 30;
            //    foreach (var item in items)
            //    {
            //        ///////////  shape of curve  //////////////
            //        curve.curveShape = item.Element("shape").Value;
            //        if (oldCount > 0)
            //            curve0.curveShape = item.Element("shape").Value;

            //        ////////// pt1 ////////////
            //        string ptStr = item.Element("pt1").Value;
            //        string[] ptsStr = ptStr.Split(',');
            //        Point3D pt = new Point3D(Convert.ToInt32(Convert.ToDouble(ptsStr[0])), Convert.ToInt32(Convert.ToDouble(ptsStr[1])), Convert.ToInt32(Convert.ToDouble(ptsStr[2])));
            //        BezierMarkPoint3D mark = new BezierMarkPoint3D(i, "MotionPath", "point", pt)
            //        {
            //            Center = new Point3D(0, 0, 0),
            //            Radius = 15,
            //            Material = ptMain
            //        };
            //        mark.Transform = new TranslateTransform3D(new Vector3D(pt.X, pt.Y, pt.Z));
            //        curve.ptCurve = new List<BezierMarkPoint3D>();
            //        curve.ptCurve.Add(mark);
            //        curve.lblVisual3D = new List<BillboardTextVisual3D>();
            //        var textVis3d1 = new BillboardTextVisual3D()
            //        {
            //            Position = new Point3D(pt.X + text_offset, pt.Y + text_offset, pt.Z + text_offset),
            //            Foreground = Brushes.White,
            //            Text = "0",
            //            FontSize = 16
            //        };
            //        curve.lblVisual3D.Add(textVis3d1);
            //        if (oldCount > 0 && i == oldCount + 1)
            //        {
            //            curve0.ptCurve = new List<BezierMarkPoint3D>();
            //            mark.curveNum -= 1;
            //            curve0.ptCurve.Add(mark);
            //            var textVis3d2 = new BillboardTextVisual3D()
            //            {
            //                Position = new Point3D(pt.X + text_offset, pt.Y + text_offset, pt.Z + text_offset),
            //                Foreground = Brushes.White,
            //                Text = "0",
            //                FontSize = 16
            //            };
            //            curve0.lblVisual3D.Add(textVis3d2);
            //        }

            //        ////////// pt2 //////////
            //        if (i == 0)
            //        {
            //            ptStr = item.Element("pt2").Value;
            //            ptsStr = ptStr.Split(',');
            //            pt = new Point3D(Convert.ToInt32(Convert.ToDouble(ptsStr[0])), Convert.ToInt32(Convert.ToDouble(ptsStr[1])), Convert.ToInt32(Convert.ToDouble(ptsStr[2])));
            //            {
            //                mark = new BezierMarkPoint3D(i, "MotionPath", "point", pt)
            //                {
            //                    Center = new Point3D(0, 0, 0),
            //                    Radius = 15,
            //                    Material = ptMain
            //                };
            //                mark.Transform = new TranslateTransform3D(new Vector3D(pt.X, pt.Y, pt.Z));
            //            }
            //            curve.ptCurve.Add(mark);
            //            var textVis3d3 = new BillboardTextVisual3D()
            //            {
            //                Position = new Point3D(pt.X + text_offset, pt.Y + text_offset, pt.Z + text_offset),
            //                Foreground = Brushes.White,
            //                Text = "0",
            //                FontSize = 16
            //            };
            //            curve.lblVisual3D.Add(textVis3d3);
            //        }
            //        if (oldCount > 0 && i == oldCount + 1)
            //        {
            //            ptStr = item.Element("pt2").Value;
            //            ptsStr = ptStr.Split(',');
            //            pt = new Point3D(Convert.ToInt32(Convert.ToDouble(ptsStr[0])), Convert.ToInt32(Convert.ToDouble(ptsStr[1])), Convert.ToInt32(Convert.ToDouble(ptsStr[2])));
            //            {
            //                mark = new BezierMarkPoint3D(i, "MotionPath", "point", pt)
            //                {
            //                    Center = new Point3D(0, 0, 0),
            //                    Radius = 15,
            //                    Material = ptMain
            //                };
            //                mark.Transform = new TranslateTransform3D(new Vector3D(pt.X, pt.Y, pt.Z));
            //            }
            //            curve.ptCurve[0] = mark;
            //            var textVis3d4 = new BillboardTextVisual3D()
            //            {
            //                Position = new Point3D(pt.X + text_offset, pt.Y + text_offset, pt.Z + text_offset),
            //                Foreground = Brushes.White,
            //                Text = "0",
            //                FontSize = 16
            //            };
            //            curve.lblVisual3D[0] = textVis3d4;
            //        }

            //        ///////// pt3 ///////////
            //        ptStr = item.Element("pt3").Value;
            //        ptsStr = ptStr.Split(',');
            //        pt = new Point3D(Convert.ToInt32(Convert.ToDouble(ptsStr[0])), Convert.ToInt32(Convert.ToDouble(ptsStr[1])), Convert.ToInt32(Convert.ToDouble(ptsStr[2])));
            //        {
            //            mark = new BezierMarkPoint3D(i, "MotionPath", PointType.CONTROL, pt)
            //            {
            //                Center = new Point3D(0, 0, 0),
            //                Radius = 15,
            //                Material = ptControl
            //            };
            //            mark.Transform = new TranslateTransform3D(new Vector3D(pt.X, pt.Y, pt.Z));
            //            curve.ptAdjust = new List<BezierMarkPoint3D>();
            //            curve.ptAdjust.Add(mark);
            //        }

            //        ////////// pt4 ///////////
            //        ptStr = item.Element("pt4").Value;
            //        ptsStr = ptStr.Split(',');
            //        pt = new Point3D(Convert.ToInt32(Convert.ToDouble(ptsStr[0])), Convert.ToInt32(Convert.ToDouble(ptsStr[1])), Convert.ToInt32(Convert.ToDouble(ptsStr[2])));
            //        {
            //            mark = new BezierMarkPoint3D(i, "MotionPath", PointType.CONTROL, pt)
            //            {
            //                Center = new Point3D(0, 0, 0),
            //                Radius = 15,
            //                Material = ptControl
            //            };
            //            mark.Transform = new TranslateTransform3D(new Vector3D(pt.X, pt.Y, pt.Z));
            //            curve.ptAdjust.Add(mark);
            //        }


            //        if (oldCount != 0 && i == oldCount + 1)
            //        {
            //            targetPaths.Add(curve0);
            //        }
            //        targetPaths.Add(curve);

            //        /////////// joint data ///////////
            //        double[] temp = { 0, 0, 0 };
            //        if (i == 0 || (oldCount > 0 && i == oldCount + 1))
            //        {
            //            ptStr = item.Element("joint0").Value;
            //            ptsStr = ptStr.Split(',');
            //            for (int k = 0; k < 3; k++)
            //                temp[k] = Convert.ToDouble(ptsStr[k]);
            //            ptArray.Add(temp);
            //        }

            //        double[] tempp = { 0, 0, 0 };
            //        ptStr = item.Element("joint1").Value;
            //        ptsStr = ptStr.Split(',');
            //        for (int k = 0; k < 3; k++)
            //            tempp[k] = Convert.ToDouble(ptsStr[k]);
            //        ptArray.Add(tempp);

            //        i++;
            //    }

            //    renameLbl();
            //    UpdateOpenData();

            //    parentWindow.btn_reset_target.IsEnabled = true;
            //}
        }

        public void SaveFile()
        {
            if (targetPaths.Count == 0)
                return;
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Path file (*.xml)|*.xml";
            string time = DateTime.Now.ToString("hhmmss"); // includes leading zeros
            string date = DateTime.Now.ToString("yyMMdd"); // includes leading zeros
            saveFileDialog.FileName = $"targetpath_{date}_{time}.xml";
            string fileName;
            if (saveFileDialog.ShowDialog() == true)
            {
                fileName = saveFileDialog.FileName;
                // ... add items...
                System.Data.DataTable dt = new System.Data.DataTable();
                dt.TableName = "PathData";
                dt.Columns.Add("shape");
                dt.Columns.Add("pt1");
                dt.Columns.Add("pt2");
                dt.Columns.Add("pt3");
                dt.Columns.Add("pt4");
                dt.Columns.Add("joint0");
                dt.Columns.Add("joint1");
                string tempp = "";
                for (int i = 0; i < targetPaths.Count; i++)
                {
                    ///////////  point data  /////////////
                    dt.Rows.Add();
                    dt.Rows[dt.Rows.Count - 1]["shape"] = targetPaths[i].curveShape;
                    dt.Rows[dt.Rows.Count - 1]["pt1"] = targetPaths[i].ptCurve[0].point;
                    if (i == 0)
                        dt.Rows[dt.Rows.Count - 1]["pt2"] = targetPaths[i].ptCurve[1].point;
                    dt.Rows[dt.Rows.Count - 1]["pt3"] = targetPaths[i].ptAdjust[0].point;
                    dt.Rows[dt.Rows.Count - 1]["pt4"] = targetPaths[i].ptAdjust[1].point;

                    ///////////  joint data  ////////////
                    if (i == 0)
                    {
                        tempp = Convert.ToString(ptArray[0][0]);
                        for (int k = 1; k < 3; k++)
                            tempp += ("," + Convert.ToString(ptArray[0][k]));
                        dt.Rows[dt.Rows.Count - 1]["joint0"] = tempp;
                    }
                    tempp = Convert.ToString(ptArray[i + 1][0]);
                    for (int k = 1; k < 3; k++)
                        tempp += ("," + Convert.ToString(ptArray[i + 1][k]));
                    dt.Rows[dt.Rows.Count - 1]["joint1"] = tempp;
                }

                dt.WriteXml(fileName);

            }
        }

        #endregion
    }
}
