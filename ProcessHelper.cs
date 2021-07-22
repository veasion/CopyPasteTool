using System;
using System.Text;
using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace CopyPasteTool
{
    public class ProcessHelper
    {
        // SendMessage第二个参数wMsg类型
        public static int WM_CREATE = 0x0001; // 应用程序创建一个窗口
        public static int WM_DESTROY = 0x0002; // 当一个窗口被破坏时发送
        public static int WM_MOVE = 0x0003; // 移动一个窗口
        public static int WM_SIZE = 0x0005; // 改变一个窗口的大小
        public static int WM_ACTIVATE = 0x0006; // 一个窗口被激活或失去激活状态
        public static int WM_SETFOCUS = 0x0007; // 一个窗口获得焦点
        public static int WM_KILLFOCUS = 0x0008; // 一个窗口失去焦点
        public static int WM_ENABLE = 0x000A; // 一个窗口改变成Enable状态
        public static int WM_SETREDRAW = 0x000B; // 设置窗口是否能重画
        public static int WM_SETTEXT = 0x000C; // 应用程序发送此消息来设置一个窗口的文本
        public static int WM_GETTEXT = 0x000D; // 应用程序发送此消息来复制对应窗口的文本到缓冲区（获取输入值*）
        public static int WM_GETTEXTLENGTH = 0x000E; // 得到与一个窗口有关的文本的长度（不包含空字符）
        public static int WM_PAINT = 0x000F; // 要求一个窗口重画自己
        public static int WM_CLOSE = 0x0010; // 当一个窗口或应用程序要关闭时发送一个信号
        public static int WM_QUERYENDSESSION = 0x0011; // 当用户选择结束对话框或程序自己调用ExitWindows函数

        private static Hashtable processWnd = null;
        public delegate bool WNDENUMPROC(IntPtr hwnd, uint lParam);

        static ProcessHelper()
        {
            if (processWnd == null)
            {
                processWnd = new Hashtable();
            }
        }

        #region API

        // 打开进程获取句柄（访问权限[16进制]，是否继承句柄，进程ID）
        [DllImportAttribute("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);
        // 关闭句柄
        [DllImport("kernel32.dll")]
        private static extern void CloseHandle(IntPtr hObject);

        // 读取内存
        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, IntPtr lpBuffer, int nSize, IntPtr lpNumberOfBytesRead);
        // 写入内存
        [DllImportAttribute("kernel32.dll")]
        public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, int[] lpBuffer, int nSize, IntPtr lpNumberOfBytesWritten);

        // 查找窗体句柄
        [DllImport("User32.dll", EntryPoint = "FindWindow")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        // 查找窗口中的子窗口
        [DllImport("User32.dll", EntryPoint = "FindWindowEx")]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpClassName, string lpWindowName);
        // 查找窗口子句柄
        [DllImport("User32.dll", EntryPoint = "FindEx")]
        public static extern IntPtr FindEx(IntPtr hwnd, IntPtr hwndChild, string lpClassName, string lpWindowName);
        // 将窗口指定的句柄消息发送到一个或多个窗口
        [DllImport("user32.dll", EntryPoint = "SendMessageA")]
        private static extern int SendMessage(IntPtr hwnd, int wMsg, int wParam, StringBuilder lParam);

        // 该函数枚举所有屏幕上的顶层窗口，并将窗口句柄传送给应用程序定义的回调函数。回调函数返回false将停止枚举，否则EnumWindows函数继续到所有顶层窗口枚举完为止
        [DllImport("user32.dll", EntryPoint = "EnumWindows", SetLastError = true)]
        public static extern bool EnumWindows(WNDENUMPROC lpEnumFunc, uint lParam);
        // 获得一个指定子窗口的父窗口句柄
        [DllImport("user32.dll", EntryPoint = "GetParent", SetLastError = true)]
        public static extern IntPtr GetParent(IntPtr hWnd);
        // 查找窗口的创建者（线程或进程）
        [DllImport("user32.dll", EntryPoint = "GetWindowThreadProcessId")]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, ref uint lpdwProcessId);
        // 判断窗体是否有效
        [DllImport("user32.dll", EntryPoint = "IsWindow")]
        public static extern bool IsWindow(IntPtr hWnd);
        // 设置最后错误
        [DllImport("kernel32.dll", EntryPoint = "SetLastError")]
        public static extern void SetLastError(uint dwErrCode);

        #endregion

        #region 方法

        /// <summary>
        /// 根据窗口标题获取PID
        /// </summary>
        /// <param name="windowTitle">窗口标题</param>
        /// <returns></returns>
        public static int GetPidByTitle(string windowTitle)
        {
            int rs = 0;
            Process[] arrayProcess = Process.GetProcesses();
            foreach (Process p in arrayProcess)
            {
                if (p.MainWindowTitle.IndexOf(windowTitle) != -1)
                {
                    rs = p.Id;
                    break;
                }
            }
            return rs;
        }

        /// <summary>
        /// 根据进程名获取PID
        /// </summary>
        /// <param name="processName">进程名字</param>
        /// <returns></returns>
        public static int GetPidByProcessName(string processName)
        {
            Process[] arrayProcess = Process.GetProcessesByName(processName);
            foreach (Process p in arrayProcess)
            {
                return p.Id;
            }
            return 0;
        }

        /// <summary>
        /// 根据PID获取主窗口句柄
        /// </summary>
        /// <param name="pid">进程ID</param>
        /// <returns></returns>
        public static IntPtr GetWindow(uint pid)
        {
            IntPtr ptrWnd = IntPtr.Zero;
            if (pid == 0)
            {
                // 当前进程 ID
                pid = (uint)Process.GetCurrentProcess().Id;
            }
            object objWnd = processWnd[pid];
            if (objWnd != null)
            {
                ptrWnd = (IntPtr)objWnd;
                // 从缓存中获取句柄
                if (ptrWnd != IntPtr.Zero && IsWindow(ptrWnd))
                {
                    return ptrWnd;
                }
                else
                {
                    ptrWnd = IntPtr.Zero;
                }
            }
            bool bResult = EnumWindows(new WNDENUMPROC(EnumWindowsProc), pid);
            // 枚举窗口返回 false 并且没有错误号时表明获取成功
            if (!bResult && Marshal.GetLastWin32Error() == 0)
            {
                objWnd = processWnd[pid];
                if (objWnd != null)
                {
                    ptrWnd = (IntPtr)objWnd;
                }
            }
            // Console.WriteLine(Marshal.PtrToStringAnsi(ptrWnd));
            return ptrWnd;
        }

        private static bool EnumWindowsProc(IntPtr hwnd, uint lParam)
        {
            uint uiPid = 0;
            if (GetParent(hwnd) == IntPtr.Zero)
            {
                GetWindowThreadProcessId(hwnd, ref uiPid);
                // 找到进程对应的主窗口句柄
                if (uiPid == lParam)
                {
                    // 把句柄缓存起来
                    processWnd[uiPid] = hwnd;
                    // 设置无错误
                    SetLastError(0);
                    // 返回 false 以终止枚举窗口
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 获取句柄文本信息
        /// </summary>
        /// <param name="hwnd">窗口的句柄</param>
        /// <returns></returns>
        public static string GetWindowTextData(IntPtr hwnd)
        {
            const int bufferSize = 1024;
            StringBuilder buffer = new StringBuilder(bufferSize);
            SendMessage(hwnd, WM_GETTEXT, bufferSize, buffer);
            return buffer.ToString();
        }

        /// <summary>
        /// 读取内存中的值
        /// </summary>
        /// <param name="baseAddress">地址</param>
        /// <param name="pid">进程ID</param>
        /// <returns></returns>
        public static int ReadMemoryValue(IntPtr baseAddress, int pid)
        {
            try
            {
                var buffer = new byte[4];
                //获取缓冲区地址
                IntPtr byteAddress = Marshal.UnsafeAddrOfPinnedArrayElement(buffer, 0);
                IntPtr hProcess = OpenProcess(0x1F0FFF, false, pid);
                // 将制定内存中的值读入缓冲区
                ReadProcessMemory(hProcess, baseAddress, byteAddress, 4, IntPtr.Zero);
                CloseHandle(hProcess);
                return Marshal.ReadInt32(byteAddress);
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// 将值写入指定内存地址中
        /// </summary>
        /// <param name="baseAddress">地址</param>
        /// <param name="pid">进程ID</param>
        /// <param name="value">值</param>
        public static void WriteMemoryValue(IntPtr baseAddress, int pid, int value)
        {
            IntPtr hProcess = OpenProcess(0x1F0FFF, false, pid);// 0x1F0FFF 最高权限
            WriteProcessMemory(hProcess, baseAddress, new[] { value }, 4, IntPtr.Zero);
            CloseHandle(hProcess);
        }

        #endregion
    }
}
