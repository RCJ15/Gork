using System;
using System.Reflection;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;

namespace Gork.Editor
{
    /// <summary>
    /// The <see cref="EditorWindow"/> for the entire GorkGraphEditor window.
    /// </summary>
    public class GorkGraphEditor : EditorWindow
    {
        public const string DEFAULT_WINDOW_NAME = "Gork Graph Editor";

        #region GetPath
        private static string _path = null;
        public static string GorkPath
        {
            get
            {
                if (!string.IsNullOrEmpty(_path))
                {
                    return _path;
                }

                foreach (string guid in AssetDatabase.FindAssets($"{nameof(GorkGraphEditor)} t: MonoScript"))
                {
                    _path = AssetDatabase.GUIDToAssetPath(guid);

                    if (!string.IsNullOrEmpty(_path))
                    {
                        break;
                    }
                }

                if (string.IsNullOrEmpty(_path))
                {
                    throw new Exception("The script GorkGraphEditor.cs is missing!");
                }

                return _path;
            }
        }
        public static string GetPath(string extension)
        {
            return Path.ChangeExtension(GorkPath, extension);
        }

        public static VisualTreeAsset GetVisualTree(string fileName)
        {
            string path = GorkPath;
            int lastSlashIndex = path.LastIndexOf('/');

            path = path.Substring(0, lastSlashIndex + 1);
            path = Path.Combine(path, $"{fileName}.uxml");

            return AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path);
        }
        #endregion

        public static Action UpdateAllMinimaps;
        public static Action<float> UpdateAllSplitWidth;

        private VisualElement _parent;
        private Label _noGraphText;

        private Label _currentlyEditingText;
        private string _currentlyEditingDefaultText;

        [SerializeField] private Vector2 scrollPosition;
        [SerializeField] private float zoomScale;

        [SerializeField] private bool splitViewMinimized;
        [SerializeField] private bool splitViewMaximized;

        [SerializeField] private GorkInspectorView.InspectorMode inspectorMode;

        [SerializeField] private GorkGraph graph;
        private GorkGraphView _graphView;
        private GorkInspectorView _inspectorView;
        private GorkSplitView _splitView;

        #region Minimap Button
        private ToolbarButton _toggleMinimapButton;
        private VisualElement _minimapOpenIcon;
        private VisualElement _minimapClosedIcon;
        private VisualElement _minimapHighlight;
        #endregion

        private static VisualTreeAsset _gorkGraphViewVisualTree = null;
        private static StyleSheet _gorkGraphViewStyleSheet = null;

        // Getting current open file path in the project window using System.Reflection
        private MethodInfo _getFolderPath = typeof(ProjectWindowUtil).GetMethod("GetActiveFolderPath", BindingFlags.Static | BindingFlags.NonPublic);

        private static Texture2D _windowIcon;
        public static Texture2D WindowIcon
        {
            get
            {
                if (_windowIcon == null)
                {
                    _windowIcon = Resources.Load<Texture2D>("Gork Textures/gork_window_icon");
                }

                return _windowIcon;
            }
        }

        [MenuItem("Window/Gork Graph Editor")]
        public static GorkGraphEditor Open()
        {
            // Loop through every window open and find the window that already has our graph (or none at all)
            foreach (GorkGraphEditor editorWindow in Resources.FindObjectsOfTypeAll<GorkGraphEditor>())
            {
                if (editorWindow.graph == null)
                {
                    editorWindow.Focus();
                    return editorWindow;
                }
            }

            GorkGraphEditor window = CreateInstance<GorkGraphEditor>();
            window.titleContent = new GUIContent(DEFAULT_WINDOW_NAME, WindowIcon);
            window.Show();

            return window;
        }

        public static void Open(GorkGraph graph)
        {
            // Loop through every window open and find the window that already has our graph (or none at all)
            foreach (GorkGraphEditor editorWindow in Resources.FindObjectsOfTypeAll<GorkGraphEditor>())
            {
                if (editorWindow.graph == graph)
                {
                    editorWindow.Focus();
                    return;
                }
                else if (editorWindow.graph == null)
                {
                    editorWindow.Focus();
                    editorWindow.OpenGraph(graph, true);
                    return;
                }
            }

            GorkGraphEditor window = Open();
            //window.titleContent.text = graph.name;
            window.OpenGraph(graph, true);
        }

