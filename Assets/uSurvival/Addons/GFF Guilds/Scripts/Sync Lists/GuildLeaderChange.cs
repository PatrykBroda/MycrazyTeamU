using Mirror;
using System;

[Serializable]
public partial struct GuildLeaderChange
{
    public string name;
    public bool state;

    // constructors
    public GuildLeaderChange(string name, bool state)
    {
        this.name = name;
        this.state = state;
    }
}

public class SyncListNewMasterVoting : SyncList<GuildLeaderChange> { }
