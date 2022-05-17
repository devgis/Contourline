using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.DataSourcesRaster;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.ArcMapUI;

namespace FeaturePointDataModel
{
    public class FeaturePoint
    {
        private IMxDocument document;
        private IFeatureClass feaCla;
        private ISpatialReference spatialRef;

        public IMxDocument Document
        {
            get { return document; }
            set { document = value; }
        }

        public IFeatureClass FeaCla
        {
            get { return feaCla; }
            set { feaCla = value; }
        }

        public ISpatialReference SpatialRef
        {
            get { return spatialRef; }
            set { spatialRef = value; }
        }
        public FeaturePoint(IFeatureClass cfeaCla, ISpatialReference cspatialRef, IMxDocument cdocument)
        {
            feaCla = cfeaCla;
            spatialRef = cspatialRef;
            document = cdocument;
        }

        /// <summary>
        /// 获取特征点集合
        /// </summary>
        /// <param name="feaCla"></param>
        /// <returns></returns>
        public void getPtShp(int tempAngle)
        {
            IFeatureCursor pfcurse = feaCla.Search(null, false);
            IFeature pFeature = pfcurse.NextFeature();
            List<IPoint> ptList = new List<IPoint>();

            IFeatureClass p_feaPtCla = MapHelp.getFeaCla("testFeaPt", MapHelp.OpenInMemoryWorkspace() as IFeatureWorkspace, spatialRef);//特征点featureclass(数据集)
            IFeatureClass inflection_feaPtCla = MapHelp.getFeaCla("testInfle", MapHelp.OpenInMemoryWorkspace() as IFeatureWorkspace, spatialRef);//特征点featureclass(数据集)
            while (pFeature != null)
            {
                List<int> inflectionPtIndex = new List<int>();
                Dictionary<int, double> dicList = new Dictionary<int, double>();
                IPolyline pl = pFeature.Shape as IPolyline;
                IPointCollection pc = pl as IPointCollection;
                List<double> differList = new List<double>();//拐点的转角值
                int count = 0;
                for (int i = 0; i < pc.PointCount - 3; i++)
                {
                    
                    double differ1 = Angle(pc.get_Point(i), pc.get_Point(i + 1), pc.get_Point(i + 2));
                    double differ2 = Angle(pc.get_Point(i + 1), pc.get_Point(i + 2), pc.get_Point(i + 3));
                    if (differ1 * differ2 <= 0)
                    {
                        differList.Add(differ2);
                        inflectionPtIndex.Add(i + 2);
                    }
                    else
                    {
                        count++;//始终是接近圆的即单一方向，没顺时针逆时针变化
                    }
                }
                IFeature p_Feature = null;
                IFeature infle_Feature = null;
                if (count == pc.PointCount - 3)//生成特征点features，添加（闭合而且始终是一个方向的特征点）的字段
                {
                    int mAngleIndex = getMaxAngleIndex(pc);//如果闭合而且始终是一个方向,则找最大的转角点
                    p_Feature = p_feaPtCla.CreateFeature();//生成特征点feature
                    p_Feature.Shape = pc.get_Point(mAngleIndex);
                    p_Feature.set_Value(1, pFeature.OID);
                    p_Feature.set_Value(2, mAngleIndex);
                    p_Feature.set_Value(3, Convert.ToInt32(pc.get_Point(mAngleIndex).Z));
                    p_Feature.set_Value(5, Angle(pc.get_Point(mAngleIndex - 1), pc.get_Point(mAngleIndex), pc.get_Point(mAngleIndex + 1)) * 180 / Math.PI);
                    if (Angle(pc.get_Point(mAngleIndex - 1), pc.get_Point(mAngleIndex), pc.get_Point(mAngleIndex + 1)) < 0)
                    {
                        p_Feature.set_Value(4, -1);
                    }
                    else
                    {
                        p_Feature.set_Value(4, 1);
                    }
                    p_Feature.Store();
                }
                for (int n = 0; n < inflectionPtIndex.Count - 1; n++)//每条线中 在每两个拐点找转角最大值
                {
                    infle_Feature = inflection_feaPtCla.CreateFeature();//生成拐点feature
                    infle_Feature.Shape = pc.get_Point(inflectionPtIndex.ElementAt(n));//生成features，给拐点featureclass添加字段
                    infle_Feature.set_Value(1, pFeature.OID);
                    infle_Feature.set_Value(2, inflectionPtIndex.ElementAt(n));
                    infle_Feature.set_Value(3, pc.get_Point(inflectionPtIndex.ElementAt(n)).Z);
                    infle_Feature.set_Value(5, differList.ElementAt(n) * 180 / Math.PI);
                    if (differList.ElementAt(n) * 180 / Math.PI < 0)
                    {
                        infle_Feature.set_Value(4, -1);
                    }
                    else
                    {
                        infle_Feature.set_Value(4, 1);
                    }
                    infle_Feature.Store();
                    if (n == inflectionPtIndex.Count - 2)//生成最后一个拐点
                    {
                        infle_Feature = inflection_feaPtCla.CreateFeature();
                        infle_Feature.Shape = pc.get_Point(inflectionPtIndex.ElementAt(n + 1));
                        infle_Feature.set_Value(1, pFeature.OID);
                        infle_Feature.set_Value(2, inflectionPtIndex.ElementAt(n + 1));
                        infle_Feature.set_Value(3, pc.get_Point(inflectionPtIndex.ElementAt(n + 1)).Z);
                        infle_Feature.set_Value(5, Convert.ToInt32(differList.ElementAt(n + 1) * 180 / Math.PI));
                        if (differList.ElementAt(n + 1) * 180 / Math.PI < 0)
                        {
                            infle_Feature.set_Value(4, -1);
                        }
                        else
                        {
                            infle_Feature.set_Value(4, 1);
                        }
                        infle_Feature.Store();
                    }

                    double negativeMinAgle = 0, positivemaxAgle = 0;
                    int negativePoIndex = 0;
                    bool negativeIsTure = false, positveIsTrue = false;
                    int frm = inflectionPtIndex.ElementAt(n);//拐点在pc中的索引
                    int to = inflectionPtIndex.ElementAt(n + 1);
                    for (int m = frm; m < to - 2; m++)
                    {
                        double agle = Angle(pc.get_Point(m), pc.get_Point(m + 1), pc.get_Point(m + 2));
                        if (agle < negativeMinAgle)
                        {
                            negativeMinAgle = agle;
                            negativePoIndex = m + 1;
                            negativeIsTure = true;
                        }
                        else if (agle > positivemaxAgle)
                        {
                            positivemaxAgle = agle;
                            negativePoIndex = m + 1;
                            positveIsTrue = true;
                        }
                    }

                    if (negativeIsTure)
                    {
                        if (Math.Abs(negativeMinAgle * 180 / Math.PI) >= tempAngle)
                        {
                            p_Feature = p_feaPtCla.CreateFeature();//生成特征点feature
                            p_Feature.Shape = pc.get_Point(negativePoIndex);
                            p_Feature.set_Value(1, pFeature.OID);
                            p_Feature.set_Value(2, negativePoIndex);
                            p_Feature.set_Value(3, Convert.ToInt32(pc.get_Point(negativePoIndex).Z));
                            p_Feature.set_Value(5, negativeMinAgle * 180 / Math.PI);
                            p_Feature.set_Value(4, -1);
                            p_Feature.Store();
                        }

                    }
                    else if (positveIsTrue)
                    {
                        if (Math.Abs(positivemaxAgle * 180 / Math.PI) >= tempAngle)
                        {
                            p_Feature = p_feaPtCla.CreateFeature();//生成特征点feature
                            p_Feature.Shape = pc.get_Point(negativePoIndex);
                            p_Feature.set_Value(1, pFeature.OID);
                            p_Feature.set_Value(2, negativePoIndex);
                            p_Feature.set_Value(3, pc.get_Point(negativePoIndex).Z);
                            p_Feature.set_Value(5, Convert.ToInt32(positivemaxAgle * 180 / Math.PI));
                            p_Feature.set_Value(4, 1);
                            p_Feature.Store();
                        }
                    }
                }

                pFeature = pfcurse.NextFeature();
            }


            //for (int f = 0; f < ptList.Count; f++)
            //{
            //    IFeature p_Feature = p_feaPtCla.CreateFeature();
            //    p_Feature.Shape = ptList.ElementAt(f);
            //    //p_Feature.set_Value(pFeature.Fields.FindField("LineId"), pFeature.OID);
            //    //p_Feature.set_Value(pFeature.Fields.FindField("Number"), i + 1);
            //    //p_Feature.set_Value(pFeature.Fields.FindField("Elev"), pc.get_Point(i + 1).Z);
            //    //p_Feature.set_Value(pFeature.Fields.FindField("CancaveType"), 1);
            //    p_Feature.Store();
            //}
            MapHelp.AddFeatureClass2Map(p_feaPtCla, document.FocusMap, "feaPt");
            MapHelp.AddFeatureClass2Map(inflection_feaPtCla, document.FocusMap, "inflection");
        }
        /// <summary>
        /// 求转角angle
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <returns></returns>
        private double Angle(IPoint p1, IPoint p2, IPoint p3)
        {
            ILine line1 = new LineClass();
            ILine line2 = new LineClass();
            line1.PutCoords(p1, p2);
            line2.PutCoords(p2, p3);
            double angle1 = line1.Angle;
            double angle2 = line2.Angle;
            if (line1.Angle < 0)
            {
                angle1 = 2 * Math.PI + angle1;
            }
            if (line2.Angle < 0)
            {
                angle2 = 2 * Math.PI + angle2;
            }
            double differ = angle2 - angle1;
            if (Math.Abs(differ) > Math.PI)
            {
                if (differ > 0)
                {
                    differ = (differ - 2 * Math.PI);
                }
                else
                {
                    differ = (differ + 2 * Math.PI);
                }
            }
            return differ;
        }
        /// <summary>
        /// 求最大转角的索引
        /// </summary>
        /// <param name="pc"></param>
        /// <returns></returns>
        private int getMaxAngleIndex(IPointCollection pc)
        {
            double maxAngle = 0;
            int index = 0;
            for (int i = 0; i < pc.PointCount - 2; i++)
            {
                double angle = Angle(pc.get_Point(i), pc.get_Point(i + 1), pc.get_Point(i + 2));
                if (Math.Abs(angle) > Math.Abs(maxAngle))
                {
                    maxAngle = angle;
                    index = i + 1;
                }
            }
            return index;
        }

        /// <summary>
        /// reverse等高线方向时，判断等高线是否已经遍历
        /// </summary>
        /// <param name="table"></param>
        /// <param name="oid"></param>
        /// <returns></returns>
        private bool isLook(ITable table, int oid, int fieldIndex)
        {
            IRow row = table.GetRow(oid);
            object mark = row.get_Value(fieldIndex);
            if (Convert.ToInt16(mark) == 1)
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
