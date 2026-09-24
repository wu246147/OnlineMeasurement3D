namespace OnlineMeasurement
{
    partial class FormValue
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormValue));
            this.label1 = new System.Windows.Forms.Label();
            this.buttonOut = new System.Windows.Forms.Button();
            this.buttonIn = new System.Windows.Forms.Button();
            this.textBox_Value = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            this.label1.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.ForeColor = System.Drawing.Color.FromArgb(239, 243, 248);
            // 
            // buttonOut
            // 
            this.buttonOut.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            resources.ApplyResources(this.buttonOut, "buttonOut");
            this.buttonOut.Name = "buttonOut";
            this.buttonOut.Click += new System.EventHandler(this.buttonOut_Click);
            this.buttonOut.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.buttonOut.BackColor = System.Drawing.Color.FromArgb(49, 61, 77);
            this.buttonOut.ForeColor = System.Drawing.Color.FromArgb(239, 243, 248);
            this.buttonOut.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonOut.UseVisualStyleBackColor = false;
            // 
            // buttonIn
            // 
            resources.ApplyResources(this.buttonIn, "buttonIn");
            this.buttonIn.Name = "buttonIn";
            this.buttonIn.Click += new System.EventHandler(this.buttonIn_Click);
            this.buttonIn.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.buttonIn.BackColor = System.Drawing.Color.FromArgb(49, 61, 77);
            this.buttonIn.ForeColor = System.Drawing.Color.FromArgb(239, 243, 248);
            this.buttonIn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonIn.UseVisualStyleBackColor = false;
            // 
            // textBox_Value
            // 
            resources.ApplyResources(this.textBox_Value, "textBox_Value");
            this.textBox_Value.Name = "textBox_Value";
            this.textBox_Value.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textBox_Value.BackColor = System.Drawing.Color.FromArgb(54, 66, 83);
            this.textBox_Value.ForeColor = System.Drawing.Color.FromArgb(239, 243, 248);
            // 
            // FormValue
            // 
            this.AcceptButton = this.buttonIn;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonOut;
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonOut);
            this.Controls.Add(this.buttonIn);
            this.Controls.Add(this.textBox_Value);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "FormValue";
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.FormValue_Paint);
            this.BackColor = System.Drawing.Color.FromArgb(31, 37, 47);
            this.ForeColor = System.Drawing.Color.FromArgb(239, 243, 248);
            this.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button buttonOut;
        private System.Windows.Forms.Button buttonIn;
        private System.Windows.Forms.TextBox textBox_Value;
    }
}