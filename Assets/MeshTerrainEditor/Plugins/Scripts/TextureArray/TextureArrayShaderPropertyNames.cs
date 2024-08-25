using UnityEngine;

namespace MTE
{
    /// <summary>
    /// Shader property names used in MTE texture array shaders.
    /// </summary>
    public class TextureArrayShaderPropertyNames
    {
        public const string WeightMap0PropertyName = "_Control0";
        public const string WeightMap1PropertyName = "_Control1";
        public const string WeightMap2PropertyName = "_Control2";
        public const string TextureArray0PropertyName = "_TextureArray0";
        public const string AlbedoArrayPropertyName = TextureArray0PropertyName;
        public const string AlbedoMetallicArrayPropertyName = TextureArray0PropertyName;
        public const string TextureArray1PropertyName = "_TextureArray1";
        public const string NormalArrayPropertyName = TextureArray1PropertyName;
        public const string RoughnessNormalAOArrayPropertyName = TextureArray1PropertyName;
        public const string NormalIntensityPropertyName = "_NormalIntensity";
        public const string UVScalePropertyName = "_UVScale";//_UVScaleOffset is renamed to _UVScale and use single float instead of Vector2 at MTE 4.1.0
        public const string LayerUVScalesPropertyName = "LayerUVScales";
        public const string SpecularColorPropertyName = "_SpecularColor";

        public const string LegacySpecColorPropertyName = "_SpecColor";//for Unity's builtin BlinnPhong lighting model
        public const string LegacyShininessPropertyName = "_Shininess";//for Unity's builtin BlinnPhong lighting model
        public const string LegacyGlossPropertyName = "_Gloss";//for Unity's builtin BlinnPhong lighting model

        public static string[] ControlTexturePropertyNames =
        {
            WeightMap0PropertyName,
            WeightMap1PropertyName,
            WeightMap2PropertyName
        };

    }

    public static class ShaderPropertyID
    {
        public static readonly int _Control0 = Shader.PropertyToID(TextureArrayShaderPropertyNames.WeightMap0PropertyName);
        public static readonly int _Control1 = Shader.PropertyToID(TextureArrayShaderPropertyNames.WeightMap1PropertyName);
        public static readonly int _Control2 = Shader.PropertyToID(TextureArrayShaderPropertyNames.WeightMap2PropertyName);
        public static readonly int _TextureArray0 = Shader.PropertyToID(TextureArrayShaderPropertyNames.TextureArray0PropertyName);
        public static readonly int _TextureArray1 = Shader.PropertyToID(TextureArrayShaderPropertyNames.TextureArray1PropertyName);
        public static readonly int _UVScale = Shader.PropertyToID(TextureArrayShaderPropertyNames.UVScalePropertyName);
        public static readonly int LayerUVScales = Shader.PropertyToID(TextureArrayShaderPropertyNames.LayerUVScalesPropertyName);
        public static readonly int _SpecularColor = Shader.PropertyToID(TextureArrayShaderPropertyNames.SpecularColorPropertyName);
    }
}