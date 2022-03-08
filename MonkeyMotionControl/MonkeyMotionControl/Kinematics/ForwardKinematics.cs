using System;

namespace MonkeyMotionControl.Kinematics
{
    public class ForwardKinematics
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

    public ForwardKinematics()
    {
        InitializeMoment();
    }

    public ForwardKinematics(double t1, double t2, double t3, double t4, double t5, double t6)
    {
        thetaIn[0] = t1;
        thetaIn[1] = t2;
        thetaIn[2] = t3;
        thetaIn[3] = t4;
        thetaIn[4] = t5;
        thetaIn[5] = t6;
        for (int i = 0; i < thetaIn.Length; i++)
        {
            thetai[i] = thetaIn[i] * Math.PI / 180;
        }
        InitializeMoment();
    }

    public ForwardKinematics(double[] ti)
    {
        for (int i = 0; i < ti.Length; i++)
        {
            thetai[i] = ti[i] * Math.PI / 180;
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
    private static double[] qrot, qpos;
    public double[] Calculate(double[] thetaIn)
    {
        for (int i = 0; i < thetaIn.Length; i++)
        {
            //Console.WriteLine(thetaIn[i]);
            thetai[i] = thetaIn[i];
        }
        for (int num = 0; num < di.GetLength(0); num++)
        {
            qi[num] = new DualQuaternion(new Quaternion(thetai[num], new double[3] {di[num,0], di[num,1], di[num,2]}, true), new Quaternion(thetai[num], new double[3] {mi[num,0], mi[num,1], mi[num,2]}, false));
        }

        for (int num = 0; num < qi.Length; num++)
        {
            qInit = qInit * qi[num];
            qf[num] = qInit;
            qfCon[num] = qInit;
            qfCon[num] = qfCon[num].ConjugateClassical();
        }
        DualQuaternion q16 = qf[qf.Length-1]*li[li.Length-1]*qfCon[qfCon.Length-1];
        DualQuaternion L16 = q16;
        DualQuaternion L15 = qf[qf.Length-1]*li[li.Length-2]*qfCon[qfCon.Length-1];
        double[] q16pos = Add(CrossProduct(L16.qr.u, L16.qd.u),ScalarMultiply(DotProduct(CrossProduct(L15.qr.u,L15.qd.u),L16.qr.u),L16.qr.u));
        qrot = new double[4] {q16.s1, q16.x1, q16.y1, q16.z1};
        qpos = new double[3] {q16pos[0], q16pos[1], q16pos[2]};

        return new double[8] {q16.s1, q16.x1, q16.y1, q16.z1, 0, q16pos[0], q16pos[1], q16pos[2]};
    }
}

}