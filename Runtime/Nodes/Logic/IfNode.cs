namespace Gork
{
    /// <summary>
    /// Will check if the input statement is true or false and will trigger another connected <see cref="GorkNode"/> based on the result. <para/>
    /// This node requires a signal first through the "Trigger" input port before the connected <see cref="GorkNode"/> is triggered.
    /// </summary>
    [GorkNodeInfo("Logic/If", GorkColors.TOOL_COlOR, 0, 
        WikiSummary = "Will check if the input statement is true of false and will trigger one of it's outputs based on the result",
        WikiDescription = "One of the staples of programming is the If statement.\nDoes this make GorkGraph a programming language?",
        WikiUsage = "Use this node like how you would use an If statement in regular programming terms.\n" +
        "Chain multiple of these together to create else if chains"
        )]
    [GorkInputPort("Trigger", WikiDescription = "Will make this node start it's comparison and then call one of it's output ports based on the value of the Statement input port")]
    [GorkInputPort("Statement", typeof(bool), WikiDescription = "The value that will be checked when this node is triggered")]
    [GorkOutputPort("True", WikiDescription = "Is triggered if the statement was true")]
    [GorkOutputPort("False", WikiDescription = "Is triggered if the statement was false")]
    public class IfNode : GorkNode
    {
        public override void NodeCall(int port)
        {
            if (GetValueFromPort<bool>(1))
            {
                // True
                CallPort(0);
            }
            else
            {
                // False
                CallPort(1);
            }
        }
    }
}
