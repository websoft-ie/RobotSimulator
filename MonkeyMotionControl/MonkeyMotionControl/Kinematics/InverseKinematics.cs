using System;

namespace MonkeyMotionControl.Kinematics
{
    public class InverseKinematics
{
    private static Configuration configXml = new Configuration("Robot.xml");
    public static double[] thetaIn = new double[6] {0, 0, 0, 0, 0, 0};
    public static double[] thetai = new double[6] {0, 0, 0, 0, 0, 0};
    public static double lz0 = 550;
    public static double lz1 = 825;
    public static double lz2 = 925;
    public static double lz3 = 110;
    public static double[,] di = configXml.di;

    public static double[,] Pi = configXml.Pi;
    private static double[,] mi = configXml.mi;
    private static DualQuaternion q0 = new DualQuaternion(1, 0, 0, 0, 0, 0, 0, 0);
    private static DualQuaternion[] qd0 = new DualQuaternion[6] {q0, q0, q0, q0, q0, q0};
    private static DualQuaternion[] li = new DualQuaternion[6] {q0, q0, q0, q0, q0, q0};
    private static DualQuaternion[] qi = new DualQuaternion[6] {q0, q0, q0, q0, q0, q0};
    private static DualQuaternion[] qf = new DualQuaternion[6] {q0, q0, q0, q0, q0, q0};
    private static DualQuaternion[] qfCon = new DualQuaternion[6] {q0, q0, q0, q0, q0, q0};
    private DualQuaternion qInit = new DualQuaternion(1, 0, 0, 0, 0, 0, 0, 0);
    private bool Degrees = false;

