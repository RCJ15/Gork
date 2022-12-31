using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Experimental.GraphView;
#endif

namespace Gork
{
    /// <summary>
    /// Will stop all <see cref="GorkNode"/> that have a certain tag
    /// </summary>
    [GorkNodeInfo("Entries & Exits/Disrupt Node", GorkColors.ENTRY_COLOR, 2, 
        WikiSummary = "Will stop all nodes that have a certain tag",
        WikiDescription = "Uses a tag and stops all nodes that have that specific tag attached",
        WikiUsage = "Use this like how you would use a Stop Node, except that you only stop a few hand picked nodes instead of everything all at once"
        )]
    [GorkInputPort("Trigger", WikiDescription = "Will stop the tagged nodes when triggered")]
    [NoOutputPorts]
    public class DisruptNode : GorkNode
    {
        [GorkWikiInfo("The tag that will determine which nodes to stop.\nIs displayed like a dropdown in the editor")]
        public string Tag = "Tag Name";

        public override void NodeCall(int port)
        {
            // Stop all nodes with the set tag
            List<GorkNode> nodes = Graph.GetNodesWithTag(Tag);

            // No nodes with that tag :(
            if (nodes == null)
            {
                return;
            }

            foreach (GorkNode node in nodes)
            {
                Graph.OnNodeStop(node);
            }
        }

#if UNITY_EDITOR
        public override void Initialize(Node node)
        {
            OnExpand();
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
            // Do button
            if (!EditorGUILayout.DropdownButton(new GUIContent(Tag), FocusType.Keyboard))
            {
                // This is when the button is not pressed
                return;
            }

            // Button has been pressed

            // Get the name property
            SerializedProperty prop = serializedObject.FindProperty(nameof(Tag));
            string propValue = prop.stringValue;

            // Create an empty generic menu which will be our dropdown menu
            GenericMenu menu = new GenericMenu();

            // Loop through all tags
            foreach (string tag in Graph.Tags)
            {
                bool on = propValue == tag;

                // Add the menu item
                menu.AddItem(new GUIContent(tag), on, () =>
                {
                    // Do nothing if the tag is already set to this value
                    if (on)
                    {
                        return;
                    }

                    // Set name value and apply the changes
                    prop.stringValue = tag;
                    serializedObject.ApplyModifiedProperties();
                });
            }

            // Display the menu as a dropdown menu at the correct position
            Rect rect = EditorGUILayout.GetControlRect();
            rect.y += 18;
            menu.DropDown(rect);
        }
#endif
    }
}
