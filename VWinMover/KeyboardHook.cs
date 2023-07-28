using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualDesktop
{
    using System;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;

    public class KeyboardHook
    {
        // キーボードフックのイベントデリゲート
        public delegate void HookKeyEventHandler(Keys key);
        public static event HookKeyEventHandler HookKey;
        public static event HookKeyEventHandler KeyUp;

        // キーボードフックのイベントID
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_SYSKEYDOWN = 0x0104;

        private const int WM_KEYUP = 0x0101;

        // フックプロシージャのデリゲート
        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        // フックプロシージャのポインタ
        private static LowLevelKeyboardProc _proc = HookCallback;

        // キーボードフックのハンドル
        private static IntPtr _hookID = IntPtr.Zero;

        // キーボードフックを開始するメソッド
        public static void Hook()
        {
            _hookID = SetHook(_proc);
        }

        // キーボードフックを解除するメソッド
        public static void Unhook()
        {
            UnhookWindowsHookEx(_hookID);
        }

        // フックプロシージャの設定
        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (var curProcess = System.Diagnostics.Process.GetCurrentProcess())
            using (var curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        // フックプロシージャのコールバックメソッド
        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN))
            {
                int vkCode = Marshal.ReadInt32(lParam);
                Keys key = (Keys)vkCode;
                HookKey?.Invoke(key);
            }
            if (nCode >= 0 && (wParam == (IntPtr)WM_KEYUP))
            {
                int vkCode = Marshal.ReadInt32(lParam);
                Keys key = (Keys)vkCode;
                KeyUp?.Invoke(key);
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        // Win32 API関数のインポート
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
