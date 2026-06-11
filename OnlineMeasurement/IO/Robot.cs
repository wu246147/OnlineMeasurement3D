//using HalconDotNet;
using HslCommunication;
using HslCommunication.Core;
using HslCommunication.ModBus;
using HslCommunication.Robot.YASKAWA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolTip;
using System.Globalization;
using System.Text.RegularExpressions;
using HalconDotNet;
using OnlineMeasurement.Resources;
using BaslerCamera.IO;
using BaslerCamera;

namespace OnlineMeasurement.IO
{
    [Serializable]
    public class RobotParam
    {
        public string IpAddress = "127.0.0.1";
        public int Port = 2000;
    }

    /// <summary>
    /// 机器人服务器基本库
    /// </summary>
    public interface IRobot
    {
        //多个机器人时，用来判断是哪个机器人，以及文件的读取和保存路径
        string robotName { get; set; }
        RobotParam Param { get; set; }
        Dictionary<string, IoAddress> IoDict { get; set; }
        bool IsOpen { get; }

        /// <summary>
        /// 获取最后一次错误信息
        /// </summary>
        /// <returns></returns>
        string ErrMsg { get; }
        /// <summary>
        /// 加载参数
        /// </summary>
        /// <returns></returns>
        bool Load();
        /// <summary>
        /// 保存参数
        /// </summary>
        /// <returns></returns>
        bool Save();
        /// <summary>
        /// 打开（连接）
        /// </summary>
        /// <returns></returns>
        bool Open();
        /// <summary>
        /// 关闭（断开）
        /// </summary>
        /// <returns></returns>
        bool Close();
        /// <summary>
        /// 获取坐标
        /// </summary>
        /// <param name="hPose"></param>
        /// <returns></returns>
        bool ReadPose(out HPose hPose);

        bool ReadAngle(out HTuple hAngel);

        bool Read(DI eDI, out string value);
        bool Read(DO eDO, out string value);
        bool Read(DI eDI, out bool value);
        bool Read(DO eDO, out bool value);
        bool Read(DI eDI, out ushort value);
        bool Read(DO eDO, out ushort value);
        bool Write(DO eDO, object value);
    }

    /// <summary>
    /// 安川机器人
    /// </summary>
    public class YRCRobot /*: IRobot*/
    {
        public string ErrMsg => _errMsg;
        string _errMsg;

        string ip = string.Empty;
        int port = 10040;
        YRCHighEthernet yrc = new YRCHighEthernet();
        public YRCRobot() { }

        public bool Read坐标(out string[] value)
        {
            OperateResult<byte[]> operateResult = yrc.ReadCommand(117, 101, 0, 1, null);
            if (operateResult.IsSuccess)
            {
                string[] array = new string[operateResult.Content.Length / 4];
                for (int i = 0; i < array.Length; i++)
                {
                    array[i] = byteTransform.TransInt32(operateResult.Content, i * 4).ToString();
                }
                value = array;
                return true;
            }
            else
            {
                value = null;
                _errMsg = operateResult.Message;
                return false;
            }
        }
        public bool Read坐标(out HPose hPose)
        {
            OperateResult<string[]> read = yrc.ReadPose();//关节坐标
            if (read.IsSuccess)
            {
                double x = double.Parse(read.Content[0]) / 1000;
                double y = double.Parse(read.Content[1]) / 1000;
                double z = double.Parse(read.Content[2]) / 1000;
                double rx = double.Parse(read.Content[3]) / 10000;
                double ry = double.Parse(read.Content[4]) / 10000;
                double rz = double.Parse(read.Content[5]) / 10000;
                hPose = new HPose(x,y,z,rx,ry,rz, "Rp+T", "abg", "point");
                //hPose.x = x; hPose.y = y; hPose.z = z;
                //hPose.rx = rx; hPose.ry = ry; hPose.rz = rz;
                //hPose.PoseType = 2;

            }
            else
            {
                hPose = null;
            }
            _errMsg = read.Message;
            return read.IsSuccess;
        }
        private IByteTransform byteTransform = new RegularByteTransform();
        public bool ReadPose(out HPose hPose)
        {
            OperateResult<byte[]> operateResult = yrc.ReadCommand(117, 101, 0, 1, null);
            if (operateResult.IsSuccess && operateResult.Content.Length >= 44)
            {
                int[] array = new int[6];
                for (int i = 0; i < array.Length; i++)
                {
                    array[i] = byteTransform.TransInt32(operateResult.Content, 20 + i * 4);
                }
                hPose = new HPose(array[0] / 1000000, array[1] / 1000000, array[2] / 1000000, array[3] / 10000, array[4] / 10000, array[5] / 10000, "Rp+T", "abg", "point");

                //hPose.x = array[0] / 1000000; hPose.y = array[1] / 1000000; hPose.z = array[2] / 1000000;
                //hPose.rx = array[3] / 10000; hPose.ry = array[4] / 10000; hPose.rz = array[5] / 10000;
                //hPose.PoseType = 2;

                return true;
            }
            else
            {
                hPose = null;
                _errMsg = operateResult.Message;
                return false;
            }
        }

        public bool Load()
        {
            ip = "192.168.255.1";
            port = 10040;
            return true;
        }

        public bool Save()
        {
            return true;
        }

        public bool Open()
        {
            yrc.IpAddress = ip;
            yrc.Port = port;
            return true;
        }

        public bool Close()
        {
            return true;
        }
    }
    /// <summary>
    /// JAKA机器人
    /// </summary>
    public class JAKARobot : IRobot
    {
        public string ErrMsg => _errMsg;
        string _errMsg;
        public bool IsOpen => _isOpen;
        bool _isOpen = false;

        HslCommunication.ModBus.ModbusTcpNet modbus = new HslCommunication.ModBus.ModbusTcpNet();

        string name = null;
        public string robotName { get => name; set => name = value; }


