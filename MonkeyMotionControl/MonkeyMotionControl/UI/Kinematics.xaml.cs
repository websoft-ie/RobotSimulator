using System;
using System.Windows.Controls;
using MonkeyMotionControl.Kinematics;
using MonkeyMotionControl.Simulator;

namespace MonkeyMotionControl.UI
{
    /// <summary>
    /// Interaction logic for Kinematics.xaml
    /// </summary>
    public partial class Kinematics : UserControl
    {
        // Test Quaternion
        public static Quaternion q1 = new Quaternion(1, 0, 0, 0);
        public static Quaternion q2 = new Quaternion(0, 0, 0, 0);
        public static DualQuaternion dq1 = new DualQuaternion(q1, q2);
        public static DualQuaternion dq2 = new DualQuaternion(q1, q2);
        public static Quaternion qMultiply = q1 * q2;
        public static DualQuaternion dqMultiply = dq1 * dq2;
        public static DualQuaternion dqCon = dq1.ConjugateClassical();

        public static Quaternion q12 = q1 + q2;
        public static Quaternion conX = q12.Conjugate();
        public static Quaternion conX2 = q2.Conjugate();
        public static double conNorm = q1.Normalize();
        public static double[,] dt = new double[2, 3] { { 1, 2, 3 }, { 4, 5, 6 } };
        public static double[][] dy = new double[2][] { new double[3] { 1, 2, 3 }, new double[3] { 4, 5, 6 } };


        private static ForwardKinematics fwd = new ForwardKinematics();
        private static string fileName = "Robot.xml";
        //private static Configuration configXml = new Configuration(fileName);
        private static double[] fwdout = fwd.Calculate(new double[6] { 0, -20, 135, 10, 65, 0 });
        private static InverseKinematics inv = new InverseKinematics(true);
        private static double[] invout = inv.Calculate(180, 0, 90, fwdout[5], fwdout[6], fwdout[7]);
        // private static double[] invout2 = inv.Calculate(new Quaternion(0, fwdout[1], fwdout[2], fwdout[3]), new Quaternion(0, fwdout[5], fwdout[6], fwdout[7]));

        private static Quaternion testE2Q = eul2quat(0, 0, 0);
        
        private static double[,] testE2R = eul2rotm(-180, 0, 180);

        private bool _isEnabled = false;
        public bool IsEnabled
        {
            get
            {
                return _isEnabled;
            }
            set
            {
                _isEnabled = value;
                chk_Enabled.IsChecked = _isEnabled;
            }
        }

        public Kinematics()
        {
            InitializeComponent();
        }

        public void Update(JointPos joint, CartesianPos robot)
        {
            if (IsEnabled)
            {
                Calculate(joint, robot);
            }
        }

        private void Calculate(JointPos joint, CartesianPos robot)
        {
            tb_InputJ1.Text = joint.J1.ToString();
            tb_InputJ2.Text = joint.J2.ToString();
            tb_InputJ3.Text = joint.J3.ToString();
            tb_InputJ4.Text = joint.J4.ToString();
            tb_InputJ5.Text = joint.J5.ToString();
            tb_InputJ6.Text = joint.J6.ToString();
            tb_RefJ1.Text = joint.J1.ToString();
            tb_RefJ2.Text = joint.J2.ToString();
            tb_RefJ3.Text = joint.J3.ToString();
            tb_RefJ4.Text = joint.J4.ToString();
            tb_RefJ5.Text = joint.J5.ToString();
            tb_RefJ6.Text = joint.J6.ToString();
            tb_InputX.Text = robot.X.ToString();
            tb_InputY.Text = robot.Y.ToString();
            tb_InputZ.Text = robot.Z.ToString();
            tb_InputRX.Text = robot.RX.ToString();
            tb_InputRY.Text = robot.RY.ToString();
            tb_InputRZ.Text = robot.RZ.ToString();
            tb_RefX.Text = robot.X.ToString();
            tb_RefY.Text = robot.Y.ToString();
            tb_RefZ.Text = robot.Z.ToString();
            tb_RefRX.Text = robot.RX.ToString();
            tb_RefRY.Text = robot.RY.ToString();
            tb_RefRZ.Text = robot.RZ.ToString();
            double[] resFK = fwd.Calculate(new double[6] { joint.J1, joint.J2, joint.J3, joint.J4, joint.J5, joint.J6 });
            tb_OutputX.Text = resFK[0].ToString("F4");
            tb_OutputY.Text = resFK[1].ToString("F4");
            tb_OutputZ.Text = resFK[2].ToString("F4");
            tb_OutputRX.Text = resFK[3].ToString("F4");
            tb_OutputRY.Text = resFK[4].ToString("F4");
            tb_OutputRZ.Text = resFK[5].ToString("F4");
            double[] resIK = inv.Calculate(robot.X, robot.Y, robot.Z, robot.RX, robot.RY, robot.RZ);
            tb_OutputJ1.Text = resIK[0].ToString("F4");
            tb_OutputJ2.Text = resIK[1].ToString("F4");
            tb_OutputJ3.Text = resIK[2].ToString("F4");
            tb_OutputJ4.Text = resIK[3].ToString("F4");
            tb_OutputJ5.Text = resIK[4].ToString("F4");
            tb_OutputJ6.Text = resIK[5].ToString("F4");
        }

