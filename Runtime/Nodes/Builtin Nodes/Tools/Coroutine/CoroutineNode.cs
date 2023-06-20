using System.Collections;

namespace Gork
{
    /// <summary>
    /// 
    /// </summary>
    [GorkInputPort("Trigger")]
    [GorkOutputPort("When Done")]
    public abstract class CoroutineNode : GorkNode
    {
        public override IEnumerator ProcessIEnumerator(int port)
        {
            yield return CoroutineOperation();

            CallPort(port);
        }

        public abstract IEnumerator CoroutineOperation();
    }
}
