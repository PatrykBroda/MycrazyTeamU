using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using uSurvival;

namespace GFFAddons
{
    public static class UtilsExtended
    {
        public static void BalancePrefabsExtended(GameObject prefab, int amount, Transform parent)
        {
            // instantiate until amount
            for (int i = 0; i < amount; ++i)
            {
                var go = GameObject.Instantiate(prefab);
                go.transform.SetParent(parent.GetChild(i).transform, false);
            }

            // delete everything that's too much
            // (backwards loop because Destroy changes childCount)
            for (int i = 0; i < amount; ++i)
                if (parent.GetChild(i).transform.childCount > 1)
                    GameObject.Destroy(parent.GetChild(i).transform.GetChild(1).gameObject);
        }

        public static Player FindPlayerByName(string name)
        {
            if (Player.onlinePlayers.ContainsKey(name))
            {
                return Player.onlinePlayers[name].GetComponent<Player>();
            }
            else return null;
        }

        //colors
        static string DecToHex(int value)
        {
            return value.ToString("X2");
        }
        static string FloatNormToHex(float value)
        {
            return DecToHex(Mathf.RoundToInt(value * 255f));
        }
        public static string GetStringFromColor(Color color)
        {
            string red = FloatNormToHex(color.r);
            string green = FloatNormToHex(color.g);
            string blue = FloatNormToHex(color.b);

            return red + green + blue;
        }

        // invoke multiple functions by prefix via reflection.
        // -> works for static classes too if object = null
        // -> cache it so it's fast enough for Update calls
        static Dictionary<KeyValuePair<Type, string>, MethodInfo[]> check = new Dictionary<KeyValuePair<Type, string>, MethodInfo[]>();
        public static MethodInfo[] GetMethodsByPrefix(Type type, string methodPrefix)
        {
            KeyValuePair<Type, string> key = new KeyValuePair<Type, string>(type, methodPrefix);
            if (!check.ContainsKey(key))
            {
                MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)
                                           .Where(m => m.Name.StartsWith(methodPrefix))
                                           .ToArray();
                check[key] = methods;
            }
            return check[key];
        }

        public static void InvokeMany(Type type, object onObject, string methodPrefix, params object[] args)
        {
            foreach (MethodInfo method in GetMethodsByPrefix(type, methodPrefix))
                method.Invoke(onObject, args.ToArray());
        }
        public static bool InvokeBool(Type type, object onObject, string methodPrefix, params object[] args)
        {
            foreach (MethodInfo method in GetMethodsByPrefix(type, methodPrefix))
            {
                return (bool)method.Invoke(onObject, args);
            }

            return false;
        }

        public static float InvokeFloat(Type type, object onObject, string methodPrefix, params object[] args)
        {
            foreach (MethodInfo method in GetMethodsByPrefix(type, methodPrefix))
            {
                return (float)method.Invoke(onObject, args);
            }

            return 0;
        }

        public static Item InvokeManyItem(Type type, object onObject, string methodPrefix, params object[] args)
        {
            Item item = (Item)args[0];
            foreach (MethodInfo method in GetMethodsByPrefix(type, methodPrefix))
            {
                item = (Item)method.Invoke(onObject, args);
                args[0] = item;
            }

            return item;
        }
        public static ItemSlot InvokeManyItemSlot(Type type, object onObject, string methodPrefix, params object[] args)
        {
            ItemSlot itemslot = (ItemSlot)args[0];
            foreach (MethodInfo method in GetMethodsByPrefix(type, methodPrefix))
            {
                itemslot = (ItemSlot)method.Invoke(onObject, args);
                args[0] = itemslot;
            }

            return itemslot;
        }

        public static int ToInt(this bool Value)
        {
            return Value ? 1 : 0;
        }
        public static bool ToBool(this int Value)
        {
            return Value == 1 ? true : false;
        }

        // pretty print seconds as hours:minutes:seconds(.milliseconds/100)s
        public static string PrettySeconds(float seconds)
        {
            TimeSpan t = TimeSpan.FromSeconds(seconds);
            string res = "";
            if (t.Days > 0) res += t.Days + "d";
            if (t.Hours > 0)
            {
                if (res == "") res += t.Hours + "h";
                else res += " " + t.Hours + "h";
            }
            if (t.Minutes > 0)
            {
                if (res == "") res += t.Minutes + "m";
                else res += " " + t.Minutes + "m";
            }
            // 0.5s, 1.5s etc. if any milliseconds. 1s, 2s etc. if any seconds
            if (t.Milliseconds > 0) res += " " + t.Seconds + "." + (t.Milliseconds / 100) + "s";
            else if (t.Seconds > 0) res += " " + t.Seconds + "s";
            // if the string is still empty because the value was '0', then at least
            // return the seconds instead of returning an empty string
            return res != "" ? res : "0s";
        }


        public static void AddEventTrigger(this EventTrigger eventTrigger, UnityAction<BaseEventData> action, EventTriggerType triggerType)
        {
            EventTrigger.TriggerEvent trigger = new EventTrigger.TriggerEvent();
            trigger.AddListener(action);

            EventTrigger.Entry entry = new EventTrigger.Entry { callback = trigger, eventID = triggerType };
            eventTrigger.triggers.Add(entry);
        }

        public static bool AddListenerOnce(UnityEvent unityEvent, UnityAction unityAction)
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
    }
}