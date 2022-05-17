using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using BusinessLayer;
using Utility;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Geometry;

namespace GetFeaturePoint.WinForm
{
    public partial class SmoothWinform : Form
    {
        IFeatureClass _fc;
        IMap _map;
        private IMxDocument _document;
        public SmoothWinform(IMxDocument iMxDocument, IFeatureClass fc,IMap map)
        {
            InitializeComponent();
            _fc = fc;
            _document = iMxDocument;
            input_textBox.Text = _fc.AliasName;
            _map = map;
            field_comboBox.DataSource = GeoDBHelper.GetFieldsName(fc.Fields, new esriFieldType[] { esriFieldType.esriFieldTypeDouble, esriFieldType.esriFieldTypeInteger, esriFieldType.esriFieldTypeSingle }, false);
            
        }

        private void ok_button_Click(object sender, EventArgs e)
        {
            string message = string.Empty;
            IDataset dataset = _fc as IDataset;
            IWorkspace wrk = dataset.Workspace;
            //IWorkspace wk=GeoDBHelper.OpenInMemoryWorkspace();
            CalculateSmooth cfl = new CalculateSmooth(_document, _fc, field_comboBox.SelectedItem.ToString());
            message = cfl.ConstructFeatureLine(Convert.ToInt32(footstep_numericUpDown.Value));
            MessageBox.Show(message);

        }

        private void open_button_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "Shape Files|*.shp";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                // Assign the cursor in the Stream to the Form's Cursor property.
                input_textBox.Text = openFileDialog1.FileName;
            }
        }

        private void save_button_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                save_textBox.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void field_comboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            //IFields fds = _fc.Fields;
            //IField fd;
            //for (int i = 0; i < fds.FieldCount; i++)
            //{
            //    fd = fds.Field[i];
            //    field_comboBox.Items.Add(fd.Name);
            //}
        }

        private void footstep_numericUpDown_ValueChanged(object sender, EventArgs e)
        {
            footstep_numericUpDown.DecimalPlaces = 1;
            //footstep_numericUpDown.Maximum = 2;
            //footstep_numericUpDown.Minimum = 0.1m;
            //footstep_numericUpDown.Increment = 0.1m;
        }
    }
}
