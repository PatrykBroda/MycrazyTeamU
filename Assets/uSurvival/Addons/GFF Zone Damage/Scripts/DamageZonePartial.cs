using Mirror;
using UnityEngine;

namespace uSurvival
{
    public partial class Player
    {
        // safe zone flag
        // -> needs to be in Entity because both player and pet need it
        [HideInInspector] public int inDamageZone;
        [HideInInspector] public int inRestoreZone;
        [SyncVar] public Vector3 savePosition;

        //// ontrigger ///////////////////////////////////////////////////////////////
        //// protected so that inheriting classes can use OnTrigger too, while also
        //// calling those here via base.OnTriggerEnter/Exit
        //protected virtual void OnTriggerEnter(Collider col)
        //{
        //    // check if trigger first to avoid GetComponent tests for environment
        //    if (col.isTrigger && col.GetComponent<DamageZone>())
        //        inDamageZone = col.GetComponent<DamageZone>().damage;
        //    else if (col.isTrigger && col.GetComponent<SafeZone>())
        //    {
        //        if (savePosition != col.gameObject.transform.position)
        //        {
        //            savePosition = col.gameObject.transform.position;
        //            //chat.TargetMsgSafeZone("Место возрождения сохранено");
        //        }
        //    }
        //    else if (col.isTrigger && col.GetComponent<RestoreZone>())
        //    {
        //        inRestoreZone = col.GetComponent<RestoreZone>().restoreHealth;
        //    }
        //}

        //protected virtual void OnTriggerExit(Collider col)
        //{
        //    // check if trigger first to avoid GetComponent tests for environment
        //    if (col.isTrigger && col.GetComponent<DamageZone>())
        //    {
        //        inDamageZone = 0;
        //    }
        //    else if (col.isTrigger && col.GetComponent<RestoreZone>())
        //    {
        //        inRestoreZone = 0;
        //    }
        //}

        /*public override void OnStartServer()
        {
            // health recovery every second
            InvokeRepeating(nameof(DamageZone), 1, 1);
        }*/

        //void DamageZone()
        //{
        //    if (inDamageZone > 0 && health.current > 0)
        //    {
        //        int value = 0;
        //        if (combat.anomalyResistanceBonus >= 1) value = 0;
        //        else value = inDamageZone - (int)(inDamageZone * combat.anomalyResistanceBonus);

        //        health.current -= value;
        //        chat.TargetMsgInfo("Вы попали в аномалию, Здоровье -" + value);
        //    }
        //    else if (inRestoreZone > 0 && health.current > 0 && health.current < health.max)
        //    {
        //        health.current += inRestoreZone;
        //        //chat.TargetMsgSafeZone("Востановление Здоровья +" + inRestoreZone);
        //    }
        //}
    }
}


