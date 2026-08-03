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
    public partial class FormZero : Form
    {
        public Dictionary<int, Point3D> dL = new Dictionary<int, Point3D>();
        public Dictionary<int, Point3D> dR = new Dictionary<int, Point3D>();
        bool isNeedAll = false;
        public FormZero(bool isNeedAll)
        {
            this.isNeedAll = isNeedAll;
            InitializeComponent();

            // 初始化界面
            GeneralFunc.ChangeLanguateFun(typeof(FormZero), this);

        }
        private void FormZero_Load(object sender, EventArgs e)
        {
            dataGridViewL.Rows.Clear();
            foreach (int key in dL.Keys)
            {
                if (dL[key] == null)
                {
                    dataGridViewL.Rows.Add(key, null, null, null);
                }
                else
                {
                    dataGridViewL.Rows.Add(key, dL[key].X, dL[key].Y, dL[key].Z);
                }
            }
            dataGridViewR.Rows.Clear();
            foreach (int key in dR.Keys)
            {
                if (dR[key] == null)
                {
                    dataGridViewR.Rows.Add(key, null, null, null);
                }
                else
                {
                    dataGridViewR.Rows.Add(key, dR[key].X, dR[key].Y, dR[key].Z);
                }
            }
        }
        private void buttonOK_Click(object sender, EventArgs e)
        {
            dL.Clear();
            for (int i = 0; i < dataGridViewL.Rows.Count; i++)
            {
                try
                {
                    int key = int.Parse(dataGridViewL.Rows[i].Cells[0].Value.ToString());
                    float dx = float.Parse(dataGridViewL.Rows[i].Cells[1].Value.ToString());
                    float dy = float.Parse(dataGridViewL.Rows[i].Cells[2].Value.ToString());
                    float dz = float.Parse(dataGridViewL.Rows[i].Cells[3].Value.ToString());
                    dL.Add(key, new Point3D(dx, dy, dz));
                }
                catch (Exception ex)
                {
                    if (isNeedAll)
                    {
                        MessageBox.Show($"{Resources.LanguageDic.format_error}，{Resources.LanguageDic.need_input_all_value}：" + ex.Message);
                        return;
                    }
                }
            }
            dR.Clear();
            for (int i = 0; i < dataGridViewR.Rows.Count; i++)
            {
                try
                {
                    int key = int.Parse(dataGridViewR.Rows[i].Cells[0].Value.ToString());
                    float dx = float.Parse(dataGridViewR.Rows[i].Cells[1].Value.ToString());
                    float dy = float.Parse(dataGridViewR.Rows[i].Cells[2].Value.ToString());
                    float dz = float.Parse(dataGridViewR.Rows[i].Cells[3].Value.ToString());
                    dR.Add(key, new Point3D(dx, dy, dz));
                }
                catch (Exception ex)
                {
                    if (isNeedAll)
                    {
                        MessageBox.Show($"{Resources.LanguageDic.format_error}，{Resources.LanguageDic.need_input_all_value}：" + ex.Message);
                        return;
                    }
                }
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void dataGridViewL_KeyPress(object sender, KeyPressEventArgs e)
        {
            //在选定的单元格，如果按下Ctrl+V
            if (e.KeyChar == 22)
            {
                PasteData(dataGridViewL);
            }
        }
        private void dataGridViewR_KeyPress(object sender, KeyPressEventArgs e)
        {
            //在选定的单元格，如果按下Ctrl+V
            if (e.KeyChar == 22)
            {
                PasteData(dataGridViewR);
            }
        }
        private void PasteData(DataGridView dataGridView)
        {
            try
            {
                string clipboardText = Clipboard.GetText(); //获取剪贴板中的内容
                if (string.IsNullOrEmpty(clipboardText))
                {
                    return;
                }
                int colnum = 0;
                int rownum = 0;
                for (int i = 0; i < clipboardText.Length; i++)
                {
                    if (clipboardText.Substring(i, 1) == "\t")
                    {
                        colnum++;
                    }
                    if (clipboardText.Substring(i, 1) == "\n")
                    {
                        rownum++;
                    }
                }
                colnum = colnum / rownum + 1;
                int selectedRowIndex, selectedColIndex;
                selectedRowIndex = dataGridView.CurrentRow.Index;
                selectedColIndex = dataGridView.CurrentCell.ColumnIndex;
                if (selectedRowIndex + rownum > dataGridView.RowCount || selectedColIndex + colnum > dataGridView.ColumnCount)
                {
                    MessageBox.Show(Resources.LanguageDic.inconsistent_size_of_pasting_area);
                    return;
                }
                String[][] temp = new String[rownum][];
                for (int i = 0; i < rownum; i++)
                {
                    temp[i] = new String[colnum];
                }
                int m = 0, n = 0, len = 0;
                while (len != clipboardText.Length)
                {
                    String str = clipboardText.Substring(len, 1);
                    if (str == "\t")
                    {
                        n++;
                    }
                    else if (str == "\n")
                    {
                        m++;
                        n = 0;
                    }
                    else
                    {
                        temp[m][n] += str;
                    }
                    len++;
                }
                for (int i = selectedRowIndex; i < selectedRowIndex + rownum; i++)
                {
                    for (int j = selectedColIndex; j < selectedColIndex + colnum; j++)
                    {
                        dataGridView.Rows[i].Cells[j].Value = temp[i - selectedRowIndex][j - selectedColIndex];
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void buttonNG_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void FormZero_Paint(object sender, PaintEventArgs e)
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
