#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
#endif

namespace Gork
{
    /// <summary>
    /// Gives out a constant <see cref="bool"/> value.
    /// </summary>
    [GorkNodeInfo("Values/Bool Value", GorkColors.BOOL_COLOR, 2)]
    [NoInputPorts]
    [GorkOutputPort("Value", typeof(bool), false)]
    public class BoolValueNode : GorkNode
    {
        public bool Value;

#if UNITY_EDITOR
        public override void Initialize(Node node)
        {
            Toggle field = new Toggle();
            field.value = Value;
            field.RegisterValueChangedCallback(data =>
            {
                Undo.RecordObject(this, $"Modified Property in {name}");
                Value = data.newValue;
            });

            node.inputContainer.Add(field);
            node.RefreshExpandedState();
        }
#endif

        public override bool BoolCall(int port)
        {
            return Value;
        }
    }
}
