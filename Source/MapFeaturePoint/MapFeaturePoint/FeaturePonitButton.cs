using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using ESRI.ArcGIS.Carto;
using MapFeaturePoint.WinForm;
using System.Windows.Forms;


namespace MapFeaturePoint
{
    public class FeaturePonitButton : ESRI.ArcGIS.Desktop.AddIns.Button
    {
        public FeaturePonitButton()
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
                GetFeaturePoint_Form getFpFrm = new GetFeaturePoint_Form(layer.FeatureClass, ArcMap.Document.FocusMap.SpatialReference, ArcMap.Document);
                getFpFrm.ShowDialog();
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
        }

        protected override void OnUpdate()
        {
            //Enabled = ArcMap.Application != null;
        }
    }

}
