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

        public static bool isWeb = false;
        private static bool hookRun = false;
        private static string param2 = null;
        private static string otherForText = "";
        public static string htmlDir;

        private string currentDir = Directory.GetCurrentDirectory();

        public MainWindow()
        {
            InitializeComponent();
            this.webBrowser.LoadCompleted += WebBrowser_LoadCompleted;
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
                text = invokeJsMethod(text, false);
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

        private void RadioCustomize_Checked(object sender, RoutedEventArgs e)
        {
            open = true;
            if (this.otherText != null)
            {
                this.otherText.Visibility = Visibility.Visible;
                OtherText_TextChanged(null, null);
            }
        }

        private void RadioClose_Checked(object sender, RoutedEventArgs e)
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
                changeWebBrowser(htmlDir);
            }
            else if (otherForText.StartsWith("http"))
            {
                changeWebBrowser(otherForText);
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

        private void changeWebBrowser(string urlOrPath)
        {
            this.webBrowser.Navigate(new Uri(urlOrPath));
            isWeb = true;
            CacheHelper.cacheOtherText(otherForText);
        }

        private void WebBrowser_LoadCompleted(object sender, NavigationEventArgs e)
        {
            object result = invokeJsMethod("params", null, false);
            if (result != null)
            {
                string[] items = result.ToString().Split('|');
                if (items.Length == 0)
                {
                    return;
                }
                this.combo_params.Items.Clear();
                this.combo_params.Items.Add("NULL");
                foreach (var item in items)
                {
                    this.combo_params.Items.Add(item);
                }
                this.combo_params.SelectedIndex = 0;
            }
        }

        private void Combo_params_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            param2 = this.combo_params.SelectedItem.ToString();
            if ("NULL".Equals(param2))
            {
                param2 = null;
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

        public string invokeJsMethod(string param, bool debug)
        {
            if (!isWeb)
            {
                return param;
            }
            object result = invokeJsMethod("change", param, debug);
            return result != null ? result.ToString() : param;
        }

        public object invokeJsMethod(string method, string param, bool debug)
        {
            object result = null;
            try
            {
                this.webBrowser.Dispatcher.Invoke(new js((param1) =>
                {
                    result = this.webBrowser.InvokeScript(method, param1, param2);
                    Console.WriteLine("执行结果：" + result);
                }), param);
            }
            catch (Exception e)
            {
                Console.WriteLine("发生异常：" + e.Message);
                if (debug)
                {
                    return "发生异常：" + e.Message;
                }
            }
            return result;
        }

        private void Combo_params_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.combo_params.Items.Clear();
            this.combo_params.SelectedIndex = -1;
            WebBrowser_LoadCompleted(null, null);
        }
    }
}
