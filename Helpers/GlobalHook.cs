using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using LanguageLearningRecorder.Models;

namespace LanguageLearningRecorder.Helpers;

public class GlobalHook : IDisposable
{
    private const int WH_KEYBOARD_LL = 13;
    private const int WH_MOUSE_LL = 14;
    private const int WM_KEYDOWN = 0x0100;
    private const int WM_LBUTTONDOWN = 0x0201;
    private const int WM_RBUTTONDOWN = 0x0204;
    private const int WM_MBUTTONDOWN = 0x0207;

    private IntPtr _keyboardHookId = IntPtr.Zero;
    private IntPtr _mouseHookId = IntPtr.Zero;
    private LowLevelKeyboardProc? _keyboardProc;
    private LowLevelMouseProc? _mouseProc;

    public event EventHandler<KeyPressedEventArgs>? KeyPressed;
    public event EventHandler<MouseClickedEventArgs>? MouseClicked;

    private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
    private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string? lpModuleName);

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int x;
        public int y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MSLLHOOKSTRUCT
    {
        public POINT pt;
        public uint mouseData;
        public uint flags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    public void StartHook()
    {
        _keyboardProc = KeyboardHookCallback;
        _mouseProc = MouseHookCallback;

        using var curProcess = Process.GetCurrentProcess();
        using var curModule = curProcess.MainModule;
        
        if (curModule != null)
        {
            _keyboardHookId = SetWindowsHookEx(WH_KEYBOARD_LL, _keyboardProc, GetModuleHandle(curModule.ModuleName), 0);
            _mouseHookId = SetWindowsHookEx(WH_MOUSE_LL, _mouseProc, GetModuleHandle(curModule.ModuleName), 0);
        }
    }

    public void StopHook()
    {
        if (_keyboardHookId != IntPtr.Zero)
        {
            UnhookWindowsHookEx(_keyboardHookId);
            _keyboardHookId = IntPtr.Zero;
        }

        if (_mouseHookId != IntPtr.Zero)
        {
            UnhookWindowsHookEx(_mouseHookId);
            _mouseHookId = IntPtr.Zero;
        }
    }

    private IntPtr KeyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
        {
            int vkCode = Marshal.ReadInt32(lParam);
            KeyPressed?.Invoke(this, new KeyPressedEventArgs(vkCode));
        }
        return CallNextHookEx(_keyboardHookId, nCode, wParam, lParam);
    }

    private IntPtr MouseHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0)
        {
            var hookStruct = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);
            MouseButton? button = null;

            if (wParam == (IntPtr)WM_LBUTTONDOWN)
                button = MouseButton.Left;
            else if (wParam == (IntPtr)WM_RBUTTONDOWN)
                button = MouseButton.Right;
            else if (wParam == (IntPtr)WM_MBUTTONDOWN)
                button = MouseButton.Middle;

            if (button.HasValue)
            {
                MouseClicked?.Invoke(this, new MouseClickedEventArgs(hookStruct.pt.x, hookStruct.pt.y, button.Value));
            }
        }
        return CallNextHookEx(_mouseHookId, nCode, wParam, lParam);
    }

    public void Dispose()
    {
        StopHook();
        GC.SuppressFinalize(this);
    }
}

public class KeyPressedEventArgs : EventArgs
{
    public int VirtualKeyCode { get; }

    public KeyPressedEventArgs(int virtualKeyCode)
    {
        VirtualKeyCode = virtualKeyCode;
    }
}

public class MouseClickedEventArgs : EventArgs
{
    public int X { get; }
    public int Y { get; }
    public MouseButton Button { get; }

    public MouseClickedEventArgs(int x, int y, MouseButton button)
    {
        X = x;
        Y = y;
        Button = button;
    }
}
