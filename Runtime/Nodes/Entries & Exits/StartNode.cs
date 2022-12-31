namespace Gork
{
    /// <summary>
    /// Sends a signal to it's output port as soon as the <see cref="GorkGraph"/> is started.
    /// </summary>
    [GorkNodeInfo("Entries & Exits/Start", GorkColors.ENTRY_COLOR, 0,
        WikiSummary = "Sends a signal to it's output port as soon as the GorkGraph is started",
        WikiDescription = "Functions like a Start() method in a MonoBehaviour",
        WikiUsage = "Use this node as a start to any connections you plan to make"
        )]
    [NoInputPorts]
    [GorkOutputPort("Start Port", WikiDescription = "Will trigger as soon as the GorkGraph is started")]
    public class StartNode : GorkNode
    {

    }
}
