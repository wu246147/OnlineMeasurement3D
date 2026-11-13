using HslCommunication;
using HslCommunication.Profinet.Melsec;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace OnlineMeasurement.IO
{
    public class MelsecPlc : IHsl
    {
        PlcParam param = new PlcParam();
        
        //Dictionary<string, IoAddress> ioDict = new Dictionary<string, IoAddress>();

        MelsecMcNet plc = new MelsecMcNet();
        bool _isOpen = false;
        string _errMsg = string.Empty;
        public string ErrMsg => _errMsg;
        public bool IsOpen => _isOpen;
        public PlcParam Param { get => param; set => param = value; }
        //public Dictionary<string, IoAddress> IoDict { get => ioDict; set => ioDict = value; }

        public MelsecPlc()
        {
           
            param.Port = 6000;
            param.DataFormat = plc.ByteTransform.DataFormat;
            param.IsStringReverseByteWord = plc.ByteTransform.IsStringReverseByteWord;
        }

        public bool Load()
        {
            bool result = true;
            string basePath = AppDomain.CurrentDomain.BaseDirectory + "Data\\";
            try
            {
                string paramPath = basePath + "PlcParam.xml";
                if (File.Exists(paramPath))
                {
                    XmlSerializer xml = new XmlSerializer(param.GetType());
                    using (FileStream stream = new FileStream(paramPath, FileMode.OpenOrCreate))
                    {
                        PlcParam _ = (PlcParam)xml.Deserialize(stream);
                        if (_ != null)
                        {
                            param = _;
                        }
                        else
                        {
                            result = false;
                            _errMsg = $"{paramPath} {Resources.LanguageDic.file_format_error}";
                        }
                    }
                }
                else
                {
                    result = false;
                    _errMsg = $"{paramPath} {Resources.LanguageDic.file_not_exist}";
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
                string basePath = AppDomain.CurrentDomain.BaseDirectory + "Data\\";
                if (!Directory.Exists(basePath))
                {
                    Directory.CreateDirectory(basePath);
                }

                string OpcParamPath = basePath + "PlcParam.xml";
                XmlSerializer xml = new XmlSerializer(param.GetType());
                using (FileStream stream = new FileStream(OpcParamPath, FileMode.Create))
                {
                    xml.Serialize(stream, param);
                }

            }
            catch (Exception ex) { result = false; _errMsg = ex.ToString(); }
            return result;
        }

        /// <summary>
        /// 将当前参数应用到 PLC 通信对象并建立长连接，成功时设置已打开状态，
        /// 失败时将底层结果的错误码和错误信息保存在 ErrMsg，供主流程和调试窗口记录。
        /// </summary>
        /// <returns>本次连接成功返回 true，失败返回 false；失败原因从 ErrMsg 获取。</returns>
        /// <remarks>
        /// 同步连接的超时沿用 2000 毫秒设置，参数须在调用前配置完成。
        /// 会修改通信配置和连接状态，调用方负责在使用结束时调用 Close，并协调连接操作与运行线程。
        /// 未转换为通信结果的底层异常沿用原有传播方式。
        /// </remarks>
        public bool Open()
        {
            plc.ConnectTimeOut = 2000;
            plc.ByteTransform.DataFormat = param.DataFormat;
            plc.ByteTransform.IsStringReverseByteWord = param.IsStringReverseByteWord;

            plc.IpAddress = param.IpAddress;
            plc.Port = param.Port;

            var result = plc.ConnectServer();
            if (result.IsSuccess)
            {
                _isOpen = true;
                return true;
            }
            else
            {
                _errMsg = $"错误码：{result.ErrorCode}，错误信息：{result.Message}";
                return false;
            }
        }
        public void Close()
        {
            plc.ConnectClose();
            _isOpen = false;
        }


        public OperateResult<bool> ReadBool(string eDO)
        {
            return plc.ReadBool(eDO);
        }
        public OperateResult<ushort> ReadUInt16(string eDI)
        {
            return plc.ReadUInt16(eDI);
        }
        public OperateResult<short> ReadInt16(string eDI)
        {
            return plc.ReadInt16(eDI);
        }
        public OperateResult<uint> ReadUInt32(string eDI)
        {
            return plc.ReadUInt32(eDI);
        }
        public OperateResult<int> ReadInt32(string eDI)
        {
            return plc.ReadInt32(eDI);
        }

        public OperateResult<float> ReadFloat(string eDI)
        {
            return plc.ReadFloat(eDI);
        }
        public OperateResult<string> ReadString(string eDI, ushort length)
        {
            return plc.ReadString(eDI, length);
        }


        /// <summary>
        /// 写入 PLC 布尔信号；D 寄存器的位地址先读取整字、修改目标位，再延时 2 毫秒写回，
        /// 其他地址直接交给底层布尔写入；读取整字失败时保留原始错误码和错误信息返回上层。
        /// </summary>
        /// <param name="eDO">目标地址，不能为 null；D 位地址使用 D寄存器地址.位号 格式，位号按十六进制解析，调用方应提供 0 至 F 的有效位。</param>
        /// <param name="value">目标位的新值；true 表示置位，false 表示复位。</param>
        /// <returns>底层写入结果；若预读整字失败，则返回包含该次读取错误码和错误信息的失败结果，不继续写入。</returns>
        /// <exception cref="FormatException">D 位地址中的位号无法转换为十六进制整数。</exception>
        /// <exception cref="OverflowException">D 位地址中的位号超出 Int32 可表示的范围。</exception>
        /// <remarks>
        /// 会同步读取并修改 PLC 数据，不创建或关闭连接。
        /// 读整字再写回未在本方法内加锁，调用方仍需协调并发修改同一寄存器的操作。
        /// </remarks>
        public OperateResult Write(string eDO, bool value)
        {
            //D寄存器不支持位写入
            string address = eDO;
            if (address.Contains('.'))//存在位地址
            {
                if (address.ToUpper().Contains("D"))
                {
                    string[] strings = address.Split('.');
                    if (strings.Length == 2)
                    {
                        string io = strings[0];
                        int index = Convert.ToInt32(strings[1], 16);
                        var read = plc.ReadUInt16(io);
                        if (read.IsSuccess)
                        {
                            ushort num;
                            if (value)
                            {
                                num = (ushort)(read.Content | (1 << index));
                            }
                            else
                            {
                                num = (ushort)(read.Content & (ushort.MaxValue - (1 << index)));
                            }
                            Thread.Sleep(2);
                            return plc.Write(io, num);
                        }
                        else
                        {
                            var re = new OperateResult();
                            re.IsSuccess = false;
                            // 保留读取整字失败的原始错误码，供上层写入失败日志定位通信问题。
                            re.ErrorCode = read.ErrorCode;
                            re.Message = read.Message;
                            return re;
                        }
                    }
                }
            }
            return plc.Write(eDO, value);
        }
        public OperateResult Write(string eDO, ushort value)
        {
            return plc.Write(eDO, value);
        }
        public OperateResult Write(string eDO, short value)
        {
            return plc.Write(eDO, value);
        }
        public OperateResult Write(string eDO, uint value)
        {
            return plc.Write(eDO, value);
        }
        public OperateResult Write(string eDO, int value)
        {
            return plc.Write(eDO, value);
        }
        public OperateResult Write(string eDO, int[] value)
        {
            return plc.Write(eDO, value);
        }
        public OperateResult Write(string eDO, float value)
        {
            return plc.Write(eDO, value);
        }
        public OperateResult Write(string eDO, string value)
        {
            return plc.Write(eDO, value);
        }


        public void ShowForm()
        {
            new HslForm(this, false).ShowDialog();
        }
    }
}
