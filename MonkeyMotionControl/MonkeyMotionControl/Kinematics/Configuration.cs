using System;
using System.Xml;

namespace MonkeyMotionControl.Kinematics
{
    public class Configuration
{
    private string fileName;
    public string[] jointLabels = new string[6];
    public double[] alpha = new double[6];
    public double[] a = new double[6];
    public double[] thetaOffset = new double[6];
    public double[] d = new double[6];
    private double[] lz = new double[6];
    private double[] lzi = new double[6];
    public double[,] di = new double[6,3];
    public double[,] Pi = new double[6,3];
    public double[,] mi = new double[6,3];
    //TODO: Set up vector direction of joint reading from XML and change code to allow modular vector orientation setup.
    private static double[] x = new double[3] {1, 0, 0};
    private static double[] y = new double[3] {0, 1, 0};
    private static double[] z = new double[3] {0, 0, 1};

    public Configuration(string fileName)
    {
        XmlDocument doc = new XmlDocument();
        doc.Load(fileName);
        this.fileName = fileName;
        foreach(XmlNode node in doc.DocumentElement)
        {
            string sectionName = node.Name;
            if(node.Name == "DH_parameter")
            {
                int jointCount = node.ChildNodes.Count - 1;
                int i = 0;
                foreach(XmlNode child in node.ChildNodes)
                {
                    if(child.Attributes[0].InnerText!="Base")
                    {
                        this.jointLabels[i] = child.Attributes[0].InnerText;
                        string alphai = child.Attributes[3].InnerText;
                        string Ai = child.Attributes[4].InnerText;
                        string thetaOffseti = child.Attributes[5].InnerText;
                        string Di = child.Attributes[6].InnerText;
                        this.alpha[i] = Convert.ToDouble(alphai);
                        this.a[i] = Convert.ToDouble(Ai);
                        this.thetaOffset[i] = Convert.ToDouble(thetaOffseti);
                        this.d[i] = Convert.ToDouble(Di);
                        i++;
                    }

                }
                int aiter2 = 0;
                for(int aiter = 0; aiter < a.Length; aiter++)
                {
                    this.lz[aiter] = a[aiter] + d[aiter];
                    if(this.lz[aiter] != 0)
                    {
                        this.lzi[aiter2] = this.lz[aiter];
                        aiter2++;
                    }
                }
                di = new double[6,3] 
                {{0, 0, 1},
                {0, 1, 0}, 
                {0, 1, 0},
                {0, 0, 1},
                {0, 1, 0},
                {0, 0, 1}};
                Pi = new double[6,3] 
                {{0, 0, lzi[0]},
                {0, 0, lzi[0]},
                {0, 0, lzi[0]+lzi[1]},
                {0, 0, lzi[0]+lzi[1]+lzi[2]},
                {0, 0, lzi[0]+lzi[1]+lzi[2]},
                {0, 0, lzi[0]+lzi[1]+lzi[2]}};
                mi = new double[6,3] 
                {{0, 0, 0},
                {0, 0, 0},
                {0, 0, 0},
                {0, 0, 0},
                {0, 0, 0},
                {0, 0, 0}};
            }
        }
    }
}

}