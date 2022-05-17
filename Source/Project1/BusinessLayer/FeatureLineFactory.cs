using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesFile;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geoprocessor;
using ESRI.ArcGIS.DataManagementTools;
using ESRI.ArcGIS.Geoprocessing;
using ESRI.ArcGIS.ConversionTools;
using ESRI.ArcGIS.Analyst3DTools;
using Utility;
using ESRI.ArcGIS.ArcMapUI;
using System.Diagnostics;
using log4net;
using System.Reflection;
using System.ComponentModel;
using DataStructure;

[assembly: log4net.Config.XmlConfigurator(ConfigFile = @"c:\temp\log\log4net.xml", Watch = true)]
namespace BusinessLayer
{
    /// <summary>
    /// 提取地貌结构线的工厂类
    /// </summary>
    public class FeatureLineFactory
    {
        IMxDocument _document;
        IFeatureClass _souceFC;//输入数据，待处理的等高线要素集

        IWorkspace _ws;//存放中间数据的工作空间


        Dictionary<int, IGeometry> _geometries = new Dictionary<int, IGeometry>();//保存原始等高线数据集内要素的geometry，用于后期选取相交边要素

        string _heightFieldName = string.Empty;//高程字段名称
        double _elevetionDiference = 20;//相信等高线的差

        double _angleMax;//跟踪三角形的最大角限制，大于该值不跟踪
        double _angleMin;//跟踪三角形的最小角限制，大于该值不跟踪

        string _uniqueName;//不重复名，采用当时时间，用于给中间要素集命名

        string _finalTinName = "final_tin";//最后生成Tin的名称

        IFeatureClass _featureLineFC = null;//用于存放生成的特征线

        ILog _log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);//日志对象

        BackgroundWorker _backWorker;//后台工作对象
        public BackgroundWorker BackWorker
        {
            get { return _backWorker; }
            set { _backWorker = value; }
        }

