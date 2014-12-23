using Haaya.GuardClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace GuardClient_Win
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var form1 = new Form1();
            ServiceImp.win = form1;
            Application.Run(form1);
        }
    }
}
