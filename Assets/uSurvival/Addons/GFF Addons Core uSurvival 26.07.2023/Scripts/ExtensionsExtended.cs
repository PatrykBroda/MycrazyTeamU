using System;
using UnityEngine;

namespace GFFAddons
{
    public static class ExtensionsExtended
    {
        public static byte ToByte(this string value, byte errVal = 0)
        {
            Byte.TryParse(value, out errVal);
            return errVal;
        }

        // string to long (returns errVal if failed)
        public static long ToLong(this string value, long errVal = 0)
        {
            Int64.TryParse(value, out errVal);
            return errVal;
        }

        public static ushort ToUshort(this string value, ushort errVal = 0)
        {
            UInt16.TryParse(value, out errVal);
            return errVal;
        }

        // Mathf.Clamp only works for float and int. we need some more versions:
        public static long Clamp(long value, long min, long max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        public static uint ClampUInt(uint value, uint min, uint max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        public static void SetLayerRecursively(GameObject go, int layerNumber)
        {
            if (go == null) return;
            foreach (Transform trans in go.GetComponentsInChildren<Transform>(true))
            {
                if (trans.gameObject.layer != LayerMask.NameToLayer("MinimapMarker")) trans.gameObject.layer = layerNumber;
            }
        }
    }
}


