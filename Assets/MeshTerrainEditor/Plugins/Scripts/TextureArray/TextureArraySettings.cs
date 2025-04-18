using System.Collections.Generic;
using UnityEngine;

namespace MTE
{
    /// <summary>
    /// Specifies textures and other required parameters when creating <see cref="Texture2DArray"/>.
    /// </summary>
#if UNITY_EDITOR
    [CreateAssetMenu(fileName = "NewTextureArraySettings.asset",
        menuName = "Mesh Terrain Editor/TextureArraySettings")]
    [UnityEngine.Scripting.APIUpdating.MovedFrom("PBRTextureArraySettings")]
#endif
    public class TextureArraySettings : ScriptableObject
    {
        public TextureArrayMode textureMode = TextureArrayMode.PBR;

        public string TextureArrayName = "NewTexture2DArray";

        public int TextureSize = 512;

        public List<TextureLayer> Layers = new List<TextureLayer>(12);

        public PerTextureSettings Array0Settings = new PerTextureSettings();
        public PerTextureSettings Array1Settings = new PerTextureSettings();

        public const string ColorArrayPostfix = "_" + Color;
        public const string ColorSmoothnessArrayPostfix = "_" + ColorSmoothness;
        public const string NormalArrayPostfix = "_" + Normal;
        public const string AlbedoArrayPostfix = "_" + Albedo;
        public const string AlbedoMetallicArrayPostfix = "_" + AlbedoMetallic;
        public const string RoughnessNormalAOArrayPostfix = "_" + RoughnessNormalAO;
        public const string Color = "Color";
        public const string ColorSmoothness = "ColorSmoothness";
        public const string Normal = "Normal";
        public const string Albedo = "Albedo";
        public const string AlbedoMetallic = "AlbedoMetallic";
        public const string RoughnessNormalAO = "RoughnessNormalAO";
        public const int MinLayerNumber = 2;
        public const int MaxLayerNumber = 12;

        public void SetCompressionForBothArrays(bool compressed, int compressionQuality = 50/*UnityEditor.TextureCompressionQuality.Normal*/)
        {
            Array0Settings.compressed = Array1Settings.compressed = compressed;
            Array0Settings.textureCompressionQuality = Array1Settings.textureCompressionQuality = compressionQuality;
        }

        public void SetTextureFormatForBothArrays(TextureFormat textureFormat)
        {
            Array0Settings.textureFormat = Array1Settings.textureFormat = textureFormat;
        }

        public void SetTextureSamplerStates(TextureWrapMode wrapMode, FilterMode filterMode, int anisoLevel)
        {
            Array0Settings.wrapMode = Array1Settings.wrapMode = wrapMode;
            Array0Settings.filterMode = Array1Settings.filterMode = filterMode;
            Array0Settings.anisoLevel = Array1Settings.anisoLevel = anisoLevel;
        }
    }

    public enum TextureArrayMode
    {
        /// <summary>
        /// Use color map only.
        /// </summary>
        Color,

        /// <summary>
        /// Use color and normal map.
        /// </summary>
        ColorAndNormal,

        /// <summary>
        /// Use the same texture sets as Unity's builtin Standard terrain material Default-Terrain-Standard.<br/>
        /// Array0: Albedo(RGB) Smoothness(A)<br/>
        /// Array1: Normal(RGB)
        /// </summary>
        LegacyUnityStandard,

        /// <summary>
        /// Use PBR maps including albedo, normal, roughness and AO. No metallic.
        /// </summary>
        PBR,
        
        /// <summary>
        /// Use PBR maps including albedo, normal, roughness, AO and metallic.
        ///
        /// Not supported in RuntimeTextureArrayLoader yet.
        /// </summary>
        PBRWithMetallic
    }
    
    [System.Serializable]
    public class PerTextureSettings
    {
        public bool compressed = true;
        public TextureFormat textureFormat;
        public int textureCompressionQuality = 50;/*UnityEditor.TextureCompressionQuality.Normal*/
        public TextureWrapMode wrapMode = TextureWrapMode.Repeat;
        public FilterMode filterMode = FilterMode.Bilinear;
        public int anisoLevel = 0;

        public PerTextureSettings ShallowClone()
        {
            return new PerTextureSettings
            {
                compressed = this.compressed,
                textureFormat = this.textureFormat,
                textureCompressionQuality = this.textureCompressionQuality,
                wrapMode = this.wrapMode,
                filterMode = this.filterMode,
                anisoLevel = this.anisoLevel
            };
        }
    }
}