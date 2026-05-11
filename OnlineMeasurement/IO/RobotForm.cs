using HalconDotNet;
using HslCommunication.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OnlineMeasurement.IO
{
    public partial class RobotForm : Form
    {
        IRobot signal = null;
        bool isAlter = false;

        public RobotForm(IRobot signal)
        {
            InitializeComponent();

            // 初始化界面
            GeneralFunc.ChangeLanguateFun(typeof(RobotForm), this);
            groupBox1.Enabled = false;

            this.signal = signal;
        }

        private void RobotForm_Load(object sender, EventArgs e)
        {
            signal.Load();

            textBoxRobotIP.Text = signal.Param.IpAddress;
            numericUpDownPort.Value = signal.Param.Port;

            textBoxRobotIP.TextChanged += UpData;
            numericUpDownPort.ValueChanged += UpData;

        }

        private void UpData(object sender, EventArgs e)
        {
            signal.Param.IpAddress = textBoxRobotIP.Text;
            signal.Param.Port = (int)numericUpDownPort.Value;

            isAlter = true;
        }

        private void RobotForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (isAlter)
            {
                DialogResult dialogResult = MessageBox.Show($"{Resources.LanguageDic.is_save_para}？", $"{Resources.LanguageDic.tip}", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
                if (dialogResult == DialogResult.Yes)
                {
                    if (!signal.Save())
                    {
                        MessageBox.Show($"{Resources.LanguageDic.save_fail}！！", $"{Resources.LanguageDic.tip}", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        e.Cancel = true;
                        return;
                    }
                    else
                    {
                        isAlter = false;
                    }
                }
                else if (dialogResult == DialogResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
            }
            signal.Close();

        }

        private void button_Open_Click(object sender, EventArgs e)
        {
            if (button_Open.Enabled)//没有效果
            {
                button_Open.Enabled = false;
                groupBox1.Enabled = true;

                if (signal.Open())
                {
                    ShowMessage($"{Resources.LanguageDic.connection_successful}");
                    button_Close.Enabled = true;
                    groupBox1.Enabled = true;

                }
                else
                {
                    ShowMessage($"{Resources.LanguageDic.connection_failed}");
                    button_Open.Enabled = true;
                    groupBox1.Enabled = false;

                }
            }
        }

        private void button_Close_Click(object sender, EventArgs e)
        {
            signal.Close();
            button_Close.Enabled = false;
            button_Open.Enabled = true;
            groupBox1.Enabled = false;

            ShowMessage($"{Resources.LanguageDic.close_success}");
        }


        void ShowMessage(string message)
        {
            textBoxLog.Text += DateTime.Now.TimeOfDay.ToString("hh\\:mm\\:ss") + "  " + message + "\r\n";
        }

        private void buttonGetRobotPose_Click(object sender, EventArgs e)
        {
            signal.ReadPose(out HPose hPose);
            signal.ReadAngle(out HTuple hAngel);

            //显示
            textBoxRobotX.Text = $"{hPose[0].D * 1000:G6}";
            textBoxRobotY.Text = $"{hPose[1].D * 1000:G6}";
            textBoxRobotZ.Text = $"{hPose[2].D * 1000:G6}";
            textBoxRobotRX.Text = $"{hPose[3].D:G6}";
            textBoxRobotRY.Text = $"{hPose[4].D:G6}";
            textBoxRobotRZ.Text = $"{hPose[5].D:G6}";

            textBoxRobotA1.Text = $"{hAngel[0].D:G6}";
            textBoxRobotA2.Text = $"{hAngel[1].D:G6}";
            textBoxRobotA3.Text = $"{hAngel[2].D:G6}";
            textBoxRobotA4.Text = $"{hAngel[3].D:G6}";
            textBoxRobotA5.Text = $"{hAngel[4].D:G6}";
            textBoxRobotA6.Text = $"{hAngel[5].D:G6}";
        }
    }
}
