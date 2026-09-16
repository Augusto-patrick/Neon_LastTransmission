using System.Collections.Generic;

// Stored at users/{mobile} in the database. One of these per player.
[System.Serializable]
public class UserProfile
{
    public string name;
    public string mobile;
    public string email;
    public Dictionary<string, int> scores = new Dictionary<string, int>();
    public int cumulativeScore;
}
