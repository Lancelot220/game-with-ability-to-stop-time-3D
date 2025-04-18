#if UNITY_EDITOR
using System.Text;
using UnityEditor;
using UnityEngine;

namespace MTE
{
    [CustomEditor(typeof(MeshTerrain))]
    public class MeshTerrainInspector : Editor
    {
        private void OnEnable()
        {
            meshTerrain = target as MeshTerrain;
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var oldColor = GUI.backgroundColor;
            if (enableMouseWeightSample)
            {
                GUI.backgroundColor = Color.green;
            }
            enableMouseWeightSample = GUILayout.Toggle(enableMouseWeightSample, "SampleHeight", "button");
            GUI.backgroundColor = oldColor;
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            if (!enableMouseWeightSample)
            {
                return;
            }
            
            if (!(EditorWindow.mouseOverWindow is SceneView))
            {
                return;
            }

            var e = Event.current;
            var ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            RaycastHit hit;
            if (!Physics.Raycast(ray, out hit, Mathf.Infinity, ~meshTerrain.gameObject.layer))
            {
                return;
            }

            stringBuilder.Clear();
            var weights = meshTerrain.SampleWeight(hit.textureCoord);
            for (int i = 0; i < weights.Count; i++)
            {
                stringBuilder.AppendLine($"Layer[{i:00}] = {weights[i]}");
            }

            Handles.BeginGUI();
            GUILayout.Label(stringBuilder.ToString());
            Handles.EndGUI();

            sceneView.Repaint();
        }

        private static bool enableMouseWeightSample = true;

        private MeshTerrain meshTerrain;
        private readonly StringBuilder stringBuilder = new StringBuilder(128);
    }
}

#endif