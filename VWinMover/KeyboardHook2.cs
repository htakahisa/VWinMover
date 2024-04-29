using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Diagnostics;

namespace VWinMover
{
    
        public class KeyboardHook2
        {
            private const int WH_KEYBOARD_LL = 13;
            private const int WM_KEYDOWN = 0x0100;
            private const int WM_KEYUP = 0x0101;
            private LowLevelKeyboardProc proc;
            private IntPtr hookId = IntPtr.Zero;

            public event KeyEventHandler KeyDown;
            public event KeyEventHandler KeyUp;

            public KeyboardHook2()
            {
                proc = HookCallback;
                hookId = SetHook(proc);
            }

            ~KeyboardHook2()
            {
                UnhookWindowsHookEx(hookId);
            }

            private IntPtr SetHook(LowLevelKeyboardProc proc)
            {
                using (ProcessModule module = Process.GetCurrentProcess().MainModule)
                {
                    return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(module.ModuleName), 0);
                }
            }

            private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

            private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
            {
                if (nCode >= 0)
                {
                    int vkCode = Marshal.ReadInt32(lParam);
                    Keys key = (Keys)vkCode;
                if (wParam == (IntPtr)WM_KEYDOWN)
                    {

                    if (Control.ModifierKeys == Keys.Control && (key == Keys.Right || key == Keys.Left || key == Keys.Up || key == Keys.Down))
                    {

                        KeyDown?.Invoke(this, new KeyEventArgs(key));
                        return (IntPtr)1;
                    }

                }
                    else if (wParam == (IntPtr)WM_KEYUP)
                    {
                        
                        KeyUp?.Invoke(this, new KeyEventArgs(key));
                    }
                }
                return CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
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
