using HalconDotNet;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OnlineMeasurement
{
    /// <summary>
    /// 此类用于实现一些可用于Form窗体的功能操作
    /// </summary>
    public class FormTool
    {

        //using System.Runtime.InteropServices;  
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();
        [DllImport("user32.dll")]
        public static extern bool SendMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);

        #region  无边框窗体的移动

        // 在Form的MouseDown(object sender, MouseEventArgs e)事件中调用此函数，可控制窗体移动
        public static void MouseDown_MoveForm(Form form)
        {
            //常量  
            int WM_SYSCOMMAND = 0x0112;

            //窗体移动  
            int SC_MOVE = 0xF010;
            int HTCAPTION = 0x0002;

            ReleaseCapture();
            SendMessage(form.Handle, WM_SYSCOMMAND, SC_MOVE + HTCAPTION, 0);

            saveFormInfo(form);        // 保存界面信息
        }

        #endregion


        #region  无边框窗体的尺寸缩放

        public static int formResizeMode = 0;       // 此处指定窗体尺寸修改的模式

        // 在Form的MouseDown(object sender, MouseEventArgs e)事件中调用此函数，可调整窗体大小
        public static void MouseDown_ResizeForm(Form form)
        {
            //常量
            int WM_SYSCOMMAND = 0x0112;

            //改变窗体大小
            //int WMSZ_LEFT = 0xF001;
            //int WMSZ_RIGHT = 0xF002;
            //int WMSZ_TOP = 0xF003;
            //int WMSZ_TOPLEFT = 0xF004;
            //int WMSZ_TOPRIGHT = 0xF005;
            //int WMSZ_BOTTOM = 0xF006;
            //int WMSZ_BOTTOMLEFT = 0xF007;
            //int WMSZ_BOTTOMRIGHT = 0xF008;

            if (formResizeMode == 0) return;    // 若无缩放模式，则返回

            ReleaseCapture();
            SendMessage(form.Handle, WM_SYSCOMMAND, formResizeMode, 0);
            //SendMessage(form.Handle, WM_SYSCOMMAND, WMSZ_BOTTOM, 0);
            //SendMessage(this.Handle, WM_SYSCOMMAND, WMSZ_TOP, 0);
        }



        // 在处于最前端的控件的MouseMove事件中调用此函数，根据鼠标位置设置对应的缩放模式

        public static void MouseMove(object sender, MouseEventArgs e)
        {
            Control ctrl = (Control)sender;             // 获取鼠标处的控件
            Rectangle client = ctrl.ClientRectangle;    // 获取控件尺寸

            setSizeMode(client, new Point(e.X, e.Y), 10);   // 根据鼠标在区域client中的位置设置缩放模式
                                                            //MessageBox.Show("鼠标坐标:" + e.X + "," + e.Y + " 窗体尺寸" + client.Width + "," + client.Height);
        }

        // 根据坐标 x,y 在rect上的位置控制尺寸调节模式
        private static void setSizeMode(Rectangle R, Point P, int W)
        {
            formResizeMode = getSizeMode(R, P, W);
        }

        // 根据坐标 x,y 在rect上的位置控制尺寸调节模式
        private static int getSizeMode(Rectangle R, Point P, int W)
        {
            //改变窗体大小相关参数
            int WMSZ_LEFT = 0xF001;
            int WMSZ_RIGHT = 0xF002;
            int WMSZ_TOP = 0xF003;
            int WMSZ_TOPLEFT = 0xF004;
            int WMSZ_TOPRIGHT = 0xF005;
            int WMSZ_BOTTOM = 0xF006;
            int WMSZ_BOTTOMLEFT = 0xF007;
            int WMSZ_BOTTOMRIGHT = 0xF008;

            // 中间区域
            Rectangle main = new Rectangle(R.Left + W, R.Top + W, R.Width - 2 * W, R.Height - 2 * W);
            if (main.Contains(P)) return 0;

            // 左侧区域
            Rectangle LeftRect = new Rectangle(R.Left + W, R.Top + W, W, R.Height - 2 * W);
            if (LeftRect.Contains(P)) return WMSZ_LEFT;

            // 右侧区域
            Rectangle RightRect = new Rectangle(R.Right - W, R.Top + W, W, R.Height - 2 * W);
            if (RightRect.Contains(P)) return WMSZ_RIGHT;

            // 顶部区域
            Rectangle TopRect = new Rectangle(R.Left + W, R.Top, R.Width - 2 * W, W);
            if (TopRect.Contains(P)) return WMSZ_TOP;

            // 底部区域
            Rectangle BottomRect = new Rectangle(R.Left + W, R.Bottom - W, R.Width - 2 * W, W);
            if (BottomRect.Contains(P)) return WMSZ_BOTTOM;

            // 左上区域
            Rectangle TOPLEFT = new Rectangle(R.Left, R.Top, W, W);
            if (TOPLEFT.Contains(P)) return WMSZ_TOPLEFT;

            // 右上区域
            Rectangle TOPRIGHT = new Rectangle(R.Right - W, R.Top, W, W);
            if (TOPRIGHT.Contains(P)) return WMSZ_TOPRIGHT;

            // 左下区域
            Rectangle BOTTOMLEFT = new Rectangle(R.Left, R.Bottom - W, W, W);
            if (BOTTOMLEFT.Contains(P)) return WMSZ_BOTTOMLEFT;

            // 右下区域
            Rectangle BOTTOMRIGHT = new Rectangle(R.Right - W, R.Bottom - W, W, W);
            if (BOTTOMRIGHT.Contains(P)) return WMSZ_BOTTOMRIGHT;

            return 0;
        }

        #endregion


        #region 保存和恢复窗体的尺寸和坐标信息

        static Dictionary<string, int[]> Info = new Dictionary<string, int[]>();

        // 保存窗体属性信息到注册表中
        public static void saveFormInfo(Form form)
        {
            if (form == null) return;

            String formKey = form.Text; // 获取窗体名称
            Point P = form.Location;    // 获取窗体form的坐标位置

            //string formInfo = form.Width + "_" + form.Height + "_" + P.X + "_" + P.Y;

            // 保存窗体信息到注册表
            //Tool.RegistrySave(@"Scimence\TaskManager\Set\FormInfo", formKey, formInfo);

            if (Info.ContainsKey(formKey))
            {
                Info[formKey] = new int[] { form.Width, form.Height, P.X, P.Y };
            }
            else
            {
                Info.Add(formKey, new int[] { form.Width, form.Height, P.X, P.Y });
            }
        }

        // 从注册表中获取信息并设置窗体属性
        public static void RestoreFormInfo(Form form)
        {
            if (form == null) return;

            String formKey = form.Text; // 获取窗体名称

            // 获取信息
            //string formInfo = Tool.RegistryStrValue(@"Scimence\TaskManager\Set\FormInfo", formKey);
            //if (formInfo.Equals("")) saveFormInfo(form);
            //else
            //{
            //    string[] info = formInfo.Split('_');
            //    form.Width = Tool.toInt(info[0], form.Width);
            //    form.Height = Tool.toInt(info[1], form.Height);

            //    int x = Tool.toInt(info[2], form.Location.X);
            //    int y = Tool.toInt(info[3], form.Location.Y);
            //    form.Location = new Point(x, y);
            //}

            if (Info.ContainsKey(formKey))
            {
                form.Width = Info[formKey][0];
                form.Height = Info[formKey][1];
                form.Location = new Point(Info[formKey][2], Info[formKey][3]);
            }
            else
            {
                saveFormInfo(form);
            }
        }

        #endregion


        #region 控制窗体在桌面边缘区域的自动隐藏

        private static Dictionary<string, Timer> DeactivateTimer = new Dictionary<string, Timer>();
        private static Timer getTimer(String formKey)
        {
            Timer timer;
            if (DeactivateTimer.ContainsKey(formKey)) timer = DeactivateTimer[formKey];
            else
            {
                timer = new Timer();
                DeactivateTimer.Add(formKey, timer);
            }

            return timer;
        }

        // 
        private void FloatMenu_Deactivate(object sender, EventArgs e)
        {
            MessageBox.Show("FloatMenu_Deactivate");
        }

        // 控制窗体在桌面边缘的隐藏
        public static void BorderHide(Form form)
        {
            if (form == null) return;

            String formKey = form.Text; // 获取窗体名称
            getTimer(formKey);

            // 判定窗体form在桌面中的位置隐藏逻辑
            //if

        }

        //SystemInformation.WorkingArea.Width

        #endregion


        #region 控制Form2在Form1的下方停靠

        // 记录Form的停靠信息
        public static Dictionary<string, string> dockFormInfo = new Dictionary<string, string>();

        // 设置Form2停靠于Form1的下方
        public static void setDock(Form form1, Form form2)
        {
            Rectangle Rect1 = form1.ClientRectangle;
            Rectangle Rect2 = form2.ClientRectangle;

            if (Math.Abs(Rect1.Bottom - Rect2.Bottom) < 10 && Math.Abs(Rect1.Left - Rect2.Left) < 30)
            {

            }
        }


        #endregion

        // 设置form2自动显示在form1的两边
        public static void AutoLocation_OnSide(Form form1, Form form2)
        {
            int x = form1.Location.X + form1.Width + 10;
            if (x > SystemInformation.WorkingArea.Width - form2.Width) x = form1.Location.X - form2.Width - 10;
            form2.Location = new Point(x, form1.Location.Y);
        }
    }
}
