using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;

namespace CopyPasteTool
{

    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {

        KeyboardHook hook;

        private bool open = true;
        private bool isVar = false;
        private const string html = ".html";

        private string otherForText = "";
        public static bool isWeb = false;
        public static string evalResult;
        public static string htmlDir;

        public MainWindow()
        {
            InitializeComponent();
            // 按键钩子
            hook = new KeyboardHook();
            // 钩住键按下
            hook.KeyDownEvent += KeyDownEvent;
            // 钩住键弹起
            hook.KeyPressEvent += KeyPressEvent;
            //安装键盘钩子
            hook.Start();
            // html保存路径
            htmlDir = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            htmlDir = htmlDir.Substring(0, htmlDir.LastIndexOf("\\") + 1) + "webBrowserTemp.html";
            StringBuilder sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine("js/html");
            sb.AppendLine(".html/.js");
            sb.AppendLine("create js/html");
            // 提示
            this.otherText.ToolTip = sb.ToString();
        }

        private void KeyPressEvent(object sender, KeyPressEventArgs e)
        {
            //int i = (int)e.KeyChar;
            //System.Windows.Forms.MessageBox.Show(i.ToString());
        }

        private void KeyDownEvent(object sender, KeyEventArgs e)
        {
            // 判断按下的键（Ctrl + C） 
            if ((int)System.Windows.Forms.Control.ModifierKeys == (int)Keys.Control && e.KeyValue == (int)Keys.C)
            {
                ThreadStart threadStart = new ThreadStart(Handle);
                Thread thread = new Thread(threadStart);
                thread.TrySetApartmentState(ApartmentState.STA);
                thread.IsBackground = true;
                thread.Start();
            }
        }

        private void Handle()
        {
            try
            {
                if (!open)
                {
                    return;
                }
                Thread.Sleep(200);
                string text = System.Windows.Clipboard.GetText();
                if (text == null || text == "")
                {
                    Thread.Sleep(300);
                    text = System.Windows.Clipboard.GetText();
                }
                if (isVar)
                {
                    text = LowerCamelCase(text);
                }
                else
                {
                    text = Other(text);
                }
                System.Windows.Clipboard.SetText(text);
            }
            catch (Exception e)
            {
                Console.WriteLine("发生异常：" + e.Message);
            }
        }

        private String GetSpace(String code)
        {
            return code.Substring(0, code.Length - code.TrimStart().Length);
        }

        private String GetEnum(String code)
        {
            code = code.Trim();
            int index = code.IndexOf("(");
            if (index == -1)
            {
                index = code.IndexOf(",");
            }
            if (index == -1)
            {
                index = code.IndexOf(";");
            }
            if (index != -1)
            {
                return code.Substring(0, index).Trim();
            }
            else
            {
                return code;
            }
        }

        private string LowerCamelCase(string name)
        {
            if (name == null || "".Equals(name.Trim()))
            {
                return null;
            }
            if (name.StartsWith("_"))
            {
                name = name.Substring(1, name.Length - 1);
            }
            if (name.EndsWith("_"))
            {
                name = name.Substring(0, name.Length - 1);
            }
            StringBuilder sb = new StringBuilder();
            int len = name.Length;
            if (len > 3)
            {
                name = name.Replace("-", "_");
            }
            len = name.Length;
            bool u = true;
            for (int i = 0; i < len; i++)
            {
                char c = name[i];
                if ((int)c < 97)
                {
                    if (u)
                    {
                        if (i != 0 && i + 1 < len && (int)name[i + 1] >= 97)
                        {
                            if (name[i + 1] == 's' && i + 1 == len - 1)
                            {
                                sb.Append(c.ToString().ToLower());
                            }
                            else
                            {
                                sb.Append(c);
                            }
                        }
                        else
                        {
                            sb.Append(c.ToString().ToLower());
                        }
                    }
                    else
                    {
                        u = true;
                        sb.Append(c);
                    }
                }
                else
                {
                    u = false;
                    sb.Append(c);
                }
            }
            name = sb.ToString();
            if (name.IndexOf("_") != -1)
            {
                while (name.IndexOf("__") != -1)
                {
                    name = name.Replace("__", "_");
                }
                sb = new StringBuilder();
                len = name.Length;
                for (int i = 0; i < len; i++)
                {
                    char c = name[i];
                    if (i == 0)
                    {
                        sb.Append(c.ToString().ToLower());
                    }
                    else if (c == '_')
                    {
                        if (++i < len)
                        {
                            sb.Append(name[i].ToString().ToUpper());
                        }
                    }
                    else
                    {
                        sb.Append(c);
                    }
                }
            }
            return sb.ToString();
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            hook.Stop();
        }

        private void Radio1_Checked(object sender, RoutedEventArgs e)
        {
            open = true;
            isVar = true;
            if (this.otherText != null)
            {
                this.otherText.Visibility = Visibility.Hidden;
            }
        }

        private void Radio2_Checked(object sender, RoutedEventArgs e)
        {
            open = true;
            isVar = false;
            if (this.otherText != null)
            {
                this.otherText.Visibility = Visibility.Visible;
            }
        }

        private void Radio3_Checked(object sender, RoutedEventArgs e)
        {
            open = false;
        }

        private void OtherText_TextChanged(object sender, TextChangedEventArgs e)
        {
            isWeb = false;
            otherForText = this.otherText.Text.Trim();
            if (!"".Equals(otherForText))
            {
                if (otherForText.EndsWith(html) && System.IO.File.Exists(otherForText))
                {
                    string htmlCode = System.IO.File.ReadAllText(otherForText);
                    CreateHtml(htmlCode);
                    this.webBrowser.Navigate(new Uri(htmlDir));
                    isWeb = true;
                }
                else if (otherForText.StartsWith("http"))
                {
                    this.webBrowser.Navigate(new Uri(otherForText));
                    isWeb = true;
                }
                else if (otherForText.EndsWith(".js") && System.IO.File.Exists(otherForText))
                {
                    string jsCode = System.IO.File.ReadAllText(otherForText);
                    CreateHtmlByJs(jsCode);
                    this.webBrowser.Navigate(new Uri(htmlDir));
                    isWeb = true;
                }
                else if (otherForText.StartsWith("create"))
                {
                    string file = otherForText.Replace("create", "").Trim();
                    if ("js".Equals(file))
                    {
                        file = "temp.js";
                    }
                    else if ("html".Equals(file))
                    {
                        file = "temp.html";
                    }
                    string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\" + file;
                    if (file.EndsWith(".js"))
                    {
                        File.WriteAllText(path, GetJsCode());
                        this.otherText.Text = path;
                        this.otherText.Select(path.Length, 0);
                    }
                    else if (file.EndsWith(".html"))
                    {
                        File.WriteAllText(path, GetHtmlCode());
                        this.otherText.Text = path;
                        this.otherText.Select(path.Length, 0);
                    }
                }
            }
            if ("html".Equals(otherForText))
            {
                System.Windows.Clipboard.SetText(GetHtmlCode());
            }
            else if ("js".Equals(otherForText))
            {
                System.Windows.Clipboard.SetText(GetJsCode());
            }
            // this.otherText.ToolTip = otherForText;
        }

        private string GetHtmlCode()
        {
            return "<html>\r\n<body></body>\r\n<script type=\"text/javascript\">\r\n" + GetJsCode() + "\r\n</script>\r\n</html>";
        }

        private string GetJsCode()
        {
            return "\r\n// 自定义改变文本函数\r\nfunction change(code) {\r\n  return '改变后的: ' + code;\r\n}\r\n";
        }

        private void CreateHtml(string htmlCode)
        {
            StreamWriter sw = new StreamWriter(htmlDir, false, Encoding.GetEncoding("GB2312"));
            sw.WriteLine(htmlCode);
            sw.Close();
        }

        private void CreateHtmlByJs(string jsCode)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<html>\r\n<body></body>\r\n");
            bool hasScriptEl = jsCode.Contains("<script") || jsCode.Contains("text/javascript");
            if (!hasScriptEl)
            {
                sb.Append("<script type=\"text/javascript\">\r\n	");
            }
            sb.Append(jsCode);
            if (!hasScriptEl)
            {
                sb.Append("\r\n</script>");
            }
            sb.Append("\r\n</html>");
            CreateHtml(sb.ToString());
        }

        private delegate void js(string text);

        private void eval(string text)
        {
            try
            {
                object result = this.webBrowser.InvokeScript("change", text);
                evalResult = result != null ? result.ToString() : "";
                Console.WriteLine("执行结果：" + evalResult);
            }
            catch (Exception e)
            {
                Console.WriteLine("发生错误：" + e.Message);
            }
        }

        private string Other(string code)
        {
            string text = code;
            try
            {
                String otherText = otherForText;

                if (otherText == null || "".Equals(otherText.Trim()))
                {
                    return text;
                }
                if (isWeb)
                {
                    this.webBrowser.Dispatcher.Invoke(new js(eval), code);
                    text = evalResult;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("发生异常：" + e.Message);
            }
            return text;
        }
    }
}
