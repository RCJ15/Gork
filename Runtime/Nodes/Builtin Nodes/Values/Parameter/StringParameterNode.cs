using System;

namespace Gork
{
    /// <summary>
    /// 
    /// </summary>
    [GorkMenuItem("Values/Parameter/String Parameter", GorkColors.STRING_COLOR, Order = 3)]
    [GorkOutputPort("Value", typeof(string))]
    public class StringParameterNode : ParameterNode
    {
        public override Type ParameterType => typeof(string);
    }
}