        public void CreateGUI()
        {
            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;

            root.RegisterCallback<DragEnterEvent>(DragAndDropEnter);
            root.RegisterCallback<DragLeaveEvent>(DragAndDropLeave);
            root.RegisterCallback<DragExitedEvent>(DragAndDropExit);
            root.RegisterCallback<DragUpdatedEvent>(DragAndDropUpdated);

            // Import UXML
            if (_gorkGraphViewVisualTree == null)
            {
                _gorkGraphViewVisualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(GetPath("uxml"));
            }
            _gorkGraphViewVisualTree.CloneTree(root);

            if (_gorkGraphViewStyleSheet == null)
            {
                _gorkGraphViewStyleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(GetPath("uss"));
            }
            root.styleSheets.Add(_gorkGraphViewStyleSheet);

            // Get the graph view
            _graphView = root.Q<GorkGraphView>();

            #region Toolbar Buttons
            // Ping object
            root.Q<ToolbarButton>("HighlightGraph").clicked += () =>
            {
                if (_graphView.Graph == null)
                {
                    return;
                }

                EditorGUIUtility.PingObject(_graphView.Graph);
            };

            // Save as
            root.Q<ToolbarButton>("SaveCopy").clicked += () =>
            {
                GorkGraph currentGraph = _graphView.Graph;

                if (currentGraph == null)
                {
                    return;
                }

                string oldPath = AssetDatabase.GetAssetPath(currentGraph);

                // Get the currently open path in the project window
                string projectPath = _getFolderPath.Invoke(null, new object[0]).ToString();

                // Open file explorer
                string path = EditorUtility.SaveFilePanel($"Save a copy of \"{currentGraph.name}\"", projectPath, currentGraph.name, "asset");

                // Return conditions
                if (string.IsNullOrEmpty(path))
                {
                    return;
                }

                if (!path.EndsWith(".asset"))
                {
                    return;
                }
                 
                // Get the File Name of the path
                string fileName = Path.GetFileName(path);

                // Remove the dataPath part of the path
                path = path.Substring(Application.dataPath.Length - 6);

                // Not valid path (we remove the file name part of the path to check only the folder)
                if (!AssetDatabase.IsValidFolder(path.Substring(0, path.Length - fileName.Length)))
                {
                    Debug.LogWarning("Not a valid file path!");
                    return;
                }

                if (!AssetDatabase.CopyAsset(oldPath, path))
                {
                    Debug.LogWarning($"Failed to copy \"{oldPath}\" to \"{path}\"");
                    return;
                }

                GorkGraph graph = AssetDatabase.LoadMainAssetAtPath(path) as GorkGraph;

                OpenGraph(graph, true);

                // Update selection
                Selection.objects = new UnityEngine.Object[] { graph };
            };

            // Open Asset
            root.Q<ToolbarButton>("OpenGraph").clicked += () =>
            {
                OpenExisting();
            };

            // Open Wiki
            root.Q<ToolbarButton>("OpenWiki").clicked += () =>
            {
                // Open the gork wiki
                GorkWikiWindow.Open();
            };

            _toggleMinimapButton = root.Q<ToolbarButton>("ToggleMinimap");
            _minimapOpenIcon = _toggleMinimapButton.Q<VisualElement>("OpenImage");
            _minimapClosedIcon = _toggleMinimapButton.Q<VisualElement>("ClosedImage");
            _minimapHighlight = _toggleMinimapButton.Q<VisualElement>("Highlight");

            void UpdateMinimapButtonStateLocal(GeometryChangedEvent evt)
            {
                UpdateMinimapButtonState();

                _graphView.UnregisterCallback<GeometryChangedEvent>(UpdateMinimapButtonStateLocal);
            }

            _graphView.RegisterCallback<GeometryChangedEvent>(UpdateMinimapButtonStateLocal);

            // Toggle Minimap
            _toggleMinimapButton.clicked += _graphView.ToggleMiniMap;
            #endregion

            #region No Graph State
            // Disable the parent of the Graph
            _parent = root.Q<VisualElement>("Parent");
            _parent.SetEnabled(false);

            // Get the No Graph Text
            _noGraphText = root.Q<Label>("NoGraphText");

            // Create New Button
            _noGraphText.Q<Button>("CreateNewButton").clicked += () =>
            {
                // Get the currently open path in the project window
                string projectPath = _getFolderPath.Invoke(null, new object[0]).ToString();

                // Open file explorer
                string path = EditorUtility.SaveFilePanel("Save a Gork Graph", projectPath, "Gork Graph", "asset");

                // Return conditions
                if (string.IsNullOrEmpty(path))
                {
                    return;
                }

                if (!path.EndsWith(".asset"))
                {
                    return;
                }

                // Get the File Name of the path
                string fileName = Path.GetFileName(path);

                // Remove the dataPath part of the path
                path = path.Substring(Application.dataPath.Length - 6);

                // Not valid path (we remove the file name part of the path to check only the folder)
                if (!AssetDatabase.IsValidFolder(path.Substring(0, path.Length - fileName.Length)))
                {
                    Debug.LogWarning("Not a valid file path!");
                    return;
                }

                // Create and open the Gork Graph
                GorkGraph graph = CreateInstance<GorkGraph>();

                AssetDatabase.CreateAsset(graph, path);
                OpenGraph(graph, true);

                // Update selection
                Selection.objects = new UnityEngine.Object[] { graph };
            };

            // Open Existing Button
            _noGraphText.Q<Button>("OpenExistingButton").clicked += () =>
            {
                OpenExisting();
            };
            #endregion

            #region Split View
            _splitView = root.Q<GorkSplitView>();

            _splitView.IsMinimized = splitViewMinimized;
            _splitView.IsMaximized = splitViewMaximized;

            _splitView.StartSize = _splitView.fixedPaneInitialDimension;
            _splitView.fixedPaneInitialDimension = GorkEditorSaveData.SplitViewWidth;

            UpdateAllSplitWidth += _splitView.SetSize;
            UpdateAllSplitWidth += UpdateMinimapsOnChangeSplitWidth;

            // Extremely odd method for saving the split view width, but it works so...
            void SplitViewStartSaving(GeometryChangedEvent evt)
            {
                _splitView.fixedPane.generateVisualContent += _ =>
                {
                    if (_splitView.IsMinimized || _splitView.IsMaximized)
                    {
                        return;
                    }

                    GorkEditorSaveData.SplitViewWidth = _splitView.fixedPane.style.width.value.value;
                    UpdateAllSplitWidth?.Invoke(GorkEditorSaveData.SplitViewWidth);
                };

                _splitView.UnregisterCallback<GeometryChangedEvent>(SplitViewStartSaving);
            }
            _splitView.RegisterCallback<GeometryChangedEvent>(SplitViewStartSaving);
            #endregion

            _inspectorView = root.Q<GorkInspectorView>();

            _inspectorView.Mode = inspectorMode;
            _inspectorView.OnChangeInspectorMode += mode => inspectorMode = mode;

            _inspectorView.Initialize(root);
            _inspectorView.GraphView = _graphView;
            _graphView.OnOpenGraph += _inspectorView.OnOpenGraph;
            
            // Currently editing text
            _currentlyEditingText = root.Q<Label>("CurrentlyEditingText");
            _currentlyEditingDefaultText = _currentlyEditingText.text;

            // Set visual element references on the Graph View
            _graphView.Inspector = _inspectorView;

            Undo.undoRedoPerformed += _inspectorView.OnUndoRedo;
            Undo.undoRedoPerformed += _graphView.OnUndoRedo;

            // Subscribe the update minimap event
            UpdateAllMinimaps += _graphView.UpdateMinimap;
            UpdateAllMinimaps += UpdateMinimapButtonState;

            if (graph == null)
            {
                _noGraphText.visible = true;
                return;
            }

            OpenGraph(graph, false);

            _graphView.ScrollPosition = scrollPosition;
            _graphView.ZoomScale = zoomScale;
        }

