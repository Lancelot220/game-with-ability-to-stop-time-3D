using MTE;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ObjectDetailList))]
public class ObjectDetailListEditor : MTEInspector
{
    public override void OnEnable()
    {
        base.OnEnable();
        detailList = target as ObjectDetailList;
    }

    private ObjectDetailList detailList;


    private static class Styles
    {
        public static GUIContent LockedContent;
        public static GUIContent NotLockedContent;

        private static bool unloaded = true;

        public static void Init()
        {
            if (!unloaded) return;
            LockedContent = new GUIContent(MTEStyles.LockIconClosed, StringTable.Get(C.Info_AutoName));
            NotLockedContent = new GUIContent(MTEStyles.LockIconOpen, StringTable.Get(C.Info_CustomName));
            unloaded = false;
        }
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        Styles.Init();

        if (detailList == null)
        {
            return; //corrupt asset
        }

        EditorGUI.BeginChangeCheck();

        for (var i = 0; i < detailList.list.Count; i++)
        {
            var objectDetail = detailList.list[i];
            EditorGUILayout.BeginHorizontal("box");
            {
                var controlRect = EditorGUILayout.GetControlRect(true, GUILayout.Width(80), GUILayout.Height(80));
                ObjectDetailPreview.DrawObjectPreview(objectDetail, controlRect);

                EditorGUILayout.BeginVertical(GUILayout.Height(128f));
                {
                    EditorGUILayout.BeginHorizontal();
                    if (objectDetail.autoName)
                    {
                        objectDetail.name = objectDetail.Object ? objectDetail.Object.name : "empty";
                        GUI.enabled = false;
                        EditorGUILayout.TextField(objectDetail.name);
                        GUI.enabled = true;
                    }
                    else
                    {
                        objectDetail.name = EditorGUILayout.TextField(objectDetail.name);
                    }
                    objectDetail.autoName = GUILayout.Toggle(objectDetail.autoName,
                        objectDetail.autoName ? Styles.LockedContent : Styles.NotLockedContent,
                        EditorStyles.miniButton,
                        GUILayout.Width(22));
                    EditorGUILayout.EndHorizontal();

                    objectDetail.Object = (GameObject)EditorGUILayout.ObjectField(StringTable.Get(C.Prefab),
                        objectDetail.Object, typeof(GameObject), false);

                    EditorGUILayout.PrefixLabel(StringTable.Get(C.Scale));
                    EditorGUI.indentLevel++;
                    objectDetail.UseUnifiedScale = EditorGUILayout.Toggle(StringTable.Get(C.UnifiedScale), objectDetail.UseUnifiedScale);
                    if (objectDetail.UseUnifiedScale)
                    {
                        var minScale = objectDetail.MinScale.x;
                        var maxScale = objectDetail.MaxScale.x;
                        EditorGUILayoutEx.MinMaxSlider(ref minScale, ref maxScale, 0.01f, 100.0f);
                        objectDetail.MinScale.z = objectDetail.MinScale.y = objectDetail.MinScale.x = minScale;
                        objectDetail.MaxScale.z = objectDetail.MaxScale.y = objectDetail.MaxScale.x = maxScale;
                    }
                    else
                    {
                        EditorGUILayoutEx.MinMaxSlider(StringTable.Get(C.Scale), ref objectDetail.MinScale, ref objectDetail.MaxScale, 0.01f, 100.0f);
                    }
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();

            var e = Event.current;
            var clickArea = GUILayoutUtility.GetLastRect();
            if (clickArea.Contains(e.mousePosition) && e.type == EventType.ContextClick)
            {
                ShowContextMenuForItem(i);
            }
        }

        if (detailList.list.Count == 0)
        {
            EditorGUILayout.HelpBox(StringTable.Get(C.Warning_NoPrototypes), MessageType.Warning, true);
        }

        if (GUILayout.Button(StringTable.Get(C.Add)))
        {
            detailList.list.Add(new ObjectDetail());
        }

        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(target);
        }
    }

    private void ShowContextMenuForItem(int i)
    {
        var menu = new GenericMenu();
        menu.AddItem(new GUIContent(StringTable.Get(C.Delete)), false, RemoveCallback, i);
        menu.ShowAsContext();
        Event.current.Use();
    }

    private void RemoveCallback(object userData)
    {
        bool confirmed = EditorUtility.DisplayDialog(
            StringTable.Get(C.Warning),
            StringTable.Get(C.Warning_Confirm),
            StringTable.Get(C.Yes), StringTable.Get(C.No));
        if (!confirmed)
        {
            return;
        }

        var i = (int)userData;
        this.detailList.list.RemoveAt(i);
        this.SaveDetailList();
    }

    private void SaveDetailList()
    {
        EditorUtility.SetDirty(target);
        MTEDebug.Log($"{nameof(ObjectDetailList)}<{this.name}> saved");
    }
}