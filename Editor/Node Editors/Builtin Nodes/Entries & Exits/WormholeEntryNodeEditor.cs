using UnityEditor;
using UnityEditor.Experimental.GraphView;

namespace Gork.Editor
{
    /// <summary>
    /// The node editor for the <see cref="WormholeEntryNode"/>.
    /// </summary>
    [CustomGorkNodeEditor(typeof(WormholeEntryNode))]
    public class WormholeEntryNodeEditor : GorkNodeEditor
    {
        private WormholeEntryNode _node;
        private SerializedProperty _tagNameProp;

        public override void SetupEditor()
        {
            _node = Node as WormholeEntryNode;
            _tagNameProp = FindProperty(nameof(_node.Tag));
        }

        public override void SetupDraw(Node node)
        {
            if (Expanded)
            {
                OnExpand();
            }
        }

        public override void OnRenameTag(string oldName, string newName)
        {
            if (_tagNameProp.stringValue == oldName)
            {
                _tagNameProp.stringValue = newName;
                serializedObject.ApplyModifiedProperties();
            }
        }

        public override void OnCollapse()
        {
            NodeView.outputContainer.Remove(IMGUIContainer);
        }

        public override void OnExpand()
        {
            NodeView.outputContainer.Add(IMGUIContainer);
        }

        protected override void OnInspectorGUI()
        {
            // Create a tag dropdown
            GorkEditorUtility.TagDropdown(_tagNameProp.stringValue, Graph.Tags, newTag =>
            {
                _tagNameProp.stringValue = newTag;
                serializedObject.ApplyModifiedProperties();
            }, true, "<Ignore Tags>");
        }
    }
}
