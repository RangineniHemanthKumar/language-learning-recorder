using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using LanguageLearningRecorder.Models;

namespace LanguageLearningRecorder.Services;

public class SessionManager
{
    private readonly string _sessionsDirectory;

    public SessionManager()
    {
        string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        _sessionsDirectory = Path.Combine(documentsPath, "LanguageLearningRecorder", "Sessions");
        
        if (!Directory.Exists(_sessionsDirectory))
        {
            Directory.CreateDirectory(_sessionsDirectory);
        }
    }

    public void SaveSession(Session session)
    {
        try
        {
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string fileName = $"{session.Name}_{timestamp}.json";
            string filePath = Path.Combine(_sessionsDirectory, fileName);

            string json = JsonConvert.SerializeObject(session, Formatting.Indented);
            File.WriteAllText(filePath, json);

            Console.WriteLine($"✅ Session saved: {fileName}");
            Console.WriteLine($"   Location: {filePath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error saving session: {ex.Message}");
        }
    }

    public Session? LoadSession(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"❌ File not found: {filePath}");
                return null;
            }

            string json = File.ReadAllText(filePath);
            var session = JsonConvert.DeserializeObject<Session>(json);
            
            if (session != null)
            {
                Console.WriteLine($"✅ Session loaded: {Path.GetFileName(filePath)}");
                Console.WriteLine($"   Name: {session.Name}");
                Console.WriteLine($"   Created: {session.CreatedDate}");
                Console.WriteLine($"   Actions: {session.Actions.Count}");
            }

            return session;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error loading session: {ex.Message}");
            return null;
        }
    }

    public List<string> GetSessionFiles()
    {
        try
        {
            if (!Directory.Exists(_sessionsDirectory))
            {
                return new List<string>();
            }

            return Directory.GetFiles(_sessionsDirectory, "*.json")
                           .OrderByDescending(f => File.GetCreationTime(f))
                           .ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error listing sessions: {ex.Message}");
            return new List<string>();
        }
    }

    public bool DeleteSession(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"❌ File not found: {filePath}");
                return false;
            }

            File.Delete(filePath);
            Console.WriteLine($"✅ Session deleted: {Path.GetFileName(filePath)}");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error deleting session: {ex.Message}");
            return false;
        }
    }

    public string GetSessionsDirectory()
    {
        return _sessionsDirectory;
    }
}