        private void OnDisable()
        {
            // Unsubcribe from everything, with null checks for no errors!

            if (_inspectorView != null)
            {
                Undo.undoRedoPerformed -= _inspectorView.OnUndoRedo;
            }

            if (_graphView != null)
            {
                Undo.undoRedoPerformed -= _graphView.OnUndoRedo;

                UpdateAllMinimaps -= _graphView.UpdateMinimap;

                // Also save position
                scrollPosition = _graphView.ScrollPosition;
                zoomScale = _graphView.ZoomScale;
            }

            if (_splitView != null)
            {
                UpdateAllSplitWidth -= _splitView.SetSize;

                splitViewMinimized = _splitView.IsMinimized;
                splitViewMaximized = _splitView.IsMaximized;
            }

            UpdateAllSplitWidth -= UpdateMinimapsOnChangeSplitWidth;
            UpdateAllMinimaps -= UpdateMinimapButtonState;
        }

        private void UpdateMinimapsOnChangeSplitWidth(float width)
        {
            UpdateAllMinimaps?.Invoke();
        }

        #region TODO: Drag and Drop Support
        private void DragAndDropEnter(DragEnterEvent evt)
        {

        }

        private void DragAndDropLeave(DragLeaveEvent evt)
        {

        }

        private void DragAndDropExit(DragExitedEvent evt)
        {

        }

