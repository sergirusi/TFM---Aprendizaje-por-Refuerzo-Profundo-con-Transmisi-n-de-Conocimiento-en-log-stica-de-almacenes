using System.Collections.Generic;
using UnityEngine;

public class Utils
{
    public static void Log(string message, object obj)
    {
        var output = JsonUtility.ToJson(obj, true);
        Debug.Log(message + ' ' + output);
    }

    public static List<GameObject> GetChildrenAsList(GameObject parent)
    {
        List<GameObject> children = new List<GameObject>();
        for (int i = 0; i < parent.transform.childCount; i++)
        {
            children.Add(parent.transform.GetChild(i).gameObject);
        }
        return children;
    }

    public static bool CheckIfPositionIsFree(Vector3 position, Bounds objectBounds) {
        return Physics.CheckSphere(position, objectBounds.extents.magnitude);
    }

    public static Vector3 NormalizePosition(Vector3 position, Vector3 minBounds, Vector3 maxBounds)
    {
        // Normalizing position in a range of [0, 1]
        return new Vector3(
            (position.x - minBounds.x) / (maxBounds.x - minBounds.x),
            (position.y - minBounds.y) / (maxBounds.y - minBounds.y),
            (position.z - minBounds.z) / (maxBounds.z - minBounds.z)
        );
    }
}