    public InverseKinematics(bool Deg)
    {
        if (Deg == true)
        {
            this.Degrees = true;
        } else
        {
            this.Degrees = false;
        }
        InitializeMoment();
    }
    private static double DotProduct(double[] a, double[] b)
    {
        return a[0]*b[0] + a[1]*b[1] + a[2]*b[2];
    }
    private static double[] CrossProduct(double[] a, double[] b)
    {
        return new double[3] {a[1]*b[2] - a[2]*b[1], a[2]*b[0] - a[0]*b[2], a[0]*b[1] - a[1]*b[0]};
    }
    private static double[] Add(double[] a, double[] b)
    {
        return new double[3] {a[0] + b[0], a[1] + b[1], a[2] + b[2]};
    }
    private static double[] Subtract(double[] a, double[] b)
    {
        return new double[3] {a[0] - b[0], a[1] - b[1], a[2] - b[2]};
    }
    private static double[] ScalarMultiply(double a, double[] b)
    {
        return new double[3] {a*b[0], a*b[1], a*b[2]};
    }
    private static void InitializeMoment()
    {
        double[] cross;
        double[] crossPi;
        double[] crossdi;
        //Console.WriteLine(di.GetLength(0));
        for (int num = 0; num < di.GetLength(0); num++)
        {
            crossPi = new double[3] {Pi[num,0], Pi[num,1], Pi[num,2]};
            crossdi = new double[3] {di[num,0], di[num,1], di[num,2]};
            cross = CrossProduct(crossPi, crossdi);
            for (int num2 = 0; num2 < 2; num2++)
            {
                mi[num,num2] = cross[num2];
            }
            Quaternion lr = new Quaternion(0, di[num,0], di[num,1], di[num,2]);
            Quaternion ld = new Quaternion(0, mi[num,0], mi[num,1], mi[num,2]);

            li[num] = new DualQuaternion(lr, ld);
        }
    }
    private static double PadenKahan1(Vector a, Vector b, Vector R, Vector Axis)
    {
        Vector a_R = a - R;
        Vector b_R = b - R;

        Quaternion l = new Quaternion(0, Axis.x, Axis.y, Axis.z);
        Quaternion x = new Quaternion(0, a_R.x, a_R.y, a_R.z);
        Quaternion y = new Quaternion(0, b_R.x, b_R.y, b_R.z);

        Quaternion xn = x + (((l * x).s) * l);
        Quaternion yn = y + (((l * y).s) * l);

        double th = Math.Atan2((l * xn * yn).s, (xn * yn).s);
        return th;
    }
        private static double PadenKahan1(Quaternion x, Quaternion y, Quaternion Axis)
    {
        Quaternion l = new Quaternion(0, Axis.x, Axis.y, Axis.z);

        Quaternion xn = x + (((l * x).s) * l);
        Quaternion yn = y + (((l * y).s) * l);

        double th = Math.Atan2((l * xn * yn).s, (xn * yn).s);
        return th;
    }
    private static double[] PadenKahan2(Vector a, Vector b, Vector R, Vector axis1, Vector axis2)
    {
        Vector a_R = a - R;
        Vector b_R = b - R;

        Quaternion x = new Quaternion(0, a_R.x, a_R.y, a_R.z);
        Quaternion y = new Quaternion(0, b_R.x, b_R.y, b_R.z);

        Quaternion Axis1 = new Quaternion(0, axis1.x, axis1.y, axis1.z);
        Quaternion Axis2 = new Quaternion(0, axis2.x, axis2.y, axis2.z);

        double alpha = (((Axis1 * Axis2).s * (Axis2 * x).s) - (Axis1 * y).s) / (Math.Pow((Axis1 * Axis2).s, 2) - 1);
        double beta = (((Axis1 * Axis2).s * (Axis1 * y).s) - (Axis2 * x).s) / (Math.Pow((Axis1 * Axis2).s, 2) - 1);
        double gamma = (Math.Pow(x.Normalize(), 2) - Math.Pow(alpha, 2) - Math.Pow(beta, 2) - (2 * alpha * beta * (Axis1 * Axis2).s)) / (Math.Pow((Axis1 * Axis2).v.Normalize(), 2));
        double gammaAbs = Math.Abs(gamma);
        double gamma1 = Math.Sqrt(gammaAbs);
        double gamma2 = gamma1 * -1;

        Quaternion z1 = (alpha * Axis1) + (beta * Axis2) + (gamma1 * (Axis1 * Axis2));
        Quaternion z2 = (alpha * Axis1) + (beta * Axis2) + (gamma2 * (Axis1 * Axis2));

        double theta1 = PadenKahan1(z1, y, Axis1);
        double theta2 = PadenKahan1(x, z1, Axis2);
        double theta1b = PadenKahan1(z2, y, Axis1);
        double theta2b = PadenKahan1(x, z2, Axis2);

        return new double[] {theta1, theta2};
    }
    private static double[] PadenKahan3(Vector a, Vector b, Vector R, Vector L, Vector dirac)
    {
        Vector a_R = a - R;
        Vector b_R = b - R;
        Vector a_b = a - b;

        Quaternion ab = new Quaternion(0, a_b.x, a_b.y, a_b.z);
        Quaternion l = new Quaternion(0, L.x, L.y, L.z);
        Quaternion x = new Quaternion(0, a_R.x, a_R.y, a_R.z);
        Quaternion y = new Quaternion(0, b_R.x, b_R.y, b_R.z);

        Quaternion xn = x - (((l * x).s) * l);
        Quaternion yn = y - (((l * y).s) * l);

        double diracn = (dirac * dirac) - Math.Abs(Math.Pow((l * ab).s, 2));
        double th0 = Math.Atan2((l * xn * yn).s,(xn * yn).s);
        double acosth = Math.Acos((Math.Pow(xn.Normalize(), 2) + Math.Pow(yn.Normalize(), 2) - diracn) / (2 * xn.Normalize() * yn.Normalize()));
        double thf1 = th0 + acosth;
        double thf2 = th0 - acosth + Math.PI;
        return new double[2] {thf1, thf2};
    }
    // private static Quaternion eul2q(double Rx, double Ry, double Rz)
    // {
    //     double c1 = Math.Cos(Ry);
    //     double s1 = Math.Sin(Ry);
    //     double c2 = Math.Cos(Rz);
    //     double s2 = Math.Sin(Rz);
    //     double c3 = Math.Cos(Rx);
    //     double s3 = Math.Sin(Rx);
    //     double w = Math.Sqrt(1.0 + c1 * c2 + c1*c3 - s1 * s2 * s3 + c2*c3) / 2.0;
    //     double w4 = (4.0 * w);
    //     double x = (c2 * s3 + c1 * s3 + s1 * s2 * c3) / w4;
    //     double y = (s1 * c2 + s1 * c3 + c1 * s2 * s3) / w4;
    //     double z = (-s1 * s3 + c1 * s2 * c3 +s2) / w4;
    //     return new Quaternion(w, x, y, z);
    // }

