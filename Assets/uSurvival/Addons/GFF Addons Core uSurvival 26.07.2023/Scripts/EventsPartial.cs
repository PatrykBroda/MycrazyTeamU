using System;
using UnityEngine.Events;
using uSurvival;

#if UNITY_EDITOR
using UnityEditor.Events;
#endif

public class EventsPartial
{
    private static bool CheckListener(UnityEvent unityEvent, UnityAction unityAction)
    {
        for (int index = 0; index < unityEvent.GetPersistentEventCount(); index++)
        {
            string curEventName = unityEvent.GetPersistentMethodName(index);
            if (curEventName == unityAction.Method.Name) return false;
        }

        return true;
    }

    private static bool CheckListener(UnityEventPlayer unityEvent, UnityAction<Player> unityAction)
    {
        for (int index = 0; index < unityEvent.GetPersistentEventCount(); index++)
        {
            string curEventName = unityEvent.GetPersistentMethodName(index);
            if (curEventName == unityAction.Method.Name) return false;
        }

        return true;
    }

    public static bool AddListenerOnceUnityEventPlayer(UnityEventPlayer unityEvent, UnityAction<Player> unityAction)
    {
        for (int index = 0; index < unityEvent.GetPersistentEventCount(); index++)
        {
            string curEventName = unityEvent.GetPersistentMethodName(index);
            if (curEventName == unityAction.Method.Name) return false;
        }

        return true;
    }

#if UNITY_EDITOR
    public static void AddListenerOnceOnConnected(UnityEvent unityEvent, UnityAction unityAction, Database database)
    {
        if (CheckListener(unityEvent, unityAction))
        {
            var targetinfo = UnityEvent.GetValidMethodInfo(database, "onConnected", new Type[0]);
            UnityEventTools.AddPersistentListener(database.onConnected, unityAction);
        }
    }

    public static void AddListenerOnceCharacterLoad(UnityEventPlayer unityEvent, UnityAction<Player> unityAction, Database database)
    {
        if (CheckListener(unityEvent, unityAction))
        {
            var targetinfo = UnityEvent.GetValidMethodInfo(database, "onCharacterLoad", new Type[0]);
            UnityEventTools.AddPersistentListener(database.onCharacterLoad, unityAction);
        }
    }

    public static void AddListenerOnceCharacterSave(UnityEventPlayer unityEvent, UnityAction<Player> unityAction, Database database)
    {
        if (CheckListener(unityEvent, unityAction))
        {
            var targetinfo = UnityEvent.GetValidMethodInfo(database, "onCharacterSave", new Type[0]);
            UnityEventTools.AddPersistentListener(database.onCharacterSave, unityAction);
        }
    }
#endif
}
