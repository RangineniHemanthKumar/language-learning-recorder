using System;
using System.Runtime.InteropServices;
using System.Threading;
using LanguageLearningRecorder.Models;

namespace LanguageLearningRecorder.Helpers;

public static class InputSimulator
{
    private const int MOUSEEVENTF_LEFTDOWN = 0x0002;
    private const int MOUSEEVENTF_LEFTUP = 0x0004;
    private const int MOUSEEVENTF_RIGHTDOWN = 0x0008;
    private const int MOUSEEVENTF_RIGHTUP = 0x0010;
    private const int MOUSEEVENTF_MIDDLEDOWN = 0x0020;
    private const int MOUSEEVENTF_MIDDLEUP = 0x0040;
    private const int KEYEVENTF_KEYUP = 0x0002;

    [DllImport("user32.dll")]
    private static extern bool SetCursorPos(int x, int y);

    [DllImport("user32.dll")]
    private static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

    [DllImport("user32.dll")]
    private static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

    public static void SimulateMouseClick(int x, int y, MouseButton button)
    {
        SetCursorPos(x, y);
        Thread.Sleep(50);

        int downFlag, upFlag;
        switch (button)
        {
            case MouseButton.Left:
                downFlag = MOUSEEVENTF_LEFTDOWN;
                upFlag = MOUSEEVENTF_LEFTUP;
                break;
            case MouseButton.Right:
                downFlag = MOUSEEVENTF_RIGHTDOWN;
                upFlag = MOUSEEVENTF_RIGHTUP;
                break;
            case MouseButton.Middle:
                downFlag = MOUSEEVENTF_MIDDLEDOWN;
                upFlag = MOUSEEVENTF_MIDDLEUP;
                break;
            default:
                return;
        }

        mouse_event(downFlag, x, y, 0, 0);
        Thread.Sleep(50);
        mouse_event(upFlag, x, y, 0, 0);
    }

    public static void SimulateKeyPress(int virtualKeyCode)
    {
        keybd_event((byte)virtualKeyCode, 0, 0, 0);
        Thread.Sleep(50);
        keybd_event((byte)virtualKeyCode, 0, KEYEVENTF_KEYUP, 0);
    }
}