        RobotParam param = new RobotParam();
        Dictionary<string, IoAddress> ioDict = new Dictionary<string, IoAddress>();
        public RobotParam Param { get => param; set => param = value; }
        public Dictionary<string, IoAddress> IoDict { get => ioDict; set => ioDict = value; }

        public JAKARobot() { }

        public void ShowForm()
        {
            //new WindowRobot(this).ShowDialog();
        }

        public bool ReadPose(out HPose hPose)
        {
            var operateResult = modbus.ReadFloat("x=4;406", 6);
            if (operateResult.IsSuccess)
            {
                var array = operateResult.Content;
                hPose = new HPose(array[0] / 1000, array[1] / 1000, array[2] / 1000, array[3], array[4], array[5], "Rp+T", "abg", "point");

                //hPose = new HPose();

                //hPose.x = array[0] / 1000; hPose.y = array[1] / 1000; hPose.z = array[2] / 1000;
                //hPose.rx = array[3]; hPose.ry = array[4]; hPose.rz = array[5];
                //hPose.PoseType = 2;

                return true;
            }
            else
            {
                hPose = null;
                _errMsg = operateResult.Message;
                return false;
            }
        }

        public bool ReadAngle(out HTuple hAngle)
        {
            hAngle = new HTuple();

            return true;
        }
        /// <summary>
        /// 读取输出信号DO1~DO128
        /// </summary>
        /// <param name="index">1~128</param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool ReadDO(int index, out bool value)
        {
            int address = 8 + index - 1;
            var operateResult = modbus.ReadBool($"x=2;{address}");
            value = operateResult.Content;
            _errMsg = operateResult.Message;
            return operateResult.IsSuccess;
        }
        /// <summary>
        /// 读取输入信号DI1~DI128
        /// </summary>
        /// <param name="index">1~128</param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool ReadDI(int index, out bool value)
        {
            int address = 40 + index - 1;
            var operateResult = modbus.ReadBool($"x=1;{address}");
            value = operateResult.Content;
            _errMsg = operateResult.Message;
            return operateResult.IsSuccess;
        }
        /// <summary>
        /// 写入输入信号DI1~DI128
        /// </summary>
        /// <param name="index">1~128</param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool WriteDI(int index, bool value)
        {
            int address = 40 + index - 1;
            var operateResult = modbus.Write($"x=1;{address}", value);
            _errMsg = operateResult.Message;
            return operateResult.IsSuccess;
        }
        /// <summary>
        /// 读取输出信号AO1~AO32
        /// </summary>
        /// <param name="index">1~32</param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool ReadAO(int index, out ushort value)
        {
            int address = 96 + index - 1;
            var operateResult = modbus.ReadUInt16($"x=4;{address}");
            value = operateResult.Content;
            _errMsg = operateResult.Message;
            return operateResult.IsSuccess;
        }
        /// <summary>
        /// 读取输出信号AI1~AI32
        /// </summary>
        /// <param name="index">1~32</param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool ReadAI(int index, out ushort value)
        {
            int address = 100 + index - 1;
            var operateResult = modbus.ReadUInt16($"x=3;{address}");
            value = operateResult.Content;
            _errMsg = operateResult.Message;
            return operateResult.IsSuccess;
        }
        /// <summary>
        /// 写入输入信号AI1~AI32
        /// </summary>
        /// <param name="index">1~32</param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool WriteAI(int index, ushort value)
        {
            int address = 100 + index - 1;
            var operateResult = modbus.Write($"x=3;{address}", value);
            _errMsg = operateResult.Message;
            return operateResult.IsSuccess;
        }
        /// <summary>
        /// 读取输出信号AO33~AO64
        /// </summary>
        /// <param name="index">33~64</param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool ReadAO(int index, out float value)
        {
            int address = 128 + (index - 33) * 2;
            var operateResult = modbus.ReadUInt16($"x=4;{address}");
            value = operateResult.Content;
            _errMsg = operateResult.Message;
            return operateResult.IsSuccess;
        }
        /// <summary>
        /// 读取输出信号AI33~AI64
        /// </summary>
        /// <param name="index">33~64</param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool ReadAI(int index, out float value)
        {
            int address = 132 + (index - 33) * 2;
            var operateResult = modbus.ReadUInt16($"x=3;{address}");
            value = operateResult.Content;
            _errMsg = operateResult.Message;
            return operateResult.IsSuccess;
        }
        /// <summary>
        /// 写入输入信号AI33~AI64
        /// </summary>
        /// <param name="index">33~64</param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool WriteAI(int index, float value)
        {
            int address = 132 + (index - 33) * 2;
            var operateResult = modbus.Write($"x=3;{address}", value);
            _errMsg = operateResult.Message;
            return operateResult.IsSuccess;
        }

