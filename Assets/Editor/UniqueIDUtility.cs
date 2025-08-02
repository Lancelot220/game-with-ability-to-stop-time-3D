#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class UniqueIDUtility
{
    [MenuItem("Tools/Generate Unique IDs For Scene Objects")]
    public static void GenerateIDs()
    {
        var objects = GameObject.FindObjectsOfType<UniqueID>();

        int count = 0;

        foreach (var obj in objects)
        {
            var so = new SerializedObject(obj);
            var prop = so.FindProperty("uniqueID");

            if (string.IsNullOrEmpty(prop.stringValue))
            {
                prop.stringValue = System.Guid.NewGuid().ToString();
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(obj);
                count++;
            }
        }

        Debug.Log($"✅ Generated Unique IDs for {count} object(s).");
    }
}
#endif
