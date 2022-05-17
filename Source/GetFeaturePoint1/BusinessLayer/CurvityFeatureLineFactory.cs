using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Geodatabase;
using Utility;
using ESRI.ArcGIS.Geometry;
using DataStructure;
using System.Drawing;
using System.Diagnostics;

namespace BusinessLayer
{
    public class CalculateSmooth
    {
        IMxDocument _document;
        IFeatureClass _sourceFC;//输入数据，待处理的等高线要素集

        IWorkspace _ws;//存放中间数据的工作空间
        
        private IFeatureClass _dividentPolyLineFC = null;

        
        string _heightFieldName = string.Empty;//高程字段名称
       
        
        private string readname;

        public string Readname
        {
            get { return readname; }
            set { readname = value; }
        }

        /// <summary>
        /// 计算圆滑
        /// </summary>
        /// <param name="document"></param>
        /// <param name="sourceFC"></param>
        /// <param name="heightFieldName"></param>
        /// <param name="span"></param>
        /// <param name="distance"></param>
        /// <param name="angleLimit"></param>
        public CalculateSmooth(IMxDocument document, IFeatureClass sourceFC, string heightFieldName)
        {
            _document = document;
            _sourceFC = sourceFC;
            _heightFieldName = heightFieldName;
        }


        /// <summary>
        /// 形成新线(chen)
        /// </summary>
        public string ConstructFeatureLine(int footsteplength)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            StringBuilder message = new StringBuilder();

            _ws = GeoDBHelper.OpenInMemoryWorkspace();// OpenFileGdbScratchWorkspace();

            IFeatureCursor ftCursor = _sourceFC.Search(null, false);
            IFeature feature = ftCursor.NextFeature();

            CreateFeatureLineFC(feature.Shape.SpatialReference);

            //遍历要素
            while (feature != null)
            {
                //IConstructMultipoint multipoint = new MultipointClass() as IConstructMultipoint;
                //multipoint.ConstructDivideLength(feature.Shape as ICurve, _distance);
                //IGeometry geo = feature.Shape;
                IPolyline pl = feature.Shape as IPolyline;
                IPointCollection pc = pl as IPointCollection;
               
                List<IPoint> PList = new List<IPoint>();
                IPoint Head = new PointClass();
                IPoint Tail = new PointClass();
                if (pl.IsClosed == false)//如多边形不闭合
                {
                    Tail = SmoothFormula.AddPoint(pc.get_Point(pc.PointCount - 3), pc.get_Point(pc.PointCount - 2), pc.get_Point(pc.PointCount - 1));
                    object missing = Type.Missing;
                    pc.AddPoint(Tail, ref missing, ref missing);//在尾部插入一个虚点
                }
                else
                {
                    Tail = pc.get_Point(1);
                    object missing = Type.Missing;
                    pc.AddPoint(Tail, ref missing, ref missing);//在尾部插入一个虚点
                }
                Head = SmoothFormula.AddPoint(pc.get_Point(0), pc.get_Point(1), pc.get_Point(2));
                pc.InsertPoints(0, 1, ref Head);//在头插入一个虚点

                PList.Add(pc.get_Point(1));//将第一个实点加入到点集合中
                for (int i = 0; i < pc.PointCount - 3; i++)
                {
                    double p0 = SmoothFormula.P0(pc.get_Point(i + 1).X);
                    double q0 = SmoothFormula.Q0(pc.get_Point(i + 1).Y);
                    double r = SmoothFormula.R(pc.get_Point(i + 1), pc.get_Point(i + 2));
                    IPoint pt1 = new PointClass();
                    IPoint pt2 = new PointClass();
                    double o1 = SmoothFormula.Angle(pc.get_Point(i), pc.get_Point(i + 1), pc.get_Point(i + 2), pt1);
                    double o2 = SmoothFormula.Angle(pc.get_Point(i + 1), pc.get_Point(i + 2), pc.get_Point(i + 3), pt2);
                    double length = SmoothFormula.Length(pc.get_Point(i + 1), pc.get_Point(i + 2));
                    int count = Convert.ToInt16(length / footsteplength);
                    for (int j = 0; j < count - 1; j++)
                    {
                        IPoint p = new PointClass();

                        double r1 = SmoothFormula.R1(r, 1.0 / count * (j + 1), o1, o2);
                        double p1 = SmoothFormula.P1(r1, o1);
                        double p2 = SmoothFormula.P2(pc.get_Point(i + 1).X, pc.get_Point(i + 2).X, o1, o2, r1);
                        double p3 = SmoothFormula.P3(pc.get_Point(i + 1).X, pc.get_Point(i + 2).X, o1, o2, r1);
                        double q1 = SmoothFormula.Q1(r1, o1);
                        double q2 = SmoothFormula.Q2(pc.get_Point(i + 1).Y, pc.get_Point(i + 2).Y, o1, o2, r1);
                        double q3 = SmoothFormula.Q3(pc.get_Point(i + 1).Y, pc.get_Point(i + 2).Y, o1, o2, r1);

                        double x = SmoothFormula.X(p0, p1, p2, p3, 1.0 / count * (j + 1));
                        double y = SmoothFormula.Y(q0, q1, q2, q3, 1.0 / count * (j + 1));
                        p.PutCoords(x, y);
                        PList.Add(p);
                    }
                    PList.Add(pc.get_Point(i + 2));
                }
                IPointCollection4 PointCollection = new MultipointClass();
                IPointCollection PointCollection2 = new MultipointClass();
                IGeometryBridge GeometryBridge = new GeometryEnvironmentClass();
                IPoint[] points = new PointClass[PList.Count];

                for (int k = 0; k < PList.Count; k++)
                {
                    IPoint point = new PointClass();
                    point.PutCoords(PList.ElementAt(k).X, PList.ElementAt(k).Y);
                    points[k] = point;
                }
                GeometryBridge.SetPoints(PointCollection, ref points);
                PointCollection2.AddPointCollection(PointCollection);
                int height = int.Parse(GeoDBHelper.GetValueOfFeature(feature, _heightFieldName).ToString());
                OutputMultiPoint(PointCollection2, height);
                feature = ftCursor.NextFeature();
            }

