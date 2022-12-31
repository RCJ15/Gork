using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Experimental.GraphView;
#endif

namespace Gork
{
    /// <summary>
    /// A node that will transport it's signal to a <see cref="WormholeExitNode"/> with the same tag. Use this to fix spaghetti connections and create reusable "functions" in your graphs
    /// </summary>
    [GorkNodeInfo("Entries & Exits/Wormhole Entry", GorkColors.WORMHOLE_COLOR, 10,
        WikiSummary = "Will transport it's signal to a Wormhole Exit node with the same tag",
        WikiDescription = "Will call upon all Womrhole Exit nodes that have the same tag as this nodes tag field.\n" +
        "The tag field can also be set to \"" + NoTagText + "\" which will make this node activate ALL Wormhole Exit nodes in the entire graph",
        WikiUsage = "Can be used as a way to fix connections that reach halfway accross the entire graph.\nCan also be used to make reusable nodes that can be called over and over again, like a function in code"
        )]
    [GorkInputPort("Signal", WikiDescription = "Will trigger all other Wormhole Exit nodes with the same tag")]
    [NoOutputPorts]
    public class WormholeEntryNode : GorkNode
    {
        [GorkWikiInfo("The tag that will determine which Wormhole Exit nodes to call.\nIs displayed like a dropdown in the editor")]
        public string Tag = "";

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

        private const string NoTagText = "<Ignore Tags>";
        protected override void OnInspectorGUI()
        {
            // Do button
            if (!EditorGUILayout.DropdownButton(new GUIContent(string.IsNullOrEmpty(Tag) ? NoTagText : Tag), FocusType.Keyboard))
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
            // Also add a menu item for no tags at all
            bool noTagsActive = string.IsNullOrEmpty(prop.stringValue);

            menu.AddItem(new GUIContent(NoTagText), noTagsActive, () =>
            {
                // Do nothing if the tag is already set to this value
                if (noTagsActive)
                {
                    return;
                }

                // Set name value and apply the changes
                prop.stringValue = "";
                serializedObject.ApplyModifiedProperties();
            });

            // Display the menu as a dropdown menu at the correct position
            Rect rect = EditorGUILayout.GetControlRect();
            rect.y += 18;
            menu.DropDown(rect);
        }
#endif

        public override void NodeCall(int port)
        {
            List<WormholeExitNode> exitNodes;

            // Tag is empty, which means that we activate ALL Wormhole Exit nodes in the entire graph
            if (string.IsNullOrEmpty(Tag))
            {
                exitNodes = Graph.GetNodesOfType<WormholeExitNode>();
            }
            // Otherwise just search for Wormhole Exit nodes with the corresponding tag
            else
            {
                exitNodes = Graph.GetNodesWithTag<WormholeExitNode>(Tag);
            }

            // No nodes :(
            if (exitNodes == null)
            {
                return;
            }

            foreach (WormholeExitNode node in exitNodes)
            {
                Graph.OnNodeCalled.Invoke(node, 0);
            }
        }
    }
}
