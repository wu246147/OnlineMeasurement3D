namespace OnlineMeasurement.IO
{
    partial class HslForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HslForm));
            this.button_Close = new System.Windows.Forms.Button();
            this.button_Open = new System.Windows.Forms.Button();
            this.numericUpDownPort = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.numericUpDownDA2 = new System.Windows.Forms.NumericUpDown();
            this.textBoxIpAddress = new System.Windows.Forms.TextBox();
            this.labelInName = new System.Windows.Forms.Label();
            this.labelOutName = new System.Windows.Forms.Label();
            this.labelInAddress = new System.Windows.Forms.Label();
            this.labelInValue = new System.Windows.Forms.Label();
            this.textBoxLog = new System.Windows.Forms.TextBox();
            this.labelOutAddress = new System.Windows.Forms.Label();
            this.labelOutValue = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.checkBox_IsStringReverseByteWord = new System.Windows.Forms.CheckBox();
            this.comboBox_cam = new System.Windows.Forms.ComboBox();
            this.comboBox_DataFormat = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPort)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDA2)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // button_Close
            // 
            resources.ApplyResources(this.button_Close, "button_Close");
            this.button_Close.Name = "button_Close";
            this.button_Close.UseVisualStyleBackColor = true;
            this.button_Close.Click += new System.EventHandler(this.button_Close_Click);
            // 
            // button_Open
            // 
            resources.ApplyResources(this.button_Open, "button_Open");
            this.button_Open.Name = "button_Open";
            this.button_Open.UseVisualStyleBackColor = true;
            this.button_Open.Click += new System.EventHandler(this.button_Open_Click);
            // 
            // numericUpDownPort
            // 
            resources.ApplyResources(this.numericUpDownPort, "numericUpDownPort");
            this.numericUpDownPort.Maximum = new decimal(new int[] {
            99999999,
            0,
            0,
            0});
            this.numericUpDownPort.Name = "numericUpDownPort";
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // numericUpDownDA2
            // 
            resources.ApplyResources(this.numericUpDownDA2, "numericUpDownDA2");
            this.numericUpDownDA2.Maximum = new decimal(new int[] {
            99999999,
            0,
            0,
            0});
            this.numericUpDownDA2.Name = "numericUpDownDA2";
            // 
            // textBoxIpAddress
            // 
            resources.ApplyResources(this.textBoxIpAddress, "textBoxIpAddress");
            this.textBoxIpAddress.Name = "textBoxIpAddress";
            // 
            // labelInName
            // 
            resources.ApplyResources(this.labelInName, "labelInName");
            this.labelInName.BackColor = System.Drawing.Color.Transparent;
            this.labelInName.Name = "labelInName";
            // 
            // labelOutName
            // 
            resources.ApplyResources(this.labelOutName, "labelOutName");
            this.labelOutName.BackColor = System.Drawing.Color.Transparent;
            this.labelOutName.Name = "labelOutName";
            // 
            // labelInAddress
            // 
            resources.ApplyResources(this.labelInAddress, "labelInAddress");
            this.labelInAddress.BackColor = System.Drawing.Color.Transparent;
            this.labelInAddress.Name = "labelInAddress";
            // 
            // labelInValue
            // 
            resources.ApplyResources(this.labelInValue, "labelInValue");
            this.labelInValue.BackColor = System.Drawing.Color.Transparent;
            this.labelInValue.Name = "labelInValue";
            // 
            // textBoxLog
            // 
            resources.ApplyResources(this.textBoxLog, "textBoxLog");
            this.textBoxLog.Name = "textBoxLog";
            // 
            // labelOutAddress
            // 
            resources.ApplyResources(this.labelOutAddress, "labelOutAddress");
            this.labelOutAddress.BackColor = System.Drawing.Color.Transparent;
            this.labelOutAddress.Name = "labelOutAddress";
            // 
            // labelOutValue
            // 
            resources.ApplyResources(this.labelOutValue, "labelOutValue");
            this.labelOutValue.BackColor = System.Drawing.Color.Transparent;
            this.labelOutValue.Name = "labelOutValue";
            // 
            // groupBox1
            // 
            this.groupBox1.BackColor = System.Drawing.Color.Transparent;
            this.groupBox1.Controls.Add(this.checkBox_IsStringReverseByteWord);
            this.groupBox1.Controls.Add(this.comboBox_cam);
            this.groupBox1.Controls.Add(this.comboBox_DataFormat);
            this.groupBox1.Controls.Add(this.textBoxIpAddress);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.numericUpDownPort);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.numericUpDownDA2);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.label3);
            resources.ApplyResources(this.groupBox1, "groupBox1");
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.TabStop = false;
            // 
            // checkBox_IsStringReverseByteWord
            // 
            resources.ApplyResources(this.checkBox_IsStringReverseByteWord, "checkBox_IsStringReverseByteWord");
            this.checkBox_IsStringReverseByteWord.Name = "checkBox_IsStringReverseByteWord";
            this.checkBox_IsStringReverseByteWord.UseVisualStyleBackColor = true;
            // 
            // comboBox_cam
            // 
            this.comboBox_cam.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_cam.FormattingEnabled = true;
            resources.ApplyResources(this.comboBox_cam, "comboBox_cam");
            this.comboBox_cam.Name = "comboBox_cam";
            this.comboBox_cam.SelectedIndexChanged += new System.EventHandler(this.comboBox_cam_SelectedIndexChanged);
            // 
            // comboBox_DataFormat
            // 
            this.comboBox_DataFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_DataFormat.FormattingEnabled = true;
            resources.ApplyResources(this.comboBox_DataFormat, "comboBox_DataFormat");
            this.comboBox_DataFormat.Name = "comboBox_DataFormat";
            // 
            // label6
            // 
            resources.ApplyResources(this.label6, "label6");
            this.label6.Name = "label6";
            // 
            // label5
            // 
            resources.ApplyResources(this.label5, "label5");
            this.label5.Name = "label5";
            // 
            // label4
            // 
            resources.ApplyResources(this.label4, "label4");
            this.label4.BackColor = System.Drawing.Color.Transparent;
            this.label4.Name = "label4";
            // 
            // HslForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.label4);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.labelOutValue);
            this.Controls.Add(this.labelOutAddress);
            this.Controls.Add(this.labelInValue);
            this.Controls.Add(this.labelInAddress);
            this.Controls.Add(this.labelOutName);
            this.Controls.Add(this.labelInName);
            this.Controls.Add(this.textBoxLog);
            this.Controls.Add(this.button_Close);
            this.Controls.Add(this.button_Open);
            this.Name = "HslForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.HslForm_FormClosing);
            this.Load += new System.EventHandler(this.HslForm_Load);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.HslForm_Paint);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPort)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDA2)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button_Close;
        private System.Windows.Forms.Button button_Open;
        private System.Windows.Forms.NumericUpDown numericUpDownPort;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown numericUpDownDA2;
        private System.Windows.Forms.TextBox textBoxIpAddress;
        private System.Windows.Forms.Label labelInName;
        private System.Windows.Forms.Label labelOutName;
        private System.Windows.Forms.Label labelInAddress;
        private System.Windows.Forms.Label labelInValue;
        private System.Windows.Forms.TextBox textBoxLog;
        private System.Windows.Forms.Label labelOutAddress;
        private System.Windows.Forms.Label labelOutValue;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox checkBox_IsStringReverseByteWord;
        private System.Windows.Forms.ComboBox comboBox_DataFormat;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox comboBox_cam;
        private System.Windows.Forms.Label label6;
    }
}