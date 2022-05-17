using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using ESRI.ArcGIS.Carto;
using FeaturePointDataModel;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.DataSourcesFile;
using System.IO;
using ESRI.ArcGIS.ArcMapUI;
using System.Diagnostics;

namespace MapFeaturePoint.WinForm
{
    public partial class GetFeaturePoint_Form : Form
    {
        IFeatureClass fc;
        ISpatialReference spatialRef;
        private IMxDocument document;
        public GetFeaturePoint_Form(IFeatureClass cfeaCla, ISpatialReference csparef, IMxDocument cdocument)
        {
            InitializeComponent();
            fc = cfeaCla;
            spatialRef = csparef;
            document = cdocument;
        }
        private void ok_button_Click(object sender, EventArgs e)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            FeaturePoint feaPoint = new FeaturePoint(fc, spatialRef, document);
            feaPoint.getPtShp(Convert.ToInt16(yuzhi_textBox.Text));//生成特征点
            this.Close();
            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            // Format and display the TimeSpan value.
            string message = String.Format("任务完成，总运行时间为：{0:00}:{1:00}:{2:00}", ts.Hours, ts.Minutes, ts.Seconds);
            MessageBox.Show(message);
        }

        private void GetFeaturePoint_Form_Load(object sender, EventArgs e)
        {
            //FieldIndex();
        }

        private void cancel_button_Click(object sender, EventArgs e)
        {
            this.Close();
        }

       

    }
}