        public bool Load()
        {
            //ip = "192.168.100.120";
            //port = 6502;
            bool result = true;
            string basePath = AppDomain.CurrentDomain.BaseDirectory + $"Data\\Robot\\{robotName}\\";
            try
            {
                string paramPath = basePath + "JAKAParam.xml";
                if (File.Exists(paramPath))
                {
                    XmlSerializer xml = new XmlSerializer(param.GetType());
                    using (FileStream stream = new FileStream(paramPath, FileMode.OpenOrCreate))
                    {
                        RobotParam _ = (RobotParam)xml.Deserialize(stream);
                        if (_ != null)
                        {
                            param = _;
                        }
                        else
                        {
                            _errMsg = paramPath + Resources.LanguageDic.file_format_error;
                            result = false;
                        }
                    }
                }
                else
                {
                    _errMsg = paramPath + Resources.LanguageDic.file_not_exist;
                    result = false;
                }
            }
            catch (Exception ex)
            {
                result = false;
                _errMsg = ex.ToString();
            }

            try
            {
                string paramPath = basePath + "JAKAIoParam.xml";
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
                        result = false;
                        _errMsg = paramPath + Resources.LanguageDic.file_format_error;
                    }
                    else
                    {
                        ioDict = ios.ToDictionary(n => { return n.IoName; });
                    }
                }
                else
                {
                    result = false;
                    _errMsg = paramPath + Resources.LanguageDic.file_not_exist;
                }
            }
            catch (Exception ex)
            {
                result = false;
                _errMsg = ex.ToString();
            }
            return result;
        }

        public bool Save()
        {
            bool result = true;
            try
            {
                string basePath = AppDomain.CurrentDomain.BaseDirectory + $"Data\\Robot\\{robotName}\\";
                if (!Directory.Exists(basePath))
                {
                    Directory.CreateDirectory(basePath);
                }

                string OpcParamPath = basePath + "JAKAParam.xml";
                XmlSerializer xml = new XmlSerializer(param.GetType());
                using (FileStream stream = new FileStream(OpcParamPath, FileMode.Create))
                {
                    xml.Serialize(stream, param);
                }

                List<IoAddress> ios = ioDict.Values.ToList();
                string ioParamPath = basePath + "JAKAIoParam.xml";
                XmlSerializer ioXml = new XmlSerializer(ios.GetType());
                using (FileStream stream = new FileStream(ioParamPath, FileMode.Create))
                {
                    ioXml.Serialize(stream, ios);
                }
            }
            catch (Exception ex)
            {
                result = false;
                _errMsg = ex.ToString();
            }
            return result;
        }

        public bool Open()
        {
            modbus.IpAddress = param.IpAddress;
            modbus.Port = param.Port;
            modbus.ConnectTimeOut = 5000;     // 连接超时，单位毫秒
            modbus.ReceiveTimeOut = 3000;     // 接收超时，单位毫秒
            modbus.Station = 1;
            modbus.AddressStartWithZero = true;
            modbus.IsCheckMessageId = true;
            modbus.IsStringReverse = false;
            modbus.DataFormat = HslCommunication.Core.DataFormat.ABCD;

            var result = modbus.ConnectServer();
            if (result.IsSuccess)
            {
                _isOpen = true;
                return true;
            }
            else
            {
                _errMsg = result.Message;
                return false;
            }
        }

        public bool Close()
        {
            modbus.ConnectClose();
            _isOpen = false;
            return true;
        }

        public bool Read(DI eDI, out string value)
        {
            _errMsg = Resources.LanguageDic.not_support_strings;
            value = "";
            return false;
        }

        public bool Read(DO eDO, out string value)
        {
            _errMsg = Resources.LanguageDic.not_support_strings;
            value = "";
            return false;
        }

        public bool Read(DI eDI, out bool value)
        {
            return Read(eDI.ToString(), out value);
        }
        public bool Read(DO eDO, out bool value)
        {
            return Read(eDO.ToString(), out value);
        }
        private bool Read(string ioName, out bool value)
        {
            if (ioDict.ContainsKey(ioName))
            {
                string ioAddress = ioDict[ioName].Address.Trim();
                if (!string.IsNullOrEmpty(ioAddress))
                {
                    if (ioAddress.Length > 2 && ioAddress[0] == 'D' && int.TryParse(ioAddress.Substring(2, ioAddress.Length - 2), out int index))
                    {
                        if (ioAddress[1] == 'I')
                        {
                            return ReadDI(index, out value);
                        }
                        else if (ioAddress[1] == 'O')
                        {
                            return ReadDO(index, out value);
                        }
                    }
                    _errMsg = ioName + Resources.LanguageDic.Address_format_mismatch; 
                }
                else
                {
                    value = false;
                    return true;
                }
            }
            else
            {
                _errMsg = ioName + Resources.LanguageDic.Address_not_assigned; 
            }
            value = false;
            return false;
        }

        public bool Read(DI eDI, out ushort value)
        {
            return Read(eDI.ToString(), out value);
        }
        public bool Read(DO eDO, out ushort value)
        {
            return Read(eDO.ToString(), out value);
        }
        private bool Read(string ioName, out ushort value)
        {
            if (ioDict.ContainsKey(ioName))
            {
                string ioAddress = ioDict[ioName].Address.Trim();
                if (!string.IsNullOrEmpty(ioAddress))
                {
                    if (ioAddress.Length > 2 && ioAddress[0] == 'A' && int.TryParse(ioAddress.Substring(2, ioAddress.Length - 2), out int index))
                    {
                        if (ioAddress[1] == 'I')
                        {
                            return ReadAI(index, out value);
                        }
                        else if (ioAddress[1] == 'O')
                        {
                            return ReadAO(index, out value);
                        }
                    }
                    _errMsg = ioName + Resources.LanguageDic.Address_format_mismatch;
                }
                else
                {
                    value = 0;
                    return true;
                }
            }
            else
            {
                _errMsg = ioName + Resources.LanguageDic.Address_not_assigned;
            }
            value = 0;
            return false;
        }

        object iolock = new object();
        public bool Write(DO eDO, object value)
        {
            if (ioDict.ContainsKey(eDO.ToString()))
            {
                string ioAddress = ioDict[eDO.ToString()].Address.Trim();
                if (!string.IsNullOrEmpty(ioAddress))
                {
                    lock (iolock)
                    {
                        if (value is bool)
                        {
                            if (ioAddress.Length > 2 && ioAddress[0] == 'D' && int.TryParse(ioAddress.Substring(2, ioAddress.Length - 2), out int index))
                            {
                                if (ioAddress[1] == 'I')
                                {
                                    return WriteDI(index, (bool)value);
                                }
                            }
                            _errMsg = eDO.ToString() + Resources.LanguageDic.Address_format_mismatch;
                        }
                        else if (value is ushort)
                        {
                            if (ioAddress.Length > 2 && ioAddress[0] == 'A' && int.TryParse(ioAddress.Substring(2, ioAddress.Length - 2), out int index))
                            {
                                if (ioAddress[1] == 'I')
                                {
                                    return WriteAI(index, (ushort)value);
                                }
                            }
                            _errMsg = eDO.ToString() + Resources.LanguageDic.Address_format_mismatch;
                        }
                        else
                        {
                            _errMsg = Resources.LanguageDic.Writing_format_not_supported;
                            return false;
                        }
                    }
                }
                else
                {
                    return true;
                }
            }
            else
            {
                _errMsg = eDO.ToString() + Resources.LanguageDic.Address_not_assigned; 
            }
            return false;
        }

    }
    /// <summary>
    /// 发那科机器人
    /// </summary>
    public class FanucRobot : IRobot
    {
        public string ErrMsg => _errMsg;
        string _errMsg;
        public bool IsOpen => _isOpen;
        bool _isOpen = false;

        string name = null;
        public string robotName { get => name; set => name = value; }


        RobotParam param = new RobotParam();
        Dictionary<string, IoAddress> ioDict = new Dictionary<string, IoAddress>();
        public RobotParam Param { get => param; set => param = value; }
        public Dictionary<string, IoAddress> IoDict { get => ioDict; set => ioDict = value; }

        HslCommunication.Robot.FANUC.FanucInterfaceNet robot = new HslCommunication.Robot.FANUC.FanucInterfaceNet();
        public FanucRobot() { }

        public void ShowForm()
        {
            //new WindowRobot(this).ShowDialog();
        }

        public bool ReadPose(out HPose hPose)
        {
            OperateResult<float[]> xyzwpr = robot.ReadFloat("D751", 9);
            if (xyzwpr.IsSuccess)
            {
                double x = xyzwpr.Content[0] / 1000;
                double y = xyzwpr.Content[1] / 1000;
                double z = xyzwpr.Content[2] / 1000;
                double rx = xyzwpr.Content[3];
                double ry = xyzwpr.Content[4];
                double rz = xyzwpr.Content[5];
                hPose = new HPose(x, y, z, rx, ry, rz, "Rp+T", "abg", "point");

                //hPose = new HPose();

                //hPose.x = x;
                //hPose.y = y;
                //hPose.z = z;
                //hPose.rx = rx;
                //hPose.ry = ry;
                //hPose.rz = rz;
                //hPose.PoseType = 2;
            }
            else
            {
                hPose = null;
            }
            _errMsg = xyzwpr.Message;
            return xyzwpr.IsSuccess;
        }
        public bool ReadAngle(out HTuple hAngle)
        {
            hAngle = new HTuple();

            return true;
        }
        public bool Load()
        {
            bool result = true;
            string basePath = AppDomain.CurrentDomain.BaseDirectory + $"Data\\Robot\\{robotName}\\";
            try
            {
                string paramPath = basePath + "FanucParam.xml";
                if (File.Exists(paramPath))
                {
                    XmlSerializer xml = new XmlSerializer(param.GetType());
                    using (FileStream stream = new FileStream(paramPath, FileMode.OpenOrCreate))
                    {
                        RobotParam _ = (RobotParam)xml.Deserialize(stream);
                        if (_ != null)
                        {
                            param = _;
                        }
                        else
                        {
                            _errMsg = paramPath + Resources.LanguageDic.file_format_error;
                            result = false;
                        }
                    }
                }
                else
                {
                    _errMsg = paramPath + Resources.LanguageDic.file_not_exist;
                    result = false;
                }
            }
            catch (Exception ex)
            {
                result = false;
                _errMsg = ex.ToString();
            }

            try
            {
                string paramPath = basePath + "FanucIoParam.xml";
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
                        result = false;
                        _errMsg = paramPath + Resources.LanguageDic.file_format_error;
                    }
                    else
                    {
                        ioDict = ios.ToDictionary(n => { return n.IoName; });
                    }
                }
                else
                {
                    result = false;
                    _errMsg = paramPath + Resources.LanguageDic.file_not_exist;
                }
            }
            catch (Exception ex)
            {
                result = false;
                _errMsg = ex.ToString();
            }
            return result;
        }

        public bool Save()
        {
            bool result = true;
            try
            {
                string basePath = AppDomain.CurrentDomain.BaseDirectory + $"Data\\Robot\\{robotName}\\";
                if (!Directory.Exists(basePath))
                {
                    Directory.CreateDirectory(basePath);
                }

                string OpcParamPath = basePath + "FanucParam.xml";
                XmlSerializer xml = new XmlSerializer(param.GetType());
                using (FileStream stream = new FileStream(OpcParamPath, FileMode.Create))
                {
                    xml.Serialize(stream, param);
                }

                List<IoAddress> ios = ioDict.Values.ToList();
                string ioParamPath = basePath + "FanucIoParam.xml";
                XmlSerializer ioXml = new XmlSerializer(ios.GetType());
                using (FileStream stream = new FileStream(ioParamPath, FileMode.Create))
                {
                    ioXml.Serialize(stream, ios);
                }
            }
            catch (Exception ex)
            {
                result = false;
                _errMsg = ex.ToString();
            }
            return result;
        }

        public bool Open()
        {
            robot.IpAddress = param.IpAddress;
            robot.Port = param.Port;
            robot.ConnectTimeOut = 5000;     // 连接超时，单位毫秒
            robot.ReceiveTimeOut = 3000;     // 接收超时，单位毫秒
            //robot.Station = 1;
            //robot.AddressStartWithZero = true;
            //robot.IsCheckMessageId = true;
            //robot.IsStringReverse = false;
            //robot.DataFormat = HslCommunication.Core.DataFormat.ABCD;

            var result = robot.ConnectServer();
            if (result.IsSuccess)
            {
                _isOpen = true;
                return true;
            }
            else
            {
                _errMsg = result.Message;
                return false;
            }
        }

        public bool Close()
        {
            robot.ConnectClose();
            _isOpen = false;
            return true;
        }

        public bool Read(DI eDI, out string value)
        {
            _errMsg = Resources.LanguageDic.not_support_strings;
            value = "";
            return false;
        }

        public bool Read(DO eDO, out string value)
        {
            _errMsg = Resources.LanguageDic.not_support_strings;
            value = "";
            return false;
        }

        public bool Read(DI eDI, out bool value)
        {
            return Read(eDI.ToString(), out value);
        }
        public bool Read(DO eDO, out bool value)
        {
            return Read(eDO.ToString(), out value);
        }
        private bool Read(string ioName, out bool value)
        {
            if (ioDict.ContainsKey(ioName))
            {
                string ioAddress = ioDict[ioName].Address.Trim();
                if (!string.IsNullOrEmpty(ioAddress))
                {
                    if (ioAddress[0] == 'R')
                    {
                        var operateResult = robot.ReadUInt16(ioAddress);
                        value = operateResult.Content > 0;
                        _errMsg = operateResult.Message;
                        return operateResult.IsSuccess;
                    }
                    else
                    {
                        var operateResult = robot.ReadBool(ioAddress);
                        value = operateResult.Content;
                        _errMsg = operateResult.Message;
                        return operateResult.IsSuccess;
                    }
                }
                else
                {
                    value = false;
                    return true;
                }
            }
            else
            {
                _errMsg = ioName + Resources.LanguageDic.Address_not_assigned;
            }
            value = false;
            return false;
        }

        public bool Read(DI eDI, out ushort value)
        {
            return Read(eDI.ToString(), out value);
        }
        public bool Read(DO eDO, out ushort value)
        {
            return Read(eDO.ToString(), out value);
        }
        private bool Read(string ioName, out ushort value)
        {
            if (ioDict.ContainsKey(ioName))
            {
                string ioAddress = ioDict[ioName].Address.Trim();
                if (!string.IsNullOrEmpty(ioAddress))
                {
                    var operateResult = robot.ReadUInt16(ioAddress);
                    value = operateResult.Content;
                    _errMsg = operateResult.Message;
                    return operateResult.IsSuccess;
                }
                else
                {
                    value = 0;
                    return true;
                }
            }
            else
            {
                _errMsg = ioName + Resources.LanguageDic.Address_not_assigned;
            }
            value = 0;
            return false;
        }

        object iolock = new object();
        public bool Write(DO eDO, object value)
        {
            if (ioDict.ContainsKey(eDO.ToString()))
            {
                string ioAddress = ioDict[eDO.ToString()].Address.Trim();
                if (!string.IsNullOrEmpty(ioAddress))
                {
                    lock (iolock)
                    {
                        if (value is bool)
                        {
                            if (ioAddress[0] == 'R')
                            {
                                var operateResult = robot.Write(ioAddress, (bool)value ? 1 : 0);
                                _errMsg = operateResult.Message;
                                return operateResult.IsSuccess;
                            }
                            else
                            {
                                var operateResult = robot.Write(ioAddress, (bool)value);
                                _errMsg = operateResult.Message;
                                return operateResult.IsSuccess;
                            }
                        }
                        else if (value is ushort)
                        {
                            var operateResult = robot.Write(ioAddress, (ushort)value);
                            _errMsg = operateResult.Message;
                            return operateResult.IsSuccess;
                        }
                        else
                        {
                            _errMsg = Resources.LanguageDic.Writing_format_not_supported;
                            return false;
                        }
                    }
                }
                else
                {
                    return true;
                }
            }
            else
            {
                _errMsg = eDO.ToString() + Resources.LanguageDic.Address_not_assigned;
            }
            return false;
        }
    }
    /// <summary>
    /// 库卡机器人
    /// </summary>
    public class KukaRobot : IRobot
    {
        public string ErrMsg => _errMsg;
        string _errMsg;
        public bool IsOpen => _isOpen;
        bool _isOpen = false;

        string name = null;
        public string robotName { get => name; set => name = value; }

        RobotParam param = new RobotParam();
        Dictionary<string, IoAddress> ioDict = new Dictionary<string, IoAddress>();
        public RobotParam Param { get => param; set => param = value; }
        public Dictionary<string, IoAddress> IoDict { get => ioDict; set => ioDict = value; }

        HslCommunication.Robot.KUKA.KukaTcpNet robot = new HslCommunication.Robot.KUKA.KukaTcpNet();
        public KukaRobot() { }

        public void ShowForm()
        {
            new RobotForm(this).ShowDialog();
        }

        public bool FormatTransformPose(string info, out HPose hPose)
        {
            hPose = new HPose();
            try
            {
                var pattern = @"\b([XYZABC])\s+([-+]?\d*\.?\d+)";
                var matches = Regex.Matches(info, pattern);

                var dict = new Dictionary<string, double>();
                foreach (Match m in matches)
                {
                    dict[m.Groups[1].Value] = double.Parse(m.Groups[2].Value, CultureInfo.InvariantCulture);
                }

                if (dict.ContainsKey("X"))
                {
                    double x = dict["X"];
                    hPose[0] = x/1000;
                }
                else { return false; }

                if (dict.ContainsKey("Y"))
                {
                    double y = dict["Y"];
                    hPose[1] = y/1000;
                }
                else { return false; }
                if (dict.ContainsKey("Z"))
                {
                    double z = dict["Z"];
                    hPose[2] = z / 1000;
                }
                else { return false; }
                if (dict.ContainsKey("A"))
                {
                    double a = dict["A"];
                    hPose[5] = a;
                }
                else { return false; }
                if (dict.ContainsKey("B"))
                {
                    double b = dict["B"];
                    hPose[4] = b;
                }
                else { return false; }
                if (dict.ContainsKey("C"))
                {
                    double c = dict["C"];
                    hPose[3] = c;
                }
                else { return false; }

                hPose[6] = 2;
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }
        public bool FormatTransformAXIS(string info, out HTuple hAngle)
        {
            hAngle = new HTuple(0,0,0,0,0,0);
            try
            {
                var pattern = @"\b(A[1-6])\s+([-+]?\d*\.?\d+)";
                var matches = Regex.Matches(info, pattern);

                var dict = new Dictionary<string, double>();
                foreach (Match m in matches)
                {
                    dict[m.Groups[1].Value] = double.Parse(m.Groups[2].Value, CultureInfo.InvariantCulture);
                }

                if (dict.ContainsKey("A1"))
                {
                    hAngle[0] = dict["A1"];
                }
                else { return false; }

                if (dict.ContainsKey("A2"))
                {
                    hAngle[1] = dict["A2"];
                }
                else { return false; }
                if (dict.ContainsKey("A3"))
                {
                    hAngle[2] = dict["A3"];
                }
                else { return false; }
                if (dict.ContainsKey("A4"))
                {
                    hAngle[3] = dict["A4"];
                }
                else { return false; }
                if (dict.ContainsKey("A5"))
                {
                    hAngle[4] = dict["A5"];
                }
                else { return false; }
                if (dict.ContainsKey("A6"))
                {
                    hAngle[5] = dict["A6"];
                }
                else { return false; }
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }


        public bool ReadPose(out HPose hPose)
        {
            bool rt = Read("$POS_ACT", out string info);
            hPose = new HPose();

            if (rt)
            {
                rt = FormatTransformPose(info, out hPose); 
            }
            

            return rt;
        }
        public bool ReadAngle(out HTuple hAngle)
        {
            bool rt = Read("$AXIS_ACT", out string info);
            hAngle = new HTuple();

            if (rt)
            {
                rt = FormatTransformAXIS(info, out hAngle);
            }


            return rt;
        }
        public bool Read(string key, out string info)
        {
            var value = robot.ReadString(key);

            info = value.Content;
            return value.IsSuccess;
        }


        public bool Load()
        {
            bool result = true;
            string basePath = AppDomain.CurrentDomain.BaseDirectory + $"Data\\Robot\\{robotName}\\";
            try
            {
                string paramPath = basePath + "KukaParam.xml";
                if (File.Exists(paramPath))
                {
                    XmlSerializer xml = new XmlSerializer(param.GetType());
                    using (FileStream stream = new FileStream(paramPath, FileMode.OpenOrCreate))
                    {
                        RobotParam _ = (RobotParam)xml.Deserialize(stream);
                        if (_ != null)
                        {
                            param = _;
                        }
                        else
                        {
                            _errMsg = paramPath + Resources.LanguageDic.file_format_error;
                            result = false;
                        }
                    }
                }
                else
                {
                    _errMsg = paramPath + Resources.LanguageDic.file_not_exist;
                    result = false;
                }
            }
            catch (Exception ex)
            {
                result = false;
                _errMsg = ex.ToString();
            }

            try
            {
                string paramPath = basePath + "KukaIoParam.xml";
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
                        result = false;
                        _errMsg = paramPath + Resources.LanguageDic.file_format_error;
                    }
                    else
                    {
                        ioDict = ios.ToDictionary(n => { return n.IoName; });
                    }
                }
                else
                {
                    result = false;
                    _errMsg = paramPath + Resources.LanguageDic.file_not_exist;
                }
            }
            catch (Exception ex)
            {
                result = false;
                _errMsg = ex.ToString();
            }
            return result;
        }

        public bool Save()
        {
            bool result = true;
            try
            {
                string basePath = AppDomain.CurrentDomain.BaseDirectory + $"Data\\Robot\\{robotName}\\";
                if (!Directory.Exists(basePath))
                {
                    Directory.CreateDirectory(basePath);
                }

                string OpcParamPath = basePath + "KukaParam.xml";
                XmlSerializer xml = new XmlSerializer(param.GetType());
                using (FileStream stream = new FileStream(OpcParamPath, FileMode.Create))
                {
                    xml.Serialize(stream, param);
                }

                List<IoAddress> ios = ioDict.Values.ToList();
                string ioParamPath = basePath + "KukaIoParam.xml";
                XmlSerializer ioXml = new XmlSerializer(ios.GetType());
                using (FileStream stream = new FileStream(ioParamPath, FileMode.Create))
                {
                    ioXml.Serialize(stream, ios);
                }
            }
            catch (Exception ex)
            {
                result = false;
                _errMsg = ex.ToString();
            }
            return result;
        }

        public bool Open()
        {
            robot.IpAddress = param.IpAddress;
            robot.Port = param.Port;
            robot.ConnectTimeOut = 5000;     // 连接超时，单位毫秒
            robot.ReceiveTimeOut = 3000;     // 接收超时，单位毫秒
            //robot.Station = 1;
            //robot.AddressStartWithZero = true;
            //robot.IsCheckMessageId = true;
            //robot.IsStringReverse = false;
            //robot.DataFormat = HslCommunication.Core.DataFormat.ABCD;

            var result = robot.ConnectServer();
            if (result.IsSuccess)
            {
                _isOpen = true;
                return true;
            }
            else
            {
                _errMsg = result.Message;
                return false;
            }
        }

        public bool Close()
        {
            robot.ConnectClose();
            _isOpen = false;
            return true;
        }

        public bool Read(DI eDI, out string value)
        {
            return Read(eDI.ToString(), out value);
        }

        public bool Read(DO eDO, out string value)
        {
            return Read(eDO.ToString(), out value);
        }

        public bool Read(DI eDI, out bool value)
        {
            _errMsg = "不支持bool";
            value = false;
            return false;
        }
        public bool Read(DO eDO, out bool value)
        {
            return Read(eDO.ToString(), out value);
        }
        private bool Read(string ioName, out bool value)
        {
            if (ioDict.ContainsKey(ioName))
            {
                string ioAddress = ioDict[ioName].Address.Trim();
                if (!string.IsNullOrEmpty(ioAddress))
                {
                    _errMsg = "不支持bool";
                    value = false;
                    return false;


                }
                else
                {
                    value = false;
                    return true;
                }
            }
            else
            {
                _errMsg = ioName + "地址未分配";
            }
            value = false;
            return false;
        }

        public bool Read(DI eDI, out ushort value)
        {
            return Read(eDI.ToString(), out value);
        }
        public bool Read(DO eDO, out ushort value)
        {
            return Read(eDO.ToString(), out value);
        }
        private bool Read(string ioName, out ushort value)
        {
            if (ioDict.ContainsKey(ioName))
            {
                string ioAddress = ioDict[ioName].Address.Trim();
                if (!string.IsNullOrEmpty(ioAddress))
                {
                    _errMsg = "不支持int";
                    value = 0;
                    return false;


                }
                else
                {
                    value = 0;
                    return true;
                }
            }
            else
            {
                _errMsg = ioName + "地址未分配";
            }
            value = 0;
            return false;
        }

        object iolock = new object();
        public bool Write(DO eDO, object value)
        {
            if (ioDict.ContainsKey(eDO.ToString()))
            {
                string ioAddress = ioDict[eDO.ToString()].Address.Trim();
                if (!string.IsNullOrEmpty(ioAddress))
                {
                    lock (iolock)
                    {
                        _errMsg = "不支持写入";
                        value = false;
                        return false;
                    }
                }
                else
                {
                    return true;
                }
            }
            else
            {
                _errMsg = eDO.ToString() + "地址未分配";
            }
            return false;
        }
    }


    public class KawasakiRobot : IRobot
    {

        public string serializationInfo(double x, double y, double z, double a, double b, double c, int result, string setflat)
        {
            return "";
        }

        private static bool deserializationInfo(string info, out float X, out float Y, out float Z, out float RX, out float RY, out float RZ, out float R1, out float R2, out float R3, out float R4, out float R5, out float R6, out int programID, out int commandID, out int pointID)
        {
            X = 0;
            Y = 0;
            Z = 0;
            RX = 0;
            RY = 0;
            RZ = 0;
            R1 = 0;
            R2 = 0;
            R3 = 0;
            R4 = 0;
            R5 = 0;
            R6 = 0;
            programID = 0;
            commandID = 0;
            pointID = 0;

            try
            {
                // 去掉末尾的 #
                string raw = info.TrimEnd('#').Trim();
                string[] parts = raw.Split(',');

                // 辅助：安全解析 float，空串或解析失败返回默认值
                float SafeFloat(int index, float defaultValue = 0f)
                {
                    if (index < 0 || index >= parts.Length) return defaultValue;
                    string s = parts[index].Trim();
                    if (string.IsNullOrEmpty(s)) return defaultValue;
                    return float.TryParse(s, System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture, out float v) ? v : defaultValue;
                }

                // 辅助：安全解析 int
                int SafeInt(int index, int defaultValue = 0)
                {
                    if (index < 0 || index >= parts.Length) return defaultValue;
                    string s = parts[index].Trim();
                    if (string.IsNullOrEmpty(s)) return defaultValue;
                    return int.TryParse(s, out int v) ? v : defaultValue;
                }

                // ---- 关节角 J1~J6 (索引 7~12) ----
                R1 = SafeFloat(7);    // J1: -86.6277
                R2 = SafeFloat(8);    // J2: -27.53063
                R3 = SafeFloat(9);    // J3: -18.65816
                R4 = SafeFloat(10);   // J4: -178.16177
                R5 = SafeFloat(11);   // J5: 12.41696
                R6 = SafeFloat(12);   // J6: -5.59366

                // ---- 末端位姿 X,Y,Z,RX,RY,RZ (倒数第 7~2 位) ----
                int total = parts.Length;
                X = SafeFloat(total - 6);   // -2446.65137
                Y = SafeFloat(total - 5);   // 218.11565
                Z = SafeFloat(total - 4);   // 2085.89941
                RX = SafeFloat(total - 3);   // 87.2569
                RY = SafeFloat(total - 2);   // 86.23324
                RZ = SafeFloat(total - 1);   // -3.54589

                //zyz 转 xyz
                double transformRX = 0, transformRY = 0, transformRz = 0;
                int robot_r_type = 2;   //机器人的坐标系类型，0为xyz，1为zyx，2为zyz
                int alg_r_type = 0;      //相机的坐标系类型，默认都是0，0为xyz，1为zyx，2为zyz

                Tool.transformCartPose2(RX, RY, RZ, robot_r_type, ref transformRX, ref transformRY, ref transformRz, alg_r_type);

                RX = (float)transformRX;
                RY = (float)transformRY;
                RZ = (float)transformRz;

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[deserializationInfo] 解析异常: {ex.Message}");
                return false;
            }
        }


        public string ErrMsg => _errMsg;
        string _errMsg;
        public bool IsOpen => _isOpen;
        bool _isOpen = false;

        string name = null;
        public string robotName { get => name; set => name = value; }

        RobotParam param = new RobotParam();
        Dictionary<string, IoAddress> ioDict = new Dictionary<string, IoAddress>();
        public RobotParam Param { get => param; set => param = value; }
        public Dictionary<string, IoAddress> IoDict { get => ioDict; set => ioDict = value; }


        //string _ip = string.Empty;
        //int _port = 60008;
        //HslCommunication.Robot.FANUC.FanucInterfaceNet robot = new HslCommunication.Robot.FANUC.FanucInterfaceNet();

        TCP_Client robot;


        //寄存数据
        bool _isExist = false;
        float _X = 0, _Y = 0, _Z = 0, _RX = 0, _RY = 0, _RZ = 0,
            _R1 = 0, _R2 = 0, _R3 = 0, _R4 = 0, _R5 = 0, _R6 = 0;
        int _programID = 0, _commandID = 0, _pointID = 0;

        bool _isConnected = false;

        public KawasakiRobot()
        {

        }
        public void ShowForm()
        {
            new RobotForm(this).ShowDialog();
        }
        public bool ReadPose(out HPose hPose)
        {
            //var read = robot.ReadFanucData();
            if (_isExist)
            {
                double x = _X / 1000;
                double y = _Y / 1000;
                double z = _Z / 1000;
                double rx = _RX;
                double ry = _RY;
                double rz = _RZ;
                hPose = new HPose(x, y, z, rx, ry, rz, "Rp+T", "abg", "point");


                string info = serializationInfo(_X, _Y, _Z, _RZ, _RY, _RX, 1, "11");
                robot.Send(info + "\r\n");
            }
            else
            {
                hPose = null;
            }
            return _isExist;
        }
        public bool ReadAngle(out HTuple hAngle)
        {
            if (_isExist)
            {
                hAngle = new HTuple(_R1, _R2, _R3, _R4, _R5, _R6);
            }
            else
            {
                hAngle = null;
            }
            return _isExist;
        }

        public bool Load()
        {
            bool result = true;
            string basePath = AppDomain.CurrentDomain.BaseDirectory + $"Data\\Robot\\{robotName}\\";
            try
            {
                string paramPath = basePath + "KawasakiParam.xml";
                if (File.Exists(paramPath))
                {
                    XmlSerializer xml = new XmlSerializer(param.GetType());
                    using (FileStream stream = new FileStream(paramPath, FileMode.OpenOrCreate))
                    {
                        RobotParam _ = (RobotParam)xml.Deserialize(stream);
                        if (_ != null)
                        {
                            param = _;
                        }
                        else
                        {
                            _errMsg = paramPath + Resources.LanguageDic.file_format_error;
                            result = false;
                        }
                    }
                }
                else
                {
                    _errMsg = paramPath + Resources.LanguageDic.file_not_exist;
                    result = false;
                }
            }
            catch (Exception ex)
            {
                result = false;
                _errMsg = ex.ToString();
            }

            try
            {
                string paramPath = basePath + "KawasakiIoParam.xml";
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
                        result = false;
                        _errMsg = paramPath + Resources.LanguageDic.file_format_error;
                    }
                    else
                    {
                        ioDict = ios.ToDictionary(n => { return n.IoName; });
                    }
                }
                else
                {
                    result = false;
                    _errMsg = paramPath + Resources.LanguageDic.file_not_exist;
                }
            }
            catch (Exception ex)
            {
                result = false;
                _errMsg = ex.ToString();
            }
            return result;
        }

        public bool Save()
        {
            bool result = true;
            try
            {
                string basePath = AppDomain.CurrentDomain.BaseDirectory + $"Data\\Robot\\{robotName}\\";
                if (!Directory.Exists(basePath))
                {
                    Directory.CreateDirectory(basePath);
                }

                string OpcParamPath = basePath + "KawasakiParam.xml";
                XmlSerializer xml = new XmlSerializer(param.GetType());
                using (FileStream stream = new FileStream(OpcParamPath, FileMode.Create))
                {
                    xml.Serialize(stream, param);
                }

                List<IoAddress> ios = ioDict.Values.ToList();
                string ioParamPath = basePath + "KawasakiIoParam.xml";
                XmlSerializer ioXml = new XmlSerializer(ios.GetType());
                using (FileStream stream = new FileStream(ioParamPath, FileMode.Create))
                {
                    ioXml.Serialize(stream, ios);
                }
            }
            catch (Exception ex)
            {
                result = false;
                _errMsg = ex.ToString();
            }
            return result;
        }


        void ProcessInfo(string info)
        {
            ///
            /// 信号处理
            ///

            //序列化信息
            float X, Y, Z, RX, RY, RZ, R1, R2, R3, R4, R5, R6;
            int programID, commandID, pointID;

            bool rt = deserializationInfo(info, out X, out Y, out Z, out RX, out RY, out RZ, out R1, out R2, out R3, out R4, out R5, out R6, out programID, out commandID, out pointID);
            // 更新缓存数据
            if (rt)
            {
                _isExist = true;
            }
            else
            {
                _isExist = false;
            }
            _X = X;
            _Y = Y;
            _Z = Z;
            _RX = RX;
            _RY = RY;
            _RZ = RZ;

            _R1 = R1;
            _R2 = R2;
            _R3 = R3;
            _R4 = R4;
            _R5 = R5;
            _R6 = R6;

        }

        public bool isOpen()
        {
            return IsOpen;
        }

        public bool isConnected()
        {
            return true;
        }
        public bool Open()
        {
            robot = new TCP_Client(param.IpAddress, param.Port);
            //绑定委托与事件
            robot.OnDataReceived += ProcessInfo;
            //开始监听
            robot.Connect();

            _isOpen = true;
            return true;
        }
        public bool Open(string ip, int port)
        {
            param.IpAddress = ip;
            param.Port = port;
            return Open();
        }

        public bool Close()
        {
            if (robot != null)
            {
                robot.Disconnect();
            }
            _isOpen = false;
            return true;
        }


        public bool Read(DI eDI, out string value)
        {
            _errMsg = "不支持string";
            value ="";
            return false;
        }

        public bool Read(DO eDO, out string value)
        {
            _errMsg = "不支持string";
            value = "";
            return false;
        }

        public bool Read(DI eDI, out bool value)
        {
            _errMsg = "不支持bool";
            value = false;
            return false;
        }
        public bool Read(DO eDO, out bool value)
        {
            value = true;
            return false;
        }
        private bool Read(string ioName, out bool value)
        {
            value = true;
            return false;
        }

        public bool Read(DI eDI, out ushort value)
        {
            return Read(eDI.ToString(), out value);
        }
        public bool Read(DO eDO, out ushort value)
        {
            return Read(eDO.ToString(), out value);
        }
        private bool Read(string ioName, out ushort value)
        {
            value = 0;
            return false;
        }

        object iolock = new object();
        public bool Write(DO eDO, object value)
        {
            return false;
        }
    }

}
