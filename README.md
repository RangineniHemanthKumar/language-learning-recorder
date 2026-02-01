# 🎯 Language Learning Recorder

A powerful C# console application for Windows that records and replays user interactions (mouse clicks and keyboard inputs) to automate repetitive tasks in language learning applications.

## 📋 Features

- **🔴 Recording**: Capture mouse clicks (left, right, middle) and keyboard inputs with precise timing
- **▶️ Playback**: Replay recorded actions with adjustable speed (0.5x, 1x, 2x, 3x)
- **💾 Session Management**: Save, load, list, and delete recording sessions as JSON files
- **🎯 Global Hooks**: Uses Windows API for system-wide input capture
- **⚡ Input Simulation**: Accurate playback using Windows API SendInput/mouse_event/keybd_event
- **🎨 User-Friendly Console UI**: Interactive menu with clear visual feedback and emojis
- **📊 Real-time Feedback**: See actions as they're recorded and played back

## 💻 System Requirements

- **Operating System**: Windows 10 or Windows 11
- **.NET Runtime**: .NET 8.0 SDK or Runtime
- **Privileges**: Administrator privileges recommended for reliable global hooks
- **Platform**: x64 (64-bit)

## 🚀 Quick Start

### Building from Source

1. **Clone the repository**:
   ```bash
   git clone https://github.com/RangineniHemanthKumar/language-learning-recorder.git
   cd language-learning-recorder
   ```

2. **Restore dependencies**:
   ```bash
   dotnet restore
   ```

3. **Build the project**:
   ```bash
   dotnet build
   ```

4. **Run the application**:
   ```bash
   dotnet run
   ```

### Creating a Standalone Executable

To create a self-contained executable that doesn't require .NET runtime installation:

```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

The executable will be in `bin/Release/net8.0/win-x64/publish/`

## 📖 Usage Guide

### Main Menu

When you launch the application, you'll see an interactive menu with the following options:

1. **🔴 Start Recording** - Begin capturing mouse and keyboard actions
2. **▶️ Playback Recording** - Replay recorded or loaded actions
3. **💾 Save Session** - Save current recording to a JSON file
4. **📂 Load Session** - Load a previously saved session
5. **📋 List All Sessions** - View all saved sessions
6. **🗑️ Delete Session** - Remove a saved session
7. **ℹ️ Info & Help** - View help and information
8. **ESC/Q** - Exit the application

### Recording Your Actions

1. Select option **1** from the main menu
2. Perform your mouse clicks and keyboard inputs
3. Press **ESC** to stop recording
4. Your actions are now captured and ready for playback or saving

**What gets recorded:**
- Mouse click positions (X, Y coordinates)
- Mouse button (left, right, middle)
- Keyboard key presses (virtual key codes)
- Timing delays between each action

### Playing Back Actions

1. Select option **2** from the main menu
2. Choose playback speed:
   - **0.5x** - Slow motion (useful for debugging)
   - **1.0x** - Normal speed (original timing)
   - **2.0x** - Fast (2x faster)
   - **3.0x** - Very fast (3x faster)
3. Watch the 3-2-1 countdown
4. Actions will replay automatically

⚠️ **Warning**: Playback simulates real mouse clicks and keyboard inputs. Make sure you're ready before starting!

### Saving Sessions

1. Record some actions or have a loaded session
2. Select option **3** from the main menu
3. Enter a name for your session
4. Session is saved to `Documents/LanguageLearningRecorder/Sessions/`

**File naming format**: `SessionName_YYYYMMDD_HHMMSS.json`

Example: `DuolingoLesson1_20260201_143025.json`

### Loading Sessions

1. Select option **4** from the main menu
2. Choose from the list of available sessions
3. The session is loaded and ready for playback

### Managing Sessions

- **List All Sessions** (option 5): View all saved sessions with details (creation date, size, path)
- **Delete Session** (option 6): Remove unwanted sessions with confirmation prompt

## 🏗️ Project Structure

```
LanguageLearningRecorder/
├── Program.cs                      # Main entry point with interactive menu
├── Models/
│   ├── ActionType.cs              # Enums for action types and mouse buttons
│   ├── RecordedAction.cs          # Data model for recorded actions
│   └── Session.cs                 # Session container with metadata
├── Services/
│   ├── RecorderService.cs         # Recording logic with global hooks
│   ├── PlaybackService.cs         # Playback engine with speed control
│   └── SessionManager.cs          # JSON serialization and file management
├── Helpers/
│   ├── GlobalHook.cs              # Windows API hooks (SetWindowsHookEx)
│   └── InputSimulator.cs          # Input simulation (SendInput, mouse_event)
├── LanguageLearningRecorder.csproj # Project configuration
├── README.md                       # This file
└── .gitignore                      # Git ignore rules
```

## 🔧 Technical Details

### Technologies Used

- **Framework**: .NET 8.0
- **Language**: C# 12
- **JSON Library**: Newtonsoft.Json 13.0.3
- **Platform**: Windows x64 with P/Invoke for Windows API

### Windows API Functions

The application uses P/Invoke to call native Windows API functions:

**For Recording (GlobalHook.cs)**:
- `SetWindowsHookEx` - Install low-level keyboard and mouse hooks
- `UnhookWindowsHookEx` - Remove hooks
- `CallNextHookEx` - Pass events to next hook in chain
- `GetModuleHandle` - Get module handle for hook installation

**For Playback (InputSimulator.cs)**:
- `SetCursorPos` - Move mouse cursor
- `mouse_event` - Simulate mouse clicks
- `keybd_event` - Simulate keyboard input

### Thread Safety

- Console operations are thread-safe when called from hook callbacks
- Recording service uses locking for action list access
- Proper disposal pattern implemented for hook cleanup

### Error Handling

- Try-catch blocks for file I/O operations
- Graceful handling of hook installation failures
- Validation of user inputs
- Clear error messages with emoji indicators

## ⚠️ Important Notes

### Administrator Privileges

For the most reliable global hook functionality, **run the application as Administrator**:

1. Right-click on the executable or shortcut
2. Select "Run as administrator"
3. Confirm the UAC prompt

Without administrator privileges, hooks may not capture all events reliably.

### Security Considerations

- The application captures system-wide keyboard and mouse inputs
- Be cautious when recording sensitive information (passwords, personal data)
- Sessions are saved as plain text JSON files
- Keep your session files secure

### Limitations

- Windows-only application (uses Windows API)
- May not work in some sandboxed or protected applications
- Playback timing may vary slightly depending on system performance
- Some special keys or complex input sequences may not be captured perfectly

## 🐛 Troubleshooting

### Hooks Not Working

- **Run as Administrator**: This is the most common solution
- **Antivirus**: Some antivirus software may block global hooks
- **Windows Security**: Check if Windows Defender is blocking the application

### Playback Not Working

- **Target Application**: Ensure the target application window is active
- **Timing Issues**: Try adjusting playback speed
- **Screen Resolution**: Recorded coordinates may not work if screen resolution changed

### Session Files Not Found

- Default location: `C:\Users\YourUsername\Documents\LanguageLearningRecorder\Sessions\`
- Check if directory has proper permissions
- Verify JSON files are not corrupted

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🤝 Contributing

Contributions are welcome! Please feel free to submit pull requests or open issues for bugs and feature requests.

## 📧 Support

If you encounter any issues or have questions, please open an issue on GitHub.

## 🙏 Acknowledgments

- Built with .NET 8.0
- Uses Newtonsoft.Json for serialization
- Inspired by the need to automate repetitive language learning tasks

---

**Made with ❤️ for language learners**