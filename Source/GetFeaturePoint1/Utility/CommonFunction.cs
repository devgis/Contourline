using System;
using System.Collections.Generic;
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

namespace Utility
{
    public class CommonFunction
    {

        static public bool SplitPolygonsWithLines(string line_features, string polygon_features, string new_features)
        {
            try
            {
                ESRI.ArcGIS.Geoprocessor.Geoprocessor gp = new ESRI.ArcGIS.Geoprocessor.Geoprocessor();
                gp.OverwriteOutput = true;
                IGeoProcessorResult Result;

                string path = System.IO.Path.GetDirectoryName(new_features);
                string name = System.IO.Path.GetFileNameWithoutExtension(new_features);
                FeatureClassToFeatureClass ftf = new FeatureClassToFeatureClass(polygon_features, path, name);
                Result = gp.Execute(ftf, null) as IGeoProcessorResult;
                if (Result.Status != ESRI.ArcGIS.esriSystem.esriJobStatus.esriJobSucceeded)
                {
                    gp = null;
                    return false;
                }

                if (!SplitPolygonsWithLines(line_features, new_features))
                {
                    gp = null;
                    return false;
                }
                gp = null;
                return true;
            }
            catch
            {               
                return false;
            }
        }

        /// <summary>
        /// 用线图层切割面图层
        /// </summary>
        /// <param name="line_name">线图层文件名</param>
        /// <param name="polygon_name">面图层文件名</param>
        /// <returns></returns>
        static public bool SplitPolygonsWithLines(string line_features, string polygon_features)
        {
            try
            {
                IFeatureClass pFC_L = GetFeatureClass(line_features);
                IFeatureClass pFC_P = GetFeatureClass(polygon_features);
                //IFeatureClass pFC_New = CommonFunction.CreatFeatureClass(out_features, esriGeometryType.esriGeometryPolygon, (pFC_P as IGeoDataset).SpatialReference, pFC_P.Fields);

                IDataset pDataset = (IDataset)pFC_P;
                IWorkspaceEdit pWorkspaceEdit = (IWorkspaceEdit)pDataset.Workspace;
                pWorkspaceEdit.StartEditOperation();
                pWorkspaceEdit.StartEditing(true);

                IFeatureCursor pFCursor_P = pFC_P.Search(null, false);
                IFeature pF_P = pFCursor_P.NextFeature();
                IGeometry pGeo_P = null;
                IFeature pF_L;
                IFeatureCursor pFCursor_L;
                IFeatureEdit pFEdit;

                ITopologicalOperator2 pTopo = null;
                IGeometry pGeo_L = null;
                IGeometryBag pGeoBag = null;
                IGeometryCollection pCol_L = null;
                object before = Type.Missing;
                object after = Type.Missing;
                int count = pFC_P.FeatureCount(null);
                while (pF_P != null)
                {
                    try
                    {
                        pGeo_P = pF_P.Shape;

                        pGeo_L = new PolylineClass();
                        pGeoBag = new GeometryBagClass();
                        pGeoBag.SpatialReference = (pFC_L as IGeoDataset).SpatialReference;


                        pFCursor_L = SearchGeometry(pFC_L, pGeo_P, esriSpatialRelEnum.esriSpatialRelIntersects);
                        pF_L = pFCursor_L.NextFeature();
                        pGeo_L = pF_L.ShapeCopy;
                        pCol_L = pGeoBag as IGeometryCollection;
                        //pF_L = pFCursor_L.NextFeature();
                        while (pF_L != null)
                        {
                            pCol_L.AddGeometry(pF_L.Shape, ref before, ref after);
                            pF_L = pFCursor_L.NextFeature();
                        }
                        //pGeo_L.SpatialReference = pGeoBag.SpatialReference;
                        pTopo = pGeo_L as ITopologicalOperator2;
                        pTopo.ConstructUnion(pGeoBag as IEnumGeometry);
                        pTopo.Simplify();

                        pFEdit = pF_P as IFeatureEdit;
                        pFEdit.Split(pGeo_L);
                        pFEdit = null;
                        //pF_P.Delete();
                    }
                    catch
                    {
                    }
                    finally
                    {
                        pF_P = pFCursor_P.NextFeature();
                    }
                }
                pWorkspaceEdit.StopEditOperation();
                pWorkspaceEdit.StopEditing(true);

                //pDataset = (IDataset)pFC_P;
                //pWorkspaceEdit = (IWorkspaceEdit)pDataset.Workspace;
                //pWorkspaceEdit.StartEditOperation();
                //pWorkspaceEdit.StartEditing(true);

                //for (int i = 0; i < count; i++)
                //{
                //    pFC_P.GetFeature(i).Delete();
                //}
                //pWorkspaceEdit.StopEditOperation();
                //pWorkspaceEdit.StopEditing(true);
                //关闭工作空间
                pWorkspaceEdit = null;
                //pFcon = null;
                pFEdit = null;
                return true;
            }
            catch(Exception e)
            {
                throw new Exception("切割多边形出错！\n" + e.Message);
            }
        }

        /// <summary>
        /// 空间选择要素并输出到shp文件
        /// </summary>
        /// <param name="from_features_name">被选要素集文件名</param>
        /// <param name="by_features_name">空间关系要素集文件名</param>
        /// <param name="out_features_name">输出要素文件名</param>
        /// <param name="selMethod">空间关系类型</param>
        /// <returns></returns>
        static public bool SelectFeatures(string from_features_name, string by_features_name, string out_features_name, esriLayerSelectionMethod selMethod)
        {
            try
            {
                IFeatureClass pFC_By = GetFeatureClass(by_features_name);
                IFeatureClass pFC_From = GetFeatureClass(from_features_name);

                ISelectionSet pSelectionSet = SelectFeatures(pFC_By, pFC_From, selMethod);

                ExportSelectedFeatures(pFC_From, pSelectionSet, out_features_name);

                return true;
            }
            catch(Exception e)
            {
                throw e;
            }

        }
        /// <summary>
        /// 空间选择要素
        /// </summary>
        /// <param name="pFC_By">空间关系要素集文件名</param>
        /// <param name="pFC_From">被选要素集文件名</param>
        /// <param name="selMethod">空间关系类型</param>
        /// <returns>选择集</returns>
        static public ISelectionSet SelectFeatures(IFeatureClass pFC_By, IFeatureClass pFC_From, esriLayerSelectionMethod selMethod)
        {
            try
            {
                IQueryByLayer query = new QueryByLayerClass();
                IFeatureLayer pFL_By = new FeatureLayerClass();
                pFL_By.FeatureClass = pFC_By;
                IFeatureLayer pFL_From = new FeatureLayerClass();
                pFL_From.FeatureClass = pFC_From;
                query.ByLayer = pFL_By;
                query.FromLayer = pFL_From;
                query.UseSelectedFeatures = false;
                query.ResultType = esriSelectionResultEnum.esriSelectionResultAdd;
                query.LayerSelectionMethod = esriLayerSelectionMethod.esriLayerSelectIntersect;
                ISelectionSet pSelectionSet =  query.Select();

                return pSelectionSet;
        
            }
            catch (Exception e)
            {
                throw new Exception("选择要素出错！\n" + e.Message);
            }
 
        }

