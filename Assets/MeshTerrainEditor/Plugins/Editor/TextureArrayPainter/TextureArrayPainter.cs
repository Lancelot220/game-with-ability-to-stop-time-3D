﻿using System;
using System.Collections.Generic;
using System.IO;
using MTE.Undo;
using UnityEditor;
using UnityEngine;
using static MTE.TextureArrayShaderPropertyNames;

namespace MTE
{
    /// <summary>
    /// Texture-array-based Mesh-Terrain texture editor.
    /// </summary>
    internal class TextureArrayPainter : IEditor
    {
        public int Id { get; } = 10;

        public bool Enabled { get; set; } = true;

        public string Name { get; } = nameof(TextureArrayPainter);

        public Texture Icon { get; } =
            EditorGUIUtility.IconContent("TerrainInspector.TerrainToolSplat").image;

        public string Header { get { return StringTable.Get(C.TextureArrayPainter_Header); } }

        public string Description { get { return StringTable.Get(C.TextureArrayPainter_Description); } }

        public bool WantMouseMove { get; } = true;

        public bool WillEditMesh { get; } = false;


        #region Parameters

        #region Constant
        // default
        const EditorFilterMode DefaultPainterMode
            = EditorFilterMode.FilteredGameObjects;
        const float DefaultBrushSize = 1;
        const float DefaultBrushFlow = 0.5f;
        const float DefaultBrushDirection = 0;
        // min/max
        const float MinBrushSize = 0.1f;
        const float MaxBrushSize = 10f;
        const float MinBrushFlow = 0.01f;
        const float MaxBrushFlow = 1f;
        const int MaxHotkeyNumberForTexture = 8;
        #endregion

        public int brushIndex;
        public float brushSize;
        public float brushFlow;
        private int selectedTextureIndex;
        
        private EditorFilterMode painterMode;

        private EditorFilterMode PainterMode
        {
            get { return this.painterMode; }
            set
            {
                if (value != this.painterMode)
                {
                    EditorPrefs.SetInt("MTE_SplatPainter.painterMode", (int)value);
                    this.painterMode = value;
                }
            }
        }

        /// <summary>
        /// Index of selected texture in the texture list; not the layer index.
        /// </summary>
        public int SelectedTextureIndex
        {
            get { return this.selectedTextureIndex; }
            set
            {
                var textureListCount = TextureList.Count;
                if (value < textureListCount)
                {
                    this.selectedTextureIndex = value;
                }
            }
        }

        /// <summary>
        /// Index of selected brush
        /// </summary>
        public int BrushIndex
        {
            get { return brushIndex; }
            set
            {
                if (brushIndex != value)
                {
                    preview.SetPreviewMaskTexture(value);

                    brushIndex = value;
                }
            }
        }


        /// <summary>
        /// Brush size (unit: 1 BrushUnit)
        /// </summary>
        public float BrushSize
        {
            get { return brushSize; }
            set
            {
                value = Mathf.Clamp(value, MinBrushSize, MaxBrushSize);

                if (!MathEx.AlmostEqual(brushSize, value))
                {
                    brushSize = value;

                    EditorPrefs.SetFloat("MTE_TextureArrayPainter.brushSize", value);
                    if (PainterMode == EditorFilterMode.FilteredGameObjects)
                    {
                        preview.SetPreviewSize(BrushSizeInU3D/2);
                    }
                    else
                    {
                        //preview size for SelectedGameObject mode are set in OnSceneGUI
                    }
                }
            }
        }

        //real brush size
        private float BrushSizeInU3D { get { return BrushSize * Settings.BrushUnit; } }

        /// <summary>
        /// Brush flow
        /// </summary>
        public float BrushFlow
        {
            get { return brushFlow; }
            set
            {
                value = Mathf.Clamp(value, MinBrushFlow, MaxBrushFlow);
                if (Mathf.Abs(brushFlow - value) > 0.0001f)
                {
                    brushFlow = value;
                    EditorPrefs.SetFloat("MTE_TextureArrayPainter.brushFlow", value);
                }
            }
        }

        private float brushDirection = 0;
        /// <summary>
        /// Brush direction, angle to north(+z)
        /// </summary>
        public float BrushDirection
        {
            get
            {
                return this.brushDirection;
            }

            set
            {
                value = Mathf.Clamp(value, 0, 2 * Mathf.PI);
                if (!MathEx.AlmostEqual(value, this.brushDirection))
                {
                    EditorPrefs.SetFloat($"MTE_{nameof(TextureArrayPainter)}.brushDirection", this.brushDirection);
                    this.brushDirection = value;
                }
            }
        }
        #endregion

        #region UI
        private static readonly GUIContent[] EditorFilterModeContents =
        {
            new GUIContent(StringTable.Get(C.SplatPainter_Mode_Filtered),
                StringTable.Get(C.SplatPainter_Mode_FilteredDescription)),
            new GUIContent(StringTable.Get(C.SplatPainter_Mode_Selected),
                StringTable.Get(C.SplatPainter_Mode_SelectedDescription)),
        };
        #endregion
        
        public TextureArrayPainter()
        {
            MTEContext.EnableEvent += (sender, args) =>
            {
                if (MTEContext.editor == this)
                {
                    LoadSavedParameter();
                    LoadTextureList();
                    if (PainterMode == EditorFilterMode.SelectedGameObject)
                    {
                        BuildEditingInfoForLegacyMode(Selection.activeGameObject);
                    }
                    if (TextureList.Count != 0)
                    {
                        if (SelectedTextureIndex < 0)
                        {
                            SelectedTextureIndex = 0;
                        }
                        LoadPreview();
                    }
                }
            };

            MTEContext.EditTypeChangedEvent += (sender, args) =>
            {
                if (MTEContext.editor == this)
                {
                    LoadSavedParameter();
                    LoadTextureList();
                    if (PainterMode == EditorFilterMode.SelectedGameObject)
                    {
                        BuildEditingInfoForLegacyMode(Selection.activeGameObject);
                    }
                    if (TextureList.Count != 0)
                    {
                        if (SelectedTextureIndex < 0 || SelectedTextureIndex > TextureList.Count - 1)
                        {
                            SelectedTextureIndex = 0;
                        }
                        LoadPreview();
                    }
                }
                else
                {
                    if (preview != null)
                    {
                        preview.UnLoadPreview();
                    }
                }
            };

            MTEContext.SelectionChangedEvent += (sender, args) =>
            {
                if (args.SelectedGameObject)
                {
                    if (PainterMode == EditorFilterMode.SelectedGameObject)
                    {
                        BuildEditingInfoForLegacyMode(args.SelectedGameObject);
                    }
                }
            };

            MTEContext.TextureChangedEvent += (sender, args) =>
            {
                if (MTEContext.editor == this)
                {
                    LoadTextureList();
                    if (PainterMode == EditorFilterMode.SelectedGameObject)
                    {
                        BuildEditingInfoForLegacyMode(Selection.activeGameObject);
                    }
                }
            };

            MTEContext.DisableEvent += (sender, args) =>
            {
                if (preview != null)
                {
                    preview.UnLoadPreview();
                }
            };

            MTEContext.EditTargetsLoadedEvent += (sender, args) =>
            {
                if (MTEContext.editor == this)
                {
                    LoadTextureList();
                }
            };
            
            // Load default parameters
            painterMode = DefaultPainterMode;
            brushSize = DefaultBrushSize;
            brushFlow = DefaultBrushFlow;
            brushDirection = DefaultBrushDirection;
        }

