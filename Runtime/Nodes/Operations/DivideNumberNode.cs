#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
#endif

namespace Gork
{
    /// <summary>
    /// Divides a number with another
    /// </summary>
    [GorkNodeInfo("Operations/Divide Number", GorkColors.FLOAT_COLOR)]
    [GorkInputPort("Numerator", typeof(float), false)]
    [GorkInputPort("Denumerator", typeof(float), false)]
    [GorkOutputPort("Result", typeof(float), false)]
    public class DivideNumberNode : GorkNode
    {
        public override float FloatCall(int port)
        {
            // Division
            return GetValueFromPort<float>(0) / GetValueFromPort<float>(1);
        }
    }
}
