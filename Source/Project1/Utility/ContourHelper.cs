using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;

namespace Utility
{
    public class ContourHelper
    {
        readonly public static double _threshold = 0.000001;

        //public static void UnifyDirectionOfContour(IFeatureClass fc)
        //{
        //    UnifyDirectionOfContour(fc);
        //}

        //统一方向成逆时针方向
        public static void UnifyDirectionOfContour(IFeatureClass fc)
        {
            IGeoDataset gds = fc as IGeoDataset;
            IEnvelope extext = gds.Extent;

            IFeatureCursor cur = fc.Search(null, false);
            IFeature ft = cur.NextFeature();

            while (ft != null)
            {
                IPolyline pl = ft.Shape as IPolyline;
                IPointCollection pc = pl as IPointCollection;

                if (pl.IsClosed)//如多边形闭合
                {
                    if (IsClockWise(pl as IPointCollection))
                        pl.ReverseOrientation();
                }
                else//如折线不闭合
                {
                    //if (pl.FromPoint.M)//如始未点都在上边
                    //{
                    //    if (pl.FromPoint.X > pl.ToPoint.X)
                    //        pl.ReverseOrientation();
                    //}
                    if (pl.FromPoint.Y == pl.ToPoint.Y && pl.FromPoint.Y == extext.YMax)//如始未点都在上边
                    {
                            if (pl.FromPoint.X > pl.ToPoint.X)
                                pl.ReverseOrientation();
                    }
                    else if (pl.FromPoint.Y == pl.ToPoint.Y && pl.FromPoint.Y == extext.YMin)//如始未点都在下边
                    {
                            if (pl.FromPoint.X < pl.ToPoint.X)
                                pl.ReverseOrientation();
                    }
                    else if (pl.FromPoint.X == pl.ToPoint.X && pl.FromPoint.X == extext.XMax)//如始未点都在右边
                    {
                            if (pl.FromPoint.Y < pl.ToPoint.Y)
                                pl.ReverseOrientation();
                    }
                    else if (pl.FromPoint.X == pl.ToPoint.X && pl.FromPoint.X == extext.XMin)//如始未点都在左边
                    {
                            if (pl.FromPoint.Y > pl.ToPoint.Y)
                                pl.ReverseOrientation();
                    }
                    else//如始末点在不同边上
                    {
                        int fromEdgeID = PointInWhichEdge(pl.FromPoint, extext);
                        int toEdgeID = PointInWhichEdge(pl.ToPoint, extext);

                        if (fromEdgeID > toEdgeID)
                            pl.ReverseOrientation();
                    }
                }

                ft.Store();
                ft = cur.NextFeature();
            }
        }

        /// <summary>
        ///     1 
        ///   -----
        /// 4|     | 2
        ///   _____
        ///     3
        /// 计算点在那条边上，边代码如上图所示
        /// </summary>
        /// <param name="pc"></param>
        /// <param name="isClockWise"></param>
        /// <returns></returns>
        public static int PointInWhichEdge(IPoint pt, IEnvelope extent)
        {
            if(pt.Y == extent.YMax)
                return 1;
            if (pt.X == extent.XMax)
                return 2;
            if (pt.Y == extent.YMin)
                return 3;
            if (pt.X == extent.XMin)
                return 4;

            throw new Exception("等高线起始点不在边界上！检查断线情况！");
        }

        //判读是否为顺时针，如是，返回真
        public static bool IsClockWise(IPointCollection pc)
        {
            IPointCollection ring = new RingClass() as IPointCollection;
            IRing rng = ring as IRing;
            for (int i = 0; i < pc.PointCount; i++)
			{
                ring.AddPoint(pc.get_Point(i));
			}
            if (rng.IsExterior)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
