using UnityEditor;
using UnityEngine;

namespace MTE
{
    internal static class GrassDetailPreview
    {
        public static void DrawGrassPreview(GrassDetail detail, Rect containerRect)
        {
            //draw preview texture
            LoadPrototypePreviews();
            var grassType = detail.GrassType;
            Texture grassMeshPreviewTexture = null;
            switch (grassType)
            {
                case GrassType.OneQuad:
                    grassMeshPreviewTexture = s_grassPrototypeQuadPreviewTexture;
                    break;
                case GrassType.ThreeQuad:
                    grassMeshPreviewTexture = s_grassPrototypeStarPreviewTexture;
                    break;
                case GrassType.CustomMesh:
                    Utility.LoadMeshPreviewAsync(detail.GrassMesh, t => grassMeshPreviewTexture = t);
                    break;
            }

            var rect = containerRect;
            rect.min += new Vector2(4, 4);
            rect.size = new Vector2(64, 64);
            if (grassMeshPreviewTexture)
            {
                GUI.DrawTexture(rect, grassMeshPreviewTexture);
            }

            //draw texture
            var material = detail.Material;
            var texture = material ? material.GetTexture("_MainTex") : null;
            if (texture)
            {
                rect.min += new Vector2(32, 32);
                rect.size = new Vector2(32, 32);
                GUI.DrawTexture(rect, texture);
            }

            var textRect = containerRect;
            var rectMin = textRect.min;
            rectMin.y = rect.max.y - EditorStyles.miniBoldLabel.lineHeight;
            textRect.min = rectMin;
            string objectName = detail.name;
            if (string.IsNullOrEmpty(detail.name))
            {
                switch (grassType)
                {
                    case GrassType.OneQuad:
                        objectName = "quad";
                        break;
                    case GrassType.ThreeQuad:
                        objectName = "star";
                        break;
                    case GrassType.CustomMesh:
                        if (detail.grassMesh)
                        {
                            objectName = detail.grassMesh.name;
                        }
                        else
                        {
                            objectName = "(missing mesh)";
                        }
                        break;
                }
            }
            GUI.Label(textRect, objectName, MTEStyles.middleLeftMiniBoldLabel);
        }

        private static void LoadPrototypePreviews()
        {
            var quadMesh = Resources.Load<Mesh>("Grass/Prototype_GrassQuad");
            Utility.LoadMeshPreviewAsync(quadMesh, t => { s_grassPrototypeQuadPreviewTexture = t; });
            var starMesh = Resources.Load<Mesh>("Grass/Prototype_GrassStar");
            Utility.LoadMeshPreviewAsync(starMesh, t => { s_grassPrototypeStarPreviewTexture = t; });
        }

        private static Texture2D s_grassPrototypeQuadPreviewTexture;
        private static Texture2D s_grassPrototypeStarPreviewTexture;
    }
}