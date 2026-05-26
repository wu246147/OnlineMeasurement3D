namespace OnlineMeasurement
{
    partial class FormRun
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormRun));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            this.buttonRun = new System.Windows.Forms.Button();
            this.buttonStop = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.labelCarKind = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.labelCarNum = new System.Windows.Forms.Label();
            this.dataGridViewLog = new System.Windows.Forms.DataGridView();
            this.Column1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.label2 = new System.Windows.Forms.Label();
            this.lblSysStatus = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.labelCarName = new System.Windows.Forms.Label();
            this.dataGridViewShow = new System.Windows.Forms.DataGridView();
            this.Column3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column5 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column6 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column7 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column8 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column9 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column10 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.textBoxTestNum = new System.Windows.Forms.TextBox();
            this.buttonTest = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.button_setRobotR = new System.Windows.Forms.Button();
            this.button_setRobotL = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.buttonSave = new System.Windows.Forms.Button();
            this.checkBoxZero = new System.Windows.Forms.CheckBox();
            this.checkBoxFrame = new System.Windows.Forms.CheckBox();
            this.label6 = new System.Windows.Forms.Label();
            this.button_setPLC = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.checkBoxLedL = new System.Windows.Forms.CheckBox();
            this.checkBoxLightL = new System.Windows.Forms.CheckBox();
            this.checkBoxLightR = new System.Windows.Forms.CheckBox();
            this.checkBoxLedR = new System.Windows.Forms.CheckBox();
            this.buttonSetting = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.label8 = new System.Windows.Forms.Label();
            this.checkBox_KeepOK = new System.Windows.Forms.CheckBox();
            this.buttonClear = new System.Windows.Forms.Button();
            this.buttonSkipStart = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.labelResult = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewLog)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewShow)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonRun
            // 
            resources.ApplyResources(this.buttonRun, "buttonRun");
            this.buttonRun.Name = "buttonRun";
            this.buttonRun.UseVisualStyleBackColor = true;
            this.buttonRun.Click += new System.EventHandler(this.buttonRun_Click);
            // 
            // buttonStop
            // 
            resources.ApplyResources(this.buttonStop, "buttonStop");
            this.buttonStop.Name = "buttonStop";
            this.buttonStop.UseVisualStyleBackColor = true;
            this.buttonStop.Click += new System.EventHandler(this.buttonStop_Click);
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.Name = "label1";
            // 
            // labelCarKind
            // 
            resources.ApplyResources(this.labelCarKind, "labelCarKind");
            this.labelCarKind.BackColor = System.Drawing.Color.Transparent;
            this.labelCarKind.Name = "labelCarKind";
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.BackColor = System.Drawing.Color.Transparent;
            this.label3.Name = "label3";
            // 
            // labelCarNum
            // 
            resources.ApplyResources(this.labelCarNum, "labelCarNum");
            this.labelCarNum.BackColor = System.Drawing.Color.Transparent;
            this.labelCarNum.Name = "labelCarNum";
            // 
            // dataGridViewLog
            // 
            this.dataGridViewLog.AllowUserToAddRows = false;
            this.dataGridViewLog.AllowUserToDeleteRows = false;
            this.dataGridViewLog.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.dataGridViewLog.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            this.dataGridViewLog.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewLog.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column1,
            this.Column2});
            resources.ApplyResources(this.dataGridViewLog, "dataGridViewLog");
            this.dataGridViewLog.Name = "dataGridViewLog";
            this.dataGridViewLog.ReadOnly = true;
            this.dataGridViewLog.RowHeadersVisible = false;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewLog.RowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridViewLog.RowTemplate.Height = 23;
            // 
            // Column1
            // 
            resources.ApplyResources(this.Column1, "Column1");
            this.Column1.Name = "Column1";
            this.Column1.ReadOnly = true;
            this.Column1.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // Column2
            // 
            resources.ApplyResources(this.Column2, "Column2");
            this.Column2.Name = "Column2";
            this.Column2.ReadOnly = true;
            this.Column2.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.BackColor = System.Drawing.Color.Transparent;
            this.label2.Name = "label2";
            // 
            // lblSysStatus
            // 
            resources.ApplyResources(this.lblSysStatus, "lblSysStatus");
            this.lblSysStatus.BackColor = System.Drawing.Color.Transparent;
            this.lblSysStatus.Name = "lblSysStatus";
            // 
            // label4
            // 
            resources.ApplyResources(this.label4, "label4");
            this.label4.BackColor = System.Drawing.Color.Transparent;
            this.label4.Name = "label4";
            // 
            // labelCarName
            // 
            resources.ApplyResources(this.labelCarName, "labelCarName");
            this.labelCarName.BackColor = System.Drawing.Color.Transparent;
            this.labelCarName.Name = "labelCarName";
            // 
            // dataGridViewShow
            // 
            this.dataGridViewShow.AllowUserToAddRows = false;
            this.dataGridViewShow.AllowUserToDeleteRows = false;
            this.dataGridViewShow.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewShow.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column3,
            this.Column4,
            this.Column5,
            this.Column6,
            this.Column7,
            this.Column8,
            this.Column9,
            this.Column10});
            resources.ApplyResources(this.dataGridViewShow, "dataGridViewShow");
            this.dataGridViewShow.Name = "dataGridViewShow";
            this.dataGridViewShow.ReadOnly = true;
            this.dataGridViewShow.RowHeadersVisible = false;
            this.dataGridViewShow.RowTemplate.Height = 23;
            // 
            // Column3
            // 
            resources.ApplyResources(this.Column3, "Column3");
            this.Column3.Name = "Column3";
            this.Column3.ReadOnly = true;
            // 
            // Column4
            // 
            resources.ApplyResources(this.Column4, "Column4");
            this.Column4.Name = "Column4";
            this.Column4.ReadOnly = true;
            // 
            // Column5
            // 
            resources.ApplyResources(this.Column5, "Column5");
            this.Column5.Name = "Column5";
            this.Column5.ReadOnly = true;
            // 
            // Column6
            // 
            resources.ApplyResources(this.Column6, "Column6");
            this.Column6.Name = "Column6";
            this.Column6.ReadOnly = true;
            // 
            // Column7
            // 
            resources.ApplyResources(this.Column7, "Column7");
            this.Column7.Name = "Column7";
            this.Column7.ReadOnly = true;
            // 
            // Column8
            // 
            resources.ApplyResources(this.Column8, "Column8");
            this.Column8.Name = "Column8";
            this.Column8.ReadOnly = true;
            // 
            // Column9
            // 
            resources.ApplyResources(this.Column9, "Column9");
            this.Column9.Name = "Column9";
            this.Column9.ReadOnly = true;
            // 
            // Column10
            // 
            resources.ApplyResources(this.Column10, "Column10");
            this.Column10.Name = "Column10";
            this.Column10.ReadOnly = true;
            // 
            // textBoxTestNum
            // 
            resources.ApplyResources(this.textBoxTestNum, "textBoxTestNum");
            this.textBoxTestNum.Name = "textBoxTestNum";
            // 
            // buttonTest
            // 
            resources.ApplyResources(this.buttonTest, "buttonTest");
            this.buttonTest.Name = "buttonTest";
            this.buttonTest.UseVisualStyleBackColor = true;
            this.buttonTest.Click += new System.EventHandler(this.buttonTest_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.BackColor = System.Drawing.Color.Transparent;
            this.groupBox1.Controls.Add(this.button_setRobotR);
            this.groupBox1.Controls.Add(this.button_setRobotL);
            this.groupBox1.Controls.Add(this.button3);
            this.groupBox1.Controls.Add(this.buttonSave);
            this.groupBox1.Controls.Add(this.checkBoxZero);
            this.groupBox1.Controls.Add(this.checkBoxFrame);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.textBoxTestNum);
            this.groupBox1.Controls.Add(this.buttonTest);
            this.groupBox1.Controls.Add(this.button_setPLC);
            resources.ApplyResources(this.groupBox1, "groupBox1");
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.TabStop = false;
            // 
            // button_setRobotR
            // 
            resources.ApplyResources(this.button_setRobotR, "button_setRobotR");
            this.button_setRobotR.Name = "button_setRobotR";
            this.button_setRobotR.UseVisualStyleBackColor = true;
            this.button_setRobotR.Click += new System.EventHandler(this.button_setRobotR_Click);
            // 
            // button_setRobotL
            // 
            resources.ApplyResources(this.button_setRobotL, "button_setRobotL");
            this.button_setRobotL.Name = "button_setRobotL";
            this.button_setRobotL.UseVisualStyleBackColor = true;
            this.button_setRobotL.Click += new System.EventHandler(this.button_setRobotL_Click);
            // 
            // button3
            // 
            resources.ApplyResources(this.button3, "button3");
            this.button3.Name = "button3";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // buttonSave
            // 
            resources.ApplyResources(this.buttonSave, "buttonSave");
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.UseVisualStyleBackColor = true;
            this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);
            // 
            // checkBoxZero
            // 
            resources.ApplyResources(this.checkBoxZero, "checkBoxZero");
            this.checkBoxZero.Name = "checkBoxZero";
            this.checkBoxZero.UseVisualStyleBackColor = true;
            // 
            // checkBoxFrame
            // 
            resources.ApplyResources(this.checkBoxFrame, "checkBoxFrame");
            this.checkBoxFrame.Name = "checkBoxFrame";
            this.checkBoxFrame.UseVisualStyleBackColor = true;
            // 
            // label6
            // 
            resources.ApplyResources(this.label6, "label6");
            this.label6.Name = "label6";
            // 
            // button_setPLC
            // 
            resources.ApplyResources(this.button_setPLC, "button_setPLC");
            this.button_setPLC.Name = "button_setPLC";
            this.button_setPLC.UseVisualStyleBackColor = true;
            this.button_setPLC.Click += new System.EventHandler(this.button_setPLC_Click);
            // 
            // button2
            // 
            resources.ApplyResources(this.button2, "button2");
            this.button2.Name = "button2";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button1
            // 
            resources.ApplyResources(this.button1, "button1");
            this.button1.Name = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // checkBoxLedL
            // 
            resources.ApplyResources(this.checkBoxLedL, "checkBoxLedL");
            this.checkBoxLedL.Name = "checkBoxLedL";
            this.checkBoxLedL.UseVisualStyleBackColor = true;
            this.checkBoxLedL.CheckedChanged += new System.EventHandler(this.checkBoxLedL_CheckedChanged);
            // 
            // checkBoxLightL
            // 
            resources.ApplyResources(this.checkBoxLightL, "checkBoxLightL");
            this.checkBoxLightL.Name = "checkBoxLightL";
            this.checkBoxLightL.UseVisualStyleBackColor = true;
            this.checkBoxLightL.CheckedChanged += new System.EventHandler(this.checkBoxLightL_CheckedChanged);
            // 
            // checkBoxLightR
            // 
            resources.ApplyResources(this.checkBoxLightR, "checkBoxLightR");
            this.checkBoxLightR.Name = "checkBoxLightR";
            this.checkBoxLightR.UseVisualStyleBackColor = true;
            this.checkBoxLightR.CheckedChanged += new System.EventHandler(this.checkBoxLightR_CheckedChanged);
            // 
            // checkBoxLedR
            // 
            resources.ApplyResources(this.checkBoxLedR, "checkBoxLedR");
            this.checkBoxLedR.Name = "checkBoxLedR";
            this.checkBoxLedR.UseVisualStyleBackColor = true;
            this.checkBoxLedR.CheckedChanged += new System.EventHandler(this.checkBoxLedR_CheckedChanged);
            // 
            // buttonSetting
            // 
            resources.ApplyResources(this.buttonSetting, "buttonSetting");
            this.buttonSetting.Name = "buttonSetting";
            this.buttonSetting.UseVisualStyleBackColor = true;
            this.buttonSetting.Click += new System.EventHandler(this.buttonSetting_Click);
            // 
            // panel1
            // 
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.BackColor = System.Drawing.Color.Transparent;
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.groupBox2);
            this.panel1.Controls.Add(this.comboBox1);
            this.panel1.Controls.Add(this.label8);
            this.panel1.Controls.Add(this.checkBox_KeepOK);
            this.panel1.Controls.Add(this.buttonClear);
            this.panel1.Controls.Add(this.buttonSkipStart);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.groupBox1);
            this.panel1.Controls.Add(this.buttonSetting);
            this.panel1.Controls.Add(this.buttonRun);
            this.panel1.Controls.Add(this.dataGridViewShow);
            this.panel1.Controls.Add(this.buttonStop);
            this.panel1.Controls.Add(this.label7);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.label4);
            this.panel1.Controls.Add(this.dataGridViewLog);
            this.panel1.Controls.Add(this.labelResult);
            this.panel1.Controls.Add(this.labelCarKind);
            this.panel1.Controls.Add(this.lblSysStatus);
            this.panel1.Controls.Add(this.labelCarName);
            this.panel1.Controls.Add(this.labelCarNum);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Name = "panel1";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.button2);
            this.groupBox2.Controls.Add(this.checkBoxLedL);
            this.groupBox2.Controls.Add(this.button1);
            this.groupBox2.Controls.Add(this.checkBoxLightL);
            this.groupBox2.Controls.Add(this.checkBoxLedR);
            this.groupBox2.Controls.Add(this.checkBoxLightR);
            resources.ApplyResources(this.groupBox2, "groupBox2");
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.TabStop = false;
            // 
            // comboBox1
            // 
            this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Items.AddRange(new object[] {
            resources.GetString("comboBox1.Items"),
            resources.GetString("comboBox1.Items1")});
            resources.ApplyResources(this.comboBox1, "comboBox1");
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // label8
            // 
            resources.ApplyResources(this.label8, "label8");
            this.label8.Name = "label8";
            // 
            // checkBox_KeepOK
            // 
            resources.ApplyResources(this.checkBox_KeepOK, "checkBox_KeepOK");
            this.checkBox_KeepOK.Name = "checkBox_KeepOK";
            this.checkBox_KeepOK.UseVisualStyleBackColor = true;
            // 
            // buttonClear
            // 
            resources.ApplyResources(this.buttonClear, "buttonClear");
            this.buttonClear.Name = "buttonClear";
            this.buttonClear.UseVisualStyleBackColor = true;
            this.buttonClear.Click += new System.EventHandler(this.buttonClear_Click);
            // 
            // buttonSkipStart
            // 
            resources.ApplyResources(this.buttonSkipStart, "buttonSkipStart");
            this.buttonSkipStart.Name = "buttonSkipStart";
            this.buttonSkipStart.UseVisualStyleBackColor = true;
            this.buttonSkipStart.Click += new System.EventHandler(this.buttonSkipStart_Click);
            // 
            // label7
            // 
            resources.ApplyResources(this.label7, "label7");
            this.label7.BackColor = System.Drawing.Color.Transparent;
            this.label7.Name = "label7";
            // 
            // labelResult
            // 
            resources.ApplyResources(this.labelResult, "labelResult");
            this.labelResult.BackColor = System.Drawing.Color.Transparent;
            this.labelResult.Name = "labelResult";
            // 
            // FormRun
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "FormRun";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormRun_FormClosing);
            this.Load += new System.EventHandler(this.FormRun_Load);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.FormRun_Paint);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewLog)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewShow)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonRun;
        private System.Windows.Forms.Button buttonStop;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label labelCarKind;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label labelCarNum;
        private System.Windows.Forms.DataGridView dataGridViewLog;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblSysStatus;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label labelCarName;
        private System.Windows.Forms.DataGridView dataGridViewShow;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column3;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column4;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column5;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column6;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column7;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column8;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column9;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column10;
        private System.Windows.Forms.TextBox textBoxTestNum;
        private System.Windows.Forms.Button buttonTest;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column1;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.CheckBox checkBoxLedL;
        private System.Windows.Forms.CheckBox checkBoxLightR;
        private System.Windows.Forms.CheckBox checkBoxLightL;
        private System.Windows.Forms.CheckBox checkBoxLedR;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button buttonSetting;
        private System.Windows.Forms.CheckBox checkBoxFrame;
        private System.Windows.Forms.CheckBox checkBoxZero;
        private System.Windows.Forms.Button buttonSave;
        private System.Windows.Forms.Button buttonSkipStart;
        private System.Windows.Forms.Button buttonClear;
        private System.Windows.Forms.CheckBox checkBox_KeepOK;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label labelResult;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button_setPLC;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button_setRobotR;
        private System.Windows.Forms.Button button_setRobotL;
    }
}

