#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
#endif

namespace Gork
{
    /// <summary>
    /// Gives a constant <see cref="int"/> value.
    /// </summary>
    [GorkNodeInfo("Values/Int Value", GorkColors.INT_COLOR, 1)]
    [NoInputPorts]
    [GorkOutputPort("Value", typeof(int), false)]
    public class IntValueNode : GorkNode
    {
        public int Value;

#if UNITY_EDITOR
        public override void Initialize(Node node)
        {
            IntegerField field = new IntegerField();
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

        public override int IntCall(int port)
        {
            return Value;
        }
    }
}
