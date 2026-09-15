using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class RankSaver
{
    private static List<RankingEntry> _entries;

    private static string SavePath
    {
        get
        {
            return Path.Combine(
                Application.persistentDataPath,
                "Rankings",
                "rankings.json"
            );
        }
    }

    public static List<RankingEntry> Load()
    {
        if (_entries != null)
            return _entries;

        _entries = new List<RankingEntry>();

        try
        {
            if (File.Exists(SavePath))
            {
                string json = File.ReadAllText(SavePath);

                if (!string.IsNullOrEmpty(json))
                {
                    RankListWrapper wrapper =
                        JsonUtility.FromJson<RankListWrapper>(json);

                    if (wrapper != null && wrapper.entries != null)
                    {
                        _entries.AddRange(wrapper.entries);
                        SortAndRank();
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Failed to load rankings: " + e.Message);
        }

        return _entries;
    }

    public static int Insert(RankingEntry entry)
    {
        Load();
        _entries.Add(entry);
        SortAndRank();
        Save();
        return entry.rank;
    }

    public static void Save()
    {
        try
        {
            string dir = Path.GetDirectoryName(SavePath);

            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            RankListWrapper wrapper =
                new RankListWrapper { entries = _entries };

            File.WriteAllText(
                SavePath,
                JsonUtility.ToJson(wrapper, true)
            );
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Failed to save rankings: " + e.Message);
        }
    }

    public static void Clear()
    {
        _entries = new List<RankingEntry>();
        Save();
    }

    public static List<RankingEntry> GetTop(int count)
    {
        Load();

        if (count < 1)
            return new List<RankingEntry>();

        int n = Mathf.Min(count, _entries.Count);
        return _entries.GetRange(0, n);
    }

    private static void SortAndRank()
    {
        _entries.Sort(delegate (RankingEntry a, RankingEntry b)
        {
            int scoreCompare = b.score.CompareTo(a.score);

            if (scoreCompare != 0)
                return scoreCompare;

            int killCompare = b.kills.CompareTo(a.kills);

            if (killCompare != 0)
                return killCompare;

            return b.survivalTime.CompareTo(a.survivalTime);
        });

        for (int i = 0; i < _entries.Count; i++)
        {
            _entries[i].rank = i + 1;
        }
    }

    [System.Serializable]
    private class RankListWrapper
    {
        public List<RankingEntry> entries =
            new List<RankingEntry>();
    }
}