using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;

using Utility;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geoprocessor;
using ESRI.ArcGIS.DataManagementTools;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geoprocessing;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.DataSourcesFile;

namespace Utility
{
    public class GeoDBHelper
    {
        //创建临时工作空间，用于存放临时数据
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

        //创建临时工作空间，用于存放临时数据
        public static IWorkspace OpenFileGdbScratchWorkspace()
        {
            // Create a file scratch workspace factory.
            Type factoryType = Type.GetTypeFromProgID(
                "esriDataSourcesGDB.FileGDBScratchWorkspaceFactory");
            IScratchWorkspaceFactory scratchWorkspaceFactory = (IScratchWorkspaceFactory)
                Activator.CreateInstance(factoryType);

            // Get the default scratch workspace.
            IWorkspace scratchWorkspace = scratchWorkspaceFactory.DefaultScratchWorkspace;
            return scratchWorkspace;
        }

        public static IFeatureClass OpenShpFC(string path)
        {
            IWorkspaceFactory workspaceFactory = new ShapefileWorkspaceFactoryClass();

            // System.IO.Path.GetDirectoryName(shapefileLocation) returns the directory part of the string. Example: "C:\test\"
            IFeatureWorkspace featureWorkspace = (IFeatureWorkspace)workspaceFactory.OpenFromFile(System.IO.Path.GetDirectoryName(path), 0); // Explicit Cast

            // System.IO.Path.GetFileNameWithoutExtension(shapefileLocation) returns the base filename (without extension). Example: "cities"
            return featureWorkspace.OpenFeatureClass(System.IO.Path.GetFileNameWithoutExtension(path));

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
        static public IFeatureClass CreatFeatureClass(IFeatureWorkspace pFWS, string file_name, esriGeometryType geometrytype, ISpatialReference SpatialRef, IFields fields, bool isZValue)
        {
            try
            {
                //string path = System.IO.Path.GetDirectoryName(file_name);
                //string Name = System.IO.Path.GetFileNameWithoutExtension(file_name);
                //新建一个图层
                //IWorkspaceFactory pWsf = new ShapefileWorkspaceFactoryClass();
                //IFeatureWorkspace pFWS = pWsf.OpenFromFile(path, 0) as IFeatureWorkspace;
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

                pOutFeatureClass = pFWS.CreateFeatureClass(file_name, pFields, null, null, esriFeatureType.esriFTSimple, "SHAPE", "");

                //插入属性表
                if (fields != null)
                {
                    AddFieldToFeatureClass(pOutFeatureClass, fields);
                }
                //关闭空间
                //pFWS = null;
                //pWsf = null;
                //pFields = null;
                //pField = null;
                return pOutFeatureClass;
            }
            catch (Exception e)
            {
                throw new Exception("建新featureclass失败！\n" + e.Message, e);
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

        /// <summary>
        /// 删除图层中的要素
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="gp"></param>
        static public void DeleteFeature(IFeatureLayer layer, Geoprocessor gp)
        {
            ISelectionSet TempSelectionset1 = null;
            IFeatureSelection FeaSelection = (IFeatureSelection)layer;
            TempSelectionset1 = FeaSelection.SelectionSet;

            if (TempSelectionset1.Count > 0)
            {
                string in_feature = layer.Name;
                ESRI.ArcGIS.DataManagementTools.DeleteFeatures del = new ESRI.ArcGIS.DataManagementTools.DeleteFeatures();
                del.in_features = (object)in_feature;
                GPHelper.RunTool(gp, (IGPProcess)del, null);
            }
        }

        //通过文件名得到featureclass
        //static public IFeatureClass GetFeatureClass(string file_name, IFeatureWorkspace ws)
        //{
        //    try
        //    {
        //        //string Dir = System.IO.Path.GetDirectoryName(file_name);
        //        //string Name = System.IO.Path.GetFileNameWithoutExtension(file_name);
        //        //IWorkspaceFactory factory = new ShapefileWorkspaceFactoryClass();
        //        //IFeatureWorkspace featureWS = factory.OpenFromFile(Dir, 0) as IFeatureWorkspace;
        //        return ws.OpenFeatureClass(file_name);
        //    }
        //    catch (Exception e)
        //    {
        //        throw e;
        //    }
        //}

        /// <summary>
        /// 如果原来存在文件，则删除该文件
        /// </summary>
        /// <param name="existShapeFile"></param>
        static public void CleanShapeFile(string existShapeFile)
        {
            try
            {
                string filePath = System.IO.Path.GetDirectoryName(existShapeFile);
                string fileName = System.IO.Path.GetFileNameWithoutExtension(existShapeFile);

                if (System.IO.File.Exists(existShapeFile))
                {
                    System.IO.File.Delete(filePath + "\\" + fileName + ".shp");
                    System.IO.File.Delete(filePath + "\\" + fileName + ".shx");
                    System.IO.File.Delete(filePath + "\\" + fileName + ".dbf");
                    System.IO.File.Delete(filePath + "\\" + fileName + ".shp.xml");
                }
            }
            catch (Exception err)
            {
                throw err;
            }
        }

        static public IEnumerable<string> GetFieldsName(IFields flds, esriFieldType[] types, bool withID)
        {
            List<string> names = new List<string>();

            for (int i = 0; i < flds.FieldCount; ++i)
            {
                IField fld = flds.get_Field(i);
                foreach (esriFieldType type in types)
                {
                    if (fld.Type == type)
                    {
                        if (!withID && fld.Name.Contains("ID"))
                            continue;
                        names.Add(fld.Name);
                        break;
                    }
                }
            }
            return names;
        }

        /// <summary>
        /// 添加要素
        /// </summary>
        /// <param name="geo"></param>
        /// <param name="file_name"></param>
        /// <returns></returns>
        static public void AddFeature2FC(IFeatureClass fc, IGeometry geo, IFeature ft)
        {
            try
            {
                IFeatureBuffer pFBuffer = fc.CreateFeatureBuffer();
                IFeatureCursor pFCursor = fc.Insert(true);

                if (ft != null)
                {
                    for (int i = 0; i < ft.Fields.FieldCount; i++)
                    {
                        if (ft.Fields.Field[i].Type != esriFieldType.esriFieldTypeOID && ft.Fields.Field[i].Type != esriFieldType.esriFieldTypeGeometry)
                            pFBuffer.set_Value(i, ft.get_Value(i));
                    }
                }

                if (!geo.IsEmpty)
                {
                    pFBuffer.Shape = geo;
                    pFCursor.InsertFeature(pFBuffer);
                    pFCursor.Flush();
                }
            }
            catch (Exception e)
            {
                throw new Exception("存入要素出错！\n" + e.Message);
            }
        }

        /// <summary>
        /// 添加要素
        /// </summary>
        /// <param name="geo"></param>
        /// <param name="file_name"></param>
        /// <returns></returns>
        static public void AddFeature2FC(IFeatureClass fc, IGeometry geo, int[] index, object[] value)
        {
            try
            {
                IFeatureBuffer pFBuffer = fc.CreateFeatureBuffer();
                IFeatureCursor pFCursor = fc.Insert(true);

                if (index != null && index.Length > 0)
                {
                    for (int i = 0; i < index.Length; i++)
                    {
                        pFBuffer.set_Value(index[i], value[i]);
                    }
                }

                if (!geo.IsEmpty)
                {
                    pFBuffer.Shape = geo;
                    pFCursor.InsertFeature(pFBuffer);
                    pFCursor.Flush();
                }
            }
            catch (Exception e)
            {
                throw new Exception("存入要素出错！\n" + e.Message);
            }
        }

        /// <summary>
        /// 取得要素属性值
        /// </summary>
        static public object GetValueOfFeature(IFeature feature, string fieldName)
        {
            int index = feature.Fields.FindField(fieldName);
            return feature.get_Value(index);
        }
    }
}
