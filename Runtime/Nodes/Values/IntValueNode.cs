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
    [GorkOutputPort("Value", typeof(int))]
    public class IntValueNode : GorkNode
    {
        public int Value;

#if UNITY_EDITOR
        private IntegerField _field;

        public override void Initialize(Node node)
        {
            _field = new IntegerField();
            _field.value = Value;
            _field.RegisterValueChangedCallback(data =>
            {
                Undo.RecordObject(this, $"Modified Property in {name}");
                Value = data.newValue;
            });

            OnExpand();
        }

        public override void OnCollapse()
        {
            NodeView.inputContainer.Remove(_field);
        }

        public override void OnExpand()
        {
            NodeView.inputContainer.Add(_field);
        }
#endif

        public override int IntCall(int port)
        {
            return Value;
        }
    }
}
