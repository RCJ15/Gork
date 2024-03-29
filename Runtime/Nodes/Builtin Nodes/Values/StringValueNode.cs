using UnityEngine;

namespace Gork
{
    /// <summary>
    /// Gives out a constant <see cref="string"/> value.
    /// </summary>
    [GorkMenuItem("Values/String Value", GorkColors.STRING_COLOR, Order = 3)]
    [GorkOutputPort("Value", typeof(string))]
    public class StringValueNode : ValueNode<string>
    {

    }
}
