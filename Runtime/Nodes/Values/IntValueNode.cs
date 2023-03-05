#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
#endif

namespace Gork
{
    /// <summary>
    /// Gives out a constant <see cref="int"/> value.
    /// </summary>
    [GorkNodeInfo("Values/Int Value", GorkColors.INT_COLOR, 1, WikiSummary = "Gives out a constant int value",
        WikiDescription = "Integer values maximum positive value is 2,147,483,647\n<i>(two billion, one hundred fourty seven million, four hundred eighty three thousand, six hundred fourty seven)</b>",
        WikiUsage = "Use this like a int field")]
    [NoInputPorts]
    [GorkOutputPort("Value", typeof(int))]
    public class IntValueNode : GorkNode
    {
        [GorkWikiInfo("The int value of this node")]
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

        public override int IntCall(int port)
        {
            return Value;
        }
    }
}
