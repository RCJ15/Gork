using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Gork.Editor
{
    /// <summary>
    /// The side panel of the <see cref="GorkGraphEditor"/> which displays Parameters, Tags and Events.
    /// </summary>
    public class GorkInspectorView : VisualElement
    {
        private const string DROPDOWN_ARROW_BASE64 = "iVBORw0KGgoAAAANSUhEUgAAAAkAAAAJCAYAAADgkQYQAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAABMSURBVChTY/z//z8DIcB45MgRgqqYgLgOwsQJ6hhBJNC0WiDVBGKjgTobG5tmsCIQwKIQrADKRgCQQpAboRpwA2wKiAoCkO8IAAYGADYvH/HWQhuAAAAAAElFTkSuQmCC";
        private static Texture2D _dropdownTexture = null;

        private GenericMenu _createParameterMenu, _createEventMenu = null;

        private ModeEnum _mode;
        private ModeEnum mode
        {
            get => _mode;
            set
            {
                _mode = value;
                GorkEditorSaveData.CurrentInspectorMode = (int)_mode;
            }
        }

        private string _searchFilterString = "";
        private Type _parametersSearchFilter = null;
        private GorkGraph.Event.Type? _eventsSearchFilter = null;
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
                    _cachedParametersList.elementHeightCallback = GetParameterElementHeight;

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
                    _cachedParametersSearchList.elementHeightCallback = i =>
                    {
                        if (i < 0 || i >= _parametersSearchFilterList.Count)
                        {
                            return _cachedParametersSearchList.elementHeight;
                        }

                        return GetParameterElementHeight(_parametersSearchFilterList[i]);
                    };

                    _cachedParametersSearchList.showDefaultBackground = false;

                    _cachedParametersSearchList.elementHeight = LIST_ELEMENT_SIZE;
                }

                return _cachedParametersSearchList;
            }
        }
        #endregion

        #region Tags List
        private ReorderableList _cachedTagsList;
        private ReorderableList TagsList
        {
            get
            {
                // Create and cache list if null
                if (_cachedTagsList == null)
                {
                    SerializedProperty prop = GraphSerializedObject.FindProperty("_tags");

                    _cachedTagsList = new ReorderableList(GraphSerializedObject, prop, true, false, false, true);

                    _cachedTagsList.drawElementCallback = TagListDrawElement;
                    _cachedTagsList.drawHeaderCallback = null;
                    _cachedTagsList.onRemoveCallback = list => RemoveTag(list.index);

                    _cachedTagsList.showDefaultBackground = false;

                    _cachedTagsList.elementHeight = LIST_ELEMENT_SIZE;
                }

                return _cachedTagsList;
            }
        }
        private int? _focusTagElement = null;

        private List<int> _tagsSearchFilterList = new List<int>();
        private ReorderableList _cachedTagsSearchList;
        private ReorderableList TagsSearchList
        {
            get
            {
                if (_cachedTagsSearchList == null)
                {
                    _cachedTagsSearchList = new ReorderableList(_tagsSearchFilterList, typeof(int), false, false, false, true);

                    _cachedTagsSearchList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => TagListDrawElement(rect, _tagsSearchFilterList[index], isActive, isFocused);
                    _cachedTagsSearchList.drawHeaderCallback = null;
                    _cachedTagsSearchList.onRemoveCallback = list =>
                    {
                        RemoveTag(list.index);

                        UpdateTagsSearchList(false);
                    };

                    _cachedTagsSearchList.showDefaultBackground = false;

                    _cachedTagsSearchList.elementHeight = LIST_ELEMENT_SIZE;
                }

                return _cachedTagsSearchList;
            }
        }
        #endregion

        #region Events List
        private ReorderableList _cachedEventsList;
        private ReorderableList EventsList
        {
            get
            {
                // Create and cache list if null
                if (_cachedEventsList == null)
                {
                    SerializedProperty prop = GraphSerializedObject.FindProperty("_events");

                    _cachedEventsList = new ReorderableList(GraphSerializedObject, prop, true, false, false, true);

                    _cachedEventsList.drawElementCallback = EventListDrawElement;
                    _cachedEventsList.drawHeaderCallback = null;
                    _cachedEventsList.onRemoveCallback = list => RemoveEvent(list.index);

                    _cachedEventsList.showDefaultBackground = false;

                    _cachedEventsList.elementHeight = LIST_ELEMENT_SIZE;
                }

                return _cachedEventsList;
            }
        }
        private int? _focusEventElement = null;

        private List<int> _eventsSearchFilterList = new List<int>();
        private ReorderableList _cachedEventsSearchList;
        private ReorderableList EventsSearchList
        {
            get
            {
                if (_cachedEventsSearchList == null)
                {
                    _cachedEventsSearchList = new ReorderableList(_eventsSearchFilterList, typeof(int), false, false, false, true);

                    _cachedEventsSearchList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => EventListDrawElement(rect, _eventsSearchFilterList[index], isActive, isFocused);
                    _cachedEventsSearchList.drawHeaderCallback = null;
                    _cachedEventsSearchList.onRemoveCallback = list =>
                    {
                        RemoveEvent(list.index);

                        UpdateEventsSearchList(false);
                    };

                    _cachedEventsSearchList.showDefaultBackground = false;

                    _cachedEventsSearchList.elementHeight = LIST_ELEMENT_SIZE;
                }

                return _cachedEventsSearchList;
            }
        }

        private static readonly Color _externalEventColor = new Color(0.564705f, 0, 1);
        private static readonly Color _internalEventColor = new Color(0, 0.717647f, 0.094117f);
        #endregion

        private static readonly Color _listBackgroundColor = new Color(1, 1, 1, 0.015686f);
        private static readonly Color _listLineColor = new Color(0, 0, 0, 0.372549f);
        private static readonly Color _defaultLeftSideColor = new Color(1, 1, 1, 0.1f);

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

        private List<Rect> _rects = new List<Rect>();

        public GorkGraphView GraphView { get; internal set; }
        private GorkGraph Graph => GraphView?.Graph;
        private SerializedObject GraphSerializedObject => Graph == null ? null : Graph.SerializedObject;

        public new class UxmlFactory : UxmlFactory<GorkInspectorView, VisualElement.UxmlTraits> { }

        public GorkInspectorView()
        {
            _mode = (ModeEnum)GorkEditorSaveData.CurrentInspectorMode;

            RegisterCallback<MouseUpEvent>(HandleRightClick);

            RegisterCallback<FocusInEvent>(OnFocusIn);
            RegisterCallback<FocusOutEvent>(OnFocusOut);
        }

        public void OnOpenGraph(GorkGraph graph)
        {
            _createParameterMenu = null;
            _createEventMenu = null;

            _cachedParametersList = null;
            _cachedTagsList = null;
            _cachedEventsList = null;
        }

        private void HandleRightClick(MouseUpEvent evt)
        {
            // Return if this is not a right click or if the GraphSerializedObject is null
            if (evt.button != (int)MouseButton.RightMouse || GraphSerializedObject == null)
            {
                return;
            }

            // Determine what list we are doing
            SerializedProperty prop;
            Action<int> duplicateElement;
            Action<int> removeElement; 

            switch (mode)
            {
                default:
                    prop = ParametersList.serializedProperty;

                    duplicateElement = DuplicateParameter;
                    removeElement = RemoveParameter;
                    break;

                case ModeEnum.Tags:
                    prop = TagsList.serializedProperty;

                    duplicateElement = DuplicateTag;
                    removeElement = RemoveTag;
                    break;

                case ModeEnum.Events:
                    prop = EventsList.serializedProperty;

                    duplicateElement = DuplicateEvent;
                    removeElement = RemoveEvent;
                    break;
            }

            // Loop through every single element in the list
            int length = prop.arraySize;

            for (int i = 0; i < length; i++)
            {
                Rect rect = _rects[i];

                // Check if the mouse is inside this element
                if (rect.Contains(evt.localMousePosition))
                {
                    // If so, then bring up a context menu
                    GenericMenu menu = new GenericMenu();

                    menu.AddItem(new GUIContent("Duplicate"), false, () => duplicateElement?.Invoke(i));
                    menu.AddItem(new GUIContent("Delete"), false, () => removeElement?.Invoke(i));

                    menu.ShowAsContext();
                    break;
                }
            }

            // Nothing was right clicked :(
        }

        public void Initialize(VisualElement root)
        {
            // Setup IMGUI Container
            _imguiContainer = new IMGUIContainer(OnGui);

            Add(_imguiContainer);

            // Parameter & event buttons
            ToolbarButton parameterButton = root.Q<ToolbarButton>("Parameters");
            ToolbarButton tagsButton = root.Q<ToolbarButton>("Tags");
            ToolbarButton eventsButton = root.Q<ToolbarButton>("Events");

            VisualElement parameterHighlight = parameterButton.Q<VisualElement>("Highlight");
            VisualElement tagsHighlight = tagsButton.Q<VisualElement>("Highlight");
            VisualElement eventsHighlight = eventsButton.Q<VisualElement>("Highlight");

            void ParametersButtonColorChange()
            {
                parameterHighlight.visible = true;
                tagsHighlight.visible = false;
                eventsHighlight.visible = false;
            }

            void TagsButtonColorChange()
            {
                parameterHighlight.visible = false;
                tagsHighlight.visible = true;
                eventsHighlight.visible = false;
            }

            void EventsButtonColorChange()
            {
                parameterHighlight.visible = false;
                tagsHighlight.visible = false;
                eventsHighlight.visible = true;
            }

            switch (_mode)
            {
                case ModeEnum.Parameters:
                    ParametersButtonColorChange();
                    break;
                case ModeEnum.Tags:
                    TagsButtonColorChange();
                    break;
                case ModeEnum.Events:
                    EventsButtonColorChange();
                    break;
            }

            parameterButton.clicked += () =>
            {
                if (mode == ModeEnum.Parameters)
                {
                    return;
                }

                ParametersButtonColorChange();

                mode = ModeEnum.Parameters;

                UpdateParametersSearchList();
            };

            tagsButton.clicked += () =>
            {
                if (mode == ModeEnum.Tags)
                {
                    return;
                }

                TagsButtonColorChange();

                mode = ModeEnum.Tags;

                _parametersSearchFilter = null;
                UpdateTagsSearchList();
            };

            eventsButton.clicked += () =>
            {
                if (mode == ModeEnum.Events)
                {
                    return;
                }

                EventsButtonColorChange();

                mode = ModeEnum.Events;

                _parametersSearchFilter = null;
            };

            ToolbarButton addButton = root.Q<ToolbarButton>("AddButton");

            #region Dropdown Arrow
            // Cache and load the texture if it's not been loaded yet
            if (_dropdownTexture == null)
            {
                // Load texture from base 64
                _dropdownTexture = GorkEditorUtility.Texture2DFromBase64(DROPDOWN_ARROW_BASE64);
            }

            addButton.Q<VisualElement>("Image").style.backgroundImage = _dropdownTexture;
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
                        AddParameterToGraph("New Float", typeof(float), 0f);
                    });
                    _createParameterMenu.AddItem(new GUIContent("Int"), false, () =>
                    {
                        AddParameterToGraph("New Int", typeof(int), 0);
                    });
                    _createParameterMenu.AddItem(new GUIContent("Bool"), false, () =>
                    {
                        AddParameterToGraph("New Bool", typeof(bool), true);
                    });
                    _createParameterMenu.AddItem(new GUIContent("String"), false, () =>
                    {
                        AddParameterToGraph("New String", typeof(string), null);
                    });
                }
                #endregion

                #region Create Events Menu
                // Create and cache the create event menu if it's null
                if (_createEventMenu == null)
                {
                    _createEventMenu = new GenericMenu();

                    // Add an entry for creating every type of event
                    _createEventMenu.AddItem(new GUIContent("External Event"), false, () =>
                    {
                        AddEventToGraph("New External Event", GorkGraph.Event.Type.External);
                    });

                    _createEventMenu.AddItem(new GUIContent("Internal Event"), false, () =>
                    {
                        AddEventToGraph("New Internal Event", GorkGraph.Event.Type.Internal);
                    });
                }
                #endregion

                // Determine where to spawn the dropdown
                Rect addButtonRect = new Rect(addButton.LocalToWorld(Vector2.zero), addButton.contentRect.size * 1.5f);

                // Spawn the correct dropdown based on which mode is currently active
                switch (mode)
                {
                    case ModeEnum.Parameters:
                        _createParameterMenu.DropDown(addButtonRect);
                        break;

                    case ModeEnum.Tags:
                        AddTagToGraph("New Tag");
                        break;

                    case ModeEnum.Events:
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

                switch (mode)
                {
                    case ModeEnum.Parameters:
                        UpdateParametersSearchList();
                        break;

                    case ModeEnum.Tags:
                        UpdateTagsSearchList();
                        break;

                    case ModeEnum.Events:
                        UpdateEventsSearchList();
                        break;
                }
            });

            searchFieldButton.clicked += () =>
            {
                // Determine where to spawn the dropdown
                Rect searchFieldButtonRect = new Rect(searchFieldButton.LocalToWorld(Vector2.zero), searchFieldButton.contentRect.size * 1.5f);

                // Spawn the correct dropdown based on which mode is currently active
                // These menus can't be cached since the entries in the menus will display if they are selected or not which can't be edited once the menu is created (stupid Unity thing)
                switch (mode)
                {
                    case ModeEnum.Parameters:
                        #region Create Filter Parameters Menu
                        // Create the filter parameters menu
                        GenericMenu filterParametersMenu = new GenericMenu();

                        // Add an entry for setting the filter for each data type
                        filterParametersMenu.AddItem(new GUIContent("Name"), _parametersSearchFilter == null, () =>
                        {
                            _parametersSearchFilter = null;
                            UpdateParametersSearchList();
                        });
                        filterParametersMenu.AddItem(new GUIContent("Float"), _parametersSearchFilter != null && _parametersSearchFilter == typeof(float), () =>
                        {
                            _parametersSearchFilter = typeof(float);
                            UpdateParametersSearchList();
                        });
                        filterParametersMenu.AddItem(new GUIContent("Int"), _parametersSearchFilter != null && _parametersSearchFilter == typeof(int), () =>
                        {
                            _parametersSearchFilter = typeof(int);
                            UpdateParametersSearchList();
                        });
                        filterParametersMenu.AddItem(new GUIContent("Bool"), _parametersSearchFilter != null && _parametersSearchFilter == typeof(bool), () =>
                        {
                            _parametersSearchFilter = typeof(bool);
                            UpdateParametersSearchList();
                        });
                        filterParametersMenu.AddItem(new GUIContent("String"), _parametersSearchFilter != null && _parametersSearchFilter == typeof(string), () =>
                        {
                            _parametersSearchFilter = typeof(string);
                            UpdateParametersSearchList();
                        });
                        #endregion

                        filterParametersMenu.DropDown(searchFieldButtonRect);
                        break;

                    case ModeEnum.Events:

                        #region Filter Events Menu
                        // Create the filter events menu
                        GenericMenu filterEventsMenu = new GenericMenu();

                        // Add an entry for setting the filter for each event type
                        filterEventsMenu.AddItem(new GUIContent("Name"), !_eventsSearchFilter.HasValue, () =>
                        {
                            _eventsSearchFilter = null;
                            UpdateEventsSearchList();
                        });
                        filterEventsMenu.AddItem(new GUIContent("External Event"), _eventsSearchFilter.HasValue && _eventsSearchFilter.Value == GorkGraph.Event.Type.External, () =>
                        {
                            _eventsSearchFilter = GorkGraph.Event.Type.External;
                            UpdateEventsSearchList();
                        });
                        filterEventsMenu.AddItem(new GUIContent("Internal Event"), _eventsSearchFilter.HasValue && _eventsSearchFilter.Value == GorkGraph.Event.Type.Internal, () =>
                        {
                            _eventsSearchFilter = GorkGraph.Event.Type.Internal;
                            UpdateEventsSearchList();
                        });
                        #endregion

                        filterEventsMenu.DropDown(searchFieldButtonRect);
                        break;
                }
            };
        }

        private void OnFocusIn(FocusInEvent e)
        {
            if (GraphView == null)
            {
                return;
            }

            GraphView.isReframable = false;
        }

        private void OnFocusOut(FocusOutEvent e)
        {
            if (GraphView == null)
            {
                return;
            }

            GraphView.isReframable = true;
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

        private void OnGui()
        {
            if (GraphSerializedObject == null)
            {
                return;
            }

            ReorderableList list;
            ReorderableList searchList;

            switch (mode)
            {
                default:
                    list = ParametersList;
                    searchList = ParametersSearchList;
                    break;

                case ModeEnum.Tags:
                    list = TagsList;
                    searchList = TagsSearchList;
                    break;

                case ModeEnum.Events:
                    list = EventsList;
                    searchList = EventsSearchList;
                    break;
            }

            GraphSerializedObject.UpdateIfRequiredOrScript();

            Rect contentRect = _imguiContainer.parent.contentRect;

            bool displaySearchBar = (mode == ModeEnum.Parameters && _parametersSearchFilter != null) || (mode == ModeEnum.Events && _eventsSearchFilter.HasValue) || !string.IsNullOrEmpty(_searchFilterString);

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
            ReorderableList appropriateList = displaySearchBar ? searchList : list;

            float singleElementSize = appropriateList.elementHeight;
            float listHeight = -singleElementSize;

            int count = appropriateList.count;

            for (int i = 0; i < count; i++)
            {
                listHeight += (appropriateList.elementHeightCallback == null ? appropriateList.elementHeight : appropriateList.elementHeightCallback.Invoke(i)) + 2;
            }

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
                searchList.DoList(rect);
            }
            // Display the regular parameters list
            else
            {
                list.DoList(rect);
            }

            GUI.EndScrollView();

            // Apply the changes and record undo
            GraphSerializedObject.ApplyModifiedProperties();
        }

        private string GetUniqueName(string name, SerializedProperty prop, Func<SerializedProperty, string> getName, int? index = null)
        {
            string startName = name;
            List<string> arrayNames = new List<string>();

            // Loop through entire parameter list and add the names to the arrayNames list
            int length = prop.arraySize;

            for (int i = 0; i < length; i++)
            {
                if (index.HasValue && index.Value == i)
                {
                    continue;
                }

                arrayNames.Add(getName.Invoke(prop.GetArrayElementAtIndex(i)).ToLower());
            }

            // Try to detect the last number in the name
            string trimmedName = startName.TrimEnd();
            int lastNumber = 0;

            // Check if the last letter is a number
            int trimmedNameLength = trimmedName.Length;

            // This only works if the name is more than 1 characters
            if (trimmedNameLength > 1)
            {
                char lastChar = trimmedName[trimmedNameLength - 1];

                if (char.IsNumber(lastChar))
                {
                    // If so, then we loop from there until we find a whitespace and compile that to our numberString
                    string numberString = lastChar.ToString();

                    if (trimmedNameLength > 2)
                    {
                        for (int i = trimmedNameLength - 2; i >= 0; i--)
                        {
                            char c = trimmedName[i];

                            if (!char.IsNumber(c))
                            {
                                break;
                            }

                            numberString += c;
                        }
                    }

                    int.TryParse(numberString, out lastNumber);
                }

            }
            // Do while loop to determine if the name is unique or not
            int loopAmount = lastNumber;

            // Remove numbers and whitespace at the end of the name
            string trimmedNameNoNumbers = trimmedName.TrimEnd('0', '1', '2', '3', '4', '5', '6', '7', '8', '9').TrimEnd();
            do
            {
                // Name is NOT in array name list?
                if (!arrayNames.Contains(name.ToLower()))
                {
                    // We found a unique name!
                    break;
                }

                name = $"{trimmedNameNoNumbers} {loopAmount}";

                loopAmount++;
            }
            while (loopAmount <= 1000 + lastNumber); // Limit to only loop a maximum of 1000 times

            return name;
        }

        private void SetRect(int index, Rect rect)
        {
            // Loop and add to the _rects list until we have enough entries
            while (index > _rects.Count - 1)
            {
                _rects.Add(default);
            }

            rect.y -= _scrollPosition.y;

            // Set rect for usage in context menu detection
            _rects[index] = rect;
        }

        private void DrawListElementLines(Rect rect)
        {
            // Draw a top line first so that anything can be drawn above it
            Rect lineRect = rect;
            lineRect.height = LIST_LINE_SIZE;
            lineRect.y -= LIST_LINE_SIZE;

            // Modify horizontally to make sure it covers the entire list and not just the editable area 
            lineRect.x -= Screen.width;
            lineRect.width += Screen.width * 2;

            EditorGUI.DrawRect(lineRect, _listLineColor);

            // Draw bottom line by reusing the same rect
            lineRect.y = rect.y + rect.height;

            EditorGUI.DrawRect(lineRect, _listLineColor);
        }

        private void DrawLeftSideColor(Rect rect, Color color)
        {
            // Move entirely left
            rect.width = 1000;
            rect.x -= rect.width;

            EditorGUI.DrawRect(rect, color);
        }

        #region Parameter GUI
        private void AddParameterToGraph(string name, Type type, object defaultValue)
        {
            name = GetUniqueParameterName(name);

            if (!name.ToLower().Contains(_searchFilterString.ToLower()))
            {
                _searchField.value = "";
            }

            SerializedProperty prop = ParametersList.serializedProperty;

            int index = prop.arraySize;
            prop.InsertArrayElementAtIndex(index);

            _focusParameterElement = index;

            SerializedProperty newParameter = prop.GetArrayElementAtIndex(index);

            newParameter.FindPropertyRelative("Name").stringValue = name;
            newParameter.FindPropertyRelative("SerializedType").stringValue = type.AssemblyQualifiedName;
            newParameter.FindPropertyRelative("Value").stringValue = GorkUtility.ToJson(defaultValue, type);

            // Reset search filter
            _parametersSearchFilter = null;
            UpdateParametersSearchList(false);

            GraphSerializedObject.ApplyModifiedProperties();

            _rects.Add(new Rect());

            _scrollPosition = new Vector2(0, (float)ParametersList.elementHeight * (float)index);
        }

        private void DuplicateParameter(int index)
        {
            // Get properties
            SerializedProperty prop = ParametersList.serializedProperty;
            SerializedProperty copiedProp = prop.GetArrayElementAtIndex(index);

            // Create the new paramter
            prop.InsertArrayElementAtIndex(index);
            SerializedProperty newProp = prop.GetArrayElementAtIndex(index + 1);

            // Copy values from the original property
            string startName = copiedProp.FindPropertyRelative("Name").stringValue;
            newProp.FindPropertyRelative("Name").stringValue = GetUniqueParameterName(startName, index + 1);
            newProp.FindPropertyRelative("SerializedType").stringValue = copiedProp.FindPropertyRelative("SerializedType").stringValue;
            newProp.FindPropertyRelative("Value").stringValue = copiedProp.FindPropertyRelative("Value").stringValue;

            GraphSerializedObject.ApplyModifiedProperties();

            // Select the new parameter
            ParametersList.Select(index + 1);

            Vector2 scrollPos = _scrollPosition;
            UpdateTagsSearchList();

            _scrollPosition = scrollPos;
        }

        private void RemoveParameter(int index)
        {
            // Delete the array element
            ParametersList.serializedProperty.DeleteArrayElementAtIndex(index);
            GraphSerializedObject.ApplyModifiedProperties();

            // Select the element above and ensure that the selection index is above 0
            ParametersList.Select(Mathf.Max(index - 1, 0));

            Vector2 scrollPos = _scrollPosition;
            UpdateParametersSearchList();

            _scrollPosition = scrollPos;
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
                if (_parametersSearchFilter == null)
                {
                    _parametersSearchFilterList.Add(i);
                }
                // Otherwise we will check and only add the matching DataTypes
                else
                {
                    SerializedProperty typeProp = arrayElement.FindPropertyRelative("SerializedType");

                    if (_parametersSearchFilter == Type.GetType(typeProp.stringValue))
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

        private string GetUniqueParameterName(string startName, int? index = null)
        {
            SerializedProperty prop = ParametersList.serializedProperty;

            return GetUniqueName(startName, prop, prop => prop.FindPropertyRelative("Name").stringValue, index);
        }

        private void ParameterListDrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            // Cache this array property
            SerializedProperty prop = ParametersList.serializedProperty.GetArrayElementAtIndex(index);

            // Remove offscreen offset + 5 for a bit of extra space
            rect.width -= LIST_SIZE_OFFSET + 5;

            // Cache the DataType of the array property
            string typeName = prop.FindPropertyRelative("SerializedType").stringValue;
            Type type = string.IsNullOrEmpty(typeName) ? null : Type.GetType(typeName);

            #region DataType Color
            // Determine DataType color using the DataType of the field
            Color? color = null;

            if (type == typeof(float))
            {
                color = _floatColor;
            }
            else if (type == typeof(int))
            {
                color = _intColor;
            }
            else if (type == typeof(bool))
            {
                color = _boolColor;
            }
            else if (type == typeof(string))
            {
                color = _stringColor;
            }
            #endregion

            #region Draw background color
            Rect rightRect = rect;

            // Add width to the rect to make sure it covers the entire right side of the array property
            rightRect.width += 1000;

            EditorGUI.DrawRect(rightRect, _listBackgroundColor);

            // Draw a background color to the left side
            Color leftColor;

            if (color.HasValue)
            {
                leftColor = color.Value;
                leftColor.a = 0.1f; // Make the color transparent to make it fit in more
            }
            // Draw left side with the default left side color if the color doesn't have a value
            else
            {
                leftColor = _defaultLeftSideColor;
            }

            DrawLeftSideColor(rect, leftColor);
            #endregion

            // Decrease overall height by 1 so we can leave some room for the bottom line
            rect.height -= LIST_LINE_SIZE;

            // Text field
            Rect textFieldRect = rect;
            textFieldRect.height = LIST_ELEMENT_SIZE - 9;
            textFieldRect.y += (LIST_ELEMENT_SIZE - textFieldRect.height) / 2;
            textFieldRect.width -= 160;
            textFieldRect.width = Mathf.Clamp(textFieldRect.width, 50, 250);

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
            if (!string.IsNullOrEmpty(textValue.Trim()) && textValue != nameProp.stringValue)
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
            object value;

            if (type == typeof(string))
            {
                value = valueProp.stringValue;
            }
            else
            {
                value = GorkUtility.FromJson(valueProp.stringValue, type);
            }

            if (type == typeof(float))
            {
                valueProp.stringValue = GorkUtility.ToJson(EditorGUI.FloatField(valueFieldRect, (float)value), type);
            }
            else if (type == typeof(int))
            {
                valueProp.stringValue = GorkUtility.ToJson(EditorGUI.IntField(valueFieldRect, (int)value), type);
            }
            else if (type == typeof(bool))
            {
                valueProp.stringValue = GorkUtility.ToJson(EditorGUI.Toggle(valueFieldRect, (bool)value), type);
            }
            else
            {
                valueProp.stringValue = EditorGUI.TextArea(valueFieldRect, (string)value);
            }
            #endregion

            DrawListElementLines(rect);

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

            SetRect(index, rect);
        }

        private static readonly string StringAssemblyQualifiedName = typeof(string).AssemblyQualifiedName;
        private float GetParameterElementHeight(int index)
        {
            if (index >= ParametersList.serializedProperty.arraySize)
            {
                return LIST_ELEMENT_SIZE;
            }

            SerializedProperty prop = ParametersList.serializedProperty.GetArrayElementAtIndex(index);

            string typeName = prop.FindPropertyRelative("SerializedType").stringValue;

            // Check if the paramter type is of type string 
            if (typeName == StringAssemblyQualifiedName)
            {
                // Get the value from the property
                string value = prop.FindPropertyRelative("Value").stringValue;

                // Count how many line breaks there are in the string
                int linebreakAmount = value.Count(c => c == '\n');

                return LIST_ELEMENT_SIZE + (14.5f * linebreakAmount);
            }

            return LIST_ELEMENT_SIZE;
        }
        #endregion

        #region Tags GUI
        private void AddTagToGraph(string name)
        {
            name = GetUniqueTagName(name);

            if (!name.ToLower().Contains(_searchFilterString.ToLower()))
            {
                _searchField.value = "";
            }

            SerializedProperty prop = TagsList.serializedProperty;

            int index = prop.arraySize;
            prop.InsertArrayElementAtIndex(index);

            _focusTagElement = index;

            SerializedProperty newTag = prop.GetArrayElementAtIndex(index);

            newTag.stringValue = name;

            // Reset search filter
            UpdateTagsSearchList(false);

            GraphSerializedObject.ApplyModifiedProperties();

            _scrollPosition = new Vector2(0, (float)TagsList.elementHeight * (float)index);
        }

        private void DuplicateTag(int index)
        {
            // Get properties
            SerializedProperty prop = TagsList.serializedProperty;
            SerializedProperty copiedProp = prop.GetArrayElementAtIndex(index);

            // Create the new paramter
            prop.InsertArrayElementAtIndex(index);
            SerializedProperty newProp = prop.GetArrayElementAtIndex(index + 1);

            // Copy values from the original property
            newProp.stringValue = GetUniqueTagName(copiedProp.stringValue, index + 1);

            GraphSerializedObject.ApplyModifiedProperties();

            // Select the new parameter
            TagsList.Select(index + 1);

            Vector2 scrollPos = _scrollPosition;
            UpdateTagsSearchList();

            _scrollPosition = scrollPos;
        }

        private void RemoveTag(int index)
        {
            // Delete the array element
            TagsList.serializedProperty.DeleteArrayElementAtIndex(index);
            GraphSerializedObject.ApplyModifiedProperties();

            // Select the element above and ensure that the selection index is above 0
            TagsList.Select(Mathf.Max(index - 1, 0));

            Vector2 scrollPos = _scrollPosition;
            UpdateTagsSearchList();

            _scrollPosition = scrollPos;
        }

        private void UpdateTagsSearchList(bool repaint = true)
        {
            _tagsSearchFilterList.Clear();

            SerializedProperty prop = TagsList.serializedProperty;

            int length = prop.arraySize;

            // Filter
            for (int i = 0; i < length; i++)
            {
                SerializedProperty arrayElement = prop.GetArrayElementAtIndex(i);

                // Check if the name matches the search filter string
                if (!string.IsNullOrEmpty(_searchFilterString))
                {
                    if (!arrayElement.stringValue.ToLower().Contains(_searchFilterString.ToLower()))
                    {
                        continue;
                    }
                }

                _tagsSearchFilterList.Add(i);
            }

            if (repaint)
            {
                _imguiContainer.MarkDirtyRepaint();
            }

            _scrollPosition = Vector2.zero;
        }

        private string GetUniqueTagName(string startName, int? index = null)
        {
            SerializedProperty prop = TagsList.serializedProperty;

            return GetUniqueName(startName, prop, prop => prop.stringValue, index);
        }

        private void TagListDrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            // Cache this array property
            SerializedProperty prop = TagsList.serializedProperty.GetArrayElementAtIndex(index);

            // Remove offscreen offset + 5 for a bit of extra space
            rect.width -= LIST_SIZE_OFFSET + 5;

            Rect textFieldRect = rect;
            textFieldRect.height = rect.height - 9;
            textFieldRect.y += (rect.height - textFieldRect.height) / 2;

            // Set a control so we can force focus on this text field if needed
            GUI.SetNextControlName("TextField");

            // Draw the text field
            string textValue = EditorGUI.TextField(textFieldRect, prop.stringValue);

            // Prevent setting the name to empty strings
            if (!string.IsNullOrEmpty(textValue.Trim()) && textValue != prop.stringValue)
            {
                // Also ensure the name is unique
                prop.stringValue = GetUniqueTagName(textValue, index);
            }

            // Focus this text field if it's supposed to be focused
            if (_focusTagElement.HasValue && _focusTagElement.Value == index)
            {
                GUI.FocusControl("TextField");

                _focusTagElement = null;
            }

            // Draw colored left side
            DrawLeftSideColor(rect, _defaultLeftSideColor);

            DrawListElementLines(rect);

            SetRect(index, rect);
        }
        #endregion

        #region Event GUI
        private void AddEventToGraph(string name, GorkGraph.Event.Type eventType)
        {
            name = GetUniqueEventName(name);

            if (!name.ToLower().Contains(_searchFilterString.ToLower()))
            {
                _searchField.value = "";
            }

            SerializedProperty prop = EventsList.serializedProperty;

            int index = prop.arraySize;
            prop.InsertArrayElementAtIndex(index);

            _focusEventElement = index;

            SerializedProperty newTag = prop.GetArrayElementAtIndex(index);

            newTag.FindPropertyRelative("Name").stringValue = name;
            newTag.FindPropertyRelative("EventType").enumValueIndex = (int)eventType;

            // Reset search filter
            UpdateEventsSearchList(false);

            GraphSerializedObject.ApplyModifiedProperties();

            _scrollPosition = new Vector2(0, (float)EventsList.elementHeight * (float)index);
        }

        private void DuplicateEvent(int index)
        {
            // Get properties
            SerializedProperty prop = EventsList.serializedProperty;
            SerializedProperty copiedProp = prop.GetArrayElementAtIndex(index);

            // Create the new paramter
            prop.InsertArrayElementAtIndex(index);
            SerializedProperty newProp = prop.GetArrayElementAtIndex(index + 1);

            // Copy values from the original property
            newProp.FindPropertyRelative("Name").stringValue = GetUniqueEventName(copiedProp.FindPropertyRelative("Name").stringValue, index + 1);

            GraphSerializedObject.ApplyModifiedProperties();

            // Select the new parameter
            EventsList.Select(index + 1);

            Vector2 scrollPos = _scrollPosition;
            UpdateEventsSearchList();

            _scrollPosition = scrollPos;
        }

        private void RemoveEvent(int index)
        {
            // Delete the array element
            EventsList.serializedProperty.DeleteArrayElementAtIndex(index);
            GraphSerializedObject.ApplyModifiedProperties();

            // Select the element above and ensure that the selection index is above 0
            EventsList.Select(Mathf.Max(index - 1, 0));

            Vector2 scrollPos = _scrollPosition;
            UpdateEventsSearchList();

            _scrollPosition = scrollPos;
        }

        private void UpdateEventsSearchList(bool repaint = true)
        {
            _eventsSearchFilterList.Clear();

            SerializedProperty prop = EventsList.serializedProperty;

            int length = prop.arraySize;

            // Filter
            for (int i = 0; i < length; i++)
            {
                SerializedProperty arrayElement = prop.GetArrayElementAtIndex(i);

                // Check if the name matches the search filter string
                if (!string.IsNullOrEmpty(_searchFilterString))
                {
                    SerializedProperty nameProp = arrayElement.FindPropertyRelative("Name");
                    
                    if (!nameProp.stringValue.ToLower().Contains(_searchFilterString.ToLower()))
                    {
                        continue;
                    }
                }

                // Add this index regularly if the search filter type has no value
                if (!_eventsSearchFilter.HasValue)
                {
                    _eventsSearchFilterList.Add(i);
                }
                // Otherwise we will check and only add the matching DataTypes
                else
                {
                    SerializedProperty typeProp = arrayElement.FindPropertyRelative("EventType");

                    if (_eventsSearchFilter.Value == (GorkGraph.Event.Type)typeProp.enumValueIndex)
                    {
                        _eventsSearchFilterList.Add(i);
                    }
                }
            }

            if (repaint)
            {
                _imguiContainer.MarkDirtyRepaint();
            }

            _scrollPosition = Vector2.zero;
        }

        private string GetUniqueEventName(string startName, int? index = null)
        {
            SerializedProperty prop = EventsList.serializedProperty;

            return GetUniqueName(startName, prop, prop => prop.FindPropertyRelative("Name").stringValue, index);
        }

        private void EventListDrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            // Cache this array property
            SerializedProperty prop = EventsList.serializedProperty.GetArrayElementAtIndex(index);

            // Remove offscreen offset + 5 for a bit of extra space
            rect.width -= LIST_SIZE_OFFSET + 5;

            Rect textFieldRect = rect;
            textFieldRect.height = rect.height - 9;
            textFieldRect.y += (rect.height - textFieldRect.height) / 2;

            // Set a control so we can force focus on this text field if needed
            GUI.SetNextControlName("TextField");

            SerializedProperty nameProp = prop.FindPropertyRelative("Name");

            // Draw the text field
            string textValue = EditorGUI.TextField(textFieldRect, nameProp.stringValue);

            // Prevent setting the name to empty strings
            if (!string.IsNullOrEmpty(textValue.Trim()) && textValue != nameProp.stringValue)
            {
                // Also ensure the name is unique
                nameProp.stringValue = GetUniqueEventName(textValue, index);
            }

            // Focus this text field if it's supposed to be focused
            if (_focusEventElement.HasValue && _focusEventElement.Value == index)
            {
                GUI.FocusControl("TextField");

                _focusEventElement = null;
            }

            // Draw colored left side based on the event type
            // External = Purple
            // Internal = Green
            GorkGraph.Event.Type eventType = (GorkGraph.Event.Type)prop.FindPropertyRelative("EventType").enumValueIndex;
            Color color = _defaultLeftSideColor;

            switch (eventType)
            {
                case GorkGraph.Event.Type.External:
                    color = _externalEventColor;
                    break;

                case GorkGraph.Event.Type.Internal:
                    color = _internalEventColor;
                    break;
            }

            color.a = 0.1f;

            DrawLeftSideColor(rect, color);

            DrawListElementLines(rect);

            SetRect(index, rect);
        }

        #endregion

        private enum ModeEnum
        {
            Parameters,
            Tags,
            Events,
        }
    }
}
