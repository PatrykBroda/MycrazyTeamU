using Mirror;
using System;

namespace GFFAddons
{
    [Serializable]
    public struct Friend
    {
        public string name;
        public string className;
        public int level;
        public string guild;
        public bool online;

        // constructors
        public Friend(string name, string className, int level, string guild, bool online)
        {
            this.name = name;
            this.className = className;
            this.level = level;
            this.guild = guild;
            this.online = online;
        }
    }
    public class SyncListFriend : SyncList<Friend> { }
}


