#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
#endif

namespace Gork
{
    /// <summary>
    /// Multiplies a number with another
    /// </summary>
    [GorkNodeInfo("Operations/Multiply Number", GorkColors.FLOAT_COLOR, 2)]
    [GorkInputPort("Value", typeof(float), false)]
    [GorkInputPort("Multiplier", typeof(float), false)]
    [GorkOutputPort("Result", typeof(float), false)]
    public class MultiplyNumberNode : GorkNode
    {
        public override float FloatCall(int port)
        {
            // Multiplication
            return GetValueFromPort<float>(0) * GetValueFromPort<float>(1);
        }
    }
}
