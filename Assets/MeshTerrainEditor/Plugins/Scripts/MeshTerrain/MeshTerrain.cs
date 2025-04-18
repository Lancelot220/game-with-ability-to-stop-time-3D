using System;
using System.Collections.Generic;
using UnityEngine;

namespace MTE
{
#if UNITY_EDITOR
    //Keep this attribute to make MeshTerrainInspector running in edit mode
    [ExecuteInEditMode]
#endif
    public class MeshTerrain : MonoBehaviour
    {
        /// <summary>
        /// Samples the weight at a position in world space, which is clamped to mesh-terrain's bounds.
        /// </summary>
        /// <param name="worldPosition">sample position in world space</param>
        /// <remarks>This method is slow, don't call every frame.</remarks>
        public List<float> SampleWeight(Vector3 worldPosition)
        {
            if (meshRenderer == null)
            {
                meshRenderer = GetComponent<MeshRenderer>();
            }
            var bounds = meshRenderer.bounds;
            Rect boundRect2D = Rect.MinMaxRect(bounds.min.x, bounds.min.z, bounds.max.x, bounds.max.z);
            Vector2 position2D = new Vector2(worldPosition.x, worldPosition.z);
            position2D.x = Mathf.Clamp(position2D.x, boundRect2D.xMin, boundRect2D.xMax);
            position2D.y = Mathf.Clamp(position2D.y, boundRect2D.yMin, boundRect2D.yMax);
            //Now position2D is a 2D point on x-z plane, inside boundRect2D.
            var weightMapUV = Rect.PointToNormalized(boundRect2D, position2D);
            return SampleWeight(weightMapUV);
        }

        /// <summary>
        /// Samples the weight with a specific uv
        /// </summary>
        /// <param name="weightMapUV">sample uv of the point</param>
        /// <remarks>This method is slow, don't call every frame.</remarks>
        /// <example>
        /// This shows how to sample weights at a hit point of raycast.
        /// <code>
        ///    Ray ray = new Ray(new Vector3(0, 10000, 0), Vector3.down);
        ///    RaycastHit hit;
        ///    if (Physics.Raycast(ray, out hit))
        ///    {
        ///        var weightsAtHitPoint = SampleWeight(hit.textureCoord);
        ///    }
        /// </code>
        /// </example>
        public List<float> SampleWeight(Vector2 weightMapUV)
        {
            if (meshRenderer == null)
            {
                meshRenderer = GetComponent<MeshRenderer>();
            }
            var material = meshRenderer.sharedMaterial;
            var controlTexture = material.GetTexture(LegacyControlTextureNames[0]);
            bool isLegacy = controlTexture != null;
            List<Texture2D> weightMaps = new List<Texture2D>(3);
            if (isLegacy)
            {
                weightMaps.Add(controlTexture as Texture2D);
                var controlExtraTexture = material.GetTexture(LegacyControlTextureNames[1]);
                if (controlExtraTexture != null)
                {
                    weightMaps.Add(controlExtraTexture as Texture2D);
                }
            }
            else
            {
                for (int i = 0; i < WeightMapNames.Count; i++)
                {
                    var weightTexture = material.GetTexture(WeightMapNames[i]);
                    if (weightTexture == null)
                    {
                        throw new InvalidOperationException("MeshTerrain don't have a WeightMap/Control texture.");
                    }

                    var weightMap = weightTexture as Texture2D;
                    if (weightMap == null)
                    {
                        throw new InvalidOperationException("The WeightMap/Control texture is not a Texture2D.");
                    }
                    weightMaps.Add(weightMap);
                }
            }
            
            var result = new List<float>(12);
            foreach (var weightMap in weightMaps)
            {
                var sample = weightMap.GetPixelBilinear(weightMapUV.x, weightMapUV.y);
                for (int i = 0; i < 4; i++)
                {
                    result.Add(sample[i]);
                }
            }

            return result;
        }

        private MeshRenderer meshRenderer;

        private readonly List<string> LegacyControlTextureNames = new List<string>
        {
            "_Control",
            "_ControlExtra",
        };

        private readonly List<string> WeightMapNames = new List<string>
        {
            "WeightMap0",
            "WeightMap1",
            "WeightMap2",
        };
    }
}