        DoWorkEventArgs _backWorkerEventArgs;//后台工作参数对象
        public DoWorkEventArgs BackWorkerEventArgs
        {
            get { return _backWorkerEventArgs; }
            set { _backWorkerEventArgs = value; }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="document">地图文档</param>
        /// <param name="fcPath">输入等高线要素集的路径</param>
        /// <param name="heightFieldName">高程字段名称</param>
        /// <param name="angleMax">跟踪三角形的最大角限制</param>
        /// <param name="angleMin">跟踪三角形的最小角限制</param>
        /// <param name="elevationDiff"></param>
        public FeatureLineFactory(IMxDocument document, string fcPath, string heightFieldName, double angleMax, double angleMin, double elevationDiff)
        {
            _document = document;
            _souceFC = GeoDBHelper.OpenShpFC(fcPath);
            _heightFieldName = heightFieldName;

            _angleMax = angleMax;
            _angleMin = angleMin;
            _elevetionDiference = elevationDiff;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="document">地图文档</param>
        /// <param name="fc">输入等高线要素集</param>
        /// <param name="heightFieldName">高程字段名称</param>
        /// <param name="angleMax">跟踪三角形的最大角限制</param>
        /// <param name="angleMin">跟踪三角形的最小角限制</param>
        /// <param name="elevationDiff"></param>

        public FeatureLineFactory(IMxDocument document, IFeatureClass fc, string heightFieldName, double angleMax, double angleMin, double elevationDiff)
        {
            _document = document;
            _souceFC = fc;
            _heightFieldName = heightFieldName;

            _angleMax = angleMax;
            _angleMin = angleMin;
            _elevetionDiference = elevationDiff;
        }

        /// <summary>
        /// 构造平三角网
        /// </summary>
        /// <param name="path">三角网输出路径</param>
        public string ConstructFeatureLine(string outputPath)
        {
            try
            {
                StringBuilder message = new StringBuilder();

                _uniqueName = 'A' + DateTime.Now.Ticks.ToString();
                _ws = GeoDBHelper.OpenInMemoryWorkspace();// OpenFileGdbScratchWorkspace();

                DateTime start = DateTime.Now;
                CreateSingleTin(_ws.PathName);
                TimeSpan ts = DateTime.Now - start;
                message.AppendLine(String.Format("拆分单根等高线构Tin，并转边耗时{0}秒", ts.TotalSeconds));
                
                //start = DateTime.Now;
                //DeleteIntersectEdge(gp);
                //ts = DateTime.Now - start;
                //message.AppendLine(String.Format("删除与等高线相交边耗时{0}秒", ts.TotalSeconds));
                
                start = DateTime.Now;
                ITin tin = CreateFinalTin(outputPath);
                MapHelper.AddTin2Map(tin, _document.FocusMap, _finalTinName);
                ts = DateTime.Now - start;
                message.AppendLine(String.Format("构造最终结果Tin耗时{0}秒", ts.TotalSeconds));

                start = DateTime.Now;
                //TrackFeautreLine(tin);
                MapHelper.AddFeatureClass2Map(_featureLineFC, _document.FocusMap);
                ts = DateTime.Now - start;
                message.AppendLine(String.Format("跟踪特征线耗时{0}秒", ts.TotalSeconds));

                _log.Info(message.ToString());
                return message.ToString();
            }
            catch (ApplicationException err)
            {
                if (_backWorkerEventArgs.Cancel)
                    return "用户取消！";
                else
                    throw err;
            }            
        }

        /// <summary>
        /// 跟踪特征线
        /// </summary>
        /// <param name="tin"></param>
        private void TrackFeautreLine(ITin tin)
        {
            ITinAdvanced2 tinAdv = tin as ITinAdvanced2;
            object missing = Type.Missing;
            
            for (int i = 1; i <= tin.DataTriangleCount; ++i)
            {
                ITinTriangle tri = tinAdv.GetTriangle(i);
                if (!tri.IsInsideDataArea)
                    continue;

                ITinEdge softEdge = null;
                ITinEdge regularEdge1 = null;
                ITinEdge regularEdge2 = null;

                if (tri.Edge[0].Type == esriTinEdgeType.esriTinSoftEdge && tri.Edge[1].Type == esriTinEdgeType.esriTinRegularEdge && tri.Edge[2].Type == esriTinEdgeType.esriTinRegularEdge)
                { softEdge = tri.Edge[0]; regularEdge1 = tri.Edge[1]; regularEdge2 = tri.Edge[2]; }
                else if (tri.Edge[1].Type == esriTinEdgeType.esriTinSoftEdge && tri.Edge[0].Type == esriTinEdgeType.esriTinRegularEdge && tri.Edge[2].Type == esriTinEdgeType.esriTinRegularEdge)
                { softEdge = tri.Edge[1]; regularEdge1 = tri.Edge[2]; regularEdge2 = tri.Edge[0]; }
                else if (tri.Edge[2].Type == esriTinEdgeType.esriTinSoftEdge && tri.Edge[1].Type == esriTinEdgeType.esriTinRegularEdge && tri.Edge[0].Type == esriTinEdgeType.esriTinRegularEdge)
                { softEdge = tri.Edge[2]; regularEdge1 = tri.Edge[0]; regularEdge2 = tri.Edge[1]; }
                
                if(softEdge != null && regularEdge1 != null  && regularEdge2 != null)
                {
                    IPoint midPtInHardline = new PointClass();
                    midPtInHardline.X = (softEdge.FromNode.X + softEdge.ToNode.X) / 2;
                    midPtInHardline.Y = (softEdge.FromNode.Y + softEdge.ToNode.Y) / 2;
                    midPtInHardline.Z = (softEdge.FromNode.Z + softEdge.ToNode.Z) / 2;

                    IPoint pt1 = GeometryHelper.CreatePoint(softEdge.FromNode.X, softEdge.FromNode.Y, softEdge.FromNode.Z);
                    IPoint pt2 = GeometryHelper.CreatePoint(softEdge.ToNode.X, softEdge.ToNode.Y, softEdge.ToNode.Z);
                    IPolyline pl = GeometryHelper.CreatePolylineFromTwoPt(pt1, pt2);
                    
                    if (pl != null)
                    {
                        ISpatialFilter filter = new SpatialFilter();
                        filter.WhereClause = string.Format("{0} = {1} OR {0} = {2}", _heightFieldName, pl.FromPoint.Z, pl.ToPoint.Z);
                        filter.Geometry = pl;
                        filter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                        filter.GeometryField = "SHAPE";

                        IFeatureCursor cursor = _featureLineFC.Search(filter, false);

                        IFeature ft = cursor.NextFeature();
                        while (ft != null)
                        {
                            IPolyline selectPl = ft.Shape as IPolyline;

                            if ((selectPl.FromPoint.X == midPtInHardline.X && selectPl.FromPoint.Y == midPtInHardline.Y)
                                || (selectPl.ToPoint.X == midPtInHardline.X && selectPl.ToPoint.Y == midPtInHardline.Y))
                            {
                                pt1 = GeometryHelper.CreatePoint(regularEdge1.ToNode.X, regularEdge1.ToNode.Y, regularEdge1.ToNode.Z);
                                pt2 = GeometryHelper.CreatePoint(midPtInHardline.X, midPtInHardline.Y, midPtInHardline.Z);
                                IPolyline ftPl = GeometryHelper.CreatePolylineFromTwoPt(pt1, pt2);
                                if (ftPl != null)
                                {
                                    int index = _featureLineFC.Fields.FindField(_heightFieldName);
                                    GeoDBHelper.AddFeature2FC(_featureLineFC, ftPl, new int[] { index }, new object[] { (ftPl.FromPoint.Z + ftPl.ToPoint.Z) / 2 });
                                }
                                break;
                            }
                            ft = cursor.NextFeature();
                        }
                    }

                }

            }
        }

        /// <summary>
        /// 生成结果Tin
        /// </summary>
        /// <param name="outputPath"></param>
        /// <param name="gp"></param>
        private ITin CreateFinalTin(string outputPath)
        {
            CheckBackWorker(_geometries.Count, "生成结果Tin");// 检查用户是否取消操作、报告进度
            int progress = 0;

            _log.Info("调试输出：生成结果Tin");
            ITinEdit tinEdit = null;
            try
            {
                IGeoDataset ds = _souceFC as IGeoDataset;
                tinEdit = new TinClass();
                tinEdit.InitNew(ds.Extent);

                ITinEdit2 tin = tinEdit as ITinEdit2;
                tinEdit.StartEditing();
                tin.SetToConstrainedDelaunay();

                object missing = Type.Missing;

                IField heightField = null;
                IField tagField = null;
                for (int i = 0; i < _souceFC.Fields.FieldCount; ++i)
                {
                    IField field = _souceFC.Fields.get_Field(i);
                    if (field.Type == esriFieldType.esriFieldTypeGeometry)
                        heightField = field;
                }

                tinEdit.AddFromFeatureClass(_souceFC, null, heightField, tagField, esriTinSurfaceType.esriTinHardLine, ref missing);

                foreach (int idName in _geometries.Keys)
                {
                    CheckBackWorker(progress++, null);// 检查用户是否取消操作、报告进度

                    _log.Info(string.Format("调试输出：加入id={0}号等高线生成的边，共{1}个", idName, _geometries.Keys.Count));

                    IFeatureWorkspace fws = _ws as IFeatureWorkspace;
                    IFeatureClass fc = fws.OpenFeatureClass(_uniqueName + '_' + idName.ToString() + "_TinEdge");

                    heightField = null;
                    tagField = null;
                    for (int i = 0; i < fc.Fields.FieldCount; ++i)
                    {
                        IField field = fc.Fields.get_Field(i);
                        if (field.Type == esriFieldType.esriFieldTypeGeometry)
                            heightField = field;
                        if (field.AliasName == "Code")
                            tagField = field;
                    }

                    if (heightField == null)
                        throw new Exception("生成Tin失败，打不到几何字段！");

                    tinEdit.AddFromFeatureClass(fc, null, heightField, tagField, esriTinSurfaceType.esriTinSoftLine, ref missing);
                }
                tinEdit.SaveAs(outputPath, true);
            }
            finally
            {
                tinEdit.StopEditing(true);
            }

            return tinEdit as ITin;
        }

        /// <summary>
        /// 为每根等高线生成Tin
        /// </summary>
        private void CreateSingleTin(string path)
        {
            CheckBackWorker(_souceFC.FeatureCount(null), "拆分单棵等高线于Tin，并转边集");// 检查用户是否取消操作、报告进度
            int progress = 0;

            _geometries.Clear();

            IFeatureClass sourceFC = _souceFC;
            IFeatureCursor pFCursor = sourceFC.Search(null, false);
            IFeature pFeature = pFCursor.NextFeature();

            if (_featureLineFC == null)
            {
                CreateFeatureLineFC(pFeature.Shape.SpatialReference);
            }

            //遍历要素，为每个要素生成Tin
            while (pFeature != null)
            {
                CheckBackWorker(progress++, null);// 检查用户是否取消操作、报告进度

                int number = pFeature.OID;
                _geometries[number] = pFeature.ShapeCopy;//保存要素形状

                _log.Info(string.Format("调试输出：转换oid={0}要素", number));

                // Instantiate a new empty TIN.
                ITinEdit TinEdit = new TinClass();

                // Initialize the TIN with an envelope. 
                TinEdit.InitNew(pFeature.Extent);

                object Missing = Type.Missing;
                TinEdit.AddShapeZ(pFeature.Shape, esriTinSurfaceType.esriTinHardLine, 0, ref Missing);

                IFeatureClass fc = GeoDBHelper.CreatFeatureClass(_ws as IFeatureWorkspace, _uniqueName + '_' + number.ToString() + "_TinEdge", 
                    esriGeometryType.esriGeometryPolyline, pFeature.Shape.SpatialReference, null, true);
                Tin2FeatureClass(fc, TinEdit as ITin);

                pFeature = pFCursor.NextFeature();
            }

        }

        /// <summary>
        /// convert Tin to featureclass
        /// </summary>
        /// <param name="path"></param>
        /// <param name="gp"></param>
        /// <param name="number"></param>
        /// <param name="tin"></param>
        private void Tin2FeatureClass(IFeatureClass fc, ITin tin)
        {
            ITinAdvanced2 tinAdv = tin as ITinAdvanced2;

            Dictionary<int, int> validTwoHardLineTri = new Dictionary<int, int>();
            HashSet<int> processedTri = new HashSet<int>();
            for (int i = 1; i <= tin.DataTriangleCount; ++i)
            {
                ITinTriangle tri = tinAdv.GetTriangle(i);
                if (!processedTri.Contains(tri.Index))
                {
                    if (tri.IsInsideDataArea)
                    {
                        int noneHardLineID = TinHelper.HasTwoHardEdge(tri);

                        if (noneHardLineID >= 0)
                        {
                            processedTri.Add(tri.Index);

                            IPolyline pl = TinHelper.CreatePolylineFromTinEdge(tri.Edge[noneHardLineID]);
                            if (IsNotIntersectWithContour(pl))//
                            {
                                GeoDBHelper.AddFeature2FC(fc, pl, null);

                                double angle = CalcuAngleOfEdges(tri.Edge[(noneHardLineID + 1) % 3], tri.Edge[(noneHardLineID + 2) % 3]);
                                if (angle >= _angleMax)
                                {
                                    //ConstructFeautureLine(tri, noneHardLineID);

                                    ProcessNeighbourTri(tri.Edge[noneHardLineID].LeftTriangle, tri.Edge[noneHardLineID], fc, false, processedTri);
                                }
                                else
                                {
                                    validTwoHardLineTri[i] = noneHardLineID;
                                }
                            }
                        } 
                    }
                }
            }

            foreach (KeyValuePair<int, int> item in validTwoHardLineTri)//从角度合格的两硬边三角形开始构造特征线、并输出平三角的边
            {
                ITinTriangle tri = tinAdv.GetTriangle(item.Key);
                int noneHardLineID = item.Value;
                ConstructFeautureLine(tri, noneHardLineID);

                ProcessNeighbourTri(tri.Edge[noneHardLineID].LeftTriangle, tri.Edge[noneHardLineID], fc, true, processedTri);

            }

            //MapHelper.AddFeatureClass2Map(fc, _document.FocusMap);

        }

        /// <summary>
        /// 计算两条边的夹角
        /// </summary>
        /// <param name="edge1"></param>
        /// <param name="edge2"></param>
        /// <returns></returns>
        private static double CalcuAngleOfEdges(ITinEdge edge1, ITinEdge edge2)
        {
            Vector_T vec1 = new Vector_T(edge2.ToNode.X - edge2.FromNode.X, edge2.ToNode.Y - edge2.FromNode.Y);
            Vector_T vec2 = new Vector_T(edge1.FromNode.X - edge1.ToNode.X, edge1.FromNode.Y - edge1.ToNode.Y);

            return vec2.Angle(vec1);
        }

        Dictionary<int, ITinEdge> _innerTri = new Dictionary<int, ITinEdge>();//用于记录内部三规则边的三角形，三角形的index，及跟踪边的index
        /// <summary>
        /// 递归处理以规则边相邻的三角形，提取特征线，及输出结构线
        /// </summary>
        /// <param name="iTinTriangle"></param>
        private void ProcessNeighbourTri(ITinTriangle tri, ITinEdge edge, IFeatureClass fc, bool trackFeatureLine, HashSet<int> processedTri)
        {
            if (tri == null || !tri.IsInsideDataArea || processedTri.Contains(tri.Index))
                return;

            if (TinHelper.HasThreeEdge(tri, esriTinEdgeType.esriTinRegularEdge))//如三个规则边，则连接质心到三个边的中心
            {
                for (int i = 0; i < 3; i++)
                {
                    IPolyline pl = TinHelper.CreatePolylineFromTinEdge(tri.Edge[i]);//edge

                    if (edge.GetNeighbor().Index != tri.Edge[i].Index)
                    {
                        if (!IsNotIntersectWithContour(pl))
                        {
                            processedTri.Add(tri.Index);
                            return;
                        }
                    }
                }

                if (trackFeatureLine)
                {
                    processedTri.Add(tri.Index);

                    if (_innerTri.ContainsKey(tri.Index))//如有一条不需跟踪的边，则连接另二条边的中点
                    {
                        ITinEdge invalidEdge = _innerTri[tri.Index];
                        ITinEdge edge1 = invalidEdge.GetNextInTriangle();
                        ITinEdge edge2 = invalidEdge.GetPreviousInTriangle();

                        IPolyline pl = TinHelper.ConnectMidOfEdges(edge1, edge2);//edge
                        int index = _featureLineFC.Fields.FindField(_heightFieldName);
                        GeoDBHelper.AddFeature2FC(_featureLineFC, pl, new int[] { index }, new object[] { pl.FromPoint.Z });

                        pl = TinHelper.CreatePolylineFromTinEdge(edge1);
                        GeoDBHelper.AddFeature2FC(fc, pl, null);
                        pl = TinHelper.CreatePolylineFromTinEdge(edge2);
                        GeoDBHelper.AddFeature2FC(fc, pl, null);

                        ProcessNeighbourTri(edge1.LeftTriangle, edge1, fc, trackFeatureLine, processedTri);
                        ProcessNeighbourTri(edge2.LeftTriangle, edge2, fc, trackFeatureLine, processedTri);
                    }
                    else
                    {
                        IPoint pt = new PointClass();
                        tri.QueryCentroid(pt);

                        for (int i = 0; i < 3; i++)
                        {
                            IPolyline pl = TinHelper.ConnectPt2MidOfEdge(tri.Edge[i], pt);//edge

                            int index = _featureLineFC.Fields.FindField(_heightFieldName);
                            GeoDBHelper.AddFeature2FC(_featureLineFC, pl, new int[] { index }, new object[] { pl.FromPoint.Z });

                            if (!(edge.FromNode.X == tri.Edge[i].ToNode.X && edge.FromNode.Y == tri.Edge[i].ToNode.Y))
                            {
                                pl = TinHelper.CreatePolylineFromTinEdge(tri.Edge[i]);
                                GeoDBHelper.AddFeature2FC(fc, pl, null);

                                ProcessNeighbourTri(tri.Edge[i].LeftTriangle, tri.Edge[i], fc, trackFeatureLine, processedTri);
                            }
                        }
                    }
                }
                else
                {
                    IPolyline pl = TinHelper.CreatePolylineFromTinEdge(edge);
                    GeoDBHelper.AddFeature2FC(fc, pl, null);

                    if (_innerTri.ContainsKey(tri.Index))//如已有一条不需跟踪的边，则不处理该内三角形
                    {
                        _innerTri.Remove(tri.Index);
                        processedTri.Add(tri.Index);

                    }
                    else
                    {
                        _innerTri[tri.Index] = edge.GetNeighbor();//记录内三角形的号，及不需构造特征线的边
                    }
                }

            }
            else
            {
                int hardLineID = TinHelper.HasTwoRegularEdge(tri);

                if (hardLineID >= 0)//如果有两个规则边，则连接两规则边中点
                {
                    processedTri.Add(tri.Index);
                    ITinEdge edge1 = tri.Edge[(hardLineID + 1) % 3];
                    ITinEdge edge2 = tri.Edge[(hardLineID + 2) % 3];

                    if (edge.FromNode.X == edge1.ToNode.X && edge.FromNode.Y == edge1.ToNode.Y)
                        edge = edge2;
                    else
                        edge = edge1;

                    if (hardLineID >= 0)
                    {
                        IPolyline pl = TinHelper.CreatePolylineFromTinEdge(edge);
                        if (IsNotIntersectWithContour(pl))//
                        {
                            GeoDBHelper.AddFeature2FC(fc, pl, null);
                            if (trackFeatureLine)
                            {
                                ConstructFeautureLine(edge1, edge2);
                            }

                            ProcessNeighbourTri(edge.LeftTriangle, edge, fc, trackFeatureLine, processedTri);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 检验几何图形是否与其它等高线相交 
        /// </summary>
        /// <param name="tri"></param>
        /// <returns></returns>
        private bool IsNotIntersectWithContour(IPolyline pl)
        {
            if (pl == null)
                return false;

            ISpatialFilter filter = new SpatialFilter();
            filter.WhereClause = string.Format("{0} = {1} OR {0} = {2}", _heightFieldName, pl.FromPoint.Z - _elevetionDiference, pl.FromPoint.Z + _elevetionDiference);
            filter.Geometry = pl;
            filter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
            filter.GeometryField = "SHAPE";
            
            IFeatureCursor cursor = _souceFC.Search(filter, false);

            IFeature ft = cursor.NextFeature();
            if (ft != null)
            {
                    return false;
            }
            return true;
        }

        /// <summary>
        /// 检验几何图形是否与其它等高线相交 
        /// </summary>
        /// <param name="tri"></param>
        /// <returns></returns>
        private bool IsNotIntersectWithContour(ILine line)
        {
            if (line == null)
                return false;

            ISpatialFilter filter = new SpatialFilter();
            filter.WhereClause = string.Format("{0} = {1} OR {0} = {2}", _heightFieldName, line.FromPoint.Z - _elevetionDiference, line.FromPoint.Z + _elevetionDiference);
            filter.Geometry = line;
            filter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
            filter.GeometryField = "SHAPE";

            IFeatureCursor cursor = _souceFC.Search(filter, false);

            IFeature ft = cursor.NextFeature();
            if (ft != null)
            {
                return false;
            }
            return true;
        }
        /// <summary>
        /// 从Tin中提取特征线
        /// </summary>
        /// <param name="tri"></param>
        /// <param name="noneHardLine"></param>
        /// <param name="spatialReference"></param>
        /// <param name="fc"></param>
        private void ConstructFeautureLine(ITinTriangle tri, int edgeID)
        {
            IPolyline pl = TinHelper.ConnectNode2Edge(tri.Edge[(edgeID + 1) % 3].ToNode, tri.Edge[edgeID]);

            int index = _featureLineFC.Fields.FindField(_heightFieldName);
            GeoDBHelper.AddFeature2FC(_featureLineFC, pl, new int[] { index }, new object[] { pl.FromPoint.Z });

        }

        /// <summary>
        /// 从Tin中提取特征线
        /// </summary>
        /// <param name="tri"></param>
        /// <param name="noneHardLine"></param>
        /// <param name="spatialReference"></param>
        /// <param name="fc"></param>
        private void ConstructFeautureLine(ITinEdge edge1, ITinEdge edge2)
        {
            IPolyline pl = TinHelper.ConnectMidOfEdges(edge1, edge2);

            int index = _featureLineFC.Fields.FindField(_heightFieldName);
            GeoDBHelper.AddFeature2FC(_featureLineFC, pl, new int[] { index }, new object[] { pl.FromPoint.Z });

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

            _featureLineFC = GeoDBHelper.CreatFeatureClass(_ws as IFeatureWorkspace, "FeatureLine", esriGeometryType.esriGeometryPolyline, spatialReference, pFields, true);
        }
        
        #region 后台工作
        /// <summary>
        /// 后台工作
        /// </summary>
        private void CheckBackWorker()
        {
            CheckBackWorker(-1, null);
        }

        /// <summary>
        /// 后台工作
        /// </summary>
        private void CheckBackWorker(int progress, object value)
        {
            if (_backWorker != null)
            {
                if (_backWorker != null && _backWorker.CancellationPending)//用户是否取消
                {
                    _backWorkerEventArgs.Cancel = true;
                    throw new ApplicationException("用户取消操作！");
                }

                if (progress >= 0)//报告进度
                {
                    if (value != null)
                        _backWorker.ReportProgress(progress, value);
                    else
                        _backWorker.ReportProgress(progress);
                }
            }
        }

        #endregion

        /// <summary>
        /// 删除与原始等高线相交的边
        /// </summary>
        /// <param name="gp"></param>
        //private void DeleteIntersectEdge(Geoprocessor gp)
        //{
        //    CheckBackWorker(_geometries.Count, "删除与原始等高线相交的边");// 检查用户是否取消操作、报告进度
        //    int progress = 0;

        //    foreach (int idName in _geometries.Keys)
        //    {
        //        CheckBackWorker(progress++, null);// 检查用户是否取消操作、报告进度

        //        _log.Info(string.Format("调试输出：删除id={0}号要素集的相交线，共{1}个", idName, _geometries.Keys.Count));

        //        IFeatureWorkspace fws = _ws as IFeatureWorkspace;
        //        IFeatureClass fc = fws.OpenFeatureClass(_uniqueName + '_' + idName.ToString() + "_TinEdge");

        //        IPolyline pl = _geometries[idName] as IPolyline;
        //        double zValue = pl.FromPoint.Z;

        //        foreach (int id in _geometries.Keys)//遍历等高线的几何形状
        //        {
        //            if (id != idName)
        //            {
        //                IPolyline pl1 = _geometries[id] as IPolyline;
        //                double zValue1 = pl1.FromPoint.Z;

        //                if (Math.Abs(zValue - zValue1) == _elevetionDiference)
        //                {
        //                    ISpatialFilter sFilter = new SpatialFilterClass();
        //                    sFilter.Geometry = _geometries[id];

        //                    sFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
        //                    sFilter.GeometryField = "SHAPE";

        //                    //删除相交的线要素
        //                    IFeatureCursor cursor = fc.Update(sFilter, false);
        //                    IFeature ft = cursor.NextFeature();
        //                    while (ft != null)
        //                    {
        //                        //DeleteFeatureLine(ft.Shape);
        //                        cursor.DeleteFeature();
        //                        ft = cursor.NextFeature();
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

        /// <summary>
        /// 删除由此被删除边生成的特征线
        /// </summary>
        /// <param name="basePL"></param>
        //private void DeleteFeatureLine(IGeometry basePL)
        //{
        //    ISpatialFilter sFilter = new SpatialFilterClass();
        //    sFilter.Geometry = basePL;

        //    sFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
        //    sFilter.GeometryField = "SHAPE";

        //    //删除相交的线要素
        //    IFeatureCursor cursor = _featureLineFC.Update(sFilter, false);
        //    IFeature ft = cursor.NextFeature();
        //    while (ft != null)
        //    {
        //        IPolyline pl1 = basePL as IPolyline;

        //        IPolyline pl2 = ft.Shape as IPolyline;

        //        if (pl1 != null & pl2 != null)
        //        {
        //            IPoint midPt = new PointClass();
        //            midPt.PutCoords((pl1.FromPoint.X + pl1.ToPoint.X) / 2, (pl1.FromPoint.Y + pl1.ToPoint.Y) / 2);
        //            //bool isDel = false;
        //            //不删除与该几何对象起始点相连的对象
        //            //if ((pl1.ToPoint.X != pl2.ToPoint.X || pl1.ToPoint.Y != pl2.ToPoint.Y) &&
        //            //    (pl1.FromPoint.X != pl2.ToPoint.X || pl1.FromPoint.Y != pl2.ToPoint.Y) &&
        //            //    (pl1.ToPoint.X != pl2.FromPoint.X || pl1.ToPoint.Y != pl2.FromPoint.Y) &&
        //            //    (pl1.FromPoint.X != pl2.FromPoint.X || pl1.FromPoint.Y != pl2.FromPoint.Y))
        //            if ((pl2.FromPoint.X == midPt.X && pl2.FromPoint.Y == midPt.Y) ||
        //                (pl2.ToPoint.X == midPt.X && pl2.ToPoint.Y == midPt.Y))
        //            {
        //                cursor.DeleteFeature();
        //                //isDel = true;
        //            }
        //            //if (isDel == false)
        //            //    isDel = false;
        //        }
        //        ft = cursor.NextFeature();
        //    }
        //}
    }
}
