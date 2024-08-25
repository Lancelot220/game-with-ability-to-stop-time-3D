using UnityEngine;

namespace MTE
{
    [ExecuteAlways]//Ensures UV scales are applied in Editor.
    public class LayerUVScalesHolder : MonoBehaviour
    {
        [SerializeField, HideInInspector]
        public float[] uvScales = new float[12]
        {
            1, 1, 1, 1, //Layer 0,1,2,3
            1, 1, 1, 1, //Layer 4,5,6,7
            1, 1, 1, 1  //Layer 8,9,10,11
        };

        public void SetScale(int layerIndex, float scale)
        {
            uvScales[layerIndex] = scale;
        }

        public void Apply()
        {
            if (!GetMaterial(out var material, out var error))
            {
                Debug.LogError(error);
            }
            material.SetFloatArray(ShaderPropertyID.LayerUVScales, uvScales);
        }
        
#if UNITY_EDITOR
        void OnWillRenderObject()
        {
            //Ensure that the material array applies when code recompile starts in Editor
            //https://forum.unity.com/threads/unity-resets-all-my-shader-properties-specifically-my-arrays.428318/#post-2769535
            Apply();
        }
#endif

        public void OnEnable()
        {
            Apply();
        }

        public bool GetMaterial(out Material material, out string error)
        {
            material = null;
            error = null;
            var meshRenderer = GetComponent<Renderer>();
            if (!meshRenderer)
            {
                error = $"Cannot find Renderer component on GameObject<{gameObject}>.";
                return false;
            }

            material = meshRenderer.sharedMaterial;
            if (!material)
            {
                error = $"Cannot get material used on GameObject<{gameObject.name}>. Make sure Renderer material is assigned.";
                return false;
            }

            return true;
        }

        public bool GetLayerNumber(Material material, out int layerNumber, out string error)
        {
            layerNumber = -1;
            error = null;

            bool hasArray0Property = material.HasProperty(ShaderPropertyID._TextureArray0);
            bool hasArray1Property = material.HasProperty(ShaderPropertyID._TextureArray1);

            if (!hasArray0Property && !hasArray1Property)
            {
                error = $"Cannot find property <{TextureArrayShaderPropertyNames.TextureArray0PropertyName}> or <{TextureArrayShaderPropertyNames.TextureArray1PropertyName}> on material <{material.name}>. Make sure to use a property name that's compatible with MTE if you are using a custom shader.";
                return false;
            }

            Texture2DArray array0 = null;
            Texture2DArray array1 = null;
            if (hasArray0Property)
            {
                array0 = material.GetTexture(ShaderPropertyID._TextureArray0) as Texture2DArray;
            }
            if (hasArray1Property)
            {
                array1 = material.GetTexture(ShaderPropertyID._TextureArray1) as Texture2DArray;
            }

            if (!array0 && !array1)
            {
                error = $"Cannot get a Texture2DArray from property <{TextureArrayShaderPropertyNames.TextureArray0PropertyName}> or <{TextureArrayShaderPropertyNames.TextureArray1PropertyName}> on material <{material.name}>." +
                        " Make sure at least one of them is assigned.";
                return false;
            }

            var textureArray = array0 ? array0 : array1;
            System.Diagnostics.Debug.Assert(textureArray != null, $"{nameof(textureArray)} != null");
            layerNumber = textureArray.depth;
            return true;
        }

    }
}
