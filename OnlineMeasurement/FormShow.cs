using HalconDotNet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace OnlineMeasurement
{
    public partial class FormShow : Form
    {
        public bool IsShow = false;
        public bool EnableClose = false;
        public FormShow()
        {
            InitializeComponent();
            // 初始化界面
            GeneralFunc.ChangeLanguateFun(typeof(FormShow), this);

        }

        private void tableLayoutPanel_Max_Paint(object sender, PaintEventArgs e)
        {
            Control control = (Control)sender;
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;//抗锯齿
            using (GraphicsPath graphicsPath = new GraphicsPath())
            {
                Rectangle rectangle = new Rectangle(control.ClientRectangle.Left, control.ClientRectangle.Top + 56, control.ClientRectangle.Width, control.ClientRectangle.Height - 56);
                graphicsPath.AddEllipse(rectangle);
                using (PathGradientBrush pathGradientBrush = new PathGradientBrush(graphicsPath))
                {
                    pathGradientBrush.CenterColor = Color.FromArgb(0, 100, 251);
                    pathGradientBrush.CenterPoint = new PointF(control.Width / 2, control.Height / 2);
                    pathGradientBrush.SurroundColors = new Color[] { control.BackColor };
                    g.FillPath(pathGradientBrush, graphicsPath);
                }
            }
        }

        private void FormShow_SizeChanged(object sender, EventArgs e)
        {
            Refresh();
        }

        bool L灯 = false;
        bool R灯 = false;
        private void label_camL_Paint(object sender, PaintEventArgs e)
        {
            灯(sender, e, L灯);
        }
        private void label_camR_Paint(object sender, PaintEventArgs e)
        {
            灯(sender, e, R灯);
        }
        void 灯(object sender, PaintEventArgs e, bool 开关)
        {
            Control control = (Control)sender;
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;//抗锯齿
            using (GraphicsPath graphicsPath = new GraphicsPath())
            {
                graphicsPath.AddEllipse(control.ClientRectangle);
                using (PathGradientBrush pathGradientBrush = new PathGradientBrush(graphicsPath))
                {
                    pathGradientBrush.CenterPoint = new PointF(control.Width / 2, control.Height / 2);
                    pathGradientBrush.CenterColor = 开关 ? Color.LightGreen : Color.LightGray;
                    pathGradientBrush.SurroundColors = 开关 ? new Color[] { Color.Green } : new Color[] { Color.Gray };
                    g.FillPath(pathGradientBrush, graphicsPath);
                }
            }
        }

        private void panel_Title_Paint(object sender, PaintEventArgs e)
        {
            Control control = (Control)sender;
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;//抗锯齿
            float f = 3;

            using (GraphicsPath graphicsPath = new GraphicsPath())
            {
                graphicsPath.AddLines(new PointF[] { new PointF(0, 0), new PointF(control.Width * 0.05f, control.Height), new PointF(control.Width * 0.95f, control.Height), new PointF(control.Width, 0) });
                graphicsPath.AddLines(new PointF[] { new PointF(f, 0), new PointF(control.Width * 0.05f + f, control.Height - f), new PointF(control.Width * 0.95f - f, control.Height - f), new PointF(control.Width - f, 0) });
                using (LinearGradientBrush brush = new LinearGradientBrush(new PointF(0, 0), new PointF(f, 0), Color.FromArgb(0, 77, 191), control.BackColor))
                {
                    g.FillPath(brush, graphicsPath);
                }
            }

            using (GraphicsPath graphicsPath2 = new GraphicsPath())
            {
                graphicsPath2.AddLines(new PointF[] { new PointF(control.Width * 0.1f, 0), new PointF(control.Width * 0.05f, control.Height - f), new PointF(control.Width * 0.95f, control.Height - f), new PointF(control.Width * 0.9f, 0) });
                using (LinearGradientBrush brush2 = new LinearGradientBrush(new PointF(0, control.Height * 0.8f), new PointF(0, 0), Color.FromArgb(0, 77, 191), control.BackColor))
                {
                    brush2.WrapMode = WrapMode.TileFlipX;
                    g.FillPath(brush2, graphicsPath2);
                }
            }

            //GC.Collect();
        }
        bool isMove = true;
        private void panel10_MouseDown(object sender, MouseEventArgs e)
        {
            if (isMove)
            {
                FormTool.MouseDown_MoveForm(this);
            }
        }
        private void FormShow_MouseDown(object sender, MouseEventArgs e)
        {
            if (isMove)
            {
                FormTool.MouseDown_ResizeForm(this);
            }
        }
        private void ResizeForm(object sender, MouseEventArgs e)
        {
            if (isMove)
            {
                Control control = (Control)sender;
                //MouseEventArgs mouseEventArgs = new MouseEventArgs(e.Button, e.Clicks, control.Location.X + e.X, control.Location.Y + e.Y, e.Delta);
                //FormTool.MouseMove(this, mouseEventArgs);

                if (int.TryParse(control.Tag.ToString(), out int n))
                {
                    FormTool.formResizeMode = 0xf000 + n;
                }
            }
        }
        private void MinimizeBox_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        public void MaximizeBox_Click(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Normal)
            {
                SetFormWindowState(FormWindowState.Maximized);
            }
            else if (WindowState == FormWindowState.Maximized)
            {
                SetFormWindowState(FormWindowState.Normal);
            }
        }

        public void SetFormWindowState(FormWindowState state)
        {
            if (state == FormWindowState.Normal)
            {
                label_MaximizeBox.Text = "□";
                isMove = true;
                最大化ToolStripMenuItem.Text = $"{Resources.LanguageDic.Maximize}";
            }
            else if (state == FormWindowState.Maximized)
            {
                label_MaximizeBox.Text = "❐";
                isMove = false;
                最大化ToolStripMenuItem.Text = $"{Resources.LanguageDic.Restore}";
            }
            WindowState = state;
        }

        private void 最小化ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MinimizeBox_Click(null, null);
        }

        private void 最大化ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MaximizeBox_Click(null, null);
        }
        private void 切换屏幕ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string DeviceName = Screen.FromControl(this).DeviceName;
            int index = 0;
            for (int i = 0; i < Screen.AllScreens.Length; i++)
            {
                if (Screen.AllScreens[i].DeviceName == DeviceName)
                {
                    index = i;
                    break;
                }
            }
            index += 1;
            if (index >= Screen.AllScreens.Length)
            {
                index = 0;
            }
            SetFormWindowState(FormWindowState.Normal);
            Left = Screen.AllScreens[index].Bounds.Left;
            Top = Screen.AllScreens[index].Bounds.Top;
            SetFormWindowState(FormWindowState.Maximized);
        }

        private void panel10_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                MaximizeBox_Click(null, null);
            }
        }
        private void CloseBox_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void MinimizeBox_MouseEnter(object sender, EventArgs e)
        {
            Control control = (Control)sender;
            Color color = Color.FromArgb(130, 180, 255);
            if (control is Label)
            {
                control.Parent.BackColor = color;
            }
            else
            {
                control.BackColor = color;
            }
        }

        private void MinimizeBox_MouseLeave(object sender, EventArgs e)
        {
            Control control = (Control)sender;
            Color color = Color.Transparent;
            if (control is Label)
            {
                control.Parent.BackColor = color;
            }
            else
            {
                control.BackColor = color;
            }
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            //panel1.Refresh();
            if (this.IsHandleCreated)
            {
                BeginInvoke(new Action(() =>
                {
                    label_time.Text = DateTime.Now.ToString("G");
                }));
            }
        }
        private void panel_Top_Paint(object sender, PaintEventArgs e)
        {
            Control control = (Control)sender;
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;//抗锯齿

            using (GraphicsPath graphicsPath = new GraphicsPath())
            {
                graphicsPath.AddRectangle(control.ClientRectangle);
                using (LinearGradientBrush brush = new LinearGradientBrush(control.ClientRectangle, control.BackColor, Color.FromArgb(0, 50, 125), LinearGradientMode.BackwardDiagonal))
                {
                    g.FillPath(brush, graphicsPath);
                }
            }

            using (GraphicsPath graphicsPath2 = new GraphicsPath())
            {
                float f = 15;
                float x_1 = panel_MinimizeBox.Location.X + control.Height;
                float x0 = panel_MinimizeBox.Location.X;
                float x1 = panel_MinimizeBox.Location.X - control.Height;
                float x2 = panel_MinimizeBox.Location.X - control.Height * 2;
                graphicsPath2.StartFigure();
                graphicsPath2.AddCurve(new PointF[] { new PointF(x2 - f, control.Height), new PointF(x1 - f, control.Height), new PointF(x0 - f, -1), new PointF(x_1 - f, -1) });
                graphicsPath2.AddCurve(new PointF[] { new PointF(x_1, -1), new PointF(x0, -1), new PointF(x1, control.Height), new PointF(x2, control.Height) });
                graphicsPath2.CloseFigure();
                using (LinearGradientBrush brush2 = new LinearGradientBrush(new PointF(0, 0), new PointF(f, 0), Color.FromArgb(0, 50, 125), Color.FromArgb(0, 50, 125)))
                {
                    g.FillPath(brush2, graphicsPath2);
                }
            }

            //g.Dispose();
            //GC.Collect();
        }

        //改为兼容一个车型，多组结果图片显示形式
        public Dictionary<string, List<ShowCarXYZ>> ShowCarXYZs = new Dictionary<string, List<ShowCarXYZ>>();
        public Dictionary<string, List<Image>> CarImages = new Dictionary<string, List<Image>>();
        //一个队列，记录当前车型的结果图片

        public List<Image> CarResultImages = new List<Image>();

        public bool isShowAll = true;

        private void FormShow_Load(object sender, EventArgs e)
        {
            label1.Text = Text;
            IsShow = true;
            try
            {
                ShowCarXYZs.Clear();
                CarImages.Clear();


                // 获取"数模图"下所有子目录，每个子目录代表一个车型
                string basePath = "数模图";
                if (!Directory.Exists(basePath))
                    return;

                string[] carFolders = Directory.GetDirectories(basePath);

                foreach (string folder in carFolders)
                {
                    string carName = Path.GetFileName(folder);

                    // --- 加载该车型下所有 xml ---
                    List<ShowCarXYZ> xyzList = new List<ShowCarXYZ>();
                    string[] xmlFiles = Directory.GetFiles(folder, "*.xml");
                    foreach (string xmlPath in xmlFiles)
                    {
                        ShowCarXYZ showCarXYZ = new ShowCarXYZ();
                        if (showCarXYZ.Load(xmlPath))
                        {
                            xyzList.Add(showCarXYZ);
                        }
                    }
                    if (xyzList.Count > 0)
                    {
                        ShowCarXYZs.Add(carName, xyzList);
                    }

                    // --- 加载该车型下所有 png ---
                    List<Image> imageList = new List<Image>();
                    string[] pngFiles = Directory.GetFiles(folder, "*.png");
                    foreach (string pngPath in pngFiles)
                    {
                        // 使用 MemoryStream 做中转，避免文件被锁定
                        byte[] bytes = File.ReadAllBytes(pngPath);
                        MemoryStream ms = new MemoryStream(bytes);
                        imageList.Add((Image)Bitmap.FromStream(ms));
                    }
                    if (imageList.Count > 0)
                    {
                        CarImages.Add(carName, imageList);
                    }
                }

            }
            catch (Exception ex)
            {


            }
        }

        private void FormShow_FormClosed(object sender, FormClosedEventArgs e)
        {
            IsShow = false;
        }

        private void FormShow_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!EnableClose)
            {
                e.Cancel = true;

                timer1.Enabled = false;
            }
        }

        public void UpData灯(string Name, bool value)
        {
            BeginInvoke(new Action(() =>
            {
                try
                {
                    if (Name == "L")
                    {
                        L灯 = value;
                        label_camL.Refresh();
                    }
                    else if (Name == "R")
                    {
                        R灯 = value;
                        label_camR.Refresh();
                    }
                }
                catch { }
            }));
        }
        string CarNameNow = string.Empty;
        string CarNumNow = string.Empty;
        public void UpDataCarInform(string carName, string carNum)
        {
            CarNameNow = carName;
            CarNumNow = carNum;
            BeginInvoke(new Action(() =>
            {
                try
                {
                    label_carName.Text = $"{Resources.LanguageDic.model_no}：" + CarNameNow;
                    label_carNum.Text = $"{Resources.LanguageDic.car_no}：" + CarNumNow;

                    CarResultImages.Clear();
                    if (CarImages.ContainsKey(CarNameNow))
                    {
                        for (int i = 0; i < CarImages[CarNameNow].Count; i++)
                        {
                            CarResultImages.Add((Image)CarImages[CarNameNow][i].Clone());
                        }
                    }
                    

                    comboBox_showPictureID.Items.Clear();
                    for (int i = 0; i < CarImages[CarNameNow].Count; i++)
                    {
                        comboBox_showPictureID.Items.Add($"{i + 1}");
                    }
                    comboBox_showPictureID.SelectedIndex = 0;
                }
                catch { }
            }));
        }
        public void UpDataCamImage(HImage hImage, string pointName)
        {
            HImage _hImage = hImage != null ? hImage.Clone() : null;
            BeginInvoke(new Action(() =>
            {
                try
                {
                    if (pointName == "Clear")
                    {
                        pictureBox1.Image = null;
                        pictureBox2.Image = null;
                        pictureBox3.Image = null;
                        pictureBox4.Image = null;
                        pictureBox5.Image = null;
                        pictureBox6.Image = null;
                        pictureBox7.Image = null;
                        pictureBox8.Image = null;
                        pictureBox9.Image = null;
                        pictureBox10.Image = null;
                        return;
                    }
                    if (_hImage == null) return;
                    Bitmap image = HImage2Bitmap(_hImage);
                    _hImage?.Dispose();
                    switch (pointName)
                    {
                        case "L1":
                            pictureBox1.Image = image;
                            break;
                        case "L2":
                            pictureBox2.Image = image;
                            break;
                        case "L3":
                            pictureBox3.Image = image;
                            break;
                        case "L4":
                            pictureBox4.Image = image;
                            break;
                        case "L5":
                            pictureBox5.Image = image;
                            break;
                        case "R1":
                            pictureBox6.Image = image;
                            break;
                        case "R2":
                            pictureBox7.Image = image;
                            break;
                        case "R3":
                            pictureBox8.Image = image;
                            break;
                        case "R4":
                            pictureBox9.Image = image;
                            break;
                        case "R5":
                            pictureBox10.Image = image;
                            break;
                        case "Clear":
                            pictureBox1.Image = null;
                            pictureBox2.Image = null;
                            pictureBox3.Image = null;
                            pictureBox4.Image = null;
                            pictureBox5.Image = null;
                            pictureBox6.Image = null;
                            pictureBox7.Image = null;
                            pictureBox8.Image = null;
                            pictureBox9.Image = null;
                            pictureBox10.Image = null;
                            break;
                        default:
                            break;
                    }
                }
                catch { }
            }));
        }

        //public void UpDataXYZ(Point3D point3D, string pointName)
        //{
        //    BeginInvoke(new Action(() =>
        //    {
        //        try
        //        {
        //            //GC.Collect();
        //            //GC.WaitForPendingFinalizers();



        //            if (!ShowCarXYZs.ContainsKey(CarNameNow) || !ShowCarXYZs[CarNameNow].Points.ContainsKey(pointName)) return;
        //            if (pictureBox11.Image == null) return;
        //            Image image = (Image)pictureBox11.Image.Clone();


        //            using (Graphics g = Graphics.FromImage(image))
        //            {
        //                g.SmoothingMode = SmoothingMode.AntiAlias;//抗锯齿

        //                if (isShowAll)
        //                {
        //                    Rectangle rectangle = new Rectangle(ShowCarXYZs[CarNameNow].Points[pointName].Location.X, ShowCarXYZs[CarNameNow].Points[pointName].Location.Y, 120, 70);
        //                    //背景
        //                    g.FillRectangle(new SolidBrush(Color.LightGreen), rectangle);
        //                    PointF connection = new PointF();
        //                    switch (ShowCarXYZs[CarNameNow].Points[pointName].Connection)
        //                    {
        //                        case 1:
        //                            connection.X = rectangle.Left;
        //                            connection.Y = rectangle.Top;
        //                            break;
        //                        case 2:
        //                            connection.X = (rectangle.Left + rectangle.Right) / 2;
        //                            connection.Y = rectangle.Top;
        //                            break;
        //                        case 3:
        //                            connection.X = rectangle.Right;
        //                            connection.Y = rectangle.Top;
        //                            break;
        //                        case 4:
        //                            connection.X = rectangle.Left;
        //                            connection.Y = (rectangle.Top + rectangle.Bottom) / 2;
        //                            break;
        //                        case 5:
        //                            connection.X = rectangle.Right;
        //                            connection.Y = (rectangle.Top + rectangle.Bottom) / 2;
        //                            break;
        //                        case 6:
        //                            connection.X = rectangle.Left;
        //                            connection.Y = rectangle.Bottom;
        //                            break;
        //                        case 7:
        //                            connection.X = (rectangle.Left + rectangle.Right) / 2;
        //                            connection.Y = rectangle.Bottom;
        //                            break;
        //                        case 8:
        //                            connection.X = rectangle.Right;
        //                            connection.Y = rectangle.Bottom;
        //                            break;
        //                        default:
        //                            connection.X = (rectangle.Left + rectangle.Right) / 2;
        //                            connection.Y = (rectangle.Top + rectangle.Bottom) / 2;
        //                            break;
        //                    }
        //                    //连接线
        //                    g.DrawLine(new Pen(Color.LightGreen), ShowCarXYZs[CarNameNow].Points[pointName].Point, connection);
        //                    //边框
        //                    g.DrawRectangle(new Pen(Color.Green), rectangle);
        //                    //文字
        //                    g.DrawString($"X:{point3D.X:0.00}\nY:{point3D.Y:0.00}\nZ:{point3D.Z:0.00}", new Font(new FontFamily("Arial"), 16, FontStyle.Italic), new SolidBrush(Color.Black), rectangle, StringFormat.GenericDefault);

        //                }
        //                else 
        //                {
        //                    Rectangle rectangle = new Rectangle(ShowCarXYZs[CarNameNow].Points[pointName].Location.X, ShowCarXYZs[CarNameNow].Points[pointName].Location.Y, 60, 25);
        //                    //背景
        //                    g.FillRectangle(new SolidBrush(Color.LightGreen), rectangle);
        //                    PointF connection = new PointF();
        //                    switch (ShowCarXYZs[CarNameNow].Points[pointName].Connection)
        //                    {
        //                        case 1:
        //                            connection.X = rectangle.Left;
        //                            connection.Y = rectangle.Top;
        //                            break;
        //                        case 2:
        //                            connection.X = (rectangle.Left + rectangle.Right) / 2;
        //                            connection.Y = rectangle.Top;
        //                            break;
        //                        case 3:
        //                            connection.X = rectangle.Right;
        //                            connection.Y = rectangle.Top;
        //                            break;
        //                        case 4:
        //                            connection.X = rectangle.Left;
        //                            connection.Y = (rectangle.Top + rectangle.Bottom) / 2;
        //                            break;
        //                        case 5:
        //                            connection.X = rectangle.Right;
        //                            connection.Y = (rectangle.Top + rectangle.Bottom) / 2;
        //                            break;
        //                        case 6:
        //                            connection.X = rectangle.Left;
        //                            connection.Y = rectangle.Bottom;
        //                            break;
        //                        case 7:
        //                            connection.X = (rectangle.Left + rectangle.Right) / 2;
        //                            connection.Y = rectangle.Bottom;
        //                            break;
        //                        case 8:
        //                            connection.X = rectangle.Right;
        //                            connection.Y = rectangle.Bottom;
        //                            break;
        //                        default:
        //                            connection.X = (rectangle.Left + rectangle.Right) / 2;
        //                            connection.Y = (rectangle.Top + rectangle.Bottom) / 2;
        //                            break;
        //                    }
        //                    //连接线
        //                    g.DrawLine(new Pen(Color.LightGreen), ShowCarXYZs[CarNameNow].Points[pointName].Point, connection);
        //                    //边框
        //                    g.DrawRectangle(new Pen(Color.Green), rectangle);
        //                    //文字
        //                    g.DrawString($"{pointName}", new Font(new FontFamily("Arial"), 16, FontStyle.Italic), new SolidBrush(Color.Black), rectangle, StringFormat.GenericDefault);
        //                }
                        
        //            }

        //            if (pictureBox11.Image != null)
        //            {
        //                pictureBox11.Image.Dispose();
        //            }
        //            pictureBox11.Image = image;
        //        }
        //        catch (Exception ex) { Console.WriteLine(ex.Message); }
        //    }));
        //}
       
        public void UpDataXYZ(Point3D point3D, string pointName)
        {
            BeginInvoke(new Action(() =>
            {
                try
                {
                    // 检查当前车型是否存在
                    if (!ShowCarXYZs.ContainsKey(CarNameNow)) return;

                    List<ShowCarXYZ> xyzList = ShowCarXYZs[CarNameNow];


                    int currentIndex = comboBox_showPictureID.SelectedIndex;


                    // 遍历当前车型的每一组 ShowCarXYZ
                    for (int i = 0; i < xyzList.Count; i++)
                    {
                        ShowCarXYZ xyz = xyzList[i];

                        // 该组不包含此点，跳过
                        if (!xyz.Points.ContainsKey(pointName)) continue;
                        // 索引越界保护
                        if (i >= CarResultImages.Count) continue;
                        // 当前结果图片为空，跳过
                        if (CarResultImages[i] == null) continue;

                        // 克隆当前结果图片（保留之前已绘制的标注）
                        Image image = (Image)CarResultImages[i].Clone();

                        using (Graphics g = Graphics.FromImage(image))
                        {
                            g.SmoothingMode = SmoothingMode.AntiAlias;

                            if (isShowAll)
                            {
                                Rectangle rectangle = new Rectangle(
                                    xyz.Points[pointName].Location.X,
                                    xyz.Points[pointName].Location.Y,
                                    120, 95);

                                // 背景
                                g.FillRectangle(new SolidBrush(Color.LightGreen), rectangle);

                                PointF connection = new PointF();
                                switch (xyz.Points[pointName].Connection)
                                {
                                    case 1:
                                        connection.X = rectangle.Left;
                                        connection.Y = rectangle.Top;
                                        break;
                                    case 2:
                                        connection.X = (rectangle.Left + rectangle.Right) / 2;
                                        connection.Y = rectangle.Top;
                                        break;
                                    case 3:
                                        connection.X = rectangle.Right;
                                        connection.Y = rectangle.Top;
                                        break;
                                    case 4:
                                        connection.X = rectangle.Left;
                                        connection.Y = (rectangle.Top + rectangle.Bottom) / 2;
                                        break;
                                    case 5:
                                        connection.X = rectangle.Right;
                                        connection.Y = (rectangle.Top + rectangle.Bottom) / 2;
                                        break;
                                    case 6:
                                        connection.X = rectangle.Left;
                                        connection.Y = rectangle.Bottom;
                                        break;
                                    case 7:
                                        connection.X = (rectangle.Left + rectangle.Right) / 2;
                                        connection.Y = rectangle.Bottom;
                                        break;
                                    case 8:
                                        connection.X = rectangle.Right;
                                        connection.Y = rectangle.Bottom;
                                        break;
                                    default:
                                        connection.X = (rectangle.Left + rectangle.Right) / 2;
                                        connection.Y = (rectangle.Top + rectangle.Bottom) / 2;
                                        break;
                                }

                                // 连接线
                                g.DrawLine(new Pen(Color.LightGreen),
                                    xyz.Points[pointName].Point, connection);
                                // 边框
                                g.DrawRectangle(new Pen(Color.Green), rectangle);
                                // 文字
                                g.DrawString(
                                    
                                    $"{pointName}\nX:{point3D.X:0.00}\nY:{point3D.Y:0.00}\nZ:{point3D.Z:0.00}",
                                    new Font(new FontFamily("Arial"), 16, FontStyle.Italic),
                                    new SolidBrush(Color.Black),
                                    rectangle,
                                    StringFormat.GenericDefault);
                            }
                            else
                            {
                                Rectangle rectangle = new Rectangle(
                                    xyz.Points[pointName].Location.X,
                                    xyz.Points[pointName].Location.Y,
                                    60, 25);

                                // 背景
                                g.FillRectangle(new SolidBrush(Color.LightGreen), rectangle);

                                PointF connection = new PointF();
                                switch (xyz.Points[pointName].Connection)
                                {
                                    case 1:
                                        connection.X = rectangle.Left;
                                        connection.Y = rectangle.Top;
                                        break;
                                    case 2:
                                        connection.X = (rectangle.Left + rectangle.Right) / 2;
                                        connection.Y = rectangle.Top;
                                        break;
                                    case 3:
                                        connection.X = rectangle.Right;
                                        connection.Y = rectangle.Top;
                                        break;
                                    case 4:
                                        connection.X = rectangle.Left;
                                        connection.Y = (rectangle.Top + rectangle.Bottom) / 2;
                                        break;
                                    case 5:
                                        connection.X = rectangle.Right;
                                        connection.Y = (rectangle.Top + rectangle.Bottom) / 2;
                                        break;
                                    case 6:
                                        connection.X = rectangle.Left;
                                        connection.Y = rectangle.Bottom;
                                        break;
                                    case 7:
                                        connection.X = (rectangle.Left + rectangle.Right) / 2;
                                        connection.Y = rectangle.Bottom;
                                        break;
                                    case 8:
                                        connection.X = rectangle.Right;
                                        connection.Y = rectangle.Bottom;
                                        break;
                                    default:
                                        connection.X = (rectangle.Left + rectangle.Right) / 2;
                                        connection.Y = (rectangle.Top + rectangle.Bottom) / 2;
                                        break;
                                }

                                // 连接线
                                g.DrawLine(new Pen(Color.LightGreen),
                                    xyz.Points[pointName].Point, connection);
                                // 边框
                                g.DrawRectangle(new Pen(Color.Green), rectangle);
                                // 文字
                                g.DrawString(
                                    $"{pointName}",
                                    new Font(new FontFamily("Arial"), 16, FontStyle.Italic),
                                    new SolidBrush(Color.Black),
                                    rectangle,
                                    StringFormat.GenericDefault);
                            }
                        }

                        // 替换对应的结果图片
                        CarResultImages[i].Dispose();
                        CarResultImages[i] = image;

                        // 如果 pictureBox11 正在显示这张图，同步更新
                        if (i == currentIndex)
                        {
                            pictureBox11.Image.Dispose();
                            pictureBox11.Image = (Image)CarResultImages[i].Clone();
                        }
                    }

                    // 触发界面刷新，显示更新后的 CarResultImages
                    // 根据你的实际 UI 结构调用，例如：
                    // RefreshResultDisplay();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }));
        }

        private Bitmap HImage2Bitmap(HImage image)
        {
            IntPtr r, g, b;
            HTuple conut = image.CountChannels();
            g = image.GetImagePointer1(out string type, out int w, out int h);
            r = g;
            b = g;
            if (conut.D > 1)
            {
                // 获取存放r，g，b值的指针
                image.GetImagePointer3(out r, out g, out b, out type, out w, out h);
            }

            byte[] red = new byte[w * h];
            byte[] green = new byte[w * h];
            byte[] blue = new byte[w * h];
            // 将指针指向地址的值取出来放到byte数组中
            Marshal.Copy(r, red, 0, w * h);
            Marshal.Copy(g, green, 0, w * h);
            Marshal.Copy(b, blue, 0, w * h);

            Bitmap bitmap2 = new Bitmap(w, h, PixelFormat.Format32bppRgb);
            Rectangle rect2 = new Rectangle(0, 0, w, h);
            BitmapData bitmapData2 = bitmap2.LockBits(rect2, ImageLockMode.ReadWrite, PixelFormat.Format32bppRgb);
            unsafe
            {
                byte* bptr2 = (byte*)bitmapData2.Scan0;
                for (int i = 0; i < w * h; i++)
                {
                    bptr2[i * 4] = blue[i];
                    bptr2[i * 4 + 1] = green[i];
                    bptr2[i * 4 + 2] = red[i];
                    bptr2[i * 4 + 3] = 255;
                }
            }
            bitmap2.UnlockBits(bitmapData2);
            return bitmap2;
        }

        private void panel_bian_Paint(object sender, PaintEventArgs e)
        {
            Control control = (Control)sender;
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;//抗锯齿
            using (GraphicsPath graphicsPath = new GraphicsPath())
            {
                graphicsPath.AddRectangle(control.ClientRectangle);
                using (PathGradientBrush pathGradientBrush = new PathGradientBrush(graphicsPath))
                {
                    pathGradientBrush.CenterColor = Color.FromArgb(0, 100, 251);
                    pathGradientBrush.CenterPoint = new PointF(control.Width / 2, control.Height / 2);
                    pathGradientBrush.SurroundColors = new Color[] { control.BackColor };
                    g.FillPath(pathGradientBrush, graphicsPath);
                }
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            isShowAll = checkBox_show_all.Checked;
        }

        private void comboBox_showPictureID_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                int showImageID = comboBox_showPictureID.SelectedIndex;


                if (pictureBox11.Image != null)
                {
                    pictureBox11.Image.Dispose();
                }
                if (CarResultImages.Count > showImageID)
                {
                    pictureBox11.Image = (Image)CarResultImages[showImageID].Clone();

                }
                else
                {
                    pictureBox11.Image = null;
                }
            }
            catch (Exception ex)
            {
            }
         
        }

    }
}
