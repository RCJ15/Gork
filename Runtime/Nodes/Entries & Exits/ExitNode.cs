namespace Gork
{
    /// <summary>
    /// Will exit and stop all threads in a <see cref="GorkGraph"/>.
    /// </summary>
    [GorkNodeInfo("Entries & Exits/Exit", GorkColors.ENTRY_COLOR)]
    [NoOutputPorts]
    [GorkInputPort("Trigger")]
    [GorkInputPort("Disabled", typeof(bool))]
    public class ExitNode : GorkNode
    {

    }
}
