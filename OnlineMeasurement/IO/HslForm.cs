using HalconDotNet;
using HslCommunication.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace OnlineMeasurement.IO
{
    public partial class HslForm : Form
    {
        List<Label> diNameLabels = new List<Label>();
        List<TextBox> diAddressTextBoxs = new List<TextBox>();
        List<TextBox> diValueTextBoxs = new List<TextBox>();

        List<Label> doNameLabels = new List<Label>();
        List<TextBox> doAddressTextBoxs = new List<TextBox>();
        List<TextBox> doValueTextBoxs = new List<TextBox>();

        IHsl signal = null;
        PlcParam param => signal.Param;
        public Dictionary<string, Dictionary<string, IoAddress>> camIODict = new Dictionary<string, Dictionary<string, IoAddress>>();
        Dictionary<string, IoAddress> ioDict = new Dictionary<string, IoAddress>();
        string camName = "";

        bool isAlter = false;

        public HslForm(IHsl signal, bool DA2)
        {
            InitializeComponent();

            // 初始化界面
            GeneralFunc.ChangeLanguateFun(typeof(HslForm), this);

            numericUpDownDA2.Enabled = DA2;
            comboBox_DataFormat.Items.AddRange(Enum.GetNames(typeof(DataFormat)));

            //string[] diNames = Enum.GetNames(typeof(DI));
            string[] diNames = Enum.GetNames(typeof(DI));

            for (int i = 0; i < diNames.Length; i++)
            {
                TextBox textBox = new TextBox();
                textBox.Tag = i;
                textBox.Size = new Size(80, 21);
                textBox.Location = new Point(labelInAddress.Location.X, (int)(labelInAddress.Location.Y - 5 + (labelInAddress.Size.Height * 2.5 * (i + 1))));
                diAddressTextBoxs.Add(textBox);
                this.Controls.Add(textBox);

                Label lbl = new Label();
                lbl.Text = diNames[i];
                lbl.AutoSize = true;
                lbl.Tag = i;
                lbl.Location = new Point(labelInName.Location.X, (int)(labelInName.Location.Y + (labelInName.Size.Height * 2.5 * (i + 1))));
                lbl.BackColor = Color.Transparent;
                lbl.DoubleClick += LblIn_DoubleClick;
                diNameLabels.Add(lbl);
                this.Controls.Add(lbl);

                TextBox textBoxValue = new TextBox();
                textBoxValue.Tag = i;
                textBoxValue.Size = new Size(80, 21);
                textBoxValue.Location = new Point(labelInValue.Location.X, (int)(labelInValue.Location.Y - 5 + (labelInValue.Size.Height * 2.5 * (i + 1))));
                textBoxValue.ReadOnly = true;
                diValueTextBoxs.Add(textBoxValue);
                this.Controls.Add(textBoxValue);
            }

            string[] doNames = Enum.GetNames(typeof(DO));

            for (int i = 0; i < doNames.Length; i++)
            {
                TextBox textBox = new TextBox();
                textBox.Tag = i;
                textBox.Size = new Size(80, 21);
                textBox.Location = new Point(labelOutAddress.Location.X, (int)(labelOutAddress.Location.Y - 5 + (labelOutAddress.Size.Height * 2.5 * (i + 1))));
                doAddressTextBoxs.Add(textBox);
                this.Controls.Add(textBox);

                Label lbl = new Label();
                lbl.Text = doNames[i];
                lbl.AutoSize = true;
                lbl.Tag = i;
                lbl.Location = new Point(labelOutName.Location.X, (int)(labelOutName.Location.Y + (labelOutName.Size.Height * 2.5 * (i + 1))));
                lbl.BackColor = Color.Transparent;
                lbl.DoubleClick += LblOut_DoubleClick;
                doNameLabels.Add(lbl);
                this.Controls.Add(lbl);

                TextBox textBoxValue = new TextBox();
                textBoxValue.Tag = i;
                textBoxValue.Size = new Size(80, 21);
                textBoxValue.Location = new Point(labelOutValue.Location.X, (int)(labelOutValue.Location.Y - 5 + (labelOutValue.Size.Height * 2.5 * (i + 1))));
                doValueTextBoxs.Add(textBoxValue);
                this.Controls.Add(textBoxValue);
            }

            this.signal = signal;
        }

        private void HslForm_Load(object sender, EventArgs e)
        {
            signal.Load();

            textBoxIpAddress.Text = param.IpAddress;
            numericUpDownPort.Value = param.Port;
            numericUpDownDA2.Value = param.DA2;
            comboBox_DataFormat.Text = param.DataFormat.ToString();
            checkBox_IsStringReverseByteWord.Checked = param.IsStringReverseByteWord;

            textBoxIpAddress.TextChanged += UpData;
            numericUpDownPort.ValueChanged += UpData;
            numericUpDownDA2.ValueChanged += UpData;
            comboBox_DataFormat.TextChanged += UpData;
            checkBox_IsStringReverseByteWord.CheckedChanged += UpData;

            //读取参数
            camIODict.Clear();
            comboBox_cam.Items.Clear();
            string[] camPaths = Directory.GetDirectories("Data\\Cam");
            foreach (var item in camPaths)
            {
                Dictionary<string, IoAddress> ioDict = new Dictionary<string, IoAddress>();
                string name = Path.GetFileNameWithoutExtension(item);

                string paramPath = item + "\\IoParam.xml";
                if (File.Exists(paramPath))
                {
                    List<IoAddress> ios = new List<IoAddress>();
                    XmlSerializer xml = new XmlSerializer(ios.GetType());
                    using (FileStream stream = new FileStream(paramPath, FileMode.OpenOrCreate))
                    {
                        ios = (List<IoAddress>)xml.Deserialize(stream);
                    }
                    if (ios == null)
                    {
                        ShowMessage($"PLC IO {Resources.LanguageDic.Para_read_fail}");
                    }
                    else
                    {
                        ioDict = ios.ToDictionary(n => { return n.IoName; });
                    }

                    camIODict.Add(name, ioDict);
                    comboBox_cam.Items.Add(name);
                }
                else
                {
                    ShowMessage($"{paramPath}{Resources.LanguageDic.file_not_exist}");
                }

            }

            comboBox_cam.SelectedIndex = 0;

          
        }

        private void UpData(object sender, EventArgs e)
        {
            param.IpAddress = textBoxIpAddress.Text;
            param.Port = (int)numericUpDownPort.Value;
            param.DA2 = (byte)numericUpDownDA2.Value;
            param.DataFormat = (DataFormat)Enum.Parse(typeof(DataFormat), comboBox_DataFormat.Text);
            param.IsStringReverseByteWord = checkBox_IsStringReverseByteWord.Checked;
            isAlter = true;
        }
        private void UpDataDi(object sender, EventArgs e)
        {
            if (sender is TextBox)
            {
                int i = (int)((TextBox)sender).Tag;
                if (ioDict.ContainsKey(diNameLabels[i].Text))
                {
                    ioDict[diNameLabels[i].Text].Address = diAddressTextBoxs[i].Text;
                }
                else
                {
                    ioDict.Add(diNameLabels[i].Text, new IoAddress() { IoName = diNameLabels[i].Text, Address = diAddressTextBoxs[i].Text });
                }
                isAlter = true;
            }
        }
        private void UpDataDo(object sender, EventArgs e)
        {
            if (sender is TextBox)
            {
                int i = (int)((TextBox)sender).Tag;
                if (ioDict.ContainsKey(doNameLabels[i].Text))
                {
                    ioDict[doNameLabels[i].Text].Address = doAddressTextBoxs[i].Text;
                }
                else
                {
                    ioDict.Add(doNameLabels[i].Text, new IoAddress() { IoName = doNameLabels[i].Text, Address = doAddressTextBoxs[i].Text });
                }
                isAlter = true;
            }
        }

        /// <summary>
        /// 响应输入信号标签双击，在连接已打开时按信号枚举区间选择对应数据类型读取 PLC，
        /// 成功则更新输入值文本框，失败则显示该次请求的错误码和错误信息。
        /// </summary>
        /// <param name="sender">被双击的输入标签；仅处理 Label，Tag 应为有效输入控件索引，文本须对应已配置的 DI 枚举和地址。</param>
        /// <param name="e">双击事件参数；本方法不读取该参数。</param>
        /// <remarks>
        /// 必须在界面线程调用，会同步等待 PLC 响应并更新控件；通信对象由主窗口持有，本方法不管理连接。
        /// 控件索引、枚举名称或地址配置无效时，现有转换和索引操作可能抛出异常，本方法不捕获这些配置异常。
        /// </remarks>
        private void LblIn_DoubleClick(object sender, EventArgs e)
        {
            if (signal.IsOpen)
            {
                if (sender is Label)
                {
                    Label lbl = (Label)sender;
                    int i = (int)lbl.Tag;
                    DI eIO = (DI)Enum.Parse(typeof(DI), lbl.Text);
                    if ((int)eIO < 256)
                    {
                        var result = signal.ReadBool(ioDict[eIO.ToString()].Address);
                        if (result.IsSuccess)
                        {
                            diValueTextBoxs[i].Text = result.Content.ToString();
                        }
                        else
                        {
                            ShowMessage($"{Resources.LanguageDic.signal_read_fail}，错误码：{result.ErrorCode}，错误信息：{result.Message}");
                        }
                    }
                    else if ((int)eIO < 512)
                    {
                        var result = signal.ReadUInt16(ioDict[eIO.ToString()].Address);
                        if (result.IsSuccess)
                        {
                            diValueTextBoxs[i].Text = result.Content.ToString();
                        }
                        else
                        {
                            ShowMessage($"{Resources.LanguageDic.signal_read_fail}，错误码：{result.ErrorCode}，错误信息：{result.Message}");
                        }
                    }
                    else if ((int)eIO < 768)
                    {
                        var result = signal.ReadInt16(ioDict[eIO.ToString()].Address);
                        if (result.IsSuccess)
                        {
                            diValueTextBoxs[i].Text = result.Content.ToString();
                        }
                        else
                        {
                            ShowMessage($"{Resources.LanguageDic.signal_read_fail}，错误码：{result.ErrorCode}，错误信息：{result.Message}");
                        }
                    }
                    else if ((int)eIO < 1024)
                    {
                        var result = signal.ReadUInt32(ioDict[eIO.ToString()].Address);
                        if (result.IsSuccess)
                        {
                            diValueTextBoxs[i].Text = result.Content.ToString();
                        }
                        else
                        {
                            ShowMessage($"{Resources.LanguageDic.signal_read_fail}，错误码：{result.ErrorCode}，错误信息：{result.Message}");
                        }
                    }
                    else if ((int)eIO < 1280)
                    {
                        var result = signal.ReadInt32(ioDict[eIO.ToString()].Address);
                        if (result.IsSuccess)
                        {
                            diValueTextBoxs[i].Text = result.Content.ToString();
                        }
                        else
                        {
                            ShowMessage($"{Resources.LanguageDic.signal_read_fail}，错误码：{result.ErrorCode}，错误信息：{result.Message}");
                        }
                    }
                    else if ((int)eIO < 1536)
                    {
                        var result = signal.ReadFloat(ioDict[eIO.ToString()].Address);
                        if (result.IsSuccess)
                        {
                            diValueTextBoxs[i].Text = result.Content.ToString();
                        }
                        else
                        {
                            ShowMessage($"{Resources.LanguageDic.signal_read_fail}，错误码：{result.ErrorCode}，错误信息：{result.Message}");
                        }
                    }
                    else if ((int)eIO < 1792)
                    {
                        var result = signal.ReadString(ioDict[eIO.ToString()].Address, 10);
                        if (result.IsSuccess)
                        {
                            diValueTextBoxs[i].Text = result.Content.ToString();
                        }
                        else
                        {
                            ShowMessage($"{Resources.LanguageDic.signal_read_fail}，错误码：{result.ErrorCode}，错误信息：{result.Message}");
                        }
                    }

                }
            }
        }

        /// <summary>
        /// 响应输出信号标签双击，按信号枚举区间解析文本框中的值并写入 PLC，
        /// 格式错误时显示原有提示，通信失败时显示对应请求的错误码和错误信息。
        /// </summary>
        /// <param name="sender">被双击的输出标签；仅处理 Label，Tag 应为有效输出控件索引，文本须对应已配置的 DO 枚举和地址。</param>
        /// <param name="e">双击事件参数；本方法不读取该参数。</param>
        /// <remarks>
        /// 必须在界面线程调用，同步写入会修改 PLC 信号并可能阻塞界面；字符串沿用最多十个字符的现有限制。
        /// 通信对象由主窗口持有；配置索引或枚举名称无效导致的异常沿用现有传播方式。
        /// </remarks>
        private void LblOut_DoubleClick(object sender, EventArgs e)
        {
            if (signal.IsOpen)
            {
                if (sender is Label)
                {
                    Label lbl = (Label)sender;
                    int i = (int)lbl.Tag;
                    DO eIO = (DO)Enum.Parse(typeof(DO), lbl.Text);
                    if ((int)eIO < 256)
                    {
                        if (bool.TryParse(doValueTextBoxs[i].Text, out bool value))
                        {
                            var result = signal.Write(ioDict[eIO.ToString()].Address, value);
                            if (!result.IsSuccess)
                            {
                                ShowMessage($"{Resources.LanguageDic.signal_rewrite_fail}，错误码：{result.ErrorCode}，错误信息：{result.Message}");
                            }
                        }
                        else
                        {
                            ShowMessage($"bool {Resources.LanguageDic.Incorrect_format_conversion_failed}！");
                        }
                    }
                    else if ((int)eIO < 512)
                    {
                        if (ushort.TryParse(doValueTextBoxs[i].Text, out ushort value))
                        {
                            var result = signal.Write(ioDict[eIO.ToString()].Address, value);
                            if (!result.IsSuccess)
                            {
                                ShowMessage($"{Resources.LanguageDic.signal_rewrite_fail}，错误码：{result.ErrorCode}，错误信息：{result.Message}");
                            }
                        }
                        else
                        {
                            ShowMessage($"uint16 {Resources.LanguageDic.Incorrect_format_conversion_failed}！");
                        }
                    }
                    else if ((int)eIO < 768)
                    {
                        if (short.TryParse(doValueTextBoxs[i].Text, out short value))
                        {
                            var result = signal.Write(ioDict[eIO.ToString()].Address, value);
                            if (!result.IsSuccess)
                            {
                                ShowMessage($"{Resources.LanguageDic.signal_rewrite_fail}，错误码：{result.ErrorCode}，错误信息：{result.Message}");
                            }
                        }
                        else
                        {
                            ShowMessage($"int16 {Resources.LanguageDic.Incorrect_format_conversion_failed}！");
                        }
                    }
                    else if ((int)eIO < 1024)
                    {
                        if (uint.TryParse(doValueTextBoxs[i].Text, out uint value))
                        {
                            var result = signal.Write(ioDict[eIO.ToString()].Address, value);
                            if (!result.IsSuccess)
                            {
                                ShowMessage($"{Resources.LanguageDic.signal_rewrite_fail}，错误码：{result.ErrorCode}，错误信息：{result.Message}");
                            }
                        }
                        else
                        {
                            ShowMessage($"uint32 {Resources.LanguageDic.Incorrect_format_conversion_failed}！");
                        }
                    }
                    else if ((int)eIO < 1280)
                    {
                        if (int.TryParse(doValueTextBoxs[i].Text, out int value))
                        {
                            var result = signal.Write(ioDict[eIO.ToString()].Address, (ushort)value);
                            if (!result.IsSuccess)
                            {
                                ShowMessage($"{Resources.LanguageDic.signal_rewrite_fail}，错误码：{result.ErrorCode}，错误信息：{result.Message}");
                            }
                        }
                        else
                        {
                            ShowMessage($"int32 {Resources.LanguageDic.Incorrect_format_conversion_failed}！");
                        }
                    }
                    else if ((int)eIO < 1536)
                    {
                        if (float.TryParse(doValueTextBoxs[i].Text, out float value))
                        {
                            var result = signal.Write(ioDict[eIO.ToString()].Address, value);
                            if (!result.IsSuccess)
                            {
                                ShowMessage($"{Resources.LanguageDic.signal_rewrite_fail}，错误码：{result.ErrorCode}，错误信息：{result.Message}");
                            }
                        }
                        else
                        {
                            ShowMessage($"float {Resources.LanguageDic.Incorrect_format_conversion_failed}！");
                        }
                    }
                    else if ((int)eIO < 1792)
                    {
                        var value = doValueTextBoxs[i].Text;
                        if (value.Length <= 10)
                        {
                            var result = signal.Write(ioDict[eIO.ToString()].Address, value);
                            if (!result.IsSuccess)
                            {
                                ShowMessage($"{Resources.LanguageDic.signal_rewrite_fail}，错误码：{result.ErrorCode}，错误信息：{result.Message}");
                            }
                        }
                        else
                        {
                            ShowMessage($"{Resources.LanguageDic.too_long}！");
                        }
                    }
                }
            }
        }

        private void HslForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (isAlter)
            {
                DialogResult dialogResult = MessageBox.Show($"{Resources.LanguageDic.is_save_para}？", $"{Resources.LanguageDic.tip}", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
                if (dialogResult == DialogResult.Yes)
                {
                    if (!Save()||!signal.Save())
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

        /// <summary>
        /// 响应连接按钮，按当前 PLC 参数建立连接并更新按钮状态；
        /// 连接失败时显示通信对象保存的错误码和错误信息，并恢复连接按钮。
        /// </summary>
        /// <param name="sender">触发连接操作的按钮对象；本方法不读取该参数。</param>
        /// <param name="e">按钮点击事件参数；本方法不读取该参数。</param>
        /// <remarks>
        /// 必须在界面线程调用，同步连接可能阻塞至连接超时；打开的是主窗口共用的通信对象，不创建新的 PLC 实例。
        /// </remarks>
        private void button_Open_Click(object sender, EventArgs e)
        {
            if (button_Open.Enabled)//没有效果
            {
                button_Open.Enabled = false;
                if (signal.Open())
                {
                    ShowMessage($"{Resources.LanguageDic.connection_successful}");
                    button_Close.Enabled = true;
                }
                else
                {
                    ShowMessage($"{Resources.LanguageDic.connection_failed}：{signal.ErrMsg}");
                    button_Open.Enabled = true;
                }
            }
        }

        private void button_Close_Click(object sender, EventArgs e)
        {
            signal.Close();
            button_Close.Enabled = false;
            button_Open.Enabled = true;
            ShowMessage($"{Resources.LanguageDic.close_success}");
        }
        void ShowMessage(string message)
        {
            textBoxLog.Text += DateTime.Now.TimeOfDay.ToString("hh\\:mm\\:ss") + "  " + message + "\r\n";
        }
        private void HslForm_Paint(object sender, PaintEventArgs e)
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


        public bool Save()
        {
            bool result = true;
            try
            {
                string name = camName;
                List<IoAddress> ios = ioDict.Values.ToList();
                string ioParamPath = $"Data\\Cam\\{name}\\" + "IoParam.xml";
                XmlSerializer ioXml = new XmlSerializer(ios.GetType());
                using (FileStream stream = new FileStream(ioParamPath, FileMode.Create))
                {
                    ioXml.Serialize(stream, ios);
                }
            }
            catch (Exception ex) 
            { 
                result = false;
                ShowMessage(ex.Message);
            }
            return result;
        }

        private void comboBox_cam_SelectedIndexChanged(object sender, EventArgs e)
        {
            //判断是否需要保存
            if (isAlter)
            {
                DialogResult dialogResult = MessageBox.Show($"{Resources.LanguageDic.is_save_para}？", $"{Resources.LanguageDic.tip}", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
                if (dialogResult == DialogResult.Yes)
                {
                    if (!Save() || !signal.Save())
                    {
                        MessageBox.Show($"{Resources.LanguageDic.save_fail}！！", $"{Resources.LanguageDic.tip}", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else
                    {
                        isAlter = false;
                    }
                }
                else if (dialogResult == DialogResult.Cancel)
                {
                    isAlter = false;
                }
            }

            //关闭更新
            if (camIODict.Count > 0 && camIODict.Keys.Contains(comboBox_cam.SelectedItem.ToString()))
            {
                for (int i = 0; i < diNameLabels.Count; i++)
                {
                    diAddressTextBoxs[i].TextChanged -= UpDataDi;
                }
                for (int i = 0; i < doNameLabels.Count; i++)
                {
                    doAddressTextBoxs[i].TextChanged -= UpDataDo;
                }
            }


            // 更新界面，放到后面的选项控件里
            if (camIODict.Count > 0 && camIODict.Keys.Contains(comboBox_cam.SelectedItem.ToString()))
            {
                camName = comboBox_cam.SelectedItem.ToString();
                ioDict = camIODict[camName];
                for (int i = 0; i < diNameLabels.Count; i++)
                {
                    if (ioDict.ContainsKey(diNameLabels[i].Text))
                    {
                        diAddressTextBoxs[i].Text = ioDict[diNameLabels[i].Text].Address;
                    }
                    diAddressTextBoxs[i].TextChanged += UpDataDi;
                }
                for (int i = 0; i < doNameLabels.Count; i++)
                {
                    if (ioDict.ContainsKey(doNameLabels[i].Text))
                    {
                        doAddressTextBoxs[i].Text = ioDict[doNameLabels[i].Text].Address;
                    }
                    doAddressTextBoxs[i].TextChanged += UpDataDo;
                }
            }
            
        }
    }
}
