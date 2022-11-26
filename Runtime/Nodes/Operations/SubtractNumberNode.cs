#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
#endif

namespace Gork
{
    /// <summary>
    /// Subtracts a number with another
    /// </summary>
    [GorkNodeInfo("Operations/Subtract Number", GorkColors.FLOAT_COLOR)]
    [GorkInputPort("Value", typeof(float), false)]
    [GorkInputPort("Adder", typeof(float), false)]
    [GorkOutputPort("Result", typeof(float), false)]
    public class SubtractNumberNode : GorkNode
    {
        public override float FloatCall(int port)
        {
            // Subtraction
            return GetValueFromPort<float>(0) - GetValueFromPort<float>(1);
        }
    }
}
