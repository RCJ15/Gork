using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;

namespace Gork.Editor
{
    /// <summary>
    /// The node editor for the <see cref="StringFormatNode"/>.
    /// </summary>
    [CustomGorkNodeEditor(typeof(StringFormatNode))]
    public class StringFormatNodeEditor : GorkNodeEditor
    {
        private static readonly Regex Regex = new Regex("{[0-9]+}");

        private StringFormatNode _node;

        private SerializedProperty _formatProp;
        private SerializedProperty _highestArgIdProp;
        private SerializedProperty _argCountProp;
        private SerializedProperty _argIdsProp;

        public override void SetupEditor()
        {
            _node = Node as StringFormatNode;

            _formatProp = FindProperty(nameof(_node.Format));
            _highestArgIdProp = FindProperty("highestArgId");
            _argCountProp = FindProperty("argCount");
            _argIdsProp = FindProperty("argIds");
        }

        public override void OnViewEnable()
        {
            RegexCheck(_formatProp.stringValue);
        }

        protected override void OnInspectorGUI()
        {
            SetupInspector();

            EditorGUILayout.PropertyField(_formatProp, GUIContent.none);

            /*
            EditorGUILayout.PropertyField(_highestArgIdProp);
            EditorGUILayout.PropertyField(_argCountProp);
            EditorGUILayout.PropertyField(_argIdsProp);
            */

            if (serializedObject.ApplyModifiedProperties())
            {
                RegexCheck(_formatProp.stringValue);

                UpdateNodeView();
            }
        }

        private void RegexCheck(string text)
        {
            IEnumerable<Match> matches = Regex.Matches(text);
            List<string> matchesDistinct = new List<string>();
            _argIdsProp.ClearArray();

            _highestArgIdProp.intValue = 0;

            foreach (Match match in matches)
            {
                string value = match.Value;

                if (matchesDistinct.Contains(value))
                {
                    continue;
                }

                matchesDistinct.Add(value);

                int intValue = int.Parse(value.Trim('{', '}'));

                int index = _argIdsProp.arraySize;

                _argIdsProp.InsertArrayElementAtIndex(index);
                _argIdsProp.GetArrayElementAtIndex(index).intValue = intValue;

                if (intValue > _highestArgIdProp.intValue)
                {
                    _highestArgIdProp.intValue = intValue;
                }
            }

            _argCountProp.intValue = matchesDistinct.Count;

            if (_argIdsProp.arraySize > 0)
            {
                // Sort the array using quick sort
                QuickSort(0, _argIdsProp.arraySize - 1);
            }

            serializedObject.ApplyModifiedProperties();

            UpdatePorts();
        }

        /// <summary>
        /// This method is a modified Quick Sort algorithm implementation that was taken from https://code-maze.com/csharp-quicksort-algorithm/
        /// </summary>
        private void QuickSort(int leftIndex, int rightIndex)
        {
            int i = leftIndex;
            int j = rightIndex;

            int pivot = _argIdsProp.GetArrayElementAtIndex(leftIndex).intValue;

            while (i <= j)
            {
                while (_argIdsProp.GetArrayElementAtIndex(i).intValue < pivot)
                {
                    i++;
                }

                while (_argIdsProp.GetArrayElementAtIndex(j).intValue > pivot)
                {
                    j--;
                }

                if (i <= j)
                {
                    SerializedProperty iProp = _argIdsProp.GetArrayElementAtIndex(i);
                    SerializedProperty jProp = _argIdsProp.GetArrayElementAtIndex(j);

                    int temp = iProp.intValue;
                    iProp.intValue = jProp.intValue;
                    jProp.intValue = temp;

                    i++;
                    j--;
                }
            }

            if (leftIndex < j)
            {
                QuickSort(leftIndex, j);
            }
            if (i < rightIndex)
            {
                QuickSort(i, rightIndex);
            }
        }

        private void UpdatePorts()
        {
            int propertyCount = _argCountProp.intValue;
            int currentCount = CustomInputPorts.Count;
            int max = Mathf.Max(currentCount, propertyCount);

            int? removalStartIndex = null;
            int removalCount = 0;

            for (int i = 0; i < max; i++)
            {
                string PortName() => $"{{{_argIdsProp.GetArrayElementAtIndex(i).intValue}}}";

                // Add a new port
                if (i >= currentCount)
                {
                    CustomInputPorts.Add(new NodePort(PortName(), typeof(string)));
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
                    CustomInputPorts[i].Name = PortName();
                }
            }

            // Remove all of the marked ports
            if (removalStartIndex.HasValue)
            {
                CustomInputPorts.RemoveRange(removalStartIndex.Value, removalCount);
            }
        }
    }
}
