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
    public class UnifyDirection
    {
        private IFeatureClass feaCla;
        int lookCount = 0;
        static IPoint p1, p2, p3, p4;
        public IFeatureClass FeaCla
        {
            get { return feaCla; }
            set { feaCla = value; }
        }
        public UnifyDirection(IFeatureClass cfeaCla)
        {
            feaCla = cfeaCla;
        }
        /// <summary>
        /// 获取闭合等高线中面积由小到大的feature的FID
        /// </summary>
        /// <param name="feacla"></param>
        /// <returns></returns>
        public List<int> getClosed()
        {
            List<int> oidList = new List<int>();
            SortedDictionary<double, int> sortFidDic = new SortedDictionary<double, int>();
            AddField(feaCla, "Mark", esriFieldType.esriFieldTypeSmallInteger);//添加标记字段
            AddField(feaCla, "Where", esriFieldType.esriFieldTypeSmallInteger);//
            IFeatureCursor pfcurse = feaCla.Search(null, false);
            IFeature pFeature = pfcurse.NextFeature();
            while (pFeature != null)
            {
                IPolyline pLine = pFeature.Shape as IPolyline;
                if (pLine.IsClosed)
                {
                    pFeature.set_Value(pFeature.Fields.FindField("Mark"), 2);//将此标记为1已遍历
                    pFeature.Store();//保存

                    IPointCollection pc = pLine as IPointCollection;
                    IPolygon pgn = MapHelp.polylineToPolygon(pc);
                    IArea area = pgn as IArea;
                    double d_area = Math.Abs(area.Area);
                    sortFidDic.Add(d_area, pFeature.OID);
                }
                pFeature = pfcurse.NextFeature();
            }
            foreach (var item in sortFidDic)//将面积由大到小的feature的FID加入到集合中
            {
                oidList.Add(item.Value);
            }
            MapHelp.maxValue = oidList.Count;
            return oidList;
        }

        /// <summary>
        /// 调整等高线方向
        /// </summary>
        /// <param name="feaCla"></param>
        /// <param name="oidList"></param>
        public void reverseLeftRight(List<int> oidList, bool leftRight)
        {
            List<int> isLookList = new List<int>();//存储已经遍历的等高线
            /*图框4个边角点*/
            IPoint p1 = Opt(feaCla).ElementAt(0);
            IPoint p2 = Opt(feaCla).ElementAt(1);
            IPoint p3 = Opt(feaCla).ElementAt(2);
            IPoint p4 = Opt(feaCla).ElementAt(3);
            try
            {
                lookCount++;
                for (int i = 0; i < oidList.Count; i++)
                {
                    if (oidList.ElementAt(i) == 310)
                    {
                        int a = 10;
                    }
                    int flag = 0;
                    foreach (var item in isLookList)
                    {
                        if (item == oidList.ElementAt(i))
                        {
                            flag = 1;
                            break;
                        }
                    }
                    if (flag == 1)//若原始的出发圆圈等高线已经被标记，则跳出进入下一个循环
                    {
                        continue;
                    }
                    IFeature fea = feaCla.GetFeature(oidList.ElementAt(i));
                    IPolyline pl = fea.Shape as IPolyline;//用来探测的圆圈等高线
                    IPointCollection pointCollection = pl as IPointCollection;
                    IPoint fp = pointCollection.get_Point(0);
                    IPoint tp = new PointClass();
                    if (leftRight)//向右探测线
                    {
                        fp.X = fp.X + 0.1;//探测线起点x值,y值不变
                        tp.Y = fp.Y;//探测线终点的y值
                        tp.X = p2.X;//探测线终点的x值
                    }
                    else//向下探测线
                    {
                        fp.X = fp.X - 0.1;
                        tp.Y = fp.Y;
                        tp.X = p1.X;
                    }

                    IPolyline line = new PolylineClass();//探测线
                    line.FromPoint = fp;
                    line.ToPoint = tp;
                    Dictionary<int, IPoint> dic = new Dictionary<int, IPoint>();//探测线与等高线相交于某条等高线的某个点
                    SortedDictionary<double, int> dictanceSort = new SortedDictionary<double, int>();//根据距离排序相交等高线的FID
                    dic = getPtCollection(feaCla, line, fp, oidList.ElementAt(i));
                    foreach (var item in dic)
                    {
                        IPolyline p_line = new PolylineClass();
                        p_line.FromPoint = fp;
                        p_line.ToPoint = item.Value;
                        double dictance = p_line.Length;
                        dictanceSort.Add(dictance, item.Key);
                    }
                    double origin = fp.Z;
                    bool isTop = false;
                    bool isLow = false;
                    IFeature p_fea = null;

                    for (int j = 0; j < dictanceSort.Count; j++)
                    {
                        int p_flag = 0;
                        if (getZ(feaCla.GetFeature(dictanceSort.ElementAt(j).Value)) < origin)//山顶圆圈线
                        {
                            if (isLow)
                            {
                                break;
                            }
                            isTop = true;
                            p_fea = feaCla.GetFeature(dictanceSort.ElementAt(j).Value);
                            IPolyline p_pl = p_fea.Shape as IPolyline;
                            if (j == 0)
                            {
                                if (MapHelp.isClockWise(pl as IPointCollection))//若是顺时针，则反向
                                {
                                    pl.ReverseOrientation();//只要离圆圈线最近的一个交点高程值比它低，则肯定是山顶
                                    fea.Store();
                                }
                            }
                            if (p_pl.IsClosed == false)
                            {
                                //foreach (var item in isLookList)
                                //{
                                //    if (item == p_fea.OID)//如果探测的等高线已经遍历标记过，则跳出接着下一根探测
                                //    {
                                //        p_flag = 1;
                                //    }
                                //}
                                int markIndex = Convert.ToInt16(p_fea.get_Value(p_fea.Fields.FindField("Mark")));
                                if (markIndex == 1 || markIndex == 2)
                                {
                                    origin = getZ(feaCla.GetFeature(dictanceSort.ElementAt(j).Value));//即使已经遍历过，等高线的高程值要更新
                                    continue;
                                }
                                //if (p_flag == 1)//如果探测的等高线已经遍历标记过，则跳出接着下一根探测
                                //{
                                //    origin = getZ(feaCla.GetFeature(dictanceSort.ElementAt(j).Value));
                                //    continue;
                                //}

                                unify(fp, p1, p2, p3, p4, p_fea, pl, true);//pl为圆圈等高线，p_fea为探测到的等高线
                                p_fea.set_Value(p_fea.Fields.FindField("Mark"), 1);//遍历后就标记等高线
                                p_fea.set_Value(p_fea.Fields.FindField("Where"), oidList.ElementAt(i));
                                isLookList.Add(p_fea.OID);
                                p_fea.Store();
                                origin = getZ(feaCla.GetFeature(dictanceSort.ElementAt(j).Value));
                            }
                            else//若探测到的等高线是闭合的
                            {
                                if (ptIsContain(p_pl, fp))//且又包含出发的圆圈等高线，则标记
                                {
                                    foreach (var item in isLookList)
                                    {
                                        if (item == p_fea.OID)
                                        {
                                            p_flag = 1;
                                        }
                                    }
                                    if (p_flag == 1)
                                    {
                                        origin = getZ(feaCla.GetFeature(dictanceSort.ElementAt(j).Value));
                                        continue;
                                    }
                                    unify(fp, p1, p2, p3, p4, p_fea, pl, true);//pl为圆圈等高线，p_fea为探测到的等高线
                                    p_fea.set_Value(p_fea.Fields.FindField("Mark"), 1);//遍历后就标记等高线
                                    p_fea.set_Value(p_fea.Fields.FindField("Where"), oidList.ElementAt(i));
                                    isLookList.Add(p_fea.OID);
                                    p_fea.Store();
                                    origin = getZ(feaCla.GetFeature(dictanceSort.ElementAt(j).Value));
                                }
                                else
                                {
                                    //origin = getZ(feaCla.GetFeature(dictanceSort.ElementAt(j).Value));
                                }
                            }

                        }
                        else if (getZ(feaCla.GetFeature(dictanceSort.ElementAt(j).Value)) > origin)//则为洼地圆圈
                        {
                            if (isTop)
                            {
                                break;
                            }
                            isLow = true;
                            p_fea = feaCla.GetFeature(dictanceSort.ElementAt(j).Value);
                            IPolyline p_pl = p_fea.Shape as IPolyline;
                            if (j == 0)
                            {
                                if (MapHelp.isClockWise(pl as IPointCollection) == false)//若是逆时针，则反向
                                {
                                    pl.ReverseOrientation();//只要离圆圈线最近的一个交点高程值比它高，则肯定是山洼地
                                    fea.Store();
                                }
                            }
                            if (p_pl.IsClosed == false)
                            {
                                //foreach (var item in isLookList)
                                //{
                                //    if (item == p_fea.OID)
                                //    {
                                //        p_flag = 1;
                                //    }
                                //}
                                int markIndex = Convert.ToInt16(p_fea.get_Value(p_fea.Fields.FindField("Mark")));
                                if (markIndex == 1 || markIndex == 2)
                                {
                                    origin = getZ(feaCla.GetFeature(dictanceSort.ElementAt(j).Value));//即使已经遍历过，等高线的高程值要更新
                                    continue;
                                }
                                //if (p_flag == 1)
                                //{
                                //    origin = getZ(feaCla.GetFeature(dictanceSort.ElementAt(j).Value));
                                //    continue;
                                //}
                                unify(fp, p1, p2, p3, p4, p_fea, pl, false);
                                p_fea.set_Value(p_fea.Fields.FindField("Mark"), 1);
                                p_fea.set_Value(p_fea.Fields.FindField("Where"), oidList.ElementAt(i));
                                isLookList.Add(p_fea.OID);
                                p_fea.Store();
                                origin = getZ(feaCla.GetFeature(dictanceSort.ElementAt(j).Value));
                            }
                            else//若探测到的等高线时闭合的
                            {
                                if (ptIsContain(p_pl, fp))//且又包含出发的圆圈等高线，则标记
                                {
                                    foreach (var item in isLookList)
                                    {
                                        if (item == p_fea.OID)
                                        {
                                            p_flag = 1;
                                        }
                                    }
                                    if (p_flag == 1)
                                    {
                                        origin = getZ(feaCla.GetFeature(dictanceSort.ElementAt(j).Value));
                                        continue;
                                    }
                                    unify(fp, p1, p2, p3, p4, p_fea, pl, true);//pl为圆圈等高线，p_fea为探测到的等高线
                                    p_fea.set_Value(p_fea.Fields.FindField("Mark"), 1);//遍历后就标记等高线
                                    p_fea.set_Value(p_fea.Fields.FindField("Where"), oidList.ElementAt(i));
                                    isLookList.Add(p_fea.OID);
                                    p_fea.Store();
                                    origin = getZ(feaCla.GetFeature(dictanceSort.ElementAt(j).Value));
                                }
                                else
                                {
                                    //origin = getZ(feaCla.GetFeature(dictanceSort.ElementAt(j).Value));
                                }
                            }
                        }
                        else if (getZ(feaCla.GetFeature(dictanceSort.ElementAt(j).Value)) == getZ(fea))
                        {
                            continue;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                if (lookCount > 1)
                {
                    return;
                }
                reverseLeftRight(oidList, false);

            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
        }


        public void reverseUpDown(List<int> oidList, bool updown)
        {
            List<int> isLookList = new List<int>();//存储已经遍历的等高线
            /*图框4个边角点*/
            IPoint p1 = Opt(feaCla).ElementAt(0);
            IPoint p2 = Opt(feaCla).ElementAt(1);
            IPoint p3 = Opt(feaCla).ElementAt(2);
            IPoint p4 = Opt(feaCla).ElementAt(3);
            try
            {
                lookCount++;
                for (int i = 0; i < oidList.Count; i++)
                {
                    if (oidList.ElementAt(i) == 141)
                    {
                        int a = 10;
                    }
                    int flag = 0;
                    foreach (var item in isLookList)
                    {
                        if (item == oidList.ElementAt(i))
                        {
                            flag = 1;
                            break;
                        }
                    }
                    if (flag == 1)//若原始的出发圆圈等高线已经被标记，则跳出进入下一个循环
                    {
                        continue;
                    }
                    IFeature fea = feaCla.GetFeature(oidList.ElementAt(i));
                    IPolyline pl = fea.Shape as IPolyline;//用来探测的圆圈等高线
                    IPointCollection pointCollection = pl as IPointCollection;
                    IPoint fp = pointCollection.get_Point(0);
                    IPoint tp = new PointClass();
                    if (updown)//向下探测线
                    {
                        fp.Y = fp.Y - 0.1;
                        tp.Y = p3.Y;
                        tp.X = fp.X;
                    }
                    else//向上探测线
                    {
                        fp.Y = fp.Y + 0.1;
                        tp.Y = p2.Y;
                        tp.X = fp.X;
                    }

                    IPolyline line = new PolylineClass();//探测线
                    line.FromPoint = fp;
                    line.ToPoint = tp;
                    Dictionary<int, IPoint> dic = new Dictionary<int, IPoint>();//探测线与等高线相交于某条等高线的某个点
                    SortedDictionary<double, int> dictanceSort = new SortedDictionary<double, int>();//根据距离排序相交等高线的FID
                    dic = getPtCollection(feaCla, line, fp, oidList.ElementAt(i));
                    foreach (var item in dic)
                    {
                        IPolyline p_line = new PolylineClass();
                        p_line.FromPoint = fp;
                        p_line.ToPoint = item.Value;
                        double dictance = p_line.Length;
                        dictanceSort.Add(dictance, item.Key);
                    }
                    double origin = fp.Z;
                    bool isTop = false;
                    bool isLow = false;
                    IFeature p_fea = null;

                    for (int j = 0; j < dictanceSort.Count; j++)
                    {
                        int p_flag = 0;
                        if (getZ(feaCla.GetFeature(dictanceSort.ElementAt(j).Value)) < origin)//山顶圆圈线
                        {
                            if (isLow)
                            {
                                break;
                            }
                            isTop = true;
                            p_fea = feaCla.GetFeature(dictanceSort.ElementAt(j).Value);
                            IPolyline p_pl = p_fea.Shape as IPolyline;
                            if (j == 0)
                            {
                                if (MapHelp.isClockWise(pl as IPointCollection))//若是顺时针，则反向，防止山顶圆圈线方向没有被调整
                                {
                                    pl.ReverseOrientation();//只要离圆圈线最近的一个交点高程值比它低，则肯定是山顶
                                    fea.Store();
                                }
                            }
                            if (p_pl.IsClosed == false)
                            {
                                //foreach (var item in isLookList)
                                //{
                                //    if (item == p_fea.OID)//如果探测的等高线已经遍历标记过，则跳出接着下一根探测
                                //    {
                                //        p_flag = 1;
                                //    }
                                //}
                                int markIndex = Convert.ToInt16(p_fea.get_Value(p_fea.Fields.FindField("Mark")));
                                if (markIndex == 1 || markIndex == 2)
                                {
                                    origin = getZ(feaCla.GetFeature(dictanceSort.ElementAt(j).Value));//即使已经遍历过，等高线的高程值要更新
                                    continue;
                                }
                                //if (p_flag == 1)//如果探测的等高线已经遍历标记过，则跳出接着下一根探测
                                //{
                                //    origin = getZ(feaCla.GetFeature(dictanceSort.ElementAt(j).Value));
                                //    continue;
                                //}

                                unify(fp, p1, p2, p3, p4, p_fea, pl, true);//pl为圆圈等高线，p_fea为探测到的等高线
                                p_fea.set_Value(p_fea.Fields.FindField("Mark"), 1);//遍历后就标记等高线
                                p_fea.set_Value(p_fea.Fields.FindField("Where"), oidList.ElementAt(i));
                                isLookList.Add(p_fea.OID);
                                p_fea.Store();
                                origin = getZ(feaCla.GetFeature(dictanceSort.ElementAt(j).Value));
                            }
                            else//若探测到的等高线是闭合的
                            {
                                if (ptIsContain(p_pl, fp))//且又包含出发的圆圈等高线，则标记
                                {
                                    foreach (var item in isLookList)
                                    {
                                        if (item == p_fea.OID)
                                        {
                                            p_flag = 1;
                                        }
                                    }
                                    if (p_flag == 1)
                                    {
                                        origin = getZ(feaCla.GetFeature(dictanceSort.ElementAt(j).Value));
                                        continue;
                                    }
                                    unify(fp, p1, p2, p3, p4, p_fea, pl, true);//pl为圆圈等高线，p_fea为探测到的等高线
                                    p_fea.set_Value(p_fea.Fields.FindField("Mark"), 1);//遍历后就标记等高线
                                    p_fea.set_Value(p_fea.Fields.FindField("Where"), oidList.ElementAt(i));
                                    isLookList.Add(p_fea.OID);
                                    p_fea.Store();
                                    origin = getZ(feaCla.GetFeature(dictanceSort.ElementAt(j).Value));
                                }
                                else
                                {
                                    //origin = getZ(feaCla.GetFeature(dictanceSort.ElementAt(j).Value));
                                }
                            }

                        }
                        else if (getZ(feaCla.GetFeature(dictanceSort.ElementAt(j).Value)) > origin)//则为洼地圆圈
                        {
                            if (isTop)
                            {
                                break;
                            }
                            isLow = true;
                            p_fea = feaCla.GetFeature(dictanceSort.ElementAt(j).Value);
                            IPolyline p_pl = p_fea.Shape as IPolyline;
                            if (j == 0)
                            {
                                if (MapHelp.isClockWise(pl as IPointCollection) == false)//若是逆时针，则反向
                                {
                                    pl.ReverseOrientation();//只要离圆圈线最近的一个交点高程值比它高，则肯定是山洼地
                                    fea.Store();
                                }
                            }
                            if (p_pl.IsClosed == false)
                            {
                                //foreach (var item in isLookList)
                                //{
                                //    if (item == p_fea.OID)
                                //    {
                                //        p_flag = 1;
                                //    }
                                //}
                                int markIndex = Convert.ToInt16(p_fea.get_Value(p_fea.Fields.FindField("Mark")));
                                if (markIndex == 1 || markIndex == 2)
                                {
                                    origin = getZ(feaCla.GetFeature(dictanceSort.ElementAt(j).Value));//即使已经遍历过，等高线的高程值要更新
                                    continue;
                                }
                                //if (p_flag == 1)
                                //{
                                //    origin = getZ(feaCla.GetFeature(dictanceSort.ElementAt(j).Value));
                                //    continue;
                                //}
                                unify(fp, p1, p2, p3, p4, p_fea, pl, false);
                                p_fea.set_Value(p_fea.Fields.FindField("Mark"), 1);
                                p_fea.set_Value(p_fea.Fields.FindField("Where"), oidList.ElementAt(i));
                                isLookList.Add(p_fea.OID);
                                p_fea.Store();
                                origin = getZ(feaCla.GetFeature(dictanceSort.ElementAt(j).Value));
                            }
                            else//若探测到的等高线时闭合的
                            {
                                if (ptIsContain(p_pl, fp))//且又包含出发的圆圈等高线，则标记
                                {
                                    foreach (var item in isLookList)
                                    {
                                        if (item == p_fea.OID)
                                        {
                                            p_flag = 1;
                                        }
                                    }
                                    if (p_flag == 1)
                                    {
                                        origin = getZ(feaCla.GetFeature(dictanceSort.ElementAt(j).Value));
                                        continue;
                                    }
                                    unify(fp, p1, p2, p3, p4, p_fea, pl, true);//pl为圆圈等高线，p_fea为探测到的等高线
                                    p_fea.set_Value(p_fea.Fields.FindField("Mark"), 1);//遍历后就标记等高线
                                    p_fea.set_Value(p_fea.Fields.FindField("Where"), oidList.ElementAt(i));
                                    isLookList.Add(p_fea.OID);
                                    p_fea.Store();
                                    origin = getZ(feaCla.GetFeature(dictanceSort.ElementAt(j).Value));
                                }
                                else
                                {
                                    //origin = getZ(feaCla.GetFeature(dictanceSort.ElementAt(j).Value));
                                }
                            }
                        }
                        else if (getZ(feaCla.GetFeature(dictanceSort.ElementAt(j).Value)) == getZ(fea))
                        {
                            continue;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                if (lookCount > 1)
                {
                    return;
                }
                reverseUpDown(oidList, false);

            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
        }
        public void reverseBorderLeftRight(List<int> oidList, bool leftRight)
        {
            List<int> isLookList = new List<int>();//存储已经遍历的等高线
            /*图框4个边角点*/
            IPoint p1 = Opt(feaCla).ElementAt(0);
            IPoint p2 = Opt(feaCla).ElementAt(1);
            IPoint p3 = Opt(feaCla).ElementAt(2);
            IPoint p4 = Opt(feaCla).ElementAt(3);
            try
            {
                lookCount++;
                for (int i = 0; i < oidList.Count; i++)
                {
                    int flag = 0;
                    foreach (var item in isLookList)
                    {
                        if (item == oidList.ElementAt(i))
                        {
                            flag = 1;
                            break;
                        }
                    }
                    if (flag == 1)//若原始的出发圆圈等高线已经被标记，则跳出进入下一个循环
                    {
                        continue;
                    }
                    IFeature fea = feaCla.GetFeature(oidList.ElementAt(i));
                    IPolyline pl = fea.Shape as IPolyline;//用来探测的圆圈等高线
                    IPointCollection pointCollection = pl as IPointCollection;
                    IPoint fp = pointCollection.get_Point(1);
                    IPoint tp = new PointClass();
                    if (leftRight)//向左探测线
                    {
                        fp.X = fp.X + 0.1;
                        tp.Y = fp.Y;
                        tp.X = p2.X;
                    }
                    else//向右探测线
                    {
                        fp.X = fp.X - 0.1;
                        tp.Y = fp.Y;
                        tp.X = p1.X;
                    }
                    IPolyline line = new PolylineClass();//探测线
                    line.FromPoint = fp;
                    line.ToPoint = tp;
                    Dictionary<int, IPoint> dic = new Dictionary<int, IPoint>();//探测线与等高线相交于某条等高线的某个点
                    SortedDictionary<double, int> dictanceSort = new SortedDictionary<double, int>();//根据距离排序相交等高线的FID
                    dic = getPtCollection(feaCla, line, fp, oidList.ElementAt(i));
                    foreach (var item in dic)
                    {
                        IPolyline p_line = new PolylineClass();
                        p_line.FromPoint = fp;
                        p_line.ToPoint = item.Value;
                        double dictance = p_line.Length;
                        dictanceSort.Add(dictance, item.Key);
                    }
                    double origin = fp.Z;
                    bool isTop = false;
                    bool isLow = false;
                    IFeature p_fea = null;


                    for (int j = 0; j < dictanceSort.Count; j++)
                    {
                        if (getZ(feaCla.GetFeature(dictanceSort.ElementAt(j).Value)) < origin)//山顶圆圈线
                        {
                            if (isLow)
                            {
                                break;
                            }
                            isTop = true;
                            p_fea = feaCla.GetFeature(dictanceSort.ElementAt(j).Value);
                            IPolyline p_pl = p_fea.Shape as IPolyline;
                            //if (j == 0)
                            //{
                            //    if (MapHelp.isClockWise(pl as IPointCollection))//若是顺时针，则反向
                            //    {
                            //        //pointCollection.RemovePoints(pointCollection.PointCount - addPointList.Count - 1, addPointList.Count + 1);
                            //        pl.ReverseOrientation();//只要离圆圈线最近的一个交点高程值比它低，则肯定是山顶
                            //        fea.Store();
                            //    }
                            //}

                            int markIndex = Convert.ToInt16(p_fea.get_Value(p_fea.Fields.FindField("Mark")));
                            if (markIndex == 1)
                            {
                                origin = getZ(feaCla.GetFeature(dictanceSort.ElementAt(j).Value));//即使已经遍历过，等高线的高程值要更新
                                continue;
                            }

                            unify(fp, p1, p2, p3, p4, p_fea, pl, true);//pl为圆圈等高线，p_fea为探测到的等高线
                            p_fea.set_Value(p_fea.Fields.FindField("Mark"), 1);//遍历后就标记等高线
                            p_fea.set_Value(p_fea.Fields.FindField("Where"), oidList.ElementAt(i));
                            isLookList.Add(p_fea.OID);
                            p_fea.Store();
                            origin = getZ(feaCla.GetFeature(dictanceSort.ElementAt(j).Value));


                        }
                        else if (getZ(feaCla.GetFeature(dictanceSort.ElementAt(j).Value)) > origin)//则为洼地圆圈
                        {
                            if (isTop)
                            {
                                break;
                            }
                            isLow = true;
                            p_fea = feaCla.GetFeature(dictanceSort.ElementAt(j).Value);
                            IPolyline p_pl = p_fea.Shape as IPolyline;
                            //if (j == 0)
                            //{
                            //    if (MapHelp.isClockWise(pl as IPointCollection) == false)//若是逆时针，则反向
                            //    {
                            //        //pointCollection.RemovePoints(pointCollection.PointCount - addPointList.Count - 1, addPointList.Count + 1);
                            //        pl.ReverseOrientation();//只要离圆圈线最近的一个交点高程值比它高，则肯定是山洼地
                            //        fea.set_Value(fea.Fields.FindField("Mark"), 1);
                            //        fea.Store();
                            //    }
                            //}

                            int markIndex = Convert.ToInt16(p_fea.get_Value(p_fea.Fields.FindField("Mark")));
                            if (markIndex == 1)
                            {
                                origin = getZ(feaCla.GetFeature(dictanceSort.ElementAt(j).Value));//即使已经遍历过，等高线的高程值要更新
                                continue;
                            }

                            unify(fp, p1, p2, p3, p4, p_fea, pl, false);
                            p_fea.set_Value(p_fea.Fields.FindField("Mark"), 1);
                            p_fea.set_Value(p_fea.Fields.FindField("Where"), oidList.ElementAt(i));
                            isLookList.Add(p_fea.OID);
                            p_fea.Store();
                            origin = getZ(feaCla.GetFeature(dictanceSort.ElementAt(j).Value));

                        }
                        else if (getZ(feaCla.GetFeature(dictanceSort.ElementAt(j).Value)) == getZ(fea))
                        {
                            continue;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                if (lookCount > 1)
                {
                    return;
                }
                reverseBorderLeftRight(oidList, false);

            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
        }
        public List<int> isNotLook()
        {
            IPoint p1 = Opt(feaCla).ElementAt(0);
            IPoint p2 = Opt(feaCla).ElementAt(1);
            IPoint p3 = Opt(feaCla).ElementAt(2);
            IPoint p4 = Opt(feaCla).ElementAt(3);
            IFeatureCursor pfcurse = feaCla.Search(null, false);
            IFeature pFeature = pfcurse.NextFeature();
            List<int> isNotlookList = new List<int>();
            SortedDictionary<double, int> sortDicArea = new SortedDictionary<double, int>();
            while (pFeature != null)
            {
                if (Convert.ToInt16(pFeature.get_Value(pFeature.Fields.FindField("Mark"))) == 0)
                {
                    IPolyline pl = pFeature.Shape as IPolyline;
                    IPointCollection pc = pl as IPointCollection;
                    //List<IPoint> addPointList = new List<IPoint>();

                    //addPointList = MapHelp.addPoint(pc.get_Point(0), pc.get_Point(pc.PointCount - 1), p1, p2, p3, p4);
                    //object missing = Type.Missing;
                    //for (int f = 0; f < addPointList.Count; f++)
                    //{
                    //    pc.AddPoint(addPointList.ElementAt(f), ref missing, ref missing);
                    //}
                    //pc.AddPoint(pc.get_Point(0), ref missing, ref missing);
                    IPolygon pgon = MapHelp.polylineToPolygon(pc);
                    IArea area = pgon as IArea;
                    double pgonArea = Math.Abs(area.Area);
                    sortDicArea.Add(pgonArea, pFeature.OID);
                }

                pFeature = pfcurse.NextFeature();
            }
            foreach (var item in sortDicArea)
            {
                isNotlookList.Add(item.Value);
            }
            return isNotlookList;
        }
        //public List<int> polylineArea(List<int> oidList)
        //{
        //    SortedDictionary<double, int> sortDicArea = new SortedDictionary<double, int>();
        //    List<int> sortBordList = new List<int>();
        //    foreach (var itemArea in oidList)
        //    {
        //        IFeature fea = feaCla.GetFeature(itemArea);
        //        IPolyline pl = fea.Shape as IPolyline;
        //        IPolygon pgon = MapHelp.polylineToPolygon(pl as IPointCollection);
        //        IArea area = pgon as IArea;
        //        double pgonArea = Math.Abs(area.Area);
        //        sortDicArea.Add(pgonArea, itemArea);
        //    }
        //    foreach (var item in sortDicArea)
        //    {
        //        sortBordList.Add(item.Value);
        //    }
        //    return sortBordList;
        //}
        //private Dictionary<int, bool> polylineIsClock(List<int> oidList)
        //{
        //    Dictionary<int, bool> polylineIsClockDic = new Dictionary<int, bool>();
        //    foreach (var item in oidList)
        //    {
        //        IFeature fea = feaCla.GetFeature(item);
        //        IPolyline pl = fea.Shape as IPolyline;
        //        bool isClock = MapHelp.isClockWise(pl as IPointCollection);
        //        polylineIsClockDic.Add(item, isClock);
        //    }
        //    return polylineIsClockDic;
        //}
        /// <summary>
        /// 所有等高线与探测线的交点集合
        /// </summary>
        /// <param name="feaCla"></param>
        /// <param name="line"></param>
        /// <returns></returns>
        private Dictionary<int, IPoint> getPtCollection(IFeatureClass feaCla, IPolyline line, IPoint fp, int oid)
        {
            IFeatureCursor pfcurse = feaCla.Search(null, false);
            IFeature pFeature = pfcurse.NextFeature();
            Dictionary<int, IPoint> dic = new Dictionary<int, IPoint>();
            while (pFeature != null)
            {
                IPolyline pl = pFeature.Shape as IPolyline;
                IPointCollection pc = pl as IPointCollection;
                IPoint pt = getMouPt(pl, line, fp);
                if (pt != null)
                {
                    dic.Add(pFeature.OID, pt);
                    pt.Z = pc.get_Point(0).Z;
                }
                pFeature = pfcurse.NextFeature();
            }
            return dic;
        }

        /// <summary>
        /// 遍历等高线时与探测线的第一个交点(与fp距离最近的那个交点)
        /// </summary>
        /// <param name="pl"></param>
        /// <param name="line"></param>
        /// <returns></returns>
        private IPoint getMouPt(IPolyline pl, IPolyline line, IPoint fp)
        {
            ITopologicalOperator topoOperator = pl as ITopologicalOperator;
            IGeometry geo = topoOperator.Intersect(line, esriGeometryDimension.esriGeometry0Dimension);
            IPoint pt = new PointClass();
            SortedDictionary<double, IPoint> sortDic = new SortedDictionary<double, IPoint>();
            ILine p_line = new LineClass();//相交线
            if (!geo.IsEmpty)
            {
                IPointCollection pc = geo as IPointCollection;
                for (int i = 0; i < pc.PointCount; i++)
                {
                    p_line.PutCoords(fp, pc.get_Point(i));
                    sortDic.Add(p_line.Length, pc.get_Point(i));
                }
                pt = sortDic.ElementAt(0).Value;//相交线最短
                return pt;
            }
            return null;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fea"></param>
        /// <returns></returns>
        private double getZ(IFeature fea)
        {
            IPolyline pl = fea.Shape as IPolyline;
            IPointCollection pc = pl as IPointCollection;
            IPoint fpt = pc.get_Point(0);
            return fpt.Z;
        }

        /// <summary>
        /// fp为山顶（洼地）圆圈线的起点
        /// extent为整个地图图幅的区域
        /// pl为山顶（洼地）圆圈线
        /// i为圆圈线中的第几条索引
        /// </summary>
        /// <param name="fp"></param>
        /// <param name="fea"></param>
        /// <param name="extent"></param>
        /// <param name="pl"></param>
        /// <param name="i"></param>
        private void unify(IPoint fp, IPoint p1, IPoint p2, IPoint p3, IPoint p4, IFeature fea, IPolyline pl, bool isTop)
        {
            IFeature p_fea = fea;
            IPolyline p_pl = p_fea.Shape as IPolyline;//探测到的等高线
            IPointCollection pc = p_pl as IPointCollection;
            IPoint fpt = pc.get_Point(0);
            IPoint tpt = pc.get_Point(pc.PointCount - 1);
            List<IPoint> vertexPointList = new List<IPoint>();
            List<IPoint> addPointList = new List<IPoint>();
            addPointList = MapHelp.addPoint(fpt, tpt, p1, p2, p3, p4);
            object missing = Type.Missing;
            for (int f = 0; f < addPointList.Count; f++)
            {
                pc.AddPoint(addPointList.ElementAt(f), ref missing, ref missing);
            }
            pc.AddPoint(pc.get_Point(0), ref missing, ref missing);
            if (isTop)//isTop为真，则是山顶
            {
                if (MapHelp.isClockWise(pl as IPointCollection) == false)//逆时针
                {
                    ptIsContain(p_pl, fp, addPointList, true);
                }
                else
                {
                    pl.ReverseOrientation();//因为已经确定为山顶，所以必须是逆时针
                    ptIsContain(p_pl, fp, addPointList, true);
                }
            }
            else//isTop为假，则是山洼地
            {
                if (MapHelp.isClockWise(pl as IPointCollection))//顺时针
                {
                    ptIsContain(p_pl, fp, addPointList, false);

                }
                else
                {
                    pl.ReverseOrientation();//因为已经确定为山洼地，所以必须是顺时针
                    ptIsContain(p_pl, fp, addPointList, false);
                }
            }

        }
        /// <summary>
        /// pl为探测到的等高线
        /// pc为探测到的线的点集合
        /// fp为山顶（洼地）圆圈线的起点，即栅格探测线的起点
        /// addPointList为探测到线上添加的点，以便形成一个闭合区域，进行包含判断
        /// </summary>
        /// <param name="pl"></param>
        /// <param name="pc"></param>
        /// <param name="fp"></param>
        /// <param name="addPointList"></param>
        private bool ptIsContain(IPolyline pl, IPoint fp)
        {
            IPolygon polygon = MapHelp.polylineToPolygon(pl as IPointCollection);
            IRelationalOperator pRelOpr = polygon as IRelationalOperator;
            if (pRelOpr.Contains(fp))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private void ptIsContain(IPolyline pl, IPoint fp, List<IPoint> addPointList, bool isTop)
        {
            IPointCollection pc = pl as IPointCollection;
            IPolygon polygon = MapHelp.polylineToPolygon(pc);
            IRelationalOperator pRelOpr = polygon as IRelationalOperator;

            if (pRelOpr.Contains(fp))//包含山顶或山洼地圆圈
            {
                if (isTop)//包含最里面山顶圆圈，则探测到的等高线就该是逆时针
                {
                    if (MapHelp.isClockWise(pc))//顺时针，则逆置
                    {
                        //删除添加的点（添加点是为了构成包围区域进行包围判断，包围判断已有，则要删除添加的多余点）
                        pc.RemovePoints(pc.PointCount - addPointList.Count - 1, addPointList.Count + 1);
                        pl.ReverseOrientation();
                    }
                    else
                    {
                        pc.RemovePoints(pc.PointCount - addPointList.Count - 1, addPointList.Count + 1);
                    }

                }
                else//包含最里面山洼地圆圈，则探测到的等高线就该是顺时针
                {
                    if (MapHelp.isClockWise(pc) == false)//逆时针，则逆置
                    {
                        pc.RemovePoints(pc.PointCount - addPointList.Count - 1, addPointList.Count + 1);
                        pl.ReverseOrientation();
                    }
                    else
                    {
                        pc.RemovePoints(pc.PointCount - addPointList.Count - 1, addPointList.Count + 1);
                    }
                }

            }
            else//若不包
            {
                if (isTop)//若不包含山顶圆圈，则探测到的等高线就该是顺时针
                {
                    if (MapHelp.isClockWise(pc))//顺时针则是对的
                    {
                        pc.RemovePoints(pc.PointCount - addPointList.Count - 1, addPointList.Count + 1);
                    }
                    else//反之逆置
                    {
                        pc.RemovePoints(pc.PointCount - addPointList.Count - 1, addPointList.Count + 1);
                        pl.ReverseOrientation();
                    }
                }
                else//若不包含山洼地圆圈，则探测到的等高线就该是逆时针
                {
                    if (MapHelp.isClockWise(pc) == false)//逆时针则是对的
                    {
                        pc.RemovePoints(pc.PointCount - addPointList.Count - 1, addPointList.Count + 1);
                    }
                    else//反之逆置
                    {
                        pc.RemovePoints(pc.PointCount - addPointList.Count - 1, addPointList.Count + 1);
                        pl.ReverseOrientation();
                    }

                }
            }
        }


        /// <summary>
        /// 返回图框的的四个顶点
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private List<IPoint> Opt(IFeatureClass fea)
        {
            IPoint p1 = new PointClass();
            IPoint p2 = new PointClass();
            IPoint p3 = new PointClass();
            IPoint p4 = new PointClass();
            IFeatureClass pFeatureClass = fea;
            IFeatureCursor pfcurse = pFeatureClass.Search(null, false);
            IFeature pFeature = pfcurse.NextFeature();
            int i = 0;
            double _xMax = 0;
            double _xMin = 0;
            double _yMax = 0;
            double _yMin = 0;
            while (pFeature != null)
            {
                IPolyline pl = pFeature.Shape as IPolyline;

                double xMax = pl.Envelope.XMax;
                double xMin = pl.Envelope.XMin;
                double yMax = pl.Envelope.YMax;
                double yMin = pl.Envelope.YMin;
                if (i == 0)
                {
                    _xMin = xMin;
                    _yMin = yMin;
                    _xMax = xMax;
                    _yMax = yMax;
                }
                i++;
                if (yMin < _yMin)
                {
                    _yMin = yMin;
                }
                if (xMin < _xMin)
                {
                    _xMin = xMin;
                }
                if (xMax > _xMax)
                {
                    _xMax = xMax;
                }
                if (yMax > _yMax)
                {
                    _yMax = yMax;
                }
                pFeature = pfcurse.NextFeature();
            }
            List<IPoint> ptList = new List<IPoint>();
            p1.PutCoords(_xMin, _yMax);
            p2.PutCoords(_xMax, _yMax);
            p3.PutCoords(_xMax, _yMin);
            p4.PutCoords(_xMin, _yMin);
            ptList.Add(p1);
            ptList.Add(p2);
            ptList.Add(p3);
            ptList.Add(p4);
            return ptList;
        }
        /// <summary>
        /// 添加字段
        /// </summary>
        /// <param name="pFeatureClass"></param>
        /// <param name="name"></param>
        private void AddField(IFeatureClass pFeatureClass, string name, esriFieldType type)
        {
            //若存在，则不需添加
            if (pFeatureClass.Fields.FindField(name) > -1) return;
            IField pField = new FieldClass();
            IFieldEdit pFieldEdit = pField as IFieldEdit;
            pFieldEdit.AliasName_2 = name;
            pFieldEdit.Name_2 = name;
            pFieldEdit.Type_2 = type;

            IClass pClass = pFeatureClass as IClass;
            pClass.AddField(pField);
        }
        /// <summary>
        /// 判断一等高线是否包含另一个等高线
        /// </summary>
        /// <param name="polygon1"></param>
        /// <param name="polygon2"></param>
        /// <returns></returns>
        //private bool polygonIsContain(IPolygon polygon1, IPolygon polygon2)
        //{
        //    IRelationalOperator pRelOpr = polygon1 as IRelationalOperator;
        //    if (pRelOpr.Contains(polygon2))
        //    {
        //        return true;
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}
    }
}
