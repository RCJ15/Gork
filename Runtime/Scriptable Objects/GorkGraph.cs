using System;
using System.Linq;
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
        public static readonly Type SignalType = typeof(SignalClass);
        private class SignalClass { }

        [SerializeField] private List<GorkNode> nodes = new List<GorkNode>();
        public List<GorkNode> Nodes => nodes;

        public delegate void OnNodeCalledEvent(GorkNode node, int portIndex);
        public OnNodeCalledEvent OnNodeCalled;

        public delegate void OnGraphStartedEvent();
        public OnGraphStartedEvent OnGraphStarted;

        public delegate void OnGraphStopEvent();
        public OnGraphStopEvent OnGraphStop;

        public delegate void OnNodeStopEvent(GorkNode node);
        public OnNodeStopEvent OnNodeStop;

#if UNITY_EDITOR
        private SerializedObject _cachedSerializedObject;
        public SerializedObject SerializedObject
        {
            get
            {
                if (_cachedSerializedObject == null)
                {
                    _cachedSerializedObject = new SerializedObject(this);
                }

                return _cachedSerializedObject;
            }
        }
#endif

        #region Gork Node Dictionary
        private Dictionary<Type, List<GorkNode>> _cachedGorkNodeDictionary = null;

        public Dictionary<Type, List<GorkNode>> GorkNodeDicitonary
        {
            get
            {
                if (_cachedGorkNodeDictionary != null)
                {
                    return _cachedGorkNodeDictionary;
                }

                // Dicitonary is null so fill it
                _cachedGorkNodeDictionary = new Dictionary<Type, List<GorkNode>>();

                // Loop through every node and determine what type it is and add it to the dicitonary
                foreach (GorkNode node in nodes)
                {
                    Type type = node.GetType();

                    if (!_cachedGorkNodeDictionary.ContainsKey(type))
                    {
                        _cachedGorkNodeDictionary[type] = new List<GorkNode>();
                    }

                    _cachedGorkNodeDictionary[type].Add(node);
                }

                return _cachedGorkNodeDictionary;
            }
        }

        private readonly Dictionary<Type, object> _cachedCastedGorkNodes = new Dictionary<Type, object>();

        public List<T> GetAllNodesOfType<T>() where T : GorkNode
        {
            Type type = typeof(T);

            if (_cachedCastedGorkNodes.ContainsKey(type))
            {
                return (List<T>)_cachedCastedGorkNodes[type];
            }

            List<GorkNode> nodes = GetAllNodesOfType(type);

            if (nodes == null)
            {
                return null;
            }

            List<T> list = nodes.Cast<T>().ToList();

            _cachedCastedGorkNodes.Add(type, list);

            return list;
        }
        public List<GorkNode> GetAllNodesOfType(Type nodeType)
        {
            if (GorkNodeDicitonary.ContainsKey(nodeType))
            {
                return GorkNodeDicitonary[nodeType];
            }

            return null;
        }
        #endregion

        #region Group Data
