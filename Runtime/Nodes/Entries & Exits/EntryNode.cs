namespace Gork
{
    /// <summary>
    /// Will send a output as soon as this node is called
    /// </summary>
    [GorkNodeInfo("Entries & Exits/Entry", COLOR)]
    [NoInputPorts]
    [GorkOutputPort("Start Port")]
    public class EntryNode : GorkNode
    {
        public const string COLOR = "443629";
    }
}
