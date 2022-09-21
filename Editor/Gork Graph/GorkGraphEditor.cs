using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Gork.Editor
{
    /// <summary>
    /// 
    /// </summary>
    public class GorkGraphEditor : EditorWindow
    {
        #region GetPath
        private static string _path = null;
        public static string Path
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
                    throw new System.Exception("The file GorkGraphEditor.cs has been renamed.");
                }

                return _path;
            }
        }
        public static string GetPath(string extension)
        {
            return System.IO.Path.ChangeExtension(Path, extension);
        }
#endregion

        private GorkGraph _graph;
        private GorkGraphView _graphView;

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

            // Open the currently selected graph TODO: Remove this and replace it with a way to save between sessions
            GorkGraph selectedGraph = Selection.activeObject as GorkGraph;

            if (selectedGraph == null)
            {
                return;
            }

            OpenGraph(selectedGraph);
        }

        private void OnDestroy()
        {
            Undo.undoRedoPerformed -= _graphView.OnUndoRedo;
        }

        private void OpenGraph(GorkGraph graph)
        {
            _graphView.OpenGraph(graph);
        }
    }
}