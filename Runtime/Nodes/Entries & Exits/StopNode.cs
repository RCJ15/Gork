namespace Gork
{
    /// <summary>
    /// Will stop all currently active nodes in the <see cref="GorkGraph"/> when triggered.
    /// </summary>
    [GorkNodeInfo("Entries & Exits/Stop", GorkColors.ENTRY_COLOR, 1,
        WikiSummary = "Will stop all currently active nodes in the GorkGraph when triggered",
        WikiDescription = "An easy way to instantly stop everything in the entire Graph all at once.",
        WikiUsage = "Use this node as how you would use a \"return\" in code"
        )]
    [NoOutputPorts]
    [GorkInputPort("Trigger", WikiDescription = "Will stop every node in the GorkGraph when triggered")]
    public class StopNode : GorkNode
    {
        public override void NodeCall(int port)
        {
            Graph.OnGraphStop.Invoke();
        }
    }
}
