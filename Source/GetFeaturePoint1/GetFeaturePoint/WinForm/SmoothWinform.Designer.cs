namespace GetFeaturePoint.WinForm
{
    partial class SmoothWinform
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
            this.input_textBox = new System.Windows.Forms.TextBox();
            this.save_textBox = new System.Windows.Forms.TextBox();
            this.ok_button = new System.Windows.Forms.Button();
            this.cancel_button = new System.Windows.Forms.Button();
            this.close_button = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripProgressBar1 = new System.Windows.Forms.ToolStripProgressBar();
            this.open_button = new System.Windows.Forms.Button();
            this.save_button = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.ziduan_label = new System.Windows.Forms.Label();
            this.field_comboBox = new System.Windows.Forms.ComboBox();
            this.footstep_label = new System.Windows.Forms.Label();
            this.footstep_numericUpDown = new System.Windows.Forms.NumericUpDown();
            this.statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.footstep_numericUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // input_label
            // 
            this.input_label.AutoSize = true;
            this.input_label.Location = new System.Drawing.Point(18, 27);
            this.input_label.Name = "input_label";
            this.input_label.Size = new System.Drawing.Size(59, 12);
            this.input_label.TabIndex = 0;
            this.input_label.Text = "输入路径:";
            // 
            // save_label
            // 
            this.save_label.AutoSize = true;
            this.save_label.Location = new System.Drawing.Point(18, 60);
            this.save_label.Name = "save_label";
            this.save_label.Size = new System.Drawing.Size(59, 12);
            this.save_label.TabIndex = 1;
            this.save_label.Text = "保存路径:";
            // 
            // input_textBox
            // 
            this.input_textBox.Enabled = false;
            this.input_textBox.Location = new System.Drawing.Point(83, 24);
            this.input_textBox.Name = "input_textBox";
            this.input_textBox.Size = new System.Drawing.Size(316, 21);
            this.input_textBox.TabIndex = 2;
            // 
            // save_textBox
            // 
            this.save_textBox.Enabled = false;
            this.save_textBox.Location = new System.Drawing.Point(84, 57);
            this.save_textBox.Name = "save_textBox";
            this.save_textBox.Size = new System.Drawing.Size(315, 21);
            this.save_textBox.TabIndex = 3;
            // 
            // ok_button
            // 
            this.ok_button.Location = new System.Drawing.Point(57, 259);
            this.ok_button.Name = "ok_button";
            this.ok_button.Size = new System.Drawing.Size(75, 23);
            this.ok_button.TabIndex = 4;
            this.ok_button.Text = "确定";
            this.ok_button.UseVisualStyleBackColor = true;
            this.ok_button.Click += new System.EventHandler(this.ok_button_Click);
            // 
            // cancel_button
            // 
            this.cancel_button.Location = new System.Drawing.Point(188, 259);
            this.cancel_button.Name = "cancel_button";
            this.cancel_button.Size = new System.Drawing.Size(75, 23);
            this.cancel_button.TabIndex = 5;
            this.cancel_button.Text = "取消";
            this.cancel_button.UseVisualStyleBackColor = true;
            // 
            // close_button
            // 
            this.close_button.Location = new System.Drawing.Point(324, 259);
            this.close_button.Name = "close_button";
            this.close_button.Size = new System.Drawing.Size(75, 23);
            this.close_button.TabIndex = 6;
            this.close_button.Text = "关闭";
            this.close_button.UseVisualStyleBackColor = true;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripProgressBar1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 288);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(493, 22);
            this.statusStrip1.TabIndex = 7;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripProgressBar1
            // 
            this.toolStripProgressBar1.Name = "toolStripProgressBar1";
            this.toolStripProgressBar1.Size = new System.Drawing.Size(100, 16);
            // 
            // open_button
            // 
            this.open_button.Location = new System.Drawing.Point(405, 24);
            this.open_button.Name = "open_button";
            this.open_button.Size = new System.Drawing.Size(75, 23);
            this.open_button.TabIndex = 8;
            this.open_button.Text = "打开";
            this.open_button.UseVisualStyleBackColor = true;
            this.open_button.Click += new System.EventHandler(this.open_button_Click);
            // 
            // save_button
            // 
            this.save_button.Location = new System.Drawing.Point(406, 55);
            this.save_button.Name = "save_button";
            this.save_button.Size = new System.Drawing.Size(75, 23);
            this.save_button.TabIndex = 9;
            this.save_button.Text = "保存";
            this.save_button.UseVisualStyleBackColor = true;
            this.save_button.Click += new System.EventHandler(this.save_button_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // ziduan_label
            // 
            this.ziduan_label.AutoSize = true;
            this.ziduan_label.Location = new System.Drawing.Point(42, 95);
            this.ziduan_label.Name = "ziduan_label";
            this.ziduan_label.Size = new System.Drawing.Size(35, 12);
            this.ziduan_label.TabIndex = 10;
            this.ziduan_label.Text = "字段:";
            // 
            // field_comboBox
            // 
            this.field_comboBox.FormattingEnabled = true;
            this.field_comboBox.Location = new System.Drawing.Point(84, 95);
            this.field_comboBox.Name = "field_comboBox";
            this.field_comboBox.Size = new System.Drawing.Size(63, 20);
            this.field_comboBox.TabIndex = 11;
            this.field_comboBox.SelectedIndexChanged += new System.EventHandler(this.field_comboBox_SelectedIndexChanged);
            // 
            // footstep_label
            // 
            this.footstep_label.AutoSize = true;
            this.footstep_label.Location = new System.Drawing.Point(42, 136);
            this.footstep_label.Name = "footstep_label";
            this.footstep_label.Size = new System.Drawing.Size(35, 12);
            this.footstep_label.TabIndex = 12;
            this.footstep_label.Text = "步长:";
            // 
            // footstep_numericUpDown
            // 
            this.footstep_numericUpDown.DecimalPlaces = 1;
            this.footstep_numericUpDown.Location = new System.Drawing.Point(83, 127);
            this.footstep_numericUpDown.Name = "footstep_numericUpDown";
            this.footstep_numericUpDown.Size = new System.Drawing.Size(64, 21);
            this.footstep_numericUpDown.TabIndex = 13;
            this.footstep_numericUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.footstep_numericUpDown.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
            this.footstep_numericUpDown.ValueChanged += new System.EventHandler(this.footstep_numericUpDown_ValueChanged);
            // 
            // SmoothWinform
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(493, 310);
            this.Controls.Add(this.footstep_numericUpDown);
            this.Controls.Add(this.footstep_label);
            this.Controls.Add(this.field_comboBox);
            this.Controls.Add(this.ziduan_label);
            this.Controls.Add(this.save_button);
            this.Controls.Add(this.open_button);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.close_button);
            this.Controls.Add(this.cancel_button);
            this.Controls.Add(this.ok_button);
            this.Controls.Add(this.save_textBox);
            this.Controls.Add(this.input_textBox);
            this.Controls.Add(this.save_label);
            this.Controls.Add(this.input_label);
            this.Name = "SmoothWinform";
            this.Text = "SmoothWinform";
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.footstep_numericUpDown)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label input_label;
        private System.Windows.Forms.Label save_label;
        private System.Windows.Forms.TextBox input_textBox;
        private System.Windows.Forms.TextBox save_textBox;
        private System.Windows.Forms.Button ok_button;
        private System.Windows.Forms.Button cancel_button;
        private System.Windows.Forms.Button close_button;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripProgressBar toolStripProgressBar1;
        private System.Windows.Forms.Button open_button;
        private System.Windows.Forms.Button save_button;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.Label ziduan_label;
        private System.Windows.Forms.ComboBox field_comboBox;
        private System.Windows.Forms.Label footstep_label;
        private System.Windows.Forms.NumericUpDown footstep_numericUpDown;
    }
}