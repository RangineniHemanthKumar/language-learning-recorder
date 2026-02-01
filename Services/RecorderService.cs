using System;
using System.Collections.Generic;
using LanguageLearningRecorder.Helpers;
using LanguageLearningRecorder.Models;

namespace LanguageLearningRecorder.Services;

public class RecorderService : IDisposable
{
    private GlobalHook? _hook;
    private List<RecordedAction> _actions = new();
    private DateTime _lastActionTime;
    private readonly object _lockObject = new();
    private bool _isRecording;

    public event EventHandler<int>? ActionCountUpdated;

    public void StartRecording()
    {
        if (_isRecording)
            return;

        lock (_lockObject)
        {
            _actions.Clear();
            _isRecording = true;
            _lastActionTime = DateTime.Now;
        }

        _hook = new GlobalHook();
        _hook.KeyPressed += OnKeyPressed;
        _hook.MouseClicked += OnMouseClicked;
        _hook.StartHook();

        Console.WriteLine("🔴 Recording started... Press ESC to stop.");
    }

    public List<RecordedAction> StopRecording()
    {
        if (!_isRecording)
            return new List<RecordedAction>();

        _isRecording = false;

        if (_hook != null)
        {
            _hook.KeyPressed -= OnKeyPressed;
            _hook.MouseClicked -= OnMouseClicked;
            _hook.StopHook();
            _hook.Dispose();
            _hook = null;
        }

        Console.WriteLine($"\n✅ Recording stopped. {_actions.Count} actions captured.");
        
        lock (_lockObject)
        {
            return new List<RecordedAction>(_actions);
        }
    }

    public List<RecordedAction> GetRecording()
    {
        lock (_lockObject)
        {
            return new List<RecordedAction>(_actions);
        }
    }

    public bool IsRecording => _isRecording;

    private void OnKeyPressed(object? sender, KeyPressedEventArgs e)
    {
        if (!_isRecording)
            return;

        // ESC key to stop recording
        if (e.VirtualKeyCode == 27) // VK_ESCAPE
        {
            return; // Don't record ESC, let it stop recording
        }

        lock (_lockObject)
        {
            var now = DateTime.Now;
            var delay = (int)(now - _lastActionTime).TotalMilliseconds;

            var action = new RecordedAction
            {
                Type = ActionType.KeyPress,
                Key = e.VirtualKeyCode.ToString(),
                DelayMs = delay,
                Timestamp = now
            };

            _actions.Add(action);
            _lastActionTime = now;

            Console.WriteLine($"⌨️  Key pressed: {GetKeyName(e.VirtualKeyCode)} (VK: {e.VirtualKeyCode}) | Actions: {_actions.Count}");
            ActionCountUpdated?.Invoke(this, _actions.Count);
        }
    }

    private void OnMouseClicked(object? sender, MouseClickedEventArgs e)
    {
        if (!_isRecording)
            return;

        lock (_lockObject)
        {
            var now = DateTime.Now;
            var delay = (int)(now - _lastActionTime).TotalMilliseconds;

            var action = new RecordedAction
            {
                Type = ActionType.MouseClick,
                X = e.X,
                Y = e.Y,
                Button = e.Button,
                DelayMs = delay,
                Timestamp = now
            };

            _actions.Add(action);
            _lastActionTime = now;

            string buttonIcon = e.Button switch
            {
                MouseButton.Left => "🖱️ L",
                MouseButton.Right => "🖱️ R",
                MouseButton.Middle => "🖱️ M",
                _ => "🖱️"
            };

            Console.WriteLine($"{buttonIcon} Mouse {e.Button} click at ({e.X}, {e.Y}) | Actions: {_actions.Count}");
            ActionCountUpdated?.Invoke(this, _actions.Count);
        }
    }

    private string GetKeyName(int vkCode)
    {
        return vkCode switch
        {
            8 => "Backspace",
            9 => "Tab",
            13 => "Enter",
            16 => "Shift",
            17 => "Ctrl",
            18 => "Alt",
            20 => "CapsLock",
            27 => "Esc",
            32 => "Space",
            33 => "PageUp",
            34 => "PageDown",
            35 => "End",
            36 => "Home",
            37 => "Left",
            38 => "Up",
            39 => "Right",
            40 => "Down",
            45 => "Insert",
            46 => "Delete",
            _ when vkCode >= 48 && vkCode <= 57 => ((char)vkCode).ToString(), // 0-9
            _ when vkCode >= 65 && vkCode <= 90 => ((char)vkCode).ToString(), // A-Z
            _ when vkCode >= 96 && vkCode <= 105 => $"Numpad{vkCode - 96}", // Numpad 0-9
            112 => "F1",
            113 => "F2",
            114 => "F3",
            115 => "F4",
            116 => "F5",
            117 => "F6",
            118 => "F7",
            119 => "F8",
            120 => "F9",
            121 => "F10",
            122 => "F11",
            123 => "F12",
            _ => $"VK_{vkCode}"
        };
    }

    public void Dispose()
    {
        if (_hook != null)
        {
            _hook.Dispose();
            _hook = null;
        }
        GC.SuppressFinalize(this);
    }
}