        private void LoadPreview()
        {
            if (TextureList == null || TextureList.Count == 0)
            {
                return;
            }

            var texture = TextureList[SelectedTextureIndex];
            preview.LoadPreview(texture, BrushSizeInU3D, BrushIndex, PainterMode);
            preview.SetRotation(BrushDirection);
        }

        private void LoadSavedParameter()
        {
            painterMode = (EditorFilterMode)EditorPrefs.GetInt("MTE_TextureArrayPainter.painterMode", (int)DefaultPainterMode);
            brushSize = EditorPrefs.GetFloat("MTE_TextureArrayPainter.brushSize", DefaultBrushSize);
            brushFlow = EditorPrefs.GetFloat("MTE_TextureArrayPainter.brushFlow", DefaultBrushFlow);
            brushDirection = EditorPrefs.GetFloat($"MTE_TextureArrayPainter.brushDirection", DefaultBrushDirection);
        }

        private GameObject targetGameObject { get; set; }
        private Mesh targetMesh { get; set; }
        private Material targetMaterial { get; set; }
        private Texture2D[] controlTextures { get; } = new Texture2D[3] {null, null, null};
        private void BuildEditingInfoForLegacyMode(GameObject gameObject)
        {
            //reset
            this.TextureList.Clear();
            this.targetGameObject = null;
            this.targetMaterial = null;
            this.targetMesh = null;

            //check gameObject
            if (!gameObject)
            {
                return;
            }
            if (PainterMode != EditorFilterMode.SelectedGameObject)
            {
                return;
            }
            var meshFilter = gameObject.GetComponent<MeshFilter>();
            if (!meshFilter)
            {
                return;
            }
            var meshRenderer = gameObject.GetComponent<MeshRenderer>();
            if (!meshRenderer)
            {
                return;
            }
            var material = meshRenderer.sharedMaterial;
            if (!material)
            {
                return;
            }
            if (!MTEShaders.IsMTETextureArrayShader(material.shader))
            {
                return;
            }

            //collect targets info
            this.targetGameObject = gameObject;
            this.targetMaterial = material;
            this.targetMesh = meshFilter.sharedMesh;
            // Texture
            LoadTextureList();
            LoadControlTextures();
            // Preview
            if (TextureList.Count != 0)
            {
                if (SelectedTextureIndex < 0 || SelectedTextureIndex > TextureList.Count - 1)
                {
                    SelectedTextureIndex = 0;
                }
                preview.LoadPreview(TextureList[SelectedTextureIndex], BrushSizeInU3D, BrushIndex, PainterMode);
            }
        }
        
        private static class Styles
        {
            public static string NoGameObjectSelectedHintText;

            private static bool unloaded= true;

            public static void Init()
            {
                if (!unloaded) return;
                NoGameObjectSelectedHintText
                    = StringTable.Get(C.Info_SelectAGameObjectWithValidMesh);
                unloaded = false;
            }
        }

