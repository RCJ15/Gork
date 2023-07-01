namespace Gork.Editor
{
    /// <summary>
    /// An interface that contains a callback which will be called if the attached <see cref="UnityEditor.Experimental.GraphView.GraphElement"/> is removed in the <see cref="GorkGraphView"/>.
    /// </summary>
    public interface IDeletionCallback
    {
        void OnDeletion();
    }
}
