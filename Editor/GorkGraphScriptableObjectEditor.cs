using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Gork.Editor
{
    /// <summary>
    /// The <see cref="UnityEditor.Editor"/> for the <see cref="GorkGraph"/> <see cref="ScriptableObject"/>.
    /// </summary>
    [CustomEditor(typeof(GorkGraph))]
    public class GorkGraphScriptableObjectEditor : UnityEditor.Editor
    {
        private GorkGraph _graph;

        private void OnEnable()
        {
            _graph = (GorkGraph)target;
        }

        public override void OnInspectorGUI()
        {
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("nodes"));
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("Open in Gork Graph Editor"))
            {
                GorkGraphEditor.Open(_graph);
            }
        }
    }
}
