#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
#endif

namespace Gork
{
    /// <summary>
    /// Is a constant <see cref="string"/> value.
    /// </summary>
    [GorkNodeInfo("Constant/Constant String", "443A1E")]
    [NoInputPorts]
    [GorkOutputPort("Value", typeof(string))]
    public class ConstantStringNode : GorkNode
    {
        public string Value = "Text";

#if UNITY_EDITOR
        public override void Initialize(Node node)
        {
            TextField field = new TextField();
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
