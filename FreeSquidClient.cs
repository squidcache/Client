using System.Windows.Forms;
using Microsoft.Win32;
using System;
using System.Security.Cryptography;
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
//using System.Management;
namespace FreeSquidClient
{
    public partial class Form1 : Form
    {

        [DllImport("wininet.dll")]
        public static extern bool InternetSetOption(IntPtr hInternet, int dwOption, IntPtr lpBuffer, int dwBufferLength);
        public const int INTERNET_OPTION_SETTINGS_CHANGED = 39;
        public const int INTERNET_OPTION_REFRESH = 37;
        static bool _settingsReturn, _refreshReturn;

        string ClientDL = Properties.Resources.myStringWebResource;
        string ClientVersion = Properties.Resources.xmlURL;
        string DynamicEncryption = null;
        string DynamicDecryption = null;

        string FetchDynamicEncryption;
        string FetchWebURL;



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
        /// <summary>
        /// 
        /// </summary>
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
                        toolStripMenuItem2.Checked = true;
                        // toolStripMenuItem2.Text = "取消随系统启动";
                        //  runKey.Close();
                        // runKey.DeleteValue("GGA");
                    }


                    else if (keyValueName != "GGA")

                    {
                        toolStripMenuItem2.Checked = false;
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

        private static void IEAutoDetectProxy(bool set)
        {
            RegistryKey registry =
                Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Internet Settings\\Connections",
                    true);
            byte[] defConnection = (byte[])registry.GetValue("DefaultConnectionSettings");
            byte[] savedLegacySetting = (byte[])registry.GetValue("SavedLegacySettings");
            if (set)
            {
                defConnection[8] = Convert.ToByte(defConnection[8] & 8);
                savedLegacySetting[8] = Convert.ToByte(savedLegacySetting[8] & 8);
            }
            else
            {
                defConnection[8] = Convert.ToByte(defConnection[8] & ~8);
                savedLegacySetting[8] = Convert.ToByte(savedLegacySetting[8] & ~8);
            }
            registry.SetValue("DefaultConnectionSettings", defConnection);
            registry.SetValue("SavedLegacySettings", savedLegacySetting);
        }


