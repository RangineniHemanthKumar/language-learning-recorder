using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using LanguageLearningRecorder.Models;
using LanguageLearningRecorder.Services;

namespace LanguageLearningRecorder;

class Program
{
    private static RecorderService _recorderService = new();
    private static PlaybackService _playbackService = new();
    private static SessionManager _sessionManager = new();
    private static List<RecordedAction> _currentActions = new();
    private static Session? _loadedSession = null;

    static async Task Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        
        ShowWelcome();

        bool running = true;
        while (running)
        {
            ShowMenu();
            var key = Console.ReadKey(true);

            Console.Clear();
            
            switch (key.Key)
            {
                case ConsoleKey.D1:
                case ConsoleKey.NumPad1:
                    await StartRecording();
                    break;

                case ConsoleKey.D2:
                case ConsoleKey.NumPad2:
                    await PlaybackRecording();
                    break;

                case ConsoleKey.D3:
                case ConsoleKey.NumPad3:
                    SaveSession();
                    break;

                case ConsoleKey.D4:
                case ConsoleKey.NumPad4:
                    LoadSession();
                    break;

                case ConsoleKey.D5:
                case ConsoleKey.NumPad5:
                    ListAllSessions();
                    break;

                case ConsoleKey.D6:
                case ConsoleKey.NumPad6:
                    DeleteSession();
                    break;

                case ConsoleKey.D7:
                case ConsoleKey.NumPad7:
                    ShowHelp();
                    break;

                case ConsoleKey.Escape:
                case ConsoleKey.Q:
                    running = false;
                    break;

                default:
                    Console.WriteLine("❌ Invalid option. Please try again.");
                    Thread.Sleep(1000);
                    break;
            }
        }

