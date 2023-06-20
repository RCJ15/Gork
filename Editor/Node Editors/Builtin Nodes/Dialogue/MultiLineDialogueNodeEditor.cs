using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace Gork.Editor
{
    /// <summary>
    /// The node editor for the <see cref="MultiChoiceDialogueNode"/>.
    /// </summary>
    [CustomGorkNodeEditor(typeof(MultiChoiceDialogueNode))]
    public class MultiLineDialogueNodeEditor : GorkNodeEditor
    {
        public const string PORT_FORMAT = "Choice {0}";

        private ReorderableList _list;
        private SerializedProperty _property;

        public override void OnViewEnable()
        {
            _property = FindProperty("Choices");

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
            int propertyCount = _property.arraySize;
            int currentCount = CustomOutputPorts.Count;
            int max = Mathf.Max(currentCount, propertyCount);

            int? removalStartIndex = null;
            int removalCount = 0;

            for (int i = 0; i < max; i++)
            {
                string PortName() => string.Format(PORT_FORMAT, i + 1);

                // Add a new port
                if (i >= currentCount)
                {
                    CustomOutputPorts.Add(new NodePort(PortName()));
                }
                // Remove the port
                else if (i >= propertyCount)
                {
                    if (!removalStartIndex.HasValue)
                    {
                        removalStartIndex = i;
                    }

                    removalCount++;
                }
                // Update name of the port
                else
                {
                    CustomOutputPorts[i].Name = PortName();
                }
            }

            // Remove all of the marked ports
            if (removalStartIndex.HasValue)
            {
                CustomOutputPorts.RemoveRange(removalStartIndex.Value, removalCount);
            }
        }
    }
}