        /// <summary>
        /// 输出选择要素集到shp文件
        /// </summary>
        /// <param name="pFC">被选要素所在的FeatureClass</param>
        /// <param name="pSelectionSet">选择集</param>
        /// <param name="out_feature_name">输出shp文件名</param>
        /// <returns>是否成功</returns>
        static public bool ExportSelectedFeatures(IFeatureClass pFC, ISelectionSet pSelectionSet, string out_feature_name)
        {
            try
            {
                //设置inputFclassname
                IDataset pInDataset = pFC as IDataset;//获取当前图层的FeatureClass所在的数据集
                IDatasetName pInputFClassName = (IDatasetName)pInDataset.FullName;//获取输入的数据集的路径全名

                ///////下面的代码是设置，导出数据的一些属性
                string ExportFileShortName = System.IO.Path.GetFileNameWithoutExtension(out_feature_name);//获取导出shapefile的名称
                string ExportFilePath = System.IO.Path.GetDirectoryName(out_feature_name);//获取导出shapefile的路劲
                int idx = ExportFilePath.LastIndexOf("\\");
                string space_name = ExportFilePath.Substring(idx + 1, ExportFilePath.Length - idx - 1);
                ExportFilePath = ExportFilePath.Remove(idx);

                IWorkspaceFactory pShpWorkspaceFactory = new ShapefileWorkspaceFactoryClass();//创建一个输出shp文件的工作空间工厂，用于管理导出的数据
                IWorkspaceName pOutWorkspaceName = new WorkspaceNameClass();//实例化一个工作空间名称接口的实例对象
                pOutWorkspaceName = pShpWorkspaceFactory.Create(ExportFilePath, space_name, null, 0);//根据输入的路径和文件名创建一个工作空间名称对象"

                IFeatureDatasetName pOutFeatureDatasetName = null;//创建一个要素数据集名称对象，设置导出要素的数据集合名称
                IFeatureClassName pOutFeatureClassName = new FeatureClassNameClass();//创建一个要素类名称对象，用于实例化IDatasetName           
                IDatasetName pOutDatasetName;
                pOutDatasetName = (IDatasetName)pOutFeatureClassName;
                pOutDatasetName.Name = ExportFileShortName;//设置导出数据集的名称
                pOutDatasetName.WorkspaceName = pOutWorkspaceName;//设置导出数据集的工作空间的名称       

                IFields pInFields = pFC.Fields;//获取输入图层的要素类的字段集合

                //通过IFieldChecker检查字段的合法性，为输出SHP获得字段集合
                IFieldChecker pFieldChecker = new FieldChecker();
                IEnumFieldError pEnumFieldError = null;//用于获取错误字段的集合
                IFields pOutFields;//用于获取正确字段的集合
                pFieldChecker.Validate(pInFields, out pEnumFieldError, out pOutFields);

                IField pGeoField = null;//用于获取，输入图层的几何字段
                for (int iCounter = 0; iCounter < pOutFields.FieldCount; iCounter++) //通过循环查找几何字段
                {
                    if (pOutFields.get_Field(iCounter).Type == esriFieldType.esriFieldTypeGeometry)//根据字段类型，判读出几何字段
                    {
                        pGeoField = pOutFields.get_Field(iCounter);
                        break;
                    }
                }
                //得到几何字段的相关属性
                IGeometryDef pOutGeometryDef = pGeoField.GeometryDef;
                //设置几何字段的空间参考和网格
                //IGeometryDefEdit pOutGeometryDefEdit = (IGeometryDefEdit)pOutGeometryDef;
                //pOutGeometryDefEdit.GridCount_2 = 1;
                //pOutGeometryDefEdit.set_GridSize(0, 1500000);


                IFeatureDataConverter2 pFeaDaCon = new FeatureDataConverterClass();
                //带入参数，导出数据
                pFeaDaCon.ConvertFeatureClass(pInputFClassName, null, pSelectionSet, pOutFeatureDatasetName, pOutFeatureClassName, pOutGeometryDef, pOutFields, "", 1000, 0);

                pFeaDaCon = null;
                pFieldChecker = null;
                //MessageBox.Show("导出成功", "系统提示");
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("the following exception occurred:" + ex.ToString());
            }


        }

