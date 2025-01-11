// Ranged weapons that raycast instead of using projectiles. E.g. pistols, guns.
using UnityEngine;

namespace uSurvival
{
    [CreateAssetMenu(menuName="uSurvival Item/Weapon(Ranged Raycast)", order=999)]
    public partial class RangedRaycastWeaponItem : RangedWeaponItem
    {
        //public override void UseHotbar(Player player, int hotbarIndex, Vector3 lookAt)
        //{
        //    // raycast to find out what we hit
        //    if (RaycastToLookAt(player, lookAt, out RaycastHit hit))
        //    {
        //        // hit an entity? then deal damage
        //        Entity victim = hit.transform.GetComponent<Entity>();
        //        if (victim)
        //        {
        //            player.combat.DealDamageAt(victim, damage, hit.point, hit.normal, hit.collider);
        //        }
        //    }

        //    // base logic (decrease ammo and durability)
        //    base.UseHotbar(player, hotbarIndex, lookAt);
        //}
    }
}