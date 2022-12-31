namespace Gork
{
    /// <summary>
    /// Allows usage of the 3 most basic logic gates: AND, OR and XOR.
    /// </summary>
    [GorkNodeInfo("Logic/Logic Gate (AND)", GorkColors.BOOL_COLOR, 1, WikiName = "AND Logic Gate",
        WikiSummary = "Will output a bool if both of it's inputs are true",
        WikiDescription = "One of the basic Logic Gates. Simply outputs if both X and Y are true.",
        WikiUsage = "Use this node like how you would use AND logic gates in any other area")]
    [GorkNodeInfo("Logic/Logic Gate (OR)", GorkColors.BOOL_COLOR, 2, WikiName = "OR Logic Gate",
        WikiSummary = "Will output a bool if at least one of it's inputs are true",
        WikiDescription = "One of the basic Logic Gates. Simply outputs if either X or Y is true.",
        WikiUsage = "Use this node like how you would use OR logic gates in any other area")]
    [GorkNodeInfo("Logic/Logic Gate (XOR)", GorkColors.BOOL_COLOR, 3, WikiName = "XOR Logic Gate",
        WikiSummary = "Will output a bool based on if it's inputs are not the same values",
        WikiDescription = "A very underused Logic Gate. Will output if X and Y have different values.",
        WikiUsage = "Use this node like how you would use XOR logic gates in any other area")]
    [GorkInputPort("X", typeof(bool), WikiDescription = "Is compared with Y")]
    [GorkInputPort("Y", typeof(bool), WikiDescription = "Is compared with X")]
    [GorkOutputPort("Result", typeof(bool), WikiDescription = "What this Logic Gate has determined")]
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
