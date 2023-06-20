using UnityEditor;

namespace Gork.Editor
{
    /// <summary>
    /// The base <see cref="GorkNodeEditor"/> script for <see cref="InternalEventNodeEditor"/> and <see cref="ExternalEventNodeEditor"/>.
    /// </summary>
    public abstract class EventNodeEditor : GorkNodeEditor
    {
        protected SerializedProperty eventNameProp;

        public abstract GorkGraph.Event.Type EventType { get; }

        public override void SetupEditor()
        {
            eventNameProp = FindProperty("EventName");
        }

        public override void OnRenameEvent(GorkGraph.Event.Type eventType, string oldName, string newName)
        {
            if (EventType == eventType && eventNameProp.stringValue == oldName)
            {
                eventNameProp.stringValue = newName;
                serializedObject.ApplyModifiedProperties();
            }
        }

        protected override void OnInspectorGUI()
        {
            GorkEditorUtility.EventDropdown(eventNameProp.stringValue, Graph.Events, newEvent =>
            {
                eventNameProp.stringValue = newEvent;
                serializedObject.ApplyModifiedProperties();
            }, EventType);
        }
    }
}