            AddResultsLayer2Map();

            stopWatch.Stop();

            TimeSpan ts = stopWatch.Elapsed;

            // Format and display the TimeSpan value.
            return String.Format("任务完成，总运行时间为：{0:00}:{1:00}:{2:00}", ts.Hours, ts.Minutes, ts.Seconds);
        }
        /// <summary>
        /// 将点数据写入要素集
        /// </summary>
        /// <param name="multipoint"></param>
        private void OutputMultiPoint(IPointCollection multipoint, int height)
        {
            IPolyline pl = new PolylineClass();
            GeometryHelper.Enable3DGeometry(pl as IGeometry);

            IPointCollection pc = pl as IPointCollection;
            for (int i = 0; i < multipoint.PointCount; ++i)
            {
                IPoint pt = multipoint.get_Point(i);
                GeometryHelper.Enable3DGeometry(pt as IGeometry);
                pt.Z = height;
                pc.AddPoint(pt);
            }

            int index = _dividentPolyLineFC.Fields.FindField(_heightFieldName);

            GeoDBHelper.AddFeature2FC(_dividentPolyLineFC, pl, new int[] { index }, new object[] { height });

        }
        /// <summary>
        /// 创建特征线要素集，具有高程字段
        /// </summary>
        /// <param name="spatialReference"></param>
        private void CreateFeatureLineFC(ISpatialReference spatialReference)
        {
            IFields pFields = new FieldsClass();
            IFieldsEdit pFieldsEdit = (IFieldsEdit)pFields;
            pFieldsEdit.FieldCount_2 = 1;

            IField pField = new FieldClass();
            IFieldEdit pFieldEdit = (IFieldEdit)pField;
            pFieldEdit.AliasName_2 = _heightFieldName;
            pFieldEdit.Name_2 = _heightFieldName;
            pFieldEdit.Type_2 = esriFieldType.esriFieldTypeInteger;
            pFieldsEdit.set_Field(0, pField);

            _dividentPolyLineFC = GeoDBHelper.CreatFeatureClass(_ws as IFeatureWorkspace, string.Format("smoothtLineFC{0}", ""), esriGeometryType.esriGeometryPolyline, spatialReference, pFields, true);
        }

        private void AddResultsLayer2Map()
        {
            MapHelper.AddFeatureClass2Map(_dividentPolyLineFC, _document.FocusMap);
        }
    }
}
