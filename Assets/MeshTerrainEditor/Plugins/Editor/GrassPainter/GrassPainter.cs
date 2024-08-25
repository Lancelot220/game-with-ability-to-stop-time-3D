using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MTE
{
    internal partial class GrassPainter : IEditor
    {
        public int Id { get; } = 6;

        public bool Enabled { get; set; } = true;

        public string Name { get; } = "GrassPainter";

        public Texture Icon { get; } =
            EditorGUIUtility.IconContent("TerrainInspector.TerrainToolPlants").image;

        public bool WantMouseMove { get; } = false;

        public bool WillEditMesh { get; } = false;

        #region Parameters

        #region Constant
        // default
        const float DefaultBrushSize = 1;
        const float DefaultBrushDirection = 0;
        const bool DefaultUseRandomDirection = true;
        const float DefaultDistance = 0.1f;
        const int DefaultReduction = 100;
        const bool DefaultAllowOverlap = false;
        // min/max
        const float MinBrushSize = 0.01f;
        const float MaxBrushSize = 10f;
        private const int MinReduction = 1;
        private const int MaxReduction = 100;
        const float MinDistance = 0.01f;
        const float MaxDistance = 1f;
        // limit
        private const int MaxPositionNumber = 1000;
        #endregion

        private float brushSize;
        private float distance;
        private int reduction;
        private bool allowOverlap;

        private GrassDetail SelectedGrassDetail => detailList[SelectedGrassIndex];

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

                    EditorPrefs.SetFloat("MTE_GrassPainter.brushSize", value);
                }
            }
        }

        //real brush size
        private float BrushSizeInU3D { get { return BrushSize * Settings.BrushUnit; } }

        /// <summary>
        /// Distance between each grass
        /// </summary>
        public float Distance
        {
            get
            {
                return distance;
            }
            set
            {
                if (distance != value)
                {
                    distance = value;
                    EditorPrefs.SetFloat("MTE_GrassPainter.distance", value);
                }
            }
        }

        /// <summary>
        /// Removing strength (percent)
        /// </summary>
        public int Reduction
        {
            get
            {
                return this.reduction;
            }
            set
            {
                if (this.reduction != value)
                {
                    this.reduction = value;
                    EditorPrefs.SetInt("MTE_GrassPainter.reduction", value);
                }
            }
        }
        
        public bool AllowOverlap
        {
            get
            {
                return this.allowOverlap;
            }

            set
            {
                if (value != this.allowOverlap)
                {
                    this.allowOverlap = value;
                    EditorPrefs.SetBool("MTE_GrassPainter.allowOverlap", value);
                }
            }
        }

        /// <summary>
        /// Selected grass texture index
        /// </summary>
        public int SelectedGrassIndex
        {
            get;
            set;
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
                    EditorPrefs.SetFloat("MTE_GrassPainter.brushDirection", this.brushDirection);
                    this.brushDirection = value;
                }
            }
        }

        private bool useRandomDirection;

        /// <summary>
        ///
        /// </summary>
        public bool UseRandomDirection
        {
            get { return this.useRandomDirection; }
            set
            {
                if (value != useRandomDirection)
                {
                    useRandomDirection = value;
                    EditorPrefs.SetBool("MTE_GrassPainter.useRandomDirection", value);
                }
            }
        }

        #endregion

        public static GrassPainter Instance;

        private List<GrassDetail> detailList = null;
        internal void LoadGrassDetailList()
        {
            detailList = null;
            var grassLoader = Object.FindObjectOfType<GrassLoader>();
            if (grassLoader == null)
            {
                MTEDebug.LogWarning("Failed to load GrassDetailList: cannot find GrassLoader in scene.");//This is normal for a new scene.
                return;
            }
            var detailListAsset = grassLoader.grassDetailList;
            if (detailListAsset != null && detailListAsset.grassDetailList != null)
            {
                detailList = detailListAsset.grassDetailList;
                MTEDebug.Log($"GrassDetailList loaded from GrassLoader on GameObject {grassLoader.gameObject.name}");
            }
            else
            {
                MTEDebug.LogError("Cannot find Grass Detail list asset. Please create an Grass Detail asset and assign it to the GrassLoader.");
            }
        }

        public GrassPainter()
        {
            MTEContext.EnableEvent += (sender, args) =>
            {
                if (MTEContext.editor == this)
                {
                    LoadSavedParamter();
                    LoadGrassDetailList();
                    CheckIfCanAttachGrassLoader();
                    ForceReloadGrass();
                }
            };

            MTEContext.EditTypeChangedEvent += (sender, args) =>
            {
                if (MTEContext.editor == this)
                {
                    LoadSavedParamter();
                    LoadGrassDetailList();
                    CheckIfCanAttachGrassLoader();
                    ForceReloadGrass();
                }
            };

            MTEContext.EditTargetsLoadedEvent += (sender, args) =>
            {
                if (MTEContext.editor == this)
                {
                    LoadGrassDetailList();
                    ForceReloadGrass();
                }
            };

            MTEContext.SelectionChangedEvent += (sender, args) =>
            {
                if (MTEContext.editor == this)
                {
                    CheckIfCanAttachGrassLoader();
                }
            };

            MTEContext.MeshColliderUpdatedEvent += (sender, args) =>
            {
                UpdateAllGrasses();
            };

            // Load default parameters
            brushSize = DefaultBrushSize;
            distance = DefaultDistance;
            brushDirection = DefaultBrushDirection;
            useRandomDirection = DefaultUseRandomDirection;
            reduction = DefaultReduction;
            allowOverlap = DefaultAllowOverlap;

            GrassPainter.Instance = this;
        }

        private void ForceReloadGrass()
        {
            // force reload the grass loader
            var foundGrassLoader = MTEContext.TheGrassLoader;
            if (foundGrassLoader != null)
            {
                GrassEditorUtil.ReloadGrassesFromFile(foundGrassLoader);
            }
        }
        
        public HashSet<Hotkey> DefineHotkeys()
        {
            return new HashSet<Hotkey>
            {
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
                new Hotkey(this, KeyCode.Minus, () =>
                {
                    Distance -= 0.1f * (MaxDistance - MinDistance);
                    MTEEditorWindow.Instance.Repaint();
                }),
                new Hotkey(this, KeyCode.Equals, () =>
                {
                    Distance += 0.1f * (MaxDistance - MinDistance);
                    MTEEditorWindow.Instance.Repaint();
                })
            };
        }

        private void LoadSavedParamter()
        {
            // Load parameters from the EditorPrefs
            brushSize = EditorPrefs.GetFloat("MTE_GrassPainter.brushSize", DefaultBrushSize);
            brushDirection = EditorPrefs.GetFloat("MTE_GrassPainter.brushDirection", DefaultBrushDirection);
            useRandomDirection = EditorPrefs.GetBool("MTE_GrassPainter.useRandomDirection", DefaultUseRandomDirection);
            distance = EditorPrefs.GetFloat("MTE_GrassPainter.distance", DefaultDistance);
            reduction = EditorPrefs.GetInt("MTE_GrassPainter.reduction", DefaultReduction);
            allowOverlap = EditorPrefs.GetBool("MTE_GrassPainter.allowOverlap", DefaultAllowOverlap);
        }

        public string Header { get { return StringTable.Get(C.PaintGrass_Header); } }
        public string Description { get { return StringTable.Get(C.PaintGrass_Description); } }

        public void DoArgsGUI()
        {
            if (!MTEContext.TheGrassLoader)
            {
                EditorGUILayout.HelpBox(StringTable.Get(C.Warning_NoGrassLoader), MessageType.Warning);
                
                EditorGUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button(StringTable.Get(C.CreateGrassLoader), GUILayout.Width(100),
                        GUILayout.Height(40)))
                    {
                        CreateGrassContainer();
                    }
                    GUILayout.Space(20);
                    EditorGUILayout.LabelField(
                        StringTable.Get(C.Info_ToolDescription_CreateGrassLoader),
                        MTEStyles.labelFieldWordwrap);
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                {
                    GUI.enabled = CanAttachGrassLoader;
                    if (GUILayout.Button(StringTable.Get(C.AttachGrassLoader), GUILayout.Width(100), GUILayout.Height(40)))
                    {
                        AttachGrassLoader();
                    }
                    GUILayout.Space(20);
                    EditorGUILayout.LabelField(StringTable.Get(C.Info_ToolDescription_AttachGrassLoader), MTEStyles.labelFieldWordwrap);
                    GUI.enabled = true;
                    if (!CanAttachGrassLoader)
                    {
                        EditorGUILayout.BeginHorizontal();
                        {
                            var content = EditorGUIUtility.IconContent("console.warnicon");
                            content.tooltip = CannotAttachGrassReason;
                            GUILayout.Label(content, "button");
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                }
                EditorGUILayout.EndHorizontal();
                return;
            }

            // Grasses
            if (!Settings.CompactGUI)
            {
                GUILayout.Label(StringTable.Get(C.Grasses), MTEStyles.SubHeader);
            }

            // grass detail list
            SelectedGrassIndex = OnGrassDetailSelectorGUI(SelectedGrassIndex);

            //Settings
            if (!Settings.CompactGUI)
            {
                EditorGUILayout.Space();
                GUILayout.Label(StringTable.Get(C.Settings), MTEStyles.SubHeader);
            }
            BrushSize = EditorGUILayoutEx.SliderLog10(StringTable.Get(C.Size), "[", "]", BrushSize, MinBrushSize, MaxBrushSize);
            Distance = EditorGUILayoutEx.SliderLog10(StringTable.Get(C.Distance), "-", "+", Distance, MinDistance, MaxDistance);
            var estimatedSampleNumber = FastPoissonDiskSampling.EstimateSampleNumber(Vector2.one*BrushSizeInU3D*2, Distance);
            if (estimatedSampleNumber > MaxPositionNumber)
            {
                EditorGUILayout.HelpBox(StringTable.Get(C.Warning_TooManyPoints), MessageType.Warning);
            }
            if (!Settings.CompactGUI)
            {
                if (estimatedSampleNumber == 0)
                {
                    EditorGUILayout.HelpBox(StringTable.Get(C.Info_PaintOneGrass), MessageType.Info);
                }
                else
                {
                    EditorGUILayoutEx.Label(StringTable.Get(C.Number), estimatedSampleNumber.ToString());
                }
            }
            Reduction = EditorGUILayoutEx.IntSlider(StringTable.Get(C.Reduction), Reduction, MinReduction, MaxReduction);

            EditorGUILayout.BeginHorizontal();
            {
                var label = new GUIContent(StringTable.Get(C.Direction));
                var size = GUIStyle.none.CalcSize(label);
                EditorGUILayout.LabelField(label, GUILayout.Width(size.x + 10), GUILayout.MinWidth(60));

                EditorGUILayout.BeginVertical();
                UseRandomDirection = GUILayout.Toggle(UseRandomDirection, StringTable.Get(C.Random));
                if (!UseRandomDirection)
                {
                    EditorGUILayout.LabelField(string.Format("{0}°", Mathf.Rad2Deg * BrushDirection));
                    EditorGUILayout.HelpBox(StringTable.Get(C.Info_HowToRotate), MessageType.Info);
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            {
                var label = new GUIContent(StringTable.Get(C.AllowOverlap));
                var size = GUIStyle.none.CalcSize(label);
                EditorGUILayout.LabelField(label, GUILayout.Width(size.x + 10), GUILayout.MinWidth(60));
                AllowOverlap = EditorGUILayout.Toggle(AllowOverlap);
            }
            EditorGUILayout.EndHorizontal();

            // Tools
            if (!Settings.CompactGUI)
            {
                EditorGUILayout.Space();
                GUILayout.Label(StringTable.Get(C.Tools), MTEStyles.SubHeader);
            }

            EditorGUILayout.BeginVertical();
            {
                EditorGUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button(StringTable.Get(C.BakePointCloudToMesh), GUILayout.Width(100),
                        GUILayout.Height(40)))
                    {
                        BakePointCloudToMesh();
                    }
                    GUILayout.Space(20);
                    EditorGUILayout.LabelField(
                        StringTable.Get(C.Info_ToolDescription_BakePointCloudToMesh),
                        MTEStyles.labelFieldWordwrap);
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();

            GUILayout.FlexibleSpace();
            EditorGUILayout.HelpBox(StringTable.Get(C.Info_WillBeSavedInstantly),
                MessageType.Info, true);
        }

        private HashSet<MeshRenderer> highlightedRenderers = new HashSet<MeshRenderer>();
        List<GrassItem> editingItems = new List<GrassItem>();

        //debug for drawing the grass brush rectangle
        private Vector2 debug_boundMin, debug_boundMax;

        public void OnSceneGUI()
        {
            var e = Event.current;
            if (e.commandName == "UndoRedoPerformed")
            {
                SceneView.RepaintAll();
                return;
            }

            if (!(EditorWindow.mouseOverWindow is SceneView))
            {
                ClearHighlight();
                return;
            }

            if(!UseRandomDirection && e.control)
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
                        10 * Settings.PointSize, EventType.Repaint);
                }
            }

            // do nothing when mouse middle/right button, control/alt key is pressed
            if (e.button != 0 || e.alt)
                return;

            // no grass
            if (detailList == null || detailList.Count == 0)
            {
                MTEDebug.Log("Return: No grass detail.");
                return;
            }

            // grass loader not specified
            if (MTEContext.TheGrassLoader == null)
            {
                MTEDebug.Log("Return: No grass loader.");
                return;
            }

            HandleUtility.AddDefaultControl(0);
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            if (Physics.Raycast(ray, out var raycastHit,
                Mathf.Infinity,
                1 << MTEContext.TargetLayer//only hit target layer
            ))
            {
                //check tag
                if (!raycastHit.transform.CompareTag(MTEContext.TargetTag))
                {
                    return;
                }

                if (Settings.ShowBrushRect)
                {
                    Utility.ShowBrushRect(raycastHit.point, BrushSizeInU3D);
                }

                var hitPoint = raycastHit.point;

                Handles.color = Color.green;
                Handles.DrawWireDisc(hitPoint, raycastHit.normal, BrushSizeInU3D);

                if (!UseRandomDirection)
                {
                    ClearHighlight();

                    if (e.control)
                    {
                        GrassMap.GetGrassItemsInCircle(hitPoint, BrushSizeInU3D, editingItems);
                        foreach (var grassItem in editingItems)
                        {
                            var renderer = grassItem.gameObject.GetComponent<MeshRenderer>();
                            Utility.SetHighlight(renderer, true);
                            highlightedRenderers.Add(renderer);
                        }
                    }
                }
                
                if (Settings.DebugMode)
                {
                    //drawing the grass brush rectangle
                    Handles.DrawSolidRectangleWithOutline(new Vector3[4]
                    {
                        new Vector3(debug_boundMin.x, 0, debug_boundMin.y),
                        new Vector3(debug_boundMin.x, 0, debug_boundMax.y),
                        new Vector3(debug_boundMax.x, 0, debug_boundMax.y),
                        new Vector3(debug_boundMax.x, 0, debug_boundMin.y)
                    }, Color.clear, Color.red);
                }

                // not using random direction
                // hold control key and scroll wheel to change
                // 1. grasses' rotationY
                // 2. brush direction
                if (!UseRandomDirection && e.control && !e.isKey && e.type == EventType.ScrollWheel)
                {
                    float oldDirection = BrushDirection;
                    float direction = oldDirection;
                    ChangeDirection(e.delta.y, ref direction);

                    if (Mathf.Abs(direction - oldDirection) > Mathf.Epsilon)
                    {
                        UpdateGrasses(editingItems, Mathf.Rad2Deg * direction);

                        MTEEditorWindow.Instance.Repaint();
                        BrushDirection = direction;
                    }
                    e.Use();
                }
                else if (e.type == EventType.MouseDown || e.type == EventType.MouseDrag)
                {
                    if (e.type == EventType.MouseDown)
                    {
                        grassPaintTransation =
                            new Undo.UndoTransaction(
                                e.shift ?
                                    "Grass Painter: Delete Grass Instances" :
                                    "Grass Painter: Create Grass Instances"
                            );
                        Undo.UndoRedoManager.Instance().StartTransaction(grassPaintTransation);
                    }

                    if (!e.shift)
                    {//adding grasses
                        MTEDebug.Log("1: generate grass positions");
                        var grassDetail = detailList[this.SelectedGrassIndex];
                        grassPositions.Clear();
                        if (BrushSize == 0 || grassDetail.MaxWidth > this.BrushSizeInU3D)
                        {//TODO single mode: click and create a single grass; dragging is not allowed.
                            grassPositions.Add(new Vector2(hitPoint.x, hitPoint.z));
                        }
                        else
                        {
                            var hitPoint2D = new Vector2(hitPoint.x, hitPoint.z);
                            var brushBoundMin = (hitPoint2D - Vector2.one * BrushSizeInU3D);
                            var brushBoundMax = (hitPoint2D + Vector2.one * BrushSizeInU3D);

                            if (Settings.DebugMode)
                            {
                                debug_boundMin = brushBoundMin;
                                debug_boundMax = brushBoundMax;
                            }
                            

                            Dev.Profile.Start("FastPoissonDiskSampling.Sampling");
                            var samples = FastPoissonDiskSampling.Sample(
                                brushBoundMin, brushBoundMax, Distance, MaxPositionNumber);
                            Dev.Profile.End();

                            if (samples.Count > MaxPositionNumber)
                            {
                                Dev.Profile.Start("MathEx.TakeRandom");
                                samples = MathEx.TakeRandom(samples, MaxPositionNumber).ToList();
                                Dev.Profile.End();
                            }
                            Dev.Profile.Start("Remove overlapped samples");
                            //remove outside-brush samples
                            for (var i = samples.Count - 1; i >= 0; i--)
                            {
                                var p = samples[i];
                                if (Vector2.Distance(p, hitPoint2D) > BrushSizeInU3D)
                                {
                                    samples.RemoveAt(i);
                                }
                            }
                            Dev.Profile.End();
                            grassPositions.AddRange(samples);
                        }
                        MTEDebug.Log("2: possible added grass positions number = " + grassPositions.Count);
                        
                        Dev.Profile.Start("CreateGrassInstances");
                        CreateGrassInstances();
                        Dev.Profile.End();
                    }
                    else
                    {//removing grasses
                        removeList.Clear();
                        GrassMap.GetGrassItemsInCircle(hitPoint, BrushSizeInU3D, removeList);
                        int removeCount = Mathf.CeilToInt(this.reduction / 100.0f * this.removeList.Count);
                        if (removeCount != 0)
                        {
                            var grassItemsRemoved = this.removeList.TakeRandom(removeCount);
                            RemoveGrassInstances(grassItemsRemoved);
                        }
                    }
                }

                // auto save when mouse up
                if (e.type == EventType.MouseUp && e.button == 0)
                {
                    SaveGrass();
                    MTEDebug.Log("5: saved grass asset file");

                    if (grassPaintTransation != null)
                    {
                        Undo.UndoRedoManager.Instance().EndTransaction(grassPaintTransation);
                        Utility.RefreshHistoryViewer();
                        grassPaintTransation = null;
                    }
                }
            }

            SceneView.RepaintAll();
        }
        
        Undo.UndoTransaction grassPaintTransation;


        private Vector2 scrollPos = Vector2.zero;
        private int OnGrassDetailSelectorGUI(int selectedIndex)
        {
            if (this.detailList == null || this.detailList.Count == 0)
            {
                EditorGUILayout.HelpBox(StringTable.Get(C.Info_NoGrassDetail), MessageType.Warning);
                return selectedIndex;
            }

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, "box", GUILayout.ExpandHeight(true));
            for (int i = 0; i < this.detailList.Count; i += 4)
            {
                EditorGUILayout.BeginHorizontal();
                {
                    for (int j = 0; j < 4; j++)
                    {
                        if (i + j >= this.detailList.Count) break;

                        EditorGUILayout.BeginVertical();

                        //toggle button
                        bool toggleOn = selectedIndex == i + j;
                        var oldBgColor = GUI.backgroundColor;
                        if (toggleOn)
                        {
                            GUI.backgroundColor = new Color(62 / 255.0f, 125 / 255.0f, 231 / 255.0f);
                        }

                        var newToggleOn = GUILayout.Toggle(toggleOn,
                            GUIContent.none,
                            GUI.skin.button,
                            GUILayout.Width(72), GUILayout.Height(72));
                        GUI.backgroundColor = oldBgColor;
                        var rect = GUILayoutUtility.GetLastRect();

                        var detailIndex = i + j;
                        GrassDetailPreview.DrawGrassPreview(this.detailList[detailIndex], rect);

                        if (newToggleOn && !toggleOn)
                        {
                            selectedIndex = i + j;
                        }

                        EditorGUILayout.EndVertical();
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
            
            return selectedIndex;
        }

        private void CreateGrassInstances()
        {
            List<GrassItem> createdInstances = new List<GrassItem>(grassPositions.Count);

            var grassDetail = SelectedGrassDetail;
            int createdGrassObjectNumber = 0;
            for (int j = 0; j < grassPositions.Count; j++)
            {
                var grassPosition = grassPositions[j];
                var width = Random.Range(grassDetail.MinWidth, grassDetail.MaxWidth);
                if (!AllowOverlap)
                {//remove overlapped positions
                    if (GrassMap.Intersect(grassPosition, Distance))
                    {
                        continue;
                    }
                }

                var height = Random.Range(grassDetail.MinHeight, grassDetail.MaxHeight);
                var rotationY = UseRandomDirection ? Random.Range(0f, 180f) : Mathf.Rad2Deg * this.BrushDirection;
                
                RaycastHit hit;
                if (Physics.Raycast(
                    new Ray(new Vector3(grassPosition.x, 10000, grassPosition.y),
                        new Vector3(0, -1f, 0)),
                    out hit,
                    Mathf.Infinity,
                    1 << MTEContext.TargetLayer //only hit target layer
                ))
                {
                    //only consider target tag
                    if (!hit.transform.CompareTag(MTEContext.TargetTag))
                    {
                        continue;
                    }

                    GrassItem grassItem = null;
                    if (grassDetail.GrassType == GrassType.OneQuad)
                    {
                        grassItem = CreateGrassQuad(
                            grassDetail.Material, hit.point, rotationY, width, height);
                    }
                    else if(grassDetail.GrassType == GrassType.ThreeQuad)
                    {
                        grassItem = CreateGrassStar(
                            grassDetail.Material, hit.point, rotationY, width, height);
                    }
                    else if (grassDetail.GrassType == GrassType.CustomMesh)
                    {
                        grassItem = CreateGrassCustomMesh(
                            grassDetail.Material, grassDetail.GrassMesh, hit.point, rotationY, width, height);
                    }
                    else
                    {
                        throw new System.ArgumentOutOfRangeException(
                            $"Unknown grass type {grassDetail.GrassType}");
                    }

                    if (grassItem == null)
                    {
                        continue;
                    }

                    createdGrassObjectNumber++;
                    createdInstances.Add(grassItem);
                }
            }

            MTEDebug.Log("4: created grass object number = " + createdGrassObjectNumber);

            if (createdInstances.Count > 0)
            {
                Undo.UndoRedoManager.Instance().Push(a =>
                {
                    UndoCreate(a);
                }, createdInstances);
            }
        }

        private void RemoveGrassInstances(IEnumerable<GrassItem> grassItemsRemoved)
        {
            List<GrassItem> removedItems = new List<GrassItem>();
            foreach (var grassItem in grassItemsRemoved)
            {
                if (grassItem.Star != null)
                {
                    MTEContext.TheGrassLoader.grassInstanceList.grasses.Remove(grassItem.Star);
                }
                else if (grassItem.Quad != null)
                {
                    MTEContext.TheGrassLoader.grassInstanceList.quads.Remove(grassItem.Quad);
                }
                else if (grassItem.CustomMesh != null)
                {
                    MTEContext.TheGrassLoader.grassInstanceList.customMeshes.Remove(grassItem.CustomMesh);
                }
                Object.DestroyImmediate(grassItem.gameObject);
                grassItem.gameObject = null;
                GrassMap.Remove(grassItem);

                removedItems.Add(grassItem);
            }
            
            Undo.UndoRedoManager.Instance().Push(a =>
            {
                RedoCreate(removedItems);
            }, removedItems);
        }

        private GrassItem CreateGrassQuad(
            Material material,
            Vector3 position, float rotationY,
            float width, float height)
        {
            GameObject grassObject;
            MeshRenderer grassMeshRenderer; //not used
            Mesh grassMesh; //not used
            var rotation = Quaternion.Euler(0, rotationY, 0);
            GrassUtil.GenerateGrassQuadObject(position, rotation, width, height, 
                material, MTEContext.TheGrassLoader.isGrassStatic, out grassObject, out grassMeshRenderer, out grassMesh);
            grassObject.transform.SetParent(MTEContext.TheGrassLoader.transform, true);
            GrassQuad quad = new GrassQuad();
            quad.Init(material, position, rotationY, width, height);
            MTEContext.TheGrassLoader.grassInstanceList.quads.Add(quad);
            var grassItem = new GrassItem(quad, grassObject);
            GrassMap.Insert(grassItem);
            return grassItem;
        }

        private GrassItem CreateGrassStar(
            Material material,
            Vector3 position, float rotationY,
            float width, float height)
        {
            GameObject grassObject;
            MeshRenderer grassMeshRenderer; //not used
            Mesh grassMesh; //not used
            var rotation = Quaternion.Euler(0, rotationY, 0);
            GrassUtil.GenerateGrassStarObject(position, rotation, width, height,
                material,  MTEContext.TheGrassLoader.isGrassStatic, out grassObject, out grassMeshRenderer, out grassMesh);

            grassObject.transform.SetParent(MTEContext.TheGrassLoader.transform, true);

            GrassStar grassInstance = new GrassStar();
            grassInstance.Init(material, position, rotationY, width, height);
            MTEContext.TheGrassLoader.grassInstanceList.grasses.Add(grassInstance);
            var grassItem = new GrassItem(grassInstance, grassObject);
            GrassMap.Insert(grassItem);
            return grassItem;
        }

        private GrassItem CreateGrassCustomMesh(
            Material material,
            Mesh grassMesh,
            Vector3 position, float rotationY,
            float scaleXZ, float scaleY)
        {
            if (grassMesh == null || material == null)
            {
                return null;
            }

            GameObject grassObject;
            MeshRenderer grassMeshRenderer; //not used
            var rotation = Quaternion.Euler(0, rotationY, 0);
            GrassUtil.GenerateGrassCustomMeshObject(position, rotation, 
                grassMesh, material, scaleXZ, scaleY, MTEContext.TheGrassLoader.isGrassStatic,
                out grassObject, out grassMeshRenderer);

            grassObject.transform.SetParent(MTEContext.TheGrassLoader.transform, true);

            GrassCustomMesh customMesh = new GrassCustomMesh();
            customMesh.Init(grassMesh, material, position, rotationY, scaleXZ, scaleY);
            MTEContext.TheGrassLoader.grassInstanceList.customMeshes.Add(customMesh);
            var grassItem = new GrassItem(customMesh, grassObject);
            GrassMap.Insert(grassItem);
            return grassItem;
        }

        private void UndoCreate(List<GrassItem> createdInstances)
        {
            List<GrassItem> removedItems
                = new List<GrassItem>(createdInstances.Count);
            //remove created grass instances
            foreach (var grassItem in createdInstances)
            {
                if (grassItem == null)
                {
                    continue;
                }

                if (grassItem.gameObject)
                {
                    Object.DestroyImmediate(grassItem.gameObject);
                    grassItem.gameObject = null;
                }

                GrassMap.Remove(grassItem);

                if (grassItem.Star == null && grassItem.Quad == null && grassItem.CustomMesh == null)
                {
                    continue;
                }

                if (grassItem.Star != null)
                {
                    MTEContext.TheGrassLoader.grassInstanceList.grasses.Remove(grassItem.Star);
                }
                else if(grassItem.Quad != null)
                {
                    MTEContext.TheGrassLoader.grassInstanceList.quads.Remove(grassItem.Quad);
                }
                else if (grassItem.CustomMesh != null)
                {
                    MTEContext.TheGrassLoader.grassInstanceList.customMeshes.Remove(grassItem.CustomMesh);
                }

                removedItems.Add(grassItem);
            }

            Undo.UndoRedoManager.Instance().Push(a =>
            {
                RedoCreate(removedItems);
            }, createdInstances);
        }
        
        private void RedoCreate(List<GrassItem> removedObjects)
        {
            List<GrassItem> createdItems = new List<GrassItem>(removeList.Count);
            foreach (var undoData in removedObjects)
            {
                if (undoData.Quad == null && undoData.Star == null && undoData.CustomMesh == null)
                {//ignore invalid grass item
                    continue;
                }

                GrassItem grassItem = null;
                if (undoData.Quad != null)
                {
                    var quad = undoData.Quad;
                    grassItem = CreateGrassQuad(
                        quad.Material,
                        quad.Position, quad.RotationY,
                        quad.Width, quad.Height);
                }
                else if(undoData.Star != null)
                {
                    var star = undoData.Star;
                    grassItem = CreateGrassStar(
                        star.Material,
                        star.Position, star.RotationY,
                        star.Width, star.Height);
                }
                else if (undoData.CustomMesh != null)
                {
                    var customMesh = undoData.CustomMesh;
                    grassItem = CreateGrassCustomMesh(customMesh.Material, customMesh.Mesh, customMesh.Position,
                        customMesh.RotationY, customMesh.Width, customMesh.Height);
                }
                else
                {
                    MTEDebug.LogWarning("Ignored a null grass item when undo/redo.");
                    continue;
                }

                createdItems.Add(grassItem);
            }
            
            Undo.UndoRedoManager.Instance().Push(a =>
            {
                UndoCreate(a);
            }, createdItems);
        }

        private void ClearHighlight()
        {
            foreach (var renderer in highlightedRenderers)
            {
                if (renderer)
                {
                    Utility.SetHighlight(renderer, false);
                }
            }

            highlightedRenderers.Clear();
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

        private void SaveGrass()
        {
            EditorUtility.SetDirty(MTEContext.TheGrassLoader.grassInstanceList);
        }

        /// <summary>
        /// Update height of grass items
        /// </summary>
        private static void UpdateGrasses(IEnumerable<GrassItem> items)
        {
            bool updated = false;
            foreach (var item in items)
            {
                var pos2D = new Vector2(item.Position2D.x, item.Position2D.y);
                var rayOrigin = new Vector3(pos2D.x, 99999f, pos2D.y);
                var ray = new Ray(rayOrigin, Vector3.down);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit,
                    Mathf.Infinity,
                    1 << MTEContext.TargetLayer//only hit target layer
                ))
                {
                    if (!hit.transform.CompareTag(MTEContext.TargetTag))
                    {
                        return;
                    }
                    item.Height = hit.point.y;
                    updated = true;
                }
            }

            if (updated)
            {
                GrassPainter.Instance.SaveGrass();
            }
        }

        /// <summary>
        /// Update rotation (Y) of grass items
        /// </summary>
        private static void UpdateGrasses(IEnumerable<GrassItem> items, float rotationY)
        {
            foreach (var item in items)
            {
                var pos2D = new Vector2(item.Position2D.x, item.Position2D.y);
                var rayOrigin = new Vector3(pos2D.x, 99999f, pos2D.y);
                var ray = new Ray(rayOrigin, Vector3.down);
                if (Physics.Raycast(ray, Mathf.Infinity, ~MTEContext.TargetLayer))
                {
                    item.RotationY = rotationY;
                }
            }

            GrassPainter.Instance.SaveGrass();
        }

        /// <summary>
        /// Update height of grass items inside a circular region
        /// </summary>
        /// <param name="center">center of the circular region</param>
        /// <param name="radius">radius of the circular region</param>
        public void UpdateGrass(Vector3 center, float radius)
        {
            var items = new List<GrassItem>();
            GrassMap.GetGrassItemsInCircle(center, radius, items);
            UpdateGrasses(items);
        }

        /// <summary>
        /// Update height of all grass items
        /// </summary>
        public void UpdateAllGrasses()
        {
            var items = GrassMap.GetAllGrassItems();
            UpdateGrasses(items);
        }

        private void BakePointCloudToMesh()
        {
            bool confirmed = EditorUtility.DisplayDialog(
                StringTable.Get(C.Warning),
                StringTable.Get(C.Warning_Confirm_UnrecoverableOperation),
                StringTable.Get(C.Yes), StringTable.Get(C.No));
            if (!confirmed)
            {
                return;
            }

            if (!MTEContext.TheGrassLoader)
            {
                EditorUtility.DisplayDialog(
                    StringTable.Get(C.Warning),
                    StringTable.Get(C.Warning_NoGrassLoader_CannotBakePointCloudToMesh),
                    StringTable.Get(C.OK));
                return;
            }

            MTEContext.TheGrassLoader.RemoveOldGrasses();
            MTEContext.TheGrassLoader.GenerateGrasses(new GrassGenerationSettings
            {
                UseStaticBatch = false,
                HideGrassObjectInEditor = false
            });

            bool removeGrassLoader = EditorUtility.DisplayDialog(
                StringTable.Get(C.Info),
                StringTable.Get(C.Info_RemoveGrassLoader),
                StringTable.Get(C.Yes),
                StringTable.Get(C.No));
            if (removeGrassLoader)
            {
                Object.DestroyImmediate(MTEContext.TheGrassLoader);
            }

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }
    }

}
