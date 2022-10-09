using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using Object = UnityEngine.Object;
using UnityEditor;
using UnityEditor.Callbacks;


namespace Gork.Editor
{
    /// <summary>
    /// Special editor script for Gork that handles the opening and deleting of a <see cref="GorkGraph"/>.
    /// </summary>
    public class AssetHandler : AssetModificationProcessor
    {
        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceID, int line)
        {
            // Load object using instance ID
            string path = AssetDatabase.GetAssetPath(instanceID);
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
