using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OnlineMeasurement
{
    internal static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            bool processExist = false;

            Process[] processes = Process.GetProcesses();
            Process currentProcess = Process.GetCurrentProcess();
            foreach (Process p in processes)
            {
                if (p.ProcessName == currentProcess.ProcessName && p.Id != currentProcess.Id)
                {
                    processExist = true;
                }
            }

            if (processExist)
            {
                Application.Exit();
                MessageBox.Show($"{Resources.LanguageDic.software_could_open_mul}！", Resources.LanguageDic.repeat, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                if (!HslCommunication.Authorization.SetAuthorizationCode("0293fde5-6e7c-4c76-bacd-e3bdb0ee6187"))
                {
                    MessageBox.Show("active failed");
                }
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new FormRun());
                //Application.Run(new FormSet());
            }
        }
    }
}
