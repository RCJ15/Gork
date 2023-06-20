using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gork
{
    /// <summary>
    /// 
    /// </summary>
    [GorkInputPort("X", typeof(float))]
    [GorkInputPort("Y", typeof(float))]
    [GorkOutputPort("Result", typeof(bool))]
    public abstract class NumberComparisonNode : GorkNode
    {
        public override bool BoolCall(int port)
        {
            float value1 = GetValueFromPort<float>(0);
            float value2 = GetValueFromPort<float>(1);

            return Operation(value1, value2);
        }

        public abstract bool Operation(float value1, float value2);
    }
}
