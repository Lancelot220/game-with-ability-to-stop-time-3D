using System;
using UnityEngine;

namespace MTE
{
    /// <summary>
    /// Grass Prototype Information
    /// </summary>
    [Serializable]
    public class GrassDetail : Detail
    {
        public const float DefaultMinWidth = 0.1f;
        public const float DefaultMaxWidth = 0.2f;
        public const float DefaultMinHeight = 0.1f;
        public const float DefaultMaxHeight = 0.2f;
        public const GrassType DefaultGrassType = GrassType.OneQuad;

        [SerializeField]
        public string name;
        [SerializeField]
        public bool autoName;
        [SerializeField]
        public Material material;
        [SerializeField]
        public float minWidth = DefaultMinWidth;
        [SerializeField]
        public float maxWidth = DefaultMaxWidth;
        [SerializeField]
        public float minHeight = DefaultMinHeight;
        [SerializeField]
        public float maxHeight = DefaultMaxHeight;
        [SerializeField]
        public GrassType grassType = DefaultGrassType;
        [SerializeField]
        public Mesh grassMesh;


        public Material Material
        {
            get
            {
                return this.material;
            }

            set
            {
                this.material = value;
            }
        }

        public float MinWidth
        {
            get
            {
                return this.minWidth;
            }

            set
            {
                this.minWidth = value;
            }
        }

        public float MaxWidth
        {
            get
            {
                return this.maxWidth;
            }

            set
            {
                this.maxWidth = value;
            }
        }

        public float MinHeight
        {
            get
            {
                return this.minHeight;
            }

            set
            {
                this.minHeight = value;
            }
        }

        public float MaxHeight
        {
            get
            {
                return this.maxHeight;
            }

            set
            {
                this.maxHeight = value;
            }
        }

        public GrassType GrassType
        {
            get { return this.grassType; }
            set { this.grassType = value; }
        }

        public Mesh GrassMesh
        {
            get { return this.grassMesh; }
            set { this.grassMesh = value; }
        }

        public GrassDetail ShallowCopy()
        {
            var copy = new GrassDetail();
            copy.grassMesh = grassMesh ;
            copy.material  = material  ;
            copy.minWidth  = minWidth  ;
            copy.maxWidth  = maxWidth  ;
            copy.minHeight = minHeight ;
            copy.maxHeight = maxHeight ;
            copy.grassType = grassType ;
            return copy;
        }
    }
}