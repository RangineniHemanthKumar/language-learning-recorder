using System;
using System.Collections.Generic;

namespace LanguageLearningRecorder.Models;

public class Session
{
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public List<RecordedAction> Actions { get; set; } = new();
}
