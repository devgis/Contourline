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
    public class CalculateMaxCurvity
    {
        IMxDocument _document;
        IFeatureClass _sourceFC;//输入数据，待处理的等高线要素集

        IWorkspace _ws;//存放中间数据的工作空间
        private IFeatureClass _featureLineFC = null;
        private IFeatureClass _featureLine2PointsFC = null;
        private IFeatureClass _dividentPolyLineFC = null;
        private IFeatureClass _inflectionPointFC = null;
        private IFeatureClass _curvityPointFC = null;        
        private IFeatureClass _curvityPolyLineFC = null;        

        double _elevetionDiference = 20;//相信等高线的差
        string _heightFieldName = string.Empty;//高程字段名称
        int _span = 2;//两点间的跨度
        int _distance = 5;
        int _angleLimit = 30;
        string _tempFold =  System.IO.Path.GetTempPath();
        string _curvityTypeName = "ConcaveType";
        double _angelLimitation = 1;

        private string _uniqueName;//用于唯一命名
        Dictionary<Point_T, bool> _curvityPoints = new Dictionary<Point_T, bool>();//曲率最大点

        /// <summary>
        /// 计算最大曲率点
        /// </summary>
        /// <param name="document"></param>
        /// <param name="sourceFC"></param>
        /// <param name="heightFieldName"></param>
        /// <param name="span"></param>
        /// <param name="distance"></param>
        /// <param name="angleLimit"></param>
        public CalculateMaxCurvity(IMxDocument document, IFeatureClass sourceFC, string tempFolder, string heightFieldName, int span, int distance, int angleLimit)
        {
            _document = document;
            _sourceFC = sourceFC;
            _heightFieldName = heightFieldName;

            //_tempFold = tempFolder;
            _span = span;
            _distance = distance;
            _angleLimit = angleLimit;
        }


        /// <summary>
        /// 计算最大曲率点主函数
        /// </summary>
        public string ConstructFeatureLine()
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            StringBuilder message = new StringBuilder();

            _uniqueName = string.Format("_{0}_{1}_{2}", _distance, _span, _angleLimit);
            _ws = GeoDBHelper.OpenInMemoryWorkspace();// OpenFileGdbScratchWorkspace();

            //ContourHelper.UnifyDirectionOfContour(_sourceFC);

            IFeatureCursor ftCursor = _sourceFC.Search(null, false);
            IFeature feature = ftCursor.NextFeature();

            CreateFeatureLineFC(feature.Shape.SpatialReference);

            //遍历要素，为每个要素生成Tin
            while (feature != null)
            {
                IConstructMultipoint multipoint = new MultipointClass() as IConstructMultipoint;
                multipoint.ConstructDivideLength(feature.Shape as ICurve, _distance);
                int height = int.Parse(GeoDBHelper.GetValueOfFeature(feature, _heightFieldName).ToString());
                OutputMultiPoint(multipoint as IPointCollection, height);
                
                ExtractPointByCurvity(multipoint as IPointCollection);

                feature = ftCursor.NextFeature();
            }


            AddResultsLayer2Map();

            stopWatch.Stop();

            TimeSpan ts = stopWatch.Elapsed;

            return String.Format("任务完成，总运行时间为：{0:00}:{1:00}:{2:00}",ts.Hours, ts.Minutes, ts.Seconds);
        }

        private void OutputFeatureLine2Points()
        {
            IFeatureCursor ftCursor = _featureLineFC.Search(null, false);
            IFeature feature = ftCursor.NextFeature();
            while (feature != null)
            {
                IPointCollection pc = feature.Shape as IPointCollection;

                for (int i = 0; i < pc.PointCount; ++i)
                {
                    IPoint pt = pc.get_Point(i);
                    IPoint value = new PointClass();
                    value.PutCoords(pt.X, pt.Y);

                    GeoDBHelper.AddFeature2FC(_featureLine2PointsFC, value, null, null);
                }

                feature = ftCursor.NextFeature();
            }
        }


        #region 曲率
        /// <summary>
        /// 通过拐点构造单区间
        /// </summary>
        /// <param name="pFeature"></param>
        private void ExtractPointByCurvity(IPointCollection pc)
        {
            if (pc == null)
                return;

            List<int> indexOfInflectionPoints = new List<int>();//拐点集合
            indexOfInflectionPoints.Add(0);
            int s = int.MinValue;
            for (int i = 1; i < pc.PointCount - 1; ++i)
            {
                IPoint fromPt = pc.get_Point(i-1);
                IPoint toPt = pc.get_Point(i);
                IPoint pt = pc.get_Point(i + 1);

                Point_T fromPoint = new Point_T(fromPt.X, fromPt.Y);
                Point_T toPoint = new Point_T(toPt.X, toPt.Y);
                Point_T point = new Point_T(pt.X, pt.Y);

                Vector_T vec1 = new Vector_T(fromPoint, toPoint);
                Vector_T vec2 = new Vector_T(toPoint, point);
                double angel = Math.Abs(vec1.Angle(vec2));

                ///////////////////////////////
                //ILine line1 = new LineClass();
                //ILine line2 = new LineClass();
                //line1.PutCoords(fromPt, toPt);
                //line2.PutCoords(toPt, pt);
                //double angle1 = line1.Angle;
                //double angle2 = line2.Angle;
                //if (line1.Angle < 0)
                //{
                //    angle1 = 2 * Math.PI + angle1;
                //}
                //if (line2.Angle < 0)
                //{
                //    angle2 = 2 * Math.PI + angle2;
                //}
                //double differ = angle2 - angle1;
                /////////////////

                if (s == int.MinValue && angel > _angelLimitation)
                {
                    s = Vector_T.WhichSidePointAt(fromPoint, toPoint, point);
                }
                else
                {
                    int s1 = Vector_T.WhichSidePointAt(fromPoint, toPoint, point);

                    if (angel > _angelLimitation && s != s1)
                    {
                        indexOfInflectionPoints.Add(i);
                        s = s1;
                    }
                }
            }
            indexOfInflectionPoints.Add(pc.PointCount - 1);

            CalculateCurvityPointByInflectionPoints(indexOfInflectionPoints, pc);

        }

        /// <summary>
        /// 在单个凸凹区内寻找曲率最大点
        /// </summary>
        private void CalculateCurvityPointByInflectionPoints(List<int> indexOfInflectionPoints, IPointCollection pc)
        {
            int indexHeightOfinflectionPointFC = _inflectionPointFC.Fields.FindField(_heightFieldName);
            //int indexHeightOfcurvityPolyLineFC = _curvityPolyLineFC.Fields.FindField(_heightFieldName);
            int indexHeightOfcurvityPointFC = _curvityPointFC.Fields.FindField(_heightFieldName);
            int indexConcaveOfCurvityFC = _curvityPointFC.Fields.FindField(_curvityTypeName);//

            IPolyline pl = new PolylineClass();
            GeometryHelper.Enable3DGeometry(pl as IGeometry);
            IPointCollection newPc = pl as IPointCollection;
            newPc.AddPoint(pc.get_Point(0));

            for (int i = 1; i < indexOfInflectionPoints.Count; i++)
            {
                IPoint startPt = pc.get_Point(indexOfInflectionPoints[i-1]);
                GeometryHelper.Enable3DGeometry(startPt as IGeometry);

                IPoint endPt = pc.Point[indexOfInflectionPoints[i]];
                GeometryHelper.Enable3DGeometry(endPt as IGeometry);

                if(i == 1)
                    GeoDBHelper.AddFeature2FC(_inflectionPointFC, startPt, new int[] { indexHeightOfinflectionPointFC }, new object[] { startPt.Z });
                GeoDBHelper.AddFeature2FC(_inflectionPointFC, endPt, new int[] { indexHeightOfinflectionPointFC }, new object[] { endPt.Z });

                IPoint maxPt = CalculateComplexMaxCurvityPoint(indexOfInflectionPoints[i - 1], indexOfInflectionPoints[i], pc);//CalculateSimpleMaxCurvityPoint

                if (maxPt != null)
                {
                    GeometryHelper.Enable3DGeometry(maxPt as IGeometry);

                    newPc.AddPoint(maxPt);

                    int type;
                    if(IsConvex(pc.get_Point(indexOfInflectionPoints[i - 1]), pc.get_Point(indexOfInflectionPoints[i]), maxPt))
                    {
                        _curvityPoints[new Point_T(maxPt.X, maxPt.Y, maxPt.Z)] = true;
                        type = 1;
                    }
                    else
                    {
                        _curvityPoints[new Point_T(maxPt.X, maxPt.Y, maxPt.Z)] = false;
                        type = -1;
                    }

                    GeoDBHelper.AddFeature2FC(_curvityPointFC, maxPt, new int[] { indexHeightOfcurvityPointFC, indexConcaveOfCurvityFC}, new object[] { maxPt.Z, type });
                }
                newPc.AddPoint(endPt);
            }

            IPoint lastPt = pc.Point[pc.PointCount - 1];
            newPc.AddPoint(lastPt);
            GeoDBHelper.AddFeature2FC(_curvityPolyLineFC, pl, new int[] { indexHeightOfcurvityPointFC }, new object[] { (int)lastPt.Z });

        }

        /// <summary>
        /// 判读当前曲率点是否为凸点（顺时针）
        /// </summary>
        /// <param name="p"></param>
        /// <param name="p_2"></param>
        /// <param name="maxPt"></param>
        /// <returns></returns>
        private bool IsConvex(IPoint fromPt, IPoint toPt, IPoint toBeJudgedPt)
        {
            Point_T from = new Point_T(fromPt.X, fromPt.Y);
            Point_T to = new Point_T(toPt.X, toPt.Y);
            Point_T pt = new Point_T(toBeJudgedPt.X, toBeJudgedPt.Y);

            if (Vector_T.WhichSidePointAt(from, to, pt) > 0)
                return true;
            else
                return false;
        }

                /// <summary>
        /// 采用多个角度计算单个凸凹区内的曲率最大点
        /// </summary>
        private IPoint CalculateComplexMaxCurvityPoint(int startIndex, int endIndex, IPointCollection pc)
        {
            if (endIndex - startIndex <= 1)
                return null;

            double maxAngle = double.MinValue;
            IPoint maxPt = null;

            int calcuCount = (endIndex - startIndex + 1) / 3;
            for (int i = startIndex + 1; i < endIndex; i++)
            {
                double angle = 0;
                IPoint secondPt = null;
                int weight = 0;
                for (int span = 1; span <= calcuCount; span++)
                {
                    int sIndex = i - span, eIndex = i + span;
                    if(sIndex < startIndex || eIndex > endIndex)
                        break;

                    IPoint firstPt = pc.get_Point(sIndex);
                    secondPt = pc.get_Point(i);
                    IPoint thirdPt = pc.get_Point(eIndex);

                    Vector_T vec1 = new Vector_T(secondPt.X - firstPt.X, secondPt.Y - firstPt.Y, 0);
                    Vector_T vec2 = new Vector_T(thirdPt.X - secondPt.X, thirdPt.Y - secondPt.Y, 0);

                    angle += span * vec1.Angle(vec2);
                    weight += span;
                }
                angle /= weight;
                if (angle > maxAngle)
                {
                    maxAngle = angle;
                    maxPt = secondPt;
                }
            }
            if (maxAngle > _angleLimit)
                return maxPt;
            else
                return null;
        }

        /// <summary>
        /// 采用一个角度计算单个凸凹区内的曲率最大点
        /// </summary>
        private IPoint CalculateSimpleMaxCurvityPoint(int startIndex, int endIndex, IPointCollection pc)
        {
            if (endIndex - startIndex <= 1)
                return null;

            double maxAngle = double.NaN;
            IPoint maxPt = new PointClass();
            for (int i = startIndex + 1; i < endIndex ; i++)
            {
                int sIndex = i - _span, eIndex = i + _span;
                if (sIndex < startIndex)
                    sIndex = startIndex;
                if (eIndex > endIndex)
                    eIndex = endIndex;

                IPoint firstPt = pc.get_Point(sIndex);
                IPoint secondPt = pc.get_Point(i);
                IPoint thirdPt = pc.get_Point(eIndex);
                //firstPt.Z = height;
                //secondPt.Z = height;
                //thirdPt.Z = height;

                Vector_T vec1 = new Vector_T(secondPt.X - firstPt.X, secondPt.Y - firstPt.Y, 0);
                Vector_T vec2 = new Vector_T(thirdPt.X - secondPt.X, thirdPt.Y - secondPt.Y, 0);

                double angle = vec1.Angle(vec2);
                if (angle > _angleLimit)
                {
                    if (double.IsNaN(maxAngle) || angle > maxAngle)
                    {
                        maxAngle = angle;
                        maxPt = secondPt;
                    }
                }
            }

            if (!double.IsNaN(maxAngle))
                return maxPt;
            return null;
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

            GeoDBHelper.AddFeature2FC(_dividentPolyLineFC, pl, new int[]{index}, new object[]{height});

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

            _dividentPolyLineFC = GeoDBHelper.CreatFeatureClass(_ws as IFeatureWorkspace, string.Format("dividentLineFC{0}", _uniqueName), esriGeometryType.esriGeometryPolyline, spatialReference, pFields, true);
            _inflectionPointFC = GeoDBHelper.CreatFeatureClass(_ws as IFeatureWorkspace, string.Format("InflectionPointFC{0}", _uniqueName), esriGeometryType.esriGeometryPoint, spatialReference, pFields, true);
            _curvityPolyLineFC = GeoDBHelper.CreatFeatureClass(_ws as IFeatureWorkspace, string.Format("curvityPolyLineFC{0}", _uniqueName), esriGeometryType.esriGeometryPolyline, spatialReference, pFields, true);
            _featureLineFC = GeoDBHelper.CreatFeatureClass(_ws as IFeatureWorkspace, string.Format("featureLineFC{0}", _uniqueName), esriGeometryType.esriGeometryPolyline, spatialReference, pFields, true);
            _featureLine2PointsFC = GeoDBHelper.CreatFeatureClass(_ws as IFeatureWorkspace, string.Format("featureLine2PointsFC{0}", _uniqueName), esriGeometryType.esriGeometryPoint, spatialReference, null, false);

            //曲率最大点，增加一个字段，用于表示凸凹
            pFields = new FieldsClass();
            pFieldsEdit = (IFieldsEdit)pFields;
            pFieldsEdit.FieldCount_2 = 2;

            pFieldsEdit.set_Field(0, pField);

            IField curvityType = new FieldClass();
            pFieldEdit = (IFieldEdit)curvityType;
            pFieldEdit.AliasName_2 = pFieldEdit.Name_2 = _curvityTypeName;
            pFieldEdit.Type_2 = esriFieldType.esriFieldTypeInteger;
            pFieldsEdit.set_Field(1, curvityType);
            _curvityPointFC = GeoDBHelper.CreatFeatureClass(_ws as IFeatureWorkspace, string.Format("curvityPointFC{0}", _uniqueName), esriGeometryType.esriGeometryPoint, spatialReference, pFields, true);

        }

        private void AddResultsLayer2Map()
        {
            MapHelper.AddFeatureClass2Map(_dividentPolyLineFC, _document.FocusMap);
            MapHelper.AddFeatureClass2Map(_inflectionPointFC, _document.FocusMap);
            MapHelper.AddFeatureClass2Map(_curvityPointFC, _document.FocusMap);
            MapHelper.AddFeatureClass2Map(_curvityPolyLineFC, _document.FocusMap);
            //MapHelper.AddFeatureClass2Map(_featureLineFC, _document.FocusMap);
            //MapHelper.AddFeatureClass2Map(_featureLine2PointsFC, _document.FocusMap);
        }

        #endregion

        #region Tin Operation
        
        private void TinOperation()
        {
            ITin tin = CreateTin();

            FindDirectConectedLine(tin);

            HashSet<int> processedTri = new HashSet<int>();
            TrackByLevelTriangle(tin, processedTri);
        }

        /// <summary>
        /// 根据平三角跟踪特征线
        /// </summary>
        /// <param name="path"></param>
        /// <param name="gp"></param>
        /// <param name="number"></param>
        /// <param name="tin"></param>
        private void TrackByLevelTriangle(ITin tin, HashSet<int> processedTri)
        {
            ITinAdvanced2 tinAdv = tin as ITinAdvanced2;

            Dictionary<int, int> validTwoHardLineTri = new Dictionary<int, int>();
            for (int i = 1; i <= tin.DataTriangleCount; ++i)
            {
                ITinTriangle tri = tinAdv.GetTriangle(i);
                if (!processedTri.Contains(tri.Index) && tri.IsInsideDataArea)
                {
                    int noneHardLineID = TinHelper.HasTwoHardEdge(tri);

                    if (noneHardLineID >= 0)
                    {
                        processedTri.Add(tri.Index);

                        ITinNode tinNode = tri.Edge[(noneHardLineID + 1)%3].ToNode;
                        Point_T tempPt = new Point_T(tinNode.X, tinNode.Y, tinNode.Z);

                        if (_curvityPoints.Keys.Contains(tempPt))
                        {

                            IPolyline pl = TinHelper.ConnectNode2Edge(tinNode, tri.Edge[noneHardLineID]);

                            GeoDBHelper.AddFeature2FC(_featureLineFC, pl, null, null);

                            ProcessNeighbourTri(tri.Edge[noneHardLineID].LeftTriangle, tri.Edge[noneHardLineID].GetNeighbor(), _featureLineFC, processedTri);
                        }

                    }
                }
            }
        }

        Dictionary<int, ITinEdge> _innerTri = new Dictionary<int, ITinEdge>();//用于记录内部三规则边的三角形，三角形的index，及跟踪边的index
        /// <summary>
        /// 递归处理以规则边相邻的三角形，提取特征线，及输出结构线
        /// </summary>
        /// <param name="iTinTriangle"></param>
        ///返回真表示跟踪出特征线，假表示未找到特征线
        private bool ProcessNeighbourTri(ITinTriangle tri, ITinEdge edge, IFeatureClass fc, HashSet<int> processedTri)
        {
            if (tri == null || !tri.IsInsideDataArea || processedTri.Contains(tri.Index))
                return false;

            if (TinHelper.HasThreeEdge(tri, esriTinEdgeType.esriTinRegularEdge))//如三个规则边，则连接质心到三个边的中心
            {
                if (!TinHelper.IsLevelTriangle(tri))//如不是平三角
                {
                    if (HasCurvityPointInTriangle(tri, edge, fc))//跟踪到另一条等高线，并有最大曲率点，结束
                        return true;
                }
                else
                {
                    processedTri.Add(tri.Index);

                    bool nextTri = ProcessNeighbourTri(edge.GetNextInTriangle().LeftTriangle, edge.GetNextInTriangle().GetNeighbor(), fc, processedTri);
                    bool prevTri = ProcessNeighbourTri(edge.GetPreviousInTriangle().LeftTriangle, edge.GetPreviousInTriangle().GetNeighbor(), fc, processedTri);

                    if (nextTri && prevTri)//如果其它二条边都跟踪出特征线
                    {
                        IPoint pt = new PointClass();
                        tri.QueryCentroid(pt);

                        for (int i = 0; i < 3; i++)
                        {
                            IPolyline pl = TinHelper.ConnectPt2MidOfEdge(tri.Edge[i], pt);//edge

                            GeoDBHelper.AddFeature2FC(fc, pl, null);
                        }
                        return true;
                    }
                    else if (prevTri)//如果只有条边都跟踪出特征线
                    {
                        IPolyline pl = TinHelper.ConnectMidOfEdges(edge, edge.GetNextInTriangle());//edge

                        GeoDBHelper.AddFeature2FC(fc, pl, null);
                        return true;
                    }
                    else if (nextTri)//如果只有条边都跟踪出特征线
                    {
                        IPolyline pl = TinHelper.ConnectMidOfEdges(edge, edge.GetPreviousInTriangle());//edge

                        GeoDBHelper.AddFeature2FC(fc, pl, null);
                        return true;
                    }
                    return false;
                }
            }
            else//如果有两个规则边，则连接两规则边中点
            {
                int hardLineID = TinHelper.HasTwoRegularEdge(tri);

                if (hardLineID >= 0)
                {
                    processedTri.Add(tri.Index);
                    ITinEdge edge1 = tri.Edge[(hardLineID + 1) % 3];
                    ITinEdge edge2 = tri.Edge[(hardLineID + 2) % 3];

                    IPolyline pl = TinHelper.ConnectMidOfEdges(edge1, edge2);
                    GeoDBHelper.AddFeature2FC(fc, pl, null);

                    ProcessNeighbourTri(edge1.LeftTriangle, edge1.GetNeighbor(), fc, processedTri);
                    ProcessNeighbourTri(edge2.LeftTriangle, edge2.GetNeighbor(), fc, processedTri);

                    return true;
                }
            }
            return false;
        }

        //表示到达另一条等高线，跟踪结束
        private bool HasCurvityPointInTriangle(ITinTriangle tri, ITinEdge edge, IFeatureClass fc)
        {
            ITinEdge nextEdge = edge.GetNextInTriangle();

            Point_T pt = new Point_T(nextEdge.ToNode.X, nextEdge.ToNode.Y, nextEdge.ToNode.Z);

            if (_curvityPoints.Keys.Contains(pt))
            {
                IPolyline pl = TinHelper.ConnectNode2Edge(nextEdge.ToNode, edge);
                GeoDBHelper.AddFeature2FC(fc, pl, null);

                return true;
            }
            return false;
        }

        /// <summary>
        /// 根据Tin，找到直接由TinEdge相连的曲率最大点
        /// </summary>
        /// <param name="pt1"></param>
        /// <param name="pt2"></param>
        /// <param name="pt3"></param>        
        private void FindDirectConectedLine(ITin tin)
        {
            ITinAdvanced2 tinAdv = tin as ITinAdvanced2;
            for (int i = 1; i <= tin.DataTriangleCount; ++i)
            {
                ITinTriangle tri = tinAdv.GetTriangle(i);
                Point_T pt1 = new Point_T(tri.Node[0].X, tri.Node[0].Y, tri.Node[0].Z);
                Point_T pt2 = new Point_T(tri.Node[1].X, tri.Node[1].Y, tri.Node[1].Z);
                Point_T pt3 = new Point_T(tri.Node[2].X, tri.Node[2].Y, tri.Node[2].Z);

                ConnectFeatureLineByPoints(pt1, pt2, pt3);
            }
        }


        private void ConnectFeatureLineByPoints(Point_T pt1, Point_T pt2, Point_T pt3)
        {
            if (pt1.Z != pt2.Z && _curvityPoints.Keys.Contains(pt1) && _curvityPoints.Keys.Contains(pt2) && _curvityPoints[pt1] == _curvityPoints[pt2])
            {
                IPolyline pl = GeometryHelper.CreatePolylineFromTwoPt(pt1, pt2);
                GeoDBHelper.AddFeature2FC(_featureLineFC, pl, null, null);
            }
            if (pt3.Z != pt2.Z && _curvityPoints.Keys.Contains(pt2) && _curvityPoints.Keys.Contains(pt3) && _curvityPoints[pt2] == _curvityPoints[pt3])
            {
                IPolyline pl = GeometryHelper.CreatePolylineFromTwoPt(pt2, pt3);
                GeoDBHelper.AddFeature2FC(_featureLineFC, pl, null, null);
            }
            if (pt1.Z != pt3.Z && _curvityPoints.Keys.Contains(pt3) && _curvityPoints.Keys.Contains(pt1) && _curvityPoints[pt3] == _curvityPoints[pt1])
            {
                IPolyline pl = GeometryHelper.CreatePolylineFromTwoPt(pt3, pt1);
                GeoDBHelper.AddFeature2FC(_featureLineFC, pl, null, null);
            }
        }


        /// <summary>
        /// 判断包围的区域面积是顺时针
        /// </summary>
        /// <param name="pc"></param>
        /// <returns></returns>
        public static bool isClockWise(IPointCollection pc)
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

        private ITin CreateTin()//string outputPath
        {
            ITinEdit tinEdit = null;
            try
            {
                IGeoDataset ds = _curvityPolyLineFC as IGeoDataset;
                tinEdit = new TinClass();
                tinEdit.InitNew(ds.Extent);

                ITinEdit2 tin = tinEdit as ITinEdit2;
                tinEdit.StartEditing();
                tin.SetToConstrainedDelaunay();

                object missing = Type.Missing;

                IField tagField = null;

                int index = _curvityPolyLineFC.Fields.FindField(_heightFieldName);
                IField heightField = _curvityPolyLineFC.Fields.get_Field(index);

                //for (int i = 0; i < _curvityPolyLineFC.Fields.FieldCount; ++i)
                //{
                //    IField field = _curvityPolyLineFC.Fields.get_Field(i);
                //    if (field.Type == esriFieldType.esriFieldTypeGeometry)
                //    {
                //        heightField = field;
                //        break;
                //    }
                //}

                tinEdit.AddFromFeatureClass(_curvityPolyLineFC, null, heightField, tagField, esriTinSurfaceType.esriTinHardLine, ref missing);

                tinEdit.SaveAs(string.Format(@"{0}\{1}", _tempFold, _uniqueName), true);
            }
            finally
            {
                tinEdit.StopEditing(true);

                MapHelper.AddTin2Map(tinEdit as ITin, _document.FocusMap, "TinLayer"); 
            }

            return tinEdit as ITin;
        }

        #endregion

    }
}
