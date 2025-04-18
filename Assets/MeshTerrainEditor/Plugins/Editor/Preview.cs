using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MTE
{
    internal class Preview
    {
        public bool IsReady = false;
        private bool IsArray = false;
        private Shader shader;
        private RenderPipeline renderPipeline = RenderPipeline.NotDetermined;
        private GameObject previewObj;
        private Texture2D UpDirectionNormalTexture;
        private static readonly int Rotation = Shader.PropertyToID("_Rotation");

        class LayerInfo
        {
            public Texture ColorTexture { get; set; }
            public Texture NormalTexture { get; set; }
            public float UVScale { get; set; }

            //TODO support other PBR textures
        }

        public Preview(bool isArray)
        {
            this.IsArray = isArray;
        }

        /// <summary>
        /// Destroy the preview object left behind caused by errors or bugs
        /// </summary>
        [MenuItem("Window/Mesh Terrain Editor/Debug/Fix Pink Brush Box")]
        public static void RemoveBuggyPreviewObject()
        {
            var previewObjects = Resources.FindObjectsOfTypeAll<GameObject>().Where(o => o.name == "MTEPreview");

            foreach (var o in previewObjects)
            {
                o.hideFlags = HideFlags.DontSave;
                var objectName = o.name;
                Object.DestroyImmediate(o.gameObject);
                MTEDebug.Log($"Removed ghost objects <{objectName}>");
            }
        }

        private bool TryGetTextureLayerInfo(Texture selectedTexture, EditorFilterMode editorFilterMode, out LayerInfo layerInfo)
        {
            bool found = false;
            layerInfo = null;
            if (editorFilterMode == EditorFilterMode.FilteredGameObjects)
            {
                foreach (var target in MTEContext.Targets)
                {
                    if (TryGetTextureLayerInfo(target, selectedTexture, out layerInfo))
                    {
                        found = true;
                        break;
                    }
                }
            }
            else if (editorFilterMode == EditorFilterMode.SelectedGameObject)
            {
                if (TryGetTextureLayerInfo(Selection.activeGameObject, selectedTexture, out layerInfo))
                {
                    found = true;
                }
            }
            else
            {
                throw new MTENotSupportedException($"Mode {editorFilterMode} is not supported.");
            }

            return found;
        }

        private bool TryGetTextureLayerInfo(GameObject obj, Texture texture, out LayerInfo layerInfo)
        {
            if (!obj)
            {
                throw new System.ArgumentNullException(nameof(obj));
            }

            if (!texture)
            {
                throw new System.ArgumentNullException(nameof(texture));
            }

            layerInfo = new LayerInfo();
            if (IsArray)
            {
                if (!obj)
                {
                    return false;
                }
                var meshRenderer = obj.GetComponent<MeshRenderer>();
                if (meshRenderer == null)
                {
                    return false;
                }
                var m = meshRenderer.sharedMaterial;
                if (m == null)
                {
                    return false;
                }

                var runtimeTextureArrayLoader = meshRenderer.GetComponent<RuntimeTextureArrayLoader>();
                Texture2DArray textureArray;
                if (runtimeTextureArrayLoader)
                {
                    runtimeTextureArrayLoader.LoadInEditor();
                    var settings = runtimeTextureArrayLoader.settings;
                    textureArray = m.GetTextureArray0();
                    TextureArrayManager.Instance.AddOrUpdate(textureArray, settings);
                }
                else
                {
                    if (!m.TryGetTextureArray0(out textureArray, out _))
                    {
                        return false;
                    }

                    if (!TextureArrayManager.Instance.IsCached(textureArray))
                    {
                        //TODO maybe we can cache it here? But side effect should be considered.
                        //TODO But there is no proper way to locate the settings file here: it's hard to get the settings file from a Texture2DArray.
                        return false;
                    }
                }

                if (!textureArray)
                {
                    return false;
                }

                var sliceIndex = TextureArrayManager.Instance.GetTextureSliceIndex(textureArray, texture);
                if (sliceIndex < 0)
                {
                    return false;
                }
                
                if(!m.TryGetLayerUVScale(sliceIndex, isArrayShader: true, out var uvScale))
                {
                    MTEDebug.LogWarning($"Fallback uvScale to 1.0: failed to get uv scale of texture<{textureArray.name}> slice[{sliceIndex}].");
                    uvScale = 1.0f;
                }

                layerInfo.UVScale = uvScale;
                layerInfo.ColorTexture = texture;
                layerInfo.NormalTexture = null;//TODO fetch from cached TextureArraySettings
                return true;
            }
            else
            {
                var meshRenderer = obj.GetComponent<MeshRenderer>();
                if (meshRenderer == null)
                {
                    return false;
                }
                var m = meshRenderer.sharedMaterial;
                if (m == null)
                {
                    return false;
                }

                var splatIndex = m.FindSplatTexture(texture);
                if (splatIndex < 0)
                {
                    return false;
                }

                if (!m.TryGetLayerUVScale(splatIndex, isArrayShader: false, out var uvScale))
                {
                    MTEDebug.LogWarning($"Fallback uvScale to 1.0: failed to get uv scale of texture<{texture.name}> layer[{splatIndex}].");
                    uvScale = 1.0f;
                }

                layerInfo.UVScale = uvScale;
                layerInfo.ColorTexture = texture;
                layerInfo.NormalTexture = null;//TODO fetch from cached TextureArraySettings
                return true;
            }
        }

        private void LoadPreviewForArray(Texture selectedTexture, float brushSizeInU3D, int brushIndex, EditorFilterMode editorFilterMode)
        {
            if (!IsArray)
            {
                throw new MTEException("Cannot load preview for a non-TextureArray brush here.");
            }
            
            if (!TryGetTextureLayerInfo(selectedTexture, editorFilterMode, out var layerInfo))
            {
                throw new MTEEditException(
                    "Failed to load texture in to preview: " +
                    "selected texture isn't a source texture slice of any texture array used in any targets' material. " +
                    "Please refresh the filter to reload the texture list.\n\n" +
                    $"Note: MTE finds source texture slice via the {nameof(TextureArraySettings)} asset next to the texture array being used.");
            }

            UnLoadPreview();

            CreatePreviewObject();

            previewObj.hideFlags = HideFlags.HideAndDontSave;
            
            SetPreviewTexture(new Vector2(layerInfo.UVScale, layerInfo.UVScale), layerInfo.ColorTexture, layerInfo.ColorTexture);
            SetPreviewSize(brushSizeInU3D / 2);
            SetPreviewMaskTexture(brushIndex);
        }

        private void LoadPreviewForNonArray(Texture selectedTexture, float brushSizeInU3D, int brushIndex, EditorFilterMode editorFilterMode)
        {
            if (IsArray)
            {
                throw new MTEException("Cannot load preview for a TextureArray brush here.");
            }

            UnLoadPreview();

            CreatePreviewObject();

            previewObj.hideFlags = HideFlags.HideAndDontSave;
            
            if (!TryGetTextureLayerInfo(selectedTexture, editorFilterMode, out var layerInfo))
            {
                throw new MTEEditException("Failed to load texture in to preview: selected texture isn't used in any targets' material. Please refresh the filter to reload the texture list.");
            }

            SetPreviewTexture(new Vector2(layerInfo.UVScale, layerInfo.UVScale), layerInfo.ColorTexture, layerInfo.NormalTexture);
            SetPreviewSize(brushSizeInU3D / 2);
            SetPreviewMaskTexture(brushIndex);
        }
        
        /// <summary>
        /// Load brush preview for selected texture.
        /// </summary>
        public void LoadPreview(Texture texture, float brushSizeInU3D, int brushIndex, EditorFilterMode editorFilterMode)
        {
            if (!texture)
            {
                return;
            }

            try
            {
                LoadShader();

                if (IsArray)
                {
                    LoadPreviewForArray(texture, brushSizeInU3D, brushIndex, editorFilterMode);
                }
                else
                {
                    LoadPreviewForNonArray(texture, brushSizeInU3D, brushIndex, editorFilterMode);
                }

                IsReady = true;
            }
            finally
            {
                if (!IsReady)
                {
                    UnLoadPreview();
                }
            }
        }

        /// <summary>
        /// Destroy the preview.
        /// </summary>
        public void UnLoadPreview()
        {
            if (previewObj != null)
            {
                UnityEngine.Object.DestroyImmediate(previewObj);
                previewObj = null;
            }
            IsReady = false;
        }

        private void SetPreviewTexture(Vector2 textureScale, Texture colorTexture, Texture normalTexture)
        {
            if(!previewObj)
            {
                return; 
            }

            if(renderPipeline != RenderPipeline.Builtin)
            {
                var renderer = previewObj.GetComponent<MeshRenderer>();
                renderer.sharedMaterial.SetTexture("_MainTex", colorTexture);
                renderer.sharedMaterial.SetTextureScale("_MainTex", textureScale);

                if (!normalTexture)
                {//Default "bump" direction is (0, 0, 1), encoded as(0.5, 0.5, 1.0);
                 //  not expected up-direction (0, 1, 0), encoded as(0.5, 1.0, 0.5).
                 //So a default up-direction normal texture is created here lazily to replaced the "bump".
                    if (!UpDirectionNormalTexture)
                    {
                        UpDirectionNormalTexture = new Texture2D(1, 1, TextureFormat.RGB24, false);
                        UpDirectionNormalTexture.SetPixel(0, 0, new Color(0.5f, 1.0f, 0.5f));
                        UpDirectionNormalTexture.Apply();
                    }
                    normalTexture = UpDirectionNormalTexture;
                }
                renderer.sharedMaterial.SetTexture("_NormalTex", normalTexture);
            }
            else
            {
                var projector = previewObj.GetComponent<Projector>();
                projector.material.SetTexture("_MainTex", colorTexture);
                projector.material.SetTextureScale("_MainTex", textureScale);
                projector.material.SetTexture("_NormalTex", normalTexture);
            }

            SceneView.RepaintAll();
        }

        public void SetPreviewMaskTexture(int maskIndex)
        {
            if(!previewObj)
            {
                return; 
            }

            if (renderPipeline == RenderPipeline.Builtin)
            {
                var projector = previewObj.GetComponent<Projector>();
                projector.material.SetTexture("_MaskTex", MTEStyles.brushTextures[maskIndex]);
                projector.material.SetTextureScale("_MaskTex", Vector2.one);
            }
            else
            {
                var renderer = previewObj.GetComponent<MeshRenderer>();
                renderer.sharedMaterial.SetTexture("_MaskTex", MTEStyles.brushTextures[maskIndex]);
                renderer.sharedMaterial.SetTextureScale("_MaskTex", Vector2.one);
            }
            SceneView.RepaintAll();
        }

        public void SetPreviewSize(float value)
        {
            if(!previewObj)
            {
                return; 
            }

            if (renderPipeline == RenderPipeline.Builtin)
            {
                var projector = previewObj.GetComponent<Projector>();
                projector.orthographicSize = value;
            }
            else
            {
                var halfBrushSizeInUnityUnit = value;
                previewObj.transform.localScale = new Vector3(
                    halfBrushSizeInUnityUnit*2,
                    halfBrushSizeInUnityUnit*2, 10000);
            }
            SceneView.RepaintAll();
        }

        public void SetRotation(float rad)
        {
            if(!previewObj)
            {
                return; 
            }
            
            if (renderPipeline == RenderPipeline.Builtin)
            {
                var projector = previewObj.GetComponent<Projector>();
                projector.material.SetFloat(Rotation, rad);
            }
            else
            {
                var renderer = previewObj.GetComponent<MeshRenderer>();
                renderer.sharedMaterial.SetFloat(Rotation, rad);
            }
        }

        public void MoveTo(Vector3 worldPosition)
        {
            if(!previewObj)
            {
                return; 
            }
            previewObj.transform.position = worldPosition;
        }

        public void SetNormalizedBrushCenter(Vector2 normalizedBrushCenter)
        {
            if (renderPipeline != RenderPipeline.Builtin)
            {
                var renderer = previewObj.GetComponent<MeshRenderer>();
                renderer.sharedMaterial.SetVector("_BrushCenter", normalizedBrushCenter);
            }
            else
            {
                //nothing
            }
        }

        public void SetNormalizedBrushSize(float normalizeBrushSize)
        {
            if (renderPipeline != RenderPipeline.Builtin)
            {
                var renderer = previewObj.GetComponent<MeshRenderer>();
                renderer.sharedMaterial.SetFloat("_NormalizedBrushSize", normalizeBrushSize);
            }
            else
            {
                //nothing
            }
        }

        private void LoadShader()
        {
            if (shader != null && RenderPipelineUtil.Current == renderPipeline)
            {
                return;
            }

            renderPipeline = RenderPipelineUtil.Current;

            var urpShaderRelativePath = Utility.GetUnityPath(Res.ShaderDir + "PaintTexturePreview_URP.shader");

            switch (RenderPipelineUtil.Current)
            {
                case RenderPipeline.Builtin:
                    shader = Shader.Find("Hidden/MTE/PaintTexturePreview");
                    break;
                case RenderPipeline.URP:
                    this.shader = AssetDatabase.LoadAssetAtPath<Shader>(urpShaderRelativePath);
                    if (shader == null)
                    {
                        MTEDebug.LogError("MTE Preview shader for URP is not found.");
                    }
                    else
                    {
                        MTEDebug.Log("Loaded Preview shader for URP.");
                    }
                    break;
                //fallback to URP
                case RenderPipeline.HDRP://HDRP is not supported yet.
                default:
                    this.shader = AssetDatabase.LoadAssetAtPath<Shader>(urpShaderRelativePath);
                    if (shader == null)
                    {
                        MTEDebug.LogError("MTE Preview shader for URP (fallback) is not found.");
                    }
                    else
                    {
                        MTEDebug.Log("Loaded Preview shader for URP (fallback).");
                    }
                    break;
            }
        }

        private void CreatePreviewObject()
        {
            if (renderPipeline != RenderPipeline.Builtin)
            {
                previewObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                var boxCollider = previewObj.GetComponent<BoxCollider>();
                Object.DestroyImmediate(boxCollider);
                previewObj.name = "MTEPreview";

                var meshRenderer = previewObj.GetComponent<MeshRenderer>();
                var material = new Material(shader);
                meshRenderer.sharedMaterial = material;

                previewObj.transform.eulerAngles = new Vector3(90, 0, 0);
            }
            else
            {
                previewObj = new GameObject("MTEPreview");
                var projector = previewObj.AddComponent<Projector>();
                projector.material = new Material(shader);
                projector.orthographic = true;
                projector.nearClipPlane = -1000;
                projector.farClipPlane = 1000;
                projector.transform.Rotate(90, 0, 0);
            }
        }
    }
}
