using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditorInternal;
using UnityEditor.Experimental.GraphView;
#endif

namespace Gork
{
    /// <summary>
    /// 
    /// </summary>
    [GorkNodeInfo("Dialogue/Multi Choice Dialogue", DialogueNode.COLOR)]
    [NoOutputPorts]
    public class MultiChoiceDialogueNode : GorkNode
    {
        [TextArea(1, 5)]
        public string[] Choices = new string[] { "Choice 1" };

#if UNITY_EDITOR
        public override void Initialize(Node node)
        {
            base.Initialize(node);
        }

        private ReorderableList _list;
        private SerializedProperty _property;

        public override void OnViewEnable()
        {
            _property = serializedObject.FindProperty("Choices");

            _list = new ReorderableList(serializedObject,
                _property,
                true, true, true, true);

            _list.drawHeaderCallback = DrawHeaderCallback;
            _list.drawElementCallback = DrawElementCallback;

            _list.elementHeight = 60;
        }

        private void DrawHeaderCallback(Rect rect)
        {
            EditorGUI.LabelField(rect, "Choices");
        }

        private void DrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty prop = _list.serializedProperty.GetArrayElementAtIndex(index);

            EditorGUI.PropertyField(rect, prop);
        }

        protected override void OnInspectorGUI()
        {
            EditorGUILayout.Space();

            serializedObject.UpdateIfRequiredOrScript();

            _list.DoLayoutList();

            serializedObject.ApplyModifiedProperties();
        }
#endif
    }
}