        public void DoArgsGUI()
        {
            Styles.Init();
            
            EditorGUI.BeginChangeCheck();
            this.PainterMode = (EditorFilterMode)GUILayout.Toolbar(
                (int)this.PainterMode, EditorFilterModeContents);
            if (EditorGUI.EndChangeCheck())
            {
                LoadTextureList();
                if (PainterMode == EditorFilterMode.SelectedGameObject)
                {
                    BuildEditingInfoForLegacyMode(Selection.activeGameObject);
                }
                LoadPreview();
            }
            if (PainterMode == EditorFilterMode.SelectedGameObject
                && Selection.activeGameObject == null)
            {
                EditorGUILayout.HelpBox(Styles.NoGameObjectSelectedHintText, MessageType.Warning);
                return;
            }

            BrushIndex = Utility.ShowBrushes(BrushIndex);

            // Splat-textures
            if (!Settings.CompactGUI)
            {
                GUILayout.Label(StringTable.Get(C.Textures), MTEStyles.SubHeader);
            }
            EditorGUILayout.BeginVertical("box");
            {
                var textureListCount = TextureList.Count;
                if (textureListCount == 0)
                {                    
                    if (PainterMode == EditorFilterMode.FilteredGameObjects)
                    {
                        EditorGUILayout.LabelField(
                            StringTable.Get(C.Info_SplatPainter_NoSplatTextureFound),
                            GUILayout.Height(64));
                        //TODO use texture-array version message
                    }
                    else
                    {
                        EditorGUILayout.LabelField(
                            StringTable.Get(C.Info_TextureArrayPainter_NoSplatTextureFoundOnSelectedObject),
                            GUILayout.Height(64));
                    }
                }
                else
                {
                    for (int i = 0; i < textureListCount; i += 4)
                    {
                        EditorGUILayout.BeginHorizontal();
                        {
                            var oldBgColor = GUI.backgroundColor;
                            for (int j = 0; j < 4; j++)
                            {
                                if (i + j >= textureListCount) break;

                                EditorGUILayout.BeginVertical();
                                var texture = TextureList[i + j];
                                bool toggleOn = SelectedTextureIndex == i + j;
                                if (toggleOn)
                                {
                                    GUI.backgroundColor = new Color(62 / 255.0f, 125 / 255.0f, 231 / 255.0f);
                                }

                                GUIContent toggleContent;
                                if (i + j + 1 <= MaxHotkeyNumberForTexture)
                                {
                                    toggleContent = new GUIContent(texture,
                                        StringTable.Get(C.Hotkey) + ':' + StringTable.Get(C.NumPad) + (i + j + 1));
                                }
                                else
                                {
                                    toggleContent = new GUIContent(texture);
                                }

                                var new_toggleOn = GUILayout.Toggle(toggleOn,
                                    toggleContent, GUI.skin.button,
                                    GUILayout.Width(64), GUILayout.Height(64));
                                GUI.backgroundColor = oldBgColor;
                                if (new_toggleOn && !toggleOn)
                                {
                                    SelectedTextureIndex = i + j;
                                    preview.LoadPreview(texture, BrushSizeInU3D, BrushIndex, PainterMode);
                                }

                                var toggleRect = GUILayoutUtility.GetLastRect();
                                var rect = toggleRect;
                                rect.min += new Vector2(5, 5);
                                rect.max -= new Vector2(5, 5);
                                var textRect = toggleRect;
                                var rectMin = textRect.min;
                                rectMin.y = rect.max.y - EditorStyles.miniBoldLabel.lineHeight;
                                textRect.min = rectMin;
                                GUI.Label(textRect, texture.name, EditorStyles.miniBoldLabel);
                                EditorGUILayout.EndVertical();
                            }
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                }
            }
            EditorGUILayout.EndVertical();

            //Settings
            if (!Settings.CompactGUI)
            {
                EditorGUILayout.Space();
                GUILayout.Label(StringTable.Get(C.Settings), MTEStyles.SubHeader);
            }
            BrushSize = EditorGUILayoutEx.Slider(StringTable.Get(C.Size), "[", "]", BrushSize, MinBrushSize, MaxBrushSize);
            BrushFlow = EditorGUILayoutEx.SliderLog10(StringTable.Get(C.Flow), "-", "+", BrushFlow, MinBrushFlow, MaxBrushFlow);
            if (!Settings.CompactGUI)
            {
                EditorGUILayout.BeginHorizontal();
                {
                    var label = new GUIContent(StringTable.Get(C.Direction));
                    var size = GUIStyle.none.CalcSize(label);
                    EditorGUILayout.LabelField(label, GUILayout.Width(size.x + 10), GUILayout.MinWidth(60));

                    EditorGUILayout.BeginVertical();
                    {
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Label($"{Mathf.Rad2Deg * BrushDirection:F2}°", GUILayout.ExpandWidth(false));
                        if (GUILayout.Button("0", GUILayout.MinWidth(80), GUILayout.ExpandWidth(false)))
                        {
                            BrushDirection = 0;
                            preview.SetRotation(BrushDirection);
                        }
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.HelpBox(StringTable.Get(C.Info_HowToRotate), MessageType.Info);
                    }
                    EditorGUILayout.EndVertical();
                }
                EditorGUILayout.EndHorizontal();
            }

            //Tools
            if (!Settings.CompactGUI)
            {
                EditorGUILayout.Space();
                GUILayout.Label(StringTable.Get(C.Tools), MTEStyles.SubHeader);
            }
            EditorGUILayout.BeginVertical();
            {
                EditorGUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button(StringTable.Get(C.CreateTextureArraySettings),
                        GUILayout.Width(100), GUILayout.Height(40)))
                    {
                        EditorApplication.ExecuteMenuItem(
                            $"Assets/Create/Mesh Terrain Editor/{nameof(TextureArraySettings)}");
                    }
                    GUILayout.Space(20);
                    EditorGUILayout.LabelField(
                        StringTable.Get(C.Info_ToolDescription_CreateTextureArraySettings),
                        MTEStyles.labelFieldWordwrap);
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();

            GUILayout.FlexibleSpace();
            EditorGUILayout.HelpBox(StringTable.Get(C.Info_WillBeSavedInstantly),
                MessageType.Info, true);
        }
        
        public HashSet<Hotkey> DefineHotkeys()
        {
            var hashSet = new HashSet<Hotkey>
            {
                new Hotkey(this, KeyCode.Minus, () =>
                {
                    BrushFlow -= 0.01f;
                    MTEEditorWindow.Instance.Repaint();
                }),
                new Hotkey(this, KeyCode.Equals, () =>
                {
                    BrushFlow += 0.01f;
                    MTEEditorWindow.Instance.Repaint();
                }),
                new Hotkey(this, KeyCode.LeftBracket, () =>
                {
                    BrushSize -= 1;
                    MTEEditorWindow.Instance.Repaint();
                }),
                new Hotkey(this, KeyCode.RightBracket, () =>
                {
                    BrushSize += 1;
                    MTEEditorWindow.Instance.Repaint();
                }),
            };

            for (int i = 0; i < MaxHotkeyNumberForTexture; i++)
            {
                int index = i;
                var hotkey = new Hotkey(this, KeyCode.Keypad0+index+1, () =>
                {
                    SelectedTextureIndex = index;
                    LoadPreview();
                });
                hashSet.Add(hotkey);
            }

            return hashSet;
        }

        // buffers of editing helpers
        private readonly List<TextureModifyGroup> modifyGroups = new List<TextureModifyGroup>(4);
        private float[] BrushStrength = new float[1024 * 1024];//buffer for brush blending to forbid re-allocate big array every frame when painting.
        private readonly List<Color[]> modifyingSections = new List<Color[]>(3);
        private UndoTransaction currentUndoTransaction;

        public void OnSceneGUI()
        {
            var e = Event.current;

            if (preview == null || !preview.IsReady || TextureList.Count == 0)
            {
                return;
            }

            if (e.commandName == "UndoRedoPerformed")
            {
                SceneView.RepaintAll();
                return;
            }

            if (!(EditorWindow.mouseOverWindow is SceneView))
            {
                return;
            }

            if(e.control && Settings.DebugMode)
            {
                RaycastHit hit;
                Ray ray1 = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                if (Physics.Raycast(ray1, out hit,
                        Mathf.Infinity,
                        1 << MTEContext.TargetLayer//only hit target layer
                    ))
                {
                    //check tag
                    if (!hit.transform.CompareTag(MTEContext.TargetTag))
                    {
                        return;
                    }

                    Handles.ArrowHandleCap(0, hit.point,
                        Quaternion.Euler(0, BrushDirection * Mathf.Rad2Deg, 0),
                        Utility.GetHandleSize(hit.point), EventType.Repaint);
                }
            }

            if (e.control && !e.isKey && e.type == EventType.ScrollWheel)
            {
                float oldDirection = BrushDirection;
                float direction = oldDirection;
                ChangeDirection(e.delta.y, ref direction);

                if (Mathf.Abs(direction - oldDirection) > Mathf.Epsilon)
                {
                    MTEEditorWindow.Instance.Repaint();
                    BrushDirection = direction;
                    preview.SetRotation(BrushDirection);
                }
                e.Use();
            }

            // do nothing when mouse middle/right button, control/alt key is pressed
            if (e.button != 0 || e.control || e.alt)
                return;

            HandleUtility.AddDefaultControl(0);
            var ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            RaycastHit raycastHit;
            
            if (PainterMode == EditorFilterMode.SelectedGameObject)
            {
                if (!targetGameObject || !targetMaterial || !targetMesh)
                {
                    return;
                }

                if (!Physics.Raycast(ray, out raycastHit, Mathf.Infinity, ~targetGameObject.layer))
                {
                    return;
                }

                if (targetGameObject != raycastHit.transform.gameObject)
                {
                    return;
                }
            
                var currentBrushSize = BrushSizeInU3D/2;

                if (Settings.ShowBrushRect)
                {
                    Utility.ShowBrushRect(raycastHit.point, currentBrushSize);
                }

                var controlIndex = SelectedTextureIndex / 4;
                Debug.Assert(0 <= controlIndex && controlIndex <= 3);
                var controlTexture = controlTextures[controlIndex];
                if (controlTexture == null)
                {
                    throw new MTEEditException("The control texture at index {controlIndex} is null.");
                }
                var controlWidth = controlTexture.width;
                var controlHeight = controlTexture.height;
                var meshSize = targetGameObject.GetComponent<MeshRenderer>().bounds.size.x;
                preview.SetNormalizedBrushSize(BrushSizeInU3D/meshSize);
                preview.SetNormalizedBrushCenter(raycastHit.textureCoord);
                preview.SetPreviewSize(BrushSizeInU3D/2);
                preview.MoveTo(raycastHit.point);
                SceneView.RepaintAll();

                if ((e.type == EventType.MouseDrag && e.alt == false && e.shift == false && e.button == 0) ||
                    (e.type == EventType.MouseDown && e.shift == false && e.alt == false && e.button == 0))
                {
                    if (e.type == EventType.MouseDown)
                    {
                        using (new UndoTransaction())
                        {
                            var material = targetMaterial;
                            if (material.HasProperty(ControlTexturePropertyNames[0]))
                            {
                                Texture2D texture = (Texture2D) material.GetTexture(ControlTexturePropertyNames[0]);
                                if (texture != null)
                                {
                                    var originalColors = texture.GetPixels();
                                    UndoRedoManager.Instance().Push(a =>
                                    {
                                        texture.ModifyPixels(a);
                                        texture.Apply();
                                        Save(texture);
                                    }, originalColors, "Paint control texture");
                                }
                            }

                            if (material.HasProperty(ControlTexturePropertyNames[1]))
                            {
                                Texture2D texture = (Texture2D) material.GetTexture(ControlTexturePropertyNames[1]);
                                if (texture != null)
                                {
                                    var originalColors = texture.GetPixels();
                                    UndoRedoManager.Instance().Push(a =>
                                    {
                                        texture.ModifyPixels(a);
                                        texture.Apply();
                                        Save(texture);
                                    }, originalColors, "Paint control texture");
                                }
                            }

                            if (material.HasProperty(ControlTexturePropertyNames[2]))
                            {
                                Texture2D texture = (Texture2D) material.GetTexture(ControlTexturePropertyNames[2]);
                                if (texture != null)
                                {
                                    var originalColors = texture.GetPixels();
                                    UndoRedoManager.Instance().Push(a =>
                                    {
                                        texture.ModifyPixels(a);
                                        texture.Apply();
                                        Save(texture);
                                    }, originalColors, "Paint control texture");
                                }
                            }
                        }
                    }

                    //brushSize in control texture's texel unit on mesh surface in world space
                    var brushSizeInTexel = (int)Mathf.Round(BrushSizeInU3D / meshSize * controlWidth);
                    var pixelUV = raycastHit.textureCoord;
                    var pX = Mathf.FloorToInt(pixelUV.x * controlWidth);
                    var pY = Mathf.FloorToInt(pixelUV.y * controlHeight);
                    
                    //texel rects, in unit of control texture texel, origin is control texture's first texel at uv (0,0)
                    var controlTextureTexelRect = new Rect(0, 0, controlWidth, controlHeight);
                    var maskTextureTexelRect = new Rect(pX - brushSizeInTexel / 2.0f, pY - brushSizeInTexel / 2.0f,
                        brushSizeInTexel, brushSizeInTexel);

                    // get modifying texel rect of control textures
                    // In single selected mode, choose texels around the hit point.
                    Rect texelRect;
                    if (!RectEx.Intersects(controlTextureTexelRect, maskTextureTexelRect, out texelRect))
                    {
                        return;
                    }

                    // get involved uv rect of brush mask texture
                    // In single selected mode, choose uv section around the hit point extended by brush size.
                    var brushUVMin = MathEx.NormalizeTo01(rangeMin: maskTextureTexelRect.min, rangeMax: maskTextureTexelRect.max, value: texelRect.min);
                    var brushUVMax = MathEx.NormalizeTo01(rangeMin: maskTextureTexelRect.min, rangeMax: maskTextureTexelRect.max, value: texelRect.max);

                    PaintTexture(controlTextures[0],
                        controlTextures[1],
                        controlTextures[2],
                        SelectedTextureIndex,
                        TextureList.Count,
                        texelRect,
                        brushUVMin, brushUVMax);
                }
                else if (e.type == EventType.MouseUp && e.alt == false && e.button == 0)
                {
                    foreach (var texture in controlTextures)
                    {
                        if (texture)
                        {
                            Save(texture);
                        }
                    }
                }

            }
            else if(PainterMode == EditorFilterMode.FilteredGameObjects)
            {
                if(Physics.Raycast(ray, out raycastHit,
                    Mathf.Infinity,
                    1 << MTEContext.TargetLayer//only hit target layer
                ))
                {
                    //check tag
                    if (!raycastHit.transform.CompareTag(MTEContext.TargetTag))
                    {
                        return;
                    }
                    var currentBrushSize = BrushSizeInU3D;

                    if (Settings.ShowBrushRect)
                    {
                        Utility.ShowBrushRect(raycastHit.point, currentBrushSize/2);
                    }

                    var hitPoint = raycastHit.point;
                    preview.MoveTo(hitPoint);

                    float meshSize = 1.0f;

                    // collect modify group
                    modifyGroups.Clear();
                    foreach (var target in MTEContext.Targets)
                    {
                        //MTEDebug.Log("Check if we can paint on target.");
                        var meshRenderer = target.GetComponent<MeshRenderer>();
                        if (meshRenderer == null) continue;
                        var meshFilter = target.GetComponent<MeshFilter>();
                        if (meshFilter == null) continue;
                        var mesh = meshFilter.sharedMesh;
                        if (mesh == null) continue;

                        Vector2 textureUVMin;//min texture uv that is to be modified
                        Vector2 textureUVMax;//max texture uv that is to be modified
                        Vector2 brushUVMin;//min brush mask uv that will be used
                        Vector2 brushUVMax;//max brush mask uv that will be used
                        {
                            //MTEDebug.Log("Start: Check if they intersect with each other.");
                            // check if the brush rect intersects with the `Mesh.bounds` of this target
                            var hitPointLocal = target.transform.InverseTransformPoint(hitPoint);//convert hit point from world space to target mesh space

                            Bounds brushBounds = new Bounds(center: new Vector3(hitPointLocal.x, 0, hitPointLocal.z), size: new Vector3(currentBrushSize, 99999, currentBrushSize));
                            Bounds meshBounds = mesh.bounds;

                            Bounds paintingBounds;
                            var intersected = meshBounds.Intersect(brushBounds, out paintingBounds);
                            if(!intersected) continue;

                            Vector2 paintingBounds2D_min = new Vector2(paintingBounds.min.x, paintingBounds.min.z);
                            Vector2 paintingBounds2D_max = new Vector2(paintingBounds.max.x, paintingBounds.max.z);

                            //calculate which part of control texture should be modified
                            Vector2 meshRendererBounds2D_min = new Vector2(meshBounds.min.x, meshBounds.min.z);
                            Vector2 meshRendererBounds2D_max = new Vector2(meshBounds.max.x, meshBounds.max.z);
                            textureUVMin = MathEx.NormalizeTo01(rangeMin: meshRendererBounds2D_min, rangeMax: meshRendererBounds2D_max, value: paintingBounds2D_min);
                            textureUVMax = MathEx.NormalizeTo01(rangeMin: meshRendererBounds2D_min, rangeMax: meshRendererBounds2D_max, value: paintingBounds2D_max);

                            if (target.transform == raycastHit.transform)
                            {
                                meshSize = meshBounds.size.x;
                            }

                            //calculate which part of brush mask texture should be used
                            Vector2 brushBounds2D_min = new Vector2(brushBounds.min.x, brushBounds.min.z);
                            Vector2 brushBounds2D_max = new Vector2(brushBounds.max.x, brushBounds.max.z);
                            brushUVMin = MathEx.NormalizeTo01(rangeMin: brushBounds2D_min, rangeMax: brushBounds2D_max, value: paintingBounds2D_min);
                            brushUVMax = MathEx.NormalizeTo01(rangeMin: brushBounds2D_min, rangeMax: brushBounds2D_max, value: paintingBounds2D_max);

                            if (Settings.DebugMode)
                            {
                                Handles.color = Color.blue;
                                HandlesEx.DrawRectangle(paintingBounds2D_min, paintingBounds2D_max);
                                Handles.color = new Color(255, 128, 166);
                                HandlesEx.DrawRectangle(meshRendererBounds2D_min, meshRendererBounds2D_max);
                                Handles.color = Color.green;
                                HandlesEx.DrawRectangle(brushBounds2D_min, brushBounds2D_max);
                            }
                            //MTEDebug.Log("End: Check if they intersect with each other.");
                        }

                        if (e.button == 0 && (e.type == EventType.MouseDown || e.type == EventType.MouseDrag))
                        {
                            //MTEDebug.Log("Start handling mouse down.");
                            // find the splat-texture in the material, get the X (splatIndex) from `_SplatX`
                            var selectedTexture = TextureList[SelectedTextureIndex];
                            var material = meshRenderer.sharedMaterial;
                            if (material == null)
                            {
                                MTEDebug.LogError("Failed to find material on target GameObject's MeshRenderer. " +
                                                  "The first material on the MeshRenderer should be editable by MTE.");
                                return;
                            }

                            //MTEDebug.Log("Finding the selected texture in the material.");
                            //Convention: for texture array, the selected texture index is the layer index
                            if (!material.HasProperty(AlbedoArrayPropertyName))
                            {
                                //zwcloud/MeshTerrainEditor-issues#218
                                var relativePath = AssetDatabase.GetAssetPath(material);
                                MTEDebug.LogError(
                                    $"Material<{material.name}> at <{relativePath}> using shader <{material.shader.name}> doesn't have a texture property" +
                                    $" '{AlbedoArrayPropertyName}'. Please capture a screenshot of the material editor, and report this issue with it.");
                                return;
                            }
                            Texture2DArray textureArray =
                                material.GetTexture(AlbedoArrayPropertyName) as Texture2DArray;
                            var layerIndex = TextureArrayManager.Instance.GetTextureSliceIndex(textureArray,
                                selectedTexture);
                            if (layerIndex < 0)
                            {
                                continue;
                            }

                            //MTEDebug.Log("get number of layer-textures in the material.");
                            int splatTotal = GetLayerTextureNumber(material);

                            //MTEDebug.Log("check control textures.");

                            // fetch control textures from material, TODO refactor to merge duplicate code below
                            Texture2D controlTexture0 = null, controlTexture1 = null, controlTexture2 = null;
                            if (splatTotal > 0)//controlTexture0 should exists
                            {
                                if (!material.HasProperty(ControlTexturePropertyNames[0]))
                                {//impossible if using a builtin TextureArray shader
                                    throw new MTEEditException($"Property {ControlTexturePropertyNames[0]} " +
                                        $"doesn't exist in material<{material.name}>.");
                                }
                                var tex = material.GetTexture(ControlTexturePropertyNames[0]);
                                if (tex != null)
                                {
                                    controlTexture0 = (Texture2D)tex;
                                }
                                else
                                {
                                    throw new MTEEditException($"{ControlTexturePropertyNames[0]} is" +
                                        $" not assigned or existing in material<{material.name}>.");
                                }
                            }

                            if (splatTotal > 4)//controlTexture1 should exists
                            {
                                if (!material.HasProperty(ControlTexturePropertyNames[1]))
                                {//impossible if using a builtin TextureArray shader
                                    throw new MTEEditException($"Property {ControlTexturePropertyNames[1]} " +
                                        $"doesn't exist in material<{material.name}>.");
                                }
                                var tex = material.GetTexture(ControlTexturePropertyNames[1]);
                                if (tex == null)
                                {
                                    throw new MTEEditException($"Property {ControlTexturePropertyNames[1]} " +
                                        $"is not assigned in material<{material.name}>.");
                                }
                                controlTexture1 = (Texture2D)tex;
                            }

                            if (splatTotal > 8)//controlTexture2 should exists
                            {
                                if (!material.HasProperty(ControlTexturePropertyNames[2]))
                                {//impossible if using a builtin TextureArray shader
                                    throw new MTEEditException($"Property {ControlTexturePropertyNames[2]} " +
                                        $"doesn't exist in material<{material.name}>.");
                                }
                                var tex = material.GetTexture(ControlTexturePropertyNames[2]);
                                if (tex == null)
                                {
                                    throw new MTEEditException($"{ControlTexturePropertyNames[2]} " +
                                        $"is not assigned in material<{material.name}>.");
                                }
                                controlTexture2 = (Texture2D)tex;
                            }

                            // check which control texture is to be modified
                            Texture2D controlTexture = controlTexture0;
                            if (layerIndex >= 4)
                            {
                                controlTexture = controlTexture1;
                            }
                            if (layerIndex >= 8)
                            {
                                controlTexture = controlTexture2;
                            }
                            if (controlTexture1 != null)
                            {
                                if (controlTexture0.width != controlTexture1.width)
                                {
                                    throw new MTEEditException(
                                        $"Size of {controlTexture0.name} is different from other control textures." +
                                        "Make sure all control textures is the same size.");
                                }
                            }
                            if (controlTexture2 != null)
                            {
                                if (controlTexture0.width != controlTexture2.width)
                                {
                                    throw new MTEEditException(
                                        $"Size of {controlTexture2.name} is different from other control textures." +
                                        "Make sure all control textures is the same size.");
                                }
                            }
                            Debug.Assert(controlTexture != null, "controlTexture != null");

                            //get modifying texel rect of the control texture
                            int x = (int)Mathf.Clamp(textureUVMin.x * (controlTexture.width - 1), 0, controlTexture.width - 1);
                            int y = (int)Mathf.Clamp(textureUVMin.y * (controlTexture.height - 1), 0, controlTexture.height - 1);
                            int width = Mathf.Clamp(Mathf.FloorToInt(textureUVMax.x * controlTexture.width) - x, 0, controlTexture.width - x);
                            int height = Mathf.Clamp(Mathf.FloorToInt(textureUVMax.y * controlTexture.height) - y, 0, controlTexture.height - y);

                            var texelRect = new Rect(x, y, width, height);
                            modifyGroups.Add(new TextureModifyGroup(target, layerIndex, splatTotal,
                                controlTexture0, controlTexture1, controlTexture2,
                                texelRect, brushUVMin, brushUVMax));

                            //MTEDebug.Log("End handling mouse down.");
                        }
                    }

                    preview.SetNormalizedBrushSize(BrushSizeInU3D/meshSize);
                    preview.SetNormalizedBrushCenter(raycastHit.textureCoord);

                    //record undo operation for targets that to be modified
                    if (e.button == 0 && e.type == EventType.MouseDown)
                    {
                        currentUndoTransaction = new UndoTransaction("Paint Texture");
                    }
                    
                    if (currentUndoTransaction != null && 
                       e.button == 0 && e.type== EventType.MouseDown)
                    {
                        //record values before modification for undo
                        foreach (var modifyGroup in modifyGroups)
                        {
                            var gameObject = modifyGroup.gameObject;
                            var material = gameObject.GetComponent<MeshRenderer>().sharedMaterial;
                            if (material.HasProperty(ControlTexturePropertyNames[0]))
                            {
                                Texture2D texture = (Texture2D)material.GetTexture(ControlTexturePropertyNames[0]);
                                if (texture != null)
                                {
                                    var originalColors = texture.GetPixels();
                                    UndoRedoManager.Instance().Push(a =>
                                    {
                                        texture.ModifyPixels(a);
                                        texture.Apply();
                                        Save(texture);
                                    }, originalColors, "Paint control texture");
                                }
                            }
                            if (material.HasProperty(ControlTexturePropertyNames[1]))
                            {
                                Texture2D texture = (Texture2D) material.GetTexture(ControlTexturePropertyNames[1]);
                                if (texture != null)
                                {
                                    var originalColors = texture.GetPixels();
                                    UndoRedoManager.Instance().Push(a =>
                                    {
                                        texture.ModifyPixels(a);
                                        texture.Apply();
                                        Save(texture);
                                    }, originalColors, "Paint control texture");
                                }
                            }
                            if (material.HasProperty(ControlTexturePropertyNames[2]))
                            {
                                Texture2D texture = (Texture2D) material.GetTexture(ControlTexturePropertyNames[2]);
                                if (texture != null)
                                {
                                    var originalColors = texture.GetPixels();
                                    UndoRedoManager.Instance().Push(a =>
                                    {
                                        texture.ModifyPixels(a);
                                        texture.Apply();
                                        Save(texture);
                                    }, originalColors, "Paint control texture");
                                }
                            }
                        }
                    }

                    if (e.button == 0 && e.type == EventType.MouseUp)
                    {
                        Debug.Assert(currentUndoTransaction != null);
                        currentUndoTransaction.Dispose();
                    }

                    // execute the modification
                    if (modifyGroups.Count != 0)
                    {
                        for (int i = 0; i < modifyGroups.Count; i++)
                        {
                            var modifyGroup = modifyGroups[i];
                            var gameObject = modifyGroup.gameObject;
                            var material = gameObject.GetComponent<MeshRenderer>().sharedMaterial;
                            //set all control textures readable, just in case
                            Utility.SetTextureReadable(material.GetTexture(ControlTexturePropertyNames[0]));
                            if (material.HasProperty(ControlTexturePropertyNames[1]))
                            {
                                Utility.SetTextureReadable(material.GetTexture(ControlTexturePropertyNames[1]));
                            }
                            if (material.HasProperty(ControlTexturePropertyNames[2]))
                            {
                                Utility.SetTextureReadable(material.GetTexture(ControlTexturePropertyNames[2]));
                            }
                            PaintTexture(modifyGroup.controlTexture0,
                                modifyGroup.controlTexture1,
                                modifyGroup.controlTexture2,
                                modifyGroup.splatIndex,
                                modifyGroup.splatTotal,
                                modifyGroup.texelRect,
                                modifyGroup.minUV, modifyGroup.maxUV);
                        }
                    }
                }
            }

            // auto save when mouse up
            if (e.type == EventType.MouseUp && e.button == 0)
            {
                foreach (var texture2D in DirtyTextureSet)
                {
                    Save(texture2D);
                }
                DirtyTextureSet.Clear();
            }

            SceneView.RepaintAll();
        }
        
        private void ChangeDirection(float delta, ref float direction)
        {
            if(delta > 0)
            {
                direction -= Mathf.PI / 12;
            }
            else if(delta < 0)
            {
                direction += Mathf.PI / 12;
            }

            if(direction < 0)
            {
                direction += 2*Mathf.PI;
            }
            if (direction > 2*Mathf.PI)
            {
                direction -= 2*Mathf.PI;
            }
        }

        private void PaintTexture(Texture2D controlTexture0, Texture2D controlTexture1,
             Texture2D controlTexture2, int splatIndex, int splatTotal, Rect texelRect,
             Vector2 minUV, Vector2 maxUV)
        {
            // check parameters
            if (splatTotal > 0 && controlTexture0 == null)
            {
                throw new System.ArgumentException(
                    $"[MTE] {nameof(controlTexture0)} is null.",
                    nameof(controlTexture0));
            }
            if (splatTotal > 4 && controlTexture1 == null)
            {
                throw new System.ArgumentException(
                    $"[MTE] splatIndex is 4/5/6/7 but {nameof(controlTexture1)} is null.",
                    nameof(controlTexture1));
            }
            if (splatTotal > 8 && controlTexture2 == null)
            {
                throw new System.ArgumentException(
                    $"[MTE] splatIndex is 8/9/10/11 but {nameof(controlTexture2)} is null.",
                    nameof(controlTexture2));
            }
            if (splatIndex < 0 || splatIndex > 11)
            {
                throw new System.ArgumentOutOfRangeException(nameof(splatIndex), splatIndex,
                    "[MTE] splatIndex should be [0, 11].");
            }

            // collect the pixel sections to modify
            modifyingSections.Clear();
            int x = Mathf.RoundToInt(texelRect.x);
            int y = Mathf.RoundToInt(texelRect.y);
            int width = Mathf.RoundToInt(texelRect.width);
            int height = Mathf.RoundToInt(texelRect.height);
            modifyingSections.Add(controlTexture0.GetPixels(x, y, width, height, 0));
            if (controlTexture1 != null)
            {
                modifyingSections.Add(controlTexture1.GetPixels(x, y, width, height, 0));
            }
            else
            {
                modifyingSections.Add(Array.Empty<Color>());
            }
            if (controlTexture2 != null)
            {
                modifyingSections.Add(controlTexture2.GetPixels(x, y, width, height, 0));
            }
            else
            {
                modifyingSections.Add(Array.Empty<Color>());
            }

            // sample brush strength from the mask texture
            var maskTexture = (Texture2D) MTEStyles.brushTextures[BrushIndex];
            if (BrushStrength.Length < width*height)//enlarge buffer if it is not big enough
            {
                BrushStrength = new float[width * height];
            }
            var unitUV_u = (maxUV.x - minUV.x)/(width-1);
            if (width == 1)
            {
                unitUV_u = maxUV.x - minUV.x;
            }
            var unitUV_v = (maxUV.y - minUV.y)/(height-1);
            if (height == 1)
            {
                unitUV_v = maxUV.y - minUV.y;
            }
            for (var i = 0; i < height; i++)
            {
                float maskV = minUV.y + i * unitUV_v;
                for (var j = 0; j < width; j++)
                {
                    var pixelIndex = i * width + j;
                    float maskU = minUV.x + j * unitUV_u;
                    var maskUV = MathEx.Rotate(new Vector2(maskU-0.5f, maskV-0.5f), BrushDirection)
                        + new Vector2(0.5f, 0.5f);
                    BrushStrength[pixelIndex] = maskTexture.GetPixelBilinear(maskUV.x, maskUV.y).a;
                }
            }

            // blend the pixel section
            Utility.BlendPixelSections(BrushFlow, BrushStrength, modifyingSections,
                splatIndex, splatTotal, height, width);

            // modify the control texture
            if (splatTotal > 8)
            {
                controlTexture0.SetPixels(x, y, width, height, modifyingSections[0]);
                controlTexture0.Apply();
                System.Diagnostics.Debug.Assert(controlTexture1 != null);
                System.Diagnostics.Debug.Assert(modifyingSections[1].Length != 0);
                controlTexture1.SetPixels(x, y, width, height, modifyingSections[1]);
                controlTexture1.Apply();
                System.Diagnostics.Debug.Assert(controlTexture2 != null);
                System.Diagnostics.Debug.Assert(modifyingSections[2].Length != 0);
                controlTexture2.SetPixels(x, y, width, height, modifyingSections[2]);
                controlTexture2.Apply();
                DirtyTextureSet.Add(controlTexture0);
                DirtyTextureSet.Add(controlTexture1);
                DirtyTextureSet.Add(controlTexture2);
            }
            else if(splatTotal > 4)
            {
                controlTexture0.SetPixels(x, y, width, height, modifyingSections[0]);
                controlTexture0.Apply();
                System.Diagnostics.Debug.Assert(controlTexture1 != null);
                System.Diagnostics.Debug.Assert(modifyingSections[1].Length != 0);
                controlTexture1.SetPixels(x, y, width, height, modifyingSections[1]);
                controlTexture1.Apply();
                DirtyTextureSet.Add(controlTexture0);
                DirtyTextureSet.Add(controlTexture1);
            }
            else
            {
                controlTexture0.SetPixels(x, y, width, height, modifyingSections[0]);
                controlTexture0.Apply();
                DirtyTextureSet.Add(controlTexture0);
            }
        }
        
        public static readonly HashSet<Texture2D> DirtyTextureSet = new HashSet<Texture2D>();

        private static void Save(Texture2D texture)
        {
            if(texture == null)
            {
                throw new System.ArgumentNullException("texture");
            }
            var path = AssetDatabase.GetAssetPath(texture);
            var bytes = texture.EncodeToPNG();
            if(bytes == null || bytes.Length == 0)
            {
                throw new System.Exception("[MTE] Failed to save texture to png file.");
            }
            File.WriteAllBytes(path, bytes);
            MTEDebug.LogFormat("Texture<{0}> saved to <{1}>.", texture.name, path);
        }

        private Preview preview = new Preview(isArray: true);

        //Don't modify this field, it's used by MTE editors internally
        public List<Texture> TextureList = new List<Texture>(16);

        /// <summary>
        /// load all splat textures form targets
        /// </summary>
        private void LoadTextureList()
        {
            TextureList.Clear();
            if (painterMode == EditorFilterMode.SelectedGameObject)
            {
                MTEDebug.Log("Loading layer textures on selected GameObject...");
                LoadTargetTextures(targetGameObject);
            }
            else
            {
                MTEDebug.Log("Loading layer textures on target GameObject(s)...");
                foreach (var target in MTEContext.Targets)
                {
                    LoadTargetTextures(target);
                }
            }

            // make collected splat textures readable
            Utility.SetTextureReadable(TextureList, true);
            MTEDebug.LogFormat("{0} layer textures loaded.", TextureList.Count);
        }

        private void LoadTargetTextures(GameObject target)
        {
            if (!target)
            {
                MTEDebug.LogWarning("Ignored an invalid/destroyed target.");
                return;
            }

            MTEDebug.Log($"Loading textures into TextureList from target<{target.name}>.");

            var meshRenderer = target.GetComponent<MeshRenderer>();
            if (meshRenderer == null)
            {
                MTEDebug.LogWarning($"Ignored target{target.name}: cannot find MeshRenderer on it.");
                return;
            }

            var material = meshRenderer.sharedMaterial;
            if (!material)
            {
                MTEDebug.LogWarning($"Ignored target{target.name}: MeshRenderer's material is invalid.");
                return;
            }

            if (!CheckIfMaterialAssetPathAvailable(material))
            {
                MTEDebug.LogWarning($"Ignored target{target.name}: Material<{material.name}> is a builtin asset, which is not supported.");
                return;
            }

            var shader = material.shader;
            if (shader == null)
            {
                MTEDebug.LogWarning($"Ignored target{target.name}: Material<{material.name}> doesn't use a valid shader!");
                return;
            }

            if (!MTEShaders.IsMTETextureArrayShader(shader))
            {
                MTEDebug.LogWarning($"Ignored target{target.name}: material<{material.name}>'s shader<{shader.name}> isn't a MTE TextureArray shader.");
                return;
            }
            
            TextureArraySettings settings = null;
            var runtimeTextureArrayLoader = target.GetComponent<RuntimeTextureArrayLoader>();
            if (runtimeTextureArrayLoader)
            {
                runtimeTextureArrayLoader.LoadInEditor();//create and assign texture array to material TODO SIDE EFFECT move this to a proper place
                settings = runtimeTextureArrayLoader.settings;
                MTEDebug.Log($"Loaded a {nameof(TextureArraySettings)} from {nameof(RuntimeTextureArrayLoader)} on target<{target.name}>.");
            }

            var textureArray = material.GetTextureArray0();
            if (textureArray == null)
            {
                MTEDebug.LogWarning($"Ignored target{target.name}: Material<{material.name}>'s property {TextureArray0PropertyName} should be assigned a texture array.");
                return;
            }

            if (!TextureArrayManager.Instance.AddOrUpdate(textureArray, settings))
            {
                MTEDebug.LogWarning($"Ignored target{target.name}: cannot load source textures of TextureArray<{textureArray.name}> from Material<{material.name}>. Turn on Debug mode to see the reason.");
                return;
            }
            TextureArrayManager.Instance.GetTextures(textureArray, out var textures);
            if (textures == null || textures.Count == 0)
            {
                MTEDebug.LogWarning($"Ignored target{target.name}: no textures loaded in TextureArray<{textureArray.name}> from Material<{material.name}>.");
                return;
            }
            
            MTEDebug.Log($"Adding textures to TextureList from target{target.name} material<{material.name}> TextureArray<{textureArray.name}>");
            bool added = false;
            foreach (var texture in textures)
            {
                if (!TextureList.Contains(texture))
                {
                    TextureList.Add(texture);
                    added = true;
                    MTEDebug.Log($"  Added texture<{texture.name}>");
                }
            }

            if (Settings.DebugMode)
            {
                if (!added)
                {
                    MTEDebug.Log(" No texture added from target because the textures being used has already been added.");
                }
            }
        }

        private void LoadControlTextures()
        {
            if (!targetMaterial)
            {
                return;
            }

            int splatTotal = GetLayerTextureNumber(targetMaterial);
            int weightMapTotal = splatTotal / 4;
            Debug.Assert(weightMapTotal <= ControlTexturePropertyNames.Length);

            int width = -1, height = -1;
            for (int weightMapIndex = 0; weightMapIndex < weightMapTotal; weightMapIndex++)
            {
                var controlPropertyName = ControlTexturePropertyNames[weightMapIndex];
                var controlTexture = targetMaterial.GetTexture(controlPropertyName) as Texture2D;
                if (controlTexture == null )
                {
                    throw new MTEEditException($"Property {controlPropertyName} isn't assigned " +
                        $"or doesn't exist in material<{targetMaterial.name}>.");
                }

                var controlTextureWidth = controlTexture.width;
                var controlTextureHeight = controlTexture.height;
                if (controlTextureWidth != controlTextureHeight)
                {
                    throw new MTEEditException($"{controlPropertyName} texture is not square.");
                }

                if (width < 0 && height < 0)
                {
                    width = controlTextureWidth;
                    height = controlTextureHeight;
                }
                if (width != controlTextureWidth || height != controlTextureHeight)
                {
                    throw new MTEEditException(
                        $"Size of {controlPropertyName} is different from others.");
                }

                controlTextures[weightMapIndex] = controlTexture;
            }
        }

        private static bool CheckIfMaterialAssetPathAvailable(Material material)
        {
            var relativePathOfMaterial = AssetDatabase.GetAssetPath(material);
            if (relativePathOfMaterial.StartsWith("Resources"))
            {//built-in material
                return false;
            }
            return true;
        }
        
        private static int GetLayerTextureNumber(Material material)
        {
            if (material.IsKeywordEnabled(KeyWords.HasWeightMap2))
            {
                return 12;
            }
            if (material.IsKeywordEnabled(KeyWords.HasWeightMap1))
            {
                return 8;
            }
            return 4;
        }
    }
}