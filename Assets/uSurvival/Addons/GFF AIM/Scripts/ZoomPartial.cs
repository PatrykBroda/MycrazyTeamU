using GFFAddons;
using UnityEngine;

namespace uSurvival
{
    public partial class Zoom
    {
        public PlayerReloading reloading;
        public LayerMask layerMaskForScope;
        public LayerMask layerMaskNormalView;

        private Camera cachedCamera;

        private void Start()
        {
            cachedCamera = Camera.main;
        }

        private void Update()
        {
            if (!isLocalPlayer) return;

            // holding down the right mouse button and using a ranged weapon?
            if (Input.GetMouseButton(1) && reloading.ReloadTimeRemaining() == 0)
            {
                //if weapon selected is present
                if (equipment.slots[equipment.selection].amount > 0 && equipment.slots[equipment.selection].item.data is RangedWeaponItem weapon)
                {
                    if (equipment.slots[equipment.selection].item.modulesHash[3] != 0 && ScriptableItem.dict.TryGetValue(equipment.slots[equipment.selection].item.modulesHash[3], out ScriptableItem itemData))
                    {
                        if (UIMainPanel.singleton.panel.activeSelf == false)
                        {
                            UIAim.singleton.Show(((ScriptableWeaponModule)itemData).sightsImage);
                            AssignFieldOfView(defaultFieldOfView - (weapon.zoom) * ((ScriptableWeaponModule)itemData).zoom);
                            cachedCamera.cullingMask = layerMaskForScope;
                        }
                    }
                    else
                    {
                        if (UIAim.singleton.AnyLockWindowActive() == false)
                            AssignFieldOfView(defaultFieldOfView - (weapon.zoom));
                    }
                }
                else
                {
                    AssignFieldOfView(defaultFieldOfView);
                    UIAim.singleton.Hide();
                    cachedCamera.cullingMask = layerMaskNormalView;
                }
            }
            // otherwise reset field of view
            else
            {
                AssignFieldOfView(defaultFieldOfView);
                UIAim.singleton.Hide();
                cachedCamera.cullingMask = layerMaskNormalView;
            }
        }
    }
}