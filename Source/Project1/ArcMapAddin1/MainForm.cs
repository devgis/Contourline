using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Utility;
using ESRI.ArcGIS.Carto;
using BusinessLayer;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.ArcMapUI;

namespace ArcMapAddin1
{
    public partial class MainForm : Form
    {
        IFeatureClass _fc;
        private IMxDocument _document;
        
        public MainForm(IMxDocument iMxDocument, IFeatureClass fc)
        {
            // TODO: Complete member initialization
            // TODO: Complete member initialization

            InitializeComponent();

            _fc = fc;
            _document = iMxDocument;
            textBox1.Text = _fc.AliasName;

            cbElevationField.DataSource = GeoDBHelper.GetFieldsName(fc.Fields, new esriFieldType[] { esriFieldType.esriFieldTypeDouble, esriFieldType.esriFieldTypeInteger, esriFieldType.esriFieldTypeSingle }, false);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                string message = string.Empty;
               //按曲率综合
                        CalculateMaxCurvity cfl = new CalculateMaxCurvity(_document, _fc, textBox2.Text, cbElevationField.SelectedItem.ToString(),
                                                                        int.Parse(tbSpan.Text), int.Parse(tbDistance.Text), int.Parse(tbAngleLimit.Text));
                        message = cfl.ConstructFeatureLine();

                
               
                MessageBox.Show(message);
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }

        }


        //取消计算操作
        private void button2_Click(object sender, EventArgs e)
        {
            backgroundWorker1.CancelAsync();
            bnCancel.Enabled = false;
        }

        private void bnOpen_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "Shape Files|*.shp";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                // Assign the cursor in the Stream to the Form's Cursor property.
                textBox1.Text = openFileDialog1.FileName;
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            //FeatureLineFactory flf = new FeatureLineFactory(ArcMap.Document, textBox1.Text, cbElevationField.SelectedText);//backgroundWorker1
            //flf.BackWorker = backgroundWorker1;
            //flf.BackWorkerEventArgs = e;
            //e.Result = flf.CreateLevelTin(textBox2.Text);

        }

        float _totalWorkCount;//当前操作工作总数，如共需处理多少根等线
        private IMxDocument iMxDocument;
        private IFeatureClass iFeatureClass;
        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.UserState != null)
            {
                toolStripStatusLabel1.Text = e.UserState.ToString();
                _totalWorkCount = e.ProgressPercentage;
                toolStripProgressBar1.Value = 0;
            }
            else
                toolStripProgressBar1.Value = (int)((float)e.ProgressPercentage / _totalWorkCount * 100);
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            bnCancel.Enabled = false;

            if (e.Error != null)
                MessageBox.Show(e.Error.Message);
            else if (e.Cancelled)
                toolStripStatusLabel1.Text = "操作取消";
            else
            {
                toolStripStatusLabel1.Text = string.Empty;
                toolStripProgressBar1.Value = 0;
                MessageBox.Show(e.Result.ToString());
            }
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                textBox2.Text = folderBrowserDialog1.SelectedPath;
            }
        }
    }
}
