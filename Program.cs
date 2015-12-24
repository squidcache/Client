using FreeSquidClient;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace SquidClient
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
            //http://stackoverflow.com/questions/6486195/ensuring-only-one-application-instance
            bool result;
            var mutex = new System.Threading.Mutex(true, "PAC", out result);

            if (!result)
            {
                MessageBox.Show("客户端已经运行!");
                return;
            }

            Application.Run(new Form1());

            GC.KeepAlive(mutex);
           
        }
    }
}