        static public bool ExportFeatures(string in_feature_name, string out_feature_name)
        {
            try
            {
                IFeatureClass pFC = GetFeatureClass(in_feature_name);
                ISelectionSet pSelectionSet = pFC.Select(null, esriSelectionType.esriSelectionTypeIDSet, esriSelectionOption.esriSelectionOptionNormal, null);
                //设置inputFclassname
                IDataset pInDataset = pFC as IDataset;//获取当前图层的FeatureClass所在的数据集
                IDatasetName pInputFClassName = (IDatasetName)pInDataset.FullName;//获取输入的数据集的路径全名

                ///////下面的代码是设置，导出数据的一些属性
                string ExportFileShortName = System.IO.Path.GetFileNameWithoutExtension(out_feature_name);//获取导出shapefile的名称
                string ExportFilePath = System.IO.Path.GetDirectoryName(out_feature_name);//获取导出shapefile的路劲
                int idx = ExportFilePath.LastIndexOf("\\");
                string space_name = ExportFilePath.Substring(idx + 1, ExportFilePath.Length - idx - 1);
                ExportFilePath = ExportFilePath.Remove(idx);

                IWorkspaceFactory pShpWorkspaceFactory = new ShapefileWorkspaceFactoryClass();//创建一个输出shp文件的工作空间工厂，用于管理导出的数据
                IWorkspaceName pOutWorkspaceName = new WorkspaceNameClass();//实例化一个工作空间名称接口的实例对象
                pOutWorkspaceName = pShpWorkspaceFactory.Create(ExportFilePath, space_name, null, 0);//根据输入的路径和文件名创建一个工作空间名称对象"

                IFeatureDatasetName pOutFeatureDatasetName = null;//创建一个要素数据集名称对象，设置导出要素的数据集合名称
                IFeatureClassName pOutFeatureClassName = new FeatureClassNameClass();//创建一个要素类名称对象，用于实例化IDatasetName           
                IDatasetName pOutDatasetName;
                pOutDatasetName = (IDatasetName)pOutFeatureClassName;
                pOutDatasetName.Name = ExportFileShortName;//设置导出数据集的名称
                pOutDatasetName.WorkspaceName = pOutWorkspaceName;//设置导出数据集的工作空间的名称       

                IFields pInFields = pFC.Fields;//获取输入图层的要素类的字段集合

                //通过IFieldChecker检查字段的合法性，为输出SHP获得字段集合
                IFieldChecker pFieldChecker = new FieldChecker();
                IEnumFieldError pEnumFieldError = null;//用于获取错误字段的集合
                IFields pOutFields;//用于获取正确字段的集合
                pFieldChecker.Validate(pInFields, out pEnumFieldError, out pOutFields);

                IField pGeoField = null;//用于获取，输入图层的几何字段
                for (int iCounter = 0; iCounter < pOutFields.FieldCount; iCounter++) //通过循环查找几何字段
                {
                    if (pOutFields.get_Field(iCounter).Type == esriFieldType.esriFieldTypeGeometry)//根据字段类型，判读出几何字段
                    {
                        pGeoField = pOutFields.get_Field(iCounter);
                        break;
                    }
                }
                //得到几何字段的相关属性
                IGeometryDef pOutGeometryDef = pGeoField.GeometryDef;
                //设置几何字段的空间参考和网格
                //IGeometryDefEdit pOutGeometryDefEdit = (IGeometryDefEdit)pOutGeometryDef;
                //pOutGeometryDefEdit.GridCount_2 = 1;
                //pOutGeometryDefEdit.set_GridSize(0, 1500000);


                IFeatureDataConverter2 pFeaDaCon = new FeatureDataConverterClass();
                //带入参数，导出数据
                pFeaDaCon.ConvertFeatureClass(pInputFClassName, null, pSelectionSet, pOutFeatureDatasetName, pOutFeatureClassName, pOutGeometryDef, pOutFields, "", 1000, 0);

                pFeaDaCon = null;
                pFieldChecker = null;
                //MessageBox.Show("导出成功", "系统提示");
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("the following exception occurred:" + ex.ToString());
            }


        }

