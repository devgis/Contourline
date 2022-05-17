using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using System.Windows.Forms;
using BusinessLayer;

namespace ArcMapAddin1
{
    public class Button1 : ESRI.ArcGIS.Desktop.AddIns.Button
    {
        public Button1()
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

                //IFeatureClass feaCla = layer.FeatureClass;
                //ITable table = feaCla as ITable;
                //IFeatureCursor pfcurse = feaCla.Search(null, false);
                //IFeature pfea = pfcurse.NextFeature();
                //while (pfea != null)
                //{
                //    IPolyline pl = pfea.Shape as IPolyline;
                //    IPointCollection pc = pl as IPointCollection;
                //    IRow row = table.GetRow(pfea.OID);
                //    object clock = row.get_Value(pfea.Fields.FindField("clock"));
                //    if (Convert.ToInt16(clock) == 2)
                //    {
                //        if (CalculateMaxCurvity.isClockWise(pc))
                //        {
                //            pl.ReverseOrientation();
                //        }
                //    }
                //    else if (Convert.ToInt16(clock) == 0)
                //    {
                //        pl.ReverseOrientation();
                //    }

                //    pfea.Store();
                //    pfea = pfcurse.NextFeature();
                //}


                MainForm frm = new MainForm(ArcMap.Document, layer.FeatureClass);
                frm.ShowDialog();
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }

            //ArcMap.Application.CurrentTool = null;
        }
        protected override void OnUpdate()
        {
            Enabled = ArcMap.Application != null;
        }
    }

}
