namespace Gork.Editor
{
    /// <summary>
    /// A callback that will notify if a <see cref="UnityEditor.Experimental.GraphView.GraphElement"/> is removed in the <see cref="GorkGraphView"/>.
    /// </summary>
    public interface IDeletionCallback
    {
        public void OnDeletion();
    }
}
