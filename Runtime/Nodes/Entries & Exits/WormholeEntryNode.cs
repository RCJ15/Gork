#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
#endif

namespace Gork
{
    /// <summary>
    /// A node that will transport it's signal to a <see cref="WormholeExitNode"/> with the same key. Use this to fix spaghetti connections.
    /// </summary>
    [GorkNodeInfo("Entries & Exits/Wormhole Entry", EntryNode.COLOR)]
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
    }
}
