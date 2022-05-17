using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using System.Windows.Forms;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.DataSourcesFile;
using ESRI.ArcGIS.Geodatabase;
using GetFeaturePoint.WinForm;


namespace GetFeaturePointNew
{
    public class Smooth : ESRI.ArcGIS.Desktop.AddIns.Button
    {
        public Smooth()
        {
        }

        protected override void OnClick()
        {
            try
            {
                IFeatureLayer layer = null;
                for (int i = 0; i < ArcMap.Document.FocusMap.LayerCount; i++)
                {
                    layer = ArcMap.Document.FocusMap.get_Layer(i) as IFeatureLayer;
                    if (layer != null && layer.Visible && layer.FeatureClass.ShapeType == ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPolyline)
                        break;
                }

                if (layer == null)
                {
                    MessageBox.Show("没有找到处于显示状态的线矢量图层！");
                    return;
                }

                SmoothWinform frm = new SmoothWinform(ArcMap.Document, layer.FeatureClass, ArcMap.Document.FocusMap);
                frm.ShowDialog();
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }

            ArcMap.Application.CurrentTool = null;
        }

        protected override void OnUpdate()
        {
            Enabled = ArcMap.Application != null;
        }
    }
}
