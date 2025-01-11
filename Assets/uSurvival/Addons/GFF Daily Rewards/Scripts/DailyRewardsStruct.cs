using Mirror;
using System;

[Serializable]
public struct DailyRewardsStruct
{
    public int day;
    public bool done;
    public bool get;

    // constructors
    public DailyRewardsStruct(int day, bool done, bool get)
    {
        this.day = day;
        this.done = done;
        this.get = get;
    }
}

public class SyncListDailyRewards : SyncList<DailyRewardsStruct> { }