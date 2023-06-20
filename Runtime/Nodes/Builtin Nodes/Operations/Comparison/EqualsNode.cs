using UnityEngine;

namespace Gork
{
    /// <summary>
    /// Compares both of it's input's and gives out a <see cref="bool"/> value if the values are equal to eachother.
    /// </summary>
    [GorkMenuItem("Operations/Comparisons/Equals (==)", GorkColors.BOOL_COLOR, Order = -6)]
    [GorkInputPort("X", typeof(object))]
    [GorkInputPort("Y", typeof(object))]
    [GorkOutputPort("Result", typeof(bool))]
    public class EqualsNode : GorkNode
    {
        [DontSaveInGorkGraph] [SerializeField] private string comparisonTypeName;

        public override bool BoolCall(int port)
        {
            object obj1 = GetValueFromPort(0);
            object obj2 = GetValueFromPort(1);

            return ModifyResult(obj1.Equals(obj2));
        }

        public virtual bool ModifyResult(bool result) => result;
    }
}
