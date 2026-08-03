namespace OnlineMeasurement
{
    partial class FormName
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormName));
            this.textBox_Name = new System.Windows.Forms.TextBox();
            this.buttonIn = new System.Windows.Forms.Button();
            this.buttonOut = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // textBox_Name
            // 
            resources.ApplyResources(this.textBox_Name, "textBox_Name");
            this.textBox_Name.Name = "textBox_Name";
            // 
            // buttonIn
            // 
            resources.ApplyResources(this.buttonIn, "buttonIn");
            this.buttonIn.Name = "buttonIn";
            this.buttonIn.UseVisualStyleBackColor = true;
            this.buttonIn.Click += new System.EventHandler(this.buttonIn_Click);
            // 
            // buttonOut
            // 
            this.buttonOut.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            resources.ApplyResources(this.buttonOut, "buttonOut");
            this.buttonOut.Name = "buttonOut";
            this.buttonOut.UseVisualStyleBackColor = true;
            this.buttonOut.Click += new System.EventHandler(this.buttonOut_Click);
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.Name = "label1";
            // 
            // FormName
            // 
            this.AcceptButton = this.buttonIn;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonOut;
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonOut);
            this.Controls.Add(this.buttonIn);
            this.Controls.Add(this.textBox_Name);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "FormName";
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.FormName_Paint);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBox_Name;
        private System.Windows.Forms.Button buttonIn;
        private System.Windows.Forms.Button buttonOut;
        private System.Windows.Forms.Label label1;
    }
}