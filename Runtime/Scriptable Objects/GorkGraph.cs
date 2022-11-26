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
    public class GorkGraph : ScriptableObject,
#if UNITY_EDITOR
        ISerializationCallbackReceiver
#endif
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

        public HashSet<GorkNode> NodesInGroups = new HashSet<GorkNode>();
        public Dictionary<GorkNode, GroupData> GetNodeGroup = new Dictionary<GorkNode, GroupData>();

        public void OnBeforeSerialize()
        {

        }

        public void OnAfterDeserialize()
        {
            foreach (GroupData group in GorkGroups)
            {
                foreach (GorkNode node in group.Nodes)
                {
                    // Ignore if any of the 2 values are null
                    if (node == null || group == null)
                    {
                        continue;
                    }

                    GetNodeGroup[node] = group;

                    if (NodesInGroups.Contains(node))
                    {
                        continue;
                    }

                    NodesInGroups.Add(node);
                }
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

            Undo.RegisterCreatedObjectUndo(node, "Create node asset");

            // Add the node as a child of this asset
            AssetDatabase.AddObjectToAsset(node, this);
#endif

            // Add node to list
            Nodes.Add(node);

            return node;
        }

        public void DeleteNode(GorkNode node)
        {
            // Remove node from list
            Nodes.Remove(node);
        }

        public void AddConnection(GorkNode parent, int parentPort, GorkNode child, int childPort)
        {
            // Get the list
            List<GorkNode.Connection> parentList = parent.GetOutputConnections(parentPort);
            List<GorkNode.Connection> childList = child.GetInputConnections(childPort);

#if UNITY_EDITOR
            Undo.RecordObject(child, $"Added connection to \"{child.name}\"");
            Undo.RecordObject(parent, $"Added connection to \"{parent.name}\"");
#endif
            // Add the connection to the list
            parentList.Add(new GorkNode.Connection(childPort, child));
            childList.Add(new GorkNode.Connection(parentPort, parent));
        }

        public void RemoveConnection(GorkNode parent, int parentPort, GorkNode child, int childPort)
        {
            // Get the list
            List<GorkNode.Connection> parentList = parent.GetOutputConnections(parentPort);
            List<GorkNode.Connection> childList = child.GetInputConnections(childPort);

#if UNITY_EDITOR
            if (child != null)
            {
                Undo.RecordObject(child, $"Removed connection from \"{child.name}\"");
            }

            if (parent != null)
            {
                Undo.RecordObject(parent, $"Removed connection from \"{parent.name}\"");
            }
#endif

            // Remove the connections from the lists
            parentList.Remove(new GorkNode.Connection(childPort, child));
            childList.Remove(new GorkNode.Connection(parentPort, parent));
        }
    }
}
