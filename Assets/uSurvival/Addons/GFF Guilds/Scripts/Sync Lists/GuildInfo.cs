using Mirror;
using System;

[Serializable]
public partial struct GuildInfo
{
    public string name;
    public string master;
    public int members;
    public bool free;
    public int wins;

    // constructors
    public GuildInfo(string name, string master, int members, bool free, int wins)
    {
        this.name = name;
        this.master = master;
        this.members = members;
        this.free = free;
        this.wins = wins;
    }
}

public class SyncListGuildInfo : SyncList<GuildInfo> { }
