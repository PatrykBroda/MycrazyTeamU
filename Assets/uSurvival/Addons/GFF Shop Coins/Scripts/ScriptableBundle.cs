using UnityEngine;
using uSurvival;

namespace GFFAddons
{
    [CreateAssetMenu(menuName = "uSurvival Item/Bundle", order = 999)]
    public class ScriptableBundle : ScriptableObject
    {
        public string description;
        public ScriptableItemAndAmount[] items;
        public uint gold;
        public uint price;
    }
}


