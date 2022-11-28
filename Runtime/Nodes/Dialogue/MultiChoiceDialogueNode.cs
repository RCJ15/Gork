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
    /// A <see cref="GorkNode"/> that will start a mulit choice option list.
    /// </summary>
    [GorkNodeInfo("Dialogue/Multi Choice Dialogue", GorkColors.DIALOGUE_COLOR)]
    [GorkInputPort("Trigger")]
    public class MultiChoiceDialogueNode : GorkNode
    {
        [TextArea(1, 5)]
        public string[] Choices = new string[] { "Yes", "No" };

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

            UpdatePorts();
        }

        private void DrawHeaderCallback(Rect rect)
        {
            EditorGUI.LabelField(rect, "Choices");
        }

        private void DrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty prop = _list.serializedProperty.GetArrayElementAtIndex(index);
            
            EditorGUI.PropertyField(rect, prop, new GUIContent($"Choice {index + 1}"));
        }

        protected override void OnInspectorGUI()
        {
            EditorGUILayout.Space();

            serializedObject.UpdateIfRequiredOrScript();

            int oldSize = _property.arraySize;

            _list.DoLayoutList();

            if (serializedObject.ApplyModifiedProperties())
            {
                if (oldSize != _property.arraySize)
                {
                    UpdatePorts();

                    UpdateNodeView();
                }
            }
        }

        private void UpdatePorts()
        {
            int length = _property.arraySize;
            int portCount = OutputPorts.Count;
            int max = Mathf.Max(portCount, length);

            for (int i = 0; i < max; i++)
            {
                if (i >= length)
                {
                    DeleteOutputPort(i);
                    continue;
                }

                SetOutputPort(i, $"Choice {i + 1}");
            }
        }
#endif
    }
}
