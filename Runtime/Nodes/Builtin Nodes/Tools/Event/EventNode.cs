namespace Gork
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class EventNode : GorkNode
    {
        [GorkWikiInfo("Determines what event this node is connected to.\nIs displayed as a dropdown in the editor")]
        public string EventName = "Event Name";
    }
}
