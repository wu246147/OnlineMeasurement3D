using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Management;

namespace OnlineMeasurement
{
    public class LicenseKey
    {
        public bool IsTrue(string productID)
        {
            LoadLicenseKey();
            string id = GetID(GetSN(GetCpuid()), productID);
            foreach (var item in keys)
            {
                if (item == id)
                {
                    return true;
                }
            }
            return false;
        }

        List<string> keys = new List<string>();
        public void LoadLicenseKey()
        {
            keys.Clear();
            if (File.Exists("LicenseKey.dat"))
            {
                using (StreamReader stream = new StreamReader("LicenseKey.dat", Encoding.Default))
                {
                    while (!stream.EndOfStream)
                    {
                        string str = stream.ReadLine().Trim();
                        if (str != null && str != "")
                        {
                            keys.Add(str);
                        }
                    }
                    //stream.Close();
                }
            }
        }
        public void SaveLicenseKey(string id)
        {
            File.AppendAllLines("LicenseKey.dat", new string[] { id }, Encoding.Default);
        }
        public string GetID(string sn, string productID)
        {
            return StrToMD5(sn + productID);
        }

        public string GetSN(string Cpuid)
        {

            return StrToMD5(Cpuid);
        }

        string StrToMD5(string str)
        {
            MD5 md5 = MD5.Create();
            byte[] buffer = Encoding.Default.GetBytes(str);
            //开始加密 返回加密好的字节数组
            byte[] bufferMd5 = md5.ComputeHash(buffer);
            md5.Dispose();
            ////转成字符串
            //string result = Convert.ToBase64String(bufferMd5);//Encoding.Default.GetString(bufferMd5);
            //return result;

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < bufferMd5.Length; i++)
            {
                sb.Append(bufferMd5[i].ToString("x2"));//x:表示将十进制转换为16进制
            }
            return sb.ToString();
        }

        /// <summary>
        /// 获得CPU编号
        /// </summary>
        /// <returns></returns>
        string GetCpuid()
        {
            var cpuid = string.Empty;
            var mc = new ManagementClass("Win32_Processor");
            var moc = mc.GetInstances();
            foreach (var o in moc)
            {
                var mo = (ManagementObject)o;
                cpuid = mo.Properties["ProcessorId"].Value.ToString();
            }
            return cpuid;
        }

        /// <summary>
        /// 获取硬盘序列号
        /// </summary>
        /// <returns></returns>
        string GetDiskSerialNumber()
        {
            //这种模式在插入一个U盘后可能会有不同的结果，如插入我的手机时
            var hDid = string.Empty;
            var mc = new ManagementClass("Win32_DiskDrive");
            var moc = mc.GetInstances();
            foreach (var o in moc)
            {
                var mo = (ManagementObject)o;
                //Dictionary<string, object> list = new Dictionary<string, object>();
                //foreach (var item in mo.Properties)
                //{
                //    list.Add(item.Name, item.Value);
                //}

                hDid = (string)mo.Properties["SerialNumber"].Value;
                //这名话解决有多个物理盘时产生的问题，只取第一个物理硬盘
                break;
            }
            return hDid;
        }
    }
}
