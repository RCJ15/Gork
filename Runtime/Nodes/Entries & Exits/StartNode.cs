namespace Gork
{
    /// <summary>
    /// Will send a signal to it's output as soon as the GorkGraph is invoked
    /// </summary>
    [GorkNodeInfo("Entries & Exits/Start", GorkColors.ENTRY_COLOR)]
    [NoInputPorts]
    [GorkOutputPort("Start Port")]
    public class StartNode : GorkNode
    {

    }
}