#if UNITY_EDITOR
        [SerializeField] private List<GroupData> gorkGroups = new List<GroupData>();
        public List<GroupData> GorkGroups => gorkGroups;

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
            foreach (GroupData group in gorkGroups)
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
            nodes.Add(node);

            return node;
        }

        public void DeleteNode(GorkNode node)
        {
            // Remove node from list
            nodes.Remove(node);
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

        #region Parameters
        [SerializeField] private List<Parameter> _parameters = new List<Parameter>();
        public List<Parameter> Parameters => _parameters;

        private Dictionary<Type, Dictionary<string, object>> _parameterStartValues = null;
        private Dictionary<Type, Dictionary<string, object>> _cachedRuntimeParameters = null;
        private Dictionary<Type, Dictionary<string, object>> RuntimeParameters
        {
            get
            {
                // Create and cache the RuntimeParameters dictionary if it's null
                if (_cachedRuntimeParameters == null)
                {
                    _parameterStartValues = new Dictionary<Type, Dictionary<string, object>>();
                    _cachedRuntimeParameters = new Dictionary<Type, Dictionary<string, object>>();

                    // Loop through all parameters in the GorkGraph
                    foreach (Parameter parameter in Parameters)
                    {
                        // Cache the type and name of the parameter
                        Type parameterType = parameter.Type;
                        string parameterName = parameter.Name;

                        // Create the dictionary for this parameter type if it doesn't already exist
                        if (!_cachedRuntimeParameters.ContainsKey(parameterType))
                        {
                            _cachedRuntimeParameters[parameterType] = new Dictionary<string, object>();
                            _parameterStartValues[parameterType] = new Dictionary<string, object>();
                        }

                        // Prevent multiple of the same parameter from being added
                        if (_cachedRuntimeParameters[parameterType].ContainsKey(parameterName))
                        {
#if UNITY_EDITOR
                            Debug.LogWarning($"Gork Graph \"{name}\" has two parameters with the name \"{parameterName}\" and with the same type \"{parameterType}\"!");
#endif
                            continue;
                        }

                        object value;

                        // Parsing with string doesn't require JSON deserialization
                        if (parameterType == typeof(string))
                        {
                            value = parameter.Value;
                        }
                        // Deserialize the JSON value
                        else
                        {
                            value = GorkUtility.FromJson(parameter.Value, parameterType);
                        }

                        // Set the parameter value and start value
                        _cachedRuntimeParameters[parameterType][parameterName] = value;
                        _parameterStartValues[parameterType][parameterName] = value;
                    }
                }

                return _cachedRuntimeParameters;
            }
        }

        #region Has Parameter
        /// <summary>
        /// Returns if a parameter with the given <paramref name="name"/> and <paramref name="type"/> exists or not.
        /// </summary>
        public bool HasParameter(string name, Type type)
        {
            if (!RuntimeParameters.ContainsKey(type))
            {
                return false;
            }

            if (!RuntimeParameters[type].ContainsKey(name))
            {
                return false;
            }

            return true;
        }
        /// <summary>
        /// Returns if a parameter with the given <paramref name="name"/> and <paramref name="T"/> exists or not.
        /// </summary>
        public bool HasParameter<T>(string name) => HasParameter(name, typeof(T));

        /// <summary>
        /// Returns if a <see cref="float"/> parameter with the given <paramref name="name"/> exists or not.
        /// </summary>
        public bool HasFloat(string name) => HasParameter<float>(name);
        /// <summary>
        /// Returns if a <see cref="int"/> parameter with the given <paramref name="name"/> exists or not.
        /// </summary>
        public bool HasInt(string name) => HasParameter<int>(name);
        /// <summary>
        /// Returns if a <see cref="bool"/> parameter with the given <paramref name="name"/> exists or not.
        /// </summary>
        public bool HasBool(string name) => HasParameter<bool>(name);
        /// <summary>
        /// Returns if a <see cref="string"/> parameter with the given <paramref name="name"/> exists or not.
        /// </summary>
        public bool HasString(string name) => HasParameter<string>(name);
        #endregion

        #region Get Parameter
        /// <summary>
        /// Returns the parameter with the type of <paramref name="type"/> and with the given <paramref name="name"/>.
        /// </summary>
        public object GetParameter(string name, Type type)
        {
            if (!HasParameter(name, type))
            {
#if UNITY_EDITOR
                Debug.LogWarning($"Gork Graph \"{name}\" does not contain any parameters that have the name \"{name}\" and type \"{type}\"!");
#endif
                return default;
            }

            // Return the value
            return RuntimeParameters[type][name];
        }
        /// <summary>
        /// Returns the parameter with the type of <paramref name="T"/> and with the given <paramref name="name"/>.
        /// </summary>
        public T GetParameter<T>(string name) => (T)GetParameter(name, typeof(T));

        /// <summary>
        /// Returns the <see cref="float"/> parameter with the given <paramref name="name"/>.
        /// </summary>
        public float GetFloat(string name) => GetParameter<float>(name);
        /// <summary>
        /// Returns the <see cref="int"/> parameter with the given <paramref name="name"/>.
        /// </summary>
        public int GetInt(string name) => GetParameter<int>(name);
        /// <summary>
        /// Returns the <see cref="bool"/> parameter with the given <paramref name="name"/>.
        /// </summary>
        public bool GetBool(string name) => GetParameter<bool>(name);
        /// <summary>
        /// Returns the <see cref="string"/> parameter with the given <paramref name="name"/>.
        /// </summary>
        public string GetString(string name) => GetParameter<string>(name);
        #endregion

        #region Set Parameter
        /// <summary>
        /// Sets the parameter with the type of <paramref name="type"/> and with the given <paramref name="name"/> to the given <paramref name="value"/>.
        /// </summary>
        public void SetParameter(string name, Type type, object value)
        {
            if (!HasParameter(name, type))
            {
#if UNITY_EDITOR
                Debug.LogWarning($"Gork Graph \"{name}\" does not contain any parameters that have the name \"{name}\" and type \"{type}\"!");
#endif
                return;
            }

            // Set the value
            RuntimeParameters[type][name] = value;
        }
        /// <summary>
        /// Sets the parameter with the type of <paramref name="T"/> and with the given <paramref name="name"/> to the given <paramref name="value"/>.
        /// </summary>
        public void SetParameter<T>(string name, T value) => SetParameter(name, typeof(T), value);

        /// <summary>
        /// Sets the <see cref="float"/> parameter with the given <paramref name="name"/> to the given <paramref name="value"/>.
        /// </summary>
        public void SetFloat(string name, float value) => SetParameter(name, value);
        /// <summary>
        /// Sets the <see cref="int"/> parameter with the given <paramref name="name"/> to the given <paramref name="value"/>.
        /// </summary>
        public void SetInt(string name, int value) => SetParameter(name, value);
        /// <summary>
        /// Sets the <see cref="bool"/> parameter with the given <paramref name="name"/> to the given <paramref name="value"/>.
        /// </summary>
        public void SetBool(string name, bool value) => SetParameter(name, value);
        /// <summary>
        /// Sets the <see cref="string"/> parameter with the given <paramref name="name"/> to the given <paramref name="value"/>.
        /// </summary>
        public void SetString(string name, string value) => SetParameter(name, value);
        #endregion

        #region Reset Parameter
        /// <summary>
        /// Resets the parameter with the type of <paramref name="type"/> and with the given <paramref name="name"/> to it's starting value set in the GorkEditorWindow.
        /// </summary>
        public void ResetParameter(string name, Type type)
        {
            if (!HasParameter(name, type))
            {
#if UNITY_EDITOR
                Debug.LogWarning($"Gork Graph \"{name}\" does not contain any parameters that have the name \"{name}\" and type \"{type}\"!");
#endif
                return;
            }

            // Set to start value
            RuntimeParameters[type][name] = _parameterStartValues[type][name];
        }
        /// <summary>
        /// Resets the parameter with the type of <paramref name="T"/> and with the given <paramref name="name"/> to it's starting value set in the GorkEditorWindow.
        /// </summary>
        public void ResetParameter<T>(string name) => ResetParameter(name, typeof(T));

        /// <summary>
        /// Resets the <see cref="float"/> parameter the given <paramref name="name"/> to it's starting value set in the GorkEditorWindow.
        /// </summary>
        public void ResetFloat(string name) => ResetParameter<float>(name);
        /// <summary>
        /// Resets the <see cref="int"/> parameter the given <paramref name="name"/> to it's starting value set in the GorkEditorWindow.
        /// </summary>
        public void ResetInt(string name) => ResetParameter<int>(name);
        /// <summary>
        /// Resets the <see cref="bool"/> parameter the given <paramref name="name"/> to it's starting value set in the GorkEditorWindow.
        /// </summary>
        public void ResetBool(string name) => ResetParameter<bool>(name);
        /// <summary>
        /// Resets the <see cref="string"/> parameter the given <paramref name="name"/> to it's starting value set in the GorkEditorWindow.
        /// </summary>
        public void ResetString(string name) => ResetParameter<string>(name);

        /// <summary>
        /// Resets all parameters in this <see cref="GorkGraph"/> back to their starting values set in the GorkEditorWindow.
        /// </summary>
        public void ResetAllParamters()
        {
            foreach (var pair1 in RuntimeParameters)
            {
                foreach (var pair2 in pair1.Value)
                {
                    RuntimeParameters[pair1.Key][pair2.Key] = _parameterStartValues[pair1.Key][pair2.Key];
                }
            }
        }
        #endregion

        /// <summary>
        /// Class that contains data for a single parameter in a <see cref="GorkGraph"/>.
        /// </summary>
        [Serializable]
        public class Parameter : ISerializationCallbackReceiver
        {
            public string Name;
            public Type Type;
            [SerializeField] private string SerializedType;

            public string Value;

            public void OnBeforeSerialize()
            {
                SerializedType = Type == null ? "" : Type.AssemblyQualifiedName;
            }

            public void OnAfterDeserialize()
            {
                if (string.IsNullOrEmpty(SerializedType))
                {
                    Type = null;
                    return;
                }

                Type = Type.GetType(SerializedType);
            }
        }
        #endregion

        #region Tags
        [SerializeField] private List<string> _tags = new List<string>();
        public List<string> Tags => _tags;

        private Dictionary<string, List<GorkNode>> _nodeTagCache = null;
        private Dictionary<string, List<GorkNode>> NodeTagCache
        {
            get
            {
                if (_nodeTagCache == null)
                {
                    _nodeTagCache = new Dictionary<string, List<GorkNode>>();

                    foreach (GorkNode node in Nodes)
                    {
                        foreach (string tag in node.Tags)
                        {
                            if (!_nodeTagCache.ContainsKey(tag))
                            {
                                _nodeTagCache[tag] = new List<GorkNode>();
                            }

                            _nodeTagCache[tag].Add(node);
                        }
                    }
                }

                return _nodeTagCache;
            }
        }

        /// <summary>
        /// Returns a list of all the nodes that have the given <paramref name="tag"/>. Will return null if there are no nodes with the given <paramref name="tag"/>.
        /// </summary>
        public List<GorkNode> GetNodesWithTag(string tag)
        {
            if (NodeTagCache.TryGetValue(tag, out List<GorkNode> list))
            {
                return list;
            }

#if UNITY_EDITOR
            Debug.LogWarning($"Gork Graph \"{name}\" does not contain any nodes that have the tag \"{tag}\"!");
#endif

            return null;
        }
        #endregion

        #region Events
        public delegate void OnCallExternalEvent(string eventName, object parameter);
        public OnCallExternalEvent OnCallExternal;

        private Dictionary<string, List<EventNode>> _internalEventNodesCache = null;

        private Dictionary<string, List<EventNode>> InternalEventNodesCache
        {
            get
            {
                if (_internalEventNodesCache == null)
                {
                    _internalEventNodesCache = new Dictionary<string, List<EventNode>>();

                    foreach (EventNode node in GetAllNodesOfType<EventNode>())
                    {
                        if (!node.IsInternal)
                        {
                            continue;
                        }

                        string eventName = node.EventName;

                        if (!_internalEventNodesCache.ContainsKey(eventName))
                        {
                            _internalEventNodesCache[eventName] = new List<EventNode>();
                        }

                        _internalEventNodesCache[eventName].Add(node);
                    }
                }

                return _internalEventNodesCache;
            }
        }

        /// <summary>
        /// Triggers all internal <see cref="EventNode"/> inside of this GorkGraph with the given <paramref name="eventName"/>.
        /// </summary>
        public void TriggerInternalEvent(string eventName)
        {
            if (!InternalEventNodesCache.TryGetValue(eventName, out List<EventNode> list))
            {
                return;
            }

            foreach (EventNode node in list)
            {
                OnNodeCalled.Invoke(node, 0);
            }
        }

        [SerializeField] private List<Event> _events = new List<Event>();
        public List<Event> Events => _events;

        [Serializable]
        public class Event
        {
            public string Name;
            public Type EventType;

            public enum Type
            {
                External,
                Internal,
            }
        }
        #endregion
    }
}
