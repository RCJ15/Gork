#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
#endif

namespace Gork
{
    /// <summary>
    /// Is a constant <see cref="int"/> value.
    /// </summary>
    [GorkNodeInfo("Constant/Constant Int", "28233E")]
    [NoInputPorts]
    [GorkOutputPort("Value", typeof(int))]
    public class ConstantIntNode : GorkNode
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
    }
}
