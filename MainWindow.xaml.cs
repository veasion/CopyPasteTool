using System;
using System.Text;
using System.Windows;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Forms;
using System.IO;
using Noesis.Javascript;

namespace CopyPasteTool
{

    public partial class MainWindow : Window
    {

        KeyboardHook hook;
        DebugForm debugForm;

        private bool open = true;

        public static bool isScript = false;
        private static bool hookRun = false;
        private static string param2 = null;
        private static string otherForText = "";

        private string currentDir = Directory.GetCurrentDirectory();

        private static string temp_script_code;

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
            StringBuilder sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine("js");
            sb.AppendLine("create js");
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
            isScript = false;
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

            if (otherForText.EndsWith(".js") && File.Exists(otherForText))
            {
                isScript = true;
                string scriptCode = File.ReadAllText(otherForText);
                LoadJavaScript(scriptCode);
                CacheHelper.cacheOtherText(otherForText);
            }
            else if (otherForText.StartsWith("create "))
            {
                string file = otherForText.Replace("create ", "").Trim();
                if ("js".Equals(file))
                {
                    file = "temp.js";
                }
                string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\" + file;
                if (file.EndsWith(".js"))
                {
                    File.WriteAllText(path, HtmlHelper.GetJsCode());
                    this.otherText.Text = path;
                    this.otherText.Select(path.Length, 0);
                }
            }
            else if (otherForText.StartsWith("use ") && (otherForText.EndsWith(".js")))
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

        private void LoadJavaScript(string jsCode)
        {
            if (jsCode != null && !"".Equals(jsCode))
            {
                temp_script_code = jsCode;
            }

            this.combo_params.Items.Clear();
            this.combo_params.SelectedIndex = -1;
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
            param2 = this.combo_params.SelectedItem != null ? this.combo_params.SelectedItem.ToString() : null;
            if ("NULL".Equals(param2))
            {
                param2 = null;
            }
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
            if (!isScript)
            {
                return param;
            }
            object result = invokeJsMethod("change", param, debug);
            return result != null ? result.ToString() : param;
        }

        public object invokeJsMethod(string method, string param, bool debug)
        {
            try
            {
                using (JavascriptContext context = new JavascriptContext())
                {

                    context.SetParameter("_request", new Func<string, string, string, string, string>
                        ((url, httpMethod, body, contentType) =>
                        {
                            return HttpHelper.request(url, httpMethod, body, contentType);
                        }));

                    context.SetParameter("_param1", param);
                    context.SetParameter("_param2", param2);
                    context.SetParameter("_result", null);


                    string code = HtmlHelper.GetDefaultJs() + temp_script_code + ";_result = " + method + "(_param1, _param2);";
                    context.Run(code);

                    object result = context.GetParameter("_result");
                    Console.WriteLine("执行结果：" + result);
                    return result;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("发生异常：" + e.Message);
                if (debug)
                {
                    return "发生异常：" + e.Message;
                }
                else
                {
                    return null;
                }
            }
        }

    }
}
