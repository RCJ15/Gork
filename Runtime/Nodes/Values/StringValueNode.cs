using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Experimental.GraphView;
#endif

namespace Gork
{
    /// <summary>
    /// Gives a constant <see cref="string"/> value.
    /// </summary>
    [GorkNodeInfo("Values/String Value", GorkColors.STRING_COLOR, 3)]
    [NoInputPorts]
    [GorkOutputPort("Value", typeof(string))]
    public class StringValueNode : GorkNode
    {
        [TextArea(0,5)] public string Value = "Text";

#if UNITY_EDITOR
        public override float InspectorFieldWidth => 70;

        public override void Initialize(Node node)
        {
            if (Expanded)
            {
                OnExpand();
            }
        }

        public override void OnCollapse()
        {
            NodeView.inputContainer.Remove(IMGUIContainer);
        }

        public override void OnExpand()
        {
            NodeView.inputContainer.Add(IMGUIContainer);
        }

        protected override void OnInspectorGUI()
        {
            SetupInspector();

            SerializedProperty prop = serializedObject.FindProperty(nameof(Value));

            Rect rect = EditorGUILayout.GetControlRect();

            string value = prop.stringValue;

            int linebreakCount = value.Count(c => c == '\n');

            float addedHeight = (float)linebreakCount * 14.5f;
            rect.height = 18 + addedHeight;

            prop.stringValue = EditorGUI.TextArea(rect, value);
            //EditorGUILayout.PropertyField(, GUIContent.none);

            EditorGUILayout.Space(addedHeight);

            serializedObject.ApplyModifiedProperties();
        }
#endif

        public override string StringCall(int port)
        {
            return Value;
        }
    }
}
