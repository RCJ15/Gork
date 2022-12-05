using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
#endif

namespace Gork
{
    /// <summary>
    /// A node that will transport it's signal to a <see cref="WormholeExitNode"/> with the same key. Use this to fix spaghetti connections or make Functions that will be called multiple times.
    /// </summary>
    [GorkNodeInfo("Entries & Exits/Wormhole Entry", GorkColors.WORMHOLE_COLOR, 10)]
    [GorkInputPort("Signal")]
    [NoOutputPorts]
    public class WormholeEntryNode : GorkNode
    {
        public string Key = "Wormhole Key";

        [UnityEngine.HideInInspector] public WormholeExitNode[] Exits;

#if UNITY_EDITOR
        public override void Initialize(Node node)
        {
            TextField field = new TextField();
            field.value = Key;
            field.RegisterValueChangedCallback(data =>
            {
                Undo.RecordObject(this, $"Modified Property in {name}");
                Key = data.newValue;
            });

            node.outputContainer.Add(field);
        }
#endif

        public override void NodeCall(int port)
        {
            List<WormholeExitNode> exitNodes = Graph.GetAllNodesOfType<WormholeExitNode>();

            foreach (WormholeExitNode exitNode in exitNodes)
            {
                if (exitNode.Key == Key)
                {
                    Graph.OnNodeCalled.Invoke(exitNode, 0);
                }
            }
        }
    }
}
