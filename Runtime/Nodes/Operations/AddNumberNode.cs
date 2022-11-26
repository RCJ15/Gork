#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
#endif

namespace Gork
{
    /// <summary>
    /// Adds a number with another
    /// </summary>
    [GorkNodeInfo("Operations/Add Number", GorkColors.FLOAT_COLOR)]
    [GorkInputPort("Value", typeof(float), false)]
    [GorkInputPort("Adder", typeof(float), false)]
    [GorkOutputPort("Result", typeof(float), false)]
    public class AddNumberNode : GorkNode
    {
        public override float FloatCall(int port)
        {
            // Addition
            return GetValueFromPort<float>(0) + GetValueFromPort<float>(1);
        }
    }
}
