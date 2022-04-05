using System;
using System.Windows;
using System.Windows.Input;
using System.Diagnostics;
using System.Net.Sockets;
using System.Net;
using System.IO;
using Files;
using System.Text.RegularExpressions;

namespace Yuuz12_Frp
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            //设置默认配置文件
            FilesINI ConfigINI = new FilesINI();

            if (File.Exists(INIPath_id))
            {
                //文件存在，无事发生
            }
            else
            {
                //无此文件，新建保存id的ini
                ConfigINI.INIWrite("frp", "id", "0", INIPath_id);// 新建保存id的ini
            }

            string remote_port;
            remote_port = ConfigINI.INIRead("ssh" + ConfigINI.INIRead("frp", "id", INIPath_id), "remote_port", INIPath);
            Port_port.Text = remote_port.ToString();
            ssh.Text = ConfigINI.INIRead("frp", "id", INIPath_id);
            Port_game.Text = ConfigINI.INIRead("ssh" + ConfigINI.INIRead("frp", "id", INIPath_id), "local_port", INIPath);
            if (IpCombo.Items.Count != 0) // 自动选择第一个IP地址
            {
                IpCombo.SelectedIndex = 0;
            }
        }
        private void Open_Web_register(object sender, RoutedEventArgs e)
        {
            Process[] ps = Process.GetProcessesByName("frpc");
            if (ps.Length > 0)
            {
                foreach (Process p in ps)
                p.Kill();
                MessageBox.Show("已关闭映射隧道", "提示");
            }
            else
            {
                MessageBox.Show("进程已不存在", "提示");
            }
        }

        private void TcpClientCheck(object sender, RoutedEventArgs e)
        {
            TcpClient tcp = null;
            try
            {
                string remote_port;
                FilesINI ConfigINI = new FilesINI();
                remote_port = ConfigINI.INIRead("ssh" + ConfigINI.INIRead("frp", "id", INIPath_id), "remote_port", INIPath);
                IPAddress ipa = IPAddress.Parse("");// 搭建好Frp的服务器地址
                IPEndPoint point = new IPEndPoint(ipa, Convert.ToInt32(remote_port));
                

                tcp = new TcpClient();
                tcp.Connect(point);
                MessageBox.Show("端口已被占用，请尝试更换其它端口", "提示");
            }
            catch (Exception ex)
            {
                Start_Game();
            }
            finally
            {
                if (tcp != null)
                {
                    tcp.Close();
                }
            }
        }


        private void Start_Game()
        {
            Process[] ps = Process.GetProcessesByName("frpc");
            Process proc = null;
            if (ps.Length > 0)
            {
                foreach (Process p in ps)
                    MessageBox.Show("进程已存在", "提示");
            }
            else
            {
                try
                {
                    string targetDir1 = AppDomain.CurrentDomain.BaseDirectory; //获取程序目录
                    proc = new Process();
                    proc.StartInfo.WorkingDirectory = targetDir1;
                    proc.StartInfo.FileName = "!start.bat";//bat文件名称
                    proc.StartInfo.Arguments = string.Format("10");//this is argument
                    //proc.StartInfo.CreateNoWindow = true;
                    proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;//设置DOS窗口不显示，经实践可行
                    proc.Start();
                    proc.WaitForExit();
                    MessageBox.Show("已启动映射隧道", "提示");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("启动隧道发生严重错误", "启动失败");
                }
            }
        }
        private void Open_Web_help(object sender, RoutedEventArgs e)//使用文档
        {
            System.Diagnostics.Process.Start("https://yuuz12.top/archives/74.html");
        }

        public string INIPath = Convert.ToString(System.Threading.Thread.GetDomain().BaseDirectory) + "frpc.ini";//获取配置文件路径
        public string INIPath_id = Convert.ToString(System.Threading.Thread.GetDomain().BaseDirectory) + "id.ini";//获取配置文件路径

        public void limitnumber(object sender, TextCompositionEventArgs e) //限制端口输入框只能输入数字
        {
            Regex re = new Regex("[^0-9]+");
            e.Handled = re.IsMatch(e.Text);
        }
        private void deport(object sender, RoutedEventArgs e)//自定义设置地址和端口
        {
            try
            {
                FilesINI ConfigINI = new FilesINI();
                for(int i = 0; i < 1001; i++)// 循环删除0-100的id，没删除的话可能会造成映射失败
                {
                    ConfigINI.EraseSection("ssh" + i, INIPath);// 删除节点
                }
                ConfigINI.INIWrite("frp", "id", ssh.Text, INIPath_id);
                ConfigINI.INIWrite("ssh" + ConfigINI.INIRead("frp", "id", INIPath_id), "type", "tcp", INIPath);// 映射类型
                ConfigINI.INIWrite("ssh" + ConfigINI.INIRead("frp", "id", INIPath_id), "local_ip", "127.0.0.1", INIPath);// 映射ip地址
                ConfigINI.INIWrite("ssh" + ConfigINI.INIRead("frp", "id", INIPath_id), "local_port", Port_game.Text, INIPath);// 本地端口
                ConfigINI.INIWrite("ssh" + ConfigINI.INIRead("frp", "id", INIPath_id), "remote_port", Port_port.Text, INIPath);// 端口
                ConfigINI.INIWrite("ssh" + ConfigINI.INIRead("frp", "id", INIPath_id), "custom_domains", IpCombo.Text, INIPath);// 解析到的域名
                Clipboard.SetText(IpCombo.Text + ":" + Port_port.Text);// 剪贴板复制地址

                MessageBox.Show("地址端口已复制并修改成功，已将映射地址端口设置为：" + IpCombo.Text + ":" + Port_port.Text, "修改成功");
            }
            catch (Exception ex)
            {
                MessageBox.Show("地址端口修改或地址端口复制失败，详细信息：" + ex.Message, "修改失败");
            }
        }
    }
}



namespace Files
{
    class FilesINI
    {
        // 声明INI文件的写操作函数 WritePrivateProfileString()
        [System.Runtime.InteropServices.DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

        // 声明INI文件的读操作函数 GetPrivateProfileString()
        [System.Runtime.InteropServices.DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, System.Text.StringBuilder retVal, int size, string filePath);


        /// 写入INI的方法
        public void INIWrite(string section, string key, string value, string path)
        {
            // section=配置节点名称，key=键名，value=返回键值，path=路径
            WritePrivateProfileString(section, key, value, path);
        }

        // 读取INI的方法
        public string INIRead(string section, string key, string path)
        {
            // 每次从ini中读取多少字节
            System.Text.StringBuilder temp = new System.Text.StringBuilder(255);

            // section=配置节点名称，key=键名，temp=上面，path=路径
            GetPrivateProfileString(section, key, "", temp, 255, path);
            return temp.ToString();

        }

        // 删除配置节点
        public void EraseSection(string section,string Path)
        {
            WritePrivateProfileString(section, null, null, Path);
        }

        //删除一个INI文件
        public void INIDelete(string FilePath)
        {
            File.Delete(FilePath);
        }
    }
}