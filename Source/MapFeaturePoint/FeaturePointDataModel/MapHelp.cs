using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.DataSourcesRaster;
using System.IO;
using ESRI.ArcGIS.DataSourcesFile;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.esriSystem;


namespace FeaturePointDataModel
{
    public static class MapHelp
    {
        public static int value = 0;
        public static int maxValue = 0;
        /// <summary>
        /// 获取某行某列的栅格值
        /// </summary>
        /// <param name="rasterLayer"></param>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <returns></returns>
        public static object rasterValue(IRasterLayer rasterLayer, int row, int col)
        {
            IRaster clipRaster = rasterLayer.Raster;
            IRasterProps rasterProps = (IRasterProps)clipRaster;
            int dHeight = rasterProps.Height;//当前栅格数据集的行数
            int dWidth = rasterProps.Width; //当前栅格数据集的列数
            double dX = rasterProps.MeanCellSize().X; //栅格的宽度
            double dY = rasterProps.MeanCellSize().Y; //栅格的高度
            IEnvelope extent = rasterProps.Extent; //当前栅格数据集的范围
            rstPixelType pixelType = rasterProps.PixelType; //当前栅格像素类型
            IPnt pntSize = new PntClass();
            pntSize.SetCoords(dX, dY);//扫面像素块的大小
            IPixelBlock pixelBlock = clipRaster.CreatePixelBlock(pntSize);//创建pixelBlock像素块
            IPnt pnt = new PntClass();
            pnt.SetCoords(row, col);//查找的位置即某行某列
            clipRaster.Read(pnt, pixelBlock);
            if (pixelBlock != null)
            {
                object obj = pixelBlock.GetVal(0, 0, 0);
                if (obj != null)
                {
                    return obj;
                }
                else
                {
                    return null;
                }
            }
            return null;
        }

