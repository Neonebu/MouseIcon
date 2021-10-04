using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MouseIcon
{
    static class Program
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private static LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;
        static Form1 form;
        static bool lControl = false, lShift = false;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            _hookID = SetHook(_proc);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            form = new Form1();
            Application.Run(form);
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

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                Console.WriteLine((Keys)vkCode);
                if((Keys)vkCode == Keys.LShiftKey)
                {
                    lShift = true;
                }
                if((Keys)vkCode == Keys.LControlKey)
                {
                    lControl = true;
                }
                if ((Keys)vkCode == Keys.S && lShift && lControl)
                {
                    Console.WriteLine("burda");
                    form.check = false;
                    form.sfd = new SaveFileDialog();
                    form.sfd.Title = "Save Icon";
                    form.sfd.Filter = "PNG Image|*.png";
                    form.sfd.ShowDialog();
                    if (form.sfd.FileName != "")
                    {
                        // Saves the Image via a FileStream created by the OpenFile method.
                        System.IO.FileStream fs = (System.IO.FileStream)form.sfd.OpenFile();
                        form.pictureBox1.Image.Save(fs, System.Drawing.Imaging.ImageFormat.Png);
                        fs.Close();

                    }
                    form.check = true;
                    lShift = false;
                    lControl = false;
                }
            }

            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

    }
}
