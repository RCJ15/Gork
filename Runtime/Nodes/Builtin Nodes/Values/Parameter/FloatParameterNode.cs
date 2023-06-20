using System;

namespace Gork
{
    /// <summary>
    /// 
    /// </summary>
    [GorkMenuItem("Values/Parameter/Float Parameter", GorkColors.FLOAT_COLOR, Order = 0)]
    [GorkOutputPort("Value", typeof(float))]
    public class FloatParameterNode : ParameterNode
    {
        public override Type ParameterType => typeof(float);
    }
}
