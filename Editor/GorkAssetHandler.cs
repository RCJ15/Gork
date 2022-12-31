using System;
using Object = UnityEngine.Object;
using UnityEditor;
using UnityEditor.Callbacks;


namespace Gork.Editor
{
    /// <summary>
    /// Special editor script for Gork that handles the opening and deleting of a <see cref="GorkGraph"/>.
    /// </summary>
    public class GorkAssetHandler : AssetModificationProcessor
    {
        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceID, int line)
        {
            // Load object using instance ID
            string path = AssetDatabase.GetAssetPath(instanceID);

            // Check if the path is a GWP file
            if (path.ToLower().EndsWith(".gwp"))
            {
                // If it is, then open the file up in the Gork Wiki
                GorkWikiWindow.OpenGWPFile(path);
                return true;
            }
            
            // If the file is not a GWP file, then we check if the file is a gork graph file
            Object obj = AssetDatabase.LoadMainAssetAtPath(path);

            // Get the object type
            Type objType = obj.GetType();

            // Check if the loaded object is of type gork graph
            if (objType != typeof(GorkGraph))
            {
                // Is not gork graph
                return false;
            }
            // It is gork graph!!

            // Open the gork graph in the Gork Graph Editor
            GorkGraphEditor.Open(obj as GorkGraph);

            return true;
        }

        public static AssetDeleteResult OnWillDeleteAsset(string path, RemoveAssetOptions opt)
        {
            GorkGraph graph = AssetDatabase.LoadMainAssetAtPath(path) as GorkGraph;

            if (graph != null && EditorWindow.HasOpenInstances<GorkGraphEditor>())
            {
                EditorWindow.GetWindow<GorkGraphEditor>().DeletedGraph(graph);
            }

            return AssetDeleteResult.DidNotDelete;
        }
    }
}
