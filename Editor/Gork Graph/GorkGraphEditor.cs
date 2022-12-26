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
                    throw new Exception("The file GorkGraphEditor.cs has been renamed.");
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

        private VisualElement _parent;
        private Label _noGraphText;

        private Label _currentlyEditingText;
        private string _currentlyEditingDefaultText;

        private GorkGraph _graph;
        private GorkGraphView _graphView;
        private GorkInspectorView _inspectorView;

        private static VisualTreeAsset _gorkGraphViewVisualTree = null;
        private static StyleSheet _gorkGraphViewStyleSheet = null;

        private const string EYEBALL_OPEN_BASE64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsIAAA7CARUoSoAAAALfSURBVFhH7ZRLaFNBFIZvXhYTxAha0F0FKRIRi6UNaXQpCIILBauWoiiC0khRURD6cmcXvhBFTVdGcKVVcOvGNLUhBESKdOFCN2o3itSGPJr4nbmTa5qmvdl7fzj85zVzzj13ZgwHDhz893BptkU2m92Sy+X2VyqVqMvliuBqRQ/Afuyf8JyW98Fg8GUoFFpAt4VtA1NTU3ugy8gxCvqU0wY0tEjupNfrfRgOh5Pa3RCrNpDJZLbm8/nHbHRIbDYtoku+V+w1UCKXVLNZ9ITH47lCIz9UtA4NG+CrD7NBHHUzG/yGbyGf8T2VuB1Y00/uDvgivBHXL2QgGo0+Uwk1cGu2kEqlhlg0iSrFn/t8vvaenp4x7FMqoQmwvp9iwy0tLTsxXyBBJJFMJoclXotlDfDlsXK5fAN1ye12xyh8vKur67vE2HS3cDOgcZXb2dn5jUaOYF9TAcMYo8ao1hWsBgj0QncRKX40EoncV4F/WK/ZFjTr16oCHzJebYLYCJM4qQKgdgKjBOVMDFBcfkE9ZjU3gxW5tU3AQ8oJrAYonhcm2KYcdcA/oVVbrJGrJlOtJaidwBALywSv8jtuap8FphIn/labq0JyJFebFthTbsQIqlzT66a3pgEOy2sCl0SXJrgN47B1TYlVONV9azUhMZ1T0S7Zy6MP3h3xI6f5HW/MKGs0WyD5AovuoXrE5ECe54s+qiCQpmjuLHwGM2R6jVk2ntBTsorPzMy0lUqlBLkR/GVccrMemFETDR+i6enpg1zHOAu3YZaQR7wHT7q7uz+oBBvwEbsgeb5PsMc6+CvSx5TfwcvQsAEBV2UDJG9CDJFpyIhlEq/gT0xmDpkvFAp+2E+h7UiU+D5kr+SDJSQRCAQGOzo65DVcgVUbqCKdToeKxeI51F4KtJpeWyzIL4FvM/IvpqsxbBuoguJeRnsANYy0a9lEoT/EFtHn0VOIjDnNecjBDhw4cGADw/gLI3wpk62HVKwAAAAASUVORK5CYII=";
        private const string EYEBALL_CLOSE_BASE64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsIAAA7CARUoSoAAAAMySURBVFhH7ZY9aFNRFMdfEptWiw5SqYtQQdRBXExDklY3LbooRRG1gl9TbQUdFIq1dnCwoINKJ1vFDxAnrYiLOEiStkgztYODizioqIOCIc2Xv3NzX/pe3ntJh1RB+ofLufec887/fz8TYxn/Gj5tlwTFYtGXSCT2Sr+jo+OVz+crqoAFfm3rDiFPJpN36b6UZgqpxJIIMMmxp7TLE3UX4EbO0o/LFuihDXU9A17ksVjsjNv+CxYtIJVKrUun0110Y7R2SFop2qKChvGd8RdsM22r8oBa5IKaAjg8uyh+liIHsEHtronFkAs8BUxOTrbl8/kbdLtlTKEsAiR/hYyrwSSnxnG+6Q4EAn2RSOSTDtvgKoB9PMSH92mrKPYT103aB8YPVUIVmOQzMzMbMpnMLN+slhrY3s7Ozsc6rQzHLYB8uFAoPNXkTxoaGrZwgocJnShleMMkn5qaCodCoY+NjY3b8E1Qaw3hR/F4/EopcwE2ASQMQC5Jeb/f3w/xkXA4/FliFNku1gsmuSw7W/dCfCKCGvuJXVJJhjHMmbqq+wplAcz8KOYaTcgPUuyOCixgpbYOWMkReg+XLRcRI6YI4kNM9JgKgLIAAuby9FHsme5bMaetDZXkNKnpyLWKwA4qJ7AKyIgluFE5KoB/THfL8CB3zRXgD4g1uQRlAWCQhALBi+zTde1TwCe3JVIalVCF/A1++RGygZonyVFbzLW8XPJaBHBFJvj4gvRFBGdiRIileT2vbuSc/B6s7fGBfIiccakF+qPRqDqkAsc7QHIvibfoynLFOZBfuRnqMRJYyKP4n+Nqos3hH5OZW8kRvomcUbq7aXli5zkLt1VQw/Uh4pTuEyKEtGqXgkluJXHD9PT05mw2e47uaVoT+T+YyGFm/lolWOAqQJYK9Q+wPdplnfke+s3s43uK/pqfn5c9bcnlcm2ktdPkB2uHXu4C/TG2ZYA34Rt9BxwCNPmi9twL5MvzLc/uKEs+q5wesAmoRo4tEusithN3CLseuxa/1JDZyc/xO4aJYDD4lhn/ZlwTNgGy9xj5D6dgJdeuusNzKf8GucCxBVzDqn+jl7GM/wyG8QdWAdchDSqEIwAAAABJRU5ErkJggg==";
        private static Texture2D _eyeballOpenTexture, _eyeballCloseTexture = null;

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
            window.OpenGraph(graph, true);
        }

        public void CreateGUI()
        {
            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;

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

            // Get our components
            _graphView = root.Q<GorkGraphView>();
            Undo.undoRedoPerformed += _graphView.OnUndoRedo;

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

            #region Eyeball Icon for Minimap Toggle
            ToolbarButton toggleMinimap = root.Q<ToolbarButton>("ToggleMinimap");
            VisualElement minimapEyeballIcon = toggleMinimap.Q<VisualElement>("Image");
            VisualElement minimapHighlight = toggleMinimap.Q<VisualElement>("Highlight");

            // Cache and load the textures if they have not been loaded yet
            if (_eyeballOpenTexture == null)
            {
                // Load texture from base 64
                _eyeballOpenTexture = GorkEditorUtility.Texture2DFromBase64(EYEBALL_OPEN_BASE64);
            }
            if (_eyeballCloseTexture == null)
            {
                // Load texture from base 64
                _eyeballCloseTexture = GorkEditorUtility.Texture2DFromBase64(EYEBALL_CLOSE_BASE64);
            }
            #endregion

            UpdateMinimapButtonState();

            // Toggle Minimap
            toggleMinimap.clicked += () =>
            {
                _graphView.ToggleMiniMap();

                UpdateMinimapButtonState();
            };

            void UpdateMinimapButtonState()
            {
                bool minimapVisible = _graphView.MiniMap.visible;

                minimapEyeballIcon.style.backgroundImage = minimapVisible ? _eyeballOpenTexture : _eyeballCloseTexture;
                minimapHighlight.visible = minimapVisible;
            }
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
            GorkSplitView splitView = root.Q<GorkSplitView>();
            splitView.fixedPaneInitialDimension = GorkEditorSaveData.InspectorWidth;

            // Extremely odd method for saving the inspector width value
            void SplitViewChangedEvent(GeometryChangedEvent evt)
            {
                splitView.fixedPane.generateVisualContent += _ => GorkEditorSaveData.InspectorWidth = splitView.fixedPane.style.width.value.value;
            }
            splitView.RegisterCallback<GeometryChangedEvent>(SplitViewChangedEvent);
            #endregion

            _inspectorView = root.Q<GorkInspectorView>();
            Undo.undoRedoPerformed += _inspectorView.OnUndoRedo;

            _inspectorView.Initialize(root);
            _inspectorView.GraphView = _graphView;

            // Currently editing text
            _currentlyEditingText = root.Q<Label>("CurrentlyEditingText");
            _currentlyEditingDefaultText = _currentlyEditingText.text;

            _graphView.LoadScrollAndZoomData();

            #region Load Currently Edited Graph
            // Don't load if the graph view somehow already has a loaded graph
            if (_graphView.Graph != null)
            {
                return;
            }

            string currentlyEditingGraph = GorkEditorSaveData.CurrentlyEditingGraph;

            if (string.IsNullOrEmpty(currentlyEditingGraph))
            {
                return;
            }

            // Load asset
            GorkGraph graph = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(currentlyEditingGraph)) as GorkGraph;
            
            // Check if the asset load has failed
            if (graph == null)
            {
                return;
            }

            // Open graph
            OpenGraph(graph, false);
            #endregion
        }

        private void OnDestroy()
        {
            try
            {
                Undo.undoRedoPerformed -= _graphView.OnUndoRedo;
            }
            catch (Exception)
            {
                // What
            }

            try
            {
                Undo.undoRedoPerformed -= _inspectorView.OnUndoRedo;
            }
            catch (Exception)
            {
                // What
            }

            _graphView.SaveScrollAndZoomData();
            _graphView.SaveMinimapData();
            _graphView.SaveCurrentlyEditingGraph();
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

                OpenGraph(graph, true);

                // Update selection
                Selection.objects = new UnityEngine.Object[] { graph };
            };

            SearchWindow.Open(new SearchWindowContext(GUIUtility.GUIToScreenPoint(Event.current.mousePosition)), window);
        }

        private void OpenGraph(GorkGraph graph, bool frameAllGraphElements)
        {
            if (graph == null)
            {
                return;
            }

            _graphView.OpenGraph(graph, frameAllGraphElements);

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