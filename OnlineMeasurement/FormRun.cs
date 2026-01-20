#define GW

using BaslerCamera;
using HalconDotNet;
using HslCommunication;
using HslCommunication.Profinet.Melsec;
using HslCommunication.Profinet.Omron;
using MySqlX.XDevAPI.Common;
using OnlineMeasurement.IO;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Remoting.Contexts;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;
using static BaslerCamera.Cam;
using static BaslerCamera.MvCamera;
using static OnlineMeasurement.IO.IHsl;

namespace OnlineMeasurement
{
    public partial class FormRun : Form
    {
        Thread mainThread = null;
        bool stop = true;
        System.Timers.Timer alive = new System.Timers.Timer();
        //private OmronFinsNet omronFinsNet = null;
        //private MelsecMcNet plc = null;
        private IHsl plc = new MelsecPlc();


        SerialPort sp = new SerialPort();
        Dictionary<int, string> 车型号转名称 = new Dictionary<int, string>();
        List<string> 车型参数排序key = new List<string>();
        Dictionary<string, Car> 车型参数 = new Dictionary<string, Car>();
        Dictionary<string, CamSetting> 相机参数 = new Dictionary<string, CamSetting>();
        Dictionary<string, BaslerCamera.Cam> 相机 = new Dictionary<string, BaslerCamera.Cam>();
        OtherSet otherSet = new OtherSet();

        wSql sql = null;
        bool sqlEnable = false;
        bool 预载车型参数 = false;

        Color 原色 = Color.White;

        FormShow formShow = null;

        public Dictionary<string, Dictionary<string, IoAddress>> camIODict = new Dictionary<string, Dictionary<string, IoAddress>>();
    public FormRun()
        {
            InitializeComponent();

            //读取语言id
            GlobalVarAndFunc.ReadLanguageID();


#if GW
            Text += "（GW）";
#else
            Text += "（SR）";
#endif

            原色 = checkBoxLedL.BackColor;

            //            plc = new MelsecMcNet();
            //            plc.ConnectTimeOut = 2000;
            //#if GW
            //            plc.IpAddress = "192.168.26.230";//GW
            //#else
            //            omronFinsNet.IpAddress = "192.168.1.31";//SR
            //#endif
            //            plc.Port = 9600;
            //            //plc.DA2 = 0;
            //            plc.ByteTransform.DataFormat = HslCommunication.Core.DataFormat.CDAB;
            //            plc.ByteTransform.IsStringReverseByteWord = true;

            sp = new SerialPort();
            sp.PortName = "COM1";
            sp.BaudRate = 19200;
            sp.DataBits = 8;
            sp.StopBits = StopBits.One;
            sp.Parity = Parity.None;
            sp.WriteTimeout = 2000;
            sp.DataReceived += Sp_DataReceived;


            alive.Interval = 500;
            alive.AutoReset = true;
            alive.Elapsed += alive_Tick;

        }
        private void FormRun_Load(object sender, EventArgs e)
        {

            comboBox1.SelectedIndex = GlobalVarAndFunc.LANGUAGE_ID;
            GlobalVarAndFunc.SHOW_MESSAGE = true;

            buttonRun_Click(null, null);

        }
        private void Sp_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort serialPort = (SerialPort)sender;
            string str = serialPort.ReadExisting();
            //ShowMessage("串口接收：" + str);
        }

        bool b = true;
        void alive_Tick(object sender, EventArgs e)
        {
            //非UI线程执行,且会并行执行
            if (b)//防止触发间隔小于执行时间造成信号紊乱与线程积累
            {
                b = false;
                if (mainThread != null && mainThread.IsAlive && !stop)
                {
                    var result = plc.ReadBool(camIODict["L"]["HeartBeat"].Address);
                    if (result.IsSuccess)
                    {
                        bool b2;
                        lock (iolock)
                        {
                            b2 = plc.Write(camIODict["L"]["HeartBeat"].Address, !result.Content).IsSuccess;
                        }
                        if (!b2)
                        {
                            //ShowMessage("心跳写入失败", Color.Red);
                        }
                    }
                    else
                    {
                        //ShowMessage("心跳读取失败", Color.Red);
                    }
                }
                b = true;
            }
        }
        private void buttonRun_Click(object sender, EventArgs e)
        {
            // 临时屏蔽
            //LicenseKey key = new LicenseKey();
            //string productID = textBoxTestNum.Text;
            //if (key.IsTrue(productID))
            //{
            RunState();
            //开启主流程
            if (mainThread == null || mainThread.ThreadState == ThreadState.Stopped)
            {
                mainThread = new Thread(MainThread);
                mainThread.Start();
                b = true;
            }
            //}
            //else
            //{
            //    MessageBox.Show(Resources.LanguageDic.License_error, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //}

        }
        DateTime dateTimeNow = DateTime.Now;
        Task taskL = null;
        Task taskR = null;
        bool bAbort = false;
        bool bDry_mode = false;


