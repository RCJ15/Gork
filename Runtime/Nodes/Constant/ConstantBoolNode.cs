#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
#endif

namespace Gork
{
    /// <summary>
    /// Is a constant <see cref="bool"/> value.
    /// </summary>
    [GorkNodeInfo("Constant/Constant Bool", GorkColors.BOOL_COLOR)]
    [NoInputPorts]
    [GorkOutputPort("Value", typeof(bool))]
    public class ConstantBoolNode : GorkNode
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
    }
}
