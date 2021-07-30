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
using System.Windows.Threading;

namespace CopyPasteTool
{

    public partial class MainWindow : Window
    {

        KeyboardHook hook;
        DebugForm debugForm;

        private bool open = true;
        private bool isVar = false;

        public static bool isWeb = false;
        private static bool hookRun = false;
        private static string param2 = null;
        private static string otherForText = "";
        public static string evalResult;
        public static string htmlDir;

        private string currentDir = Directory.GetCurrentDirectory();

        public MainWindow()
        {
            InitializeComponent();
            // 按键钩子
            hook = new KeyboardHook();
            // 钩住键按下
            hook.KeyDownEvent += KeyDownEvent;
            // 钩住键弹起
            // hook.KeyPressEvent += KeyPressEvent;
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
            sb.AppendLine("use cv.js");
            // 提示
            this.otherText.ToolTip = sb.ToString();
            this.otherText.Text = CacheHelper.getOtherText();
            debugForm = new DebugForm(this);
        }

        private void KeyPressEvent(object sender, KeyPressEventArgs e)
        {
            // ctrl + c
            //System.Windows.Forms.MessageBox.Show((e.KeyChar == '\u0003'));
        }

        private new void KeyDownEvent(object sender, KeyEventArgs e)
        {
            // 判断按下的键（Ctrl + C） 
            if ((int)System.Windows.Forms.Control.ModifierKeys == (int)Keys.Control && e.KeyValue == (int)Keys.C)
            {
                if (!open || hookRun)
                {
                    return;
                }
                Thread thread = new Thread(new ThreadStart(Handle));
                thread.TrySetApartmentState(ApartmentState.STA);
                thread.IsBackground = true;
                thread.Start();
            }
            else
            {
                hookRun = false;
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
                hookRun = true;
                string text = this.GetClipboardText();
                if (isVar)
                {
                    text = StringHelper.LowerCamelCase(text);
                }
                else
                {
                    text = invokeJsMethod(text, false);
                }
                this.SetClipboardText(text);
            }
            catch (Exception e)
            {
                Console.WriteLine("发生异常：" + e.Message);
            }
            finally
            {
                hookRun = false;
            }
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            hook.Stop();
        }

        private string GetClipboardText()
        {
            try
            {
                string text = System.Windows.Clipboard.GetText();
                if (text == null || "".Equals(text))
                {
                    Thread.Sleep(200);
                    text = System.Windows.Clipboard.GetText();
                }
                return text;
            }
            catch (Exception)
            {
                return GetClipboardText();
            }
        }

        private void SetClipboardText(string text)
        {
            try
            {
                System.Windows.Clipboard.SetText(text);
            }
            catch (Exception)
            {
                SetClipboardText(text);
            }
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
                OtherText_TextChanged(null, null);
            }
        }

        private void Radio3_Checked(object sender, RoutedEventArgs e)
        {
            open = false;
        }

        private void OtherText_TextChanged(object s, TextChangedEventArgs e)
        {
            bool debug = false;
            isWeb = false;
            otherForText = this.otherText.Text.Trim();
            if ("".Equals(otherForText))
            {
                return;
            }

            if (!open && "debug".Equals(otherForText))
            {
                debug = true;
                otherForText = this.otherText.Text = CacheHelper.getOtherText();
            }

            int idx = -1;
            if ((idx = otherForText.IndexOf(".js:")) > 0)
            {
                param2 = otherForText.Substring(idx + 4);
                otherForText = otherForText.Substring(0, idx + 3);
            }
            else if ((idx = otherForText.IndexOf(".html:")) > 0)
            {
                param2 = otherForText.Substring(idx + 6);
                otherForText = otherForText.Substring(0, idx + 5);
            }

            if ((otherForText.EndsWith(".html") || otherForText.EndsWith(".js")) && File.Exists(otherForText))
            {
                string txt = File.ReadAllText(otherForText);
                if (otherForText.EndsWith(".js"))
                {
                    CreateHtmlByJs(txt);
                }
                else
                {
                    CreateHtml(txt);
                }
                this.webBrowser.Navigate(new Uri(htmlDir));
                isWeb = true;
                CacheHelper.cacheOtherText(otherForText);
            }
            else if (otherForText.StartsWith("http"))
            {
                this.webBrowser.Navigate(new Uri(otherForText));
                isWeb = true;
                CacheHelper.cacheOtherText(otherForText);
            }
            else if (otherForText.StartsWith("create "))
            {
                string file = otherForText.Replace("create ", "").Trim();
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
                    File.WriteAllText(path, HtmlHelper.GetJsCode());
                    this.otherText.Text = path;
                    this.otherText.Select(path.Length, 0);
                }
                else if (file.EndsWith(".html"))
                {
                    File.WriteAllText(path, HtmlHelper.GetHtmlCode());
                    this.otherText.Text = path;
                    this.otherText.Select(path.Length, 0);
                }
            }
            else if (otherForText.StartsWith("use ") && (otherForText.EndsWith(".js") || otherForText.EndsWith(".html")))
            {
                string file = otherForText.Replace("use ", "").Trim();
                string path = currentDir + "\\" + file;
                if (!File.Exists(path))
                {
                    path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\" + file;
                    if (!File.Exists(path))
                    {
                        if (file.EndsWith(".js"))
                        {
                            File.WriteAllText(path, HtmlHelper.GetJsCode());
                        }
                        else if (file.EndsWith(".html"))
                        {
                            File.WriteAllText(path, HtmlHelper.GetHtmlCode());
                        }
                    }
                }
                this.otherText.Text = path;
                this.otherText.Select(path.Length, 0);
            }
            if (debug)
            {
                debugForm.ShowAndInit(param2);
            }
        }

        private void CreateHtml(string htmlCode)
        {
            StreamWriter sw = new StreamWriter(htmlDir, false, Encoding.GetEncoding("GB2312"));
            sw.WriteLine(htmlCode);
            sw.Close();
        }

        private void CreateHtmlByJs(string jsCode)
        {
            CreateHtml(HtmlHelper.GetHtmlCode(jsCode));
        }

        private delegate void js(string text);

        private void eval(string text)
        {
            try
            {
                object result = this.webBrowser.InvokeScript("change", text, param2);
                evalResult = result != null ? result.ToString() : "";
                Console.WriteLine("执行结果：" + evalResult);
            }
            catch (Exception e)
            {
                Console.WriteLine("发生错误：" + e.Message);
            }
        }

        public void changeParam2(string p)
        {
            if (p == null)
            {
                OtherText_TextChanged(null, null);
            }
            else
            {
                param2 = p;
            }
        }

        public string invokeJsMethod(string code, bool debug)
        {
            string text = code;
            try
            {
                if (isWeb)
                {
                    this.webBrowser.Dispatcher.Invoke(new js(eval), code);
                    text = evalResult;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("发生异常：" + e.Message);
                if (debug)
                {
                    return "发生异常：" + e.Message;
                }
            }
            return text;
        }

    }
}