        bool plcInit()
        {
            if (plc.Load())
            {
                ShowMessage($"plc {Resources.LanguageDic.para_load_success}");
            }
            else
            {
                ShowMessage($"plc {Resources.LanguageDic.para_load_fail}:{plc.ErrMsg}", Color.Red);
                this.BeginInvoke(new Action(() => { this.Activate(); }));
                return false;
            }
            if (plc.Open())
            {
                ShowMessage($"PLC{Resources.LanguageDic.connection_successful}");
            }
            else
            {
                ShowMessage($"PLC{Resources.LanguageDic.connection_failed}:" + plc.ErrMsg, Color.Red);
                this.BeginInvoke(new Action(() => { this.Activate(); }));
                return false;
            }

            //读取参数
            camIODict.Clear();
            string[] camPaths = Directory.GetDirectories("Data\\Cam");
            foreach (var item in camPaths)
            {
                Dictionary<string, IoAddress> ioDict = new Dictionary<string, IoAddress>() ;
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
                        ShowMessage($"PLC IO {Resources.LanguageDic.para_load_fail}", Color.Red);
                        return false;
                    }
                    else
                    {
                        ioDict = ios.ToDictionary(n => { return n.IoName; });
                    }

                    camIODict.Add(name, ioDict);
                }
                else
                {
                    ShowMessage($"{paramPath} {Resources.LanguageDic.file_not_exist}", Color.Red);
                    return false;

                }

            }
            return true;

        }


        void plcClose()
        {
            if (plc.IsOpen)
            {
                plc?.Close();
            }
        }

        void MainThread()
        {
            try
            {
                //加载参数
                车型号转名称.Clear();
                车型参数.Clear();
                车型参数排序key.Clear();
                if (预载车型参数)
                {
                    string[] carPaths = Directory.GetDirectories("Data\\Car");
                    foreach (var item in carPaths)
                    {
                        string dirName = Path.GetFileNameWithoutExtension(item);
                        string[] strings = dirName.Split('-');
                        if (strings.Length == 3 && int.TryParse(strings[0], out int 车型号) && int.TryParse(strings[1], out int 托盘号))
                        {
                            if (!车型号转名称.ContainsKey(车型号))
                            {
                                车型号转名称.Add(车型号, strings[2]);
                            }

                            Car car = new Car(dirName);
                            if (car.Load())
                            {
                                车型参数.Add($"{车型号}-{托盘号}", car);
                                车型参数排序key.Add($"{车型号}-{托盘号}");
                                ShowMessage($"{dirName}{Resources.LanguageDic.para_load_success}");
                            }
                            else
                            {
                                ShowMessage($"{dirName}{Resources.LanguageDic.para_load_fail}", Color.Red);
                            }
                        }
                    }
                }

                相机.Clear();
                相机参数.Clear();
                string[] camPaths = Directory.GetDirectories("Data\\Cam");
                foreach (var item in camPaths)
                {
                    string name = Path.GetFileNameWithoutExtension(item);

                    相机.Add(name, new BaslerCamera.Cam());

                    CamSetting camSetting = new CamSetting(name);
                    if (camSetting.Load(item))
                    {
                        相机参数.Add(name, camSetting);
                        ShowMessage($"{name}{Resources.LanguageDic.cam}{Resources.LanguageDic.para_load_success}");
                    }
                    else
                    {
                        ShowMessage($"{name}{Resources.LanguageDic.cam}{Resources.LanguageDic.para_load_fail}", Color.Red);
                    }
                }

                otherSet.Load();

                if (!plcInit())
                {
                    return;
                }
                ////连接设备
                //OperateResult connect = plc.ConnectServer();
                //if (connect.IsSuccess)
                //{
                //    ShowMessage($"PLC{Resources.LanguageDic.connection_successful}");
                //}
                //else
                //{
                //    ShowMessage($"PLC{Resources.LanguageDic.connection_failed}:" + connect.ToMessageShowString(), Color.Red);
                //    return;
                //}
                //try
                //{
                //    if (!sp.IsOpen)
                //    {
                //        sp.Open();
                //        ShowMessage(Resources.LanguageDic.Serial_port_successfully_opened);
                //    }
                //    else
                //    {
                //        ShowMessage(Resources.LanguageDic.Serial_port_already_opened);
                //    }
                //}
                //catch (Exception ex)
                //{
                //    ShowMessage($"{Resources.LanguageDic.Serial_port_failly_opened}：" + ex.Message, Color.Red);
                //    return;
                //}
                foreach (var item in 相机.Keys)
                {
                    if (相机[item].OpenByName(item))
                    {
                        ShowMessage(item + $"{Resources.LanguageDic.cam_open_success}");
                    }
                    else
                    {
                        ShowMessage(item + $"{Resources.LanguageDic.cam_open_fail}", Color.Red);
                        return;
                    }
                }
                if (!sqlEnable)
                {
                    try
                    {
                        sql = new wSql("127.0.0.1", "admin", "admin", "honda");
                        sql.Open();
                        ShowMessage($"{Resources.LanguageDic.database_connect_success}！");
                        sql.Close();
                        sqlEnable = true;
                    }
                    catch (Exception ex)
                    {
                        ShowMessage($"{Resources.LanguageDic.database_connect_fail}！" + ex.Message, Color.Red);
                        sqlEnable = false;
                    }
                }

                threadSaveImage = new Thread(SaveImage);
                threadSaveImage.Start();

                alive.Enabled = true;
                //DIO dIO_L = new DIO("L");
                //DIO dIO_R = new DIO("R");
                while (!stop)
                {
                    if (loadAgain)//热重载参数
                    {
                        车型参数.Clear();
                        车型参数排序key.Clear();
                        otherSet.Load();
                        loadAgain = false;
                    }

                    bAbort = false;
                    //信号复位
                    if (!plc.Write(camIODict["L"]["Readily"].Address, false).IsSuccess)
                    {
                        ShowMessage(Resources.LanguageDic.ready_write_fail, Color.Red);
                    }
                    if (!plc.Write(camIODict["L"]["Running"].Address, false).IsSuccess)
                    {
                        ShowMessage(Resources.LanguageDic.running_write_fail, Color.Red);
                    }
                    if (!plc.Write(camIODict["L"]["Check_Finish"].Address, false).IsSuccess)
                    {
                        ShowMessage(Resources.LanguageDic.check_result_readable_write_fail, Color.Red);
                    }
                    if (!plc.Write(camIODict["L"]["Result"].Address, false).IsSuccess)
                    {
                        ShowMessage(Resources.LanguageDic.check_result_write_fail, Color.Red);
                    }
                    if (!plc.Write(camIODict["L"]["Acq_Finish"].Address, false).IsSuccess)
                    {
                        ShowMessage(Resources.LanguageDic.L_photo_finish_write_fail, Color.Red);
                    }
                    if (!plc.Write(camIODict["R"]["Acq_Finish"].Address, false).IsSuccess)
                    {
                        ShowMessage(Resources.LanguageDic.R_photo_finish_write_fail, Color.Red);
                    }


                    //

                    if (!plc.Write(camIODict["L"]["Feedback_Car_Model"].Address, 0).IsSuccess)
                    {
                        ShowMessage(Resources.LanguageDic.model_no_code_write_fail, Color.Red);
                    }

                    if (!plc.Write(camIODict["L"]["Feedback_Check_Point_NO"].Address, 0).IsSuccess)
                    {
                        ShowMessage("L" + $"{Resources.LanguageDic.Point_number_writing_failed}", Color.Red);
                    }

                    if (!plc.Write(camIODict["R"]["Feedback_Check_Point_NO"].Address, 0).IsSuccess)
                    {
                        ShowMessage("R" + $"{Resources.LanguageDic.Point_number_writing_failed}", Color.Red);
                    }
                    Thread.Sleep(100);
                    var lastStartResult = plc.ReadBool(camIODict["L"]["Start"].Address);
                    if (!lastStartResult.IsSuccess)
                    {
                        ShowMessage(Resources.LanguageDic.start_signal_read_fail, Color.Red);
                        return;
                    }

                    if (!plc.Write(camIODict["L"]["Readily"].Address, true).IsSuccess)
                    {
                        ShowMessage(Resources.LanguageDic.ready_write_fail, Color.Red);
                    }
                    ShowMessage(Resources.LanguageDic.output_ready_signal);

                    lblSysStatus.BeginInvoke(new Action(() =>
                    {
                        lblSysStatus.Text = Resources.LanguageDic.wait;
                        lblSysStatus.BackColor = Color.Yellow;
                    }));

                    System.Diagnostics.Stopwatch sp = new System.Diagnostics.Stopwatch();
                    ShowMessage(Resources.LanguageDic.wait_start_signal);
                    while (true)
                    {
                        if (stop) return;
                        var val = plc.ReadBool(camIODict["L"]["Start"].Address);
                        if (val.IsSuccess)
                        {
                            if ((val.Content == true && lastStartResult.Content == false) || bSkipStart)
                            {
                                bSkipStart = false;
                                sp.Start();
                                ShowMessage(Resources.LanguageDic.reserve_start_signal);
                                break;
                            }
                            lastStartResult = val;
                        }
                        else
                        {
                            ShowMessage(Resources.LanguageDic.start_signal_read_fail, Color.Red);
                            //return;
                        }
                        Thread.Sleep(50);
                    }

                    ////是否空运行
                    //var dry_mode = plc.ReadBool(camIODict["L"]["空运行"].Address);
                    //if (!dry_mode.IsSuccess)
                    //{
                    //    ShowMessage(Resources.LanguageDic.run_empty_signal_read_fail, Color.Red);
                    //    return;
                    //}
                    //bDry_mode = dry_mode.Content;
                    bDry_mode = false;
                    ShowMessage(Resources.LanguageDic.run_empty_signal + bDry_mode);

                    //读车型连番
                    var carKind = plc.ReadInt16(camIODict["L"]["Car_Model"].Address);
                    if (!carKind.IsSuccess)
                    {
                        ShowMessage(Resources.LanguageDic.model_no_code_read_fail, Color.Red);
                        return;
                    }

                    //反馈车型码
                    //读车型连番
                    var writecarKind = plc.Write(camIODict["L"]["Feedback_Car_Model"].Address, carKind.Content);
                    if (!writecarKind.IsSuccess)
                    {
                        ShowMessage(Resources.LanguageDic.model_no_code_write_fail, Color.Red);
                        return;
                    }

                    //转换成整十数，暂时屏蔽，没啥用
                    //carKind.Content[0] = (short)(carKind.Content[0] / 10 * 10);

                    // 暂时不用托盘号和连番号，都写死为1
                    short carNum = 1;

                    short TRnum = 1;

                    //var carNumInfo = ReadInt16(DIO.连番信息, 1);
                    //if (!carNumInfo.IsSuccess)
                    //{
                    //    ShowMessage(Resources.LanguageDic.car_no_info_read_fail, Color.Red);
                    //    return;
                    //}
                    //short carNum = carNumInfo.Content[0];

                    //var TRnumInfo = ReadInt16(DIO.托盘号, 1);
                    //if (!TRnumInfo.IsSuccess)
                    //{
                    //    ShowMessage(Resources.LanguageDic.pallet_no_read_fail, Color.Red);
                    //    return;
                    //}
                    //short TRnum = TRnumInfo.Content[0];


                    var carNumStringInfo = plc.ReadString(camIODict["L"]["Car_NO"].Address, 10);
                    if (!carNumStringInfo.IsSuccess)
                    {
                        ShowMessage(Resources.LanguageDic.vin_read_fail, Color.Red);
                        return;
                    }
                    string carNumString = carNumStringInfo.Content;

                    ShowMessage(Resources.LanguageDic.model_no2 + carKind.Content);
                    ShowMessage(Resources.LanguageDic.pallet_no + TRnum);
                    ShowMessage(Resources.LanguageDic.car_no2 + carNum);
                    ShowMessage(Resources.LanguageDic.vin2 + carNumString);
                    dateTimeNow = DateTime.Now;

                    string carVIN = carNumString;
                    var chars = Path.GetInvalidFileNameChars();
                    string path = $"{otherSet.imagePath}\\{carKind.Content}\\{TRnum}\\{dateTimeNow:yyyy-MM-dd HH_mm_ss} {carVIN}";
                    foreach (char c in chars)
                    {
                        if (carNumString.Contains(c))
                        {
                            ShowMessage($"{Resources.LanguageDic.vin2}{Resources.LanguageDic.contains_illegal_characters}" + c, Color.Yellow);
                            //carVIN = "";
                            path = $"{otherSet.imagePath}\\{carKind.Content}\\{TRnum}\\{dateTimeNow:yyyy-MM-dd HH_mm_ss}";
                            break;
                        }
                    }


                    //if (!bDry_mode)
                    //{
                    //    //try
                    //    //{
                    //    Directory.CreateDirectory(path);
                    //    //}
                    //    //catch (Exception ex)
                    //    //{
                    //    //    ShowMessage(ex.ToString(), Color.Red);
                    //    //    path = $"{otherSet.imagePath}\\{carKind.Content[0]}\\{TRnum.Content[0]}\\{dateTimeNow:yyyy-MM-dd HH_mm_ss}";
                    //    //    Directory.CreateDirectory(path);
                    //    //}
                    //}

                    if (!车型参数.ContainsKey($"{carKind.Content}-{TRnum}"))
                    {
                        string[] carPaths = Directory.GetDirectories("Data\\Car", $"{carKind.Content}-{TRnum}-*");
                        if (carPaths.Length > 0)
                        {
                            string dirName = Path.GetFileNameWithoutExtension(carPaths[0]);
                            string[] strings = dirName.Split('-');
                            if (strings.Length == 3 && int.TryParse(strings[0], out int 车型号) && int.TryParse(strings[1], out int 托盘号))
                            {
                                if (!车型号转名称.ContainsKey(车型号))
                                {
                                    车型号转名称.Add(车型号, strings[2]);
                                }
                                if (!车型参数.ContainsKey($"{车型号}-{托盘号}"))
                                {
                                    Car car = new Car(dirName);
                                    if (car.Load())
                                    {
                                        if (车型参数.Count >= 8)
                                        {
                                            车型参数.Remove(车型参数排序key[0]);
                                            车型参数排序key.Remove(车型参数排序key[0]);
                                        }
                                        车型参数.Add($"{车型号}-{托盘号}", car);
                                        车型参数排序key.Add($"{车型号}-{托盘号}");
                                        ShowMessage($"{dirName}{Resources.LanguageDic.para_load_success}");
                                    }
                                    else
                                    {
                                        ShowMessage($"{dirName}{Resources.LanguageDic.para_load_fail}", Color.Red);
                                    }
                                }
                                else
                                {
                                    ShowMessage($"{dirName}{Resources.LanguageDic.para_already_load}");
                                }
                            }
                        }
                    }
                    else
                    {
                        if (车型号转名称.ContainsKey(carKind.Content))
                        {
                            ShowMessage($"{carKind.Content}-{TRnum}-{车型号转名称[carKind.Content]}{Resources.LanguageDic.para_already_load}");
                        }
                        else
                        {
                            ShowMessage($"{carKind.Content}-{TRnum}{Resources.LanguageDic.para_already_load}");
                        }
                    }

                    string carName = carKind.Content.ToString();
                    if (车型号转名称.ContainsKey(carKind.Content))
                    {
                        carName = 车型号转名称[carKind.Content];
                    }
                    //显示
                    formShow?.UpDataCamImage(null, "Clear");///////////////////////////
                    formShow?.UpDataCarInform(carName, carNumString);/////////////////////////
                    lblSysStatus.BeginInvoke(new Action(() =>
                    {
                        lblSysStatus.Text = Resources.LanguageDic.Running;
                        lblSysStatus.BackColor = Color.Green;
                        labelResult.Text = "--";
                        labelResult.BackColor = Color.Transparent;

                        labelCarKind.Text = carKind.Content.ToString();
                        if (车型号转名称.ContainsKey(carKind.Content))
                        {
                            labelCarName.Text = 车型号转名称[carKind.Content];
                        }
                        else
                        {
                            labelCarName.Text = Resources.LanguageDic.not_exist;
                        }
                        labelCarNum.Text = carNumString;

                        dataGridViewShow.Rows.Clear();
                    }));


                    //信号复位
                    //if (!plc.Write(camIODict["L"]["Check_Finish"].Address, false).IsSuccess)
                    //{
                    //    ShowMessage(Resources.LanguageDic.check_result_readable_write_fail, Color.Red);
                    //}
                    if (!plc.Write(camIODict["L"]["Running"].Address, true).IsSuccess)
                    {
                        ShowMessage(Resources.LanguageDic.running_write_fail, Color.Red);
                    }
                    Thread.Sleep(50);
                    if (!plc.Write(camIODict["L"]["Readily"].Address, false).IsSuccess)
                    {
                        ShowMessage(Resources.LanguageDic.ready_write_fail, Color.Red);
                    }

                    // 输出点结果信号,初始化
                    {
                        string IOName = "L";
                        var carSetting = 车型参数[$"{carKind.Content}-{TRnum}"].car[IOName];

                        foreach (var item in carSetting.gSets.Keys)
                        {
                            var point3d = new Point3D(0, 0, 0);

                            double dx = 0;
                            double dy = 0;
                            double dz = 0;


                            string address = (int.Parse(camIODict[IOName]["X"].Address.Substring(1, camIODict[IOName]["X"].Address.Length - 1)) + (item - 1) * 12).ToString();
                            var writeXRef = plc.Write("D" + address, (int)(point3d.X*100));
                            address = (int.Parse(camIODict[IOName]["Y"].Address.Substring(1, camIODict[IOName]["Y"].Address.Length - 1)) + (item - 1) * 12).ToString();
                            var writeYRef = plc.Write("D" + address, (int)(point3d.Y * 100));
                            address = (int.Parse(camIODict[IOName]["Z"].Address.Substring(1, camIODict[IOName]["Z"].Address.Length - 1)) + (item - 1) * 12).ToString();
                            var writeZRef = plc.Write("D" + address, (int)(point3d.Z * 100));
                            address = (int.Parse(camIODict[IOName]["Dx"].Address.Substring(1, camIODict[IOName]["Dx"].Address.Length - 1)) + (item - 1) * 12).ToString();
                            var writeDXRef = plc.Write("D" + address, (int)(dx * 100));
                            address = (int.Parse(camIODict[IOName]["Dy"].Address.Substring(1, camIODict[IOName]["Dy"].Address.Length - 1)) + (item - 1) * 12).ToString();
                            var writeDYRef = plc.Write("D" + address, (int)(dy * 100));
                            address = (int.Parse(camIODict[IOName]["Dz"].Address.Substring(1, camIODict[IOName]["Dz"].Address.Length - 1)) + (item - 1) * 12).ToString();
                            var writeDZRef = plc.Write("D" + address, (int)(dz * 100));
                        }
                        IOName = "R";
                        carSetting = 车型参数[$"{carKind.Content}-{TRnum}"].car[IOName];
                        
                        foreach (var item in carSetting.gSets.Keys)
                        {
                            var point3d = new Point3D(0, 0, 0);

                            double dx = 0;
                            double dy = 0;
                            double dz = 0;

                            string address = (int.Parse(camIODict[IOName]["X"].Address.Substring(1, camIODict[IOName]["X"].Address.Length - 1)) + (item - 1) * 12).ToString();
                            var writeXRef = plc.Write("D" + address, (int)(point3d.X * 100));
                            address = (int.Parse(camIODict[IOName]["Y"].Address.Substring(1, camIODict[IOName]["Y"].Address.Length - 1)) + (item - 1) * 12).ToString();
                            var writeYRef = plc.Write("D" + address, (int)(point3d.Y * 100));
                            address = (int.Parse(camIODict[IOName]["Z"].Address.Substring(1, camIODict[IOName]["Z"].Address.Length - 1)) + (item - 1) * 12).ToString();
                            var writeZRef = plc.Write("D" + address, (int)(point3d.Z * 100));
                            address = (int.Parse(camIODict[IOName]["Dx"].Address.Substring(1, camIODict[IOName]["Dx"].Address.Length - 1)) + (item - 1) * 12).ToString();
                            var writeDXRef = plc.Write("D" + address, (int)(dx * 100));
                            address = (int.Parse(camIODict[IOName]["Dy"].Address.Substring(1, camIODict[IOName]["Dy"].Address.Length - 1)) + (item - 1) * 12).ToString();
                            var writeDYRef = plc.Write("D" + address, (int)(dy * 100));
                            address = (int.Parse(camIODict[IOName]["Dz"].Address.Substring(1, camIODict[IOName]["Dz"].Address.Length - 1)) + (item - 1) * 12).ToString();
                            var writeDZRef = plc.Write("D" + address, (int)(dz * 100));
                        }
                    }


                    //机器人交互
                    bool 检测异常L = true; bool 数据超差L = true;
                    Dictionary<int, Point3D> point3dL = new Dictionary<int, Point3D>();
                    Dictionary<int, Point3D> point3dBaseL = new Dictionary<int, Point3D>();
                    Dictionary<int, Dictionary<string, string>> sqlValuePairsL = new Dictionary<int, Dictionary<string, string>>();
                    taskL = Task.Run(new Action(() =>
                    {
                        string key = "L";
                        if (相机参数.ContainsKey(key) && 车型参数.ContainsKey($"{carKind.Content}-{TRnum}") && 车型参数[$"{carKind.Content}-{TRnum}"].car.ContainsKey(key))
                        {
                            RobotRun(path, 相机[key], 相机参数[key], 车型参数[$"{carKind.Content}-{TRnum}"].car[key], key, camIODict[key], out point3dL, out point3dBaseL, out sqlValuePairsL, ref 检测异常L, ref 数据超差L);
                        }
                        else
                        {
                            RobotRun(path, 相机[key], null, null, key, camIODict[key], out point3dL, out point3dBaseL, out sqlValuePairsL, ref 检测异常L, ref 数据超差L);
                        }
                    }));
                    bool 检测异常R = true; bool 数据超差R = true;
                    Dictionary<int, Point3D> point3dR = new Dictionary<int, Point3D>();
                    Dictionary<int, Point3D> point3dBaseR = new Dictionary<int, Point3D>();
                    Dictionary<int, Dictionary<string, string>> sqlValuePairsR = new Dictionary<int, Dictionary<string, string>>();
                    taskR = Task.Run(new Action(() =>
                    {
                        string key = "R";
                        if (相机参数.ContainsKey(key) && 车型参数.ContainsKey($"{carKind.Content}-{TRnum}") && 车型参数[$"{carKind.Content}-{TRnum}"].car.ContainsKey(key))
                        {
                            RobotRun(path, 相机[key], 相机参数[key], 车型参数[$"{carKind.Content}-{TRnum}"].car[key],key, camIODict[key], out point3dR, out point3dBaseR, out sqlValuePairsR, ref 检测异常R, ref 数据超差R);
                        }
                        else
                        {
                            RobotRun(path, 相机[key], null, null,key, camIODict[key], out point3dR, out point3dBaseR, out sqlValuePairsR, ref 检测异常R, ref 数据超差R);
                        }
                    }));



                    //等待机器人任务完成，先屏蔽右边
                    ShowMessage(Resources.LanguageDic.wait_for_robot_finish);
                    while (!(taskL.IsCompleted && taskR.IsCompleted))
                    {
                        if (stop) return;

                        Thread.Sleep(10);
                    }
                    //while (!(taskR.IsCompleted))
                    //{
                    //    if (stop) return;

                    //    Thread.Sleep(10);
                    //}


                    if (bAbort)
                    {
                        continue;
                    }

                    if (taskR.IsFaulted)
                    {
                        string msg = "";
                        msg = taskR.Exception?.ToString();
                        ShowMessage($"{Resources.LanguageDic.R_robot_error}：" + msg, Color.Red);
                        检测异常R = false;
                    }
                    else if (taskR.IsCanceled)
                    {
                        ShowMessage($"{Resources.LanguageDic.R_robot_cancle}");
                    }
                    else
                    {
                        ShowMessage($"{Resources.LanguageDic.R_robot_finish}");
                    }


                    try
                    {
                        Dictionary<int, Point3D> point3dLNew = new Dictionary<int, Point3D>();
                        Dictionary<int, Point3D> point3dRNew = new Dictionary<int, Point3D>();


                        if (point3dBaseL.Count() + point3dBaseR.Count() >= 3)
                        {
                            HTuple px = new HTuple();
                            HTuple py = new HTuple();
                            HTuple pz = new HTuple();
                            HTuple qx = new HTuple();
                            HTuple qy = new HTuple();
                            HTuple qz = new HTuple();
                            foreach (var item in point3dBaseL.Keys)
                            {
                                if (point3dL.ContainsKey(item))
                                {
                                    px.Append(point3dL[item].X);
                                    py.Append(point3dL[item].Y);
                                    pz.Append(point3dL[item].Z);
                                    qx.Append(point3dBaseL[item].X);
                                    qy.Append(point3dBaseL[item].Y);
                                    qz.Append(point3dBaseL[item].Z);
                                }
                                else
                                {
                                    ShowMessage($"{Resources.LanguageDic.exist}point3dBaseL[{item}]{Resources.LanguageDic.but_not_exist}point3dL[{item}", Color.Red);
                                }
                            }
                            foreach (var item in point3dBaseR.Keys)
                            {
                                if (point3dR.ContainsKey(item))
                                {
                                    px.Append(point3dR[item].X);
                                    py.Append(point3dR[item].Y);
                                    pz.Append(point3dR[item].Z);
                                    qx.Append(point3dBaseR[item].X);
                                    qy.Append(point3dBaseR[item].Y);
                                    qz.Append(point3dBaseR[item].Z);
                                }
                                else
                                {
                                    ShowMessage($"{Resources.LanguageDic.exist}point3dBaseR[{item}]{Resources.LanguageDic.but_not_exist}point3dR[{item}", Color.Red);
                                }
                            }

                            if (qz.Length >= 3)
                            {
                                HHomMat3D hHomMat3D = new HHomMat3D();
                                hHomMat3D.VectorToHomMat3d("rigid", px, py, pz, qx, qy, qz);

                                数据超差L = true;
                                foreach (var item in point3dL.Keys)
                                {
                                    double nx = hHomMat3D.AffineTransPoint3d(point3dL[item].X, point3dL[item].Y, point3dL[item].Z, out double ny, out double nz);
                                    point3dLNew.Add(item, new Point3D(nx, ny, nz));

                                    //重新计算
                                    string IOName = "L";
                                    var carSetting = 车型参数[$"{carKind.Content}-{TRnum}"].car[IOName];
                                    double dx = nx - carSetting.gSets[item].X;
                                    double dy = ny - carSetting.gSets[item].Y;
                                    double dz = nz - carSetting.gSets[item].Z;
                                    double dd = Math.Sqrt(dx * dx + dy * dy + dz * dz);

                                    var 测点数据表 = sqlValuePairsL[item];
                                    测点数据表["X"] = nx.ToString("0.000");
                                    测点数据表["Y"] = ny.ToString("0.000");
                                    测点数据表["Z"] = nz.ToString("0.000");
                                    测点数据表["DX"] = dx.ToString("0.000");
                                    测点数据表["DY"] = dy.ToString("0.000");
                                    测点数据表["DZ"] = dz.ToString("0.000");
                                    测点数据表["DD"] = dd.ToString("0.000");

                                    //重新显示
                                    dataGridViewShow.BeginInvoke(new Action(() =>
                                    {
                                        int index = dataGridViewShow.Rows.Add("*" + IOName + item, nx.ToString("0.000"), ny.ToString("0.000"), nz.ToString("0.000"), dx.ToString("0.000"), dy.ToString("0.000"), dz.ToString("0.000"), dd.ToString("0.000"));
                                        if (dx < carSetting.gSets[item].minDX || dx > carSetting.gSets[item].maxDX)
                                        {
                                            dataGridViewShow.Rows[index].Cells[4].Style.BackColor = Color.Red;
                                        }
                                        if (dy < carSetting.gSets[item].minDY || dy > carSetting.gSets[item].maxDY)
                                        {
                                            dataGridViewShow.Rows[index].Cells[5].Style.BackColor = Color.Red;
                                        }
                                        if (dz < carSetting.gSets[item].minDZ || dz > carSetting.gSets[item].maxDZ)
                                        {
                                            dataGridViewShow.Rows[index].Cells[6].Style.BackColor = Color.Red;
                                        }
                                    }));
                                    if (dx < carSetting.gSets[item].minDX || dx > carSetting.gSets[item].maxDX)
                                    {
                                        数据超差L = false;
                                    }
                                    if (dy < carSetting.gSets[item].minDY || dy > carSetting.gSets[item].maxDY)
                                    {
                                        数据超差L = false;
                                    }
                                    if (dz < carSetting.gSets[item].minDZ || dz > carSetting.gSets[item].maxDZ)
                                    {
                                        数据超差L = false;
                                    }
                                    formShow?.UpDataXYZ(new Point3D(nx, ny, nz), IOName + item);////////////////////////
                                }
                                数据超差R = true;

                                foreach (var item in point3dR.Keys)
                                {
                                    double nx = hHomMat3D.AffineTransPoint3d(point3dR[item].X, point3dR[item].Y, point3dR[item].Z, out double ny, out double nz);
                                    point3dRNew.Add(item, new Point3D(nx, ny, nz));

                                    //重新计算
                                    string IOName = "R";
                                    var carSetting = 车型参数[$"{carKind.Content}-{TRnum}"].car[IOName];
                                    double dx = nx - carSetting.gSets[item].X;
                                    double dy = ny - carSetting.gSets[item].Y;
                                    double dz = nz - carSetting.gSets[item].Z;
                                    double dd = Math.Sqrt(dx * dx + dy * dy + dz * dz);

                                    var 测点数据表 = sqlValuePairsR[item];
                                    测点数据表["X"] = nx.ToString("0.000");
                                    测点数据表["Y"] = ny.ToString("0.000");
                                    测点数据表["Z"] = nz.ToString("0.000");
                                    测点数据表["DX"] = dx.ToString("0.000");
                                    测点数据表["DY"] = dy.ToString("0.000");
                                    测点数据表["DZ"] = dz.ToString("0.000");
                                    测点数据表["DD"] = dd.ToString("0.000");

                                    //重新显示
                                    dataGridViewShow.BeginInvoke(new Action(() =>
                                    {
                                        int index = dataGridViewShow.Rows.Add("*" + IOName + item, nx.ToString("0.000"), ny.ToString("0.000"), nz.ToString("0.000"), dx.ToString("0.000"), dy.ToString("0.000"), dz.ToString("0.000"), dd.ToString("0.000"));
                                        if (dx < carSetting.gSets[item].minDX || dx > carSetting.gSets[item].maxDX)
                                        {
                                            dataGridViewShow.Rows[index].Cells[4].Style.BackColor = Color.Red;
                                        }
                                        if (dy < carSetting.gSets[item].minDY || dy > carSetting.gSets[item].maxDY)
                                        {
                                            dataGridViewShow.Rows[index].Cells[5].Style.BackColor = Color.Red;
                                        }
                                        if (dz < carSetting.gSets[item].minDZ || dz > carSetting.gSets[item].maxDZ)
                                        {
                                            dataGridViewShow.Rows[index].Cells[6].Style.BackColor = Color.Red;
                                        }
                                    }));
                                    if (dx < carSetting.gSets[item].minDX || dx > carSetting.gSets[item].maxDX)
                                    {
                                        数据超差R = false;
                                    }
                                    if (dy < carSetting.gSets[item].minDY || dy > carSetting.gSets[item].maxDY)
                                    {
                                        数据超差R = false;
                                    }
                                    if (dz < carSetting.gSets[item].minDZ || dz > carSetting.gSets[item].maxDZ)
                                    {
                                        数据超差R = false;
                                    }
                                    formShow?.UpDataXYZ(new Point3D(nx, ny, nz), IOName + item);////////////////////////
                                }
                            }
                            else
                            {
                                ShowMessage($"{Resources.LanguageDic.The_number_of_coordinate_points_is_less_than}3，{Resources.LanguageDic.Unable_to_reconstruct_coordinates}", Color.Red);
                            }
                        }

                        // 判断结果数量与检测数量是否一致
                        {
                            string IOName = "L";
                            var carSetting = 车型参数[$"{carKind.Content}-{TRnum}"].car[IOName];
                            if (carSetting.gSets.Keys.Count != point3dL.Count)
                            {
                                数据超差L = false;
                            }

                        }
                        {
                            string IOName = "R";
                            var carSetting = 车型参数[$"{carKind.Content}-{TRnum}"].car[IOName];
                            if (carSetting.gSets.Keys.Count != point3dR.Count)
                            {
                                数据超差R = false;
                            }

                        }

                        // 输出前，先复位一下
                        // 输出点结果信号,初始化
                        {
                            string IOName = "L";
                            var carSetting = 车型参数[$"{carKind.Content}-{TRnum}"].car[IOName];

                            foreach (var item in carSetting.gSets.Keys)
                            {
                                var point3d = new Point3D(0, 0, 0);

                                double dx = 0;
                                double dy = 0;
                                double dz = 0;


                                string address = (int.Parse(camIODict[IOName]["X"].Address.Substring(1, camIODict[IOName]["X"].Address.Length - 1)) + (item - 1) * 12).ToString();
                                var writeXRef = plc.Write("D" + address, (int)(point3d.X * 100));
                                address = (int.Parse(camIODict[IOName]["Y"].Address.Substring(1, camIODict[IOName]["Y"].Address.Length - 1)) + (item - 1) * 12).ToString();
                                var writeYRef = plc.Write("D" + address, (int)(point3d.Y * 100));
                                address = (int.Parse(camIODict[IOName]["Z"].Address.Substring(1, camIODict[IOName]["Z"].Address.Length - 1)) + (item - 1) * 12).ToString();
                                var writeZRef = plc.Write("D" + address, (int)(point3d.Z * 100));
                                address = (int.Parse(camIODict[IOName]["Dx"].Address.Substring(1, camIODict[IOName]["Dx"].Address.Length - 1)) + (item - 1) * 12).ToString();
                                var writeDXRef = plc.Write("D" + address, (int)(dx * 100));
                                address = (int.Parse(camIODict[IOName]["Dy"].Address.Substring(1, camIODict[IOName]["Dy"].Address.Length - 1)) + (item - 1) * 12).ToString();
                                var writeDYRef = plc.Write("D" + address, (int)(dy * 100));
                                address = (int.Parse(camIODict[IOName]["Dz"].Address.Substring(1, camIODict[IOName]["Dz"].Address.Length - 1)) + (item - 1) * 12).ToString();
                                var writeDZRef = plc.Write("D" + address, (int)(dz * 100));
                            }
                            IOName = "R";
                            carSetting = 车型参数[$"{carKind.Content}-{TRnum}"].car[IOName];

                            foreach (var item in carSetting.gSets.Keys)
                            {
                                var point3d = new Point3D(0, 0, 0);

                                double dx = 0;
                                double dy = 0;
                                double dz = 0;

                                string address = (int.Parse(camIODict[IOName]["X"].Address.Substring(1, camIODict[IOName]["X"].Address.Length - 1)) + (item - 1) * 12).ToString();
                                var writeXRef = plc.Write("D" + address, (int)(point3d.X * 100));
                                address = (int.Parse(camIODict[IOName]["Y"].Address.Substring(1, camIODict[IOName]["Y"].Address.Length - 1)) + (item - 1) * 12).ToString();
                                var writeYRef = plc.Write("D" + address, (int)(point3d.Y * 100));
                                address = (int.Parse(camIODict[IOName]["Z"].Address.Substring(1, camIODict[IOName]["Z"].Address.Length - 1)) + (item - 1) * 12).ToString();
                                var writeZRef = plc.Write("D" + address, (int)(point3d.Z * 100));
                                address = (int.Parse(camIODict[IOName]["Dx"].Address.Substring(1, camIODict[IOName]["Dx"].Address.Length - 1)) + (item - 1) * 12).ToString();
                                var writeDXRef = plc.Write("D" + address, (int)(dx * 100));
                                address = (int.Parse(camIODict[IOName]["Dy"].Address.Substring(1, camIODict[IOName]["Dy"].Address.Length - 1)) + (item - 1) * 12).ToString();
                                var writeDYRef = plc.Write("D" + address, (int)(dy * 100));
                                address = (int.Parse(camIODict[IOName]["Dz"].Address.Substring(1, camIODict[IOName]["Dz"].Address.Length - 1)) + (item - 1) * 12).ToString();
                                var writeDZRef = plc.Write("D" + address, (int)(dz * 100));
                            }
                        }
                        // 延时，不知道是否真的能把结果先复位
                        Thread.Sleep(100);

                        // 输出点结果信号
                        if (point3dBaseL.Count() + point3dBaseR.Count() >= 3)
                        {
                            foreach (var item in point3dLNew.Keys)
                            {
                                string IOName = "L";
                                var carSetting = 车型参数[$"{carKind.Content}-{TRnum}"].car[IOName];
                                var point3d = point3dLNew[item];

                                double dx = point3d.X - carSetting.gSets[item].X;
                                double dy = point3d.Y - carSetting.gSets[item].Y;
                                double dz = point3d.Z - carSetting.gSets[item].Z;


                                string address = (int.Parse(camIODict[IOName]["X"].Address.Substring(1, camIODict[IOName]["X"].Address.Length - 1)) + (item - 1) * 12).ToString();
                                var writeXRef = plc.Write("D" + address, (int)(point3d.X * 100));
                                address = (int.Parse(camIODict[IOName]["Y"].Address.Substring(1, camIODict[IOName]["Y"].Address.Length - 1)) + (item - 1) * 12).ToString();
                                var writeYRef = plc.Write("D" + address, (int)(point3d.Y * 100));
                                address = (int.Parse(camIODict[IOName]["Z"].Address.Substring(1, camIODict[IOName]["Z"].Address.Length - 1)) + (item - 1) * 12).ToString();
                                var writeZRef = plc.Write("D" + address, (int)(point3d.Z * 100));
                                address = (int.Parse(camIODict[IOName]["Dx"].Address.Substring(1, camIODict[IOName]["Dx"].Address.Length - 1)) + (item - 1) * 12).ToString();
                                var writeDXRef = plc.Write("D" + address, (int)(dx * 100));
                                address = (int.Parse(camIODict[IOName]["Dy"].Address.Substring(1, camIODict[IOName]["Dy"].Address.Length - 1)) + (item - 1) * 12).ToString();
                                var writeDYRef = plc.Write("D" + address, (int)(dy * 100));
                                address = (int.Parse(camIODict[IOName]["Dz"].Address.Substring(1, camIODict[IOName]["Dz"].Address.Length - 1)) + (item - 1) * 12).ToString();
                                var writeDZRef = plc.Write("D" + address, (int)(dz * 100));

                            }
                            foreach (var item in point3dRNew.Keys)
                            {
                                string IOName = "R";
                                var carSetting = 车型参数[$"{carKind.Content}-{TRnum}"].car[IOName];
                                var point3d = point3dRNew[item];

                                double dx = point3d.X - carSetting.gSets[item].X;
                                double dy = point3d.Y - carSetting.gSets[item].Y;
                                double dz = point3d.Z - carSetting.gSets[item].Z;


                                string address = (int.Parse(camIODict[IOName]["X"].Address.Substring(1, camIODict[IOName]["X"].Address.Length - 1)) + (item - 1) * 12).ToString();
                                var writeXRef = plc.Write("D" + address, (int)(point3d.X * 100));
                                address = (int.Parse(camIODict[IOName]["Y"].Address.Substring(1, camIODict[IOName]["Y"].Address.Length - 1)) + (item - 1) * 12).ToString();
                                var writeYRef = plc.Write("D" + address, (int)(point3d.Y * 100));
                                address = (int.Parse(camIODict[IOName]["Z"].Address.Substring(1, camIODict[IOName]["Z"].Address.Length - 1)) + (item - 1) * 12).ToString();
                                var writeZRef = plc.Write("D" + address, (int)(point3d.Z * 100));
                                address = (int.Parse(camIODict[IOName]["Dx"].Address.Substring(1, camIODict[IOName]["Dx"].Address.Length - 1)) + (item - 1) * 12).ToString();
                                var writeDXRef = plc.Write("D" + address, (int)(dx * 100));
                                address = (int.Parse(camIODict[IOName]["Dy"].Address.Substring(1, camIODict[IOName]["Dy"].Address.Length - 1)) + (item - 1) * 12).ToString();
                                var writeDYRef = plc.Write("D" + address, (int)(dy * 100));
                                address = (int.Parse(camIODict[IOName]["Dz"].Address.Substring(1, camIODict[IOName]["Dz"].Address.Length - 1)) + (item - 1) * 12).ToString();
                                var writeDZRef = plc.Write("D" + address, (int)(dz * 100));
                            }
                        }
                        else 
                        {
                            foreach (var item in point3dL.Keys)
                            {
                                string IOName = "L";
                                var carSetting = 车型参数[$"{carKind.Content}-{TRnum}"].car[IOName];
                                var point3d = point3dL[item];

                                double dx = point3d.X - carSetting.gSets[item].X;
                                double dy = point3d.Y - carSetting.gSets[item].Y;
                                double dz = point3d.Z - carSetting.gSets[item].Z;


                                string address = (int.Parse(camIODict[IOName]["X"].Address.Substring(1, camIODict[IOName]["X"].Address.Length - 1)) + (item - 1) * 12).ToString();
                                var writeXRef = plc.Write("D" + address, (int)(point3d.X * 100));
                                address = (int.Parse(camIODict[IOName]["Y"].Address.Substring(1, camIODict[IOName]["Y"].Address.Length - 1)) + (item - 1) * 12).ToString();
                                var writeYRef = plc.Write("D" + address, (int)(point3d.Y * 100));
                                address = (int.Parse(camIODict[IOName]["Z"].Address.Substring(1, camIODict[IOName]["Z"].Address.Length - 1)) + (item - 1) * 12).ToString();
                                var writeZRef = plc.Write("D" + address, (int)(point3d.Z * 100));
                                address = (int.Parse(camIODict[IOName]["Dx"].Address.Substring(1, camIODict[IOName]["Dx"].Address.Length - 1)) + (item - 1) * 12).ToString();
                                var writeDXRef = plc.Write("D" + address, (int)(dx * 100));
                                address = (int.Parse(camIODict[IOName]["Dy"].Address.Substring(1, camIODict[IOName]["Dy"].Address.Length - 1)) + (item - 1) * 12).ToString();
                                var writeDYRef = plc.Write("D" + address, (int)(dy * 100));
                                address = (int.Parse(camIODict[IOName]["Dz"].Address.Substring(1, camIODict[IOName]["Dz"].Address.Length - 1)) + (item - 1) * 12).ToString();
                                var writeDZRef = plc.Write("D" + address, (int)(dz * 100));
                            }
                            foreach (var item in point3dR.Keys)
                            {
                                string IOName = "R";
                                var carSetting = 车型参数[$"{carKind.Content}-{TRnum}"].car[IOName];
                                var point3d = point3dR[item];

                                double dx = point3d.X - carSetting.gSets[item].X;
                                double dy = point3d.Y - carSetting.gSets[item].Y;
                                double dz = point3d.Z - carSetting.gSets[item].Z;


                                string address = (int.Parse(camIODict[IOName]["X"].Address.Substring(1, camIODict[IOName]["X"].Address.Length - 1)) + (item - 1) * 12).ToString();
                                var writeXRef = plc.Write("D" + address, (int)(point3d.X * 100));
                                address = (int.Parse(camIODict[IOName]["Y"].Address.Substring(1, camIODict[IOName]["Y"].Address.Length - 1)) + (item - 1) * 12).ToString();
                                var writeYRef = plc.Write("D" + address, (int)(point3d.Y * 100));
                                address = (int.Parse(camIODict[IOName]["Z"].Address.Substring(1, camIODict[IOName]["Z"].Address.Length - 1)) + (item - 1) * 12).ToString();
                                var writeZRef = plc.Write("D" + address, (int)(point3d.Z * 100));
                                address = (int.Parse(camIODict[IOName]["Dx"].Address.Substring(1, camIODict[IOName]["Dx"].Address.Length - 1)) + (item - 1) * 12).ToString();
                                var writeDXRef = plc.Write("D" + address, (int)(dx * 100));
                                address = (int.Parse(camIODict[IOName]["Dy"].Address.Substring(1, camIODict[IOName]["Dy"].Address.Length - 1)) + (item - 1) * 12).ToString();
                                var writeDYRef = plc.Write("D" + address, (int)(dy * 100));
                                address = (int.Parse(camIODict[IOName]["Dz"].Address.Substring(1, camIODict[IOName]["Dz"].Address.Length - 1)) + (item - 1) * 12).ToString();
                                var writeDZRef = plc.Write("D" + address, (int)(dz * 100));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ShowMessage($"{Resources.LanguageDic.reconstruct_coordinates_error}：" + ex.ToString(), Color.Red);
                    }

                    if (!bDry_mode)
                    {
                        try
                        {
                            if (sqlEnable)
                            {
                                sql?.Open();
                                sql?.BeginTransaction();
                                try
                                {
                                    Dictionary<string, string> 车辆信息表 = new Dictionary<string, string>
                                    {
                                        { "时间", dateTimeNow.ToString("yyyy-MM-dd HH:mm:ss") },
                                        { "车型", carKind.Content.ToString() },
                                        { "连番", carNum.ToString() },
                                        { "托盘号", TRnum.ToString() },
                                        { "车架号", carNumString }
                                    };
                                    int r = sql.InsertRow(sql.Database + ".车辆信息表", 车辆信息表);

                                    foreach (var item in sqlValuePairsL.Keys)
                                    {
                                        sql.InsertRow(sql.Database + ".测点数据表", sqlValuePairsL[item]);
                                    }
                                    foreach (var item in sqlValuePairsR.Keys)
                                    {
                                        sql.InsertRow(sql.Database + ".测点数据表", sqlValuePairsR[item]);
                                    }

                                    sql.Commit();
                                }
                                catch (Exception ex)
                                {
                                    sql.Rollback();
                                    ShowMessage(ex.ToString(), Color.Red);
                                }
                                sql?.Close();
                            }
                        }
                        catch (Exception ex)
                        {
                            ShowMessage($"{Resources.LanguageDic.database_write_error}：{ex}", Color.Red);
                        }
                    }

                    //处理结果
                    bool bResult = 数据超差L && 数据超差R;

                    ShowMessage($"{Resources.LanguageDic.check_result}" + bResult);

                    if (checkBox_KeepOK.Checked)
                    {
                        bResult = true;
                    }


                    //输出信号
                    if (!plc.Write(camIODict["L"]["Result"].Address, bResult).IsSuccess)
                    {
                        ShowMessage($"{Resources.LanguageDic.check_result_write_fail}", Color.Red);
                    }
                    if (!plc.Write(camIODict["L"]["Check_Finish"].Address, true).IsSuccess)
                    {
                        ShowMessage($"{Resources.LanguageDic.check_result_readable_write_fail}", Color.Red);
                    }
                    if (!plc.Write(camIODict["L"]["Running"].Address, false).IsSuccess)
                    {
                        ShowMessage($"{Resources.LanguageDic.running_write_fail}", Color.Red);
                    }

                    lblSysStatus.BeginInvoke(new Action(() =>
                    {
                        lblSysStatus.Text = $"{Resources.LanguageDic.finish}";
                        lblSysStatus.BackColor = Color.White;
                        labelResult.Text = bResult ? "OK" : "NG";
                        labelResult.BackColor = bResult ? Color.Green : Color.Red;
                    }));

                    ShowMessage($"{Resources.LanguageDic.wait_for_read_signal}");
                    while (true)
                    {
                        if (stop) return;
                        var val = plc.ReadBool(camIODict["L"]["Readed"].Address);
                        if (val.IsSuccess)
                        {
                            if (val.Content == true)
                            {
                                ShowMessage($"{Resources.LanguageDic.reserve_read_signal}");
                                break;
                            }
                        }
                        else
                        {
                            ShowMessage($"{Resources.LanguageDic.read_signal_read_fail}", Color.Red);
                            return;
                        }
                        var abort = plc.ReadBool(camIODict["L"]["Reset"].Address);
                        if (!abort.IsSuccess)
                        {
                            ShowMessage($"{Resources.LanguageDic.reset_signal_read_fail}", Color.Red);
                            return;
                        }
                        if (bAbort || abort.Content)
                        {
                            bAbort = true;
                            ShowMessage($"{Resources.LanguageDic.reserve_reset_signal}", Color.Yellow);
                            break;
                        }
                        Thread.Sleep(50);
                    }
                    sp.Stop();
                    ShowMessage($"{Resources.LanguageDic.use_time}" + (sp.ElapsedMilliseconds / 1000f).ToString("0.00") + "s");
                    Thread.Sleep(500);
                }
            }
            catch (Exception ex)
            {
                ShowMessage(ex.ToString(), Color.Red);
            }
            finally
            {
                stop = true;
                alive.Enabled = false;
                threadSaveImage?.Join();
                if (taskL != null)
                {
                    while (!taskL.IsCompleted)
                    {
                        Thread.Sleep(7);
                    }
                }
                if (taskR != null)
                {
                    while (!taskR.IsCompleted)
                    {
                        Thread.Sleep(7);
                    }
                }
                //if (sp.IsOpen)
                //{
                //    sp.Close();
                //}
                foreach (var cam in 相机.Values)
                {
                    cam?.Close();
                }
                //plc?.ConnectClose();
                StopState();
            }
        }

        private void RobotRun(string path, BaslerCamera.Cam cam, CamSetting camSetting, CarSetting carSetting,string camName ,Dictionary<string,IoAddress> IO, out Dictionary<int, Point3D> point3d, out Dictionary<int, Point3D> point3dBase, out Dictionary<int, Dictionary<string, string>> sqlValuePairs, ref bool 检测异常, ref bool 数据超差)
        {
            point3d = new Dictionary<int, Point3D>();
            point3dBase = new Dictionary<int, Point3D>();
            sqlValuePairs = new Dictionary<int, Dictionary<string, string>>();
            int pictureBoxIndex = 0;
            System.Diagnostics.Stopwatch sp = new System.Diagnostics.Stopwatch();
            while (true)
            {
                pictureBoxIndex++;

                ShowMessage(camName + $"{Resources.LanguageDic.wait_for_point_NO}");

                if (stop) return;
                sp.Restart();
                //等点位号

                //if (!pointNumList.IsSuccess)
                //{
                //    ShowMessage(camName + $"{Resources.LanguageDic.Point_number_reading_failed}", Color.Red);
                //    return;
                //}

                int pointNum = 0;
                while (true)
                {
                    if (stop) return;
                    var pointNumList = plc.ReadInt16(IO["Check_Point_NO"].Address);
                    if (pointNumList.IsSuccess)
                    {
                        if (pointNumList.Content != 0)
                        {
                            pointNum = (int)(pointNumList.Content);
                            ShowMessage($"{camName} { Resources.LanguageDic.success_get_point_NO} {pointNum}，{Resources.LanguageDic.use_time2}{sp.ElapsedMilliseconds}ms");
                            break;
                        }
                    } 
                    else
                    {
                        ShowMessage(camName + $"{Resources.LanguageDic.fail_get_point_NO}", Color.Red);
                        return;
                    }
                    var abort = plc.ReadBool(camIODict["L"]["Reset"].Address);
                    if (!abort.IsSuccess)
                    {
                        ShowMessage($"{Resources.LanguageDic.reset_signal_read_fail}", Color.Red);
                        return;
                    }
                    if (bAbort || abort.Content)
                    {
                        bAbort = true;
                        ShowMessage(camName + $"{Resources.LanguageDic.reserve_reset_signal}", Color.Yellow);
                        return;
                    }
                    Thread.Sleep(50);
                }

                //点位号反馈
                var writepointNumList = plc.Write(IO["Feedback_Check_Point_NO"].Address, pointNum);
                if (!writepointNumList.IsSuccess)
                {
                    ShowMessage(camName + $"{Resources.LanguageDic.Point_number_writing_failed}", Color.Red);
                    return;
                }

                ShowMessage(camName + $"{Resources.LanguageDic.Waiting_for_the_signal_that_the_camera_posture_is_in_place}ON");

                sp.Restart();

                //等姿态到位
                while (true)
                {
                    if (stop) return;
                    var val = plc.ReadBool(IO["Arrive_Photo_Spot"].Address);
                    if (val.IsSuccess)
                    {
                        if (val.Content == true)
                        {
                            ShowMessage($"{camName}{Resources.LanguageDic.reserve_the_signal_that_the_camera_posture_is_in_place}ON，{Resources.LanguageDic.use_time2}{sp.ElapsedMilliseconds}ms");
                            break;
                        }
                    }
                    else
                    {
                        ShowMessage(camName + $"{Resources.LanguageDic.Photo_posture_in_place_signal_reading_failed}", Color.Red);
                        return;
                    }
                    var abort = plc.ReadBool(camIODict["L"]["Reset"].Address );
                    if (!abort.IsSuccess)
                    {
                        ShowMessage($"{Resources.LanguageDic.reset_signal_read_fail}", Color.Red);
                        return;
                    }
                    if (bAbort || abort.Content)
                    {
                        bAbort = true;
                        ShowMessage(camName + $"{Resources.LanguageDic.reserve_reset_signal}", Color.Yellow);
                        return;
                    }
                    Thread.Sleep(50);
                }
                //读取点位号
                //var point = plc.ReadBool(IO.点位号1, 4);
                //if (!point.IsSuccess)
                //{
                //    ShowMessage(camName + $"{Resources.LanguageDic.Point_number_reading_failed}", Color.Red);
                //    return;
                //}
                //int pointNum = 0;
                //pointNum += point.Content[0] ? 1 : 0;
                //pointNum += point.Content[1] ? 2 : 0;
                //pointNum += point.Content[2] ? 4 : 0;
                //pointNum += point.Content[3] ? 8 : 0;

                


                //是否末点
                var endPoint = plc.ReadBool(IO["End_of_Check_Points"].Address);
                if (!endPoint.IsSuccess)
                {
                    ShowMessage(camName + $"{Resources.LanguageDic.Trajectory_endpoint_reading_failed}", Color.Red);
                    return;
                }
                ShowMessage(camName + $"{Resources.LanguageDic.reserve_end_point}" + pointNum);

                if (endPoint.Content)
                {
                    ShowMessage(camName + $"{Resources.LanguageDic.end_point}");
                }

                formShow?.UpData灯(camName, true);///////////////////////////////////

                //采图，计算
                HImage hImage2D = null;
                HImage hImageLight = null;
                HImage hImage2D匹配图 = null;
                HImage hImageLight匹配图 = null;
                float Px0 = 0, Py0 = 0, Px1 = 0, Py1 = 0, Px2 = 0, Py2 = 0; // 0是2D点的匹配结果，1、2是激光图拐角点的匹配结果

                bool led匹配 = false;
                bool light匹配1 = false;
                bool light匹配2 = false;
                sp.Restart();
                try
                {
                    if (carSetting == null || !carSetting.gSets.ContainsKey(pointNum))//该车型或该测点未做参数,只拍照显示
                    {
                        Thread.Sleep(1000);//到位后延时拍照
                        ShowMessage($"{camName}_{pointNum} {Resources.LanguageDic.para_has_not_made}", Color.Yellow);
                        检测异常 = false;
                        //激光图
                        //Led_Light(camName, false, true);
                        if (cam.SetExposure(600))
                        {
                            cam.SetLine1Inverter(true);
                            Thread.Sleep(20);
                            cam.OneShot(out hImageLight);
                            cam.SetLine1Inverter(false);

                            if (hImageLight != null)
                            {
                                formShow?.UpDataCamImage(hImageLight, camName + pictureBoxIndex);//////////////////////////
                            }
                            else
                            {
                                ShowMessage(camName + $"{Resources.LanguageDic.cam1_fail}", Color.Red);
                            }
                        }
                        else
                        {
                            ShowMessage(camName + $"{Resources.LanguageDic.set_cam1_export_fail}", Color.Red);
                        }
                        Thread.Sleep(20);
                        //平面图
                        //Led_Light(camName, true, false);
                        if (cam.SetExposure(4000))
                        {
                            cam.SetLine2Inverter(true);

                            Thread.Sleep(20);
                            cam.OneShot(out hImage2D);
                            cam.SetLine2Inverter(false);

                            if (hImage2D != null)
                            {
                                formShow?.UpDataCamImage(hImage2D, camName + pictureBoxIndex);//////////////////////////
                            }
                            else
                            {
                                ShowMessage(camName + $"{Resources.LanguageDic.cam0_fail}", Color.Red);
                            }
                        }
                        else
                        {
                            ShowMessage(camName + $"{Resources.LanguageDic.set_cam0_export_fail}", Color.Red);
                        }
                    }
                    else//该车型已做参数
                    {
                        Thread.Sleep(carSetting.gSets[pointNum].sleepTime);//到位后延时拍照
                        //激光图
                        //Led_Light(camName, false, true);
                        if (cam.SetExposure(carSetting.gSets[pointNum].lightExposure))
                        {
                            cam.SetLine1Inverter(true);

                            Thread.Sleep(20);
                            cam.OneShot(out hImageLight);
                            cam.SetLine1Inverter(false);

                            if (hImageLight != null)
                            {
                                formShow?.UpDataCamImage(hImageLight, camName + pictureBoxIndex);//////////////////////////
                                try
                                {
                                    //匹配计算
                                    if (camSetting != null)
                                    {
                                        int flag = 0;
                                        HImage map = hImageLight.MapImage(camSetting.mapImage);
                                        if (carSetting.rectangle1.ContainsKey(pointNum) && carSetting.Models1.ContainsKey(pointNum))
                                        {
                                            if (carSetting.FindPxPy(map, carSetting.rectangle1[pointNum], carSetting.Models1[pointNum], carSetting.gSets[pointNum].score1, carSetting.modeCenter1[pointNum].X, carSetting.modeCenter1[pointNum].Y, ref Px1, ref Py1, out double dx, out double dy))
                                            {
                                                light匹配1 = true;
                                                ShowMessage($"{camName}_{pointNum}_1 dx = {dx:0.000}{Resources.LanguageDic.pix}, dy = {dy:0.000}{Resources.LanguageDic.pix}");
                                            }
                                            else
                                            {
                                                ShowMessage($"{camName}_{pointNum}_1 {Resources.LanguageDic.Match_fail}", Color.Red);
                                                检测异常 = false;
                                            }
                                            flag += 1;
                                        }
                                        else
                                        {
                                            ShowMessage($"{camName}_{pointNum}_1 {Resources.LanguageDic.no_model}", Color.Yellow);
                                            检测异常 = false;
                                        }

                                        if (carSetting.gSets[pointNum].type != "棱")//不是棱不可跳过
                                        {
                                            if (carSetting.rectangle2.ContainsKey(pointNum) && carSetting.Models2.ContainsKey(pointNum))
                                            {
                                                if (carSetting.FindPxPy(map, carSetting.rectangle2[pointNum], carSetting.Models2[pointNum], carSetting.gSets[pointNum].score2, carSetting.modeCenter2[pointNum].X, carSetting.modeCenter2[pointNum].Y, ref Px2, ref Py2, out double dx, out double dy))
                                                {
                                                    light匹配2 = true;
                                                    ShowMessage($"{camName}_{pointNum}_2 dx = {dx:0.000}{Resources.LanguageDic.pix}, dy = {dy:0.000}{Resources.LanguageDic.pix}");
                                                }
                                                else
                                                {
                                                    ShowMessage($"{camName}_{pointNum}_2 {Resources.LanguageDic.Match_fail}", Color.Red);
                                                    检测异常 = false;
                                                }
                                                flag += 2;
                                            }
                                            else
                                            {
                                                ShowMessage($"{camName}_{pointNum}_2 {Resources.LanguageDic.no_model}", Color.Yellow);
                                                检测异常 = false;
                                            }
                                        }

                                        if (flag == 1)
                                        {
                                            hImageLight匹配图 = carSetting.GetShowHIamge(map, carSetting.rectangle1[pointNum], carSetting.modeCenter1[pointNum].X, carSetting.modeCenter1[pointNum].Y, Px1, Py1);
                                        }
                                        else if (flag == 2)
                                        {
                                            hImageLight匹配图 = carSetting.GetShowHIamge(map, carSetting.rectangle2[pointNum], carSetting.modeCenter2[pointNum].X, carSetting.modeCenter2[pointNum].Y, Px2, Py2);
                                        }
                                        else if (flag == 3)
                                        {
                                            hImageLight匹配图 = carSetting.GetShowHIamge(map, carSetting.rectangle1[pointNum], carSetting.modeCenter1[pointNum].X, carSetting.modeCenter1[pointNum].Y, Px1, Py1, carSetting.rectangle2[pointNum], carSetting.modeCenter2[pointNum].X, carSetting.modeCenter2[pointNum].Y, Px2, Py2);
                                        }

                                        if (hImageLight匹配图 != null)
                                        {
                                            formShow?.UpDataCamImage(hImageLight匹配图, camName + pictureBoxIndex);/////////////////////////
                                        }
                                    }
                                    else
                                    {
                                        ShowMessage($"{camName} {Resources.LanguageDic.no_cam_calib}", Color.Red);
                                        检测异常 = false;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    ShowMessage($"{camName}_{pointNum}_1 {Resources.LanguageDic.match_error}:{ex.Message}", Color.Red);
                                    检测异常 = false;
                                }
                            }
                            else
                            {
                                ShowMessage(camName + $"{Resources.LanguageDic.cam1_fail}", Color.Red);
                                检测异常 = false;
                            }
                        }
                        else
                        {
                            ShowMessage(camName + $"{Resources.LanguageDic.set_cam1_export_fail}", Color.Red);
                            检测异常 = false;
                        }

                        Thread.Sleep(20);
                        if (carSetting.gSets[pointNum].type != "棱" && carSetting.gSets[pointNum].type != "槽")//不是棱、不是槽不可以跳过
                        {
                            //平面图
                            //Led_Light(camName, true, false);
                            if (cam.SetExposure(carSetting.gSets[pointNum].ledExposure))
                            {
                                cam.SetLine2Inverter(true);

                                Thread.Sleep(20);
                                cam.OneShot(out hImage2D);
                                cam.SetLine2Inverter(false);

                                if (hImage2D != null)
                                {
                                    formShow?.UpDataCamImage(hImage2D, camName + pictureBoxIndex);//////////////////////////
                                    try
                                    {
                                        //匹配计算
                                        if (camSetting != null)
                                        {
                                            HImage map = hImage2D.MapImage(camSetting.mapImage);
                                            if (carSetting.rectangle0.ContainsKey(pointNum) && carSetting.ShapeModels0.ContainsKey(pointNum))
                                            {
                                                if (carSetting.FindPxPy(map, carSetting.rectangle0[pointNum], carSetting.ShapeModels0[pointNum], carSetting.gSets[pointNum].score0, carSetting.modeCenter0[pointNum].X, carSetting.modeCenter0[pointNum].Y, ref Px0, ref Py0, out double dx, out double dy))
                                                {
                                                    led匹配 = true;
                                                    ShowMessage($"{camName}_{pointNum}_0 dx = {dx:0.000}{Resources.LanguageDic.pix}, dy = {dy:0.000}{Resources.LanguageDic.pix}");
                                                }
                                                else
                                                {
                                                    ShowMessage($"{camName}_{pointNum}_0 {Resources.LanguageDic.Match_fail}", Color.Red);
                                                    检测异常 = false;
                                                }
                                                hImage2D匹配图 = carSetting.GetShowHIamge(map, carSetting.rectangle0[pointNum], carSetting.modeCenter0[pointNum].X, carSetting.modeCenter0[pointNum].Y, Px0, Py0, carSetting.ShapeModels0[pointNum]);
                                                formShow?.UpDataCamImage(hImage2D匹配图, camName + pictureBoxIndex);/////////////////////////
                                            }
                                            else if (carSetting.rectangle0.ContainsKey(pointNum) && carSetting.Models0.ContainsKey(pointNum))
                                            {
                                                if (carSetting.FindPxPy(map, carSetting.rectangle0[pointNum], carSetting.Models0[pointNum], carSetting.gSets[pointNum].score0, carSetting.modeCenter0[pointNum].X, carSetting.modeCenter0[pointNum].Y, ref Px0, ref Py0, out double dx, out double dy))
                                                {
                                                    led匹配 = true;
                                                    ShowMessage($"{camName}_{pointNum}_0 dx = {dx:0.000}{Resources.LanguageDic.pix}, dy = {dy:0.000}{Resources.LanguageDic.pix}");
                                                }
                                                else
                                                {
                                                    ShowMessage($"{camName}_{pointNum}_0 {Resources.LanguageDic.Match_fail}", Color.Red);
                                                    检测异常 = false;
                                                }
                                                hImage2D匹配图 = carSetting.GetShowHIamge(map, carSetting.rectangle0[pointNum], carSetting.modeCenter0[pointNum].X, carSetting.modeCenter0[pointNum].Y, Px0, Py0);
                                                formShow?.UpDataCamImage(hImage2D匹配图, camName + pictureBoxIndex);/////////////////////////
                                            }
                                            else
                                            {
                                                ShowMessage($"{camName}_{pointNum}_0 {Resources.LanguageDic.para_has_not_made}", Color.Yellow);
                                                检测异常 = false;
                                            }
                                        }
                                        else
                                        {
                                            ShowMessage($"{camName} {Resources.LanguageDic.no_cam_calib}", Color.Red);
                                            检测异常 = false;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        ShowMessage($"{camName}_{pointNum}_0 {Resources.LanguageDic.match_error}:{ex}", Color.Red);
                                        检测异常 = false;
                                    }
                                }
                                else
                                {
                                    ShowMessage(camName + $"{Resources.LanguageDic.cam0_fail}", Color.Red);
                                    检测异常 = false;
                                }
                            }
                            else
                            {
                                ShowMessage(camName + $"{Resources.LanguageDic.set_cam0_export_fail}", Color.Red);
                                检测异常 = false;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ShowMessage($"{camName}_{pointNum} {Resources.LanguageDic.take_photo_error}:{ex}", Color.Red);
                    检测异常 = false;
                }

                //Led_Light(camName, false, false);
                ShowMessage($"{camName}_{pointNum}{Resources.LanguageDic.take_photo_finish}，{Resources.LanguageDic.use_time2}{sp.ElapsedMilliseconds}ms");

                if (!plc.Write(IO["Acq_Finish"].Address, true).IsSuccess)
                {
                    ShowMessage(camName + $"{Resources.LanguageDic.take_photo_finish_write_fail}", Color.Red);
                }
                else
                {
                    ShowMessage(camName + $"{Resources.LanguageDic.take_photo_finish_write}ON");
                }
                ShowMessage(camName + $"{Resources.LanguageDic.Waiting_for_the_signal_that_the_camera_posture_is_in_place}OFF");
                sp.Restart();
                while (true)
                {
                    if (stop) return;
                    var val = plc.ReadBool(IO["Arrive_Photo_Spot"].Address);
                    if (val.IsSuccess)
                    {
                        if (val.Content == false)
                        {
                            ShowMessage($"{camName}{Resources.LanguageDic.reserve_the_signal_that_the_camera_posture_is_in_place}OFF，耗时{sp.ElapsedMilliseconds}ms");
                            break;
                        }
                    }
                    else
                    {
                        ShowMessage(camName + $"{Resources.LanguageDic.Photo_posture_in_place_signal_reading_failed}", Color.Red);
                        return;
                    }
                    var abort = plc.ReadBool(camIODict["L"]["Reset"].Address );
                    if (!abort.IsSuccess)
                    {
                        ShowMessage($"{Resources.LanguageDic.reset_signal_read_fail}", Color.Red);
                        return;
                    }
                    if (bAbort || abort.Content)
                    {
                        bAbort = true;
                        ShowMessage(camName + $"{Resources.LanguageDic.reserve_reset_signal}", Color.Yellow);
                        return;
                    }
                    Thread.Sleep(10);
                }

                //反馈点位号
                if (!plc.Write(IO["Feedback_Check_Point_NO"].Address, 0).IsSuccess)
                {
                    ShowMessage(camName + $"{Resources.LanguageDic.Point_number_writing_failed}", Color.Red);
                    return;
                }


                //拍照完成
                if (!plc.Write(IO["Acq_Finish"].Address, false).IsSuccess)
                {
                    ShowMessage(camName + $"{Resources.LanguageDic.take_photo_finish_write_fail}", Color.Red);
                }
                else
                {
                    ShowMessage(camName + $"{Resources.LanguageDic.take_photo_finish_write}OFF");
                }

                ShowMessage(camName + $"{Resources.LanguageDic.Start_calculating_coordinates}");
                sp.Restart();
                double X = 0, Y = 0, Z = 0;
                Dictionary<string, string> 测点数据表 = new Dictionary<string, string>();
                bool b有数据 = false;
                if (light匹配1 && carSetting.gSets[pointNum].type == "棱")
                {
                    if (camSetting.GetZFromLight(Px1, Py1, out float camZ) && camSetting.GetXYFromLight(Px1, Py1, camZ, out float camX, out float camY))
                    {
                        //相机转工具
                        double toolX = camSetting.cam2Tool.AffineTransPoint3d(camX, camY, camZ, out double toolY, out double toolZ);

                        // 转为mm为单位
                        double toolX_mm = toolX * 1000;
                        double toolY_mm = toolY * 1000;
                        double toolZ_mm = toolZ * 1000;

                        //工具转基座标
                        HHomMat3D 工具转基座标 = new HPose(carSetting.gSets[pointNum].pX, carSetting.gSets[pointNum].pY, carSetting.gSets[pointNum].pZ, carSetting.gSets[pointNum].pRX, carSetting.gSets[pointNum].pRY, carSetting.gSets[pointNum].pRZ, "Rp+T", "abg", "point").PoseToHomMat3d();
                        double rbX = 工具转基座标.AffineTransPoint3d(toolX_mm, toolY_mm, toolZ_mm, out double rbY, out double rbZ);


                        //基座标转车身
                        double carX = carSetting.robot2Car.AffineTransPoint3d(rbX, rbY, rbZ, out double carY, out double carZ);


                        //车身加补偿
                        X = carX + carSetting.gSets[pointNum].offsetX;
                        Y = carY + carSetting.gSets[pointNum].offsetY;
                        Z = carZ + carSetting.gSets[pointNum].offsetZ;

                        if (carSetting.gSets[pointNum].isBase)//重建坐标用
                        {
                            point3dBase.Add(pointNum, new Point3D(carSetting.gSets[pointNum].X, carSetting.gSets[pointNum].Y, carSetting.gSets[pointNum].Z));
                        }

                        测点数据表.Add("相机X", camX.ToString("0.000"));
                        测点数据表.Add("相机Y", camY.ToString("0.000"));
                        测点数据表.Add("相机Z", camZ.ToString("0.000"));
                        测点数据表.Add("工具X", toolX.ToString("0.000"));
                        测点数据表.Add("工具Y", toolY.ToString("0.000"));
                        测点数据表.Add("工具Z", toolZ.ToString("0.000"));
                        测点数据表.Add("基座X", rbX.ToString("0.000"));
                        测点数据表.Add("基座Y", rbY.ToString("0.000"));
                        测点数据表.Add("基座Z", rbZ.ToString("0.000"));
                        测点数据表.Add("车身X", carX.ToString("0.000"));
                        测点数据表.Add("车身Y", carY.ToString("0.000"));
                        测点数据表.Add("车身Z", carZ.ToString("0.000"));

                        b有数据 = true;
                    }
                    else
                    {
                        ShowMessage($"{camName}_{pointNum}_1 {Resources.LanguageDic.The_photography_distance_exceeds_the_calibration_range}！", Color.Red);
                        检测异常 = false;
                    }
                }
                else if (light匹配1 && light匹配2 && carSetting.gSets[pointNum].type == "槽")
                {
                    if (camSetting.GetZFromLight(Px1, Py1, out float camZ1) && camSetting.GetXYFromLight(Px1, Py1, camZ1, out float camX1, out float camY1))
                    {
                        //相机转工具
                        double toolX1 = camSetting.cam2Tool.AffineTransPoint3d(camX1, camY1, camZ1, out double toolY1, out double toolZ1);

                        // 转为mm为单位
                        double toolX1_mm = toolX1 * 1000;
                        double toolY1_mm = toolY1 * 1000;
                        double toolZ1_mm = toolZ1 * 1000;

                        //工具转基座标
                        HHomMat3D 工具转基座标 = new HPose(carSetting.gSets[pointNum].pX, carSetting.gSets[pointNum].pY, carSetting.gSets[pointNum].pZ, carSetting.gSets[pointNum].pRX, carSetting.gSets[pointNum].pRY, carSetting.gSets[pointNum].pRZ, "Rp+T", "abg", "point").PoseToHomMat3d();
                        double rbX1 = 工具转基座标.AffineTransPoint3d(toolX1_mm, toolY1_mm, toolZ1_mm, out double rbY1, out double rbZ1);


                        //基座标转车身
                        double carX1 = carSetting.robot2Car.AffineTransPoint3d(rbX1, rbY1, rbZ1, out double carY1, out double carZ1);

                        if (camSetting.GetZFromLight(Px2, Py2, out float camZ2) && camSetting.GetXYFromLight(Px2, Py2, camZ2, out float camX2, out float camY2))
                        {
                            //相机转工具
                            double toolX2 = camSetting.cam2Tool.AffineTransPoint3d(camX2, camY2, camZ2, out double toolY2, out double toolZ2);

                            // 转为mm为单位
                            double toolX2_mm = toolX2 * 1000;
                            double toolY2_mm = toolY2 * 1000;
                            double toolZ2_mm = toolZ2 * 1000;

                            //工具转基座标
                            HHomMat3D 工具转基座标2 = new HPose(carSetting.gSets[pointNum].pX, carSetting.gSets[pointNum].pY, carSetting.gSets[pointNum].pZ, carSetting.gSets[pointNum].pRX, carSetting.gSets[pointNum].pRY, carSetting.gSets[pointNum].pRZ, "Rp+T", "abg", "point").PoseToHomMat3d();
                            double rbX2 = 工具转基座标2.AffineTransPoint3d(toolX2_mm, toolY2_mm, toolZ2_mm, out double rbY2, out double rbZ2);

                            //基座标转车身
                            double carX2 = carSetting.robot2Car.AffineTransPoint3d(rbX2, rbY2, rbZ2, out double carY2, out double carZ2);

                            //车身加补偿
                            X = carX2 - carX1 + carSetting.gSets[pointNum].offsetX;
                            Y = carY2 - carY1 + carSetting.gSets[pointNum].offsetY;
                            Z = carZ2 - carZ1 + carSetting.gSets[pointNum].offsetZ;

                            测点数据表.Add("相机X", camX2.ToString("0.000"));
                            测点数据表.Add("相机Y", camY2.ToString("0.000"));
                            测点数据表.Add("相机Z", camZ2.ToString("0.000"));
                            测点数据表.Add("工具X", toolX2.ToString("0.000"));
                            测点数据表.Add("工具Y", toolY2.ToString("0.000"));
                            测点数据表.Add("工具Z", toolZ2.ToString("0.000"));
                            测点数据表.Add("基座X", rbX2.ToString("0.000"));
                            测点数据表.Add("基座Y", rbY2.ToString("0.000"));
                            测点数据表.Add("基座Z", rbZ2.ToString("0.000"));
                            测点数据表.Add("车身X", carX2.ToString("0.000"));
                            测点数据表.Add("车身Y", carY2.ToString("0.000"));
                            测点数据表.Add("车身Z", carZ2.ToString("0.000"));

                            b有数据 = true;
                        }
                        else
                        {
                            ShowMessage($"{camName}_{pointNum}_2 {Resources.LanguageDic.The_photography_distance_exceeds_the_calibration_range}！", Color.Red);
                            检测异常 = false;
                        }
                    }
                    else
                    {
                        ShowMessage($"{camName}_{pointNum}_1 {Resources.LanguageDic.The_photography_distance_exceeds_the_calibration_range}！", Color.Red);
                        检测异常 = false;
                    }
                }
                else if (led匹配 && light匹配1 && light匹配2)//孔
                {
                    //转换相机坐标
                    if (camSetting.GetZFromLight(Px1, Py1, out float z1))
                    {
                        if (camSetting.GetZFromLight(Px2, Py2, out float z2))
                        {
                            float camZ = (z1 + z2) / 2;
                            if (camSetting.GetXYFromLight(Px0, Py0, camZ, out float camX, out float camY))
                            {
                                //相机转工具
                                double toolX = camSetting.cam2Tool.AffineTransPoint3d(camX, camY, camZ, out double toolY, out double toolZ);

                                // 转为mm为单位
                                double toolX_mm = toolX * 1000;
                                double toolY_mm = toolY * 1000;
                                double toolZ_mm = toolZ * 1000;

                                //工具转基座标
                                HHomMat3D 工具转基座标 = new HPose(carSetting.gSets[pointNum].pX, carSetting.gSets[pointNum].pY, carSetting.gSets[pointNum].pZ, carSetting.gSets[pointNum].pRX, carSetting.gSets[pointNum].pRY, carSetting.gSets[pointNum].pRZ, "Rp+T", "abg", "point").PoseToHomMat3d();
                                double rbX = 工具转基座标.AffineTransPoint3d(toolX_mm, toolY_mm, toolZ_mm, out double rbY, out double rbZ);


                                //基座标转车身
                                double carX = carSetting.robot2Car.AffineTransPoint3d(rbX, rbY, rbZ, out double carY, out double carZ);

                                //车身加补偿
                                X = carX + carSetting.gSets[pointNum].offsetX;
                                Y = carY + carSetting.gSets[pointNum].offsetY;
                                Z = carZ + carSetting.gSets[pointNum].offsetZ;

                                if (carSetting.gSets[pointNum].isBase)//重建坐标用
                                {
                                    point3dBase.Add(pointNum, new Point3D(carSetting.gSets[pointNum].X, carSetting.gSets[pointNum].Y, carSetting.gSets[pointNum].Z));
                                }

                                测点数据表.Add("相机X", camX.ToString("0.000"));
                                测点数据表.Add("相机Y", camY.ToString("0.000"));
                                测点数据表.Add("相机Z", camZ.ToString("0.000"));
                                测点数据表.Add("工具X", toolX.ToString("0.000"));
                                测点数据表.Add("工具Y", toolY.ToString("0.000"));
                                测点数据表.Add("工具Z", toolZ.ToString("0.000"));
                                测点数据表.Add("基座X", rbX.ToString("0.000"));
                                测点数据表.Add("基座Y", rbY.ToString("0.000"));
                                测点数据表.Add("基座Z", rbZ.ToString("0.000"));
                                测点数据表.Add("车身X", carX.ToString("0.000"));
                                测点数据表.Add("车身Y", carY.ToString("0.000"));
                                测点数据表.Add("车身Z", carZ.ToString("0.000"));

                                b有数据 = true;
                            }
                            else
                            {
                                ShowMessage($"{camName}_{pointNum}_0 {Resources.LanguageDic.The_photography_distance_exceeds_the_calibration_range}！", Color.Red);
                                检测异常 = false;
                            }
                        }
                        else
                        {
                            ShowMessage($"{camName}_{pointNum}_2 {Resources.LanguageDic.Laser_coordinates_exceed_the_calibration_range}！", Color.Red);
                            检测异常 = false;
                        }
                    }
                    else
                    {
                        ShowMessage($"{camName}_{pointNum}_1 {Resources.LanguageDic.Laser_coordinates_exceed_the_calibration_range}！", Color.Red);
                        检测异常 = false;
                    }
                }

                if (b有数据)
                {
                    point3d.Add(pointNum, new Point3D(X, Y, Z));

                    double dx = X - carSetting.gSets[pointNum].X;
                    double dy = Y - carSetting.gSets[pointNum].Y;
                    double dz = Z - carSetting.gSets[pointNum].Z;
                    double dd = Math.Sqrt(dx * dx + dy * dy + dz * dz);
                    ShowMessage($"{camName}_{pointNum} dx = {dx:0.000}mm, dy = {dy:0.000}mm, dz = {dz:0.000}mm, dd = {dd:0.000}mm");

                    dataGridViewShow.BeginInvoke(new Action(() =>
                    {
                        int index = dataGridViewShow.Rows.Add(camName + pointNum, X.ToString("0.000"), Y.ToString("0.000"), Z.ToString("0.000"), dx.ToString("0.000"), dy.ToString("0.000"), dz.ToString("0.000"), dd.ToString("0.000"));
                        if (dx < carSetting.gSets[pointNum].minDX || dx > carSetting.gSets[pointNum].maxDX)
                        {
                            dataGridViewShow.Rows[index].Cells[4].Style.BackColor = Color.Red;
                        }
                        if (dy < carSetting.gSets[pointNum].minDY || dy > carSetting.gSets[pointNum].maxDY)
                        {
                            dataGridViewShow.Rows[index].Cells[5].Style.BackColor = Color.Red;
                        }
                        if (dz < carSetting.gSets[pointNum].minDZ || dz > carSetting.gSets[pointNum].maxDZ)
                        {
                            dataGridViewShow.Rows[index].Cells[6].Style.BackColor = Color.Red;
                        }
                    }));
                    if (dx < carSetting.gSets[pointNum].minDX || dx > carSetting.gSets[pointNum].maxDX)
                    {
                        数据超差 = false;
                    }
                    if (dy < carSetting.gSets[pointNum].minDY || dy > carSetting.gSets[pointNum].maxDY)
                    {
                        数据超差 = false;
                    }
                    if (dz < carSetting.gSets[pointNum].minDZ || dz > carSetting.gSets[pointNum].maxDZ)
                    {
                        数据超差 = false;
                    }

                    formShow?.UpDataXYZ(point3d[pointNum], camName + pointNum);////////////////////////

                    测点数据表.Add("时间", dateTimeNow.ToString("yyyy-MM-dd HH:mm:ss"));
                    测点数据表.Add("相机名", camName);
                    测点数据表.Add("点位号", pointNum.ToString());
                    测点数据表.Add("X", X.ToString("0.000"));
                    测点数据表.Add("Y", Y.ToString("0.000"));
                    测点数据表.Add("Z", Z.ToString("0.000"));
                    测点数据表.Add("DX", dx.ToString("0.000"));
                    测点数据表.Add("DY", dy.ToString("0.000"));
                    测点数据表.Add("DZ", dz.ToString("0.000"));
                    测点数据表.Add("DD", dd.ToString("0.000"));

                    sqlValuePairs.Add(pointNum, 测点数据表);

                    //发送结果
                    //var writeResultRef = plc.Write(IO["点位测量结果"].Address, 数据超差);

                    //var writeXRef = plc.Write(IO["x坐标实际值"].Address ,(float)X);
                    //var writeYRef = plc.Write(IO["y坐标实际值"].Address ,(float)Y);
                    //var writeZRef = plc.Write(IO["z坐标实际值"].Address ,(float)Z);
                    //var writeDXRef = plc.Write(IO["x坐标偏移值"].Address, (float)dx);
                    //var writeDYRef = plc.Write(IO["y坐标偏移值"].Address, (float)dy);
                    //var writeDZRef = plc.Write(IO["z坐标偏移值"].Address, (float)dz);

                }
                else
                {
                    //发送结果
                    //var writeResultRef = plc.Write(IO["点位测量结果"].Address, 数据超差);

                    //var writeXRef = plc.Write(IO["x坐标实际值"].Address, (float)X);
                    //var writeYRef = plc.Write(IO["y坐标实际值"].Address, (float)Y);
                    //var writeZRef = plc.Write(IO["z坐标实际值"].Address, (float)Z);
                    //var writeDXRef = plc.Write(IO["x坐标偏移值"].Address, (float)99999);
                    //var writeDYRef = plc.Write(IO["y坐标偏移值"].Address, (float)99999);
                    //var writeDZRef = plc.Write(IO["z坐标偏移值"].Address, (float)99999);
                }


                ShowMessage($"{camName}{Resources.LanguageDic.Coordinate_calculation_completed}，{Resources.LanguageDic.use_time2}{sp.ElapsedMilliseconds}ms");
                sp.Stop();
                formShow?.UpData灯(camName, false);/////////////////////////////////

                if (!bDry_mode)
                {
                    try
                    {
                        if (otherSet.isSaveImage)
                        {
                            //存图
                            //ShowMessage($"{camName}_{pointNum:00} 开始存图");
                            //hImage2D?.WriteImage("png 1", 0, $"{path}\\{camName}_{pointNum:00}_0.png");
                            //hImage2D匹配图?.WriteImage("jpeg 80", 0, $"{path}\\{camName}_{pointNum:00}_0p.jpg");
                            //hImageLight?.WriteImage("png 1", 0, $"{path}\\{camName}_{pointNum:00}_1.png");
                            //hImageLight匹配图?.WriteImage("jpeg 80", 0, $"{path}\\{camName}_{pointNum:00}_1p.jpg");
                            //ShowMessage($"{camName}_{pointNum:00} 存图完成");

                            ShowMessage($"{camName}_{pointNum:00} {Resources.LanguageDic.Start_storing_images_into_memory}");
                            lock (lockSaveImage)
                            {
                                if (ListImageKeys.Count < 120)
                                {
                                    {
                                        string key = $"{path}\\{camName}_{pointNum:00}_0.png";
                                        ListImageKeys.Add(key);
                                        DicImages.Add(key, hImage2D);
                                    }
                                    {
                                        string key = $"{path}\\{camName}_{pointNum:00}_0p.jpg";
                                        ListImageKeys.Add(key);
                                        DicImages.Add(key, hImage2D匹配图);
                                    }
                                    {
                                        string key = $"{path}\\{camName}_{pointNum:00}_1.png";
                                        ListImageKeys.Add(key);
                                        DicImages.Add(key, hImageLight);
                                    }
                                    {
                                        string key = $"{path}\\{camName}_{pointNum:00}_1p.jpg";
                                        ListImageKeys.Add(key);
                                        DicImages.Add(key, hImageLight匹配图);
                                    }
                                }
                                else
                                {
                                    ShowMessage($"{camName}_{pointNum:00} {ListImageKeys.Count}{Resources.LanguageDic.pix_not_save_to_hard_disk_in_memory}，{Resources.LanguageDic.skip_this_save_image}", Color.Yellow);
                                }
                            }
                            ShowMessage($"{camName}_{pointNum:00} {Resources.LanguageDic.Image_storage_into_memory_completed}");
                        }
                    }
                    catch (Exception ex)
                    {
                        ShowMessage($"{camName}_{pointNum} {Resources.LanguageDic.save_pic_error}：{ex}", Color.Red);
                    }
                }

                if (endPoint.Content)
                {
                    ShowMessage(camName + $"{Resources.LanguageDic.Process_ended}");
                    break;
                }
            }
        }

        object lockSaveImage = new object();
        List<string> ListImageKeys = new List<string>();
        Dictionary<string, HImage> DicImages = new Dictionary<string, HImage>();
        Thread threadSaveImage = null;
        void SaveImage()
        {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            while (!stop)
            {
                if (ListImageKeys.Count > 0)
                {
                    var path = ListImageKeys[0];

                    stopwatch.Restart();
                    bool bCreate = true;
                    try
                    {
                        string dir = Path.GetDirectoryName(path);
                        if (!Directory.Exists(dir))
                        {
                            Directory.CreateDirectory(dir);
                            ShowMessage($"{Resources.LanguageDic.create_dir}" + dir);
                        }
                    }
                    catch (Exception ex)
                    {
                        bCreate = false;
                        ShowMessage(ex.ToString(), Color.Red);
                    }
                    stopwatch.Stop();
                    long aTime = stopwatch.ElapsedMilliseconds;

                    stopwatch.Restart();
                    try
                    {
                        if (bCreate)
                        {
                            if (Path.GetExtension(path) == ".png")
                            {
                                DicImages[path]?.WriteImage("png 1", 0, path);
                            }
                            else
                            {
                                DicImages[path]?.WriteImage("jpeg 80", 0, path);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ShowMessage($"{Resources.LanguageDic.save_img_error}" + ex.ToString(), Color.Red);
                    }
                    stopwatch.Stop();
                    long bTime = stopwatch.ElapsedMilliseconds;
                    lock (lockSaveImage)
                    {
                        ListImageKeys.RemoveAt(0);
                        DicImages[path]?.Dispose();
                        DicImages.Remove(path);
                        ShowMessage($"{path} {Resources.LanguageDic.Image_storage_into_memory_completed}，{Resources.LanguageDic.create_dir_use_time}{aTime}ms，{Resources.LanguageDic.save_pic_use_time}{bTime}ms，{Resources.LanguageDic.remain}{ListImageKeys.Count}{Resources.LanguageDic.fix}");
                    }
                }
                Thread.Sleep(100);
            }
        }

        object splock = new object();
        private void Led_Light(string name, bool led, bool light)
        {
            lock (splock)
            {
                try
                {
                    if (!sp.IsOpen)
                    {
                        sp.Open();
                    }
                    if (name == "L")
                    {
                        string send = light ? "SA1000#" : "SA0000#";
                        send += led ? "SB1000#" : "SB0000#";
                        sp.WriteLine(send);
                    }
                    else
                    {
                        string send = light ? "SC1000#" : "SC0000#";
                        send += led ? "SD1000#" : "SD0000#";
                        sp.WriteLine(send);
                    }
                }
                catch (Exception ex)
                {
                    ShowMessage($"{Resources.LanguageDic.Serial_port_sending_exception}：" + ex.Message, Color.Red);
                }

            }
        }
        object iolock = new object();
        //OperateResult Write(string dio, bool value)
        //{
        //    bool bTry = false;
        //    while (true)
        //    {
        //        OperateResult result;
        //        lock (iolock)
        //        {
        //            result = plc.Write(dio, value);
        //        }
        //        if (result.IsSuccess)
        //        {
        //            if (bTry)
        //            {
        //                bTry = false;
        //                ShowMessage($"{Resources.LanguageDic.signal_rewrite_success}", Color.Yellow);
        //            }

        //            //通过读取信号判断是否真的写入成功
        //            OperateResult<bool> readResult = ReadBool(dio);
        //            if (readResult.IsSuccess)
        //            {
        //                if (readResult.Content == value)
        //                {
        //                    return result;
        //                }
        //                else
        //                {
        //                    ShowMessage($"{Resources.LanguageDic.The_written_value_is_inconsistent_with_the_read_value}", Color.Yellow);
        //                    Thread.Sleep(50);
        //                }
        //            }
        //        }
        //        else
        //        {
        //            if (!bTry)
        //            {
        //                bTry = true;
        //                ShowMessage($"{Resources.LanguageDic.signal_rewrite_fail},{Resources.LanguageDic.repect_again}...：" + result.Message, Color.Yellow);
        //            }
        //            Thread.Sleep(1000);
        //        }
        //        if (stop)
        //        {
        //            return result;
        //        }
        //    }
        //}
        //OperateResult Write(string dio, short value)
        //{
        //    bool bTry = false;
        //    while (true)
        //    {
        //        OperateResult result;
        //        lock (iolock)
        //        {
        //            result = plc.Write(dio, value);
        //        }
        //        if (result.IsSuccess)
        //        {
        //            if (bTry)
        //            {
        //                bTry = false;
        //                ShowMessage($"{Resources.LanguageDic.signal_rewrite_success}", Color.Yellow);
        //            }

        //            //通过读取信号判断是否真的写入成功
        //            var readResult = ReadInt16(dio);
        //            if (readResult.IsSuccess)
        //            {
        //                if (readResult.Content == value)
        //                {
        //                    return result;
        //                }
        //                else
        //                {
        //                    ShowMessage($"{Resources.LanguageDic.The_written_value_is_inconsistent_with_the_read_value}", Color.Yellow);
        //                    Thread.Sleep(50);
        //                }
        //            }
        //        }
        //        else
        //        {
        //            if (!bTry)
        //            {
        //                bTry = true;
        //                ShowMessage($"{Resources.LanguageDic.signal_rewrite_fail},{Resources.LanguageDic.repect_again}...：" + result.Message, Color.Yellow);
        //            }
        //            Thread.Sleep(1000);
        //        }
        //        if (stop)
        //        {
        //            return result;
        //        }
        //    }
        //}
        //OperateResult Write(string dio, float value)
        //{
        //    bool bTry = false;
        //    while (true)
        //    {
        //        OperateResult result;
        //        lock (iolock)
        //        {
        //            result = plc.Write(dio, value);
        //        }
        //        if (result.IsSuccess)
        //        {
        //            if (bTry)
        //            {
        //                bTry = false;
        //                ShowMessage($"{Resources.LanguageDic.signal_rewrite_success}", Color.Yellow);
        //            }

        //            //通过读取信号判断是否真的写入成功
        //            var readResult = ReadFloat(dio);
        //            if (readResult.IsSuccess)
        //            {
        //                if (readResult.Content == value)
        //                {
        //                    return result;
        //                }
        //                else
        //                {
        //                    ShowMessage($"{Resources.LanguageDic.The_written_value_is_inconsistent_with_the_read_value}", Color.Yellow);
        //                    Thread.Sleep(50);
        //                }
        //            }
        //        }
        //        else
        //        {
        //            if (!bTry)
        //            {
        //                bTry = true;
        //                ShowMessage($"{Resources.LanguageDic.signal_rewrite_fail},{Resources.LanguageDic.repect_again}...：" + result.Message, Color.Yellow);
        //            }
        //            Thread.Sleep(1000);
        //        }
        //        if (stop)
        //        {
        //            return result;
        //        }
        //    }
        //}

        //OperateResult Write(string dio, String value)
        //{
        //    bool bTry = false;
        //    while (true)
        //    {
        //        OperateResult result;
        //        lock (iolock)
        //        {
        //            result = plc.Write(dio, value);
        //        }
        //        if (result.IsSuccess)
        //        {
        //            if (bTry)
        //            {
        //                bTry = false;
        //                ShowMessage($"{Resources.LanguageDic.signal_rewrite_success}", Color.Yellow);
        //            }

        //            //通过读取信号判断是否真的写入成功
        //            var readResult = ReadString(dio, (ushort)value.Length);
        //            if (readResult.IsSuccess)
        //            {
        //                if (readResult.Content == value)
        //                {
        //                    return result;
        //                }
        //                else
        //                {
        //                    ShowMessage($"{Resources.LanguageDic.The_written_value_is_inconsistent_with_the_read_value}", Color.Yellow);
        //                    Thread.Sleep(50);
        //                }
        //            }
        //        }
        //        else
        //        {
        //            if (!bTry)
        //            {
        //                bTry = true;
        //                ShowMessage($"{Resources.LanguageDic.signal_rewrite_fail},{Resources.LanguageDic.repect_again}...：" + result.Message, Color.Yellow);
        //            }
        //            Thread.Sleep(1000);
        //        }
        //        if (stop)
        //        {
        //            return result;
        //        }
        //    }
        //}


        //OperateResult<bool> ReadBool(string dio)
        //{
        //    bool bTry = false;
        //    while (true)
        //    {
        //        OperateResult<bool> result = plc.ReadBool(dio);
        //        if (result.IsSuccess)
        //        {
        //            if (bTry)
        //            {
        //                ShowMessage($"{Resources.LanguageDic.signal_read_again_success}", Color.Yellow);
        //            }
        //            return result;
        //        }
        //        else
        //        {
        //            if (!bTry)
        //            {
        //                bTry = true;
        //                ShowMessage($"{Resources.LanguageDic.signal_read_again_success}", Color.Yellow);
        //            }
        //            Thread.Sleep(1000);
        //            //ConnectServer();
        //        }
        //        if (stop)
        //        {
        //            return result;
        //        }
        //    }
        //}

        //OperateResult<short> ReadInt16(string dio)
        //{
        //    bool bTry = false;
        //    while (true)
        //    {
        //        OperateResult<short> result = plc.ReadInt16(dio);
        //        if (result.IsSuccess)
        //        {
        //            if (bTry)
        //            {
        //                ShowMessage($"{Resources.LanguageDic.signal_read_again_success}", Color.Yellow);
        //            }
        //            return result;
        //        }
        //        else
        //        {
        //            if (!bTry)
        //            {
        //                bTry = true;
        //                ShowMessage($"{Resources.LanguageDic.signal_read_again_success}", Color.Yellow);
        //            }
        //            Thread.Sleep(1000);
        //        }
        //        if (stop)
        //        {
        //            return result;
        //        }
        //    }
        //}

        //OperateResult<short[]> ReadInt16(string dio, ushort length)
        //{
        //    bool bTry = false;
        //    while (true)
        //    {
        //        OperateResult<short[]> result = plc.ReadInt16(dio, length);
        //        if (result.IsSuccess)
        //        {
        //            if (bTry)
        //            {
        //                ShowMessage($"{Resources.LanguageDic.signal_read_again_success}", Color.Yellow);
        //            }
        //            return result;
        //        }
        //        else
        //        {
        //            if (!bTry)
        //            {
        //                bTry = true;
        //                ShowMessage($"{Resources.LanguageDic.signal_read_again_success}", Color.Yellow);
        //            }
        //            Thread.Sleep(1000);
        //        }
        //        if (stop)
        //        {
        //            return result;
        //        }
        //    }
        //}

        //OperateResult<float> ReadFloat(string dio)
        //{
        //    bool bTry = false;
        //    while (true)
        //    {
        //        var result = plc.ReadFloat(dio);
        //        if (result.IsSuccess)
        //        {
        //            if (bTry)
        //            {
        //                ShowMessage($"{Resources.LanguageDic.signal_read_again_success}", Color.Yellow);
        //            }
        //            return result;
        //        }
        //        else
        //        {
        //            if (!bTry)
        //            {
        //                bTry = true;
        //                ShowMessage($"{Resources.LanguageDic.signal_read_again_success}", Color.Yellow);
        //            }
        //            Thread.Sleep(1000);
        //        }
        //        if (stop)
        //        {
        //            return result;
        //        }
        //    }
        //}

        //OperateResult<string> ReadString(string dio, ushort length)
        //{
        //    bool bTry = false;
        //    while (true)
        //    {
        //        OperateResult<string> result = plc.ReadString(dio, length);
        //        if (result.IsSuccess)
        //        {
        //            char[] data = result.Content.ToArray();
        //            for (int i = 1; i < data.Length; i += 2)
        //            {
        //                data[i - 1] = result.Content[i];
        //                data[i] = result.Content[i - 1];
        //            }
        //            result.Content = new string(data);
        //            if (bTry)
        //            {
        //                ShowMessage($"{Resources.LanguageDic.signal_read_again_success}", Color.Yellow);
        //            }
        //            return result;
        //        }
        //        else
        //        {
        //            if (!bTry)
        //            {
        //                bTry = true;
        //                ShowMessage($"{Resources.LanguageDic.signal_read_fail},{Resources.LanguageDic.repect_again}...：" + result.Message, Color.Yellow);
        //            }
        //            Thread.Sleep(1000);
        //        }
        //        if (stop)
        //        {
        //            return result;
        //        }
        //    }
        //}
        //OperateResult ConnectServer()
        //{
        //    bool bTry = false;
        //    while (true)
        //    {
        //        OperateResult result = plc.ConnectServer();
        //        if (result.IsSuccess)
        //        {
        //            if (bTry)
        //            {
        //                ShowMessage($"PLC{Resources.LanguageDic.connect_again_success}", Color.Yellow);
        //            }
        //            return result;
        //        }
        //        else
        //        {
        //            if (!bTry)
        //            {
        //                bTry = true;
        //                ShowMessage($"PLC{Resources.LanguageDic.connection_failed},{Resources.LanguageDic.repect_again}...：" + result.Message, Color.Yellow);
        //            }
        //            Thread.Sleep(1000);
        //        }
        //        if (stop)
        //        {
        //            return result;
        //        }
        //    }
        //}

        private void buttonStop_Click(object sender, EventArgs e)
        {
            stop = true;
            ShowMessage($"{Resources.LanguageDic.user_stop}", Color.Red);
        }
        /// <summary>
        /// 运行时控件启用状态
        /// </summary>
        void RunState()
        {
            stop = false;
            buttonRun.Enabled = false;
            buttonStop.Enabled = true;
            groupBox1.Enabled = false;
        }
        /// <summary>
        /// 停止时控件启用状态
        /// </summary>
        void StopState()
        {
            stop = true;
            buttonStop.BeginInvoke(new Action(() =>
            {
                lblSysStatus.Text = $"{Resources.LanguageDic.stop}";
                lblSysStatus.BackColor = Color.Red;
                buttonRun.Enabled = true;
                buttonStop.Enabled = false;
                groupBox1.Enabled = true;
            }));
        }
        object olock = new object();
        void ShowMessage(string message)
        {
            ShowMessage(message, Color.White);
        }
        void ShowMessage(string message, Color backColor)
        {
            DateTime now = DateTime.Now;
            string day = now.ToString("yyyy-MM-dd");
            string time = now.ToString("HH:mm:ss.fff");
            try
            {
                dataGridViewLog.BeginInvoke(new Action(() =>
                {
                    if (dataGridViewLog.Rows.Count >= 1000)
                    {
                        dataGridViewLog.Rows.RemoveAt(dataGridViewLog.Rows.Count - 1);
                    }
                    dataGridViewLog.Rows.Insert(0);
                    dataGridViewLog.Rows[0].Cells[0].Value = day + " " + time;
                    dataGridViewLog.Rows[0].Cells[1].Value = message;
                    dataGridViewLog.Rows[0].DefaultCellStyle.BackColor = backColor;
                }));
            }
            catch { }
            lock (olock)
            {
                if (!Directory.Exists("RunLog"))
                {
                    Directory.CreateDirectory("RunLog");
                }
                using (StreamWriter writer = new StreamWriter("RunLog\\" + day + ".log", true))
                {
                    writer.WriteLine(time + " " + message);
                }
                if (backColor == Color.Red)
                {
                    try
                    {
                        File.AppendAllText("Error.log", now.ToString("yyyy-MM-dd HH:mm:ss  ") + message + "\r\n\r\n");
                    }
                    catch { }
                }
            }
        }

        private void FormRun_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!stop)
            {
                if (DialogResult.OK != MessageBox.Show($"{Resources.LanguageDic.software_is_running}，{Resources.LanguageDic.sure_exist}？", $"{Resources.LanguageDic.tip}", MessageBoxButtons.OKCancel))
                {
                    e.Cancel = true;
                    return;
                }
            }
            if (formShow != null && formShow.IsShow)
            {
                formShow.EnableClose = true;
                formShow.Invoke(new Action(() => { formShow.Close(); }));
            }

            StopState();
            //if (sp.IsOpen)
            //{
            //    sp.Close();
            //}

            //关闭plc通讯
            plcClose();

            while (mainThread != null && mainThread.ThreadState != ThreadState.Stopped)
            {
                Thread.Sleep(1);
            }
        }

        #region 仿真
        Thread thread = null;
        private void buttonTest_Click(object sender, EventArgs e)
        {
            if (thread != null && thread.IsAlive)
            {
                return;
            }
            //RunState();
            thread = new Thread(() =>
            {
                //threadSaveImage = new Thread(SaveImage);
                //threadSaveImage.Start();

                //加载参数
                if (预载车型参数)
                {
                    车型号转名称.Clear();
                    车型参数.Clear();
                    车型参数排序key.Clear();
                    string[] carPaths = Directory.GetDirectories("Data\\Car");
                    foreach (var item in carPaths)
                    {
                        string dirName = Path.GetFileNameWithoutExtension(item);
                        string[] strings = dirName.Split('-');
                        if (strings.Length == 3 && int.TryParse(strings[0], out int 车型号) && int.TryParse(strings[1], out int 托盘号))
                        {
                            车型号转名称.Add(车型号, strings[2]);

                            Car car = new Car(dirName);
                            if (car.Load())
                            {
                                车型参数.Add($"{车型号}-{托盘号}", car);
                                车型参数排序key.Add($"{车型号}-{托盘号}");
                                ShowMessage($"{dirName}{Resources.LanguageDic.para_load_success}");
                            }
                            else
                            {
                                ShowMessage($"{dirName}{Resources.LanguageDic.para_load_fail}", Color.Red);
                            }
                        }
                    }
                }

                相机参数.Clear();
                string[] camPaths = Directory.GetDirectories("Data\\Cam");
                foreach (var item in camPaths)
                {
                    string name = Path.GetFileNameWithoutExtension(item);

                    CamSetting camSet = new CamSetting(name);
                    if (camSet.Load(item))
                    {
                        相机参数.Add(name, camSet);
                        ShowMessage($"{name}{Resources.LanguageDic.cam}{Resources.LanguageDic.para_load_success}");
                    }
                    else
                    {
                        ShowMessage($"{name}{Resources.LanguageDic.cam}{Resources.LanguageDic.para_load_fail}", Color.Red);
                    }
                }
                dataGridViewShow.Invoke(new Action(() =>
                {
                    dataGridViewShow.Rows.Clear();
                }));

                if (!预载车型参数)
                {
                    string[] carPaths = Directory.GetDirectories("Data\\Car", $"{textBoxTestNum.Text}-*");
                    if (carPaths.Length > 0)
                    {
                        string dirName = Path.GetFileNameWithoutExtension(carPaths[0]);
                        string[] strings = dirName.Split('-');
                        if (strings.Length == 3 && int.TryParse(strings[0], out int 车型号) && int.TryParse(strings[1], out int 托盘号))
                        {
                            if (!车型号转名称.ContainsKey(车型号))
                            {
                                车型号转名称.Add(车型号, strings[2]);
                            }
                            if (!车型参数.ContainsKey($"{车型号}-{托盘号}"))
                            {
                                Car car = new Car(dirName);
                                if (car.Load())
                                {
                                    if (车型参数.Count >= 8)
                                    {
                                        车型参数.Remove(车型参数排序key[0]);
                                        车型参数排序key.Remove(车型参数排序key[0]);
                                    }
                                    车型参数.Add($"{车型号}-{托盘号}", car);
                                    车型参数排序key.Add($"{车型号}-{托盘号}");
                                    ShowMessage($"{dirName}{Resources.LanguageDic.para_load_success}");
                                }
                                else
                                {
                                    ShowMessage($"{dirName}{Resources.LanguageDic.para_load_fail}", Color.Red);
                                }
                            }
                            else
                            {
                                ShowMessage($"{dirName}{Resources.LanguageDic.para_already_load}", Color.Red);
                            }
                        }
                    }
                }
                if (!车型参数.ContainsKey(textBoxTestNum.Text))
                {
                    MessageBox.Show("车型参数不存在");
                    return;
                }

                string carName = textBoxTestNum.Text.Split('-')[0];
                if (int.TryParse(carName, out int num))
                {
                    if (车型号转名称.ContainsKey(num))
                    {
                        carName = 车型号转名称[num];
                    }
                }
                string carNum = "P02345678";
                formShow?.UpDataCamImage(null, "Clear");///////////////////////////
                formShow?.UpDataCarInform(carName, carNum);/////////////////////////
                Dictionary<int, Point3D> rbL = new Dictionary<int, Point3D>();
                Dictionary<int, Point3D> point3dL = new Dictionary<int, Point3D>();
                Dictionary<int, Point3D> point3dBaseL = new Dictionary<int, Point3D>();
                {
                    string IOName = "L";
                    string[] files = Directory.GetFiles("test-Image", "L_*_1.png");
                    Dictionary<int, Point3D> rb = rbL;
                    Dictionary<int, Point3D> point3d = point3dL;
                    Dictionary<int, Point3D> point3dBase = point3dBaseL;

                    taskL = Task.Run(new Action(() =>
                    {
                        仿真计算(IOName, files, point3d, point3dBase, rb);
                    }));
                }

                Dictionary<int, Point3D> rbR = new Dictionary<int, Point3D>();
                Dictionary<int, Point3D> point3dR = new Dictionary<int, Point3D>();
                Dictionary<int, Point3D> point3dBaseR = new Dictionary<int, Point3D>();
                {
                    string IOName = "R";
                    string[] files = Directory.GetFiles("test-Image", "R_*_1.png");
                    Dictionary<int, Point3D> rb = rbR;
                    Dictionary<int, Point3D> point3d = point3dR;
                    Dictionary<int, Point3D> point3dBase = point3dBaseR;

                    taskR = Task.Run(new Action(() =>
                    {
                        仿真计算(IOName, files, point3d, point3dBase, rb);
                    }));
                }


                // 重定位 结果显示
                while (!(taskL.IsCompleted && taskR.IsCompleted))
                {
                    Thread.Sleep(7);
                }
                if (point3dBaseL.Count() + point3dBaseR.Count() >= 3)
                {
                    HTuple px = new HTuple();
                    HTuple py = new HTuple();
                    HTuple pz = new HTuple();
                    HTuple qx = new HTuple();
                    HTuple qy = new HTuple();
                    HTuple qz = new HTuple();
                    foreach (var item in point3dBaseL.Keys)
                    {
                        px.Append(point3dL[item].X);
                        py.Append(point3dL[item].Y);
                        pz.Append(point3dL[item].Z);
                        qx.Append(point3dBaseL[item].X);
                        qy.Append(point3dBaseL[item].Y);
                        qz.Append(point3dBaseL[item].Z);
                    }
                    foreach (var item in point3dBaseR.Keys)
                    {
                        px.Append(point3dR[item].X);
                        py.Append(point3dR[item].Y);
                        pz.Append(point3dR[item].Z);
                        qx.Append(point3dBaseR[item].X);
                        qy.Append(point3dBaseR[item].Y);
                        qz.Append(point3dBaseR[item].Z);
                    }
                    HHomMat3D hHomMat3D = new HHomMat3D();
                    hHomMat3D.VectorToHomMat3d("rigid", px, py, pz, qx, qy, qz);

                    Dictionary<int, Point3D> newPoint3dBaseL = new Dictionary<int, Point3D>();
                    foreach (var item in point3dL.Keys)
                    {
                        double nx = hHomMat3D.AffineTransPoint3d(point3dL[item].X, point3dL[item].Y, point3dL[item].Z, out double ny, out double nz);
                        newPoint3dBaseL.Add(item, new Point3D(nx, ny, nz));

                        //重新显示
                        string IOName = "L";
                        var carSetting = 车型参数[textBoxTestNum.Text].car[IOName];
                        double dx = nx - carSetting.gSets[item].X;
                        double dy = ny - carSetting.gSets[item].Y;
                        double dz = nz - carSetting.gSets[item].Z;
                        double dd = Math.Sqrt(dx * dx + dy * dy + dz * dz);
                        dataGridViewShow.BeginInvoke(new Action(() =>
                        {
                            int index = dataGridViewShow.Rows.Add("*" + IOName + item, nx.ToString("0.000"), ny.ToString("0.000"), nz.ToString("0.000"), dx.ToString("0.000"), dy.ToString("0.000"), dz.ToString("0.000"), dd.ToString("0.000"));
                            if (dx < carSetting.gSets[item].minDX || dx > carSetting.gSets[item].maxDX)
                            {
                                dataGridViewShow.Rows[index].Cells[4].Style.BackColor = Color.Red;
                            }
                            if (dy < carSetting.gSets[item].minDY || dy > carSetting.gSets[item].maxDY)
                            {
                                dataGridViewShow.Rows[index].Cells[5].Style.BackColor = Color.Red;
                            }
                            if (dz < carSetting.gSets[item].minDZ || dz > carSetting.gSets[item].maxDZ)
                            {
                                dataGridViewShow.Rows[index].Cells[6].Style.BackColor = Color.Red;
                            }
                        }));
                        formShow?.UpDataXYZ(new Point3D(nx, ny, nz), IOName + item);////////////////////////
                    }
                    Dictionary<int, Point3D> newPoint3dBaseR = new Dictionary<int, Point3D>();
                    foreach (var item in point3dR.Keys)
                    {
                        double nx = hHomMat3D.AffineTransPoint3d(point3dR[item].X, point3dR[item].Y, point3dR[item].Z, out double ny, out double nz);
                        newPoint3dBaseR.Add(item, new Point3D(nx, ny, nz));

                        //重新显示
                        string IOName = "R";
                        var carSetting = 车型参数[textBoxTestNum.Text].car[IOName];
                        double dx = nx - carSetting.gSets[item].X;
                        double dy = ny - carSetting.gSets[item].Y;
                        double dz = nz - carSetting.gSets[item].Z;
                        double dd = Math.Sqrt(dx * dx + dy * dy + dz * dz);
                        dataGridViewShow.BeginInvoke(new Action(() =>
                        {
                            int index = dataGridViewShow.Rows.Add("*" + IOName + item, nx.ToString("0.000"), ny.ToString("0.000"), nz.ToString("0.000"), dx.ToString("0.000"), dy.ToString("0.000"), dz.ToString("0.000"), dd.ToString("0.000"));
                            if (dx < carSetting.gSets[item].minDX || dx > carSetting.gSets[item].maxDX)
                            {
                                dataGridViewShow.Rows[index].Cells[4].Style.BackColor = Color.Red;
                            }
                            if (dy < carSetting.gSets[item].minDY || dy > carSetting.gSets[item].maxDY)
                            {
                                dataGridViewShow.Rows[index].Cells[5].Style.BackColor = Color.Red;
                            }
                            if (dz < carSetting.gSets[item].minDZ || dz > carSetting.gSets[item].maxDZ)
                            {
                                dataGridViewShow.Rows[index].Cells[6].Style.BackColor = Color.Red;
                            }
                        }));
                        formShow?.UpDataXYZ(new Point3D(nx, ny, nz), IOName + item);////////////////////////
                    }
                }

                if (checkBoxFrame.Checked)
                {
                    FormZero form = new FormZero(true);
                    HTuple pxL = new HTuple();
                    HTuple pyL = new HTuple();
                    HTuple pzL = new HTuple();
                    foreach (var item in rbL.Keys)
                    {
                        form.dL.Add(item, null);
                        pxL.Append(rbL[item].X);
                        pyL.Append(rbL[item].Y);
                        pzL.Append(rbL[item].Z);
                    }
                    HTuple pxR = new HTuple();
                    HTuple pyR = new HTuple();
                    HTuple pzR = new HTuple();
                    foreach (var item in rbR.Keys)
                    {
                        form.dR.Add(item, null);
                        pxR.Append(rbR[item].X);
                        pyR.Append(rbR[item].Y);
                        pzR.Append(rbR[item].Z);
                    }
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        {
                            var carSetting = 车型参数[textBoxTestNum.Text].car["L"];
                            HTuple qx = new HTuple();
                            HTuple qy = new HTuple();
                            HTuple qz = new HTuple();
                            foreach (var item in form.dL.Keys)
                            {
                                qx.Append(carSetting.gSets[item].X + form.dL[item].X);
                                qy.Append(carSetting.gSets[item].Y + form.dL[item].Y);
                                qz.Append(carSetting.gSets[item].Z + form.dL[item].Z);
                            }
                            if (qx.Length > 0)
                            {
                                HHomMat3D hHomMat3D = new HHomMat3D();
                                hHomMat3D.VectorToHomMat3d("rigid", pxL, pyL, pzL, qx, qy, qz);

                                HTuple x = hHomMat3D.AffineTransPoint3d(pxL, pyL, pzL, out HTuple y, out HTuple z);
                                HTuple dx = qx - x;
                                HTuple dy = qy - y;
                                HTuple dz = qz - z;

                                //修改补偿
                                var keys = rbL.Keys.ToArray();
                                for (int i = 0; i < keys.Length; i++)
                                {
                                    carSetting.gSets[keys[i]].offsetX = (float)dx[i].D;
                                    carSetting.gSets[keys[i]].offsetY = (float)dy[i].D;
                                    carSetting.gSets[keys[i]].offsetZ = (float)dz[i].D;
                                }
                                //修改坐标转换
                                carSetting.robot2Car = hHomMat3D;
                            }

                        }
                        {

                            var carSetting = 车型参数[textBoxTestNum.Text].car["R"];
                            HTuple qx = new HTuple();
                            HTuple qy = new HTuple();
                            HTuple qz = new HTuple();
                            foreach (var item in form.dR.Keys)
                            {
                                qx.Append(carSetting.gSets[item].X + form.dR[item].X);
                                qy.Append(carSetting.gSets[item].Y + form.dR[item].Y);
                                qz.Append(carSetting.gSets[item].Z + form.dR[item].Z);
                            }

                            if (qx.Length > 0)
                            {
                                HHomMat3D hHomMat3D = new HHomMat3D();
                                hHomMat3D.VectorToHomMat3d("rigid", pxR, pyR, pzR, qx, qy, qz);

                                HTuple x = hHomMat3D.AffineTransPoint3d(pxR, pyR, pzR, out HTuple y, out HTuple z);
                                HTuple dx = qx - x;
                                HTuple dy = qy - y;
                                HTuple dz = qz - z;

                                //修改补偿
                                var keys = rbR.Keys.ToArray();
                                for (int i = 0; i < keys.Length; i++)
                                {
                                    carSetting.gSets[keys[i]].offsetX = (float)dx[i].D;
                                    carSetting.gSets[keys[i]].offsetY = (float)dy[i].D;
                                    carSetting.gSets[keys[i]].offsetZ = (float)dz[i].D;
                                }
                                //修改坐标转换
                                carSetting.robot2Car = hHomMat3D;
                            }
                        }
                        checkBoxZero.Invoke(new Action(() =>
                        {
                            checkBoxZero.Checked = false;
                            checkBoxFrame.Checked = false;
                        }));
                    }
                }
                if (checkBoxZero.Checked)
                {
                    FormZero form = new FormZero(false);
                    foreach (var item in point3dL.Keys)
                    {
                        form.dL.Add(item, null);
                    }
                    foreach (var item in point3dR.Keys)
                    {
                        form.dR.Add(item, null);
                    }
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        {
                            var carSetting = 车型参数[textBoxTestNum.Text].car["L"];
                            foreach (var item in form.dL.Keys)
                            {
                                if (carSetting.gSets.ContainsKey(item))
                                {
                                    carSetting.gSets[item].offsetX += carSetting.gSets[item].X - point3dL[item].X + form.dL[item].X;
                                    carSetting.gSets[item].offsetY += carSetting.gSets[item].Y - point3dL[item].Y + form.dL[item].Y;
                                    carSetting.gSets[item].offsetZ += carSetting.gSets[item].Z - point3dL[item].Z + form.dL[item].Z;
                                }
                            }
                        }
                        {
                            var carSetting = 车型参数[textBoxTestNum.Text].car["R"];
                            foreach (var item in form.dR.Keys)
                            {
                                if (carSetting.gSets.ContainsKey(item))
                                {
                                    carSetting.gSets[item].offsetX += carSetting.gSets[item].X - point3dR[item].X + form.dR[item].X;
                                    carSetting.gSets[item].offsetY += carSetting.gSets[item].Y - point3dR[item].Y + form.dR[item].Y;
                                    carSetting.gSets[item].offsetZ += carSetting.gSets[item].Z - point3dR[item].Z + form.dR[item].Z;
                                }
                            }
                        }

                        checkBoxZero.Invoke(new Action(() =>
                        {
                            checkBoxZero.Checked = false;
                        }));
                    }
                }

                //stop = true;
                //threadSaveImage?.Join();
                //StopState();

            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        private void 仿真计算(string IOName, string[] Lfiles, Dictionary<int, Point3D> point3d, Dictionary<int, Point3D> point3dBase, Dictionary<int, Point3D> rb)
        {
            var camSetting = 相机参数[IOName];
            if (!车型参数[textBoxTestNum.Text].car.ContainsKey(IOName))
            {
                ShowMessage($"{IOName}{Resources.LanguageDic.para_has_not_made}", Color.Red);
                return;
            }
            var carSetting = 车型参数[textBoxTestNum.Text].car[IOName];
            int pictureBoxIndex = 0;
            foreach (var file1 in Lfiles)
            {
                pictureBoxIndex++;
                var s = Path.GetFileNameWithoutExtension(file1).Split('_');
                int pointNum = int.Parse(s[1]);
                try
                {
                    string file0 = file1.Replace("_1.png", "_0.png");

                    //采图，计算
                    HImage hImage2D = null;
                    HImage hImageLight = null;
                    HImage hImage2D匹配图 = null;
                    HImage hImageLight匹配图 = null;
                    float Px0 = 0, Py0 = 0, Px1 = 0, Py1 = 0, Px2 = 0, Py2 = 0;

                    bool led匹配 = false;
                    bool light匹配1 = false;
                    bool light匹配2 = false;

                    Thread.Sleep(300);
                    formShow?.UpData灯(IOName, true);///////////////////////////////////

                    //检测定位

                    if (carSetting == null || !carSetting.gSets.ContainsKey(pointNum))//该车型或该测点未做参数,只拍照显示
                    {
                        ShowMessage($"{IOName}_{pointNum} {Resources.LanguageDic.para_has_not_made}", Color.Red);

                        //激光图
                        Thread.Sleep(20);
                        if (File.Exists(file1))
                        {
                            hImageLight = new HImage(file1);
                        }
                        if (hImageLight != null)
                        {
                            formShow?.UpDataCamImage(hImageLight, IOName + pictureBoxIndex);//////////////////////////
                        }
                        else
                        {
                            ShowMessage(IOName + $"{Resources.LanguageDic.cam1_fail}", Color.Red);
                        }

                        //平面图             
                        Thread.Sleep(20);
                        if (File.Exists(file0))
                        {
                            hImage2D = new HImage(file0);
                        }
                        if (hImage2D != null)
                        {
                            formShow?.UpDataCamImage(hImage2D, IOName + pictureBoxIndex);//////////////////////////
                        }
                        else
                        {
                            ShowMessage(IOName + $"{Resources.LanguageDic.cam0_fail}", Color.Red);
                        }
                    }
                    else//该车型已做参数
                    {
                        //激光图
                        Thread.Sleep(20);
                        if (File.Exists(file1))
                        {
                            hImageLight = new HImage(file1);
                        }
                        if (hImageLight != null)
                        {
                            formShow?.UpDataCamImage(hImageLight, IOName + pictureBoxIndex);//////////////////////////

                            try
                            {
                                //匹配计算
                                if (camSetting != null)
                                {
                                    int flag = 0;
                                    //临时屏蔽

                                    HImage map = hImageLight.MapImage(camSetting.mapImage);

                                    //HImage map = hImageLight.Clone();
                                    if (carSetting.rectangle1.ContainsKey(pointNum) && carSetting.Models1.ContainsKey(pointNum))
                                    {
                                        if (carSetting.FindPxPy(map, carSetting.rectangle1[pointNum], carSetting.Models1[pointNum], carSetting.gSets[pointNum].score1, carSetting.modeCenter1[pointNum].X, carSetting.modeCenter1[pointNum].Y, ref Px1, ref Py1, out double dx, out double dy))
                                        {
                                            light匹配1 = true;
                                            ShowMessage($"{IOName}_{pointNum}_1 dx = {dx:0.000}{Resources.LanguageDic.pix}, dy = {dy:0.000}{Resources.LanguageDic.pix}");
                                        }
                                        else
                                        {
                                            ShowMessage($"{IOName}_{pointNum}_1 {Resources.LanguageDic.Match_fail}", Color.Red);
                                        }
                                        flag += 1;
                                    }
                                    else
                                    {
                                        ShowMessage($"{IOName}_{pointNum}_1 {Resources.LanguageDic.no_model}", Color.Red);
                                    }

                                    if (carSetting.gSets[pointNum].type != "棱")//不是棱不可跳过
                                    {
                                        if (carSetting.rectangle2.ContainsKey(pointNum) && carSetting.Models2.ContainsKey(pointNum))
                                        {
                                            if (carSetting.FindPxPy(map, carSetting.rectangle2[pointNum], carSetting.Models2[pointNum], carSetting.gSets[pointNum].score2, carSetting.modeCenter2[pointNum].X, carSetting.modeCenter2[pointNum].Y, ref Px2, ref Py2, out double dx, out double dy))
                                            {
                                                light匹配2 = true;
                                                ShowMessage($"{IOName}_{pointNum}_2 dx = {dx:0.000}{Resources.LanguageDic.pix}, dy = {dy:0.000}{Resources.LanguageDic.pix}");
                                            }
                                            else
                                            {
                                                ShowMessage($"{IOName}_{pointNum}_2 {Resources.LanguageDic.Match_fail}", Color.Red);
                                            }
                                            flag += 2;
                                        }
                                        else
                                        {
                                            ShowMessage($"{IOName}_{pointNum}_2 {Resources.LanguageDic.no_model}", Color.Red);
                                        }
                                    }

                                    if (flag == 1)
                                    {
                                        hImageLight匹配图 = carSetting.GetShowHIamge(map, carSetting.rectangle1[pointNum], carSetting.modeCenter1[pointNum].X, carSetting.modeCenter1[pointNum].Y, Px1, Py1);
                                    }
                                    else if (flag == 2)
                                    {
                                        hImageLight匹配图 = carSetting.GetShowHIamge(map, carSetting.rectangle2[pointNum], carSetting.modeCenter2[pointNum].X, carSetting.modeCenter2[pointNum].Y, Px2, Py2);
                                    }
                                    else if (flag == 3)
                                    {
                                        hImageLight匹配图 = carSetting.GetShowHIamge(map, carSetting.rectangle1[pointNum], carSetting.modeCenter1[pointNum].X, carSetting.modeCenter1[pointNum].Y, Px1, Py1, carSetting.rectangle2[pointNum], carSetting.modeCenter2[pointNum].X, carSetting.modeCenter2[pointNum].Y, Px2, Py2);
                                    }

                                    if (hImageLight匹配图 != null)
                                    {
                                        formShow?.UpDataCamImage(hImageLight匹配图, IOName + pictureBoxIndex);/////////////////////////
                                    }
                                }
                                else
                                {
                                    ShowMessage($"{IOName} {Resources.LanguageDic.no_cam_calib}", Color.Red);
                                }
                            }
                            catch (Exception ex)
                            {
                                ShowMessage($"{IOName}_{pointNum}_1 {Resources.LanguageDic.match_error}:{ex.Message}", Color.Red);
                            }
                        }
                        else
                        {
                            ShowMessage(IOName + $"{Resources.LanguageDic.cam1_fail}", Color.Red);
                        }


                        if (carSetting.gSets[pointNum].type != "棱" && carSetting.gSets[pointNum].type != "槽")//不是棱、不是槽不可以跳过
                        {
                            //平面图             
                            Thread.Sleep(100);
                            if (File.Exists(file0))
                            {
                                hImage2D = new HImage(file0);
                            }
                            if (hImage2D != null)
                            {
                                formShow?.UpDataCamImage(hImage2D, IOName + pictureBoxIndex);//////////////////////////

                                try
                                {
                                    //匹配计算
                                    if (camSetting != null)
                                    {
                                        //临时屏蔽

                                        HImage map = hImage2D.MapImage(camSetting.mapImage);
                                        //HImage map = hImage2D.Clone();

                                        if (carSetting.rectangle0.ContainsKey(pointNum) && carSetting.ShapeModels0.ContainsKey(pointNum))
                                        {
                                            if (carSetting.FindPxPy(map, carSetting.rectangle0[pointNum], carSetting.ShapeModels0[pointNum], carSetting.gSets[pointNum].score0, carSetting.modeCenter0[pointNum].X, carSetting.modeCenter0[pointNum].Y, ref Px0, ref Py0, out double dx, out double dy))
                                            {
                                                led匹配 = true;
                                                ShowMessage($"{IOName}_{pointNum}_0 dx = {dx:0.000}{Resources.LanguageDic.pix}, dy = {dy:0.000}{Resources.LanguageDic.pix}");
                                            }
                                            else
                                            {
                                                ShowMessage($"{IOName}_{pointNum}_0 {Resources.LanguageDic.Match_fail}", Color.Red);
                                            }
                                            hImage2D匹配图 = carSetting.GetShowHIamge(map, carSetting.rectangle0[pointNum], carSetting.modeCenter0[pointNum].X, carSetting.modeCenter0[pointNum].Y, Px0, Py0, carSetting.ShapeModels0[pointNum]);
                                            formShow?.UpDataCamImage(hImage2D匹配图, IOName + pictureBoxIndex);/////////////////////////
                                        }
                                        else if (carSetting.rectangle0.ContainsKey(pointNum) && carSetting.Models0.ContainsKey(pointNum))
                                        {
                                            if (carSetting.FindPxPy(map, carSetting.rectangle0[pointNum], carSetting.Models0[pointNum], carSetting.gSets[pointNum].score0, carSetting.modeCenter0[pointNum].X, carSetting.modeCenter0[pointNum].Y, ref Px0, ref Py0, out double dx, out double dy))
                                            {
                                                led匹配 = true;
                                                ShowMessage($"{IOName}_{pointNum}_0 dx = {dx:0.000}{Resources.LanguageDic.pix}, dy = {dy:0.000}{Resources.LanguageDic.pix}");
                                            }
                                            else
                                            {
                                                ShowMessage($"{IOName}_{pointNum}_0 {Resources.LanguageDic.Match_fail}", Color.Red);
                                            }
                                            hImage2D匹配图 = carSetting.GetShowHIamge(map, carSetting.rectangle0[pointNum], carSetting.modeCenter0[pointNum].X, carSetting.modeCenter0[pointNum].Y, Px0, Py0);
                                            formShow?.UpDataCamImage(hImage2D匹配图, IOName + pictureBoxIndex);/////////////////////////
                                        }
                                        else
                                        {
                                            ShowMessage($"{IOName}_{pointNum}_0 {Resources.LanguageDic.para_has_not_made}", Color.Red);
                                        }
                                    }
                                    else
                                    {
                                        ShowMessage($"{IOName} {Resources.LanguageDic.no_cam_calib}", Color.Red);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    ShowMessage($"{IOName}_{pointNum}_0 {Resources.LanguageDic.match_error}:{ex}", Color.Red);
                                }
                            }
                            else
                            {
                                ShowMessage(IOName + $"{Resources.LanguageDic.cam0_fail}", Color.Red);
                            }
                        }
                    }

                    //坐标转换

                    double X = 0, Y = 0, Z = 0;
                    bool b有数据 = false;
                    if (light匹配1 && carSetting.gSets[pointNum].type == "棱")
                    {
                        if (camSetting.GetZFromLight(Px1, Py1, out float z1) && camSetting.GetXYFromLight(Px1, Py1, z1, out float camX, out float camY))
                        {
                            //相机转工具
                            double toolX = camSetting.cam2Tool.AffineTransPoint3d(camX, camY, z1, out double toolY, out double toolZ);

                            // 转为mm为单位
                            double toolX_mm = toolX * 1000;
                            double toolY_mm = toolY * 1000;
                            double toolZ_mm = toolZ * 1000;
                            //工具转基座标
                            HHomMat3D 工具转基座标 = new HPose(carSetting.gSets[pointNum].pX, carSetting.gSets[pointNum].pY, carSetting.gSets[pointNum].pZ, carSetting.gSets[pointNum].pRX, carSetting.gSets[pointNum].pRY, carSetting.gSets[pointNum].pRZ, "Rp+T", "abg", "point").PoseToHomMat3d();
                            double rbX = 工具转基座标.AffineTransPoint3d(toolX_mm, toolY_mm, toolZ_mm, out double rbY, out double rbZ);


                            //基座标转车身
                            double carX = carSetting.robot2Car.AffineTransPoint3d(rbX, rbY, rbZ, out double carY, out double carZ);

                            //车身加补偿
                            X = carX + carSetting.gSets[pointNum].offsetX;
                            Y = carY + carSetting.gSets[pointNum].offsetY;
                            Z = carZ + carSetting.gSets[pointNum].offsetZ;

                            rb.Add(pointNum, new Point3D(rbX, rbY, rbZ));//校准用
                            if (carSetting.gSets[pointNum].isBase)//重建坐标用
                            {
                                point3dBase.Add(pointNum, new Point3D(carSetting.gSets[pointNum].X, carSetting.gSets[pointNum].Y, carSetting.gSets[pointNum].Z));
                            }
                            b有数据 = true;
                        }
                        else
                        {
                            ShowMessage($"{IOName}_{pointNum}_1 {Resources.LanguageDic.The_photography_distance_exceeds_the_calibration_range}！", Color.Red);
                        }
                    }
                    else if (light匹配1 && light匹配2 && carSetting.gSets[pointNum].type == "槽")
                    {
                        if (camSetting.GetZFromLight(Px1, Py1, out float z1) && camSetting.GetXYFromLight(Px1, Py1, z1, out float camX1, out float camY1))
                        {
                            //相机转工具
                            double toolX1 = camSetting.cam2Tool.AffineTransPoint3d(camX1, camY1, z1, out double toolY1, out double toolZ1);
                            // 转为mm为单位
                            double toolX1_mm = toolX1 * 1000;
                            double toolY1_mm = toolY1 * 1000;
                            double toolZ1_mm = toolZ1 * 1000;
                            //工具转基座标
                            HHomMat3D 工具转基座标 = new HPose(carSetting.gSets[pointNum].pX, carSetting.gSets[pointNum].pY, carSetting.gSets[pointNum].pZ, carSetting.gSets[pointNum].pRX, carSetting.gSets[pointNum].pRY, carSetting.gSets[pointNum].pRZ, "Rp+T", "abg", "point").PoseToHomMat3d();
                            double rbX1 = 工具转基座标.AffineTransPoint3d(toolX1_mm, toolY1_mm, toolZ1_mm, out double rbY1, out double rbZ1);


                            //基座标转车身
                            double carX1 = carSetting.robot2Car.AffineTransPoint3d(rbX1, rbY1, rbZ1, out double carY1, out double carZ1);

                            if (camSetting.GetZFromLight(Px2, Py2, out float z2) && camSetting.GetXYFromLight(Px2, Py2, z2, out float camX2, out float camY2))
                            {
                                //相机转工具
                                double toolX2 = camSetting.cam2Tool.AffineTransPoint3d(camX2, camY2, z2, out double toolY2, out double toolZ2);
                                // 转为mm为单位
                                double toolX2_mm = toolX2 * 1000;
                                double toolY2_mm = toolY2 * 1000;
                                double toolZ2_mm = toolZ2 * 1000;
                                //工具转基座标
                                HHomMat3D 工具转基座标2 = new HPose(carSetting.gSets[pointNum].pX, carSetting.gSets[pointNum].pY, carSetting.gSets[pointNum].pZ, carSetting.gSets[pointNum].pRX, carSetting.gSets[pointNum].pRY, carSetting.gSets[pointNum].pRZ, "Rp+T", "abg", "point").PoseToHomMat3d();
                                double rbX2 = 工具转基座标2.AffineTransPoint3d(toolX2_mm, toolY2_mm, toolZ2_mm, out double rbY2, out double rbZ2);


                                //基座标转车身
                                double carX2 = carSetting.robot2Car.AffineTransPoint3d(rbX2, rbY2, rbZ2, out double carY2, out double carZ2);

                                //车身加补偿
                                X = carX2 - carX1 + carSetting.gSets[pointNum].offsetX;
                                Y = carY2 - carY1 + carSetting.gSets[pointNum].offsetY;
                                Z = carZ2 - carZ1 + carSetting.gSets[pointNum].offsetZ;

                                b有数据 = true;
                            }
                            else
                            {
                                ShowMessage($"{IOName}_{pointNum}_2 {Resources.LanguageDic.The_photography_distance_exceeds_the_calibration_range}！", Color.Red);
                            }
                        }
                        else
                        {
                            ShowMessage($"{IOName}_{pointNum}_1 {Resources.LanguageDic.The_photography_distance_exceeds_the_calibration_range}！", Color.Red);
                        }

                    }
                    else if (led匹配 && light匹配1 && light匹配2)//孔
                    {
                        //转换相机坐标
                        if (camSetting.GetZFromLight(Px1, Py1, out float z1))
                        {
                            if (camSetting.GetZFromLight(Px2, Py2, out float z2))
                            {
                                float camZ = (z1 + z2) / 2;
                                if (camSetting.GetXYFromLight(Px0, Py0, camZ, out float camX, out float camY))
                                {
                                    //相机转工具
                                    double toolX = camSetting.cam2Tool.AffineTransPoint3d(camX, camY, camZ, out double toolY, out double toolZ);

                                    // 转为mm为单位
                                    double toolX_mm = toolX * 1000;
                                    double toolY_mm = toolY * 1000;
                                    double toolZ_mm = toolZ * 1000;

                                    //工具转基座标
                                    HHomMat3D 工具转基座标 = new HPose(carSetting.gSets[pointNum].pX, carSetting.gSets[pointNum].pY, carSetting.gSets[pointNum].pZ, carSetting.gSets[pointNum].pRX, carSetting.gSets[pointNum].pRY, carSetting.gSets[pointNum].pRZ, "Rp+T", "abg", "point").PoseToHomMat3d();
                                    double rbX = 工具转基座标.AffineTransPoint3d(toolX_mm, toolY_mm, toolZ_mm, out double rbY, out double rbZ);


                                    //基座标转车身
                                    double carX = carSetting.robot2Car.AffineTransPoint3d(rbX, rbY, rbZ, out double carY, out double carZ);

                                    //车身加补偿
                                    X = carX + carSetting.gSets[pointNum].offsetX;
                                    Y = carY + carSetting.gSets[pointNum].offsetY;
                                    Z = carZ + carSetting.gSets[pointNum].offsetZ;

                                    rb.Add(pointNum, new Point3D(rbX, rbY, rbZ));//校准用
                                    if (carSetting.gSets[pointNum].isBase)//重建坐标用
                                    {
                                        point3dBase.Add(pointNum, new Point3D(carSetting.gSets[pointNum].X, carSetting.gSets[pointNum].Y, carSetting.gSets[pointNum].Z));
                                    }
                                    b有数据 = true;
                                }
                                else
                                {
                                    ShowMessage($"{IOName}_{pointNum}_0 {Resources.LanguageDic.The_photography_distance_exceeds_the_calibration_range}！", Color.Red);
                                }
                            }
                            else
                            {
                                ShowMessage($"{IOName}_{pointNum}_2 {Resources.LanguageDic.Laser_coordinates_exceed_the_calibration_range}！", Color.Red);
                            }
                        }
                        else
                        {
                            ShowMessage($"{IOName}_{pointNum}_1 {Resources.LanguageDic.Laser_coordinates_exceed_the_calibration_range}！", Color.Red);
                        }
                    }

                    //结果显示

                    if (b有数据)
                    {
                        point3d.Add(pointNum, new Point3D(X, Y, Z));

                        double dx = X - carSetting.gSets[pointNum].X;
                        double dy = Y - carSetting.gSets[pointNum].Y;
                        double dz = Z - carSetting.gSets[pointNum].Z;
                        double dd = Math.Sqrt(dx * dx + dy * dy + dz * dz);
                        ShowMessage($"{IOName}_{pointNum} dx = {dx:0.000}mm, dy = {dy:0.000}mm, dz = {dz:0.000}mm, dd = {dd:0.000}mm");

                        dataGridViewShow.BeginInvoke(new Action(() =>
                        {
                            int index = dataGridViewShow.Rows.Add(IOName + pointNum, X.ToString("0.000"), Y.ToString("0.000"), Z.ToString("0.000"), dx.ToString("0.000"), dy.ToString("0.000"), dz.ToString("0.000"), dd.ToString("0.000"));
                            if (dx < carSetting.gSets[pointNum].minDX || dx > carSetting.gSets[pointNum].maxDX)
                            {
                                dataGridViewShow.Rows[index].Cells[4].Style.BackColor = Color.Red;
                            }
                            if (dy < carSetting.gSets[pointNum].minDY || dy > carSetting.gSets[pointNum].maxDY)
                            {
                                dataGridViewShow.Rows[index].Cells[5].Style.BackColor = Color.Red;
                            }
                            if (dz < carSetting.gSets[pointNum].minDZ || dz > carSetting.gSets[pointNum].maxDZ)
                            {
                                dataGridViewShow.Rows[index].Cells[6].Style.BackColor = Color.Red;
                            }
                        }));

                        formShow?.UpDataXYZ(point3d[pointNum], IOName + pointNum);////////////////////////
                    }

                    formShow?.UpData灯(IOName, false);/////////////////////////////////

                    try
                    {
                        //存图
                        ShowMessage($"{IOName}_{pointNum:00} {Resources.LanguageDic.start_img}");
                        hImage2D匹配图?.WriteImage("jpeg 80", 0, $"{Path.GetDirectoryName(file1)}\\{IOName}_{pointNum:00}_0p.jpg");
                        hImageLight匹配图?.WriteImage("jpeg 80", 0, $"{Path.GetDirectoryName(file0)}\\{IOName}_{pointNum:00}_1p.jpg");
                        ShowMessage($"{IOName}_{pointNum:00} {Resources.LanguageDic.save_img_finish}");
                    }
                    catch (Exception ex)
                    {
                        ShowMessage($"{IOName}_{pointNum} {Resources.LanguageDic.save_img_error}：{ex}", Color.Red);
                    }

                }
                catch (Exception ex)
                {
                    ShowMessage($"{IOName}_{pointNum} {Resources.LanguageDic.cam_error}:{ex}", Color.Red);
                }
            }
        }
        #endregion

        #region 光源激光控制
        private void checkBoxLedL_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxLedL.Checked)
            {
                checkBoxLedL.BackColor = Color.Green;
                try
                {
                    if (相机["L"].IsOpen)
                    {
                        相机["L"].SetLine2Inverter(true);
                    }
                }
                catch (Exception ex)
                {
                    ShowMessage($"{Resources.LanguageDic.Serial_port_sending_exception}：" + ex.Message, Color.Red);
                }
            }
            else
            {
                checkBoxLedL.BackColor = 原色;
                try
                {
                    if (相机["L"].IsOpen)
                    {
                        相机["L"].SetLine2Inverter(false);
                    }
                }
                catch (Exception ex)
                {
                    ShowMessage($"{Resources.LanguageDic.Serial_port_sending_exception}：" + ex.Message, Color.Red);
                }
            }
        }

        private void checkBoxLightL_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxLightL.Checked)
            {
                checkBoxLightL.BackColor = Color.Green;
                try
                {
                    if (相机["L"].IsOpen)
                    {
                        相机["L"].SetLine1Inverter(true);
                    }
                }
                catch (Exception ex)
                {
                    ShowMessage($"{Resources.LanguageDic.Serial_port_sending_exception} ：" + ex.Message, Color.Red);
                }
            }
            else
            {
                checkBoxLightL.BackColor = 原色;
                try
                {
                    if (相机["L"].IsOpen)
                    {
                        相机["L"].SetLine1Inverter(false);
                    }
                }
                catch (Exception ex)
                {
                    ShowMessage($"{Resources.LanguageDic.Serial_port_sending_exception}：" + ex.Message, Color.Red);
                }
            }
        }

        private void checkBoxLedR_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxLedR.Checked)
            {
                checkBoxLedR.BackColor = Color.Green;
                try
                {
                    if (!sp.IsOpen)
                    {
                        sp.Open();
                    }
                    sp.WriteLine("SD1000#");
                }
                catch (Exception ex)
                {
                    ShowMessage($"{Resources.LanguageDic.Serial_port_sending_exception} ：" + ex.Message, Color.Red);
                }
            }
            else
            {
                checkBoxLedR.BackColor = 原色;
                try
                {
                    if (!sp.IsOpen)
                    {
                        sp.Open();
                    }
                    sp.WriteLine("SD0000#");
                }
                catch (Exception ex)
                {
                    ShowMessage($"{Resources.LanguageDic.Serial_port_sending_exception} ：" + ex.Message, Color.Red);
                }
            }
        }

        private void checkBoxLightR_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxLightR.Checked)
            {
                checkBoxLightR.BackColor = Color.Green;
                try
                {
                    if (!sp.IsOpen)
                    {
                        sp.Open();
                    }
                    sp.WriteLine("SC1000#");
                }
                catch (Exception ex)
                {
                    ShowMessage($"{Resources.LanguageDic.Serial_port_sending_exception} ：" + ex.Message, Color.Red);
                }
            }
            else
            {
                checkBoxLightR.BackColor = 原色;
                try
                {
                    if (!sp.IsOpen)
                    {
                        sp.Open();
                    }
                    sp.WriteLine("SC0000#");
                }
                catch (Exception ex)
                {
                    ShowMessage($"{Resources.LanguageDic.Serial_port_sending_exception} ：" + ex.Message, Color.Red);
                }
            }
        }
        #endregion

        private void buttonSetting_Click(object sender, EventArgs e)
        {
            var form = new FormSet();
            form.ShowDialog();
            if (form.IsSave)
            {
                buttonClear.Enabled = true;
            }
        }
        bool loadAgain = false;
        private void buttonClear_Click(object sender, EventArgs e)
        {
            loadAgain = true;
            buttonClear.Enabled = false;
        }
        private void buttonSave_Click(object sender, EventArgs e)
        {
            bool b = true;
            foreach (var item in 车型参数.Values)
            {
                if (!item.Save())
                {
                    b = false;
                    MessageBox.Show(item.CarName + $"{Resources.LanguageDic.save_fail}！");
                }
            }
            if (b)
            {
                MessageBox.Show($"{Resources.LanguageDic.save_success}！");
            }
        }
        bool bSkipStart = false;
        private void buttonSkipStart_Click(object sender, EventArgs e)
        {
            if (!bSkipStart)
            {
                if (DialogResult.OK == MessageBox.Show($"{Resources.LanguageDic.sure_skip_start_signal}？", $"{Resources.LanguageDic.tip}", MessageBoxButtons.OKCancel))
                {
                    bSkipStart = true;
                }
            }
        }

        private void FormRun_Paint(object sender, PaintEventArgs e)
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

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            //切换语言id
            GlobalVarAndFunc.LANGUAGE_ID = comboBox1.SelectedIndex;
            // 初始化界面
            GeneralFunc.ChangeLanguateFun(typeof(FormRun), this);
            if (GlobalVarAndFunc.SHOW_MESSAGE)
            {
                // 弹窗显示
                MessageBox.Show(Resources.LanguageDic.change_language_success);

            }
            //记录语言id
            GlobalVarAndFunc.WriteLanguageID();

            //从新打开formshow

            if (formShow != null && formShow.IsShow)
            {
                formShow.EnableClose = true;
                formShow.Invoke(new Action(() =>
                {
                    formShow.Close();
                }));
            }

            formShow = new FormShow();

#if GW
            formShow.Text = Resources.LanguageDic.Online_Det;
#else
            formShow.Text = "白车身后围在线测量系统";
#endif

            Thread thread = new Thread(() =>
            {
                formShow.ShowDialog();
            });
            thread.Start();
            while (!formShow.IsShow) Thread.Sleep(10);
            formShow.BeginInvoke(new Action(() =>
            {
                int index = 0;
                for (int i = 1; i < Screen.AllScreens.Length; i++)
                {
                    if (Screen.AllScreens[i].Bounds.Width >= Screen.AllScreens[i - 1].Bounds.Width)
                    {
                        index = i;
                    }
                }
                formShow.SetFormWindowState(FormWindowState.Normal);
                formShow.Left = Screen.AllScreens[index].Bounds.Left;
                formShow.Top = Screen.AllScreens[index].Bounds.Top;
                formShow.SetFormWindowState(FormWindowState.Maximized);
            }));
        }

        private void button1_Click(object sender, EventArgs e)
        {

            相机.Clear();
            相机参数.Clear();
            string[] camPaths = Directory.GetDirectories("Data\\Cam");
            foreach (var item in camPaths)
            {
                string name = Path.GetFileNameWithoutExtension(item);

                相机.Add(name, new BaslerCamera.Cam());

                CamSetting camSetting = new CamSetting(name);
                if (camSetting.Load(item))
                {
                    相机参数.Add(name, camSetting);
                    ShowMessage($"{name}{Resources.LanguageDic.cam}{Resources.LanguageDic.para_load_success}");
                }
                else
                {
                    ShowMessage($"{name}{Resources.LanguageDic.cam}{Resources.LanguageDic.para_load_fail}", Color.Red);
                }
            }
            foreach (var item in 相机.Keys)
            {
                if (相机[item].OpenByName(item))
                {
                    ShowMessage(item + $"{Resources.LanguageDic.cam_open_success}");
                }
                else
                {
                    ShowMessage(item + $"{Resources.LanguageDic.cam_open_fail}", Color.Red);
                    return;
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (相机["L"].IsOpen)
            {
                相机["L"].OneShot(out HImage img);

                formShow?.UpDataCamImage(img, "L1");

            }
        }

        private void button_setPLC_Click(object sender, EventArgs e)
        {
            //如果打开了，要先关闭
            plcClose();
            plc.ShowForm();
            //重新初始化
            plcInit();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var writeYRef = plc.Write("D1410", (int)(99999*100));

        }
    }
#if GW
    //public class DIO/*_GW*/
    //{
    //    public const string
    //                 开始 = "D1100.0",
    //                 重置流程 = "D1100.1",
    //                 已读信号 = "D1100.2",
    //                 空运行 = "D1100.3",

    //                 车型代码 = "D1101",
    //                 托盘号 = "D1102",
    //                 连番信息 = "D1103",
    //                 车身编号 = "D1200",

    //                 心跳 = "D1000.0",
    //                 准备好 = "D1000.1",
    //                 运行中 = "D1000.2",
    //                 测量结果 = "D1000.3",
    //                 测量结果完成 = "D1000.4",
    //                 车型代码反馈 = "D1001";


    //    public string Name;
    //    public string
    //                 拍照姿态到位,
    //                 轨迹末点,
    //                 点位号,

    //                 拍照完成,
    //                 点位测量结果,
    //                 屏蔽视觉,
    //                 点位号反馈,
    //                 x坐标实际值,
    //                 y坐标实际值,
    //                 z坐标实际值,
    //                 x坐标偏移值,
    //                 y坐标偏移值,
    //                 z坐标偏移值;

    //    public DIO/*_GW*/(string name)
    //    {
    //        Name = name;
    //        if (name == "L")
    //        {
    //            拍照姿态到位 = "D1110.0";
    //            轨迹末点 = "D1110.1";
    //            点位号 = "D1111";

    //            拍照完成 = "D1010.0";
    //            点位测量结果 = "D1010.1";
    //            屏蔽视觉 = "D1010.2";
    //            点位号反馈 = "D1011";
    //            x坐标实际值 = "D1012";
    //            y坐标实际值 = "D1014";
    //            z坐标实际值 = "D1016";
    //            x坐标偏移值 = "D1018";
    //            y坐标偏移值 = "D1020";
    //            z坐标偏移值 = "D1022";
    //        }
    //        else//R
    //        {
    //            拍照姿态到位 = "D1150.0";
    //            轨迹末点 = "D1150.1";
    //            点位号 = "D1151";

    //            拍照完成 = "D1050.0";
    //            点位测量结果 = "D1050.1";
    //            屏蔽视觉 = "D1050.2";
    //            点位号反馈 = "D1051";
    //            x坐标实际值 = "D1052";
    //            y坐标实际值 = "D1054";
    //            z坐标实际值 = "D1056";
    //            x坐标偏移值 = "D1058";
    //            y坐标偏移值 = "D1060";
    //            z坐标偏移值 = "D1062";
    //        }
    //    }



    //}

   

#else
    public class DIO
    {
        public const string
                     开始 = "CIO300.0",
                     已读信号 = "CIO300.1",
                     重置流程 = "CIO300.3",

                     车型代码 = "CIO302",
                     连番信息 = "CIO304",
                     托盘号 = "CIO303",

                     空运行 = "CIO300.2",
                     车架号1 = "E0.1042",
                     车架号2 = "E0.1043",
                     车架号3 = "E0.1044",
                     车架号4 = "E0.1045",
                     车架号5 = "E0.1046",
                     车架号6 = "E0.1047",
                     车架号7 = "E0.1048",

                     心跳 = "CIO3000.0",
                     准备好 = "CIO3000.1",
                     运行中 = "CIO3000.2",
                     测量结果可读 = "CIO3000.3",
                     测量结果 = "CIO3000.4";
        public string Name;
        public string
                     拍照姿态到位,
                     轨迹末点,
                     点位号1,
                     点位号2,
                     点位号4,
                     点位号8,

                     拍照完成;

        public DIO(string name)
        {
            Name = name;
            if (name == "L")
            {
                拍照姿态到位 = "CIO301.0";
                轨迹末点 = "CIO301.1";
                点位号1 = "CIO301.2";
                点位号2 = "CIO301.3";
                点位号4 = "CIO301.4";
                点位号8 = "CIO301.5";

                拍照完成 = "CIO3000.8";
            }
            else//R
            {
                拍照姿态到位 = "CIO301.8";
                轨迹末点 = "CIO301.9";
                点位号1 = "CIO301.10";
                点位号2 = "CIO301.11";
                点位号4 = "CIO301.12";
                点位号8 = "CIO301.13";

                拍照完成 = "CIO3000.12";
            }
        }
    }
#endif




}
