using System;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
#endif

namespace Gork
{
    /// <summary>
    /// Gives a constant <see cref="float"/> value.
    /// </summary>
    [GorkNodeInfo("Values/Float Value", GorkColors.FLOAT_COLOR, 0)]
    [NoInputPorts]
    [GorkOutputPort("Value", typeof(float))]
    public class FloatValueNode : GorkNode
    {
        public float Value;

#if UNITY_EDITOR
        private FloatField _field;

        public override void Initialize(Node node)
        {
            _field = new FloatField();
            _field.value = Value;
            _field.RegisterValueChangedCallback(data =>
            {
                Undo.RecordObject(this, $"Modified Property in {name}");
                Value = data.newValue;
            });

            if (Expanded)
            {
                OnExpand();
            }
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

        public override float FloatCall(int port)
        {
            return Value;
        }
    }
}
