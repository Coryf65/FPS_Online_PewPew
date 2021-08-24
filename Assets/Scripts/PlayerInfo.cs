
[System.Serializable]
public class PlayerInfo
{
    public string Name { get; set; }
    public int ActorID { get; set; }
    public int Kills { get; set; }
    public int Deaths { get; set; }

    public PlayerInfo(string name, int actorID, int kills, int deaths)
    {
        Name = name;
        ActorID = actorID;
        Kills = kills;
        Deaths = deaths;
    }
}