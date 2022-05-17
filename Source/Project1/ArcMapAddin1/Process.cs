using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geoprocessor;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.DataSourcesRaster;
using ESRI.ArcGIS.DataManagementTools;
namespace ArcMapAddin1
{
    class Process
    {
         
             /// <summary>
        /// 获取图层中的点数据
        /// </summary>
        /// <returns></returns>
        static public HashSet<IPoint> Readfeature(IFeatureClass currentFeaClass)
        {
            //获取当前图层数据       
            try
            {
                HashSet<IPoint> pointSet = new HashSet<IPoint>();
                switch (currentFeaClass.ShapeType)
                {
                    //转换数据，esri point feature class -> HLNPoints 
                    case esriGeometryType.esriGeometryPoint:
                        esriPoint_2_Point(currentFeaClass, pointSet);
                        return pointSet;

                    //case esriGeometryType.esriGeometryPolyline:
                    //    ContourLine_2_Point(currentFeaClass, pointSet);
                    //    return pointSet;

                    default:
                        return null;
                }
            }
            catch (Exception err)
            {
                MessageBox.Show(err.ToString());
                return null;
            }
        }
        static public void esriPoint_2_Point(ESRI.ArcGIS.Geodatabase.IFeatureClass esriPointFeaClass, ICollection<IPoint> pointDataSet)
        {
            if (esriPointFeaClass == null)
                return;

            try
            {
                //获得指针
                IQueryFilter m_queryfilter = new SpatialFilterClass();
                IFeatureCursor m_feaCursor = esriPointFeaClass.Search(m_queryfilter, false);
                IFeature m_currentFea = m_feaCursor.NextFeature();

                //开始转换
                IFields ptFields = m_currentFea.Fields;
                ESRI.ArcGIS.Geometry.IGeometry m_geo;
                ESRI.ArcGIS.Geometry.IPoint m_geoPt;
                //double wei = 0;
                IPoint hlnPt;

                while (m_currentFea != null)
                {
                    //获取shp point
                    // wei = Convert.ToDouble(m_currentFea.get_Value(ptFields.FindField("wei")));
                    m_geo = m_currentFea.Shape;
                    m_geoPt = m_geo as ESRI.ArcGIS.Geometry.IPoint;

                    //赋值hln point
                    hlnPt = m_geoPt;
                    //hlnPt.Character2 = wei;
                    //hlnPt.Character3 = -1;
                    //hlnPt.Character4 = -1;

                    pointDataSet.Add(hlnPt);
                    m_currentFea = m_feaCursor.NextFeature();

                }

                //释放游标
                System.Runtime.InteropServices.Marshal.ReleaseComObject(m_feaCursor);
            }
            catch (Exception err)
            {
                throw err;
            }

        }
        static public  HashSet<IPoint> Generation(ICollection<IPoint> pointDataSet)
        {
            HashSet<IPoint> resultpointSet = new HashSet<IPoint>();
            for (int i = 0; i < pointDataSet.Count - 1; i++)
            {
                if(i%2==0)
                    resultpointSet.Add(pointDataSet.ElementAt(i));
            }
                return resultpointSet; 
        }
        //static public void Writefeature(ICollection<IPoint> pointDataSet)
        //{
           