        /// <summary>
        /// polyline转化为polygon
        /// </summary>
        /// <param name="pc"></param>
        /// <returns></returns>
        public static IPolygon polylineToPolygon(IPointCollection pc)
        {
            //Ring ring = new RingClass();
            //object missing = Type.Missing;
            //for (int i = 0; i < pc.PointCount; i++)
            //{
            //    ring.AddPoint(pc.get_Point(i), ref missing, ref missing);
            //}
            //IGeometryCollection pointPolygon = new PolygonClass();
            //pointPolygon.AddGeometry(ring as IGeometry, ref missing, ref missing);
            //IPolygon polyGonGeo = pointPolygon as IPolygon;
            //return polyGonGeo;


            IPolygon pgon = new PolygonClass();
            IPointCollection p_pc = pgon as IPointCollection;
            for (int i = 0; i < pc.PointCount; i++)
            {
                p_pc.AddPoint(pc.get_Point(i));
            }
            return pgon;
        }
        /// <summary>
        /// 链状结构形成包围关系
        /// </summary>
        /// <param name="fpt"></param>
        /// <param name="tpt"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <param name="p4"></param>
        /// <returns></returns>
        public static List<IPoint> addPoint(IPoint fpt, IPoint tpt, IPoint p1, IPoint p2, IPoint p3, IPoint p4)
        {
            int fptCount = 0;
            List<IPoint> ptList = new List<IPoint>();
            ptList.Add(p1);
            ptList.Add(p2);
            ptList.Add(p3);
            ptList.Add(p4);
            ptList.Add(p1);

            //起点入集合
            if ((Math.Abs(fpt.Y - p1.Y)) < 15 && (fpt.X >= p1.X && fpt.X < p2.X))
            {
                fptCount = 1;
                ptList.Insert(1, fpt);
            }
            else if (Math.Abs(fpt.X - p2.X) < 15 && (fpt.Y >= p3.Y && fpt.Y < p2.Y))
            {
                fptCount = 2;
                ptList.Insert(2, fpt);
            }
            else if (Math.Abs(fpt.Y - p3.Y) < 15 && (fpt.X >= p4.X && fpt.X < p3.X))
            {
                fptCount = 3;
                ptList.Insert(3, fpt);
            }
            else if (Math.Abs(fpt.X - p4.X) < 15 && (fpt.Y >= p4.Y && fpt.Y < p1.Y))
            {
                fptCount = 4;
                ptList.Insert(4, fpt);
            }

            //末点入集合
            if (Math.Abs(tpt.Y - p1.Y) < 15 && (tpt.X >= p1.X && tpt.X < p2.X))
            {
                if (fptCount >= 1)//fp插在在tp的后面，则tp还是插入原索引
                {
                    ptList.Insert(1, tpt);
                }
               
            }
            else if (Math.Abs(tpt.X - p2.X) < 15 && (tpt.Y >= p3.Y && tpt.Y < p2.Y))
            {
                if (fptCount >= 2)//fp插在在tp的后面，则tp还是插入原索引
                {
                    ptList.Insert(2, tpt);
                }
                else
                {
                    ptList.Insert(3, tpt);
                }
                
            }
            else if (Math.Abs(tpt.Y - p3.Y) < 15 && (tpt.X >= p4.X && tpt.X < p3.X))
            {
                if (fptCount >= 3)//fp插在在tp的后面，则tp还是插入原索引
                {
                    ptList.Insert(3, tpt);
                }
                else
                {
                    ptList.Insert(4, tpt);
                }
            }
            else if (Math.Abs(tpt.X - p4.X) < 15 && (tpt.Y >= p4.Y && tpt.Y < p1.Y))
            {
                if (fptCount >= 4)//fp插在在tp的后面，则tp还是插入原索引
                {
                    ptList.Insert(4, tpt);
                }
                else
                {
                    ptList.Insert(5, tpt);
                }
            }

            List<IPoint> outList = new List<IPoint>();
            int index = ptList.IndexOf(tpt);//找到终点所在索引
            if (index == -1)
            {
                outList.Add(tpt);
                return outList;
            }
            index++;
            for (; index < ptList.Count; index++)
            {
                if (ptList.ElementAt(index) == p1)//遇到p1进入下一个循环
                {
                    index = 0;
                    outList.Add(ptList.ElementAt(0));
                }
                else if (ptList.ElementAt(index) != fpt)//没有遇到起点就接着遍历
                {
                    outList.Add(ptList.ElementAt(index));
                }
                else
                {
                    break;
                }

            }
            return outList;
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

        /// <summary>
        /// 建立数据集featuresclass
        /// </summary>
        /// <param name="name"></param>
        /// <param name="pFWS"></param>
        /// <param name="SpatialRef"></param>
        /// <returns></returns>
        public static IFeatureClass getFeaCla(string name, IFeatureWorkspace pFWS, ISpatialReference SpatialRef)
        {

            //string strShapeFolder = "C:/";
            //string strShapeFile = "test.shp";

            //string shapeFileFullName = strShapeFolder + strShapeFile;
            //IWorkspaceFactory pWorkspaceFactory = new ShapefileWorkspaceFactory();
            //IFeatureWorkspace pFeatureWorkspace = (IFeatureWorkspace)pWorkspaceFactory.OpenFromFile(strShapeFolder, 0);
            IFeatureClass pFeatureClass;
            //if (File.Exists(shapeFileFullName))
            //{
            //    pFeatureClass = pFeatureWorkspace.OpenFeatureClass(strShapeFile);
            //    IDataset pDataset = (IDataset)pFeatureClass;
            //    pDataset.Delete();
            //}

            IFields pFields = new FieldsClass();
            IFieldsEdit pFieldsEdit = (IFieldsEdit)pFields;

            IField pField = new FieldClass();
            IFieldEdit pFieldEdit = (IFieldEdit)pField;

            pFieldEdit.Name_2 = "SHAPE";
            pFieldEdit.Type_2 = esriFieldType.esriFieldTypeGeometry;

            IGeometryDefEdit pGeoDef = new GeometryDefClass();
            IGeometryDefEdit pGeoDefEdit = (IGeometryDefEdit)pGeoDef;
            pGeoDefEdit.GeometryType_2 = esriGeometryType.esriGeometryPoint;
            pGeoDefEdit.SpatialReference_2 = SpatialRef; //new UnknownCoordinateSystemClass();
            pGeoDefEdit.HasM_2 = false;
            pGeoDefEdit.HasZ_2 = true;
            pFieldEdit.GeometryDef_2 = pGeoDef;
            pFieldsEdit.AddField(pField);

            pField = new FieldClass();
            pFieldEdit = (IFieldEdit)pField;
            pFieldEdit.Name_2 = "LineId";
            pFieldEdit.Type_2 = esriFieldType.esriFieldTypeInteger;
            pFieldsEdit.AddField(pField);

            pField = new FieldClass();
            pFieldEdit = (IFieldEdit)pField;
            pFieldEdit.Name_2 = "Number";
            pFieldEdit.Type_2 = esriFieldType.esriFieldTypeInteger;
            pFieldsEdit.AddField(pField);

            pField = new FieldClass();
            pFieldEdit = (IFieldEdit)pField;
            pFieldEdit.Name_2 = "Elev";
            pFieldEdit.Type_2 = esriFieldType.esriFieldTypeInteger;
            pFieldsEdit.AddField(pField);

            pField = new FieldClass();
            pFieldEdit = (IFieldEdit)pField;
            pFieldEdit.Name_2 = "CancaveType";
            pFieldEdit.Type_2 = esriFieldType.esriFieldTypeInteger;
            pFieldsEdit.AddField(pField);

            pField = new FieldClass();
            pFieldEdit = (IFieldEdit)pField;
            pFieldEdit.Name_2 = "Angle";
            pFieldEdit.Type_2 = esriFieldType.esriFieldTypeSingle;
            pFieldsEdit.AddField(pField);

            pFeatureClass = pFWS.CreateFeatureClass(name, pFields, null, null, esriFeatureType.esriFTSimple, "SHAPE", "");

            //for (int i = 0; i < ptList.Count; i++)
            //{
            //    IFeature pFeature = pFeatureClass.CreateFeature();
            //    pFeature.Shape = ptList.ElementAt(i);
            //    //pFeature.set_Value(pFeature.Fields.FindField("ID"), "D-1");
            //pFeature.set_Value(pFeature.Fields.FindField("Pixels"), 1);
            //    pFeature.Store();
            //}
            return pFeatureClass;
            //IFeatureLayer pFeaturelayer = new FeatureLayerClass();
            //pFeaturelayer.FeatureClass = pFeatureClass;
            //pFeaturelayer.Name = "layer";
            //axMapControl1.AddLayer(pFeaturelayer);
        }
        /// <summary>
        /// 将数据集添加到地图上
        /// </summary>
        /// <param name="fc"></param>
        /// <param name="map"></param>
        public static void AddFeatureClass2Map(IFeatureClass fc, IMap map, string layerName)
        {
            IFeatureLayer layer = new FeatureLayerClass();
            layer.FeatureClass = fc;
            layer.Name = layerName;
            map.AddLayer(layer);
        }
        /// <summary>
        /// //创建临时工作空间，用于存放临时数据
        /// </summary>
        /// <returns></returns>
        public static IWorkspace OpenInMemoryWorkspace()
        {
            IWorkspaceFactory workspaceFactory = new InMemoryWorkspaceFactoryClass();

            // Create a new in-memory workspace. This returns a name object.
            IWorkspaceName workspaceName = workspaceFactory.Create(null, "MyWorkspace", null, 0);
            IName name = (IName)workspaceName;

            // Open the workspace through the name object.
            IWorkspace workspace = (IWorkspace)name.Open();

            return workspace;
        }


    }
}
