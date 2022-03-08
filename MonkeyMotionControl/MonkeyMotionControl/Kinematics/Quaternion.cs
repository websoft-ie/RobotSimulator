using System;

namespace MonkeyMotionControl.Kinematics
{
    public class Quaternion
{
    public static readonly Quaternion Null = new Quaternion(0, 0, 0, 0);
    public static Quaternion qSum, qDifference, qProduct, qQuotient;
    public double s, x, y, z;
    public double[] q;
    public double[] u;
    public Vector v;
    bool defined;
    public static double[] sum, difference, product, quotient;

    private static double[] cross;
    private static double dot;
    public Quaternion(double s, double x, double y, double z)
    {
        this.s = s;
        this.x = x;
        this.y = y;
        this.z = z;
        this.defined = true;
        this.u = new double[3] {x, y, z};
        this.q = new double[4] {s, x, y, z};
        this.v = new Vector(x, y, z);

    }

    public Quaternion(double[] q)
    {
        this.s = q[0];
        this.x = q[1];
        this.y = q[2];
        this.z = q[3];
        this.defined = true;
        this.u = new double[3] {q[1], q[2], q[3]};
        this.q = new double[4] {q[0], q[1], q[2], q[3]};
        this.v = new Vector(x, y, z);
    }

    public Quaternion(double th, double[] di, bool qType)
    {
        if (qType == true)
        {
            this.s = Math.Cos((th * Math.PI / 180)/2);
        } else if (qType == false)
        {
            this.s = 0;
        }
        this.x = di[0] * Math.Sin((th * Math.PI / 180)/2);
        this.y = di[1] * Math.Sin((th * Math.PI / 180)/2);
        this.z = di[2] * Math.Sin((th * Math.PI / 180)/2);
        this.defined = true;
        this.u = new double[3] {this.s, di[1] * Math.Sin((th * Math.PI / 180)/2), di[2] * Math.Sin((th * Math.PI / 180)/2)};
        this.q = new double[4] {this.s, this.x, this.y, this.z};
        this.v = new Vector(this.s, di[1] * Math.Sin((th * Math.PI / 180)/2), di[2] * Math.Sin((th * Math.PI / 180)/2));
    }
    private static double[] Add(double[] a, double[] b)
    {
        return new double[3] {a[0] + b[0], a[1] + b[1], a[2] + b[2]};
    }
    private static double[] ScalarMultiply(double a, double[] b)
    {
        return new double[3] {a*b[0], a*b[1], a*b[2]};
    }
    private static double[] CrossProduct(double[] a, double[] b)
    {
        cross = new double[3] {a[1]*b[2] - a[2]*b[1], a[2]*b[0] - a[0]*b[2], a[0]*b[1] - a[1]*b[0]};
        return cross;
    }

    private static double DotProduct(double[] a, double[] b)
    {
        dot = a[0]*b[0] + a[1]*b[1] + a[2]*b[2];
        return dot;
    }

    public static Quaternion operator +(Quaternion q1, Quaternion q2)
    {
        if (q1.defined && q2.defined)
        {
            sum = new double[4] {q1.s + q2.s, q1.x + q2.x, q1.y + q2.y, q1.z + q2.z};
            return qSum = new Quaternion(sum);
        } else
        {
            return Null;
        }
    }

    public static Quaternion operator -(Quaternion q1, Quaternion q2)
    {
        if (q1.defined && q2.defined)
        {
            difference = new double[4] {q1.s - q2.s, q1.x - q2.x, q1.y - q2.y, q1.z - q2.z};
            return qDifference = new Quaternion(difference);
        } else
        {
            return Null;
        }
    }

    private static double[] productVector;
    public static Quaternion operator *(Quaternion q1, Quaternion q2)
    {
        if (q1.defined && q2.defined)
        {
            product = new double[4] {q1.s*q2.s-DotProduct(q1.u, q2.u), 0, 0, 0};
            productVector = Add(Add(ScalarMultiply(q1.s, q2.u), ScalarMultiply(q2.s, q1.u)), CrossProduct(q1.u, q2.u));
            product[1] = productVector[0];
            product[2] = productVector[1];
            product[3] = productVector[2];
            return qProduct = new Quaternion(product);
        } else
        {
            return Null;
        }
    }
    public static Quaternion operator *(double s1, Quaternion q1)
    {
        if (q1.defined)
        {
            return new Quaternion(s1 * q1.s, s1 * q1.x, s1 * q1.y, s1 * q1.z);
        } else
        {
            return Null;
        }
    }

    public Quaternion Conjugate()
    {
        return new Quaternion(this.s, -this.x, -this.y, -this.z);
    }

    public double Normalize()
    {
        return Math.Sqrt((this.s*this.s)+(this.x*this.x)+(this.y*this.y)+(this.z*this.z));
    }

    public void print()
    {
        Console.Write(this.s + ", ");
        Console.Write(this.x + ", ");
        Console.Write(this.y + ", ");
        Console.WriteLine(this.z);
    }
}

}
