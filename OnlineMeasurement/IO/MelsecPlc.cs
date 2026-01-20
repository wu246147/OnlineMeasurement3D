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
            if (!HslCommunication.Authorization.SetAuthorizationCode("0293fde5-6e7c-4c76-bacd-e3bdb0ee6187"))
            {
                System.Windows.Forms.MessageBox.Show("active failed");
            }
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
                _errMsg = result.Message;
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
