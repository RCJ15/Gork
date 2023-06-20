using System;

namespace Gork
{
    /// <summary>
    /// 
    /// </summary>
    [GorkMenuItem("Values/Parameter/Int Parameter", GorkColors.INT_COLOR, Order = 1)]
    [GorkOutputPort("Value", typeof(int))]
    public class IntParameterNode : ParameterNode
    {
        public override Type ParameterType => typeof(int);
    }
}
