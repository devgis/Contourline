using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Forms;

using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;

namespace MapFeaturePoint
{
    public class ReverseButton : ESRI.ArcGIS.Desktop.AddIns.Button
    {
        public ReverseButton()
        {
        }

        protected override void OnUpdate()
        {

        }

        protected override void OnClick()
        {
            try
            {
                IMap map = ArcMap.Document.FocusMap;
                ISelection fs = map.FeatureSelection;
                IEnumFeature enFeature = (IEnumFeature)fs;
                enFeature.Reset();
                IFeature newFeature_rest = enFeature.Next();
                if (newFeature_rest == null)
                {
                    MessageBox.Show("请选择一个元素");
                    return;
                }
                while (newFeature_rest != null)
                {
                    IPolyline pl = newFeature_rest.Shape as IPolyline;
                    pl.ReverseOrientation();
                    newFeature_rest.Store();
                    newFeature_rest = enFeature.Next();
                }
                IActiveView pActiveView = (IActiveView)map;
                pActiveView.Refresh();
                MessageBox.Show("Ok");
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
        }
    }

}
