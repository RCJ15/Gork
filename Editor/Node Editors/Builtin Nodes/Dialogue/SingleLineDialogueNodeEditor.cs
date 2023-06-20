namespace Gork.Editor
{
    /// <summary>
    /// The node editor for the <see cref="SingleLineDialogueNode"/>.
    /// </summary>
    [CustomGorkNodeEditor(typeof(SingleLineDialogueNode))]
    public class SingleLineDialogueNodeEditor : GorkNodeEditor
    {
        public override float InspectorFieldWidth => 70;

        protected override void OnInspectorGUI()
        {
            SetupInspector();

            if (HasInputConnection(1))
            {
                return;
            }

            DoDefaultInspector();
        }
    }
}
