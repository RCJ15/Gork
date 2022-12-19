using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gork
{
    /// <summary>
    /// Allows usage of the 3 most basic logic gates: AND, OR and XOR.
    /// </summary>
    [GorkNodeInfo("Logic/Logic Gate (AND)", GorkColors.BOOL_COLOR, 1)]
    [GorkNodeInfo("Logic/Logic Gate (OR)", GorkColors.BOOL_COLOR, 2)]
    [GorkNodeInfo("Logic/Logic Gate (XOR)", GorkColors.BOOL_COLOR, 3)]
    [GorkInputPort("X", typeof(bool))]
    [GorkInputPort("Y", typeof(bool))]
    [GorkOutputPort("Result", typeof(bool))]
    public class LogicGateNode : GorkNode
    {
#if UNITY_EDITOR
        public override void OnViewEnable()
        {
            switch (AttributeID)
            {
                default:
                    Title = "AND";
                    break;

                case 1:
                    Title = "OR";
                    break;

                case 2:
                    Title = "XOR";
                    break;
            }

            UpdateNodeView();
        }
#endif

        public override bool BoolCall(int port)
        {
            bool value1 = GetValueFromPort<bool>(0);
            bool value2 = GetValueFromPort<bool>(1);

            switch (AttributeID)
            {
                // AND
                default:
                    return value1 && value2;

                // OR
                case 1:
                    return value1 || value2;

                // XOR
                case 2:
                    return value1 ^ value2;
            }
        }
    }
}