        Console.WriteLine("\n👋 Thank you for using Language Learning Recorder!");
        _recorderService.Dispose();
    }

    static void ShowWelcome()
    {
        Console.Clear();
        Console.WriteLine("╔══════════════════════════════════════════════════════════╗");
        Console.WriteLine("║                                                          ║");
        Console.WriteLine("║     🎯 LANGUAGE LEARNING RECORDER 🎯                    ║");
        Console.WriteLine("║                                                          ║");
        Console.WriteLine("║     Record and replay your learning interactions         ║");
        Console.WriteLine("║                                                          ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════╝");
        Console.WriteLine();
        Thread.Sleep(1500);
    }

    static void ShowMenu()
    {
        Console.Clear();
        Console.WriteLine("╔══════════════════════════════════════════════════════════╗");
        Console.WriteLine("║              LANGUAGE LEARNING RECORDER                  ║");
        Console.WriteLine("╠══════════════════════════════════════════════════════════╣");
        Console.WriteLine("║                                                          ║");
        Console.WriteLine("║  1. 🔴 Start Recording                                  ║");
        Console.WriteLine("║  2. ▶️  Playback Recording                              ║");
        Console.WriteLine("║  3. 💾 Save Session                                     ║");
        Console.WriteLine("║  4. 📂 Load Session                                     ║");
        Console.WriteLine("║  5. 📋 List All Sessions                                ║");
        Console.WriteLine("║  6. 🗑️  Delete Session                                  ║");
        Console.WriteLine("║  7. ℹ️  Info & Help                                     ║");
        Console.WriteLine("║                                                          ║");
        Console.WriteLine("║  ESC/Q. 🚪 Exit                                         ║");
        Console.WriteLine("║                                                          ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════╝");
        
        if (_currentActions.Count > 0)
        {
            Console.WriteLine($"\n📊 Current Session: {_currentActions.Count} actions recorded");
        }
        
        if (_loadedSession != null)
        {
            Console.WriteLine($"📁 Loaded Session: {_loadedSession.Name} ({_loadedSession.Actions.Count} actions)");
        }

        Console.Write("\n➤ Select option: ");
    }

    static async Task StartRecording()
    {
        Console.WriteLine("╔══════════════════════════════════════════════════════════╗");
        Console.WriteLine("║                    START RECORDING                       ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════╝\n");

        if (_recorderService.IsRecording)
        {
            Console.WriteLine("❌ Already recording!");
            Thread.Sleep(2000);
            return;
        }

        Console.WriteLine("⚠️  NOTE: For best results, run this application as Administrator.\n");
        Console.WriteLine("📝 Recording will capture:");
        Console.WriteLine("   • Mouse clicks (left, right, middle)");
        Console.WriteLine("   • Mouse positions");
        Console.WriteLine("   • Keyboard key presses");
        Console.WriteLine("   • Timing between actions\n");
        
        _recorderService.StartRecording();
        
        // Wait for ESC key to stop
        while (_recorderService.IsRecording)
        {
            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Escape)
                {
                    _currentActions = _recorderService.StopRecording();
                    break;
                }
            }
            await Task.Delay(100);
        }

        Console.WriteLine("\nPress any key to return to menu...");
        Console.ReadKey(true);
    }

    static async Task PlaybackRecording()
    {
        Console.WriteLine("╔══════════════════════════════════════════════════════════╗");
        Console.WriteLine("║                   PLAYBACK RECORDING                     ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════╝\n");

        var actionsToPlay = _loadedSession?.Actions ?? _currentActions;

        if (actionsToPlay.Count == 0)
        {
            Console.WriteLine("❌ No recording available. Please record or load a session first.");
            Console.WriteLine("\nPress any key to return to menu...");
            Console.ReadKey(true);
            return;
        }

        Console.WriteLine($"📊 Ready to play {actionsToPlay.Count} actions\n");
        Console.WriteLine("Select playback speed:");
        Console.WriteLine("  1. 0.5x (Slow)");
        Console.WriteLine("  2. 1.0x (Normal)");
        Console.WriteLine("  3. 2.0x (Fast)");
        Console.WriteLine("  4. 3.0x (Very Fast)");
        Console.Write("\n➤ Select speed: ");

        double speed = 1.0;
        var speedKey = Console.ReadKey(true);
        
        switch (speedKey.KeyChar)
        {
            case '1':
                speed = 0.5;
                break;
            case '2':
                speed = 1.0;
                break;
            case '3':
                speed = 2.0;
                break;
            case '4':
                speed = 3.0;
                break;
            default:
                speed = 1.0;
                break;
        }

        Console.WriteLine($"{speed}x\n");

        try
        {
            await _playbackService.PlayAsync(actionsToPlay, speed);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n❌ Playback error: {ex.Message}");
        }

        Console.WriteLine("\nPress any key to return to menu...");
        Console.ReadKey(true);
    }

    static void SaveSession()
    {
        Console.WriteLine("╔══════════════════════════════════════════════════════════╗");
        Console.WriteLine("║                      SAVE SESSION                        ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════╝\n");

        if (_currentActions.Count == 0)
        {
            Console.WriteLine("❌ No recording to save. Please record something first.");
            Console.WriteLine("\nPress any key to return to menu...");
            Console.ReadKey(true);
            return;
        }

        Console.Write("Enter session name: ");
        string? sessionName = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(sessionName))
        {
            Console.WriteLine("❌ Invalid session name.");
            Console.WriteLine("\nPress any key to return to menu...");
            Console.ReadKey(true);
            return;
        }

        var session = new Session
        {
            Name = sessionName,
            CreatedDate = DateTime.Now,
            Actions = _currentActions
        };

        _sessionManager.SaveSession(session);

        Console.WriteLine("\nPress any key to return to menu...");
        Console.ReadKey(true);
    }

    static void LoadSession()
    {
        Console.WriteLine("╔══════════════════════════════════════════════════════════╗");
        Console.WriteLine("║                      LOAD SESSION                        ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════╝\n");

        var sessionFiles = _sessionManager.GetSessionFiles();

        if (sessionFiles.Count == 0)
        {
            Console.WriteLine("❌ No saved sessions found.");
            Console.WriteLine($"   Sessions directory: {_sessionManager.GetSessionsDirectory()}");
            Console.WriteLine("\nPress any key to return to menu...");
            Console.ReadKey(true);
            return;
        }

        Console.WriteLine("Available sessions:\n");
        for (int i = 0; i < sessionFiles.Count; i++)
        {
            string fileName = Path.GetFileName(sessionFiles[i]);
            FileInfo fileInfo = new FileInfo(sessionFiles[i]);
            Console.WriteLine($"  {i + 1}. {fileName}");
            Console.WriteLine($"     Created: {fileInfo.CreationTime}");
            Console.WriteLine();
        }

        Console.Write($"➤ Select session (1-{sessionFiles.Count}): ");
        string? input = Console.ReadLine();

        if (int.TryParse(input, out int index) && index >= 1 && index <= sessionFiles.Count)
        {
            var session = _sessionManager.LoadSession(sessionFiles[index - 1]);
            if (session != null)
            {
                _loadedSession = session;
                _currentActions = session.Actions;
            }
        }
        else
        {
            Console.WriteLine("❌ Invalid selection.");
        }

        Console.WriteLine("\nPress any key to return to menu...");
        Console.ReadKey(true);
    }

    static void ListAllSessions()
    {
        Console.WriteLine("╔══════════════════════════════════════════════════════════╗");
        Console.WriteLine("║                    ALL SESSIONS                          ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════╝\n");

        var sessionFiles = _sessionManager.GetSessionFiles();

        if (sessionFiles.Count == 0)
        {
            Console.WriteLine("❌ No saved sessions found.");
            Console.WriteLine($"   Sessions directory: {_sessionManager.GetSessionsDirectory()}");
        }
        else
        {
            Console.WriteLine($"Found {sessionFiles.Count} session(s):\n");
            
            for (int i = 0; i < sessionFiles.Count; i++)
            {
                string fileName = Path.GetFileName(sessionFiles[i]);
                FileInfo fileInfo = new FileInfo(sessionFiles[i]);
                long fileSizeKB = fileInfo.Length / 1024;

                Console.WriteLine($"  {i + 1}. {fileName}");
                Console.WriteLine($"     Created: {fileInfo.CreationTime}");
                Console.WriteLine($"     Size: {fileSizeKB} KB");
                Console.WriteLine($"     Path: {sessionFiles[i]}");
                Console.WriteLine();
            }
        }

        Console.WriteLine("\nPress any key to return to menu...");
        Console.ReadKey(true);
    }

    static void DeleteSession()
    {
        Console.WriteLine("╔══════════════════════════════════════════════════════════╗");
        Console.WriteLine("║                    DELETE SESSION                        ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════╝\n");

        var sessionFiles = _sessionManager.GetSessionFiles();

        if (sessionFiles.Count == 0)
        {
            Console.WriteLine("❌ No saved sessions found.");
            Console.WriteLine("\nPress any key to return to menu...");
            Console.ReadKey(true);
            return;
        }

        Console.WriteLine("Available sessions:\n");
        for (int i = 0; i < sessionFiles.Count; i++)
        {
            string fileName = Path.GetFileName(sessionFiles[i]);
            Console.WriteLine($"  {i + 1}. {fileName}");
        }

        Console.Write($"\n➤ Select session to delete (1-{sessionFiles.Count}): ");
        string? input = Console.ReadLine();

        if (int.TryParse(input, out int index) && index >= 1 && index <= sessionFiles.Count)
        {
            string fileName = Path.GetFileName(sessionFiles[index - 1]);
            Console.Write($"\n⚠️  Are you sure you want to delete '{fileName}'? (y/n): ");
            
            var confirmKey = Console.ReadKey(true);
            Console.WriteLine();

            if (confirmKey.Key == ConsoleKey.Y)
            {
                _sessionManager.DeleteSession(sessionFiles[index - 1]);
            }
            else
            {
                Console.WriteLine("❌ Deletion cancelled.");
            }
        }
        else
        {
            Console.WriteLine("❌ Invalid selection.");
        }

        Console.WriteLine("\nPress any key to return to menu...");
        Console.ReadKey(true);
    }

    static void ShowHelp()
    {
        Console.WriteLine("╔══════════════════════════════════════════════════════════╗");
        Console.WriteLine("║                    INFO & HELP                           ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════╝\n");

        Console.WriteLine("📖 ABOUT");
        Console.WriteLine("   Language Learning Recorder helps you automate repetitive");
        Console.WriteLine("   tasks in language learning applications by recording and");
        Console.WriteLine("   replaying your mouse clicks and keyboard inputs.\n");

        Console.WriteLine("🔧 FEATURES");
        Console.WriteLine("   • Record mouse clicks (left, right, middle button)");
        Console.WriteLine("   • Record keyboard key presses");
        Console.WriteLine("   • Capture precise timing between actions");
        Console.WriteLine("   • Save sessions as JSON files");
        Console.WriteLine("   • Load and replay saved sessions");
        Console.WriteLine("   • Adjustable playback speed (0.5x - 3x)");
        Console.WriteLine("   • Session management (save, load, list, delete)\n");

        Console.WriteLine("🎯 HOW TO USE");
        Console.WriteLine("   1. Start Recording - Press 1 and perform your actions");
        Console.WriteLine("   2. Stop Recording - Press ESC when done");
        Console.WriteLine("   3. Save Session - Press 3 to save your recording");
        Console.WriteLine("   4. Playback - Press 2 to replay actions");
        Console.WriteLine("   5. Load Session - Press 4 to load a saved session\n");

        Console.WriteLine("⚙️  SYSTEM REQUIREMENTS");
        Console.WriteLine("   • Windows 10/11");
        Console.WriteLine("   • .NET 8.0 Runtime");
        Console.WriteLine("   • Administrator privileges (recommended)\n");

        Console.WriteLine("⚠️  IMPORTANT NOTES");
        Console.WriteLine("   • Run as Administrator for best hook reliability");
        Console.WriteLine("   • Recording stops when you press ESC");
        Console.WriteLine("   • Sessions are saved to: Documents/LanguageLearningRecorder/Sessions/");
        Console.WriteLine("   • Be careful with playback - it simulates real inputs!\n");

        Console.WriteLine("📝 VERSION");
        Console.WriteLine("   Language Learning Recorder v1.0");
        Console.WriteLine("   Built with .NET 8.0\n");

        Console.WriteLine("\nPress any key to return to menu...");
        Console.ReadKey(true);
    }
}
