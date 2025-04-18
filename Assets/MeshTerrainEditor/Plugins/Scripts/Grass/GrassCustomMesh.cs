using System;
using UnityEngine;

namespace MTE
{
    [Serializable]
    public class GrassCustomMesh
    {
        [Header("Basic")]
        [SerializeField]
        private Mesh mesh;
        [SerializeField]
        private Material material;
        [SerializeField]
        private Vector3 position;
        [SerializeField]
        private float rotationY;
        [SerializeField]
        private float width;
        [SerializeField]
        private float height;

        [Space(10)]

        [Header("Lightmap")]
        [SerializeField]
        private int lightmapIndex;
        [SerializeField]
        private Vector4 lightmapScaleOffset;

        public Mesh Mesh => mesh;

        public Material Material => material;

        public Vector3 Position
        {
            get => position;
            set => position = value;
        }

        public float RotationY
        {
            get => rotationY;
            set => rotationY = value;
        }

        public float Width => width;

        public float Height => height;

        public int LightmapIndex
        {
            get => lightmapIndex;
            set => lightmapIndex = value;
        }

        public Vector4 LightmapScaleOffset
        {
            get => lightmapScaleOffset;
            set => lightmapScaleOffset = value;
        }

        public bool IsValid
        {
            get { return this.material != null && this.mesh != null; }
        }

        /// <summary>Save lightmapping data to this GrassStar.</summary>
        /// <param name="lightmapIndex"></param>
        /// <param name="lightmapScaleOffset"></param>
        public void SaveLightmapData(int lightmapIndex, Vector4 lightmapScaleOffset)
        {
            this.lightmapIndex = lightmapIndex;
            this.lightmapScaleOffset = lightmapScaleOffset;
        }

        /// <summary>Initialize this Grass instance with custom mesh.</summary>
        /// <param name="mesh">grass Mesh</param>
        /// <param name="material">grass Material</param>
        /// <param name="position">position in world space</param>
        /// <param name="rotationY">rotation Y (Euler angles Y)</param>
        /// <param name="width">width of a quad</param>
        /// <param name="height">height of a quad</param>
        public void Init(Mesh mesh, Material material, Vector3 position, float rotationY, float width, float height)
        {
            this.mesh = mesh;
            this.material = material;
            this.position = position;
            this.rotationY = rotationY;
            this.width = width;
            this.height = height;
        }
    }
}