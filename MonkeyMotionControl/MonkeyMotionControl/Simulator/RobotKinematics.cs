using MonkeyMotionControl.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace MonkeyMotionControl.Simulator
{
    public enum ToolPlacement
    {
        BOTTOM, FRONT, TOP, RIGHT, LEFT
    }

    public class CartesianPos
    {
        private double _x, _y, _z, _rx, _ry, _rz;
        public double X
        {
            get
            {
                return _x;
            }
            set
            {
                _x = value;
            }
        }
        public double Y
        {
            get
            {
                return _y;
            }
            set
            {
                _y = value;
            }
        }
        public double Z
        {
            get
            {
                return _z;
            }
            set
            {
                _z = value;
            }
        }
        public double RX
        {
            get
            {
                return _rx;
            }
            set
            {
                _rx = value;
            }
        }
        public double RY
        {
            get
            {
                return _ry;
            }
            set
            {
                _ry = value;
            }
        }
        public double RZ
        {
            get
            {
                return _rz;
            }
            set
            {
                _rz = value;
            }
        }
        public CartesianPos(double x, double y, double z, double rx, double ry, double rz)
        {
            X = x;
            Y = y;
            Z = z;
            RX = rx;
            RY = ry;
            RZ = rz;
        }
    }

    public class JointPos
    {
        private double _j1, _j2, _j3, _j4, _j5, _j6;
        public double J1
        {
            get
            {
                return _j1;
            }
            set
            {
                _j1 = value;
            }
        }
        public double J2
        {
            get
            {
                return _j2;
            }
            set
            {
                _j2 = value;
            }
        }
        public double J3
        {
            get
            {
                return _j3;
            }
            set
            {
                _j3 = value;
            }
        }
        public double J4
        {
            get
            {
                return _j4;
            }
            set
            {
                _j4 = value;
            }
        }
        public double J5
        {
            get
            {
                return _j5;
            }
            set
            {
                _j5 = value;
            }
        }
        public double J6
        {
            get
            {
                return _j6;
            }
            set
            {
                _j6 = value;
            }
        }
        public JointPos(double j1, double j2, double j3, double j4, double j5, double j6)
        {
            J1 = j1;
            J2 = j2;
            J3 = j3;
            J4 = j4;
            J5 = j5;
            J6 = j6;
        }
    }

    public class RobotConfiguration
    {   
        public string MODELPATH1 = "RX160L-HB_LINK1.stl";
        public string MODELPATH2 = "RX160L-HB_LINK2.stl";
        public string MODELPATH3 = "RX160L-HB_LINK3.stl";
        public string MODELPATH4 = "RX160L-HB_LINK4.stl";
        public string MODELPATH5 = "RX160L-HB_LINK5.stl";
        public string MODELPATH6 = "RX160L-HB_LINK6.stl";
        public string MODELPATH7 = "RX160L-HB_BASE.stl";
        
        public string MODELPATH_MOUNT_UNDER = "RX160L-HB_MOUNT_UNDER.stl";
        public string MODELPATH_CAM_RED = "RX160L-HB_CAM_RED.stl";
        
        public string MODELPATH_UNDERCAM = "RX160L-HB_TOOL_UNDER.stl";
        public string MODELPATH_FRONTCAM = "RX160L-HB_TOOL_FRONT.stl";

        public List<JointDH> JointsDH;

        public RobotConfiguration()
        {
            // TODO: Read From XML File

            JointsDH = new List<JointDH>();
            for(int i=0; i < 6; i++)
            {
                JointsDH.Add(new JointDH());
            }

            JointsDH[0].angleMin = -160.0;
            JointsDH[0].angleMax = 160.0;
            JointsDH[0].rotAxisX = 0;
            JointsDH[0].rotAxisY = 0;
            JointsDH[0].rotAxisZ = 1;
            JointsDH[0].rotPointX = 0;
            JointsDH[0].rotPointY = 0;
            JointsDH[0].rotPointZ = 0;

            JointsDH[1].angleMin = -137.5;
            JointsDH[1].angleMax = 137.5;
            JointsDH[1].rotAxisX = 0;
            JointsDH[1].rotAxisY = 1;
            JointsDH[1].rotAxisZ = 0;
            JointsDH[1].rotPointX = 150;
            JointsDH[1].rotPointY = 172;
            JointsDH[1].rotPointZ = 550;

            JointsDH[2].angleMin = -150.0;
            JointsDH[2].angleMax = 150.0;
            JointsDH[2].rotAxisX = 0;
            JointsDH[2].rotAxisY = 1;
            JointsDH[2].rotAxisZ = 0;
            JointsDH[2].rotPointX = 150;
            JointsDH[2].rotPointY = 171;
            JointsDH[2].rotPointZ = 1375;

            JointsDH[3].angleMin = -270.0;
            JointsDH[3].angleMax = 270.0;
            JointsDH[3].rotAxisX = 0;
            JointsDH[3].rotAxisY = 0;
            JointsDH[3].rotAxisZ = 1;
            JointsDH[3].rotPointX = 150;
            JointsDH[3].rotPointY = 0;
            JointsDH[3].rotPointZ = 1507;
            
            JointsDH[4].angleMin = -105.0;
            JointsDH[4].angleMax = 120.0;
            JointsDH[4].rotAxisX = 0;
            JointsDH[4].rotAxisY = 1;
            JointsDH[4].rotAxisZ = 0;
            JointsDH[4].rotPointX = 150;
            JointsDH[4].rotPointY = -1;
            JointsDH[4].rotPointZ = 2300;

            JointsDH[5].angleMin = -270.0;
            JointsDH[5].angleMax = 270.0;
            JointsDH[5].rotAxisX = 0;
            JointsDH[5].rotAxisY = 0;
            JointsDH[5].rotAxisZ = 1;
            JointsDH[5].rotPointX = 150;
            JointsDH[5].rotPointY = 0;
            JointsDH[5].rotPointZ = 2410;
        }

    }
    
    public class RobotKinematics
    {
        public JointPos HomeBottomJointPos { get; set; } = new JointPos(0, -11.6, 136.8, 0, 54.8, 0);
        public JointPos HomeFrontJointPos { get; set; } = new JointPos(0, -20, 140, 0.0, -28, 0);
        public CartesianPos HomeBottomCartesianPos { get; } = new CartesianPos(706.17, 0.0, 274.32, -180.0, 0.0, -180.0);
        public CartesianPos CameraOffsetConfig { get; set; } = new CartesianPos(0, 0, 0, 0, 0, 0);
        public CartesianPos ToolOffset { get; set; } = new CartesianPos(0, 0, 0, 0, 0, 0);
        public CartesianPos CurCartPos { get; set; } = new CartesianPos(0, 0, 0, 0, 0, 0);
        public JointPos CurJointPos { get; set; } = new JointPos(0, 0, 0, 0, 0, 0);
        public double CameraCenterZOffset { get; set; }
        private RobotConfiguration config{ get; set; }

        #region INTERFACE TO C++ DLL
        [DllImport("DHKinematics.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void rlForwardKinematics(string xmlFile, double camera_center_offset, double j1, double j2, double j3, double j4, double j5, double j6, IntPtr ret);

        [DllImport("DHKinematics.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void rlInverseKinematics(string xmlFile, double camera_center_offset, IntPtr prev, double x, double y, double z, double a, double b, double c, IntPtr ret);
        #endregion

        public RobotKinematics()
        {
            config = new RobotConfiguration(); //TODO: Read from XML file
        }

        #region KINEMATICS 

        /// <summary>
        /// Forward Kinematics Robotics Library DLL Function Call
        /// </summary>
        public double[] ForwardKinematics(double j1, double j2, double j3, double j4, double j5, double j6)
        {
            string path = new FileInfo(Assembly.GetEntryAssembly().Location).Directory.ToString();
            string filePath = path + "\\helix3d_under_cam.xml";
            //if (!bCamType)
            //    filePath = path + "\\helix3d_front_cam.xml";

            double[] retArr = new double[6];
            IntPtr outPtr = Marshal.AllocHGlobal(6 * Marshal.SizeOf<double>());
            rlForwardKinematics(filePath, CameraCenterZOffset, j1, j2, j3, j4, j5, j6, outPtr);
            Marshal.Copy(outPtr, retArr, 0, 6);
            Marshal.FreeHGlobal(outPtr);

            double[] prevCartesians = new double[] { 
                CurCartPos.X, 
                CurCartPos.Y, 
                CurCartPos.Z, 
                CurCartPos.RX, 
                CurCartPos.RY,
                CurCartPos.RZ
            };

            if (Math.Abs(retArr[3] - prevCartesians[3]) == 180)
            {
                retArr[3] = prevCartesians[3];
            }
            if (Math.Abs(retArr[4] - prevCartesians[4]) == 180)
            {
                retArr[4] = prevCartesians[4];
            }
            if (Math.Abs(retArr[5] - prevCartesians[5]) == 180)
            {
                retArr[5] = prevCartesians[5];
            }
            if (Math.Abs(retArr[3] - prevCartesians[3]) > 90)
            {
                int signn = 1;
                if (retArr[3] < 0) signn = -1;
                retArr[3] = -signn * (180 - Math.Abs(retArr[3]));
            }
            if (Math.Abs(retArr[4] - prevCartesians[4]) > 90)
            {
                int signn = 1;
                if (retArr[4] < 0) signn = -1;
                retArr[4] = signn * (180 - Math.Abs(retArr[4]));
                if (Math.Abs(retArr[4] - prevCartesians[4]) > 90)
                {
                    signn = 1;
                    if (retArr[4] < 0) signn = -1;
                    retArr[4] = signn * (180 - Math.Abs(retArr[4]));
                }
            }
            if (Math.Abs(retArr[5] - prevCartesians[5]) > 90 && Math.Abs(retArr[5] - prevCartesians[5]) < 270)
            {
                int signn1 = 1;
                if (retArr[5] < 0) signn1 = -1;
                retArr[5] = -signn1 * (180 - Math.Abs(retArr[5]));
            }

            //Console.WriteLine($"Prev RX: {prevCartesians[3]}, New RX: {retArr[3]}");
            //Console.WriteLine($"Prev RY: {prevCartesians[4]}, New RY: {retArr[4]}");
            //Console.WriteLine($"Prev RZ: {prevCartesians[5]}, New RZ: {retArr[5]}");
            //X.Text = Math.Round(retArr[0], 3, MidpointRounding.ToEven).ToString(); 
            //Y.Text = Math.Round(retArr[1], 3, MidpointRounding.ToEven).ToString();
            //Z.Text = Math.Round(retArr[2], 3, MidpointRounding.ToEven).ToString();
            //A.Text = Math.Round(retArr[3], 3, MidpointRounding.ToEven).ToString();
            //B.Text = Math.Round(retArr[4], 3, MidpointRounding.ToEven).ToString();
            //C.Text = Math.Round(retArr[5], 3, MidpointRounding.ToEven).ToString();
            
            CurCartPos.X = retArr[0];
            CurCartPos.Y = retArr[1];
            CurCartPos.Z = retArr[2];
            CurCartPos.RX = retArr[3];
            CurCartPos.RY = retArr[4];
            CurCartPos.RZ = retArr[5];

            return retArr;
        }

        /// <summary>
        /// Inverse Kinematics Robotics Library DLL Function Call
        /// </summary>
        public double[] InverseKinematics(double x, double y, double z, double rx, double ry, double rz)
        {
            //double[] jointAngles = new double[6] { mainJoints[0].angle, mainJoints[1].angle, mainJoints[2].angle, mainJoints[3].angle, mainJoints[4].angle, mainJoints[5].angle };
            double[] jointAngles = new double[6] { 
                CurJointPos.J1, 
                CurJointPos.J2, 
                CurJointPos.J3, 
                CurJointPos.J4, 
                CurJointPos.J5, 
                CurJointPos.J6 
            };

            int size = Marshal.SizeOf(jointAngles[0]) * jointAngles.Length;
            IntPtr prevJoints = Marshal.AllocHGlobal(size);
            Marshal.Copy(jointAngles, 0, prevJoints, jointAngles.Length);

            string path = new FileInfo(Assembly.GetEntryAssembly().Location).Directory.ToString();
            string filePath = path + "\\helix3d_under_cam.xml";
            //if (!bCamType)
            //    filePath = path + "\\helix3d_front_cam.xml";

            double[] retArr = new double[6];
            IntPtr outPtr = Marshal.AllocHGlobal(6 * Marshal.SizeOf<double>());
            //Stopwatch sw = new Stopwatch();
            //sw.Start();
            rlInverseKinematics(filePath, CameraCenterZOffset, prevJoints, x, y, z, rx, ry, rz, outPtr);
            //sw.Stop();
            //parentWindow.Log($"IK Elapsed={sw.Elapsed}");
            Marshal.Copy(outPtr, retArr, 0, 6);
            Marshal.FreeHGlobal(outPtr);
            Marshal.FreeHGlobal(prevJoints);

            CurJointPos.J1 = retArr[0];
            CurJointPos.J2 = retArr[1];
            CurJointPos.J3 = retArr[2];
            CurJointPos.J4 = retArr[3];
            CurJointPos.J5 = retArr[4];
            CurJointPos.J6 = retArr[5];

            return retArr;
        }

        //// INTERFACE TO C++ DLL
        //[DllImport("DHKinematics.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern void rlForwardKinematics(string xmlFile, double camera_center_offset, double j1, double j2, double j3, double j4, double j5, double j6, IntPtr ret);

        //[DllImport("DHKinematics.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern void rlInverseKinematics(string xmlFile, double camera_center_offset, IntPtr prev, double x, double y, double z, double a, double b, double c, IntPtr ret);

        //private string RLTestKinematics(double curPosJ1, double curPosJ2, double curPosJ3, double curPosJ4, double curPosJ5, double curPosJ6,
        //                                double curPosX, double curPosY, double curPosZ, double curPosRX, double curPosRY, double curPosRZ)
        //{
        //    //Detect change in values to avoid repeated calculation and logs
        //    bool change_flag = false;
        //    double[] diff = { 0, 0, 0, 0, 0, 0 };
        //    diff[0] = prevJointAngles[0] - curPosJ1;
        //    diff[1] = prevJointAngles[1] - curPosJ2;
        //    diff[2] = prevJointAngles[2] - curPosJ3;
        //    diff[3] = prevJointAngles[3] - curPosJ4;
        //    diff[4] = prevJointAngles[4] - curPosJ5;
        //    diff[5] = prevJointAngles[5] - curPosJ6;
        //    for (int i = 0; i < 6; i++) { if (diff[i] != 0) change_flag = true; }
        //    if (!change_flag) return; // Return if no change in values

        //    string path = new FileInfo(Assembly.GetEntryAssembly().Location).Directory.ToString();
        //    string filePath = path + "\\helix3d_under_cam.xml";
        //    //if (!bCamType)
        //    //    filePath = path + "\\helix3d_front_cam.xml";

        //    //RL INVERSE KINEMATICS 
        //    int size = Marshal.SizeOf(prevJointAngles[0]) * prevJointAngles.Length;
        //    IntPtr pnt = Marshal.AllocHGlobal(size);
        //    Marshal.Copy(prevJointAngles, 0, pnt, prevJointAngles.Length);
        //    IntPtr outIK = Marshal.AllocHGlobal(6 * Marshal.SizeOf<double>());
        //    rlInverseKinematics(filePath, camera_center_offset, pnt, curPosX, curPosY, curPosZ, curPosRX, curPosRY, curPosRZ, outIK);
        //    double[] retIKArr = new double[6];
        //    Marshal.Copy(outIK, retIKArr, 0, 6);
        //    Marshal.FreeHGlobal(outIK);
        //    Marshal.FreeHGlobal(pnt);
        //    //Save current Joint Angles for next calculation
        //    prevJointAngles = new double[] { curPosJ1, curPosJ2, curPosJ3, curPosJ4, curPosJ5, curPosJ6 };

        //    //RL FORWARD KINEMATICS 
        //    double[] retFKArr = new double[6];
        //    IntPtr outPtr = Marshal.AllocHGlobal(6 * Marshal.SizeOf<double>());
        //    rlForwardKinematics(filePath, camera_center_offset, curPosJ1, curPosJ2, curPosJ3, curPosJ4, curPosJ5, curPosJ6, outPtr);
        //    Marshal.Copy(outPtr, retFKArr, 0, 6);
        //    Marshal.FreeHGlobal(outPtr);

        //    return
        //        $"Joint Robot:\t{curPosJ1}\t{curPosJ2}\t{curPosJ3}\t{curPosJ4}\t{curPosJ5}\t{curPosJ6}\n" +
        //        $"Joint RL IK:\t{Math.Round(retIKArr[0], 2)}\t{Math.Round(retIKArr[1], 2)}\t{Math.Round(retIKArr[2], 2)}\t{Math.Round(retIKArr[3], 2)}\t{Math.Round(retIKArr[4], 2)}\t{Math.Round(retIKArr[5], 2)}\n" +
        //        $"Joint Diff :\t{Math.Round((retIKArr[0] - curPosJ1), 2)}\t{Math.Round((retIKArr[1] - curPosJ2), 2)}\t{Math.Round((retIKArr[2] - curPosJ3), 2)}\t{Math.Round((retIKArr[3] - curPosJ4), 2)}\t{Math.Round((retIKArr[4] - curPosJ5), 2)}\t{Math.Round((retIKArr[5] - curPosJ6), 2)}\n" +
        //        $" Cart Robot:\t{curPosX}\t{curPosY}\t{curPosZ}\t{curPosRX}\t{curPosRY}\t{curPosRZ}\n" +
        //        $" Cart RL FK:\t{Math.Round(retFKArr[0], 2)}\t{Math.Round(retFKArr[1], 2)}\t{Math.Round(retFKArr[2], 2)}\t{Math.Round(retFKArr[3], 2)}\t{Math.Round(retFKArr[4], 2)}\t{Math.Round(retFKArr[5], 2)}\n" +
        //        $" Cart Diff :\t{Math.Round((retFKArr[0] - curPosX), 2)}\t{Math.Round((retFKArr[1] - curPosY), 2)}\t{Math.Round((retFKArr[2] - curPosZ), 2)}\t{Math.Round((retFKArr[3] - curPosRX), 2)}\t{Math.Round((retFKArr[4] - curPosRY), 2)}\t{Math.Round((retFKArr[5] - curPosRZ), 2)}\n";
        //}

        //if (chk_enableKinLog.IsChecked==true)
        //{
        //    AddKinematicsLog(
        //        RLTestKinematics(
        //            robotController.RobotStatus.curJointPos[0],
        //            robotController.RobotStatus.curJointPos[1],
        //            robotController.RobotStatus.curJointPos[2],
        //            robotController.RobotStatus.curJointPos[3],
        //            robotController.RobotStatus.curJointPos[4],
        //            robotController.RobotStatus.curJointPos[5],
        //            robotController.RobotStatus.curCartPos[0],
        //            robotController.RobotStatus.curCartPos[1],
        //            robotController.RobotStatus.curCartPos[2],
        //            robotController.RobotStatus.curCartPos[3],
        //            robotController.RobotStatus.curCartPos[4],
        //            robotController.RobotStatus.curCartPos[5]
        //        )
        //    );
        //}

        //public void AddKinematicsLog(String logText)
        //{
        //    tb_KinematicsLog.AppendText($"{logText}{Environment.NewLine}");
        //    tb_KinematicsLog.ScrollToEnd();
        //}

        //private void Btn_ClearKinematicsLog_Click(object sender, EventArgs e)
        //{
        //    tb_KinematicsLog.Text = string.Empty;
        //}

        #endregion

        #region CONFIGURATION

        public List<string> GetRobot3DFilesPath(ToolPlacement toolConfig)
        {
            List<string> modelsFilepath = new List<string>();
            modelsFilepath.Add(config.MODELPATH1); // 0
            modelsFilepath.Add(config.MODELPATH2); // 1
            modelsFilepath.Add(config.MODELPATH3); // 2
            modelsFilepath.Add(config.MODELPATH4); // 3
            modelsFilepath.Add(config.MODELPATH5); // 4
            modelsFilepath.Add(config.MODELPATH6); // 5
            modelsFilepath.Add(config.MODELPATH7); // 6 ROBOT BASE
            if (toolConfig == ToolPlacement.FRONT)
            {
                modelsFilepath.Add(config.MODELPATH_FRONTCAM); // 7

            }
            else if (toolConfig == ToolPlacement.BOTTOM)
            {
                modelsFilepath.Add(config.MODELPATH_MOUNT_UNDER); // 7
                modelsFilepath.Add(config.MODELPATH_CAM_RED); // 8
            }
            else
            {
                modelsFilepath.Add(config.MODELPATH_UNDERCAM);

            }
            return modelsFilepath;
        }

        public List<JointDH> GetRobotDHKinematics()
        {
            return config.JointsDH;
        }

        #endregion

        #region DISCARD

        //private double LearningRate = 0.01;
        //private double SamplingDistance = 0.15;
        //private double DistanceThreshold = 20.0;

        //private double[] InverseKinematics(Vector3D target, double[] angles)
        //{
        //    if (DistanceFromTarget(target, angles) < DistanceThreshold)
        //    {
        //        //movements = 0;
        //        return angles;
        //    }

        //    double[] oldAngles = { 0.0, 0.0, 0.0, 0.0, 0.0, 0.0 };
        //    angles.CopyTo(oldAngles, 0);
        //    for (int i = 0; i <= 5; i++)
        //    {
        //        // Gradient descent
        //        // Update : Solution -= LearningRate * Gradient
        //        double gradient = PartialGradient(target, angles, i);
        //        angles[i] -= LearningRate * gradient;

        //        // Clamp
        //        angles[i] = Clamp(angles[i], mainJoints[i].angleMin, mainJoints[i].angleMax);

        //        // Early termination
        //        if (DistanceFromTarget(target, angles) < DistanceThreshold || checkAngles(oldAngles, angles))
        //        {
        //            //movements = 0;
        //            return angles;
        //        }
        //    }

        //    return angles;
        //}

        //public double DistanceFromTarget(Vector3D target, double[] angles)
        //{
        //    //UpdateRoboticArm(angles);
        //    double[] retVal = robotSim.ForwardKinematics(angles[0], angles[1], angles[2], angles[3], angles[4], angles[5]);
        //    //Vector3D coords = new Vector3D();
        //    UpdateToolPosAndAxis(retVal);

        //    return Math.Sqrt((Math.Pow(toolPosition.X - target.X, 2.0) + Math.Pow(toolPosition.Y - target.Y, 2.0)) + Math.Pow(toolPosition.Z - target.Z, 2.0));
        //}

        //public double[] getAngleFromPosition(Vector3D target, double[] angles)
        //{
        //    if (DistanceFromTarget(target, angles) < DistanceThreshold)
        //    {
        //        //movements = 0;
        //        return angles;
        //    }

        //    double[] oldAngles = { 0.0, 0.0, 0.0, 0.0, 0.0, 0.0 };
        //    int index = 0;
        //    while (true)
        //    {
        //        index++;
        //        angles.CopyTo(oldAngles, 0);
        //        for (int i = 0; i <= 5; i++)
        //        {
        //            // Gradient descent
        //            // Update : Solution -= LearningRate * Gradient
        //            double gradient = PartialGradient(target, angles, i);
        //            angles[i] -= LearningRate * gradient;

        //            // Clamp
        //            angles[i] = Clamp(angles[i], mainJoints[i].angleMin, mainJoints[i].angleMax);

        //            UpdateRoboticArm(mainJoints, angles);
        //            // Early termination
        //            if (DistanceFromTarget(target, angles) < DistanceThreshold || checkAngles(oldAngles, angles))
        //            {
        //                //movements = 0;
        //                return angles;
        //            }

        //        }
        //        if (nCount != movePaths.Count - 1)
        //            if (index == 8000)
        //                return angles;
        //    }
        //}

        //private bool checkAngles(double[] oldAngles, double[] angles)
        //{
        //    int index = 0;
        //    while (true)
        //    {
        //        bool flag2;
        //        if (index > 5)
        //        {
        //            flag2 = true;
        //        }
        //        else
        //        {
        //            if (oldAngles[index] == angles[index])
        //            {
        //                index++;
        //                continue;
        //            }
        //            flag2 = false;
        //        }
        //        return flag2;
        //    }
        //}

        //private double PartialGradient(Vector3D target, double[] angles, int i)
        //{
        //    // Saves the angle,
        //    // it will be restored later
        //    double angle = angles[i];

        //    // Gradient : [F(x+SamplingDistance) - F(x)] / h
        //    double f_x = DistanceFromTarget(target, angles);

        //    angles[i] += SamplingDistance;
        //    double f_x_plus_d = DistanceFromTarget(target, angles);

        //    double gradient = (f_x_plus_d - f_x) / SamplingDistance;

        //    // Restores
        //    angles[i] = angle;

        //    return gradient;
        //}

        //private T Clamp<T>(T value, T min, T max) where T : IComparable<T>
        //{
        //    T local = value;
        //    if (value.CompareTo(max) > 0)
        //    {
        //        local = max;
        //    }
        //    if (value.CompareTo(min) < 0)
        //    {
        //        local = min;
        //    }
        //    return local;
        //}

        #endregion

    }
}
