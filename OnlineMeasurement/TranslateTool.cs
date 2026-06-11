using System;
using System.ComponentModel;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;
using System.IO;
using static System.Windows.Forms.ComboBox;

namespace OnlineMeasurement
{
    public static class GeneralFunc
    {

        private static void ChangeControlLanguateFun(ComponentResourceManager resources, Control control)
        {
            //将资源与控件对应
            resources.ApplyResources(control, control.Name);
            if (control.HasChildren)//子控件，比如组合框GroupBox里的控件
            {
                foreach (Control controls in control.Controls)
                    ChangeControlLanguateFun(resources, controls);
            }
            if (control is MenuStrip)//菜单栏控件
            {
                MenuStrip ms = (MenuStrip)control;
                if (ms.Items.Count > 0)
                {
                    //遍历菜单
                    foreach (ToolStripMenuItem ts in ms.Items)//主菜单
                    {
                        resources.ApplyResources(ts, ts.Name);
                        if (ts.DropDownItems.Count > 0)
                        {
                            foreach (ToolStripMenuItem tts in ts.DropDownItems)//子菜单
                            {
                                resources.ApplyResources(tts, tts.Name);
                            }
                        }
                    }
                }
            }

            if (control is DataGridView)//菜单栏控件
            {
                DataGridView dgv = (DataGridView)control;
                if (dgv.Columns.Count > 0)
                {
                    //遍历菜单
                    foreach (DataGridViewTextBoxColumn ts in dgv.Columns)//主菜单
                    {
                        resources.ApplyResources(ts, ts.Name);
                    }
                }
            }

            //if (control is ComboBox)//下拉框控件
            //{
            //    ComboBox dgv = (ComboBox)control;
            //    if (dgv.Items.Count > 0)
            //    {
            //        //遍历菜单
            //        foreach (ObjectCollection ts in dgv.Items)//主菜单
            //        {
            //            resources.ApplyResources(ts, ts.);
            //        }
            //    }
            //}
        }

        #region  界面翻译
        public static void ChangeLanguateFun(Type t, Form f)
        {
            int currentLcid = 2052; //1033代表英文，2052代表中文
            //if (GlobalVarAndFunc.LANGUAGE_ID == 1)
            //{
            //    currentLcid = 1033;
            //}
            switch (GlobalVarAndFunc.LANGUAGE_ID)
            {
                case 0:
                    currentLcid = 2052;
                    break;
                case 1:
                    currentLcid = 1033;
                    break;
                case 2:
                    currentLcid = 1036;
                    break;

            }

            Thread.CurrentThread.CurrentUICulture = new CultureInfo(currentLcid);
            ComponentResourceManager resources = new ComponentResourceManager(t);
            resources.ApplyResources(f, "$this");//窗体标题


            foreach (Control control in f.Controls)//循环当前界面所有的控件
            {
                ChangeControlLanguateFun(resources, control);
            }
            //刷新窗体，有时窗体标题无法切换成功，需要刷新一下
            f.Refresh();
        }
        #endregion
    }
    
    public static class GlobalVarAndFunc
    {


        public static int LANGUAGE_ID = 1; //0默认中文，1英文，2为法语

        public static bool SHOW_MESSAGE = false; 

        public static void ReadLanguageID()
        {
            string fPath = "Data\\LanguageID";
            if (File.Exists(fPath))
            {
                LANGUAGE_ID = int.Parse(File.ReadAllText(fPath));
            }
        }

        public static void WriteLanguageID()
        {
            if (!Directory.Exists("Data"))
            {
                Directory.CreateDirectory("Data");
            }
            File.WriteAllText("Data\\LanguageID", GlobalVarAndFunc.LANGUAGE_ID.ToString());
        }

    }
}
