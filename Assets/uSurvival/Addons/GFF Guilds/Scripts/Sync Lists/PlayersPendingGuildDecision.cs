using Mirror;
using System;

[Serializable]
public partial struct PlayersPendingGuildDecision
{
    public string character;
    public string characterClass;

    // constructors
    public PlayersPendingGuildDecision(string name, string characterClass)
    {
        this.character = name;
        this.characterClass = characterClass;
    }
}

public class SyncListApprovalRequest : SyncList<PlayersPendingGuildDecision> { }
