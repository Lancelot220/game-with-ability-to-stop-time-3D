#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace MTE
{
    [CustomEditor(typeof(LayerUVScalesHolder))]
    public class LayerUVScalesHolderEditor : Editor
    {
        private LayerUVScalesHolder _holder;

        public void OnEnable()
        {
            _holder = (LayerUVScalesHolder)target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (!_holder.GetMaterial(out var material, out var materialError))
            {
                EditorGUILayout.HelpBox($"Cannot get material: {materialError}", MessageType.Error);
                return;
            }

            if (!_holder.GetLayerNumber(material, out var layerNumber, out var error))
            {
                EditorGUILayout.HelpBox($"Cannot get correct layer number: {error}", MessageType.Error);
                return;
            }

            EditorGUI.BeginChangeCheck();
            for (int i = 0; i < layerNumber; i++)
            {
                //TODO show layer name
                _holder.uvScales[i] = EditorGUILayout.FloatField($"Layer{i}", _holder.uvScales[i]);
            }
            if (EditorGUI.EndChangeCheck())
            {
                _holder.Apply();
                EditorUtility.SetDirty(_holder);
            }
        }
    }
}
#endif
