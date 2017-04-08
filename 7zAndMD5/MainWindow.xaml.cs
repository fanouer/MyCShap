using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Security.Cryptography;
using SevenZip;
using System.IO;
using System.Xml;
using System.Runtime.InteropServices;


namespace ModifyMD5
{

    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public static string m_Dir;//压缩文件夹的路径
        public String errorKey;//错误值
        public string configTxtPatch;//生成的config.txt路径
        IList<MyScreen> screenList = new List<MyScreen>();

        [System.Runtime.InteropServices.DllImport("code_txt.dll", EntryPoint = "TXT_ENCODE", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool TXT_ENCODE([MarshalAs(UnmanagedType.LPStr)] string fn, [MarshalAs(UnmanagedType.LPStr)] string ttt);

        [System.Runtime.InteropServices.DllImport("code_txt.dll", EntryPoint = "TXT_DECODE", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool TXT_DECODE([MarshalAs(UnmanagedType.LPStr)] string fn, [MarshalAs(UnmanagedType.LPStr)] string ttt);	
        
        [System.Runtime.InteropServices.DllImport("code_txt.dll", EntryPoint = "Dec", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int Dec([MarshalAs(UnmanagedType.LPStr)] string fn, [MarshalAs(UnmanagedType.LPStr)] string ttt);
        //[DllImport("code_txt.dll", CharSet = CharSet.Unicode)]
        //static extern  bool    Loads(char[] name);
        //[DllImport("code_txt.dll", CharSet = CharSet.Unicode)]
        //static extern  bool    Saves();

        public MainWindow()
        {

            InitializeComponent();
            BindCombobox();
 
            
        }
        //打开按钮
        private void Button_Click(object sender, RoutedEventArgs e)
        {

            FolderBrowserDialog dialog = new FolderBrowserDialog();

            DialogResult result = dialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.Cancel)
            {
                return;
            }
            m_Dir = dialog.SelectedPath.Trim();
            this.openOne.Text = m_Dir;

        }
        //点击“生成”按钮
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Util util = new Util();
            m_Dir = this.openOne.Text;
            ModifyMD5.MainWindow.Util.CompressFiles();
            this.openB.Text = m_Dir + "\\log.xml";

        }

        public class Util
        {
            //调用7z.dll
            static Util()
            {
                string[] _7z = Directory.GetFiles(System.Windows.Forms.Application.StartupPath, "7z.dll");
                SevenZipCompressor.SetLibraryPath(_7z[0]);
            }
            //转压缩
            public static void CompressFiles()
            {
                if (File.Exists(m_Dir + "\\log.xml"))
                {
                    try
                    {
                        DeleteSignedFile(m_Dir, "7z");
                        File.Delete(m_Dir + "\\log.xml");
                    }
                    catch
                    {
                        System.Windows.MessageBox.Show("log.xml文件存在且删除失败");
                    }
                }
                try
                {
                    String fileName;//文件名（不包含路径和后缀）
                    SevenZipCompressor tmp = new SevenZipCompressor();//创建压缩对象
                    tmp.ArchiveFormat = OutArchiveFormat.SevenZip;//指定压缩样式
                    var files = Directory.GetFiles(m_Dir, "*.*");
                    //创建txt文件
                    FileStream fs = new FileStream(m_Dir + "\\log.txt", FileMode.Create);
                    StreamWriter sw = new StreamWriter(fs, Encoding.GetEncoding("GB2312"));

                    sw.WriteLine("<?xml version=\"1.0\" encoding=\"GB2312\"?>");
                    sw.WriteLine("<BitmapLists>");

                    for (int i = 0; i < files.Length; i++)
                    {
                        Console.WriteLine(files[i]);
                        fileName = files[i].Substring(files[i].LastIndexOf("\\") + 1);
                        fileName = getSubStr(files[i], 2);
                        //fileName = fileName.Replace("\\", "/");//若是只保存文件名放开，此为上一级目录
                        String MidFileNamePath = files[i].Substring(0, files[i].LastIndexOf("."));
                        String _7zFileNamePath = files[i] + ".7z";  //保存为7z的文件名+路径
                        Console.WriteLine(_7zFileNamePath);
                        tmp.CompressionMethod = CompressionMethod.Default;   // '压缩模式
                        tmp.CompressionLevel = CompressionLevel.Ultra;    //' 压缩等级 超高压缩

                        tmp.CompressFiles(_7zFileNamePath, files[i]);//压缩//当前文件的路径
                         
                        String md5Index = ModifyMD5.MainWindow.GetMD5HashFromFile(files[i]);//获取文件的md5值
                        Console.WriteLine(md5Index);
                        //修改写入格式
                        sw.WriteLine("<Address name=\"/" + fileName + ".7z\"" + "\t" + "md5=\"" + md5Index + "\" />",false, Encoding.GetEncoding("ASCII"));
                        Console.WriteLine("<Address name=\"/" + fileName + ".7z\"" + "\t" + "md5=\"" + md5Index + "\" />");

                    }
                    sw.WriteLine("</BitmapLists>");
                    sw.Close();
                    //txt文件更改为.xml
                    //String txtName=System.IO.Path.GetFileName(m_Dir + "\\log.txt");
                    string xmlName = System.IO.Path.ChangeExtension(m_Dir + "\\log.txt", ".xml");
                    File.Move(m_Dir + "\\log.txt", xmlName);
                    System.Windows.MessageBox.Show("生成完成！");
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(ex.Message);
                }
            }
        }
        //B点击“打开文件 ”按钮
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            //用于测试方便
            //this.openB.Text = @"C:\Users\ff\Desktop\新建文件夹 (2)\log";
            //this.openA.Text = @"C:\Users\ff\Desktop\新建文件夹 (2)\Res.xml";
            try
            {
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.InitialDirectory = @"D:\";
                dialog.Filter = "可扩展标记语言|*.xml";
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    this.openB.Text = dialog.FileName;

                }

            }
            catch (Exception ex)
            {

                System.Windows.MessageBox.Show(ex.Message);
            }
        }
        //A点击“打开文件”按钮
        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.InitialDirectory = @"D:\";
                dialog.Filter = "可扩展标记语言|*.xml";
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    this.openA.Text = dialog.FileName;
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }

        //删除某路径下指定类型的文件//传入路径,传入后缀名
        public static void DeleteSignedFile(string path, string suffixName)
        {

            DirectoryInfo di = new DirectoryInfo(path);
            foreach (FileInfo fi in di.GetFiles())
            {
                string exname = fi.Name.Substring(fi.Name.LastIndexOf(".") + 1);//得到后缀名
                if (exname == suffixName)
                {
                    File.Delete(path + "\\" + fi.Name);//删除当前文件

                }
            }
        }

        //路径中的截取
        private static String getSubStr(String str, int num)
        {
            String result = "";
            int i = 0;
            try
            {
                while (i < num)
                {
                    int lastFirst = str.LastIndexOf('\\');
                    result = str.Substring(lastFirst) + result;
                    str = str.Substring(0, lastFirst);
                    i++;
                }
            }
            catch (Exception ex)
            {
                String errorInfo = "B文件位于根目录下无法规范书写xml:" + "\n" + ex.Message + "\n";
                System.Windows.MessageBox.Show(errorInfo);
                //throw new Exception("GetMD5HashFromFile() fail,error:" + ex.Message);
            }

            return result.Substring(1);
        }
        //获取md5值 fileName传入的文件名（含路径及后缀名）
        private static string GetMD5HashFromFile(string fileName)
        {
            try
            {
                FileStream file = new FileStream(fileName, System.IO.FileMode.Open);
                MD5 md5 = new MD5CryptoServiceProvider();
                byte[] retVal = md5.ComputeHash(file);
                file.Close();
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb.Append(retVal[i].ToString("x2"));
                }
                return sb.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception("GetMD5HashFromFile() fail,error:" + ex.Message);
            }
        }
        //修改源xml文件
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            try
            {
                String nodeName = "Address";
                String addressName = "name";
                String md5 = "md5";
                Boolean flag = false;
                if (this.openB.Text == "" || this.openA.Text == "")
                {
                    System.Windows.MessageBox.Show("请输入需要对比的文件B和修改的文件A");
                    return;
                }
                String aimPath = this.openA.Text.Substring(0, this.openA.Text.LastIndexOf("\\")) + "\\xml源文件和更改报告存档";
                //创建源文件存档
                CopyDir(this.openA.Text, aimPath);
                if (!System.IO.Directory.Exists(aimPath))
                {
                    System.IO.Directory.CreateDirectory(aimPath);
                }
                else
                {
                    if (!System.IO.File.Exists(aimPath + "\\Report.xml"))
                    {
                        //创建修改报告
                        FileStream fs = new FileStream(aimPath + "\\Report.txt", FileMode.OpenOrCreate);
                        StreamWriter sw = new StreamWriter(fs);

                        sw.WriteLine("<?xml version='1.0' encoding='GB2312'?>");
                        sw.WriteLine("<BitmapLists>");
                        sw.WriteLine("<AlreadyModify>");
                        sw.WriteLine("</AlreadyModify>");
                        sw.WriteLine("<AddRange>");
                        sw.WriteLine("</AddRange>");
                        sw.WriteLine("<TheSame>");
                        sw.WriteLine("</TheSame>");
                        sw.WriteLine("</BitmapLists>");
                        sw.Close();
                        //txt文件更改为.xml

                        string xmlName = System.IO.Path.ChangeExtension(aimPath + "\\Report.txt", ".xml");
                        File.Move(aimPath + "\\Report.txt", xmlName);
                    }
                }
                //判断选择的是.xml还是其他文件
                FileInfo fi = new FileInfo(this.openB.Text);
                if (fi.Attributes == FileAttributes.Directory)
                {
                    String md5Index;
                    Dictionary<string, string> othDictionaryB = new Dictionary<string, string>();
                    DirectoryInfo theFolder = new DirectoryInfo(this.openB.Text + "\\");
                    FileInfo[] thefileInfo = theFolder.GetFiles("*.*", SearchOption.TopDirectoryOnly);
                    //遍历文件
                    foreach (FileInfo NextFile in thefileInfo)
                    {
                        md5Index = ModifyMD5.MainWindow.GetMD5HashFromFile(this.openB.Text + "\\" + NextFile.Name);
                        othDictionaryB.Add(NextFile.Name, md5Index);
                    }

                    DirectoryInfo[] dirInfo = theFolder.GetDirectories();
                    //遍历子文件夹
                    foreach (DirectoryInfo NextFolder in dirInfo)
                    {
                        FileInfo[] fileInfo = NextFolder.GetFiles("*.*", SearchOption.AllDirectories);
                        foreach (FileInfo NextFile in fileInfo)
                        {
                            md5Index = ModifyMD5.MainWindow.GetMD5HashFromFile(this.openB.Text + "\\" + NextFolder + "\\" + NextFile.Name);
                            othDictionaryB.Add(NextFile.Name, md5Index);
                        }

                    }
                    foreach (KeyValuePair<string, string> kvpB in othDictionaryB)
                    {
                        Console.WriteLine("Key = {0}, Value = {1}", kvpB.Key, kvpB.Value);
                    }
                    //String Falsemd5Index = ModifyMD5.MainWindow.GetMD5HashFromFile(this.openB.Text)+"1";//测试修改md5

                    List<string> othRetListA1 = GetAttribute(this.openA.Text, nodeName, addressName);
                    List<string> othRetListA2 = GetAttribute(this.openA.Text, nodeName, md5);

                    Dictionary<string, string> othDictionaryA = new Dictionary<string, string>();
                    for (int a = 0; a < othRetListA1.LongCount(); a++)
                    {
                        errorKey = othRetListA1[a];
                        othRetListA1[a] = othRetListA1[a].Substring(othRetListA1[a].LastIndexOf("/") + 1);
                        othDictionaryA.Add(othRetListA1[a], othRetListA2[a]);
                    }
                    foreach (KeyValuePair<string, string> kvpB in othDictionaryB)
                    {
                        flag = false;
                        foreach (KeyValuePair<string, string> kvpA in othDictionaryA)
                        {

                            if (kvpB.Key == kvpA.Key)
                            {
                                flag = true;
                                if (kvpB.Value == kvpA.Value)
                                {
                                    AddReportXml(kvpB.Key, kvpB.Value, 0);
                                    break;
                                }
                                else
                                {
                                    //Console.WriteLine("++++++++++++++++++++++++");
                                    ModifyXml(kvpA.Value, kvpB.Value);
                                    AddReportXml(kvpB.Key, kvpB.Value, 1);
                                }

                            }
                        }
                        if (!flag)
                        {
                            //Console.WriteLine("_______________________");
                            AddContentXml(kvpB.Key, kvpB.Value);
                            AddReportXml(kvpB.Key, kvpB.Value, 2);
                            flag = false;
                        }



                    }
                    System.Windows.MessageBox.Show("源文件已备份！" + "\n" + "报告已完成！" + "\n" + "修改已完成！");
                    return;
                }

                List<string> retListB1 = GetAttribute(this.openB.Text, nodeName, addressName);
                List<string> retListB2 = GetAttribute(this.openB.Text, nodeName, md5);
                List<string> retListA1 = GetAttribute(this.openA.Text, nodeName, addressName);
                List<string> retListA2 = GetAttribute(this.openA.Text, nodeName, md5);

                Dictionary<string, string> dictionaryB = new Dictionary<string, string>();
                Dictionary<string, string> dictionaryA = new Dictionary<string, string>();



                for (int b = 0; b < retListB1.LongCount(); b++)
                {
                    retListB1[b] = retListB1[b].Substring(retListB1[b].LastIndexOf("/") + 1);
                    dictionaryB.Add(retListB1[b], retListB2[b]);
                }

                foreach (KeyValuePair<string, string> kvpB in dictionaryB)
                {
                    Console.WriteLine("Key = {0}, Value = {1}", kvpB.Key, kvpB.Value);
                }


                for (int a = 0; a < retListA1.LongCount(); a++)
                {
                    errorKey = retListA1[a];
                    retListA1[a] = retListA1[a].Substring(retListA1[a].LastIndexOf("/") + 1);
                    dictionaryA.Add(retListA1[a], retListA2[a]);
                }
                foreach (KeyValuePair<string, string> kvpA in dictionaryA)
                {
                    Console.WriteLine("Key = {0}, Value = {1}", kvpA.Key, kvpA.Value);
                }
                foreach (KeyValuePair<string, string> kvpB in dictionaryB)
                {
                    flag = false;
                    foreach (KeyValuePair<string, string> kvpA in dictionaryA)
                    {
                        if (kvpB.Key == kvpA.Key)
                        {
                            flag = true;
                            if (kvpB.Value == kvpA.Value)
                            {
                                AddReportXml(kvpB.Key, kvpB.Value, 0);
                                break;
                            }
                            else
                            {
                                //Console.WriteLine("++++++++++++++++++++++++");
                                ModifyXml(kvpA.Value, kvpB.Value);
                                AddReportXml(kvpB.Key, kvpB.Value, 1);
                            }

                        }
                    }
                    if (!flag)
                    {
                        //Console.WriteLine("_______________________");
                        AddContentXml(kvpB.Key, kvpB.Value);
                        AddReportXml(kvpB.Key, kvpB.Value, 2);
                        flag = false;

                    }
                }
                System.Windows.MessageBox.Show("源文件已备份！" + "\n" + "报告已完成！" + "\n" + "修改已完成！");
            }
            catch (System.Exception ex)
            {
                String errorInfo = "错误:" + "\n" + ex.Message;
                System.Windows.MessageBox.Show(errorInfo);
                //throw new Exception("GetMD5HashFromFile() fail,error:" + ex.Message);
                return;
            }
        }
        //写入修改报告xml中
        public void AddReportXml(String name, String md5, int index)
        {
            XmlDocument xx = new XmlDocument();
            String reportPath = this.openA.Text.Substring(0, this.openA.Text.LastIndexOf("\\")) + "\\xml源文件和更改报告存档";
            reportPath = reportPath + "\\Report.xml";
            xx.Load(reportPath);
            XmlNode root = xx.SelectSingleNode("BitmapLists");
            XmlNode childroot = null;

            if (index == 1)
            {
                childroot = root.SelectSingleNode("AlreadyModify");  //查找<BitmapLists>节点
            }
            else if (index == 0)
            {
                childroot = root.SelectSingleNode("TheSame");  //查找<BitmapLists>节点
            }
            else if (index == 2)
            {
                childroot = root.SelectSingleNode("AddRange");  //查找<BitmapLists>节点
            }
            XmlElement xe = xx.CreateElement("Address");
            //string chineesName = System.Text.Encoding.GetEncoding("GB2312").GetString(System.Text.Encoding.UTF8.GetBytes(name));
            xe.SetAttribute("name", name);                         //设置该节点的name属性
            xe.SetAttribute("md5", md5);                           //设置该节点的md5属性
            childroot.AppendChild(xe);                                  //把Address添加到<BitmapLists>根节点中
            xx.Save(reportPath);
        }
        //追加源xml内容
        public void AddContentXml(String name, String md5)
        {
            XmlDocument xx = new XmlDocument();
            xx.Load(this.openA.Text);
            XmlNode root = xx.SelectSingleNode("BitmapLists");  //查找<BitmapLists>节点
            XmlElement xe1 = xx.CreateElement("Address");       //创建一个<Address>节点
            xe1.SetAttribute("name", name);                         //设置该节点的name属性
            xe1.SetAttribute("md5", md5);                           //设置该节点的md5属性
            root.AppendChild(xe1);                                  //把Address添加到<BitmapLists>根节点中
            xx.Save(this.openA.Text);
        }
        //修改源xml的节点的属性值
        public void ModifyXml(String kvpAKey, String kvpBKey)
        {
            XmlDocument xx = new XmlDocument();
            xx.Load(this.openA.Text);
            XmlNodeList theElementList = xx.DocumentElement.GetElementsByTagName("Address");
            foreach (XmlNode xn in theElementList)                          //遍历xml下的子节点
            {
                XmlElement xe = (XmlElement)xn;                 //将子节点类型转换为XmlElement类型  
                if (xe.GetAttribute("md5") == kvpAKey)
                {
                    xe.SetAttribute("md5", kvpBKey);
                    break;
                }
            }
            xx.Save(this.openA.Text);
        }
        //将xml节点属性值保存到集合
        //nodeName是节点名,attributeName是节点的属性名
        public List<string> GetAttribute(string xmlFile, string nodeName, string attributeName)
        {
            List<string> retList = new List<string>();
            XmlDocument xx = new XmlDocument();
            xx.Load(xmlFile);
            XmlNodeList xxList = xx.GetElementsByTagName(nodeName);
            foreach (XmlNode xxNode in xxList)
            {
                retList.Add(xxNode.Attributes[attributeName].Value);
            }
            return retList;
        }
        private void CopyDir(string srcPath, string aimPath)
        {

            // 检查目标目录是否以目录分割字符结束如果不是则添加 
            if (aimPath[aimPath.Length - 1] != System.IO.Path.DirectorySeparatorChar)
            {
                aimPath += System.IO.Path.DirectorySeparatorChar;
            }
            // 判断目标目录是否存在如果不存在则新建
            if (!System.IO.Directory.Exists(aimPath))
            {
                System.IO.Directory.CreateDirectory(aimPath);
            }
            // 得到源目录的文件列表，该里面是包含文件以及目录路径的一个数组 
            // 如果你指向copy目标文件下面的文件而不包含目录请使用下面的方法 
            string fileName = srcPath.Substring(srcPath.LastIndexOf("\\") + 1);
            fileName = fileName.Substring(0, fileName.LastIndexOf("."));
            string dest = aimPath;
            DateTime dt = DateTime.Now;
            String day = string.Format("{0:yyyyMMddHHmmssffff}", dt);
            dest += fileName;
            dest += day;
            dest += ".xml";
            try
            {
                File.Copy(srcPath, dest, false);
            }
            catch
            {
                System.Windows.MessageBox.Show("已经有同名文件/n不能覆盖！");
            }

        }
        //打开目录 按钮
        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();

            DialogResult result = dialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.Cancel)
            {
                return;
            }
            m_Dir = dialog.SelectedPath.Trim();
            this.openB.Text = m_Dir;
        }
        //生成加密Config.txt
        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            try
            {
                configTxtPatch = System.Environment.CurrentDirectory;
                configTxtPatch = configTxtPatch + "\\config.txt";
                List<string> txt = new List<string>();
                txt.Add("colorbits=32");
                if (screen.SelectedIndex == -1)
                {
                    System.Windows.MessageBox.Show("分辨率未选择");
                    return;
                }
                int tt = screen.SelectedIndex;
                String name = screenList[tt].Name;
                txt.Add("screenwidth=" + name.Substring(0, name.IndexOf("×")));
                txt.Add("screenheight=" + name.Substring(name.IndexOf("×") + 1));
                //复选框
                if (window.IsChecked == true)
                    txt.Add("window=1");
                else
                    txt.Add("window=0");
                if (vertsync.IsChecked == true)
                    txt.Add("vertsync=1");
                else
                    txt.Add("vertsync=0");

                txt.Add("lightmap=0");

                if (weather.IsChecked == true)
                    txt.Add("weather=1");
                else
                    txt.Add("weather=0");

                txt.Add("hardwarecursor=1");

                if (sound.IsChecked == true)
                    txt.Add("sound=1");
                else
                    txt.Add("sound=0");
                if (scenesound.IsChecked == true)
                    txt.Add("scenesound=1");
                else
                    txt.Add("scenesound=0");
                if (circumstancesound.IsChecked == true)
                    txt.Add("circumstancesound=1");
                else
                    txt.Add("circumstancesound=0");
                if (backgroundsound.IsChecked == true)
                    txt.Add("backgroundsound=1");
                else
                    txt.Add("backgroundsound=0");
                //滑动块
                txt.Add("soundvolume=" + textSlider1.Text);
                txt.Add("scenevolume=" + textSlider2.Text);
                txt.Add("circumstancevolume=" + textSlider3.Text);
                txt.Add("backvolume=" + textSlider4.Text);
                //文本框
                if (textBox1.Text == "")
                {
                    System.Windows.MessageBox.Show("服务器ip未填写");
                    return;
                }
                txt.Add("loginaddress=" + textBox1.Text);
                if (textBox2.Text == "")
                {
                    System.Windows.MessageBox.Show("服务器名字未填写");
                    return;
                }
                txt.Add("servername=" + textBox2.Text);
                if (textBox3.Text == "")
                {
                    System.Windows.MessageBox.Show("服务器端口未填写");
                    return;
                }
                txt.Add("loginport=" + textBox3.Text);
                txt.Add("zone=1");
                txt.Add("player=");
                txt.Add("ManageCode=1");
                try
                {

                    if (textBox4.Text.Substring(textBox4.Text.Length - 1, 1) != "-")
                    {

                    }
                }
                catch(Exception ex)
                {
                    System.Windows.MessageBox.Show("资源下载地址未填写");
                    Console.WriteLine(ex);
                    return;
                }
                String k = textBox4.Text.Substring(textBox4.Text.Length - 1, 1);
                char[] kChar = k.ToCharArray();
                if ((int)kChar[0] !=45) 
                {
                    System.Windows.MessageBox.Show("资源下载地址必须为-结尾");
                    return;
                }
                txt.Add("downurl=" + textBox4.Text);
                String textTotal = null;
                for (int i = 0; i < txt.Count; i++)
                {
                    textTotal += (txt[i] + "\n");

                }
                //调用外部dll的方法
                if (TXT_ENCODE(configTxtPatch, textTotal) != true)
                {
                    System.Windows.MessageBox.Show("未生成加密失败");
                    return;
                }
                System.Windows.MessageBox.Show("生成并加密成功");

                //Console.WriteLine(textTotal);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }
         //解密
        private void Button_Click_8(object sender, RoutedEventArgs e)
        {
            try
            {
                configTxtPatch = System.Environment.CurrentDirectory;
                configTxtPatch = configTxtPatch + "\\config.txt";
                if (!File.Exists(configTxtPatch))
                {
                    System.Windows.MessageBox.Show("未生成config.txt");
                    return;
                }
                //调用外部dll的方法
                TXT_DECODE(configTxtPatch, System.Environment.CurrentDirectory + "\\Kconfig.txt");
                System.Windows.MessageBox.Show("解密完成");
            }
             catch(Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }
        //打开加密或解密文件路径
        private void Button_Click_7(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(System.Environment.CurrentDirectory+"\\config.txt"))
            {
                // 目录/文件不存在，建立目录/文件 
                System.Windows.MessageBox.Show("未生成config.txt");
                return;
            }
            System.Diagnostics.Process.Start("explorer.exe", System.Environment.CurrentDirectory);
        }
        //屏幕分辨率选项卡
        public void BindCombobox()
        {

            screenList.Add(new MyScreen { Name = "1280×1024"});
            screenList.Add(new MyScreen { Name = "1280×960" });
            screenList.Add(new MyScreen { Name = "1280×800" });
            screenList.Add(new MyScreen { Name = "1280×768" });
            screenList.Add(new MyScreen { Name = "1280×720" });
            screenList.Add(new MyScreen { Name = "1152×864" });
            screenList.Add(new MyScreen { Name = "1024×768" });
            screenList.Add(new MyScreen { Name = "800×600" });
            screen.ItemsSource = screenList;
            screen.DisplayMemberPath = "Name";
            //screen.SelectedValue = 1;
        }
        public class MyScreen
        {
           public string Name{get;set;}
        }
        //音效，音量滑动块
        private void silder1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //index = silder1.Value.ToString().Substring(0, silder1.Value.ToString().IndexOf("."));
            int value = (int)silder1.Value;
            textSlider1.Text = value.ToString(); 
        }
        //音效，场景音量滑动块
        private void silder2_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int value = (int)silder2.Value;
            textSlider2.Text = value.ToString(); 
        }
        //音效，环境音量滑动块
        private void silder3_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int value = (int)silder3.Value;
            textSlider3.Text = value.ToString(); 
        }
        //音效，背景音量滑动块
        private void silder4_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int value = (int)silder4.Value;
            textSlider4.Text = value.ToString(); 
        }
        //分辨率下拉框
        private void screen_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
        //打开A目录
        private void Button_Click_9(object sender, RoutedEventArgs e)
        {
            try
            {
                String pathA = this.openA.Text.Substring(0, this.openA.Text.LastIndexOf("\\"));
                System.Diagnostics.Process.Start("explorer.exe", pathA);
            }
            catch(Exception ex)
            {
                System.Windows.MessageBox.Show("未填写源文件路径");
                Console.WriteLine(ex);
            }
        }
        //解压缩
        private void Button_Click_10(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.InitialDirectory = @"D:\";
                dialog.Filter = "7z压缩|*.7z";
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    this.openB.Text = dialog.FileName;
                    Console.WriteLine(this.openB.Text);
                    String filePath1=getSubStr(this.openB.Text,1);      //log.xml
                    String filePath2 = filePath1.Substring(0,filePath1.LastIndexOf("."));
                    String filePath3=this.openB.Text.Substring(0,this.openB.Text.LastIndexOf("\\")+1);

                    if (Directory.Exists(filePath3+"解压缩") == false)//如果不存在就创建file文件夹
                    {
                        Directory.CreateDirectory(filePath3 + "解压缩");
                    }

                    //Directory.Delete(filePath3 + "解压缩", true);//删除文件夹以及文件夹中的子目录，文件    

                    //判断文件的存在

                    if (File.Exists(filePath3 + "解压缩\\" + filePath2))
                    {
                        File.Delete(filePath3 + "解压缩\\" + filePath2);

                    }

                    String filePath = filePath3 +"解压缩\\"+ filePath2;
                    int result=Dec(this.openB.Text,filePath);
                    if (result == 0)
                    {
                        System.Windows.MessageBox.Show("解压完成");
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("解压失败");
                    }
                }


            }
            catch (Exception ex)
            {

                System.Windows.MessageBox.Show(ex.Message);
            }
        }

        private void textBox1_TextChanged(object sender, TextChangedEventArgs e)
        {
           
                if (textBox1.Text.Length == 10)
                {
                    System.Windows.MessageBox.Show("ok");
                
                    
            }

           
        }
    }
}

            
        
        
   
 



