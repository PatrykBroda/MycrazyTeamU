using System;
using UnityEngine;

public class ScaleFix : MonoBehaviour
{
    [ContextMenu("Fix Scale")]
    public void Fix()
    {
        Transform[] childArray = GetComponentsInChildren<Transform>();
        for (int i = 0; i < childArray.Length; i++)
        {
            if (childArray[i].localScale.x < 0)
            {
                childArray[i].localScale = new Vector3(1, childArray[i].localScale.y, childArray[i].localScale.z);
            }
            if (childArray[i].localScale.y < 0)
            {
                childArray[i].localScale = new Vector3(childArray[i].localScale.x, 1, childArray[i].localScale.z);
            }
            if (childArray[i].localScale.z < 0)
            {
                childArray[i].localScale = new Vector3(childArray[i].localScale.x, childArray[i].localScale.y, 1);
            }
        }
    }

    [ContextMenu("Fix Colliders Scale")]
    public void CollidersFix()
    {
        CapsuleCollider[] childArray = GetComponentsInChildren<CapsuleCollider>();
        for (int i = 0; i < childArray.Length; i++)
        {
            childArray[i].radius = (float)Math.Round(childArray[i].radius, 4);
            childArray[i].height = (float)Math.Round(childArray[i].height, 4);
        }

        BoxCollider[] boxes = GetComponentsInChildren<BoxCollider>();
        for (int i = 0; i < childArray.Length; i++)
        {
            boxes[i].center = new Vector3((float)Math.Round(boxes[i].center.x, 4), (float)Math.Round(boxes[i].center.y, 4), (float)Math.Round(boxes[i].center.z, 4));
            boxes[i].size = new Vector3((float)Math.Round(boxes[i].size.x, 4), (float)Math.Round(boxes[i].size.y, 4), (float)Math.Round(boxes[i].size.z, 4));
        }
    }
}


