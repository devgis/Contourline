using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
namespace Utility
{
    public static class SmoothFormula
    {
        public static double Angle(IPoint p1, IPoint p2, IPoint p3, IPoint p4)
        {
            IConstructPoint ConstructPoint = new PointClass();
            ConstructPoint.ConstructAngleBisector(p1, p2, p3, 1, true);
            p4 = ConstructPoint as IPoint;
            double K24 = (p4.Y - p2.Y) / (p4.X - p2.X);
            double Kef, AngleEF;
            if (K24 == 0)//防止斜率为0
            {
                AngleEF = Math.PI / 2;
            }
            else
            {
                Kef = -1 / K24;
                AngleEF = Math.Atan(Kef);
            }
            
            double Angle12 = Math.Atan2(p2.Y - p1.Y, p2.X - p1.X);
            double AngleEFOpposite = AngleEF + Math.PI;
            if (AngleEFOpposite > Math.PI)
            {
                AngleEFOpposite = -(2 * Math.PI - AngleEFOpposite);
            }
            if (Math.Abs(Angle12 - AngleEF) > Math.Abs(Angle12 - AngleEFOpposite))
            {
                AngleEF = AngleEFOpposite;
            }
            if (AngleEF < 0)
            {
                AngleEF = AngleEF + Math.PI * 2;
            }
            return AngleEF;
        }
        /// <summary>
        /// 参数曲线方程X
        /// </summary>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>

        /// <param name="z"></param>
        /// <returns></returns>
        public static double X(double p0, double p1, double p2, double p3, double z)
        {
            double x = p0 + p1 * z + p2 * z * z + p3 * z * z * z;
            return x;
        }
        /// <summary>
        /// 参数曲线方程Y
        /// </summary>
        /// <param name="q0"></param>
        /// <param name="q1"></param>
        /// <param name="q2"></param>
        /// <param name="q3"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public static double Y(double q0, double q1, double q2, double q3, double z)
        {
            double y = q0 + q1 * z + q2 * z * z + q3 * z * z * z;
            return y;
        }
        /// <summary>
        /// 实点两点之间的距离
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static double R(IPoint p1, IPoint p2)
        {
            double r = Math.Sqrt(Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2));
            return r;
        }
        /// <summary>
        /// R1为三点分段求导中的R
        /// 在以下函数sin1,sin2都是角度值
        /// </summary>
        /// <param name="r"></param>
        /// <param name="z"></param>
        /// <param name="sin1"></param>
        /// <param name="sin2"></param>
        /// <returns></returns>
        public static double R1(double r, double z, double sin1, double sin2)
        {
            double r1 = r * Math.Abs((1 - z) * Math.Sin(sin1) + z * Math.Sin(sin2));
            return r1;
        }
        public static double P0(double x)
        {
            double p0 = x;
            return p0;
        }
        public static double P1(double r1, double cos1)
        {
            double p1 = r1 * Math.Cos(cos1);
            return p1;
        }
        public static double P2(double x1, double x2, double cos1, double cos2, double r1)
        {
            double p2 = 3 * (x2 - x1) - r1 * (Math.Cos(cos2) + 2 * Math.Cos(cos1));
            return p2;
        }
        public static double P3(double x1, double x2, double cos1, double cos2, double r1)
        {
            double p3 = -2 * (x2 - x1) + r1 * (Math.Cos(cos2) + Math.Cos(cos1));
            return p3;
        }
        public static double Q0(double y)
        {
            double q0 = y;
            return q0;
        }
        public static double Q1(double r1, double sin1)
        {
            double q1 = r1 * Math.Sin(sin1);
            return q1;
        }
        public static double Q2(double y1, double y2, double sin1, double sin2, double r1)
        {
            double q2 = 3 * (y2 - y1) - r1 * (Math.Sin(sin2) + 2 * Math.Sin(sin1));
            return q2;
        }
        public static double Q3(double y1, double y2, double sin1, double sin2, double r1)
        {
            double q3 = -2 * (y2 - y1) + r1 * (Math.Sin(sin2) + Math.Sin(sin1));
            return q3;
        }
        public static double Length(IPoint p1, IPoint p2)
        {
            double length = Math.Sqrt((p2.Y - p1.Y) * (p2.Y - p1.Y) + (p2.X - p1.X) * (p2.X - p1.X));
            return length;
        }
        public static IPoint AddPoint(IPoint fromPoint, IPoint thruPoint, IPoint toPoint)
        {
            ICircularArc circularArcThreePoint = new CircularArcClass();
            IConstructCircularArc construtionCircularArc = circularArcThreePoint as IConstructCircularArc;
            construtionCircularArc.ConstructThreePoints(fromPoint, thruPoint, toPoint, true);
            double length = circularArcThreePoint.Length;
            circularArcThreePoint.Complement();
            IPoint outpoint=new PointClass();
            circularArcThreePoint.QueryPoint(esriSegmentExtension.esriExtendEmbedded, 2 * length, false, outpoint);
            return outpoint;
        }
    }
}
