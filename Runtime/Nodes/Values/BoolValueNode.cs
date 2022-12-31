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
    [GorkNodeInfo("Values/Bool Value", GorkColors.BOOL_COLOR, 2, WikiSummary = "Gives out a constant bool value",
        WikiDescription = "Boolean values can either be true or false",
        WikiUsage = "Use this like a boolean field")]
    [NoInputPorts]
    [GorkOutputPort("Value", typeof(bool))]
    public class BoolValueNode : GorkNode
    {
        [GorkWikiInfo("The bool value of this node.\nIs displayed like a toggle in the editor")]
        public bool Value;

#if UNITY_EDITOR
        private Toggle _field;
        public override void Initialize(Node node)
        {
            _field = new Toggle();
            _field.style.alignSelf = Align.Center;

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

        public override bool BoolCall(int port)
        {
            return Value;
        }
    }
}
