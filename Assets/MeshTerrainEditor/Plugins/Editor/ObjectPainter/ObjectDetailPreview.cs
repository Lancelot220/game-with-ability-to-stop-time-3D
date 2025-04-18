using MTE;
using UnityEditor;
using UnityEngine;

public static class ObjectDetailPreview
{
    public static void DrawObjectPreview(ObjectDetail detail, Rect buttonRect)
    {
        if (detail == null)
        {
            Debug.LogWarning("Ignored invalid Object detail");
            return;
        }

        var rect = buttonRect;
        rect.min += new Vector2(5, 5);
        rect.max -= new Vector2(5, 5);

        //draw preview texture
        if (detail.Object)
        {
            var previewTexture = AssetPreview.GetAssetPreview(detail.Object);
            if (previewTexture)
            {
                GUI.DrawTexture(rect, previewTexture);
            }
        }

        var textRect = buttonRect;
        var rectMin = textRect.min;
        rectMin.y = rect.max.y - EditorStyles.miniBoldLabel.lineHeight;
        textRect.min = rectMin;
        string objectName = detail.name;
        if (string.IsNullOrEmpty(objectName))
        {
            if (detail.Object)
            {
                objectName = detail.Object.name;
            }
            else
            {
                objectName = "(missing)";
            }
        }

        GUI.Label(textRect, objectName, MTEStyles.middleLeftMiniBoldLabel);
    }
}