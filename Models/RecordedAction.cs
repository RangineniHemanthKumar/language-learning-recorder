using System;

namespace LanguageLearningRecorder.Models;

public class RecordedAction
{
    public ActionType Type { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public string? Key { get; set; }
    public MouseButton Button { get; set; }
    public int DelayMs { get; set; }
    public DateTime Timestamp { get; set; }
}
