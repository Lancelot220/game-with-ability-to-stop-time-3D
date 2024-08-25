using UnityEngine;

namespace MTE
{
#if UNITY_EDITOR
    [UnityEngine.Scripting.APIUpdating.MovedFrom("PBRLayer")]
#endif
    [System.Serializable]
    public class TextureLayer
    {
        /// <summary>
        /// Albedo/Color texture
        /// </summary>
        public Texture2D Albedo;

        /// <summary>
        /// Roughness or Smoothness texture
        /// </summary>
        public Texture2D Roughness;

        /// <summary>
        /// Normal texture
        /// </summary>
        public Texture2D Normal;

        /// <summary>
        /// AO texture
        /// </summary>
        public Texture2D AO;

        /// <summary>
        /// Height texture (not used)
        /// </summary>
        public Texture2D Height;

        /// <summary>
        /// Metallic texture
        /// </summary>
        public Texture2D Metallic;

        //following is not used TODO

        public float AOScalar = 1.0f;
        public float MetallicScalar = 0.0f;
        public float RoughnessScalar = 1.0f;

        public Vector2 TextureScale = new Vector2(1, 1);//Only x is used when converting Unity Terrain
        public Vector2 TextureOffset = new Vector2(1, 1);
        public float NormalScale = 1.0f;

        public Color SpecularColor = new Color(0,0,0,0);

        public bool IsReadyForPBRTextureArray()
        {
            return Albedo != null && Roughness != null && Normal != null && AO != null;
        }
    }

    public enum TextureType
    {
        Albedo,
        Roughness,
        Normal,
        AO,
        Height,
        Metallic
    }
}