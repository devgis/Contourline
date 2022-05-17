using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geodatabase;
using DataStructure;

namespace Utility
{
    public static class GeometryHelper
    {
        /// <summary>
        /// 允许三维几何对象
        /// </summary>
        /// <param name="pcNew"></param>
        public static void Enable3DGeometry(IGeometry pcNew)
        {
            IZAware za = pcNew as IZAware;
            if (za != null)
                za.ZAware = true;
        }

        /// <summary>
        /// 将要素集中的折线变成直线段
        /// </summary>
        /// <param name="fc"></param>
        public static void Polyline2Line(IFeatureClass fc)
        {
            IFeatureCursor cursor = fc.Search(null, false);
            IFeature ft = cursor.NextFeature();
            while(ft != null)
            {
                IPointCollection pc = ft.Shape as IPointCollection;
                if (pc.PointCount > 2)//如果不是直线段
                {
                    for (int i = 0; i < pc.PointCount - 1; i++)
                    {
                        IPoint pt1 = pc.get_Point(i);
                        IPoint pt2 = pc.get_Point(i+1);

                        IGeometryCollection pcNew = new PolylineClass();
                        ISegmentCollection path = new PathClass();
                        //允许三维
                        Enable3DGeometry(pcNew as IGeometry);

                        IPolyline pl = pcNew as IPolyline;
                        pl.FromPoint = pt1;
                        pl.ToPoint = pt2;

                        GeoDBHelper.AddFeature2FC(fc, pcNew as IPolyline, ft);
                    }
                    ft.Delete();
                }
                ft = cursor.NextFeature();
            }
        }

        public static IPoint CreatePoint(double x, double y, double z)
        {
            IPoint pt = new PointClass();
            pt.X = x;
            pt.Y = y;
            pt.Z = z;

            return pt;
        }

        public static IPolyline CreatePolylineFromTwoPt(IPoint pt1, IPoint pt2)
        {
            IPolyline pl = new PolylineClass();
            GeometryHelper.Enable3DGeometry(pl);
 
            pl.FromPoint = pt1;
            pl.ToPoint = pt2;

            return pl;
        }

        /// <summary>
        /// 根据三角形边构造几何线
        /// </summary>
        /// <param name="edge"></param>
        /// <returns></returns>
        public static IPolyline CreatePolylineFromTwoPt(Point_T pt1, Point_T pt2)
        {
            IPoint from = new PointClass();
            IPoint to = new PointClass();

            from.X = pt1.X;
            from.Y = pt1.Y;
            from.Z = pt1.Z;
            to.X = pt2.X;
            to.Y = pt2.Y;
            to.Z = pt2.Z;

            object missing = Type.Missing;
            IPolyline pl = new PolylineClass();
            GeometryHelper.Enable3DGeometry(pl as IGeometry);
            pl.FromPoint = from;
            pl.ToPoint = to;

            return pl;
        }
    }
}
