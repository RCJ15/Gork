namespace Gork
{
    /// <summary>
    /// Will stop all threads in a <see cref="GorkGraph"/>.
    /// </summary>
    [GorkNodeInfo("Entries & Exits/Stop", GorkColors.ENTRY_COLOR)]
    [NoOutputPorts]
    [GorkInputPort("Trigger")]
    public class StopNode : GorkNode
    {
        public override void NodeCall(int port)
        {
            Graph.OnGraphStop.Invoke();
        }
    }
}
