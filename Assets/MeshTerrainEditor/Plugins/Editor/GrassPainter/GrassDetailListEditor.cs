using UnityEditor;
using UnityEngine;

namespace MTE
{
    [CustomEditor(typeof(GrassDetailList))]
    public class GrassDetailListEditor : MTEInspector
    {
        public override void OnEnable()
        {
            base.OnEnable();
            detailList = target as GrassDetailList;
        }

        private GrassDetailList detailList;

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
                return;//corrupt asset
            }

            EditorGUI.BeginChangeCheck();

            for (var i = 0; i < detailList.grassDetailList.Count; i++)
            {
                var grassDetail = detailList.grassDetailList[i];
                EditorGUILayout.BeginHorizontal("box");
                {
                    EditorGUILayout.BeginVertical(GUILayout.Width(70));
                    var controlRect = EditorGUILayout.GetControlRect(true, GUILayout.Width(80), GUILayout.Height(80));
                    GrassDetailPreview.DrawGrassPreview(grassDetail, controlRect);

                    EditorGUILayout.EndVertical();

                    EditorGUILayout.BeginVertical(GUILayout.Height(128f));
                    {
                        EditorGUILayout.BeginHorizontal();
                        if (grassDetail.autoName)
                        {
                            if (grassDetail.grassType == GrassType.CustomMesh)
                            {
                                grassDetail.name = grassDetail.grassMesh ? grassDetail.grassMesh.name : "mesh_grass";
                            }
                            else if (grassDetail.grassType == GrassType.OneQuad)
                            {
                                grassDetail.name = "quad_grass";
                            }
                            else
                            {
                                grassDetail.name = "star_grass";
                            }

                            GUI.enabled = false;
                            EditorGUILayout.TextField(grassDetail.name);
                            GUI.enabled = true;
                        }
                        else
                        {
                            grassDetail.name = EditorGUILayout.TextField(grassDetail.name);
                        }
                        EditorGUI.BeginChangeCheck();
                        grassDetail.autoName = GUILayout.Toggle(grassDetail.autoName,
                            grassDetail.autoName ? Styles.LockedContent : Styles.NotLockedContent, EditorStyles.miniButton,
                            GUILayout.Width(22));
                        if (EditorGUI.EndChangeCheck())
                        {
                            Repaint();
                        }
                        EditorGUILayout.EndHorizontal();

                        grassDetail.material = (Material)EditorGUILayout.ObjectField(StringTable.Get(C.Material), grassDetail.material, typeof(Material), false);
                        EditorGUILayoutEx.MinMaxSlider(StringTable.Get(C.Width), ref grassDetail.minWidth, ref grassDetail.maxWidth, 0.01f, 100f);
                        EditorGUILayoutEx.MinMaxSlider(StringTable.Get(C.Height), ref grassDetail.minHeight, ref grassDetail.maxHeight, 0.01f, 100f);
                        grassDetail.grassType = (GrassType)EditorGUILayout.EnumPopup(StringTable.Get(C.Type), grassDetail.grassType);
                        if (grassDetail.grassType == GrassType.CustomMesh)
                        {
                            grassDetail.grassMesh = (Mesh)EditorGUILayout.ObjectField(StringTable.Get(C.Mesh),
                                grassDetail.grassMesh, typeof(Mesh), false);
                        }
                        else
                        {
                            grassDetail.grassMesh = null;
                        }
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

            if (GUILayout.Button(StringTable.Get(C.Add)))
            {
                detailList.grassDetailList.Add(new GrassDetail());
            }

            if (detailList.grassDetailList.Count == 0)
            {
                EditorGUILayout.LabelField(StringTable.Get(C.Warning_NoPrototypes));
                if (GUILayout.Button(StringTable.Get(C.LoadFromGrassLoader)))
                {
                    LoadFromGrassLoader();
                }
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
            this.detailList.grassDetailList.RemoveAt(i);
            this.SaveDetailList();
        }

        private void SaveDetailList()
        {
            EditorUtility.SetDirty(target);
            MTEDebug.Log($"{nameof(GrassDetailList)}<{this.name}> saved");
        }

        public void LoadFromGrassLoader()
        {
            MTEDebug.Log("Loading prototypes from existing GrassInstanceList on the GrassLoader...");
            MTEDebug.Log("The min/max width/height of loaded prototypes will use default values because MTE cannot determine them from the grass instances.");

            var grassLoader = MTEContext.TheGrassLoader;
            if (grassLoader == null)
            {
                Debug.LogWarning("No grass loader loaded. Assign it first.");
                return;
            }

            MTEDebug.Log("Remove existing prototypes.");
            var list = this.detailList.grassDetailList;

            var instanceList = grassLoader.grassInstanceList;
            if (instanceList.grasses != null && instanceList.grasses.Count != 0)
            {
                foreach (var grassStar in instanceList.grasses)
                {
                    bool found = false;
                    foreach (var d in list)
                    {
                        var gd = d as GrassDetail;
                        if (gd.Material == grassStar.Material)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (found)
                    {
                        continue;
                    }
                    var grassDetail = new GrassDetail
                    {
                        Material = grassStar.Material,
                        MinWidth = GrassDetail.DefaultMinWidth,
                        MaxWidth = GrassDetail.DefaultMaxWidth,
                        MinHeight = GrassDetail.DefaultMinHeight,
                        MaxHeight = GrassDetail.DefaultMaxHeight,
                        GrassType = GrassType.ThreeQuad
                    };
                    list.Add(grassDetail);
                }
                MTEDebug.LogFormat("{0} prototype(s)(three quads) Loaded.", list.Count);
            }

            if (instanceList.quads != null && instanceList.quads.Count != 0)
            {
                var oldCount = list.Count;
                foreach (var quad in instanceList.quads)
                {
                    bool found = false;
                    foreach (var detail in list)
                    {
                        if (detail.Material == quad.Material)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (found)
                    {
                        continue;
                    }
                    var grassDetail = new GrassDetail
                    {
                        Material = quad.Material,
                        MinWidth = GrassDetail.DefaultMinWidth,
                        MaxWidth = GrassDetail.DefaultMaxWidth,
                        MinHeight = GrassDetail.DefaultMinHeight,
                        MaxHeight = GrassDetail.DefaultMaxHeight,
                        GrassType = GrassType.OneQuad
                    };
                    list.Add(grassDetail);
                }
                MTEDebug.LogFormat("{0} prototype(s)(one quad) Loaded.", list.Count - oldCount);
            }

            if (instanceList.customMeshes != null && instanceList.customMeshes.Count > 0)
            {
                var oldCount = list.Count;
                foreach (var customMesh in instanceList.customMeshes)
                {
                    bool found = false;
                    foreach (var detail in list)
                    {
                        if (detail.Material == customMesh.Material)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (found)
                    {
                        continue;
                    }
                    var grassDetail = new GrassDetail
                    {
                        GrassMesh = customMesh.Mesh,
                        Material = customMesh.Material,
                        MinWidth = GrassDetail.DefaultMinWidth,
                        MaxWidth = GrassDetail.DefaultMaxWidth,
                        MinHeight = GrassDetail.DefaultMinHeight,
                        MaxHeight = GrassDetail.DefaultMaxHeight,
                        GrassType = GrassType.CustomMesh
                    };
                    list.Add(grassDetail);
                }
                MTEDebug.LogFormat("{0} prototype(s)(custom mesh) Loaded.", list.Count - oldCount);
            }

            //save to asset
            SaveDetailList();

            if (MTEEditorWindow.Instance && MTEEditorWindow.Instance.IsMTEEnabled() &&
                MTEContext.editor.GetType() == typeof(GrassPainter)
                && GrassPainter.Instance != null)
            {
                GrassPainter.Instance.LoadGrassDetailList();
            }
        }
    }
}