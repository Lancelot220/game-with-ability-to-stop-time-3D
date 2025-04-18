using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine;

namespace MTE
{
    [CustomEditor(typeof(ObjectLoader))]
    internal class ObjectLoaderEditor : Editor
    {
        private SerializedProperty script;
        private SerializedProperty detailListProperty;

        public void OnEnable()
        {
            script = serializedObject.FindProperty("m_Script");
            detailListProperty = serializedObject.FindProperty(nameof(ObjectLoader.detailList));
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();

            GUI.enabled = false;
            EditorGUILayout.PropertyField(script, true);
            GUI.enabled = true;
            EditorGUILayout.PropertyField(detailListProperty);
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}