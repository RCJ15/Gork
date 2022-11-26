#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
#endif

namespace Gork
{
    /// <summary>
    /// A node that will receive a signal from a <see cref="WormholeEntryNode"/> with the same key. Use this to fix spaghetti connections.
    /// </summary>
    [GorkNodeInfo("Entries & Exits/Wormhole Exit", GorkColors.WORMHOLE_COLOR)]
    [NoInputPorts]
    [GorkOutputPort("Signal")]
    public class WormholeExitNode : GorkNode
    {
        public string Key = "Wormhole Key";

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

            node.inputContainer.Add(field);
        }
#endif
    }
}
