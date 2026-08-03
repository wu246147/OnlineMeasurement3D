using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OnlineMeasurement
{
    public partial class FormValue : Form
    {
        public double Value { get; set; }
        public FormValue()
        {
            InitializeComponent();

            // 初始化界面
            GeneralFunc.ChangeLanguateFun(typeof(FormValue), this);

        }

        private void buttonIn_Click(object sender, EventArgs e)
        {
            if (!double.TryParse( textBox_Value.Text,out double result))
            {
                MessageBox.Show($"{Resources.LanguageDic.format_error}，{Resources.LanguageDic.not_value}！", $"{Resources.LanguageDic.tip}", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            this.Value = result;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void buttonOut_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void FormValue_Paint(object sender, PaintEventArgs e)
        {
            Control control = (Control)sender;
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;//抗锯齿

            GraphicsPath graphicsPath = new GraphicsPath();
            if (control.ClientRectangle.Width > 0 && control.ClientRectangle.Height > 0)
            {
                graphicsPath.AddRectangle(control.ClientRectangle);
                LinearGradientBrush brush = new LinearGradientBrush(control.ClientRectangle, Color.FromArgb(100, 212, 225), Color.FromArgb(100, 162, 225), LinearGradientMode.BackwardDiagonal);
                g.FillPath(brush, graphicsPath);
            }
        }
    }
}
