using SharpDX;
using System;
using System.Collections.Generic;

namespace MonkeyMotionControl.Simulator
{
    public static class LagrangianCurveFitting
    {
        /// <summary>
        /// Uses Lagrange interpolation to predict y.
        /// Note! Make sure that the input point list does not contain points with the same x.
        /// </summary>
        public static int PredictY(List<Point> points, int x)
        {
            if (points.Count == 0) return 0;

            double[] lagrange = new double[points.Count];
            for (int i = 0; i < points.Count; i++)
            {
                lagrange[i] = 1;

                for (int k = 0; k < points.Count; k++)
                {
                    if (i != k)
                    {
                        lagrange[i] *= ((double)(x - points[k].X)) / (points[i].X - points[k].X);
                    }
                }
            }

            double y = 0;
            for (int i = 0; i < points.Count; i++)
            {
                y += lagrange[i] * points[i].Y;
            }

            return Convert.ToInt32(y);
        }
    }
}
