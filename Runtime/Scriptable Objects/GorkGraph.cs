using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Gork
{
    /// <summary>
    /// The main data container for Gork. A <see cref="ScriptableObject"/> which contains many <see cref="GorkNode"/>s.
    /// </summary>
    [CreateAssetMenu(fileName = "New Gork Graph", menuName = "Gork Graph", order = 150)]
    public class GorkGraph : ScriptableObject
    {
        public List<GorkNode> Nodes = new List<GorkNode>();

        #region Group Data
#if UNITY_EDITOR
        public List<GroupData> GorkGroups = new List<GroupData>();

        [Serializable]
        public class GroupData
        {
            public string Name;
            public Vector2 Pos;

            [SerializeReference]
            public List<GorkNode> Nodes = new List<GorkNode>();

            public GroupData(string name, Vector2 pos)
            {
                Name = name;
                Pos = pos;
            }
        }
#endif
        #endregion

        /// <summary>
        /// Creates a <see cref="GorkNode"/> on this <see cref="GorkGraph"/> and returns it.
        /// </summary>
        public GorkNode CreateNode(Type nodeType)
        {
            // Create a new node
            GorkNode node = CreateInstance(nodeType) as GorkNode;

            // Name it after it's type
            node.name = nodeType.Name;

#if UNITY_EDITOR
            // Generate new unique GUID
            node.GUID = GUID.Generate().ToString();

            // Register undo
            int group = Undo.GetCurrentGroup();

            string displayName = GorkNodeInfoAttribute.TypeAttributes.ContainsKey(nodeType) ? GorkNodeInfoAttribute.TypeAttributes[nodeType].NodeName : nodeType.Name;
            Undo.SetCurrentGroupName($"Created {displayName}");

            Undo.RegisterCreatedObjectUndo(node, "Create node asset");

            // Add the node as a child of this asset
            AssetDatabase.AddObjectToAsset(node, this);
            AssetDatabase.SaveAssets();

            Undo.RecordObject(this, "Add node to list");
#endif

            // Add node to list
            Nodes.Add(node);

#if UNITY_EDITOR
            Undo.CollapseUndoOperations(group);
#endif

            return node;
        }

        public void DeleteNode(GorkNode node)
        {
#if UNITY_EDITOR
            int group = Undo.GetCurrentGroup();

            Type nodeType = node.GetType();
            string displayName = GorkNodeInfoAttribute.TypeAttributes.ContainsKey(nodeType) ? GorkNodeInfoAttribute.TypeAttributes[nodeType].NodeName : nodeType.Name;
            Undo.SetCurrentGroupName($"Deleted {displayName}");

            Undo.RecordObject(this, "Removed node from list");
#endif
            // Remove node from list
            Nodes.Remove(node);

#if UNITY_EDITOR
            // Remove the node from the asset
            Undo.DestroyObjectImmediate(node);

            //AssetDatabase.RemoveObjectFromAsset(node); // This one doesn't have undo :(

            AssetDatabase.SaveAssets();

            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(this), ImportAssetOptions.ForceUpdate);

            Undo.CollapseUndoOperations(group);
#endif
        }

        public void AddConnection(GorkNode parent, int parentPort, GorkNode child, int childPort)
        {
            // Get the list
            List<GorkNode.Connection> list = parent.GetConnections(parentPort);

#if UNITY_EDITOR
            Undo.RecordObject(parent, $"Added connection(s) to {parent.name}");
#endif
            // Add the connection to the list
            list.Add(new GorkNode.Connection(childPort, child));
        }

        public void RemoveConnection(GorkNode parent, int parentPort, GorkNode child, int childPort)
        {
            // Get the list
            List<GorkNode.Connection> list = parent.GetConnections(parentPort);

#if UNITY_EDITOR
            Undo.RecordObject(parent, $"Removed connection(s) from {parent.name}");
#endif

            // Remove the connection from the list
            list.Remove(new GorkNode.Connection(childPort, child));
        }
    }
}
