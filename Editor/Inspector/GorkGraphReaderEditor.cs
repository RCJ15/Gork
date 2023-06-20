using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using UnityEditorClass = UnityEditor.Editor;

namespace Gork.Editor
{
    /// <summary>
    /// 
    /// </summary>
    [CustomEditor(typeof(GorkGraphReader))]
    public class GorkGraphReaderEditor : UnityEditorClass
    {
        private ReorderableList _list;
        private SerializedProperty _graphProp;

        private void OnEnable()
        {
            _graphProp = serializedObject.FindProperty("graph");
            SerializedProperty eventsProp = serializedObject.FindProperty("events");

            _list = new ReorderableList(serializedObject, eventsProp, true, false, true, true);
            _list.drawElementCallback += DrawEventListElement;
            _list.elementHeightCallback += EventListElementHeight;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();

            EditorGUILayout.PropertyField(_graphProp);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("activationMode"));

            EditorGUILayout.Space();

            Rect foldoutRect = EditorGUILayout.GetControlRect();
            _list.serializedProperty.isExpanded = EditorGUI.BeginFoldoutHeaderGroup(foldoutRect, _list.serializedProperty.isExpanded, new GUIContent("External Events"));
            EditorGUI.EndFoldoutHeaderGroup();

            if (_list.serializedProperty.isExpanded)
            {
                EditorGUI.indentLevel++;
                _list.DoLayoutList();
                EditorGUI.indentLevel--;
            }

            serializedObject.ApplyModifiedProperties();
        }

        private static readonly Color _backgroundColor = new Color(0, 0, 0, 0.1f);
        private void DrawEventListElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty prop = _list.serializedProperty.GetArrayElementAtIndex(index);
            SerializedProperty nameProp = prop.FindPropertyRelative("Name");

            rect.y += 2;
            rect.x += 12;
            rect.width -= 12;

            Rect foldoutRect = rect;
            foldoutRect.height = 18;
            foldoutRect.width -= 2;

            prop.isExpanded = EditorGUI.BeginFoldoutHeaderGroup(foldoutRect, prop.isExpanded, new GUIContent(string.IsNullOrEmpty(nameProp.stringValue) ? $"Element {index}" : nameProp.stringValue));

            if (!prop.isExpanded)
            {
                EditorGUI.EndFoldoutHeaderGroup();
                return;
            }

            rect.y += 22;
            rect.height -= 22;

            // Draw background panel
            Rect backgroundPanelRect = rect;

            backgroundPanelRect.x -= 14;
            backgroundPanelRect.width += 14;

            backgroundPanelRect.y -= 3;
            backgroundPanelRect.height -= 3;

            EditorGUI.DrawRect(backgroundPanelRect, _backgroundColor);

            #region Name Property
            Rect namePropRect = rect;
            namePropRect.height = 18;

            Rect namePropLabelRect = namePropRect;
            namePropLabelRect.width = 50;
            EditorGUI.LabelField(namePropLabelRect, "Name");

            namePropRect.x += namePropLabelRect.width;
            namePropRect.width -= namePropLabelRect.width;

            // Do button
            if (EditorGUI.DropdownButton(namePropRect, new GUIContent(nameProp.stringValue), FocusType.Keyboard))
            {
                string propValue = nameProp.stringValue;

                // Create an empty generic menu which will be our dropdown menu
                GenericMenu menu = new GenericMenu();

                // TODO FIX event nodes
                /*
                // Loop through all events in the graph
                foreach (GorkGraph.Event evt in (_graphProp.objectReferenceValue as GorkGraph).Events)
                {
                    // Ignore all internal events
                    if (evt.EventType != GorkGraph.Event.Type.External)
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
                        nameProp.stringValue = name;
                        serializedObject.ApplyModifiedProperties();
                    });
                }
                */

                // Display the menu as a dropdown menu at the correct position
                Rect dropdownRect = namePropRect;
                menu.DropDown(dropdownRect);
            }
            #endregion

            Rect typeRect = rect;
            typeRect.y += namePropRect.height;
            typeRect.height = 18;

            Rect typeLabelRect = typeRect;
            typeLabelRect.width = 50;
            EditorGUI.LabelField(typeLabelRect, "Type");

            typeRect.x += typeLabelRect.width;
            typeRect.width -= typeLabelRect.width;

            EditorGUI.PropertyField(typeRect, prop.FindPropertyRelative("Type"), GUIContent.none);

            // Draw the Unity Event field
            Rect unityEventRect = rect;
            unityEventRect.y += typeRect.height + namePropRect.height + 10;
            unityEventRect.height -= namePropRect.height;

            EditorGUI.PropertyField(unityEventRect, GetTriggerProp(prop));

            EditorGUI.EndFoldoutHeaderGroup();
        }

        private float EventListElementHeight(int index)
        {
            if (index >= _list.serializedProperty.arraySize)
            {
                return 22;
            }

            SerializedProperty prop = _list.serializedProperty.GetArrayElementAtIndex(index);

            float height = 22;

            if (!prop.isExpanded)
            {
                return height;
            }

            height += 40;

            height += EditorGUI.GetPropertyHeight(GetTriggerProp(prop));
            height += 10;

            return height;
        }

        private SerializedProperty GetTriggerProp(SerializedProperty prop)
        {
            GorkGraphReader.Event.EventType type = (GorkGraphReader.Event.EventType)prop.FindPropertyRelative("Type").enumValueIndex;

            switch (type)
            {
                case GorkGraphReader.Event.EventType.Float:
                    return prop.FindPropertyRelative("FloatTrigger");

                case GorkGraphReader.Event.EventType.Int:
                    return prop.FindPropertyRelative("IntTrigger");

                case GorkGraphReader.Event.EventType.Bool:
                    return prop.FindPropertyRelative("BoolTrigger");
                
                case GorkGraphReader.Event.EventType.String:
                    return prop.FindPropertyRelative("StringTrigger");
                
                case GorkGraphReader.Event.EventType.Generic:
                    return prop.FindPropertyRelative("GenericTrigger");

                default:
                    return prop.FindPropertyRelative("NormalTrigger");
            }
        }
    }
}
