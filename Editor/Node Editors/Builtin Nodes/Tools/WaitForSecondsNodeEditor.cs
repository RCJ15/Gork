using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;

namespace Gork.Editor
{
    /// <summary>
    /// The node editor for the <see cref="WaitForSecondsNode"/>.
    /// </summary>
    [CustomGorkNodeEditor(typeof(WaitForSecondsNode), Inherit = true)]
    public class WaitForSecondsNodeEditor : GorkNodeEditor
    {
        private WaitForSecondsNode _node;
        private SerializedProperty _timeProp;

        public override float InspectorLabelWidth => 30;
        public override float InspectorFieldWidth => 70;

        private bool _imguiAttached = true;

        public override void SetupEditor()
        {
            _node = Node as WaitForSecondsNode;
            _timeProp = FindProperty(nameof(_node.Time));

            UpdateTitle(false);
        }

        protected override void Draw(List<VisualElement> elements)
        {
            if (HasInputConnection(1))
            {
                _imguiAttached = false;
                return;
            }

            base.Draw(elements);
        }

        protected override void OnInspectorGUI()
        {
            SetupInspector();

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_timeProp);

            EditorGUILayout.Space();

            // Don't allow numbers below 0
            if (_timeProp.floatValue < 0)
            {
                _timeProp.floatValue = 0;
            }

            // Update name when the value of the time is changed
            if (serializedObject.ApplyModifiedProperties())
            {
                UpdateTitle();
            }
        }

        public override void OnInputConnectionAdded(int portIndex, GorkNode otherNode, int otherNodePortIndex)
        {
            if (portIndex != 1)
            {
                return;
            }

            if (_imguiAttached)
            {
                extensionContainer.Remove(IMGUIContainer);
                _imguiAttached = false;
            }

            UpdateTitle();
        }

        public override void OnInputConnectionRemoved(int portIndex, GorkNode otherNode, int otherNodePortIndex)
        {
            if (portIndex != 1)
            {
                return;
            }

            if (!_imguiAttached)
            {
                extensionContainer.Add(IMGUIContainer);
                NodeView.RefreshExpandedState();
                _imguiAttached = true;
            }

            UpdateTitle();
        }

        private void UpdateTitle(bool updateNode = true)
        {
            if (HasInputConnection(1))
            {
                ResetTitle(updateNode);
            }
            else
            {
                float time = _timeProp.floatValue;
                SetTitle(string.Format(_node.TitleFormat, time, time == 1 ? "" : "s"), updateNode);
            }
        }
    }
}
