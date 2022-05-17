using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using MapFeaturePoint.WinForm;
using System.Windows.Forms;

using ESRI.ArcGIS.Carto;

using FeaturePointDataModel;


namespace MapFeaturePoint
{
    public class UnifyDirectionButton : ESRI.ArcGIS.Desktop.AddIns.Button
    {
        public UnifyDirectionButton()
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
                UnifyDirection_Form getFpFrm = new UnifyDirection_Form(layer.FeatureClass);
                getFpFrm.ShowDialog();
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
        }

        protected override void OnUpdate()
        {
        }
    }
}
