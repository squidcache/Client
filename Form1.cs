using System.Windows.Forms;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Net;
using System.IO;
using System.Threading;
using System.Xml;
using System.Reflection;
using System.ComponentModel;
using System.Net.NetworkInformation;


namespace SquidClient
{
    public partial class Form1 : Form
    {

        [DllImport("wininet.dll")]
        public static extern bool InternetSetOption(IntPtr hInternet, int dwOption, IntPtr lpBuffer, int dwBufferLength);
        public const int INTERNET_OPTION_SETTINGS_CHANGED = 39;
        public const int INTERNET_OPTION_REFRESH = 37;
        static bool _settingsReturn, _refreshReturn;
        string ClientVersion = SquidClient.Properties.Resources.myStringWebResource;
        string ClientDL = Properties.Resources.xmlURL;
        public static void NotifyIE()
        {
            // These lines implement the Interface in the beginning of program 
            // They cause the OS to refresh the settings, causing IP to realy update
            _settingsReturn = InternetSetOption(IntPtr.Zero, INTERNET_OPTION_SETTINGS_CHANGED, IntPtr.Zero, 0);
            _refreshReturn = InternetSetOption(IntPtr.Zero, INTERNET_OPTION_REFRESH, IntPtr.Zero, 0);
        }

        public Form1()
        {
            InitializeComponent();
        }
        private void MarkStartup()
        {

            string path = Application.ExecutablePath;
            RegistryKey runKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
            string[] keyValueNames = runKey.GetValueNames();

            foreach (string keyValueName in keyValueNames)

            {

                try
                {
                    if (keyValueName == "GGA")

                    {
                        随系统启动ToolStripMenuItem.Checked = true;
                        // toolStripMenuItem2.Text = "取消随系统启动";
                        //  runKey.Close();
                        // runKey.DeleteValue("GGA");
                    }


                    else if (keyValueName != "GGA")

                    {
                        随系统启动ToolStripMenuItem.Checked = false;
                        //  toolStripMenuItem2.Text = "随系统启动";
                        //  runKey.Close();
                        //  runKey.SetValue("GGA", path);

                    }
                }



                catch (Exception e)
                {
                    MessageBox.Show(e.Message.ToString(), "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);

                }
            }
            runKey.Close();
        }



        private void SetSquidProxy()
        {
            RegistryKey loca = Registry.CurrentUser;
            RegistryKey run = loca.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Internet Settings");
            //  Software\Microsoft\\Windows\CurrentVersion\Internet Settings\Connections",


            try

            {
                string pacUrl;
                pacUrl = "http://106.186.19.201/ZhongShanPlanning.pac";
                run.SetValue("ProxyEnable", 0);
                run.SetValue("ProxyServer", "");
                run.SetValue("AutoConfigURL", pacUrl);
                NotifyIE();
                //Must Notify IE first, or the connections do not chanage

            }

            catch (Exception ee)

            {
                MessageBox.Show(ee.Message.ToString(), "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            CopyProxySettingFromLan();

        }

        private void DeleteLANSettting()
        {
            RegistryKey loca = Registry.CurrentUser;
            RegistryKey run = loca.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Internet Settings");
            //  Software\Microsoft\\Windows\CurrentVersion\Internet Settings\Connections",



            string[] keyValueNames = run.GetValueNames();

            // run.Close();

            //bool result = false;

            foreach (string keyValueName in keyValueNames)

            {

                if (keyValueName == "ProxyServer")

                {
                    run.DeleteValue("ProxyServer");
                    // result = true;

                    break;

                }

                else

                {
                    Console.WriteLine("Test");

                }
                if (keyValueName == "AutoConfigURL")

                {
                    run.DeleteValue("AutoConfigURL");
                    break;

                }

                else

                {
                    Console.WriteLine("Test");
                }

            }


            try

            {
                run.SetValue("ProxyEnable", 0);
                loca.Close();
            }

            catch (Exception ee)

            {
                MessageBox.Show(ee.Message.ToString(), "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void CheckUpdate(String xmlURL, string myStringWebResource)

        {

            // version info from xml file  
            Version newVersion = null;
            // and in this variable we will put the url we  
            // would like to open so that the user can  
            // download the new version  
            // it can be a homepage or a direct  
            // link to zip/exe file  
            string url = "";
            XmlTextReader reader = null;
            try
            {
                // provide the XmlTextReader with the URL of  
                // our xml document  
                // string xmlURL = "";
                reader = new XmlTextReader(xmlURL);
                // simply (and easily) skip the junk at the beginning  
                reader.MoveToContent();
                // internal - as the XmlTextReader moves only  
                // forward, we save current xml element name  
                // in elementName variable. When we parse a  
                // text node, we refer to elementName to check  
                // what was the node name  
                string elementName = "";
                // we check if the xml starts with a proper  
                // "ourfancyapp" element node  
                if ((reader.NodeType == XmlNodeType.Element) &&
                    (reader.Name == "SquidAPP"))
                {
                    while (reader.Read())
                    {
                        // when we find an element node,  
                        // we remember its name  
                        if (reader.NodeType == XmlNodeType.Element)
                            elementName = reader.Name;
                        else
                        {
                            // for text nodes...  
                            if ((reader.NodeType == XmlNodeType.Text) &&
                                (reader.HasValue))
                            {
                                // we check what the name of the node was  
                                switch (elementName)
                                {
                                    case "version":
                                        // thats why we keep the version info  
                                        // in xxx.xxx.xxx.xxx format  
                                        // the Version class does the  
                                        // parsing for us  
                                        newVersion = new Version(reader.Value);
                                        break;
                                    case "url":
                                        url = reader.Value;
                                        break;

                                }
                            }
                        }
                    }
                }
            }

            catch (Exception XmlReadStatus)

            {
                MessageBox.Show(XmlReadStatus.Message.ToString(), "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            finally

            {
                if (reader != null)
                    reader.Close();
            }


            Version curVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            // compare the versions  
            if (curVersion.CompareTo(newVersion) < 0)
            {

                string message = "已经存在一个新的版本" + newVersion + ", 是否更新?";
                string caption = "更新提示";
                DialogResult result;
                MessageBoxButtons buttons = MessageBoxButtons.YesNo;
                //  MessageBox.Show("new version is available");
                result = MessageBox.Show(message, caption, buttons);


                if (result == System.Windows.Forms.DialogResult.Yes)
                {
                    string path = System.Environment.CurrentDirectory;
                    // MessageBox.Show("为你下载新版");
                    string Downloadfilename = "蜂鸟Squid" + newVersion + ".exe";
                    //    string myStringWebResource = "";

                    try

                    {

                        WebClient webClient = new WebClient();
                        webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(Completed);
                        //  webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);
                        webClient.DownloadFileAsync(new Uri(myStringWebResource), @path + "/" + Downloadfilename);

                    }

                    catch (Exception SquidError)
                    {
                        MessageBox.Show(SquidError.Message.ToString(), "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }


                }

                else

                {

                    Console.ReadLine();


                    //     MessageBox.Show(path);
                }


            }
            else

            {
                notifyIcon1.Visible = true;
                notifyIcon1.ShowBalloonTip(3000, "版本提示:", "已经是最新版本", ToolTipIcon.Info);

            }



        }
        private void Completed(object sender, AsyncCompletedEventArgs e)
        {
            // MessageBox.Show("Download completed!");

            notifyIcon1.Visible = true;
            notifyIcon1.ShowBalloonTip(3000, "更新提示:", "下载完成,请重启客户端", ToolTipIcon.Info);
        }


        private void DeleteAdslSettting()
        {

            string starupPath = Application.ExecutablePath;
            //class Micosoft.Win32.RegistryKey. 表示Window注册表中项级节点,此类是注册表装.
            RegistryKey loca1 = Registry.CurrentUser;
            RegistryKey Adslrun = loca1.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Internet Settings\Connections");

            //  RegistryKey subKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Internet Settings");
            string[] keyValueNames = Adslrun.GetValueNames();

            foreach (string keyValueName in keyValueNames)

            {

                try
                {
                    Adslrun.DeleteValue(keyValueName);

                }

                catch (Exception e)
                {
                    MessageBox.Show(e.Message.ToString(), "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

        }

        private static void CopyProxySettingFromLan()
        {
            RegistryKey registry =
                Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Internet Settings\\Connections",
                    true);
            var defaultValue = registry.GetValue("DefaultConnectionSettings");
            try
            {
                var connections = registry.GetValueNames();
                foreach (String each in connections)
                {
                    if (!(each.Equals("DefaultConnectionSettings")
                        || each.Equals("LAN Connection")
                        || each.Equals("SavedLegacySettings")))
                    {
                        //set all the connections's proxy as the lan
                        registry.SetValue(each, defaultValue);
                    }
                }
                NotifyIE();
                //Must Notify IE first, or the connections do not chanage

            }
            catch (IOException e)
            {
                MessageBox.Show(e.Message.ToString(), "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void 随系统启动ToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            CheckUpdate(ClientVersion, ClientDL);
        }

        private void 主力服务器ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetSquidProxy();
            主力服务器ToolStripMenuItem.Checked = true;
        }

        private void 备用服务器ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("由蜂鸟项目Q群单独提供,QQ群:478676422", "项目提示");
        }

        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://plus.google.com/communities/100095559541855774106/");
        }

        private void sourceforgeToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void gToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://plus.google.com/communities/100095559541855774106/");
        }

        private void 退出ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DeleteAdslSettting();
            DeleteLANSettting();
            notifyIcon1.Icon = null;
            notifyIcon1.Dispose();
            Application.DoEvents();
            System.Environment.Exit(0);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
         //   CheckUpdate(ClientVersion, ClientDL);
            SetSquidProxy();
            MarkStartup();
            this.Hide();
            this.ShowInTaskbar = false;
        }
    }
}
