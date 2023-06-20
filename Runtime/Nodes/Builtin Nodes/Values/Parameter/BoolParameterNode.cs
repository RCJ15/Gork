using System;

namespace Gork
{
    /// <summary>
    /// 
    /// </summary>
    [GorkMenuItem("Values/Parameter/Bool Parameter", GorkColors.BOOL_COLOR, Order = 2)]
    [GorkOutputPort("Value", typeof(bool))]
    public class BoolParameterNode : ParameterNode
    {
        public override Type ParameterType => typeof(bool);
    }
}
