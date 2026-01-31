using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LeaderboardManager : MonoBehaviour
{
    public static LeaderboardManager Instance { get; private set; }

    [Serializable]
    public class Entry
    {
        public string name;
        public float timeSeconds;
        public bool isAnonymous;
        public long unixTime; 
    }

    [Serializable]
    private class SaveData
    {
        public List<Entry> entries = new List<Entry>();
    }

    private SaveData data = new SaveData();
    private string SavePath => Path.Combine(Application.persistentDataPath, "leaderboard.json");

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        Load();
    }

    public void AddOrUpdate(string playerName, bool isAnonymous, float timeSeconds)
    {
        Load(); 

        var e = new Entry
        {
            name = string.IsNullOrWhiteSpace(playerName) ? "Player" : playerName.Trim(),
            isAnonymous = isAnonymous,
            timeSeconds = timeSeconds,
            unixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };

        if (!isAnonymous && !IsReservedPlayerName(e.name))
        {
            int idx = data.entries.FindIndex(x => !x.isAnonymous && string.Equals(x.name, e.name, StringComparison.OrdinalIgnoreCase));
            if (idx >= 0)
            {
                if (e.timeSeconds < data.entries[idx].timeSeconds)
                {
                    data.entries[idx].timeSeconds = e.timeSeconds;
                    data.entries[idx].unixTime = e.unixTime;
                }
            }
            else
            {
                data.entries.Add(e);
            }
        }
        else
        {
            data.entries.Add(e);
        }

        Save();
    }

    public List<Entry> GetSorted()
    {
        Load();
        var list = new List<Entry>(data.entries);
        list.Sort((a, b) =>
        {
            int cmp = a.timeSeconds.CompareTo(b.timeSeconds);
            if (cmp != 0) return cmp;
            return a.unixTime.CompareTo(b.unixTime);
        });
        return list;
    }
    public bool IsReservedAnonymousPattern(string name)
    {
        return IsReservedPlayerName(name);
    }

    public string GetNextAnonymousName()
    {
        var used = new HashSet<int>();

        foreach (var e in GetSorted())
        {
            if (!e.isAnonymous) continue;

            if (!e.name.StartsWith("Player", StringComparison.OrdinalIgnoreCase)) continue;

            string suffix = e.name.Substring("Player".Length);
            if (suffix.Length == 0) { used.Add(0); continue; } 

            if (int.TryParse(suffix, out int n))
                used.Add(n);
        }

        for (int n = 1; n < 1000000; n++)
            if (!used.Contains(n))
                return $"Player{n}";

        return "Player999999"; 
    }

    public int GetPlaceFor(string playerName, bool isAnonymous, float timeSeconds)
    {
        var sorted = GetSorted();

        if (!isAnonymous && !IsReservedPlayerName(playerName))
        {
            for (int i = 0; i < sorted.Count; i++)
            {
                if (!sorted[i].isAnonymous && string.Equals(sorted[i].name, playerName, StringComparison.OrdinalIgnoreCase))
                    return i + 1;
            }
            return -1;
        }
        else
        {
            for (int i = 0; i < sorted.Count; i++)
            {
                if (sorted[i].isAnonymous == isAnonymous &&
                    string.Equals(sorted[i].name, playerName, StringComparison.OrdinalIgnoreCase) &&
                    Mathf.Abs(sorted[i].timeSeconds - timeSeconds) < 0.0001f)
                    return i + 1;
            }
            return -1;
        }
    }

    public static string FormatTime(float seconds)
    {
        int total = Mathf.Max(0, Mathf.RoundToInt(seconds));
        int m = total / 60;
        int s = total % 60;
        return $"{m}:{s:00}";
    }

    public bool IsReservedPlayerName(string n)
    {
        if (string.IsNullOrWhiteSpace(n)) return true;
        n = n.Trim();
        if (!n.StartsWith("Player", StringComparison.OrdinalIgnoreCase)) return false;
        if (n.Length == "Player".Length) return true;
        for (int i = "Player".Length; i < n.Length; i++)
            if (!char.IsDigit(n[i])) return false;
        return true;
    }

    private void Save()
    {
        try
        {
            var json = JsonUtility.ToJson(data, true);
            File.WriteAllText(SavePath, json);
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Leaderboard save failed: {ex.Message}");
        }
    }

    private void Load()
    {
        try
        {
            if (!File.Exists(SavePath)) { data = new SaveData(); return; }
            var json = File.ReadAllText(SavePath);
            data = JsonUtility.FromJson<SaveData>(json) ?? new SaveData();
            if (data.entries == null) data.entries = new List<Entry>();
        }
        catch
        {
            data = new SaveData();
        }
    }
}
