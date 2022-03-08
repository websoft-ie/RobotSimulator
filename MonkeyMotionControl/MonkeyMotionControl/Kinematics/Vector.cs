using System;

namespace MonkeyMotionControl.Kinematics
{
    public struct Vector
{
    public static readonly Vector Null = new Vector(0, 0, 0);
    public double x, y, z;
    public double[] v;
    bool defined;
    public static double[] sum, difference, cross;
    public static double dot;
    public Vector(double x, double y, double z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
        this.defined = true;
        this.v = new double[3] {x, y, z};

    }

    public Vector(double[] a)
    {
        this.x = a[0];
        this.y = a[1];
        this.z = a[2];
        this.defined = true;
        this.v = new double[3] {a[0], a[1], a[2]};
    }

    private static double DotProduct(double[] a, double[] b)
    {
        dot = a[0]*b[0] + a[1]*b[1] + a[2]*b[2];
        return dot;
    }

    public static Vector operator +(Vector v1, Vector v2)
    {
        if (v1.defined && v2.defined)
        {
            sum = new double[3] {v1.x + v2.x, v1.y + v2.y, v1.z + v2.z};
            return new Vector(sum);
        } else
        {
            return Null;
        }
    }

    public static Vector operator -(Vector v1, Vector v2)
    {
        if (v1.defined && v2.defined)
        {
            difference = new double[3] {v1.x - v2.x, v1.y - v2.y, v1.z - v2.z};
            return new Vector(difference);
        } else
        {
            return Null;
        }
    }

    public static double operator *(Vector v1, Vector v2)
    {
        if (v1.defined && v2.defined)
        {
            dot = (v1.x * v2.x) + (v1.y * v2.y) + (v1.z * v2.z);
            return dot;
        } else
        {
            return 0;
        }
    }
    public static Vector operator *(double s, Vector v)
    {
        if (v.defined)
        {
            return new Vector(s * v.x, s * v.y, s * v.z);
        } else
        {
            return Null;
        }
    }
    public static Vector operator /(Vector v1, Vector v2)
    {
        if (v1.defined && v2.defined)
        {
            cross = new double[3] {v1.y * v2.z - v1.z * v2.y, v1.z * v2.x - v1.x * v2.z, v1.x * v2.y - v1.y * v2.x};
            return new Vector(cross);
        } else
        {
            return Null;
        }
    }
    public double Normalize()
    {
        return Math.Sqrt((this.x * this.x) + (this.y * this.y) + (this.z * this.z));
    }
    public void print()
    {
        Console.Write(this.x + ", ");
        Console.Write(this.y + ", ");
        Console.WriteLine(this.z);
    }
}

}