        private void DragAndDropUpdated(DragUpdatedEvent evt)
        {

        }
        #endregion

        private void UpdateMinimapButtonState()
        {
            if (_graphView.MiniMap == null)
            {
                return;
            }

            bool minimapVisible = _graphView.MiniMap.visible;

            _minimapOpenIcon.visible = minimapVisible;
            _minimapClosedIcon.visible = !minimapVisible;

            _minimapHighlight.visible = minimapVisible;
        }

        private void OpenExisting()
        {
            #region Old File Explorer Method
            /*
            // Get the currently open path in the project window
            string projectPath = _getFolderPath.Invoke(null, new object[0]).ToString();

            // Open file explorer
            string path = EditorUtility.OpenFilePanel("Select a Gork Graph to open", projectPath, "asset");

            // Return conditions
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            if (!File.Exists(path))
            {
                return;
            }

            if (!path.EndsWith(".asset"))
            {
                return;
            }

            // Get the File Name of the path
            string fileName = Path.GetFileName(path);

            // Remove the dataPath part of the path
            path = path.Substring(Application.dataPath.Length - 6);

            // Not valid path (we remove the file name part of the path to check only the folder)
            if (!AssetDatabase.IsValidFolder(path.Substring(0, path.Length - fileName.Length)))
            {
                Debug.LogWarning("Not a valid file path!");
                return;
            }

            // Load and open the Gork Graph
            GorkGraph graph = AssetDatabase.LoadAssetAtPath<GorkGraph>(path);
            */
            #endregion

            GorkAssetSearchWindow window = GorkAssetSearchWindow.Instance;

            window.OnSelectGorkEntry = graph =>
            {
                if (graph == null)
                {
                    return;
                }

                // Update selection
                Selection.objects = new UnityEngine.Object[] { graph };

                // Loop through every window open and find the window that already has our graph (or none at all)
                foreach (GorkGraphEditor editorWindow in Resources.FindObjectsOfTypeAll<GorkGraphEditor>())
                {
                    if (editorWindow.graph == graph)
                    {
                        editorWindow.Focus();
                        Debug.LogWarning("This graph is already open in another window! Only one window can have the graph open at a time.");
                        return;
                    }
                }

                OpenGraph(graph, true);
            };

            SearchWindow.Open(new SearchWindowContext(GUIUtility.GUIToScreenPoint(Event.current.mousePosition)), window);
        }

        private void OpenGraph(GorkGraph graph, bool doFrameAll)
        {
            if (graph == null)
            {
                return;
            }

            titleContent.text = graph.name;
            
            this.graph = graph;

            _graphView.OpenGraph(graph, doFrameAll);

            _currentlyEditingText.text = $"Currently Editing: {graph.name}";

            // Enable the parent of the graph if the parent is currently disabled
            if (!_parent.enabledSelf)
            {
                _parent.SetEnabled(true);

                // Also hide the no graph text (which is the parent to the "Create New" & "Open Existing" buttons)
                _noGraphText.visible = false;
            }
        }

        public void DeletedGraph(GorkGraph graph)
        {
            if (_graphView.Graph != graph)
            {
                return;
            }

            _currentlyEditingText.text = _currentlyEditingDefaultText;

            _graphView.RemoveAllElements();

            _parent.SetEnabled(false);

            _noGraphText.visible = true;

            titleContent.text = DEFAULT_WINDOW_NAME;
        }
    }
}