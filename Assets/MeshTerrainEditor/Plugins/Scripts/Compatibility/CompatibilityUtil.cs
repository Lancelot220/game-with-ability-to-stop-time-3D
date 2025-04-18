#if UNITY_EDITOR
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace MTE
{
    public enum TerrainMaterialType
    {
        BuiltInStandard,
        BuiltInLegacyDiffuse,
        BuiltInLegacySpecular,
        Custom,
        LWRPTerrainLit_Removed,
        URPTerrainLit,
        Unknown,
    }

    public enum UnityWebRequestExResult
    {
        InProgress,
        Success,
        Error,
    }

    public static class CompatibilityUtil
    {
        /// <summary>
        /// Create a prefab file for a GameObject.
        /// </summary>
        /// <param name="obj">the GameObject</param>
        /// <param name="relativePath">Unity file path of the prefab</param>
        public static void CreatePrefab(GameObject obj, string relativePath)
        {
            PrefabUtility.SaveAsPrefabAsset(obj, relativePath);
        }

        /// <summary>
        /// Check if a gameObject is the root of a instantiated prefab.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        public static bool IsPrefab(GameObject gameObject)
        {
            return PrefabUtility.IsAnyPrefabInstanceRoot(gameObject);
        }

        public static bool IsInstanceOfPrefab(GameObject obj, GameObject prefab)
        {
            return PrefabUtility.GetCorrespondingObjectFromOriginalSource(obj) == prefab || PrefabUtility.GetCorrespondingObjectFromSource(obj) == prefab;
        }
        
        public static Object GetPrefabRoot(GameObject instance)
        {
            return PrefabUtility.GetCorrespondingObjectFromSource(instance);
        }

        //This field will invoke a compile warning if this version hasn't been supported by MTE.
        public const string VersionCheckString =
                    //Unity 2018.4 support has been removed in MTE 4.0.5
#if UNITY_2019_3
                    "2019.3"
#elif UNITY_2019_4
                    "2019.4"
#elif UNITY_2020_1
                    "2020.1"
#elif UNITY_2020_2
                    "2020.2"
#elif UNITY_2020_3
                    "2020.3"
#elif UNITY_2021_1
                    "2021.1"
#elif UNITY_2021_2
                    "2021.2"
#elif UNITY_2021_3
                    "2021.3"
#elif UNITY_2022_1
                    "2022.1"
#elif UNITY_2022_2
                    "2022.2"
#elif UNITY_2022_3
                    "2022.3"
#elif UNITY_2023_1
                    "2023.1"
#elif UNITY_2023_2
                    "2023.2"
#elif UNITY_2023_3
                    "2023.3"
#elif UNITY_6000_0
                    "6.0"
#elif UNITY_6000_1_OR_NEWER
#warning [MTE] Might be a unsupported Unity Version. If any compile error occurs, you can try to fix this file or report the issue.
#endif
            ;

        public static bool IsWebRequestError(UnityEngine.Networking.UnityWebRequest request)
        {
#if UNITY_2019_3 || UNITY_2019_4 || UNITY_2020_1
            return request.isNetworkError || request.isHttpError;
#elif UNITY_2020_2 || UNITY_2020_3 || UNITY_2021_1 || UNITY_2021_2_OR_NEWER
            return request.result == UnityEngine.Networking.UnityWebRequest.Result.ConnectionError
                || request.result == UnityEngine.Networking.UnityWebRequest.Result.ProtocolError;
#else
#error not supported
#endif
        }

        public static object AttachOnSceneGUI(System.Action<SceneView> action)
        {
            SceneView.duringSceneGui += action;
            return action;
        }

        public static void DetachOnSceneGUI(object cachedOnSceneGUI)
        {
            var action = (System.Action<SceneView>)cachedOnSceneGUI;
            SceneView.duringSceneGui -= action;
        }

        public static void DontOptimizeMesh(ModelImporter importer)
        {
            importer.optimizeMeshVertices = false;
            importer.optimizeMeshPolygons = false;
            importer.weldVertices = false;
        }

        public static TerrainMaterialType GetTerrainMaterialType(Terrain terrain)
        {
            var material = terrain.materialTemplate;
            if (material == null)
            {
                Debug.LogErrorFormat("Terrain<{0}> is using an empty material. Please check the material of the terrain", terrain.name);
                return TerrainMaterialType.Custom;
            }

            if (material.name == "Default-Terrain-Standard")
            {
                return TerrainMaterialType.BuiltInStandard;
            }
            if (material.name == "Default-Terrain-Diffuse")
            {
                return TerrainMaterialType.BuiltInLegacyDiffuse;
            }
            if (material.name == "Default-Terrain-Specular")
            {
                return TerrainMaterialType.BuiltInLegacySpecular;
            }
            if (material.shader != null
                && material.shader.name == "Lightweight Render Pipeline/Terrain/Lit")
            {
                return TerrainMaterialType.LWRPTerrainLit_Removed;
            }
            if (material.shader != null
                && material.shader.name == "Universal Render Pipeline/Terrain/Lit")
            {
                return TerrainMaterialType.URPTerrainLit;
            }

            var materialFileRelativePath = AssetDatabase.GetAssetPath(material);
            Debug.LogErrorFormat(
                "Terrain<{0}> is using a material<{1}> at {2} that is unknown to MTE." +
                "The conversion result is probably not right.",
                terrain.name, material.name, materialFileRelativePath);

            return TerrainMaterialType.Unknown;
        }

        public static Color GetTerrainMaterialSpecularColor(Terrain terrain)
        {
            var material = terrain.materialTemplate;
            if (material.HasProperty("_SpecColor"))
            {
                return material.GetColor("_SpecColor");
            }
            return Color.black;
        }

        public static float GetTerrainMaterialShininess(Terrain terrain)
        {
            var material = terrain.materialTemplate;
            return material.GetFloat("_Shininess");
        }

        public static int GetHeightmapWidth(TerrainData terrainData)
        {
            return terrainData.heightmapResolution;
        }

        public static int GetHeightmapHeight(TerrainData terrainData)
        {
            return terrainData.heightmapResolution;
        }

        public static void SetTerrainMaterialType(Terrain terrain, TerrainMaterialType materialType)
        {
            switch (materialType)
            {
                case TerrainMaterialType.BuiltInStandard:
                {
                    var material = Resources.Load<Material>("unity_builtin_extra/Default-Terrain-Standard");
                    terrain.materialTemplate = material;
                }
                    break;
                case TerrainMaterialType.BuiltInLegacyDiffuse:
                {
                    var material = Resources.Load<Material>("unity_builtin_extra/Default-Terrain-Diffuse");
                    terrain.materialTemplate = material;
                }
                    break;
                case TerrainMaterialType.BuiltInLegacySpecular:
                {
                    var material = Resources.Load<Material>("unity_builtin_extra/Default-Terrain-Specular");
                    terrain.materialTemplate = material;
                }
                    break;
                case TerrainMaterialType.Custom:
                case TerrainMaterialType.LWRPTerrainLit_Removed:
                case TerrainMaterialType.URPTerrainLit:
                case TerrainMaterialType.Unknown:
                default:
                    throw new System.NotSupportedException($"Cannot set terrain material: {materialType}.");
            }
        }

        //Unity 2021.2.0 changed
        //https://unity3d.com/unity/whats-new/2021.2.0
        //Graphics: Changed: Renamed Texture2D.Resize to Reinitialize.
        public static void ReinitializeTexture2D(Texture2D texture, int newWidth, int newHeight)
        {
#if UNITY_2019_3 || UNITY_2019_4 || UNITY_2020_1 || UNITY_2020_2 || UNITY_2020_3 || UNITY_2021_1
            texture.Resize(newWidth, newHeight);
#elif UNITY_2021_2_OR_NEWER
            texture.Reinitialize(newWidth, newHeight);
#endif
        }
        public static bool ResizeTexture2D(Texture2D texture, int width, int height, TextureFormat format, bool hasMipMap)
        {
#if UNITY_2019_3 || UNITY_2019_4 || UNITY_2020_1 || UNITY_2020_2 || UNITY_2020_3 || UNITY_2021_1
            return texture.Resize(width, height, format, hasMipMap);
#elif UNITY_2021_2_OR_NEWER
            return texture.Reinitialize(width, height, format, hasMipMap);
#else
#error not supported on any Unity build targets
#endif
        }

        public static bool GetTextureReadable(Texture texture)
        {
            return texture.isReadable;
        }

        public static bool BeginFoldoutHeaderGroup(bool foldout, string content)
        {
            return EditorGUILayout.BeginFoldoutHeaderGroup(foldout, content);
        }

        public static void EndFoldoutHeaderGroup()
        {
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        //FBXExporter support
        public static System.Func<string, UnityEngine.Object, string> exportFbxMethod = null;

        private const string UnityFbxExporterFailureHint =
            "Make sure you have installed FBX Exporter in your project. See https://docs.unity3d.com/Packages/com.unity.formats.fbx@2.0/manual/index.html\n" +
            "If you have installed Unity FbxExporter and still get this error, please report the issue.\n" +
            "It's recommended to use MTE builtin FBX support instead. See the documentation of Settings for details.";
        public static bool InitFbxExport()
        {
            var unityFormatsFbxEditorAssembly = System.AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a=>a.GetName().Name == "Unity.Formats.Fbx.Editor");
            if (unityFormatsFbxEditorAssembly == null)
            {
                Debug.LogWarning($"[MTE] Failed to fetch required assembly Unity.Formats.Fbx.Editor.dll. {UnityFbxExporterFailureHint}. This can be ignored safely if builtin FBX exporter is being used.");
                return false;
            }
            var modelExporterType = unityFormatsFbxEditorAssembly.GetType("UnityEditor.Formats.Fbx.Exporter.ModelExporter", false);
            if (modelExporterType == null)
            {
                Debug.LogWarning($"[MTE] Failed to fetch required type: UnityEditor.Formats.Fbx.Exporter.ModelExporter. {UnityFbxExporterFailureHint}");
                return false;
            }
            var methods = modelExporterType.GetMethods();
            var method = methods.FirstOrDefault(m => m.Name == "ExportObject" && m.GetParameters().Length == 2);
            if (method == null)
            {
                Debug.LogWarning($"[MTE] Failed to fetch required method: ModelExporter.ExportObject(). {UnityFbxExporterFailureHint}");
                return false;
            }

            exportFbxMethod = (System.Func<string, UnityEngine.Object, string>)System.Delegate.CreateDelegate(typeof(System.Func<string, UnityEngine.Object, string>), method);
            //Debug.Log("Successfully fetched the method: UnityEditor.Formats.Fbx.Exporter.ModelExporter().");
            return true;
        }
        
        private static Texture2D MakeTempTexture(int width, int height, Color color)
        {
            Color[] pixels = new Color[width * height];

            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }

            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pixels);
            result.Apply();
            result.hideFlags = HideFlags.DontSave;

            return result;
        }

        private static readonly Color32 s_hoverColor = Color.grey;
        private static readonly Color32 s_selectedColor = new Color32(62, 125, 231, 255);

        private static readonly Texture2D s_onHoverTexture = MakeTempTexture(1, 1, s_selectedColor);
        private static readonly Texture2D s_hoverTexture = MakeTempTexture(1, 1, s_hoverColor);
        private static readonly Texture2D s_normalTexture = MakeTempTexture(1, 1, Color.clear);
        private static readonly Texture2D s_onNormalTexture = s_onHoverTexture;

        public static GUIStyle GetGridListStyle()
        {
            var editorLabelStyle = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).label;
            var gridListStyle = new GUIStyle(editorLabelStyle)
            {
                fixedWidth = 0,
                fixedHeight = 0,
                stretchWidth = true,
                stretchHeight = true,
                alignment = TextAnchor.MiddleCenter
            };

            gridListStyle.onHover.background = s_onHoverTexture;
            gridListStyle.hover.background = s_hoverTexture;
            gridListStyle.normal.background = s_normalTexture;
            gridListStyle.onNormal.background = s_onNormalTexture;

            return gridListStyle;
        }

        //According to https://docs.unity3d.com/Manual/srp-setting-render-pipeline-asset.html
        //QualitySettings.renderPipeline overrides GraphicsSettings.renderPipelineAsset
        public static object GetRenderPipelineAsset()
        {
            if (QualitySettings.renderPipeline != null)
            {
                if (UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset == null)
                {//If GraphicsSettings.renderPipelineAsset doesn't exist, Unity will use built-in pipeline, at least in Unity 2020.3.
                    return null;
                }
                return QualitySettings.renderPipeline;
            }
            return UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset;
        }

        public static void SetTextureFormatToA8(TextureImporterPlatformSettings textureSettings)
        {
            textureSettings.format = TextureImporterFormat.R8;
        }

        public static int FindPropertyIndex(Shader shader, string propertyName)
        {
            return shader.FindPropertyIndex(propertyName);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte GetColor32ChannelValue(Color32 color32, int channel)
        {
            return color32[channel];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetColor32ChannelValue(ref Color32 color32, int channel, byte value)
        {
            color32[channel] = value;
        }

        public class GUI
        {
            public static float Slider(
                Rect position,
                float value,
                float size,
                float start,
                float end,
                GUIStyle slider,
                GUIStyle thumb,
                bool horiz,
                int id)
            {
                return UnityEngine.GUI.Slider(
                    position,
                    value,
                    size,
                    start,
                    end,
                    slider,
                    thumb,
                    horiz,
                    id);
            }
        }

        public class EditorGUILayoutEx
        {
            private static GUIStyle foldOutStyle;
            public static bool BeginFoldout(bool foldout, string content)
            {
                if (foldOutStyle == null)
                {
                    foldOutStyle = new GUIStyle(EditorStyles.foldoutHeader);
                    foldOutStyle.fontStyle = FontStyle.Normal;
                }
                return EditorGUILayout.BeginFoldoutHeaderGroup(foldout, content, foldOutStyle);
            }

            public static void EndFoldout()
            {
                EditorGUILayout.EndFoldoutHeaderGroup();
            }

        }

        public static bool IsViewToolActive()
        {
#if UNITY_2020_1_OR_NEWER
            //Tools.viewToolActive is only available on Unity 2020.1+
            return Tools.viewToolActive;
#else
            return !(Tools.current == Tool.None || Tools.current == Tool.Move);
#endif
        }

        public static UnityWebRequestExResult GetUnityWebRequestExResult(UnityWebRequest unityWebRequest)
        {
#if UNITY_2020_1_OR_NEWER
            if (unityWebRequest.result == UnityWebRequest.Result.InProgress)
            {
                return UnityWebRequestExResult.InProgress;
            }
            else if (unityWebRequest.result == UnityWebRequest.Result.Success)
            {
                return UnityWebRequestExResult.Success;
            }
            else
            {
                return UnityWebRequestExResult.Error;
            }
#else
            if (unityWebRequest.isHttpError || unityWebRequest.isNetworkError)
            {
                return UnityWebRequestExResult.Error;
            }

            if (!unityWebRequest.isDone)
            {
                return UnityWebRequestExResult.InProgress;
            }

            return UnityWebRequestExResult.Success;
#endif
        }
    }
}

#endif