        public void MainTest(string[] args)
        {
            Console.WriteLine("Hello World!");
            testE2Q.print();

            Console.WriteLine(" ");

            for (int j = 0; j < 3; j++)
            {
                for (int i = 0; i < 3; i++)
                {
                    Console.Write(Math.Round(testE2R[j, i], 10) + " ");
                }
                Console.WriteLine(" ");
            }
            Console.WriteLine(" ");

            // Console.WriteLine(q1.s);
            // Console.WriteLine(q2.s);
            // //public Quaternion sumofqs;
            // Console.WriteLine(q12.s);
            // Console.WriteLine(q12.x);
            // Console.WriteLine(q12.y);
            // Console.WriteLine(q12.z);
            // Console.WriteLine(conX.x);
            // Console.WriteLine(conX2.x);
            // Console.WriteLine(conNorm);
            // Console.WriteLine("Output = \n");

            for (int i = 0; i < fwdout.Length; i++)
            {
                Console.Write(fwdout[i] + " ");
            }
            Console.WriteLine("");
            // q1.print();
            // q2.print();
            // dq1.print();
            // dq2.print();
            // qMultiply.print();
            // dqMultiply.print();

            for (int i = 0; i < invout.Length; i++)
            {
                Console.Write(invout[i] + " ");
            }
            Console.WriteLine("");

            // for (int i = 0; i < invout2.Length; i++)
            // {
            //     Console.Write(invout2[i] + " ");
            // }
            // Console.WriteLine("");

            // q12.print();
            // dqCon.print();
        }

        private static double[,] eul2rotm(double Rx, double Ry, double Rz)
        {
            double RxIn = Rx * Math.PI / 180;
            double RyIn = Ry * Math.PI / 180;
            double RzIn = Rz * Math.PI / 180;
            //Radians
            double ch = Math.Cos(RyIn);
            double ca = Math.Cos(RzIn);
            double cb = Math.Cos(RxIn);
            double sh = Math.Sin(RyIn);
            double sa = Math.Sin(RzIn);
            double sb = Math.Sin(RxIn);

            double[,] rotm = new double[3, 3] { { 0, 0, 0 }, { 0, 0, 0 }, { 0, 0, 0 } };
            rotm[0, 0] = ch * ca;
            rotm[0, 1] = -ch * sa * cb + sh * sb;
            rotm[0, 2] = ch * sa * sb + sh * cb;
            rotm[1, 0] = sa;
            rotm[1, 1] = ca * cb;
            rotm[1, 2] = -ca * sb;
            rotm[2, 0] = -sh * ca;
            rotm[2, 1] = sh * sa * cb + ch * sb;
            rotm[2, 2] = -sh * sa * sb + ch * cb;

            return rotm;
        }

        private static Quaternion eul2quat(double Rx, double Ry, double Rz)
        {
            double RxIn = Rx * Math.PI / 180;
            double RyIn = Ry * Math.PI / 180;
            double RzIn = Rz * Math.PI / 180;
            double[] c = new double[3] { 0, 0, 0 };
            double[] s = new double[3] { 0, 0, 0 };
            double[] Eul = new double[3] { RzIn, RyIn, RxIn };
            for (int k = 0; k < 3; k++)
            {
                c[k] = Math.Cos(Eul[k] / 2);
                s[k] = Math.Sin(Eul[k] / 2);
            }
            double quat_s = c[0] * c[1] * c[2] + s[0] * s[1] * s[2];
            double quat_x = c[0] * c[1] * s[2] - s[0] * s[1] * c[2];
            double quat_y = c[0] * s[1] * c[2] + s[0] * c[1] * s[2];
            double quat_z = s[0] * c[1] * c[2] - c[0] * s[1] * s[2];
            return new Quaternion(quat_s, quat_x, quat_y, quat_z);

        }

        private void chk_Enabled_Checked(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        private void chk_Enabled_Unchecked(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        // private static double[,] eul2rotm(double Rx, double Ry, double Rz)
        // {
        //     double RxIn = Rx * Math.PI / 180;
        //     double RyIn = Ry * Math.PI / 180;
        //     double RzIn = Rz * Math.PI / 180;
        //     //Radians
        //     double[] ct = new double[3] {0, 0, 0};
        //     double[] st = new double[3] {0, 0, 0};
        //     double[] Eul = new double[3] {RzIn, RyIn, RxIn};
        //     for (int k = 0; k < 3; k++) {
        //         ct[k] = Math.Cos(Eul[k]);
        //         st[k] = Math.Sin(Eul[k]);
        //     }

        //     double[,] rotm = new double[3,3] {{0, 0, 0}, {0, 0, 0}, {0, 0, 0}};
        //     rotm[0, 0] = ct[1] * ct[0];
        //     rotm[1, 0] = st[2] * st[1] * ct[0] - ct[2] * st[0];
        //     rotm[1, 0] = ct[2] * st[1] * ct[0] + st[2] * st[0];
        //     rotm[0, 1] = ct[1] * st[0];
        //     rotm[1, 1] = st[2] * st[1] * st[0] + ct[2] * ct[0];
        //     rotm[2, 2] = ct[2] * st[1] * st[0] - st[2] * ct[0];
        //     rotm[0, 2] = -st[1];
        //     rotm[1, 2] = st[2] * ct[1];
        //     rotm[2, 2] = ct[2] * ct[1];

        //     return rotm;
        // }

    }
}
