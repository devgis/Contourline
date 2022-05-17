namespace MapFeaturePoint.WinForm
{
    partial class GetFeaturePoint_Form
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.input_label = new System.Windows.Forms.Label();
            this.save_label = new System.Windows.Forms.Label();
            this.intput_textBox = new System.Windows.Forms.TextBox();
            this.save_textBox = new System.Windows.Forms.TextBox();
            this.input_button = new System.Windows.Forms.Button();
            this.save_button = new System.Windows.Forms.Button();
            this.bottom_statusStrip = new System.Windows.Forms.StatusStrip();
            this.toolStripProgressBar1 = new System.Windows.Forms.ToolStripProgressBar();
            this.ok_button = new System.Windows.Forms.Button();
            this.cancel_button = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.yuzhi_label = new System.Windows.Forms.Label();
            this.yuzhi_textBox = new System.Windows.Forms.TextBox();
            this.bottom_statusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // input_label
            // 
            this.input_label.AutoSize = true;
            this.input_label.Location = new System.Drawing.Point(31, 28);
            this.input_label.Name = "input_label";
            this.input_label.Size = new System.Drawing.Size(65, 12);
            this.input_label.TabIndex = 0;
            this.input_label.Text = "输入路径：";
            // 
            // save_label
            // 
            this.save_label.AutoSize = true;
            this.save_label.Location = new System.Drawing.Point(31, 60);
            this.save_label.Name = "save_label";
            this.save_label.Size = new System.Drawing.Size(65, 12);
            this.save_label.TabIndex = 1;
            this.save_label.Text = "临时目录：";
            // 
            // intput_textBox
            // 
            this.intput_textBox.Enabled = false;
            this.intput_textBox.Location = new System.Drawing.Point(100, 25);
            this.intput_textBox.Name = "intput_textBox";
            this.intput_textBox.Size = new System.Drawing.Size(268, 21);
            this.intput_textBox.TabIndex = 2;
            this.intput_textBox.Text = "E:\\temp\\terlk3ddsms.shp";
            // 
            // save_textBox
            // 
            this.save_textBox.Enabled = false;
            this.save_textBox.Location = new System.Drawing.Point(100, 57);
            this.save_textBox.Name = "save_textBox";
            this.save_textBox.Size = new System.Drawing.Size(268, 21);
            this.save_textBox.TabIndex = 3;
            this.save_textBox.Text = "D:\\_temp";
            // 
            // input_button
            // 
            this.input_button.Enabled = false;
            this.input_button.Location = new System.Drawing.Point(374, 23);
            this.input_button.Name = "input_button";
            this.input_button.Size = new System.Drawing.Size(75, 23);
            this.input_button.TabIndex = 15;
            this.input_button.Text = "打开";
            // 
            // save_button
            // 
            this.save_button.Enabled = false;
            this.save_button.Location = new System.Drawing.Point(374, 57);
            this.save_button.Name = "save_button";
            this.save_button.Size = new System.Drawing.Size(75, 23);
            this.save_button.TabIndex = 5;
            this.save_button.Text = "保存";
            this.save_button.UseVisualStyleBackColor = true;
            // 
            // bottom_statusStrip
            // 
            this.bottom_statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripProgressBar1});
            this.bottom_statusStrip.Location = new System.Drawing.Point(0, 283);
            this.bottom_statusStrip.Name = "bottom_statusStrip";
            this.bottom_statusStrip.Size = new System.Drawing.Size(473, 22);
            this.bottom_statusStrip.TabIndex = 6;
            this.bottom_statusStrip.Text = "进程";
            // 
            // toolStripProgressBar1
            // 
            this.toolStripProgressBar1.Name = "toolStripProgressBar1";
            this.toolStripProgressBar1.Size = new System.Drawing.Size(100, 16);
            // 
            // ok_button
            // 
            this.ok_button.Location = new System.Drawing.Point(120, 233);
            this.ok_button.Name = "ok_button";
            this.ok_button.Size = new System.Drawing.Size(75, 23);
            this.ok_button.TabIndex = 9;
            this.ok_button.Text = "确定";
            this.ok_button.UseVisualStyleBackColor = true;
            this.ok_button.Click += new System.EventHandler(this.ok_button_Click);
            // 
            // cancel_button
            // 
            this.cancel_button.Location = new System.Drawing.Point(271, 232);
            this.cancel_button.Name = "cancel_button";
            this.cancel_button.Size = new System.Drawing.Size(75, 23);
            this.cancel_button.TabIndex = 10;
            this.cancel_button.Text = "取消";
            this.cancel_button.UseVisualStyleBackColor = true;
            this.cancel_button.Click += new System.EventHandler(this.cancel_button_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // yuzhi_label
            // 
            this.yuzhi_label.AutoSize = true;
            this.yuzhi_label.Location = new System.Drawing.Point(31, 121);
            this.yuzhi_label.Name = "yuzhi_label";
            this.yuzhi_label.Size = new System.Drawing.Size(101, 12);
            this.yuzhi_label.TabIndex = 16;
            this.yuzhi_label.Text = "转角阈值（度）：";
            // 
            // yuzhi_textBox
            // 
            this.yuzhi_textBox.Location = new System.Drawing.Point(138, 118);
            this.yuzhi_textBox.Name = "yuzhi_textBox";
            this.yuzhi_textBox.Size = new System.Drawing.Size(72, 21);
            this.yuzhi_textBox.TabIndex = 17;
            this.yuzhi_textBox.Text = "14";
            // 
            // GetFeaturePoint_Form
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(473, 305);
            this.Controls.Add(this.yuzhi_textBox);
            this.Controls.Add(this.yuzhi_label);
            this.Controls.Add(this.cancel_button);
            this.Controls.Add(this.ok_button);
            this.Controls.Add(this.bottom_statusStrip);
            this.Controls.Add(this.save_button);
            this.Controls.Add(this.input_button);
            this.Controls.Add(this.save_textBox);
            this.Controls.Add(this.intput_textBox);
            this.Controls.Add(this.save_label);
            this.Controls.Add(this.input_label);
            this.Name = "GetFeaturePoint_Form";
            this.Text = "getFeaturePoint";
            this.Load += new System.EventHandler(this.GetFeaturePoint_Form_Load);
            this.bottom_statusStrip.ResumeLayout(false);
            this.bottom_statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label input_label;
        private System.Windows.Forms.Label save_label;
        private System.Windows.Forms.TextBox intput_textBox;
        private System.Windows.Forms.TextBox save_textBox;
        private System.Windows.Forms.Button input_button;
        private System.Windows.Forms.Button save_button;
        private System.Windows.Forms.StatusStrip bottom_statusStrip;
        private System.Windows.Forms.ToolStripProgressBar toolStripProgressBar1;
        private System.Windows.Forms.Button ok_button;
        private System.Windows.Forms.Button cancel_button;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.Label yuzhi_label;
        private System.Windows.Forms.TextBox yuzhi_textBox;
    }
}