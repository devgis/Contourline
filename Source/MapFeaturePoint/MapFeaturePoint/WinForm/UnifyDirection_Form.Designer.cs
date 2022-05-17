namespace MapFeaturePoint.WinForm
{
    partial class UnifyDirection_Form
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
            this.components = new System.ComponentModel.Container();
            this.scan_groupBox = new System.Windows.Forms.GroupBox();
            this.border_radioButton = new System.Windows.Forms.RadioButton();
            this.upperLow_radioButton = new System.Windows.Forms.RadioButton();
            this.leftRight_radioButton = new System.Windows.Forms.RadioButton();
            this.bottom_statusStrip = new System.Windows.Forms.StatusStrip();
            this.toolStripProgressBar1 = new System.Windows.Forms.ToolStripProgressBar();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
            this.cancel_button = new System.Windows.Forms.Button();
            this.ok_button = new System.Windows.Forms.Button();
            this.save_button = new System.Windows.Forms.Button();
            this.input_button = new System.Windows.Forms.Button();
            this.save_textBox = new System.Windows.Forms.TextBox();
            this.intput_textBox = new System.Windows.Forms.TextBox();
            this.save_label = new System.Windows.Forms.Label();
            this.input_label = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.scan_groupBox.SuspendLayout();
            this.bottom_statusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // scan_groupBox
            // 
            this.scan_groupBox.Controls.Add(this.border_radioButton);
            this.scan_groupBox.Controls.Add(this.upperLow_radioButton);
            this.scan_groupBox.Controls.Add(this.leftRight_radioButton);
            this.scan_groupBox.Location = new System.Drawing.Point(12, 98);
            this.scan_groupBox.Name = "scan_groupBox";
            this.scan_groupBox.Size = new System.Drawing.Size(402, 83);
            this.scan_groupBox.TabIndex = 0;
            this.scan_groupBox.TabStop = false;
            this.scan_groupBox.Text = "扫描方向";
            // 
            // border_radioButton
            // 
            this.border_radioButton.AutoSize = true;
            this.border_radioButton.Location = new System.Drawing.Point(250, 35);
            this.border_radioButton.Name = "border_radioButton";
            this.border_radioButton.Size = new System.Drawing.Size(71, 16);
            this.border_radioButton.TabIndex = 2;
            this.border_radioButton.TabStop = true;
            this.border_radioButton.Text = "边角扫描";
            this.border_radioButton.UseVisualStyleBackColor = true;
            // 
            // upperLow_radioButton
            // 
            this.upperLow_radioButton.AutoSize = true;
            this.upperLow_radioButton.Location = new System.Drawing.Point(137, 35);
            this.upperLow_radioButton.Name = "upperLow_radioButton";
            this.upperLow_radioButton.Size = new System.Drawing.Size(71, 16);
            this.upperLow_radioButton.TabIndex = 1;
            this.upperLow_radioButton.Text = "上下扫描";
            this.upperLow_radioButton.UseVisualStyleBackColor = true;
            this.upperLow_radioButton.CheckedChanged += new System.EventHandler(this.upperLow_radioButton_CheckedChanged);
            // 
            // leftRight_radioButton
            // 
            this.leftRight_radioButton.AutoSize = true;
            this.leftRight_radioButton.Checked = true;
            this.leftRight_radioButton.Location = new System.Drawing.Point(27, 35);
            this.leftRight_radioButton.Name = "leftRight_radioButton";
            this.leftRight_radioButton.Size = new System.Drawing.Size(71, 16);
            this.leftRight_radioButton.TabIndex = 0;
            this.leftRight_radioButton.TabStop = true;
            this.leftRight_radioButton.Text = "左右扫描";
            this.leftRight_radioButton.UseVisualStyleBackColor = true;
            this.leftRight_radioButton.CheckedChanged += new System.EventHandler(this.leftRight_radioButton_CheckedChanged);
            // 
            // bottom_statusStrip
            // 
            this.bottom_statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripProgressBar1,
            this.toolStripStatusLabel1,
            this.toolStripStatusLabel2});
            this.bottom_statusStrip.Location = new System.Drawing.Point(0, 272);
            this.bottom_statusStrip.Name = "bottom_statusStrip";
            this.bottom_statusStrip.Size = new System.Drawing.Size(426, 22);
            this.bottom_statusStrip.TabIndex = 9;
            this.bottom_statusStrip.Text = "进程";
            // 
            // toolStripProgressBar1
            // 
            this.toolStripProgressBar1.Name = "toolStripProgressBar1";
            this.toolStripProgressBar1.Size = new System.Drawing.Size(100, 16);
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(131, 17);
            this.toolStripStatusLabel1.Text = "toolStripStatusLabel1";
            // 
            // toolStripStatusLabel2
            // 
            this.toolStripStatusLabel2.Name = "toolStripStatusLabel2";
            this.toolStripStatusLabel2.Size = new System.Drawing.Size(131, 17);
            this.toolStripStatusLabel2.Text = "toolStripStatusLabel2";
            // 
            // cancel_button
            // 
            this.cancel_button.Location = new System.Drawing.Point(252, 218);
            this.cancel_button.Name = "cancel_button";
            this.cancel_button.Size = new System.Drawing.Size(75, 23);
            this.cancel_button.TabIndex = 22;
            this.cancel_button.Text = "取消";
            this.cancel_button.UseVisualStyleBackColor = true;
            // 
            // ok_button
            // 
            this.ok_button.Location = new System.Drawing.Point(92, 220);
            this.ok_button.Name = "ok_button";
            this.ok_button.Size = new System.Drawing.Size(75, 23);
            this.ok_button.TabIndex = 21;
            this.ok_button.Text = "确定";
            this.ok_button.UseVisualStyleBackColor = true;
            this.ok_button.Click += new System.EventHandler(this.ok_button_Click);
            // 
            // save_button
            // 
            this.save_button.Enabled = false;
            this.save_button.Location = new System.Drawing.Point(346, 44);
            this.save_button.Name = "save_button";
            this.save_button.Size = new System.Drawing.Size(75, 23);
            this.save_button.TabIndex = 20;
            this.save_button.Text = "保存";
            this.save_button.UseVisualStyleBackColor = true;
            // 
            // input_button
            // 
            this.input_button.Enabled = false;
            this.input_button.Location = new System.Drawing.Point(346, 10);
            this.input_button.Name = "input_button";
            this.input_button.Size = new System.Drawing.Size(75, 23);
            this.input_button.TabIndex = 23;
            this.input_button.Text = "打开";
            // 
            // save_textBox
            // 
            this.save_textBox.Enabled = false;
            this.save_textBox.Location = new System.Drawing.Point(72, 44);
            this.save_textBox.Name = "save_textBox";
            this.save_textBox.Size = new System.Drawing.Size(268, 21);
            this.save_textBox.TabIndex = 19;
            this.save_textBox.Text = "D:\\_temp";
            // 
            // intput_textBox
            // 
            this.intput_textBox.Enabled = false;
            this.intput_textBox.Location = new System.Drawing.Point(72, 12);
            this.intput_textBox.Name = "intput_textBox";
            this.intput_textBox.Size = new System.Drawing.Size(268, 21);
            this.intput_textBox.TabIndex = 18;
            this.intput_textBox.Text = "E:\\temp\\terlk3ddsms.shp";
            // 
            // save_label
            // 
            this.save_label.AutoSize = true;
            this.save_label.Location = new System.Drawing.Point(3, 47);
            this.save_label.Name = "save_label";
            this.save_label.Size = new System.Drawing.Size(65, 12);
            this.save_label.TabIndex = 17;
            this.save_label.Text = "临时目录：";
            // 
            // input_label
            // 
            this.input_label.AutoSize = true;
            this.input_label.Location = new System.Drawing.Point(3, 15);
            this.input_label.Name = "input_label";
            this.input_label.Size = new System.Drawing.Size(65, 12);
            this.input_label.TabIndex = 16;
            this.input_label.Text = "输入路径：";
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 1000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // UnifyDirection_Form
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(426, 294);
            this.Controls.Add(this.cancel_button);
            this.Controls.Add(this.ok_button);
            this.Controls.Add(this.save_button);
            this.Controls.Add(this.input_button);
            this.Controls.Add(this.save_textBox);
            this.Controls.Add(this.intput_textBox);
            this.Controls.Add(this.save_label);
            this.Controls.Add(this.input_label);
            this.Controls.Add(this.bottom_statusStrip);
            this.Controls.Add(this.scan_groupBox);
            this.Name = "UnifyDirection_Form";
            this.Text = "UnifyDirection_Form";
            this.Load += new System.EventHandler(this.UnifyDirection_Form_Load);
            this.scan_groupBox.ResumeLayout(false);
            this.scan_groupBox.PerformLayout();
            this.bottom_statusStrip.ResumeLayout(false);
            this.bottom_statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox scan_groupBox;
        private System.Windows.Forms.RadioButton upperLow_radioButton;
        private System.Windows.Forms.RadioButton leftRight_radioButton;
        private System.Windows.Forms.StatusStrip bottom_statusStrip;
        private System.Windows.Forms.ToolStripProgressBar toolStripProgressBar1;
        private System.Windows.Forms.Button cancel_button;
        private System.Windows.Forms.Button ok_button;
        private System.Windows.Forms.Button save_button;
        private System.Windows.Forms.Button input_button;
        private System.Windows.Forms.TextBox save_textBox;
        private System.Windows.Forms.TextBox intput_textBox;
        private System.Windows.Forms.Label save_label;
        private System.Windows.Forms.Label input_label;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel2;
        private System.Windows.Forms.RadioButton border_radioButton;
    }
}