#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
#endif

namespace Gork
{
    /// <summary>
    /// Gives a constant <see cref="string"/> value.
    /// </summary>
    [GorkNodeInfo("Values/String Value", GorkColors.STRING_COLOR, 3)]
    [NoInputPorts]
    [GorkOutputPort("Value", typeof(string), false)]
    public class StringValueNode : GorkNode
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

        public override string StringCall(int port)
        {
            return Value;
        }
    }
}
