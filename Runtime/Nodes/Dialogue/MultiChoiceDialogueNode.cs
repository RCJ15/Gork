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
    /// A <see cref="GorkNode"/> that will start a multi choice option list.
    /// </summary>
    [GorkNodeInfo("Dialogue/Multi Choice Dialogue", GorkColors.DIALOGUE_COLOR, 1, 
        WikiSummary = "Will start a multi choice dialogue list when activated",
        WikiDescription = "This node will allow you to create dynamic dialogue through GorkGraph.\n" +
        "The way that the multi choice dialogue is displayed is dependant on the GorkDialogueProvider and how it handles mutli choice dialogue.\n" +
        "This node also will dynamically add output ports based on the amount of choices you have.",
        WikiOutputPorts = "This node has dynamic output ports.\nThis means that the amount of output ports will change based on it's amount of given choices.\n" +
        "Each output port will be connected to it's choice of the same name.\n\n" +
        "<b>Example:</b>\n" +
        "You have added 2 choices to the list: \"Yes\" & \"No\".\n" +
        "This means that 2 output ports are available: \"Choice 1\" & \"Choice 2\"\n" +
        "\"Choice 1\" will be triggered if \"Yes\" is the selected answer.\n" +
        "\"Choice 2\" will be triggered if \"No\" is the selected answer and so on...",
        WikiUsage = "Use this node to create multiple options that the player can choose in a dialogue")]
    [GorkInputPort("Trigger", WikiDescription = "Will activate the multi choice dialogue when triggered")]
    public class MultiChoiceDialogueNode : GorkNode
    {
        [GorkWikiInfo("An array of string that represent the choices that the player will be able to choose from")]
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