        //}
        /// <summary>
        /// 创建数据层
        /// </summary>
        /// <returns></returns>
        static public IFeatureLayer CreateGenPointFeatureLayer(IFeatureLayer referenceFeaLayer)
        {
            try
            {
                Geoprocessor gp = new Geoprocessor();
                gp.AddOutputsToMap = false;

                //获取当前选中图层数据
                IFeatureClass referenceFeaClass = referenceFeaLayer.FeatureClass;
                IDataset referenceDataset = referenceFeaClass as IDataset;
                IWorkspace referenceWorkspace = referenceDataset.Workspace;

                IWorkspace outputWS = OpenFileGdbScratchWorkspace();

                string currentPath = outputWS.PathName;
                string tempFileName = System.IO.Path.GetRandomFileName();
                string currentName = 'a' + tempFileName.Split('.')[0];//"gen_" + referenceFeaLayer.Name;// +".shp";
                string currentFullName = currentPath + "\\" + currentName;

                //如果原来存在文件，则清除文件
                //CleanShapeFile(currentFullName);

                //获取空间参考
                IGeoDataset referenceGeoDataset = referenceFeaClass as IGeoDataset;

                //设置临时层属性
                CreateFeatureclass createCurrentFeaClass = new CreateFeatureclass();
                createCurrentFeaClass.out_path = currentPath;
                createCurrentFeaClass.out_name = currentName;
                createCurrentFeaClass.spatial_reference = referenceGeoDataset.SpatialReference;
                createCurrentFeaClass.geometry_type = "POINT";
                //创建临时层
               // GPHelper.RunTool(gp, createCurrentFeaClass, null);
                gp.Execute(createCurrentFeaClass, null);

                #region 添加字段
                AddField p_addField = new AddField();
                p_addField.in_table = currentFullName;
                p_addField.field_name = "X";
                p_addField.field_type = "Double";
                //GPHelper.RunTool(gp, p_addField, null);
                gp.Execute(p_addField, null);

                p_addField = new AddField();
                p_addField.in_table = currentFullName;
                p_addField.field_name = "Y";
                p_addField.field_type = "Double";
                gp.Execute(p_addField, null);
                //GPHelper.RunTool(gp, p_addField, null);

                p_addField = new AddField();
                p_addField.in_table = currentFullName;
                p_addField.field_name = "Z";
                p_addField.field_type = "Double";
                gp.Execute(p_addField, null);
                //GPHelper.RunTool(gp, p_addField, null);
                #endregion

                //打开层
                IFeatureWorkspace feaWorkspac = outputWS as IFeatureWorkspace;
                IFeatureClass currentFeaClass = feaWorkspac.OpenFeatureClass(currentName);
                IFeatureLayer currentFeaLayer = new FeatureLayerClass();
                currentFeaLayer.FeatureClass = currentFeaClass;
                currentFeaLayer.Name = currentFeaClass.AliasName;
                return currentFeaLayer;
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
                return null;
            }
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
        /// <summary>
        /// 显示点数据
        /// </summary>
        /// <param name="shownPoints"></param>
        static public void ShowPointSet(IFeatureLayer tmpFeaLayer, ICollection<IPoint> shownPoints, IActiveView activeView)
        {
            IFeatureClass tmpFeaClass = tmpFeaLayer.FeatureClass;
            Point_2_esriPoint(shownPoints, ref tmpFeaClass);

            //添加新图层
            activeView.FocusMap.AddLayer((ILayer)tmpFeaLayer);
            activeView.PartialRefresh(esriViewDrawPhase.esriViewGeography, null, null);
        }
        /// <summary>
        /// 将生成的结果转换成shp文件，hln点-->shp点，esriPointFeaClass应该是已存在的，结构表包含完整x，y，z的，空的shp文件（可有wei字段）
        /// </summary>
        /// <param name="HLNPointDataSet"></param>
        /// <param name="esriPointFeaClass"></param>
        static public void Point_2_esriPoint(ICollection<IPoint> pointDataSet, ref ESRI.ArcGIS.Geodatabase.IFeatureClass esriPointFeaClass)
        {

            //开启编辑状态
            IDataset dataset = esriPointFeaClass as IDataset;
            IWorkspace workspace = dataset.Workspace;
            IWorkspaceEdit workspaceEdit = workspace as IWorkspaceEdit;
            workspaceEdit.StartEditing(true);
            workspaceEdit.StartEditOperation();

            IFeatureBuffer feaBuffer;
            IFeatureCursor feaCursor = null;

            int releaseNo = 100;
            try
            {
                IFields feaClassFields = esriPointFeaClass.Fields;
                int xIndex = feaClassFields.FindField("X");
                int yIndex = feaClassFields.FindField("Y");
                int zIndex = feaClassFields.FindField("Z");
                int fieldIndex = feaClassFields.FindField("wei");

                //创建buffer
                feaBuffer = esriPointFeaClass.CreateFeatureBuffer();
                esriPointFeaClass.Insert(false);
                feaCursor = esriPointFeaClass.Insert(true);

                ESRI.ArcGIS.Geometry.IPoint newPt;
                foreach (IPoint demPt in pointDataSet)
                {
                    newPt = new ESRI.ArcGIS.Geometry.PointClass();
                    newPt.X = demPt.X;
                    newPt.Y = demPt.Y;
                    newPt.Z = demPt.Z;

                    feaBuffer.Shape = newPt;
                    feaBuffer.set_Value(xIndex, demPt.X);
                    feaBuffer.set_Value(yIndex, demPt.Y);
                    feaBuffer.set_Value(zIndex, demPt.Z);
                    //  feaBuffer.set_Value(fieldIndex, demPt.Character2);
                    feaCursor.InsertFeature(feaBuffer);
                }

                feaCursor.Flush();
                //esriPointFeaClass.Insert(false);  

                //释放指针
                releaseNo = System.Runtime.InteropServices.Marshal.ReleaseComObject(feaCursor);
                workspaceEdit.StopEditOperation();
                workspaceEdit.StopEditing(true);
            }
            catch (Exception err)
            {
                workspaceEdit.StopEditOperation();
                workspaceEdit.StopEditing(false);
                throw err;
            }
            finally
            {
                //释放指针，确保释放完全
                while (releaseNo > 0)
                {
                    releaseNo = System.Runtime.InteropServices.Marshal.ReleaseComObject(feaCursor);
                }
                //if (feaCursor != null)
                //    System.Runtime.InteropServices.Marshal.ReleaseComObject(feaCursor);

                //workspaceEdit.StopEditOperation();
                //workspaceEdit.StopEditing(true);               
            }
        }
    }
}
