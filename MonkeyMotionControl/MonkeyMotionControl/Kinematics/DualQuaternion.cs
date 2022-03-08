using System;

namespace MonkeyMotionControl.Kinematics
{
    public class DualQuaternion
{
    public double s1, x1, y1, z1, s2, x2, y2, z2;
    private double[] u1, u2;
    private Vector v1, v2;
    public Quaternion qr, qd;
    bool defined;

    public static readonly DualQuaternion Null = new DualQuaternion(0, 0, 0, 0, 0, 0, 0, 0);
    public DualQuaternion(double s1, double x1, double y1, double z1, double s2, double x2, double y2, double z2)
    {
        this.defined = true;
        this.s1 = s1;
        this.x1 = x1;
        this.y1 = y1;
        this.z1 = z1;
        this.s2 = s2;
        this.x2 = x2;
        this.y2 = y2;
        this.z2 = z2;
        this.u1 = new double[3] {x1, y1, z1};
        this.u2 = new double[3] {x2, y2, z2};
        this.v1 = new Vector(x1, y1, z1);
        this.v2 = new Vector(x2, y2, z2);
        this.qr = new Quaternion(s1, x1, y1, z1);
        this.qd = new Quaternion(s2, x2, y2, z2);
    }

    public DualQuaternion(Quaternion p, Quaternion q)
    {
        this.defined = true;
        this.s1 = p.s;
        this.x1 = p.x;
        this.y1 = p.y;
        this.z1 = p.z;
        this.s2 = q.s;
        this.x2 = q.x;
        this.y2 = q.y;
        this.z2 = q.z;
        this.u1 = new double[3] {p.x, p.y, p.z};
        this.u2 = new double[3] {q.x, q.y, q.z};
        this.v1 = new Vector(p.x, p.y, p.z);
        this.v2 = new Vector(q.x, q.y, q.z);
        this.qr = new Quaternion(p.s, p.x, p.y, p.z);
        this.qd = new Quaternion(q.s, q.x, q.y, q.z);
    }

    public static DualQuaternion operator +(DualQuaternion q1, DualQuaternion q2)
    {
        if (q1.defined && q2.defined)
        {
            return new DualQuaternion(q1.s1 + q2.s1, q1.x1 + q2.x1, q1.y1 + q2.y1, q1.z1 + q2.z1, q1.s2 + q2.s2, q1.x2 + q2.x2, q1.y2 + q2.y2, q1.z2 + q2.z2);
        } else
        {
            return Null;
        }
    }

    public static DualQuaternion operator -(DualQuaternion q1, DualQuaternion q2)
    {
        if (q1.defined && q2.defined)
        {
            return new DualQuaternion(q1.s1 - q2.s1, q1.x1 - q2.x1, q1.y1 - q2.y1, q1.z1 - q2.z1, q1.s2 - q2.s2, q1.x2 - q2.x2, q1.y2 - q2.y2, q1.z2 - q2.z2);
        } else 
        {
            return Null;
        }
    }

    public static DualQuaternion operator *(DualQuaternion q1, DualQuaternion q2)
    {
        if (q1.defined && q2.defined)
        {
            return new DualQuaternion(q1.qr*q2.qr, (q1.qr*q2.qd)+(q1.qd*q2.qr));
        } else
        {
            return Null;
        }
    }
    
    // Quaternion Conjugate (Classical Method)
    public DualQuaternion ConjugateClassical()
    {
        return new DualQuaternion(this.s1, -this.x1, -this.y1, -this.z1, this.s2, -this.x2, -this.y2, -this.z2);
    }

    public void print()
    {
        Console.Write(this.s1 + ", ");
        Console.Write(this.x1 + ", ");
        Console.Write(this.y1 + ", ");
        Console.Write(this.z1 + ", ");

        Console.Write(this.s2 + ", ");
        Console.Write(this.x2 + ", ");
        Console.Write(this.y2 + ", ");
        Console.WriteLine(this.z2);
    }
}

}