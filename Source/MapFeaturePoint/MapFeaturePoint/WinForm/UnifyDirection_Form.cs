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
    public partial class UnifyDirection_Form : Form
    {
        IFeatureClass fc;
        public UnifyDirection_Form(IFeatureClass cfeaCla)
        {
            InitializeComponent();
            fc = cfeaCla;
        }

        private void ok_button_Click(object sender, EventArgs e)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            UnifyDirection unfityDir = new UnifyDirection(fc);
            if (leftRight_radioButton.Checked)
            {
                unfityDir.reverseLeftRight(unfityDir.getClosed(), true);
            }
            else if (upperLow_radioButton.Checked)
            {
                unfityDir.reverseUpDown(unfityDir.getClosed(), true);
            }
            else
            {
                unfityDir.reverseBorderLeftRight(unfityDir.isNotLook(), true);
            }
            this.Close();
            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            // Format and display the TimeSpan value.
            string message = String.Format("任务完成，总运行时间为：{0:00}:{1:00}:{2:00}", ts.Hours, ts.Minutes, ts.Seconds);
            MessageBox.Show(message);
        }

        private void upperLow_radioButton_CheckedChanged(object sender, EventArgs e)
        {
            //if (upperLow_radioButton.Checked == true)
            //{
            //    leftRight = false;
            //}
            //else
            //{
            //    leftRight = true;
            //}
        }

        private void leftRight_radioButton_CheckedChanged(object sender, EventArgs e)
        {
            //if (leftRight_radioButton.Checked == true)
            //{
            //    leftRight = true;
            //}
            //else
            //{
            //    leftRight = false;
            //}
        }

        private void UnifyDirection_Form_Load(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = System.DateTime.Now.ToString();
            toolStripStatusLabel2.Text = Convert.ToString(0);
            toolStripProgressBar1.Minimum = 0;
            toolStripProgressBar1.Value = 0;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            toolStripProgressBar1.Maximum = MapHelp.maxValue;
            toolStripStatusLabel1.Text = System.DateTime.Now.ToString();
            toolStripProgressBar1.Value = MapHelp.value;
            toolStripStatusLabel2.Text = MapHelp.value.ToString();

            //ToolStripProgressBar
        }
    }
}
