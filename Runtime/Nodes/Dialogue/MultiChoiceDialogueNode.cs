using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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
    [GorkNodeInfo("Dialogue/Multi Choice Dialogue", GorkColors.DIALOGUE_COLOR, 1)]
    [GorkInputPort("Trigger")]
    public class MultiChoiceDialogueNode : GorkNode
    {
        [TextArea(1, 5)]
        public string[] Choices = new string[] { "Yes", "No" };

        protected override void BuildOutputTypesList(List<Type> list)
        {
            int choicesLength = Choices.Length;

            for (int i = 0; i < choicesLength; i++)
            {
                list.Add(null);
            }
        }

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

            int? removalStartIndex = null;
            int removalCount = 0;

            for (int i = 0; i < max; i++)
            {
                if (i >= length)
                {
                    if (!removalStartIndex.HasValue)
                    {
                        removalStartIndex = i;
                    }

                    removalCount++;
                    continue;
                }

                SetOutputPort(i, $"Choice {i + 1}");
            }

            if (removalStartIndex.HasValue)
            {
                DeleteOutputPortRange(removalStartIndex.Value, removalCount);
            }
        }
#endif
    }
}