        private void SetStartup()
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
                        //  runKey.Close();
                        runKey.DeleteValue("GGA");
                        //  toolStripMenuItem2.Text = "随系统启动";
                        toolStripMenuItem2.Checked = false;


                    }


                    else if (keyValueName != "GGA")

                    {
                        //  runKey.Close();
                        runKey.SetValue("GGA", path);
                        toolStripMenuItem2.Checked = true;
                        //   toolStripMenuItem2.Text = "取消随系统启动";


                    }
                }



                catch (Exception e)
                {
                    MessageBox.Show(e.Message.ToString(), "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);

                }
            }
            runKey.Close();
        }


        private void Completed(object sender, AsyncCompletedEventArgs e)
        {
            // MessageBox.Show("Download completed!");

            notifyIcon1.Visible = true;
            notifyIcon1.ShowBalloonTip(3000, "更新提示:", "下载完成,请重启客户端", ToolTipIcon.Info);
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
            // string url = "";
            XmlTextReader reader = null;
            try
            {
                // provide the XmlTextReader with the URL of  
                // our xml document  
                //    var StrxmlURL = FreeSquidClient.Properties.Resources.xmlURL;
                // MessageBox.Show(StrxmlURL);

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
                                    case "DynamicEncryption":

                                        FetchDynamicEncryption = reader.Value;
                                        break;

                                    case "LoadWebsite":

                                        FetchWebURL = reader.Value;
                                        break;


                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
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
                string caption = "Hummingbird update";
                DialogResult result;
                MessageBoxButtons buttons = MessageBoxButtons.YesNo;
                //  MessageBox.Show("new version is available");
                result = MessageBox.Show(message, caption, buttons);


                if (result == System.Windows.Forms.DialogResult.Yes)
                {
                    string path = System.Environment.CurrentDirectory;
                    // MessageBox.Show("为你下载新版");
                    string Downloadfilename = "巴豆Squid" + newVersion + ".exe";

                    //System.Net.WebClient unreasonably slow 
                    //http://stackoverflow.com/questions/4415443/system-net-webclient-unreasonably-slow

                    WebClient webClient = new WebClient();
                    webClient.Proxy = null;
                    ServicePointManager.DefaultConnectionLimit = 25;
                    webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(Completed);

                    //  webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);
                    webClient.DownloadFileAsync(new Uri(myStringWebResource), @path + "/" + Downloadfilename);

                    // MessageBox.Show(myStringWebResource);
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


        private void DeleteAdslSettting()
        {
            try
            {
                string starupPath = Application.ExecutablePath;
                //class Micosoft.Win32.RegistryKey. 表示Window注册表中项级节点,此类是注册表装.
                RegistryKey loca1 = Registry.CurrentUser;
                RegistryKey Adslrun = loca1.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Internet Settings");

                Adslrun.SetValue("ProxyEnable", 0);
                Adslrun.SetValue("ProxyServer", "");
                Adslrun.SetValue("AutoConfigURL", "");


                IEAutoDetectProxy(false);
                NotifyIE();
                //Must Notify IE first, or the connections do not chanage
                CopyProxySettingFromLan();
            }

            catch

            {
                // TODO this should be moved into views
                //   MessageBox.Show(g("Failed to update registry"));
            }


            //  RegistryKey subKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Internet Settings");
            //string[] keyValueNames = Adslrun.GetValueNames();

            //foreach (string keyValueName in keyValueNames)

            //{

            //    try
            //    {
            //        Adslrun.DeleteValue(keyValueName);

            //    }

            //    catch (Exception e)
            //    {
            //        MessageBox.Show(e.Message.ToString(), "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //    }
            //}

        }


        private void SetSquidProxyBackup1()
        {
            MessageBox.Show("后续提供" + "\r\n" + "后续提供", "项目提示");

        }
        private void SetSquidProxy()
        {
            RegistryKey loca = Registry.CurrentUser;
            RegistryKey run = loca.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Internet Settings");
            //  Software\Microsoft\\Windows\CurrentVersion\Internet Settings\Connections",


            try

            {
                string pacUrl;
                pacUrl = DynamicDecryption;
                run.SetValue("ProxyEnable", 0);
                run.SetValue("ProxyServer", "");
                run.SetValue("AutoConfigURL", pacUrl);

                //  MessageBox.Show(pacUrl);
                //Must Notify IE first, or the connections do not chanage
                NotifyIE();
                IEAutoDetectProxy(false);

                CopyProxySettingFromLan();

            }





            catch (Exception ee)

            {
                MessageBox.Show(ee.Message.ToString(), "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }


        }

        //2、DES对字符串加密、解密  
        string MyDESCrypto(string str, byte[] keys, byte[] ivs)
        {
            //加密  
            byte[] strs = Encoding.Unicode.GetBytes(str);


            DESCryptoServiceProvider desc = new DESCryptoServiceProvider();
            MemoryStream mStream = new MemoryStream();

            ICryptoTransform transform = desc.CreateEncryptor(keys, ivs);//加密对象  
            CryptoStream cStream = new CryptoStream(mStream, transform, CryptoStreamMode.Write);
            cStream.Write(strs, 0, strs.Length);
            cStream.FlushFinalBlock();
            return Convert.ToBase64String(mStream.ToArray());
        }
        string MyDESCryptoDe(string str, byte[] keys, byte[] ivs)
        {
            //解密  
            byte[] strs = Convert.FromBase64String(str);

            DESCryptoServiceProvider desc = new DESCryptoServiceProvider();
            MemoryStream mStream = new MemoryStream();

            ICryptoTransform transform = desc.CreateDecryptor(keys, ivs);//解密对象  

            CryptoStream cStream = new CryptoStream(mStream, transform, CryptoStreamMode.Write);
            cStream.Write(strs, 0, strs.Length);
            cStream.FlushFinalBlock();
            return Encoding.Unicode.GetString(mStream.ToArray());
        }

        private void CallMyDES()
        {

            Byte[] key = { 62, 24, 34, 45, 77, 67, 78, 89 };
            Byte[] iv = { 120, 230, 10, 101, 10, 56, 30, 89 };

            DynamicEncryption = FetchDynamicEncryption;

            DynamicDecryption = MyDESCryptoDe(FetchDynamicEncryption, key, iv);


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

        private void Form1_Load(object sender, EventArgs e)
        {
            DeleteAdslSettting();

  

            CheckUpdate(ClientVersion, ClientDL);//fetch  version seria and  squidLatestURL

            CallMyDES(); //Decrption SquidURL info assgin to pac Menu called 

            browser form = new browser();
            form.WebsiteURL= FetchWebURL;
            form.Show();


            // MessageBox.Show(FetchDynamicEncryption);// test funcation  that  fetch a Info success or fail!
            //  MessageBox.Show(DynamicDecryption);// Test  funcation that using webclient  nornally 

            SetSquidProxy();
            MarkStartup();

            CenterToScreen();
            this.TopMost = true;
            this.Hide();
            this.ShowInTaskbar = false;

            var squid = Properties.Resources.Squid;
            richTextBox1.Text = squid;
        }


        private void 退出ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DeleteAdslSettting();
            notifyIcon1.Icon = null;
            notifyIcon1.Dispose();
            Application.DoEvents();
            System.Environment.Exit(0);
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            SetStartup();
        }

        private void 服务器1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetSquidProxy();

        }


        private void toolStripMenuItem3_Click_2(object sender, EventArgs e)
        {

            CheckUpdate(ClientVersion, ClientDL);
        }


        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://plus.google.com/communities/100095559541855774106/");
        }



        private void 备用服务器1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetSquidProxyBackup1();
            // 备用服务器1ToolStripMenuItem.Checked = true;
            //  使用主服务器ToolStripMenuItem.Checked = false;
        }

        private void 使用主服务器ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetSquidProxy();
            //备用服务器1ToolStripMenuItem.Checked = false;
            使用主服务器ToolStripMenuItem.Checked = true;
        }


        private void gToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://plus.google.com/communities/100095559541855774106/");
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("由蜂鸟项目Q群单独提供,QQ群:478676422", "项目提示");
        }

        private void Project_Click(object sender, EventArgs e)
        {

        }

        private void ProjectSourceforge_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://sourceforge.net/projects/china-badou/");
        }

        private void 在线技术支持和反馈ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/squidcache/Client");
        }

        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            this.Visible = true;
            this.WindowState = FormWindowState.Normal;
            this.notifyIcon1.Visible = true;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.WindowState = FormWindowState.Minimized;
        }

        private void richTextBox1_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(e.LinkText); // call default browser 
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Hide();
                this.notifyIcon1.Visible = true;
            }
        }



        private void 备用服务器ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            SetSquidProxyBackup1();
        }

        private void toolStripMenuItem1_Click_1(object sender, EventArgs e)
        {
            browser form = new browser();
            form.WebsiteURL = FetchWebURL;
            form.Show();
        }

        private void 服务器2ToolStripMenuItem_Click(object sender, EventArgs e)
        {

            SetSquidProxy();
        }
    }
}
