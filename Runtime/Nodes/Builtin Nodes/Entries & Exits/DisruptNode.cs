using System.Collections.Generic;

namespace Gork
{
    /// <summary>
    /// Will stop all <see cref="GorkNode"/> that have a certain tag
    /// </summary>
    [GorkMenuItem("Entries & Exits/Disrupt Node", GorkColors.ENTRY_COLOR, Order = 2)]
    [GorkInputPort("Trigger")]
    public class DisruptNode : GorkNode
    {
        [GorkWikiInfo("The tag that will determine which nodes to stop.\nIs displayed like a dropdown in the editor")]
        public string Tag = "No Tag";

        public override void Process(int port)
        {
            // Stop all nodes with the set tag
            List<GorkNode> nodes = Graph.GetNodesWithTag(Tag);

            // No nodes with that tag :(
            if (nodes == null)
            {
                return;
            }

            foreach (GorkNode node in nodes)
            {
                Graph.OnNodeStop(node);
            }
        }
    }
}