    private static Quaternion eul2quat(double Rx, double Ry, double Rz)
    {
        // Radians
        double[] c = new double[3] {0, 0, 0};
        double[] s = new double[3] {0, 0, 0};
        double[] Eul = new double[3] {Rz, Ry, Rx};
        for (int k = 0; k < 3; k++)
        {
            c[k] = Math.Cos(Eul[k]/2);
            s[k] = Math.Sin(Eul[k]/2);
        }
        double quat_s = c[0] * c[1] * c[2] + s[0] * s[1] * s[2];
        double quat_x = c[0] * c[1] * s[2] - s[0] * s[1] * c[2];
        double quat_y = c[0] * s[1] * c[2] + s[0] * c[1] * s[2];
        double quat_z = s[0] * c[1] * c[2] - c[0] * s[1] * s[2];
        return new Quaternion(quat_s, quat_x, quat_y, quat_z);
    }

    private static double[,] eul2rotm(double Rx, double Ry, double Rz)
    {
        double RxIn = Rx;
        double RyIn = Ry;
        double RzIn = Rz;
        //Radians
        double ch = Math.Cos(RyIn);
        double ca = Math.Cos(RzIn);
        double cb = Math.Cos(RxIn);
        double sh = Math.Sin(RyIn);
        double sa = Math.Sin(RzIn);
        double sb = Math.Sin(RxIn);

        double[,] rotm = new double[3,3] {{0, 0, 0}, {0, 0, 0}, {0, 0, 0}};
        rotm[0, 0] = ch*ca;
        rotm[0, 1] = -ch*sa*cb+sh*sb;
        rotm[0, 2] = ch*sa*sb+sh*cb;
        rotm[1, 0] = sa;
        rotm[1, 1] = ca*cb;
        rotm[1, 2] = -ca*sb;
        rotm[2, 0] = -sh*ca;
        rotm[2, 1] = sh*sa*cb+ch*sb;
        rotm[2, 2] = -sh*sa*sb+ch*cb;

        return rotm;
    }
    
     
    private double[] InverseOutput(double x, double y, double z, double qs, double qx, double qy, double qz, double rs, double rx, double ry, double rz)
    {
        // Theta3
        Quaternion qoin = new Quaternion(0, x, y, z);
        Quaternion pb = new Quaternion(0, Pi[1, 0], Pi[1, 1], Pi[1, 2]);
        Quaternion qoin_pb = qoin - pb;
        Vector a1 = (li[5].qr.v / li[5].qd.v) + (((li[4].qr.v / li[4].qd.v) * li[5].qr.v) * li[5].qr.v);
        Vector b1 = (li[1].qr.v / li[1].qd.v) + (((li[0].qr.v / li[0].qd.v) * li[1].qr.v) * li[1].qr.v);
        Vector L1 = new Vector(di[2, 0], di[2, 1], di[2, 2]);
        Quaternion l1 = new Quaternion(0, di[2, 0], di[2, 1], di[2, 2]);
        Vector R1 = new Vector(Pi[2, 0], Pi[2, 1], Pi[2, 2]);
        double[] th3_rad = PadenKahan3(a1, b1, R1, L1, qoin_pb.v);
        double[] th3_deg = new double[2] {(th3_rad[0] * 180 / Math.PI), (th3_rad[1] * 180 / Math.PI)};

        // Theta1 & Theta2
        double[] di3 = new double[3] {di[2, 0], di[2, 1], di[2, 2]};
        double[] mi3 = new double[3] {mi[2, 0], mi[2, 1], mi[2, 2]};
        Quaternion q3 = new Quaternion(th3_deg[1], di3, true);
        Quaternion qo3 = new Quaternion(th3_deg[1], mi3, false);
        DualQuaternion qd3 = new DualQuaternion(q3, qo3);
        DualQuaternion qdcon3 = qd3.ConjugateClassical();
        DualQuaternion l6n = qd3 * li[5] * qdcon3;
        DualQuaternion l5n = qd3 * li[4] * qdcon3;
        Vector a2 = (l6n.qr.v / l6n.qd.v) + (((l5n.qr.v / l5n.qd.v) * l6n.qr.v) * l6n.qr.v);
        Vector b2 = qoin.v;
        Vector axs1 = new Vector(di[0, 0], di[0, 1], di[0, 2]);
        Vector axs2 = new Vector(di[1, 0], di[1, 1], di[1, 2]);
        Vector R2 = new Vector(Pi[1, 0], Pi[1, 1], Pi[1, 2]);
        double[] th12 = PadenKahan2(a2, b2, R2, axs1, axs2);
        double th1_rad = th12[0];
        double th1_deg = th12[0] * 180 / Math.PI;
        double th2_rad = th12[1];
        double th2_deg = th12[1] * 180 / Math.PI;

        // Theta4 & Theta5
        // Constant variable for orientation axis
        double ga = 110;
        Vector d6 = new Vector(di[5, 0], di[5, 1], di[5, 2]);
        Vector gd6 = new Vector(qx, qy, qz);
        Vector P6 = new Vector(Pi[5, 0], Pi[5, 1], Pi[5, 2]);

        Vector pInit = P6 + (ga * d6);
        Vector d7 = new Vector(0, 1, 0);
        Vector d8 = new Vector(0, 0, 1);

        Vector m7 = pInit / d7;
        Vector m8 = pInit / d8;
        DualQuaternion l7 = new DualQuaternion(0, d7.x, d7.y, d7.z, 0, m7.x, m7.y, m7.z);
        DualQuaternion l8 = new DualQuaternion(0, d8.x, d8.y, d8.z, 0, m8.x, m8.y, m8.z);

        double[] d1 = new double[3] {di[0, 0], di[0, 1], di[0, 2]};
        double[] d2 = new double[3] {di[1, 0], di[1, 1], di[1, 2]};
        double[] d3 = new double[3] {di[2, 0], di[2, 1], di[2, 2]};

        double[] m1 = new double[3] {mi[0, 0], mi[0, 1], mi[0, 2]};
        double[] m2 = new double[3] {mi[1, 0], mi[1, 1], mi[1, 2]};
        double[] m3 = new double[3] {mi[2, 0], mi[2, 1], mi[2, 2]};

        Quaternion q1 = new Quaternion(th1_deg, d1, true);
        Quaternion qo1 = new Quaternion(th1_deg, m1, false);
        DualQuaternion qd1 = new DualQuaternion(q1, qo1);
        DualQuaternion qdcon1 = qd1.ConjugateClassical();

        Quaternion q2 = new Quaternion(th2_deg, d2, true);
        Quaternion qo2 = new Quaternion(th2_deg, m2, false);
        DualQuaternion qd2 = new DualQuaternion(q2, qo2);
        DualQuaternion qdcon2 = qd2.ConjugateClassical();

        DualQuaternion qd13 = qd1 * qd2 * qd3;
        DualQuaternion qdcon13 = qd13.ConjugateClassical();

        DualQuaternion ln7 = qd13 * l7 * qdcon13;
        DualQuaternion ln8 = qd13 * l8 * qdcon13;

        Vector a3 = (ln8.qr.v / ln8.qd.v) + (((ln7.qr.v / ln7.qd.v) * ln8.qr.v) * ln8.qr.v);
        Vector b3 = qoin.v + (ga * gd6);
        Vector axs4 = ln8.qr.v;
        Vector axs5 = ln7.qr.v;
        Vector R3 = qoin.v;

        double[] th45 = PadenKahan2(a3, b3, R3, axs4, axs5);
        double th4_rad = th45[0];
        double th4_deg = th45[0] * 180 / Math.PI;

        double th5_rad = th45[1];
        double th5_deg = th45[1] * 180 / Math.PI;

        // Theta 6
        double[] di4 = new double[3] {di[3, 0], di[3, 1], di[3, 2]};
        double[] di5 = new double[3] {di[4, 0], di[4, 1], di[4, 2]};
        
        double[] mi4 = new double[3] {mi[3, 0], mi[3, 1], mi[3, 2]};
        double[] mi5 = new double[3] {mi[4, 0], mi[4, 1], mi[4, 2]};
        
        Quaternion qrotout = new Quaternion(qs, qx, qy, qz);
        Quaternion qrotoutCon = qrotout.Conjugate();
        Vector vd5 = new Vector(di[4, 0], di[4, 1], di[4, 2]);
        Quaternion d5 = new Quaternion(0, vd5.x, vd5.y, vd5.z);
        Quaternion gd5 = new Quaternion(rs, rx, ry, rz);

        Vector Pi5 = new Vector(Pi[4, 0], Pi[4, 1], Pi[4, 2]);
        Vector pd = Pi5 + (ga * vd5);

        Vector d9 = new Vector(0, 1, 0);
        Vector d10 = new Vector(1, 0, 0);

        Vector m9 = pd / d9;
        Vector m10 = pd / d10;

        DualQuaternion l9 = new DualQuaternion(0, d9.x, d9.y, d9.z, 0, m9.x, m9.y, m9.z);
        DualQuaternion l10 = new DualQuaternion(0, d10.x, d10.y, d10.z, 0, m10.x, m10.y, m10.z);

        Quaternion q4 = new Quaternion(th4_rad, di4, true);
        Quaternion qo4 = new Quaternion(th4_rad, mi4, false);
        DualQuaternion qd4 = new DualQuaternion(q4, qo4);

        Quaternion q5 = new Quaternion(th5_rad, di5, true);
        Quaternion qo5 = new Quaternion(th5_rad, mi5, false);
        DualQuaternion qd5 = new DualQuaternion(q5, qo5);

        DualQuaternion qd15 = qd13 * qd4 * qd5;
        DualQuaternion qcon15 = qd15.ConjugateClassical();

        DualQuaternion ln9 = qd15 * l9 * qcon15;
        DualQuaternion ln10 = qd15 * l10 * qcon15;

        Vector a4 = (ln10.qr.v / ln10.qd.v) + (((ln9.qr.v / ln9.qd.v) * ln10.qr.v) * ln10.qr.v);
        Vector b4 = qoin.v + (ga * gd5.v);

        Quaternion Axis6 = new Quaternion(qrotout.s, qrotout.x, qrotout.y, qrotout.z);

        Vector r6 = qoin.v;
        Vector axs6 = Axis6.v;

        double th6 = PadenKahan1(a4, b4, r6, axs6) + Math.PI;
        double th6_rad = th6;
        double th6_deg = th6 * 180 / Math.PI;

        double[] th_out = new double[6] {th1_deg, th2_deg, th3_deg[1], th4_deg, th5_deg, th6_deg};

        for (int thi = 0; thi < th_out.Length; thi++)
        {
            th_out[thi] = Math.Round(th_out[thi], 3);
        }

        return th_out;
    }
    public double[] Calculate(double Rx, double Ry, double Rz, double x, double y, double z)
    {
        double[,] RotationM = eul2rotm(Rx * Math.PI / 180, Ry * Math.PI / 180, Rz * Math.PI / 180);
        Quaternion Orientation = new Quaternion(0, RotationM[0, 2], RotationM[1, 2], RotationM[2, 2]);
        Quaternion Rotation = new Quaternion(0, RotationM[0, 1], RotationM[1, 1], RotationM[2, 1]);
        // Console.WriteLine("Quaternion Euler = ");
        // Rotation.print();
        return InverseOutput(x, y, z, Orientation.s, Orientation.x, Orientation.y, Orientation.z, Rotation.s, Rotation.x, Rotation.y, Rotation.z);
    }

    public double[] Calculate(Quaternion orientation, Quaternion rotation, Quaternion position)
    {
        // Console.WriteLine("Quaternion =");
        // rotation.print();
        return InverseOutput(position.x, position.y, position.z, orientation.s, orientation.x, orientation.y, orientation.z, rotation.s, rotation.x, rotation.y, rotation.z);
    }
}

}