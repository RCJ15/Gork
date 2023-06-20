namespace Gork.Editor
{
    /// <summary>
    /// The node editor for the <see cref="DebugLogNode"/>.
    /// </summary>
    [CustomGorkNodeEditor(typeof(DebugLogNode))]
    public class DebugLogNodeEditor : GorkNodeEditor
    {
        public override float InspectorLabelWidth => 30;
        public override float InspectorFieldWidth => 50;

        protected override void OnInspectorGUI()
        {
            if (HasInputConnection(1))
            {
                return;
            }

            base.OnInspectorGUI();
        }
    }
}
