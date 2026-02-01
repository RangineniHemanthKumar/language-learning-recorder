using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LanguageLearningRecorder.Helpers;
using LanguageLearningRecorder.Models;

namespace LanguageLearningRecorder.Services;

public class PlaybackService
{
    public event EventHandler<int>? ProgressUpdated;

    public async Task PlayAsync(List<RecordedAction> actions, double speed = 1.0, CancellationToken cancellationToken = default)
    {
        if (actions == null || actions.Count == 0)
        {
            Console.WriteLine("❌ No actions to play back.");
            return;
        }

        Console.WriteLine($"\n▶️  Starting playback with {actions.Count} actions at {speed}x speed...");
        
        // Countdown
        for (int i = 3; i > 0; i--)
        {
            Console.WriteLine($"   Starting in {i}...");
            await Task.Delay(1000, cancellationToken);
        }

        Console.WriteLine("   GO!\n");

        for (int i = 0; i < actions.Count; i++)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                Console.WriteLine("\n⏸️  Playback cancelled.");
                return;
            }

            var action = actions[i];

            // Apply speed-adjusted delay
            if (action.DelayMs > 0)
            {
                int adjustedDelay = (int)(action.DelayMs / speed);
                await Task.Delay(adjustedDelay, cancellationToken);
            }

            // Execute the action
            try
            {
                if (action.Type == ActionType.MouseClick)
                {
                    InputSimulator.SimulateMouseClick(action.X, action.Y, action.Button);
                    Console.WriteLine($"[{i + 1}/{actions.Count}] 🖱️  Mouse {action.Button} click at ({action.X}, {action.Y})");
                }
                else if (action.Type == ActionType.KeyPress && !string.IsNullOrEmpty(action.Key))
                {
                    int vkCode = int.Parse(action.Key);
                    InputSimulator.SimulateKeyPress(vkCode);
                    Console.WriteLine($"[{i + 1}/{actions.Count}] ⌨️  Key press: VK {vkCode}");
                }

                ProgressUpdated?.Invoke(this, i + 1);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error executing action {i + 1}: {ex.Message}");
            }
        }

        Console.WriteLine($"\n✅ Playback completed! {actions.Count} actions executed.");
    }
}
