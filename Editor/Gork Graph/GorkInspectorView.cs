using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Gork.Editor
{
    /// <summary>
    /// 
    /// </summary>
    public class GorkInspectorView : VisualElement
    {
        private const string DROPDOWN_ARROW_BASE64 = "iVBORw0KGgoAAAANSUhEUgAAAAkAAAAJCAYAAADgkQYQAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAABMSURBVChTY/z//z8DIcB45MgRgqqYgLgOwsQJ6hhBJNC0WiDVBGKjgTobG5tmsCIQwKIQrADKRgCQQpAboRpwA2wKiAoCkO8IAAYGADYvH/HWQhuAAAAAAElFTkSuQmCC";
        private static Texture2D _dropdownTexture = null;

        private static GenericMenu _createParameterMenu, _createEventMenu = null;

        private ModeEnum _mode;

        private string _searchFilterString = "";
        private GorkGraph.DataType? _parametersSearchFilter = null;
        private ToolbarPopupSearchField _searchField;

        private IMGUIContainer _imguiContainer;

        #region Parameters List
        private ReorderableList _cachedParametersList;
        private ReorderableList ParametersList
        {
            get
            {
                // Create and cache list if null
                if (_cachedParametersList == null)
                {
                    SerializedProperty prop = GraphSerializedObject.FindProperty("_parameters");

                    _cachedParametersList = new ReorderableList(GraphSerializedObject, prop, true, false, false, true);

                    _cachedParametersList.drawElementCallback = ParameterListDrawElement;
                    _cachedParametersList.drawHeaderCallback = null;
                    _cachedParametersList.onRemoveCallback = list => RemoveParameter(list.index);

                    _cachedParametersList.showDefaultBackground = false;

                    _cachedParametersList.elementHeight = LIST_ELEMENT_SIZE;
                }

                return _cachedParametersList;
            }
        }
        private int? _focusParameterElement = null;

        private List<int> _parametersSearchFilterList = new List<int>();
        private ReorderableList _cachedParametersSearchList;
        private ReorderableList ParametersSearchList
        {
            get
            {
                if (_cachedParametersSearchList == null)
                {
                    _cachedParametersSearchList = new ReorderableList(_parametersSearchFilterList, typeof(int), false, false, false, true);

                    _cachedParametersSearchList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => ParameterListDrawElement(rect, _parametersSearchFilterList[index], isActive, isFocused);
                    _cachedParametersSearchList.drawHeaderCallback = null;
                    _cachedParametersSearchList.onRemoveCallback = list =>
                    {
                        RemoveParameter(list.index);

                        UpdateParametersSearchList(false);
                    };

                    _cachedParametersSearchList.showDefaultBackground = false;

                    _cachedParametersSearchList.elementHeight = LIST_ELEMENT_SIZE;
                }

                return _cachedParametersSearchList;
            }
        }
        #endregion

        private static readonly Color _listBackgroundColor = new Color(1, 1, 1, 0.015686f);
        private static readonly Color _listLineColor = new Color(0, 0, 0, 0.372549f);

        #region Data Type Colors
        private static readonly Color _floatColor = new Color(0.5098f, 0.8745f, 0.886274f);
        private static readonly Color _intColor = new Color(0.56862f, 0.49803f, 0.88235f);
        private static readonly Color _boolColor = new Color(0.53725f, 0.98039f, 0.545098f);
        private static readonly Color _stringColor = new Color(0.968627f, 0.82745f, 0.423529f);
        #endregion

        private const float LIST_LINE_SIZE = 1;
        private const float LIST_SIZE_OFFSET = 50;
        private const float LIST_ELEMENT_SIZE = 27;
        
        private Vector2 _scrollPosition;

        public GorkGraphView GraphView { get; internal set; }
        private GorkGraph Graph => GraphView?.Graph;
        private SerializedObject GraphSerializedObject => Graph == null ? null : Graph.SerializedObject;

        private List<Rect> _rects = new List<Rect>();

        public new class UxmlFactory : UxmlFactory<GorkInspectorView, VisualElement.UxmlTraits> { }

        public GorkInspectorView()
        {
            RegisterCallback<MouseUpEvent>(HandleRightClick);
        }

        private void HandleRightClick(MouseUpEvent evt)
        {
            // Return if this is not a right click or if the GraphSerializedObject is null
            if (evt.button != (int)MouseButton.RightMouse || GraphSerializedObject == null)
            {
                return;
            }

            // Loop through every single element in the list
            SerializedProperty prop = ParametersList.serializedProperty;
            int length = prop.arraySize;

            for (int i = 0; i < length; i++)
            {
                Rect rect = _rects[i];

                // Check if the mouse is inside this element
                if (rect.Contains(evt.localMousePosition))
                {
                    // If so, then bring up a context menu
                    GenericMenu menu = new GenericMenu();

                    menu.AddItem(new GUIContent("Duplicate"), false, () => DuplicateParameter(i));
                    menu.AddItem(new GUIContent("Delete"), false, () => RemoveParameter(i));

                    menu.ShowAsContext();
                    break;
                }
            }

            // Nothing was right clicked :(
        }

        private void DuplicateParameter(int index)
        {
            _rects.Add(new Rect());

            // Get properties
            SerializedProperty prop = ParametersList.serializedProperty;
            SerializedProperty copiedProp = prop.GetArrayElementAtIndex(index);

            // Create the new paramter
            prop.InsertArrayElementAtIndex(index);
            SerializedProperty newProp = prop.GetArrayElementAtIndex(index + 1);

            // Copy values from the original property
            string startName = copiedProp.FindPropertyRelative("Name").stringValue;
            newProp.FindPropertyRelative("Name").stringValue = GetUniqueParameterName(startName, index + 1);
            newProp.FindPropertyRelative("Type").enumValueIndex = copiedProp.FindPropertyRelative("Type").enumValueIndex;
            newProp.FindPropertyRelative("Value").stringValue = copiedProp.FindPropertyRelative("Value").stringValue;

            GraphSerializedObject.ApplyModifiedProperties();

            // Select the new parameter
            ParametersList.Select(index + 1);
        }

        private void RemoveParameter(int index)
        {
            // Remove a parameter from the parameter list
            _rects.RemoveAt(index);

            // Delete the array element
            ParametersList.serializedProperty.DeleteArrayElementAtIndex(index);
            GraphSerializedObject.ApplyModifiedProperties();

            // Select the new last element if the index is out of range
            if (ParametersList.count <= index)
            {
                // Ensure that the count is above 0
                ParametersList.Select(Mathf.Max(index - 1, 0));
            }
        }

        public void Initialize(VisualElement root)
        {
            // Setup IMGUI Container
            _imguiContainer = new IMGUIContainer(OnGui);

            Add(_imguiContainer);

            // Parameter & event buttons
            ToolbarButton parameterButton = root.Q<ToolbarButton>("Parameters");
            ToolbarButton eventsButton = root.Q<ToolbarButton>("Events");

            VisualElement parameterHighlight = parameterButton.Q<VisualElement>("Highlight");
            VisualElement eventsHighlight = eventsButton.Q<VisualElement>("Highlight");

            void ParameterButtonColorChange()
            {
                parameterHighlight.visible = true;
                eventsHighlight.visible = false;
            }

            void EventsButtonColorChange()
            {
                eventsHighlight.visible = true;
                parameterHighlight.visible = false;
            }

            ParameterButtonColorChange();

            parameterButton.clicked += () =>
            {
                if (_mode == ModeEnum.parameters)
                {
                    return;
                }

                ParameterButtonColorChange();

                _mode = ModeEnum.parameters;

                // Reset search filter
                ResetSearchField();
            };

            eventsButton.clicked += () =>
            {
                if (_mode == ModeEnum.events)
                {
                    return;
                }

                EventsButtonColorChange();

                _mode = ModeEnum.events;

                // Reset search filter
                ResetSearchField();
            };

            ToolbarButton addButton = root.Q<ToolbarButton>("AddButton");

            #region Dropdown Arrow
            // Cache and load the texture if it's not been loaded yet
            if (_dropdownTexture == null)
            {
                // Load texture from base 64
                _dropdownTexture = GorkEditorUtility.Texture2DFromBase64(DROPDOWN_ARROW_BASE64);
            }

            addButton.Q<VisualElement>("Image").SetTexture(_dropdownTexture);
            #endregion

            addButton.clicked += () =>
            {
                #region Create Parameters Menu
                // Create and cache the create parameter menu if it's null
                if (_createParameterMenu == null)
                {
                    _createParameterMenu = new GenericMenu();

                    // Add an entry for creating one of each data type
                    _createParameterMenu.AddItem(new GUIContent("Float"), false, () =>
                    {
                        AddParameterToGraph("New Float", GorkGraph.DataType.Float, 0f);
                    });
                    _createParameterMenu.AddItem(new GUIContent("Int"), false, () =>
                    {
                        AddParameterToGraph("New Int", GorkGraph.DataType.Int, 0);
                    });
                    _createParameterMenu.AddItem(new GUIContent("Bool"), false, () =>
                    {
                        AddParameterToGraph("New Bool", GorkGraph.DataType.Bool, true);
                    });
                    _createParameterMenu.AddItem(new GUIContent("String"), false, () =>
                    {
                        AddParameterToGraph("New String", GorkGraph.DataType.String, null);
                    });
                }
                #endregion

                #region Create Events Menu
                // Create and cache the create event menu if it's null
                if (_createEventMenu == null)
                {
                    _createEventMenu = new GenericMenu();

                    // Add an entry for creating every type of event
                    _createEventMenu.AddItem(new GUIContent("Event"), false, () =>
                    {
                        Debug.Log("Create Event");
                    });
                }
                #endregion

                // Determine where to spawn the dropdown
                Rect addButtonRect = addButton.contentRect;

                addButtonRect.y += addButtonRect.height * 4;
                addButtonRect.position -= Vector2.one * 3;

                // Spawn the correct dropdown based on which mode is currently active
                switch (_mode)
                {
                    case ModeEnum.parameters:
                        _createParameterMenu.DropDown(addButtonRect);
                        break;
                    case ModeEnum.events:
                        _createEventMenu.DropDown(addButtonRect);
                        break;
                }
            };

            // Search field
            _searchField = root.Q<ToolbarPopupSearchField>("SearchTextField");
            Button searchFieldButton = _searchField.Q<Button>("unity-search");
            Label placeholderText = _searchField.Q<Label>("PlaceholderText");

            _searchField.RegisterValueChangedCallback(text =>
            {
                // Hide the placeholder "Search..." text when there is a value given in the text field
                placeholderText.visible = string.IsNullOrEmpty(text.newValue);

                // Filter!
                _searchFilterString = text.newValue;

                UpdateParametersSearchList();
            });

            searchFieldButton.clicked += () =>
            {
                // Determine where to spawn the dropdown
                Rect searchFieldButtonRect = addButton.contentRect;

                searchFieldButtonRect.y += searchFieldButtonRect.height * 4;
                searchFieldButtonRect.position -= Vector2.one * 3;

                // Spawn the correct dropdown based on which mode is currently active
                // These menus can't be cached since the entries in the menus will display if they are selected or not which can't be edited once the menu is created (stupid Unity thing)
                switch (_mode)
                {
                    case ModeEnum.parameters:
                        #region Create Filter Parameters Menu
                        // Create the filter parameters menu
                        GenericMenu filterParametersMenu = new GenericMenu();

                        // Add an entry for setting the filter for each data type
                        filterParametersMenu.AddItem(new GUIContent("Name"), !_parametersSearchFilter.HasValue, () =>
                        {
                            _parametersSearchFilter = null;
                            UpdateParametersSearchList();
                        });
                        filterParametersMenu.AddItem(new GUIContent("Float"), _parametersSearchFilter.HasValue && _parametersSearchFilter.Value == GorkGraph.DataType.Float, () =>
                        {
                            _parametersSearchFilter = GorkGraph.DataType.Float;
                            UpdateParametersSearchList();
                        });
                        filterParametersMenu.AddItem(new GUIContent("Int"), _parametersSearchFilter.HasValue && _parametersSearchFilter.Value == GorkGraph.DataType.Int, () =>
                        {
                            _parametersSearchFilter = GorkGraph.DataType.Int;
                            UpdateParametersSearchList();
                        });
                        filterParametersMenu.AddItem(new GUIContent("Bool"), _parametersSearchFilter.HasValue && _parametersSearchFilter.Value == GorkGraph.DataType.Bool, () =>
                        {
                            _parametersSearchFilter = GorkGraph.DataType.Bool;
                            UpdateParametersSearchList();
                        });
                        filterParametersMenu.AddItem(new GUIContent("String"), _parametersSearchFilter.HasValue && _parametersSearchFilter.Value == GorkGraph.DataType.String, () =>
                        {
                            _parametersSearchFilter = GorkGraph.DataType.String;
                            UpdateParametersSearchList();
                        });
                        #endregion

                        filterParametersMenu.DropDown(searchFieldButtonRect);
                        break;

                    case ModeEnum.events:

                        #region Filter Events Menu
                        // Create the filter events menu
                        GenericMenu filterEventsMenu = new GenericMenu();

                        // Add an entry for setting the filter for each event type
                        filterEventsMenu.AddItem(new GUIContent("Name"), !_parametersSearchFilter.HasValue, () =>
                        {
                            _parametersSearchFilter = null;
                            UpdateParametersSearchList();
                        });
                        #endregion

                        filterEventsMenu.DropDown(searchFieldButtonRect);
                        break;
                }
            };
        }

        public void OnUndoRedo()
        {
            ResetSearchField();
        }

        private void ResetSearchField()
        {
            _parametersSearchFilter = null;
            _searchField.value = "";

            _imguiContainer.MarkDirtyRepaint();

            _scrollPosition = Vector2.zero;
        }

        private void UpdateParametersSearchList(bool repaint = true)
        {
            _parametersSearchFilterList.Clear();

            SerializedProperty prop = ParametersList.serializedProperty;

            int length = prop.arraySize;

            // Filter
            for (int i = 0; i < length; i++)
            {
                SerializedProperty arrayElement = prop.GetArrayElementAtIndex(i);

                // Check if the name match the search filter string
                if (!string.IsNullOrEmpty(_searchFilterString))
                {
                    SerializedProperty nameProp = arrayElement.FindPropertyRelative("Name");

                    if (!nameProp.stringValue.ToLower().Contains(_searchFilterString.ToLower()))
                    {
                        continue;
                    }
                }

                // Add this index regularly if the search filter type has no value
                if (!_parametersSearchFilter.HasValue)
                {
                    _parametersSearchFilterList.Add(i);
                }
                // Otherwise we will check and only add the matching DataTypes
                else
                {
                    SerializedProperty typeProp = arrayElement.FindPropertyRelative("Type");

                    if (_parametersSearchFilter.Value == (GorkGraph.DataType)typeProp.enumValueIndex)
                    {
                        _parametersSearchFilterList.Add(i);
                    }
                }
            }

            if (repaint)
            {
                _imguiContainer.MarkDirtyRepaint();
            }

            _scrollPosition = Vector2.zero;
        }

        private void OnGui()
        {
            switch (_mode)
            {
                case ModeEnum.parameters:
                    ParameterEditorOnGui();
                    break;

                case ModeEnum.events:
                    EventEditorOnGui();
                    break;
            }
        }

        private void AddParameterToGraph(string name, GorkGraph.DataType type, object defaultValue)
        {
            name = GetUniqueParameterName(name);

            SerializedProperty prop = ParametersList.serializedProperty;

            int index = prop.arraySize;
            prop.InsertArrayElementAtIndex(index);

            _focusParameterElement = index;

            SerializedProperty newParameter = prop.GetArrayElementAtIndex(index);

            newParameter.FindPropertyRelative("Name").stringValue = name;
            newParameter.FindPropertyRelative("Type").enumValueIndex = (int)type;
            newParameter.FindPropertyRelative("Value").stringValue = GorkUtility.ToJson(defaultValue, type.ActualType());

            // Reset search filter
            _parametersSearchFilter = null;
            UpdateParametersSearchList(false);

            GraphSerializedObject.ApplyModifiedProperties();

            _scrollPosition = new Vector2(0, (float)ParametersList.elementHeight * (float)index);
        }

        #region Parameter GUI
        private string GetUniqueParameterName(string startName, int? index = null)
        {
            string name = startName;
            SerializedProperty prop = ParametersList.serializedProperty;

            List<string> arrayNames = new List<string>();

            // Loop through entire parameter list and add the names to the arrayNames list
            int length = prop.arraySize;

            for (int i = 0; i < length; i++)
            {
                if (index.HasValue && index.Value == i)
                {
                    continue;
                }

                SerializedProperty arrayElement = prop.GetArrayElementAtIndex(i);

                SerializedProperty nameProp = arrayElement.FindPropertyRelative("Name");

                arrayNames.Add(nameProp.stringValue.ToLower());
            }

            // Do while loop to determine if the name is unique or not
            int loopAmount = 0;

            // Remove numbers and whitespace at the end of the name
            string trimmedName = startName.TrimEnd().TrimEnd('0', '1', '2', '3', '4', '5', '6', '7', '8', '9').TrimEnd();
            do
            {
                // Name is NOT in array name list?
                if (!arrayNames.Contains(name.ToLower()))
                {
                    break;
                }

                name = $"{trimmedName} {loopAmount}";

                loopAmount++;
            }
            while (loopAmount <= 1000); // Limit to only loop a maximum of 1000 times

            return name;
        }

        private void ParameterEditorOnGui()
        {
            if (GraphSerializedObject == null)
            {
                return;
            }

            GraphSerializedObject.UpdateIfRequiredOrScript();

            Rect contentRect = _imguiContainer.parent.contentRect;

            bool displaySearchBar = _parametersSearchFilter.HasValue || !string.IsNullOrEmpty(_searchFilterString);

            // Position list so the remove button isn't shown as it looks pretty ugly
            Rect rect = EditorGUILayout.GetControlRect();

            rect.width += 12;
            rect.x -= 4;

            rect.width += LIST_SIZE_OFFSET;

            // Determine the scroll rect
            Rect scrollRect = rect;
            scrollRect.x = contentRect.x;
            scrollRect.width = 0; // Remove width to make sure the horizontal bar won't appear

            // This is the float which is the size of the entire list, this also accounts for the search list
            float singleElementSize = (float)ParametersList.elementHeight + 2f;
            float listHeight = ((displaySearchBar ? (float)ParametersSearchList.count : (float)ParametersList.count) - 1) * singleElementSize;
            
            // Set the height of the scroll rect after we have determined the entire lists height
            // We also add 36 as that is the size of the bottom bar in the inspector
            scrollRect.height = listHeight + 36;

            bool displayScrollBar = contentRect.height < scrollRect.height;

            contentRect.height = Mathf.Max(contentRect.height, 50);

            _scrollPosition = GUI.BeginScrollView(contentRect, _scrollPosition, scrollRect);

            // Add spacing as otherwise clicking the list will not function properly
            // We also add another element size
            EditorGUILayout.Space(listHeight + singleElementSize);

            if (displayScrollBar)
            {
                rect.width -= 12;
            }

            // Display a search filtered parameter list
            if (displaySearchBar)
            {
                ParametersSearchList.DoList(rect);
            }
            // Display the regular parameters list
            else
            {
                ParametersList.DoList(rect);
            }

            GUI.EndScrollView();

            // Apply the changes and record undo
            GraphSerializedObject.ApplyModifiedProperties();
        }

        private void ParameterListDrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            // Cache this array property
            SerializedProperty prop = ParametersList.serializedProperty.GetArrayElementAtIndex(index);

            // Remove offscreen offset + 5 for a bit of extra space
            rect.width -= LIST_SIZE_OFFSET + 5;

            // Cache the DataType of the array property
            GorkGraph.DataType type = (GorkGraph.DataType)prop.FindPropertyRelative("Type").enumValueIndex;

            #region DataType Color
            // Determine DataType color using the DataType of the field
            Color? color = null;

            switch (type)
            {
                case GorkGraph.DataType.Float:
                    color = _floatColor;
                    break;
                case GorkGraph.DataType.Int:
                    color = _intColor;
                    break;
                case GorkGraph.DataType.Bool:
                    color = _boolColor;
                    break;
                case GorkGraph.DataType.String:
                    color = _stringColor;
                    break;
            }
            #endregion

            // Draw a top line first so that anything can be drawn above it
            Rect topLine = rect;
            topLine.height = LIST_LINE_SIZE;
            topLine.y -= LIST_LINE_SIZE;

            topLine.x -= 50000;
            topLine.width += 100000;

            EditorGUI.DrawRect(topLine, _listLineColor);

            #region Draw background color
            Rect rightRect = rect;

            // Add width to the rect to make sure it covers the entire right side of the array property
            rightRect.width += 1000;

            EditorGUI.DrawRect(rightRect, _listBackgroundColor);

            // Also draw a rect for the left side. This one is colored!
            Rect leftRect = rect;

            // Move entirely left
            leftRect.width = 1000;
            leftRect.x -= leftRect.width;

            if (color.HasValue)
            {
                Color leftColor = color.Value;
                leftColor.a = 0.1f; // Make the color transparent to make it fit in more

                EditorGUI.DrawRect(leftRect, leftColor);
            }
            // Draw left side with same default background color if the color doesn't have a value
            else
            {
                EditorGUI.DrawRect(leftRect, _listBackgroundColor);
            }
            #endregion

            // Decrease overall height by 1 so we can leave some room for the bottom line
            rect.height -= LIST_LINE_SIZE;

            // Text field
            Rect textFieldRect = rect;
            textFieldRect.height = rect.height - 9;
            textFieldRect.y += (rect.height - textFieldRect.height) / 2;
            textFieldRect.width -= 160;
            textFieldRect.width = Mathf.Clamp(textFieldRect.width, 50, 300);

            // Set a control so we can force focus on this text field if needed
            GUI.SetNextControlName("TextField");

            // Cache and display the name property
            SerializedProperty nameProp = prop.FindPropertyRelative("Name");

            string textValue = EditorGUI.TextField(textFieldRect, nameProp.stringValue);

            // Focus this text field if it's supposed to be focused
            if (_focusParameterElement.HasValue && _focusParameterElement.Value == index)
            {
                GUI.FocusControl("TextField");

                _focusParameterElement = null;
            }

            // Prevent setting the name to empty strings
            if (!string.IsNullOrEmpty(textValue) && textValue != nameProp.stringValue)
            {
                // Also ensure the name is unique
                nameProp.stringValue = GetUniqueParameterName(textValue, index);
            }

            #region Value field
            Rect valueFieldRect = rect;
            valueFieldRect.height = rect.height - 9;
            valueFieldRect.y += (rect.height - valueFieldRect.height) / 2;

            textFieldRect.width += 10;

            valueFieldRect.width -= textFieldRect.width;
            valueFieldRect.width = Mathf.Max(0, valueFieldRect.width);
            valueFieldRect.x =  Mathf.Max(rect.x + rect.width - valueFieldRect.width, textFieldRect.x + textFieldRect.width);

            SerializedProperty valueProp = prop.FindPropertyRelative("Value");
            Type actualType = type.ActualType();
            object value;

            switch (type)
            {
                case GorkGraph.DataType.Float:
                case GorkGraph.DataType.Int:
                case GorkGraph.DataType.Bool:
                    value = GorkUtility.FromJson(valueProp.stringValue, actualType);
                    break;

                default:
                    value = valueProp.stringValue;
                    break;
            }

            switch (type)
            {
                case GorkGraph.DataType.Float:
                    valueProp.stringValue = GorkUtility.ToJson(EditorGUI.FloatField(valueFieldRect, (float)value), actualType);
                    break;
                case GorkGraph.DataType.Int:
                    valueProp.stringValue = GorkUtility.ToJson(EditorGUI.IntField(valueFieldRect, (int)value), actualType);
                    break;
                case GorkGraph.DataType.Bool:
                    valueProp.stringValue = GorkUtility.ToJson(EditorGUI.Toggle(valueFieldRect, (bool)value), actualType);
                    break;
                default:
                    valueProp.stringValue = EditorGUI.TextArea(valueFieldRect, (string)value);
                    break;
            }
            #endregion

            // Draw bottom line
            Rect bottomLine = rect;
            bottomLine.height = LIST_LINE_SIZE;
            bottomLine.y += rect.height;

            // Modify horizontally to make sure it covers the entire list and not just the editable area 
            bottomLine.x -= 50000;
            bottomLine.width += 100000;

            EditorGUI.DrawRect(bottomLine, _listLineColor);

            // Draw a middle line using the data type color
            Rect lineRect = rect;
            lineRect.x = Mathf.Max(lineRect.x, textFieldRect.x + textFieldRect.width) - 6.5f;
            lineRect.width = 3;
            lineRect.y -= 1;
            lineRect.height += 1;

            if (color.HasValue)
            {
                EditorGUI.DrawRect(lineRect, color.Value);
            }

            // Loop and add to the _rects list until we have enough entries
            while (index > _rects.Count - 1)
            {
                _rects.Add(default);
            }

            // Set rect for usage in context menu detection
            _rects[index] = rect;
        }
        #endregion

        #region Event GUI
        private void EventEditorOnGui()
        {
            EditorGUILayout.LabelField("Events are work in progress...");
        }
        #endregion

        private enum ModeEnum
        {
            parameters,
            events,
        }
    }
}
