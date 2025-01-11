using Mirror;
using System;

namespace GFFAddons
{
    [Serializable]
    public struct NpcClan
    {
        public Clan clan;
        public short level;
        public uint exp;

        // constructors
        public NpcClan(Clan clan, short level = 1, uint exp = 0)
        {
            this.clan = clan;
            this.level = level;
            this.exp = exp;
        }

        public void UpdateExp(int value)
        {
            exp = (uint)(exp + value);
        }
    }
    public class SyncListNpcClan : SyncList<NpcClan> { }
}