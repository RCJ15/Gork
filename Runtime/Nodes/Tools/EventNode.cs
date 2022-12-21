using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
#endif

namespace Gork
{
    /// <summary>
    /// 
    /// </summary>
    [GorkNodeInfo("Tools/Events/Internal Event", GorkColors.INTERNAL_EVENT_COLOR, 1)]

    [GorkNodeInfo("Tools/Events/External Event", GorkColors.EXTERNAL_EVENT_COLOR, 2)]
    [GorkNodeInfo("Tools/Events/Float External Event", GorkColors.EXTERNAL_EVENT_COLOR, 3)]
    [GorkNodeInfo("Tools/Events/Int External Event", GorkColors.EXTERNAL_EVENT_COLOR, 4)]
    [GorkNodeInfo("Tools/Events/Bool External Event", GorkColors.EXTERNAL_EVENT_COLOR, 5)]
    [GorkNodeInfo("Tools/Events/String External Event", GorkColors.EXTERNAL_EVENT_COLOR, 6)]
    [GorkNodeInfo("Tools/Events/Generic External Event", GorkColors.EXTERNAL_EVENT_COLOR, 7)]
    [NoInputPorts]
    [NoOutputPorts]
    public class EventNode : GorkNode
    {
        public string EventName = "Event Name";

        public bool IsInternal => AttributeID == 0;

#if UNITY_EDITOR
        protected override void BuildInputTypesList(List<Type> list)
        {
            if (IsInternal)
            {
                return;
            }

            list.Add(GorkUtility.SignalType);

            Type type = null;

            switch (AttributeID)
            {
                case 2:
                    type = typeof(float);
                    break;

                case 3:
                    type = typeof(int);
                    break;

                case 4:
                    type = typeof(bool);
                    break;

                case 5:
                    type = typeof(string);
                    break;

                case 6:
                    type = typeof(object);
                    break;
            }

            if (type != null)
            {
                list.Add(type);
            }
        }

        protected override void BuildOutputTypesList(List<Type> list)
        {
            if (!IsInternal)
            {
                return;
            }

            list.Add(GorkUtility.SignalType);
        }

        public override void OnViewEnable()
        {
            // External
            if (!IsInternal)
            {
                SetInputPort(0, "Trigger");

                if (AttributeID == 1)
                {
                    return;
                }

                string name = "Input";

                Type inputType;

                switch (AttributeID)
                {
                    case 2:
                        inputType = typeof(float);
                        name = "Float " + name;
                        break;

                    case 3:
                        inputType = typeof(int);
                        name = "Int " + name;
                        break;

                    case 4:
                        inputType = typeof(bool);
                        name = "Bool " + name;
                        break;

                    case 5:
                        inputType = typeof(string);
                        name = "String " + name;
                        break;

                    default:
                        inputType = typeof(object);
                        name = "Object " + name;
                        break;
                }

                SetInputPort(1, name, inputType);
            }
            // Internal
            else
            {
                SetOutputPort(0, "Trigger");
            }
        }

        public override void Initialize(Node node)
        {
            if (Expanded)
            {
                OnExpand();
            }
        }

        public override void OnCollapse()
        {
            // External
            if (!IsInternal)
            {
                NodeView.outputContainer.Remove(IMGUIContainer);
            }
            // Internal
            else
            {
                NodeView.inputContainer.Remove(IMGUIContainer);
            }
        }

        public override void OnExpand()
        {
            // External
            if (!IsInternal)
            {
                NodeView.outputContainer.Add(IMGUIContainer);
            }
            // Internal
            else
            {
                NodeView.inputContainer.Add(IMGUIContainer);
            }
        }

        protected override void OnInspectorGUI()
        {
            // Do button
            if (!EditorGUILayout.DropdownButton(new GUIContent(EventName), FocusType.Keyboard))
            {
                // This is when the button is not pressed
                return;
            }

            // Button has been pressed

            // Get the name property
            SerializedProperty prop = serializedObject.FindProperty(nameof(EventName));
            string propValue = prop.stringValue;

            // Create an empty generic menu which will be our dropdown menu
            GenericMenu menu = new GenericMenu();

            GorkGraph.Event.Type evtType = IsInternal ? GorkGraph.Event.Type.Internal : GorkGraph.Event.Type.External;

            // Loop through all events
            foreach (GorkGraph.Event evt in Graph.Events)
            {
                // Ignore events without the same type
                if (evt.EventType != evtType)
                {
                    continue;
                }

                string name = evt.Name;
                bool on = propValue == name;

                // Add the menu item
                menu.AddItem(new GUIContent(name), on, () =>
                {
                    // Do nothing if the event is already set to this value
                    if (on)
                    {
                        return;
                    }

                    // Set name value and apply the changes
                    prop.stringValue = name;
                    serializedObject.ApplyModifiedProperties();
                });
            }

            // Display the menu as a dropdown menu at the correct position
            Rect rect = EditorGUILayout.GetControlRect();
            rect.y += 18;
            menu.DropDown(rect);
        }

#endif

        public override void NodeCall(int port)
        {
            // External
            if (!IsInternal)
            {
                // Trigger external event
                object parameter = null;

                switch (AttributeID)
                {
                    case 2:
                        parameter = GetValueFromPort<float>(1);
                        break;

                    case 3:
                        parameter = GetValueFromPort<int>(1);
                        break;

                    case 4:
                        parameter = GetValueFromPort<bool>(1);
                        break;

                    case 5:
                        parameter = GetValueFromPort<string>(1);
                        break;

                    case 6:
                        parameter = GetValueFromPort<object>(1);
                        break;
                }

                Graph.OnCallExternal.Invoke(EventName, parameter);
            }
            // Internal
            else
            {
                base.NodeCall(port);
            }
        }
    }
}