        /// <summary>
        /// 求一个图层的自相交部分
        /// </summary>
        /// <param name="pFC">数据源图层</param>
        /// <param name="pFC_New">结果数据目标图层</param>
        static public IFeatureClass SelfIntersect(IFeatureClass pFC, IFeatureClass pFC_New)
        {
            try
            {
                IDataset pDataset = (IDataset)pFC_New;
                IWorkspaceEdit pWorkspaceEdit = (IWorkspaceEdit)pDataset.Workspace;
                pWorkspaceEdit.StartEditOperation();
                pWorkspaceEdit.StartEditing(true);
                try
                {
                    //自相交标记
                    List<int> indexes = new List<int>();

                    IFeatureCursor pFCursor = pFC.Search(null, false);
                    IFeature pF = pFCursor.NextFeature();

                    esriGeometryDimension esGD;
                    if (pFC.ShapeType == esriGeometryType.esriGeometryPolygon)
                    {
                        esGD = esriGeometryDimension.esriGeometry2Dimension;
                    }
                    else
                    {
                        esGD = esriGeometryDimension.esriGeometry1Dimension;
                    }



                    while (pF != null)
                    {
                        IFeatureCursor pfcursor = SearchGeometry(pFC, pF.Shape, esriSpatialRelEnum.esriSpatialRelIntersects);
                        IFeature pf = pfcursor.NextFeature();
                        ITopologicalOperator2 pTopo = pF.Shape as ITopologicalOperator2;
                        pTopo.Simplify();
                        pF.Shape.SnapToSpatialReference();

                        while (pf != null)
                        {
                            //如果找到自己本身或已经做过的，则跳过，否则继续
                            if (pf.OID != pF.OID && !indexes.Contains(pf.OID))
                            {
                                IGeometry geometry = pTopo.Intersect(pf.Shape, esGD);
                                if ((geometry as IArea).Area > 0)
                                {
                                    IFeature feature = pFC_New.CreateFeature();
                                    feature.Shape = geometry;
                                    feature.Store();
                                }
                            }
                            pf = pfcursor.NextFeature();
                        }
                        indexes.Add(pF.OID);
                        pF.Store();
                        pF = pFCursor.NextFeature();
                    }

                    pWorkspaceEdit.StopEditOperation();
                    pWorkspaceEdit.StopEditing(true);

                    return pFC_New;
                }
                catch (Exception e)
                {
                    pWorkspaceEdit.StopEditOperation();
                    pWorkspaceEdit.StopEditing(false);
                    pWorkspaceEdit = null;
                    throw new Exception("自相交出错！\n" + e.Message);
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }


        static public bool SelfIntersect(string in_features, string out_features)
        {
            try
            {
                IFeatureClass pFC = GetFeatureClass(in_features);

                IFeatureClass pFC_New = CreatFeatureClass(out_features, pFC.ShapeType, (pFC as IGeoDataset).SpatialReference, null, true);
                SelfIntersect(pFC, pFC_New);
                return true;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// 将多要素merge到一起
        /// </summary>
        /// <param name="pFC"></param>
        /// <param name="pFC_New">数据源图层</param>
        /// <param name="geo_type">结果数据目标图层</param>
        /// <returns></returns>       
        static public IFeatureClass MergeTogetherToNewFeature(IFeatureClass pFC, IFeatureClass pFC_New)
        {
            try
            {
                IGeometry geo_new;              
                //if (geo_type == esriGeometryType.esriGeometryPolygon)
                //{
                //    IGeometry geo_new = new PolygonClass();
                //}

                IGeometryBag geo_bag = new GeometryBagClass();
                IGeometryCollection pGeoCol = geo_bag as IGeometryCollection;
                IFeatureCursor pFCursor = pFC.Search(null, false);
                IFeature pf = pFCursor.NextFeature();
                geo_new = pf.ShapeCopy;
                object before = Type.Missing;
                object after = Type.Missing;
                while (pf != null)
                {
                    pGeoCol.AddGeometry(pf.ShapeCopy, ref before, ref after);
                    pf = pFCursor.NextFeature();
                }
                
                ITopologicalOperator2 pTopo = geo_new as ITopologicalOperator2;
                pTopo.ConstructUnion(geo_bag as IEnumGeometry);
                pTopo.Simplify();

                IDataset pDataset = (IDataset)pFC_New;
                IWorkspaceEdit pWorkspaceEdit = (IWorkspaceEdit)pDataset.Workspace;
                pWorkspaceEdit.StartEditOperation();
                pWorkspaceEdit.StartEditing(true);

                IFeature pf_new = pFC_New.CreateFeature();
                pf_new.Shape = geo_new;
                pf_new.Store();

                pWorkspaceEdit.StopEditOperation();
                pWorkspaceEdit.StopEditing(true);

                return pFC_New;
            }
            catch (Exception e)
            {
                throw new Exception("合并多要素失败！\n" + e.Message);
            }
        }


        static public IGeometry MergeTogetherToNewGeometry(IFeatureClass pFC)
        {
            try
            {
                IGeometry geo_new;
                //if (geo_type == esriGeometryType.esriGeometryPolygon)
                //{
                //    IGeometry geo_new = new PolygonClass();
                //}

                IGeometryBag geo_bag = new GeometryBagClass();
                IGeometryCollection pGeoCol = geo_bag as IGeometryCollection;
                IFeatureCursor pFCursor = pFC.Search(null, false);
                IFeature pf = pFCursor.NextFeature();
                geo_new = pf.ShapeCopy;
                object before = Type.Missing;
                object after = Type.Missing;
                while (pf != null)
                {
                    pGeoCol.AddGeometry(pf.ShapeCopy, ref before, ref after);
                    pf = pFCursor.NextFeature();
                }

                ITopologicalOperator2 pTopo = geo_new as ITopologicalOperator2;
                pTopo.ConstructUnion(geo_bag as IEnumGeometry);
                pTopo.Simplify();

                return geo_new;
            }
            catch (Exception e)
            {
                throw new Exception("合并多要素失败！\n" + e.Message);
            }
        }
        /// <summary>
        /// 将多要素merge到一起
        /// </summary>
        /// <param name="pFC"></param>
        /// <param name="pFC_New">数据源图层</param>
        /// <param name="geo_type">结果数据目标图层</param>
        /// <returns></returns>    
        static public bool MergeTogetherToNewFeature(string in_features, string out_features)
        {
            try
            {
                IFeatureClass pFC = GetFeatureClass(in_features);

                IFeatureClass pFC_New = CreatFeatureClass(out_features, pFC.ShapeType, (pFC as IGeoDataset).SpatialReference, null, false);
                MergeTogetherToNewFeature(pFC, pFC_New);
                return true;
            }
            catch(Exception e)
            {
                throw e;
            }
        }


        //创建一个featureclass
        /// <summary>
        /// 创建一个featureclass
        /// </summary>
        /// <param name="file_name">新文件名</param>
        /// <param name="geometrytype">类型</param>
        /// <param name="SpatialRef">空间参考, 没有设为null</param>
        /// <param name="fields">属性表，没有设为null</param>
        /// <returns>新生成的FeatureClass</returns>
        static public IFeatureClass CreatFeatureClass(string file_name, esriGeometryType geometrytype, ISpatialReference SpatialRef, IFields fields, bool isZValue)
        {
            try
            {
                try
                {
                    IFeatureClass pFC_New = GetFeatureClass(file_name);
                    (pFC_New as IDataset).Delete();
                }
                catch
                {
                }

                string path = System.IO.Path.GetDirectoryName(file_name);
                string Name = System.IO.Path.GetFileNameWithoutExtension(file_name);
                //新建一个图层
                IWorkspaceFactory pWsf = new ShapefileWorkspaceFactoryClass();
                IFeatureWorkspace pFWS = pWsf.OpenFromFile(path, 0) as IFeatureWorkspace;
                IFeatureClass pOutFeatureClass;
                IFields pFields = new FieldsClass();
                IFieldsEdit pFieldsEdit = (IFieldsEdit)pFields;
                pFieldsEdit.FieldCount_2 = 2;

                IField pField = new FieldClass();
                IFieldEdit pFieldEdit = (IFieldEdit)pField;
                pFieldEdit.AliasName_2 = "FID";
                pFieldEdit.Name_2 = "SE_ROW_ID";
                pFieldEdit.Type_2 = esriFieldType.esriFieldTypeOID;
                pFieldsEdit.set_Field(0, pField);

                pField = new FieldClass();
                pFieldEdit = (IFieldEdit)pField;
                //set up Geometry Definition       
                IGeometryDef geometryDef = new GeometryDefClass();
                IGeometryDefEdit geometryDefEdit = (IGeometryDefEdit)geometryDef;
                geometryDefEdit.AvgNumPoints_2 = 5;
                geometryDefEdit.GeometryType_2 = geometrytype;
                geometryDefEdit.GridCount_2 = 1;
                geometryDefEdit.set_GridSize(0, 0); //Allow ArcGIS to determine valid grid values based on the features.       
                geometryDefEdit.HasM_2 = false;
                geometryDefEdit.HasZ_2 = isZValue;        //note that the spatial ReferenceEquals will be inherited from the feature dataset.

                if (SpatialRef != null)
                {
                    geometryDefEdit.SpatialReference_2 = SpatialRef;
                }
                pFieldEdit.Name_2 = "SHAPE";
                pFieldEdit.Type_2 = esriFieldType.esriFieldTypeGeometry;
                pFieldEdit.GeometryDef_2 = geometryDef;
                pFieldEdit.IsNullable_2 = true;
                pFieldEdit.Required_2 = true;
                pFieldsEdit.set_Field(1, pField);
               
                pOutFeatureClass = pFWS.CreateFeatureClass(Name, pFields, null, null, esriFeatureType.esriFTSimple, "SHAPE", "");

                //插入属性表
                if (fields != null)
                {                   
                    AddFieldToFeatureClass(pOutFeatureClass, fields);              
                }
                //关闭空间
                pFWS = null;
                pWsf = null;
                pFields = null;
                pField = null;
                return pOutFeatureClass;
            }
            catch(Exception e)
            {
                throw new Exception("建新featureclass失败！\n" + e.Message);
            }
        }

        //为featureclass填加字段
        static public IFeatureClass AddFieldToFeatureClass(IFeatureClass pFC, string name, esriFieldType type)
        {
            try
            {
                IDataset pDataset = (IDataset)pFC;
                IWorkspaceEdit pWorkspaceEdit = (IWorkspaceEdit)pDataset.Workspace;
                pWorkspaceEdit.StartEditOperation();
                pWorkspaceEdit.StartEditing(true);
                try
                {
                    IField newField = new FieldClass();
                    IFieldEdit fieldEdit = (IFieldEdit)newField;
                    fieldEdit.Name_2 = name;
                    fieldEdit.Type_2 = type;
                    (pFC as IClass).AddField(newField);
                    fieldEdit = null;

                    pWorkspaceEdit.StopEditOperation();
                    pWorkspaceEdit.StopEditing(true);
                    pWorkspaceEdit = null;
                    pDataset = null;

                    return pFC;
                }
                catch (Exception e)
                {
                    pWorkspaceEdit.StopEditOperation();
                    pWorkspaceEdit.StopEditing(false);
                    pWorkspaceEdit = null;
                    pDataset = null;
                    throw e;
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        static public IFeatureClass AddFieldToFeatureClass(IFeatureClass pFC, IField field)
        {

            try
            {
                pFC.AddField(field);
                return pFC;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        static public IFeatureClass AddFieldToFeatureClass(IFeatureClass pFC, IFields fields)
        {
            try
            {
                //通过IFieldChecker检查字段的合法性，为输出SHP获得字段集合
                IFieldChecker pFieldChecker = new FieldChecker();
                IEnumFieldError pEnumFieldError = null;//用于获取错误字段的集合
                IFields pOutFields;//用于获取正确字段的集合
                pFieldChecker.Validate(fields, out pEnumFieldError, out pOutFields);

                for (int i = 0; i < pOutFields.FieldCount; i++)
                {
                    pFC.AddField(pOutFields.get_Field(i));                   
                }
                pOutFields = null;
                return pFC;              
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        static private IFeatureClass CopyFeaturesToNewFeatureClass(IFeatureClass pFC_New, IFeatureCursor pFCursor)
        {
            try
            {
                 IFeature pf = pFCursor.NextFeature();
                IFeature pf_new;

                IDataset pDataset = (IDataset)pFC_New;
                IWorkspaceEdit pWorkspaceEdit = (IWorkspaceEdit)pDataset.Workspace;
                pWorkspaceEdit.StartEditOperation();
                pWorkspaceEdit.StartEditing(true);

                while (pf != null)
                {
                    pf_new = pFC_New.CreateFeature();
                    pf_new.Shape = pf.Shape;
                    //int idx = -1;
                    //string name;
                    //for (int i = 0; i < pf_new.Fields.FieldCount; i++)
                    //{
                    //    name = pf_new.Fields.get_Field(i).Name;
                    //    idx = pf.Fields.FindField(name);
                    //    if (idx != -1 && name != "FID" && name != "Shape")
                    //    {
                    //        pf_new.set_Value(i, pf.get_Value(idx));
                    //    }                     
                    //}
                    pf_new.Store();
                    pf = pFCursor.NextFeature();
                }

                pWorkspaceEdit.StopEditOperation();
                pWorkspaceEdit.StopEditing(true);
                pWorkspaceEdit = null;
                pDataset = null;
                return pFC_New;
            }
            catch(Exception e)
            {
                throw e;
            }
        }

        //通过文件名得到featureclass
        static public IFeatureClass GetFeatureClass(string file_name)
        {
            try
            {
                string Dir = System.IO.Path.GetDirectoryName(file_name);
                string Name = System.IO.Path.GetFileNameWithoutExtension(file_name);
                IWorkspaceFactory factory = new ShapefileWorkspaceFactoryClass();
                IFeatureWorkspace featureWS = factory.OpenFromFile(Dir, 0) as IFeatureWorkspace;
                return featureWS.OpenFeatureClass(Name + ".shp");
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        static public IFeatureClass CopyFieldValueToOtherField(IFeatureClass pFC, string from_field, string to_field)
        {
            try
            {
                int from_idx = pFC.FindField(from_field);
                int to_idx = pFC.FindField(to_field);
                if (to_idx == -1)
                {
                    esriFieldType type = pFC.Fields.get_Field(from_idx).Type;
                    pFC = AddFieldToFeatureClass(pFC, to_field, type);
                    to_idx = pFC.FindField(to_field);
                }

                IFeatureCursor pfcursor = pFC.Search(null, false);
                IFeature pf = pfcursor.NextFeature();
                while (pf != null)
                {
                    pf.set_Value(to_idx, pf.get_Value(from_idx));
                    pf.Store();
                    pf = pfcursor.NextFeature();
                }

                return pFC;
            }
            catch (Exception e)
            {
                throw new Exception("复制属性出错！\n" + e.Message);
            }
        }

        //寻找拓扑相关的feature
        static public IFeatureCursor SearchGeometry(IFeatureClass pFeatureClass, IGeometry p, esriSpatialRelEnum relenum)
        {
            try
            {
                ISpatialFilter pSpatialFilter = new SpatialFilterClass();
                pSpatialFilter.Geometry = p;
                pSpatialFilter.GeometryField = pFeatureClass.ShapeFieldName;
                pSpatialFilter.SpatialRel = relenum;

                return pFeatureClass.Search(pSpatialFilter, false);
            }
            catch
            {
                throw new Exception("寻找相邻多边形出错！");
            }
        }

        //寻找拓扑相关的feature
        static public ISelectionSet GetSpatialSelectionSet(IFeatureClass pFeatureClass, IGeometry p, esriSpatialRelEnum relenum, string whereclause)
        {
            try
            {
                ISpatialFilter pSpatialFilter = new SpatialFilterClass();
                pSpatialFilter.Geometry = p;
                pSpatialFilter.GeometryField = pFeatureClass.ShapeFieldName;
                pSpatialFilter.SpatialRel = relenum;
                pSpatialFilter.WhereClause = whereclause;
                return pFeatureClass.Select(pSpatialFilter, esriSelectionType.esriSelectionTypeHybrid, esriSelectionOption.esriSelectionOptionNormal, null);
            }
            catch
            {
                throw new Exception("寻找相邻多边形出错！");
            }
        }

        static public ISelectionSet GetSpatialSelectionSet(ISelectionSet pSel, IGeometry p, esriSpatialRelEnum relenum, string whereclause)
        {
            try
            {
                ISpatialFilter pSpatialFilter = new SpatialFilterClass();
                pSpatialFilter.Geometry = p;
                //pSpatialFilter.GeometryField = pSel.ShapeFieldName;
                pSpatialFilter.SpatialRel = relenum;
                pSpatialFilter.WhereClause = whereclause;
                return pSel.Select(pSpatialFilter, esriSelectionType.esriSelectionTypeHybrid, esriSelectionOption.esriSelectionOptionNormal, null);
            }
            catch
            {
                throw new Exception("寻找相邻多边形出错！");
            }
        }


        static public void UnionToOneFeature(IFeature pFeature1, IFeature pFeature2)
        {
            try
            {
                ITopologicalOperator2 topOperator = pFeature1.Shape as ITopologicalOperator2;
                topOperator.IsKnownSimple_2 = false;
                topOperator.Simplify();
                pFeature1.Shape.SnapToSpatialReference();


                ITopologicalOperator2 topOperator2 = pFeature2.Shape as ITopologicalOperator2;
                topOperator2.IsKnownSimple_2 = false;
                topOperator2.Simplify();
                pFeature2.Shape.SnapToSpatialReference();

                pFeature1.Shape = topOperator.Union(pFeature2.Shape);
                topOperator = pFeature1.Shape as ITopologicalOperator2;
                pFeature1.Store();

                //return pFeature1;
            }
            catch
            {
                throw new Exception("合并多边形出错！");
            }
        }

        static public IPolyline UnionToOnePolyline(IPolyline pl1, IPolyline  pl2)
        {
            try
            {
                ITopologicalOperator2 topOperator = pl1 as ITopologicalOperator2;
                topOperator.IsKnownSimple_2 = false;
                topOperator.Simplify();
                pl1.SnapToSpatialReference();

                //pl2.SpatialReference = pl1.SpatialReference;
                ITopologicalOperator2 topOperator2 = pl2 as ITopologicalOperator2;
                topOperator2.IsKnownSimple_2 = false;
                topOperator2.Simplify();
                pl2.SnapToSpatialReference();

                pl1 = topOperator.Union(pl2) as IPolyline;
               // topOperator = pFeature1.Shape as ITopologicalOperator2;
                //pFeature1.Store();

                //return pFeature1;

                return pl1;
            }
            catch (Exception e)
            {
                throw new Exception("合并多边形出错！", e);
            }
        }

        static public List<string> GetSameAtributesFromFeatureClass(string pFC1_Name, string pFC2_Name)
        {
            try
            {
                IFeatureClass pFC1 = GetFeatureClass(pFC1_Name);
                IFeatureClass pFC2 = GetFeatureClass(pFC2_Name);
                return GetSameAtributesFromFeatureClass(pFC1, pFC2);
            }
            catch(Exception e)
            {
                throw e;
            }
        }

        static public List<string> GetAtributesFromFeatureClass(string file_name)
        {
            try
            {
                IFeatureClass pFC = GetFeatureClass(file_name);
                List<string> resultlist = new List<string>();

                IFields fields = pFC.Fields;
                //通过IFieldChecker检查字段的合法性，为输出SHP获得字段集合
                IFieldChecker pFieldChecker = new FieldChecker();
                IEnumFieldError pEnumFieldError = null;//用于获取错误字段的集合
                IFields pOutFields;//用于获取正确字段的集合
                pFieldChecker.Validate(fields, out pEnumFieldError, out pOutFields);

                int count = pOutFields.FieldCount;
                IField pFC1_Field;
                string pFC1_Name;
                for (int i = 0; i < count; i++)
                {
                    pFC1_Field = pOutFields.get_Field(i);
                    pFC1_Name = pFC1_Field.Name;
                    resultlist.Add(pFC1_Name);
                }
                return resultlist;
            }
            catch (Exception e)
            {
                throw new Exception("找到相同属性出错！\n" + e.Message);
            }
        }

        static public List<string> GetSameAtributesFromFeatureClass(IFeatureClass pFC1, IFeatureClass pFC2)
        {
            try
            {
                List<string> resultlist = new List<string>();

                IFields fields = pFC1.Fields;
                //通过IFieldChecker检查字段的合法性，为输出SHP获得字段集合
                IFieldChecker pFieldChecker = new FieldChecker();
                IEnumFieldError pEnumFieldError = null;//用于获取错误字段的集合
                IFields pOutFields;//用于获取正确字段的集合
                pFieldChecker.Validate(fields, out pEnumFieldError, out pOutFields);

                int count = pOutFields.FieldCount;
                IField pFC1_Field;
                string pFC1_Name;
                IField pFC2_Field;
                for (int i = 0; i < count; i++)
                {
                    pFC1_Field = pOutFields.get_Field(i);
                    pFC1_Name = pFC1_Field.Name;

                    int index = pFC2.FindField(pFC1_Name);
                    if (index != -1)
                    {
                        pFC2_Field = pFC2.Fields.get_Field(index);
                        if (pFC2_Field.Type == pFC1_Field.Type)
                        {
                            resultlist.Add(pFC1_Name);
                        }
                    }
                }
                return resultlist;
            }
            catch(Exception e)
            {
                throw new Exception("找到相同属性出错！\n" + e.Message);
            }
        }

        static public List<IPoint> SortPointsInClockwise(List<IPoint> plist)
        {
            plist = Sort_XminTomax(plist);
            return Sort_Sin(plist);
        }

        /// <summary>
        /// 按X坐标值从小到大排列
        /// </summary>
        static private List<IPoint> Sort_XminTomax(List<IPoint> pointlist)
        {
            IComparer<IPoint> pointCompare = new PointCompareSort_T();
            pointlist.Sort(pointCompare);
            return pointlist;
        }

        /// <summary>
        /// 按与水平面夹角大小排序
        /// </summary>
        static private List<IPoint> Sort_Sin(List<IPoint> pointlist)
        {
            IPoint point = pointlist[0];
            List<IPoint> PList = new List<IPoint>(0);
            List<double> temp = new List<double>(0);
            temp.Add(-2);
            PList.Insert(0, point);
            for (int i = 1; i < pointlist.Count; i++)
            {
                double an = Math.Sqrt((pointlist[i].X - point.X) * (pointlist[i].X - point.X) + (pointlist[i].Y - point.Y) * (pointlist[i].Y - point.Y));
                double x = (pointlist[i].Y - point.Y) / an;
                bool insert = false;
                for (int j = 1; j < temp.Count; j++)
                {
                    if (x - temp[j - 1] >= -0.001 && x - temp[j] < -0.001)//x >= temp[j - 1] && x < temp[j])
                    {
                        PList.Insert(j, pointlist[i]);
                        temp.Insert(j, x);
                        insert = true;
                        break;
                    }
                    if (x - temp[j - 1] >= -0.001&& Math.Abs(x - temp[j]) < 0.001)//x >= temp[j - 1] && x == temp[j])
                    {
                        double bn = Math.Sqrt((PList[j].X - point.X) * (PList[j].X - point.X) + (PList[j].Y - point.Y) * (PList[j].Y - point.Y));
                        if (an <= bn)
                        {
                            PList.Insert(j, pointlist[i]);
                            temp.Insert(j, x);
                            insert = true;
                            break;
                        }
                    }
                }
                if (!insert)
                {
                    temp.Add(x);
                    PList.Add(pointlist[i]);
                }
            }

            int index = -1;
            for (int i = temp.Count - 2; i >= 0; i--)
            {
                if (temp[i] - temp[temp.Count - 1] < -0.001)
                {
                    index = i + 1;
                    break;
                }
            }
            if (index > 1 && index < temp.Count - 1)
            {
                List<IPoint> pl = PList.GetRange(index, temp.Count - index);
                pl.Reverse();
                PList.RemoveRange(index, temp.Count - index);
                PList.AddRange(pl);
            }
            return PList;
        }

        /// <summary>
        /// 得到两点间距离
        /// </summary>
        static public double GetDistance(IPoint p1, IPoint p2)
        {
            return Math.Sqrt((p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y));
        }

        /// <summary>
        /// 判断两个点是否在线的两边
        /// </summary>
        /// <param name="segments_TB"></param>
        /// <param name="fPoint"></param>
        /// <param name="tPoint"></param>
        /// <returns></returns>
        static public bool IsTwoPointsOnTwoSides(ISegment segment, IPoint fPoint, IPoint tPoint)
        {
            int f = PointPositionToLine(fPoint, segment);
            int t = PointPositionToLine(tPoint, segment);
            if ((f * t < 0) || (f * t == 0 && (f != 0 || t != 0)))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 求延长线交点
        /// </summary>
        /// <param name="line1"></param>
        /// <param name="line2"></param>
        /// <returns></returns>
        static public IPoint CaculateYCXIntersectPoint(ISegment line1, ISegment line2)
        {
            try
            {
                IPoint IntersectPoint = new PointClass();
                IPoint temp = new PointClass();
                temp.X = line1.FromPoint.X - line2.FromPoint.X;
                temp.Y = line1.FromPoint.Y - line2.FromPoint.Y;
                //求直线的向量
                IPoint Vector1 = Vectorofline(line1);
                IPoint Vector2 = Vectorofline(line2);
                double s, t, l;
                t = Vector2.Y * temp.X - Vector2.X * temp.Y;
                s = Vector2.X * Vector1.Y - Vector2.Y * Vector1.X;
                if (s == 0)
                {
                    return null;
                }
                else
                {
                    l = t / s;
                    IntersectPoint.X = line1.FromPoint.X + Vector1.X * l;
                    IntersectPoint.Y = line1.FromPoint.Y + Vector1.Y * l;


                    return IntersectPoint;
                }
            }
            catch
            {
                throw new Exception("计算延长线交点出错！");
            }
        }




        /// <summary>
        /// 求两条线夹角的cos值
        /// </summary>
        /// <param name="line1"></param>
        /// <param name="line2"></param>
        /// <returns></returns>
        static public double GetAngleBetweenline(ISegment line1, ISegment line2)
        {
            IPoint vector1 = Vectorofline(line1);
            IPoint vector2 = Vectorofline(line2);
            double length1 = line1.Length;
            double length2 = line2.Length;
            double cosangle = (vector1.X * vector2.X + vector1.Y * vector2.Y)
                            / (length1 * length2);
            double angle = Math.Abs(cosangle);
            return angle;

        }



        /// <summary>
        /// 直线的方向向量
        /// </summary>
        /// <param name="line1"></param>
        /// <returns></returns>
        static public IPoint Vectorofline(ISegment line1)
        {
            IPoint Vector = new PointClass(); //用点表示直线的方向向量
            Vector.X = line1.ToPoint.X - line1.FromPoint.X;
            Vector.Y = line1.ToPoint.Y - line1.FromPoint.Y;
            return Vector;
        }



        /// <summary>
        /// 功能：判断点在线的方位关系，左边返回1，右边返回-1, 在线的方向上返回0（不考虑是否真的在线段上）
        /// 利用矢量线乘积方法
        /// </summary>
        /// <param name="point"></param>
        /// <param name="line"></param>
        /// <returns></returns>
        /// 
        static public  int PointPositionToLine(IPoint point, ISegment line)
        {
            IPoint b = new PointClass();
            b.X = line.FromPoint.X - point.X;
            b.Y = line.FromPoint.Y - point.Y;
            IPoint a = new PointClass();
            a.X = point.X - line.ToPoint.X;
            a.Y = point.Y - line.ToPoint.Y;
            double an = Math.Sqrt((point.X - line.ToPoint.X) * (point.X - line.ToPoint.X) +
                (point.Y - line.ToPoint.Y) * (point.Y - line.ToPoint.Y));
            double bn = Math.Sqrt((line.FromPoint.X - point.X) * (line.FromPoint.X - point.X) +
                (line.FromPoint.Y - point.Y) * (line.FromPoint.Y - point.Y));

            double z = (a.X / an) * (b.Y / bn) - (a.Y / an) * (b.X / bn);

            if (z < -0.00000005)
                return -1;
            if (z > 0.00000005)
                return 1;

            return 0;
        }



        /// <summary>
        /// 求点到直线距离
        /// </summary>
        /// <param name="p"></param>
        /// <param name="line"></param>
        /// <returns></returns>
        static public double PointToLineDistance(IPoint p, ISegment line)
        {
            double x1 = line.FromPoint.X;
            double y1 = line.FromPoint.Y;
            double x2 = line.ToPoint.X;
            double y2 = line.ToPoint.Y;
            double d = Math.Abs(((y2 - y1) * p.X - (x2 - x1) * p.Y - x1 * y2 + x2 * y1) / Math.Sqrt((y2 - y1) * (y2 - y1) + (x2 - x1) * (x2 - x1)));
            return d;
        }


        static public double Get2DDistance(IPoint p1, IPoint p2)
        {
            return Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
        }
        /// <summary>
        /// 求外心
        /// </summary>
        /// <param name="tri"></param>
        /// <returns></returns>
        static public IPoint CaculateWaiXin(ITinTriangle pTinTri)
        {
            IPolygon tri = ConvertTinTriangleToPolygon(pTinTri);
            IPoint p = new PointClass();
            IPointCollection pCol = tri as IPointCollection;

            IPoint n1 = pCol.get_Point(0);
            IPoint n2 = pCol.get_Point(1);
            IPoint n3 = pCol.get_Point(2);

            p.X = ((n2.Y - n3.Y) * (n2.X - n1.X) * (n1.X + n2.X) - (n1.Y - n2.Y) * (n3.X - n2.X) * (n2.X + n3.X) + (n3.Y - n1.Y) * (n1.Y - n2.Y) * (n2.Y - n3.Y)) / (2 * ((n2.Y - n3.Y) * (n2.X - n1.X) - (n1.Y - n2.Y) * (n3.X - n2.X)));

            p.Y = ((n2.X - n3.X) * (n2.Y - n1.Y) * (n1.Y + n2.Y) - (n1.X - n2.X) * (n3.Y - n2.Y) * (n2.Y + n3.Y) + (n3.X - n1.X) * (n1.X - n2.X) * (n2.X - n3.X)) / (2 * ((n2.X - n3.X) * (n2.Y - n1.Y) - (n1.X - n2.X) * (n3.Y - n2.Y)));

            p.Z = -9999;

            IZAware za = p as IZAware;
            za.ZAware = true;

            ITopologicalOperator pTopo = p as ITopologicalOperator;
            IGeometry pp = pTopo.Buffer(0.05);
            IPolyline pl = new PolylineClass();
            IPointCollection pl_p = pl as IPointCollection;
            pl_p.AddPointCollection(tri as IPointCollection);

            IRelationalOperator pRel = pp as IRelationalOperator;
            if (pRel.Disjoint(tri) || pRel.Crosses(pl))
            {
                ////如果外心在三角形外或在三角形上，求垂心
                //p = CaculateChuiXin(pTinTri);
                double length = 0;

                ISegmentCollection pSeg = tri as ISegmentCollection;
                ISegment pS = pSeg.get_Segment(0);
                for (int i = 0; i < 3; i++)
                {
                    ISegment pS_temp = pSeg.get_Segment(i);
                    if (pS_temp.Length > length)
                    {
                        pS = pS_temp;
                        length = pS_temp.Length;
                    }
                }

                p.X = (pS.FromPoint.X + pS.ToPoint.X) / 2;
                p.Y = (pS.FromPoint.Y + pS.ToPoint.Y) / 2;
                p.Z = -9999;
            }

            return p;
        }

        /// <summary>
        /// 求垂心（三点坐标平均值）
        /// </summary>
        /// <param name="tri"></param>
        /// <returns></returns>
        static public IPoint CaculateChuiXin(ITinTriangle tri)
        {
            IPoint pp = new PointClass();

            pp.X = (tri.get_Node(0).X + tri.get_Node(1).X + tri.get_Node(2).X) / 3;
            pp.Y = (tri.get_Node(0).Y + tri.get_Node(1).Y + tri.get_Node(2).Y) / 3;
            //pp.Z = (tri.get_Node(0).Z + tri.get_Node(1).Z + tri.get_Node(2).Z) / 3;

            pp.Z = -9999;

            IZAware za = pp as IZAware;
            za.ZAware = true;

            return pp;
        }


        static public IPoint ConvertTinNodeToPoint(ITinNode node)
        {
            IPoint p = new PointClass();
            p.X = node.X;
            p.Y = node.Y;
            p.Z = node.Z;

            IZAware za = p as IZAware;
            za.ZAware = true;

            return p;
        }

        static public IPoint GetCenterPoint(ITinEdge pEdge)
        {
            IPoint p = new PointClass();
            p.X = (pEdge.FromNode.X + pEdge.ToNode.X) / 2;
            p.Y = (pEdge.FromNode.Y + pEdge.ToNode.Y) / 2;
            //p.Z = (pEdge.FromNode.Z + pEdge.ToNode.Z) / 2;
            p.Z = -9999;

            IZAware za = p as IZAware;
            za.ZAware = true;

            return p;
        }

        static public IPoint GetCenterPoint(IPoint p, ITinNode n)
        {
            IPoint pp = new PointClass();
            pp.X = (p.X + n.X) / 2;
            pp.Y = (p.Y + n.Y) / 2;
            //pp.Z = (p.Z + n.Z) / 2;
            p.Z = -9999;

            IZAware za = pp as IZAware;
            za.ZAware = true;

            return pp;
        }

        static public IPoint GetCenterPoint(IPoint p, IPoint n)
        {
            IPoint pp = new PointClass();
            pp.X = (p.X + n.X) / 2;
            pp.Y = (p.Y + n.Y) / 2;
            //pp.Z = (p.Z + n.Z) / 2;
            p.Z = -9999;

            IZAware za = pp as IZAware;
            za.ZAware = true;

            return pp;
        }

        static public IPoint GetCenterPoint(ITinNode p, ITinNode n)
        {
            IPoint pp = new PointClass();
            pp.X = (p.X + n.X) / 2;
            pp.Y = (p.Y + n.Y) / 2;
            //pp.Z = (p.Z + n.Z) / 2;
            pp.Z = -9999;

            IZAware za = pp as IZAware;
            za.ZAware = true;

            return pp;
        }

        static public IPolyline GetPolylineZM(IPoint p1, IPoint p2)
        {
            IPolyline pl_New = new PolylineClass();

            IZAware za = pl_New as IZAware;
            za.ZAware = true;
            pl_New.FromPoint = p1;
            pl_New.ToPoint = p2;

            return pl_New;
        }

        static public IPolygon ConvertTinTriangleToPolygon(ITinTriangle pTinTri)
        {
            IPolygon pl = new PolygonClass();
            object pObj = Type.Missing;
            IPointCollection pPCol = pl as IPointCollection;

            IPoint p = ConvertTinNodeToPoint(pTinTri.get_Node(0));
            pl.FromPoint = p;
            pPCol.AddPoint(ConvertTinNodeToPoint(pTinTri.get_Node(1)), ref pObj, ref pObj);
            pPCol.AddPoint(ConvertTinNodeToPoint(pTinTri.get_Node(2)), ref pObj, ref pObj);
            pPCol.AddPoint(p, ref pObj, ref pObj);

            return pl;
        }

        static public bool IsEqual(IPoint p1, IPoint p2)
        {
            if (Math.Abs(p1.X - p2.X) < 0.005 && Math.Abs(p1.Y - p2.Y) < 0.005 && Math.Abs(p1.Y - p2.Y) < 0.005)
            {
                return true;
            }
            return false;
        }

    }


    public class PointCompareSort_T : IComparer<IPoint>
    {
        public int Compare(IPoint A, IPoint B)
        {
            if (A.X < B.X)  //A小于B
                return -1;
            else if (A.X > B.X)//A大于B
                return 1;
            else if ((A.X == B.X) && (A.Y > B.Y))
                return -1;
            else if ((A.X == B.X) && (A.Y < B.Y))
                return 1;
            else if ((A.X == B.X) && (A.Y == B.Y))  //二者相等
                return 0;
            return 0;
        }
    }
    //按照z值对点进行排序
    public class PointZComapareSort_T : IComparer<IPoint>
    {
        public int Compare(IPoint A, IPoint B)
        {
            if (A.Z < B.Z)
                return -1;
            else if (A.Z > B.Z)
                return 1;
            else if (A.Z == B.Z)
                return 0;
            return 0;
            
        }
    }
    //public class LineCompare_T : IComparer<ILine_T>
    //{
    //    public int Compare(ILine_T A, ILine_T B)
    //    {
    //        if ((A.FromPoint == B.FromPoint && A.ToPoint == B.ToPoint) || (A.FromPoint == B.ToPoint && A.ToPoint == B.FromPoint))
    //            return 0;

    //        return 1;

    //    }
    //}
}
