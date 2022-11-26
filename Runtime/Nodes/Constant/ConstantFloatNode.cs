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
    /// Is a constant <see cref="float"/> value.
    /// </summary>
    [GorkNodeInfo("Constant/Constant Float", GorkColors.FLOAT_COLOR)]
    [NoInputPorts]
    [GorkOutputPort("Value", typeof(float), false)]
    public class ConstantFloatNode : GorkNode
    {
        public float Value;

#if UNITY_EDITOR
        public override void Initialize(Node node)
        {
            FloatField field = new FloatField();
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

        public override float FloatCall(int port)
        {
            return Value;
        }
    }
}
