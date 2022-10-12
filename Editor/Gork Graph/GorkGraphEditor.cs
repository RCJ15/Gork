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
    /// 
    /// </summary>
    public class GorkGraphEditor : EditorWindow
    {
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

                foreach (var guid in AssetDatabase.FindAssets($"{nameof(GorkGraphEditor)} t: MonoScript"))
                {
                    _path = AssetDatabase.GUIDToAssetPath(guid);

                    if (!string.IsNullOrEmpty(_path))
                    {
                        break;
                    }
                }

                if (string.IsNullOrEmpty(_path))
                {
                    throw new Exception("The file GorkGraphEditor.cs has been renamed.");
                }

                return _path;
            }
        }
        public static string GetPath(string extension)
        {
            return Path.ChangeExtension(GorkPath, extension);
        }
        #endregion

        private VisualElement _parent;
        private Label _noGraphText;

        private Label _currentlyEditingText;
        private string _currentlyEditingDefaultText;

        private GorkGraph _graph;
        private GorkGraphView _graphView;

        private GorkAssetSearchWindow _searchWindow;

        // Getting current open file path in the project window using System.Reflection
        private MethodInfo _getFolderPath = typeof(ProjectWindowUtil).GetMethod("GetActiveFolderPath", BindingFlags.Static | BindingFlags.NonPublic);

        [MenuItem("Window/Gork Graph Editor")]
        public static GorkGraphEditor Open()
        {
            GorkGraphEditor window = GetWindow<GorkGraphEditor>();
            window.titleContent = new GUIContent("Gork Graph Editor");

            return window;
        }

        public static void Open(GorkGraph graph)
        {
            GorkGraphEditor window = Open();
            window.OpenGraph(graph);
        }

        private void OnEnable()
        {
            _searchWindow = CreateInstance<GorkAssetSearchWindow>();
        }

        private void OnDisable()
        {
            DestroyImmediate(_searchWindow);
        }

        public void CreateGUI()
        {
            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;

            // Import UXML
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(GetPath("uxml"));
            visualTree.CloneTree(root);

            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(GetPath("uss"));
            root.styleSheets.Add(styleSheet);

            // Get our components
            _graphView = root.Q<GorkGraphView>();
            Undo.undoRedoPerformed += _graphView.OnUndoRedo;

            #region Toolbar Buttons
            // Ping object
            root.Q<ToolbarButton>("HighlightAsset").clicked += () =>
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

                OpenGraph(graph);

                // Update selection
                Selection.objects = new UnityEngine.Object[] { graph };
            };

            // Open Asset
            root.Q<ToolbarButton>("OpenAsset").clicked += () =>
            {
                OpenExisting();
            };

            // Toggle Minimap
            root.Q<ToolbarButton>("ToggleMinimap").clicked += () =>
            {
                _graphView.ToggleMiniMap();
            };
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
                OpenGraph(graph);

                // Update selection
                Selection.objects = new UnityEngine.Object[] { graph };
            };

            // Open Existing Button
            _noGraphText.Q<Button>("OpenExistingButton").clicked += () =>
            {
                OpenExisting();
            };
            #endregion

            // Currently editing text
            _currentlyEditingText = root.Q<Label>("CurrentlyEditingText");
            _currentlyEditingDefaultText = _currentlyEditingText.text;

            // Open the currently selected graph TODO: Remove this and replace it with a way to save between sessions
            GorkGraph selectedGraph = Selection.activeObject as GorkGraph;

            if (selectedGraph == null)
            {
                selectedGraph = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GetAssetPath(Selection.activeObject)) as GorkGraph;

                if (selectedGraph == null)
                {
                    return;
                }
            }

            OpenGraph(selectedGraph);
        }

        private void OnDestroy()
        {
            Undo.undoRedoPerformed -= _graphView.OnUndoRedo;
        }

        private void OpenExisting()
        {
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

            _searchWindow.OnSelectGorkEntry = graph =>
            {
                if (graph == null)
                {
                    return;
                }

                OpenGraph(graph);

                // Update selection
                Selection.objects = new UnityEngine.Object[] { graph };
            };

            SearchWindow.Open(new SearchWindowContext(GUIUtility.GUIToScreenPoint(Event.current.mousePosition)), _searchWindow);
        }

        private void OpenGraph(GorkGraph graph)
        {
            _graphView.OpenGraph(graph);

            _currentlyEditingText.text = $"Currently Editing: {graph.name}";

            // Enable the parent of the graph if the parent is currently disabled
            if (!_parent.enabledSelf)
            {
                _parent.SetEnabled(true);

                // Also remove the no graph text (which is the parent to the "Create New" & "Open Existing" buttons)
                rootVisualElement.Remove(_noGraphText);
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

            rootVisualElement.Add(_noGraphText);
        }
    }
}