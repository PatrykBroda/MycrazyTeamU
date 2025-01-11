using GFFAddons;

namespace uSurvival
{
    public partial class ScriptableItem
    {
        public bool tradable = true;
    }

    public partial struct Item
    {
        public bool tradable => data.tradable;
    }

    public partial class Player
    {
        public PlayerTrading trading;
    }
}