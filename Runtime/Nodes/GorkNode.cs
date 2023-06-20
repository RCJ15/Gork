using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.MemoryProfiler;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Gork
{
    /// <summary>
    /// The base script for every node in <see cref="GorkGraph"/>.
    /// </summary>
    public abstract class GorkNode : ScriptableObject
    {
#if UNITY_EDITOR
        /// <summary>
        /// The Globally Unique ID that this node has in the Graph View. <para/>
        /// NOTE: This is only available in the editor. Usage in builds will result in errors!
        /// </summary>
        public string GUID { get => _guid; set => _guid = value; }
        [DontSaveInGorkGraph] [HideInInspector] [SerializeField] private string _guid;

        /// <summary>
        /// The position of the node in the Graph View. <para/>
        /// NOTE: This is only available in the editor. Usage in builds will result in errors!
        /// </summary>
        public Vector2 Position { get => _position; set => _position = value; }
        [DontSaveInGorkGraph] [HideInInspector] [SerializeField] private Vector2 _position;

        /// <summary>
        /// If the node is expanded or not in the Graph View. <para/>
        /// NOTE: This is only available in the editor. Usage in builds will result in errors!
        /// </summary>
        public bool Expanded { get => _expanded; set => _expanded = value; }
        [HideInInspector] [SerializeField] private bool _expanded = true;
#endif
        /// <summary>
        /// The nodes title in the editor. Can be overwritten if you set the title in a custom Gork Node Editor script.
        /// </summary>
        public virtual string Title => null;

        #region Get Parameter
        /// <summary>
        /// Returns the parameter with the type of <paramref name="type"/> and with the given <paramref name="name"/>.
        /// </summary>
        protected object GetParameter(string name, Type type) => Graph.GetParameter(name, type);
        /// <summary>
        /// Returns the parameter with the type of <paramref name="T"/> and with the given <paramref name="name"/>.
        /// </summary>
        protected T GetParameter<T>(string name) => Graph.GetParameter<T>(name);

        /// <summary>
        /// Returns the <see cref="float"/> parameter with the given <paramref name="name"/>.
        /// </summary>
        protected float GetFloatParameter(string name) => Graph.GetFloat(name);
        /// <summary>
        /// Returns the <see cref="int"/> parameter with the given <paramref name="name"/>.
        /// </summary>
        protected int GetIntParameter(string name) => Graph.GetInt(name);
        /// <summary>
        /// Returns the <see cref="bool"/> parameter with the given <paramref name="name"/>.
        /// </summary>
        protected bool GetBoolParameter(string name) => Graph.GetBool(name);
        /// <summary>
        /// Returns the <see cref="string"/> parameter with the given <paramref name="name"/>.
        /// </summary>
        protected string GetStringParameter(string name) => Graph.GetString(name);
        #endregion

        #region GorkGraph
        [SerializeField] [HideInInspector] private GorkGraph _graph;
        public GorkGraph Graph
        {
            get
            {
#if UNITY_EDITOR
                if (_graph == null)
                {
                    _graph = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GetAssetPath(this)) as GorkGraph;
                }
#endif

                return _graph;
            }
            set => _graph = value;
        }
        #endregion

        #region Tag
        [SerializeField] [HideInInspector] private List<string> _tags = new List<string>();
#if UNITY_EDITOR
        [HideInInspector] private bool _sortedTags;
#endif
        public List<string> Tags
        {
            get
            {
#if UNITY_EDITOR
                if (!_sortedTags)
                {
                    List<string> newList = new List<string>();

                    foreach (string tag in _tags)
                    {
                        if (newList.Contains(tag))
                        {
                            continue;
                        }

                        newList.Add(tag);
                    }

                    _sortedTags = true;
                }
#endif

                return _tags;
            }
        }

        public void AddTag(string tag)
        {
#if UNITY_EDITOR
            Undo.RecordObject(this, $"Added tag \"{tag}\"");
#endif
            _tags.Add(tag);
        }

        public void RemoveTag(string tag)
        {
#if UNITY_EDITOR
            Undo.RecordObject(this, $"Removed tag \"{tag}\"");
#endif
            _tags.RemoveAll(entry => entry == tag);
        }
        #endregion

        #region Ports
        private NodePort[] _attributeInputPorts = null;
        private NodePort[] _attributeOutputPorts = null;

        /// <summary>
        /// A list of all custom input ports that have been added to this <see cref="GorkNode"/>. <para/>
        /// This does not include any ports added from a <see cref="GorkInputPortAttribute"/>. <para/>
        /// If you want a full collection of all input ports on this node, use <see cref="InputPorts"/> instead. <para/>
        /// It is recommended that you do not modify this list during runtime and only do it in the editor as this list is serialized.
        /// </summary>
        public List<NodePort> CustomInputPorts => customInputPorts;
        [HideInInspector] [SerializeField] private List<NodePort> customInputPorts = new List<NodePort>();
        /// <summary>
        /// A list of all custom output ports that have been added to this <see cref="GorkNode"/>. <para/>
        /// This does not include any ports added from a <see cref="GorkOutputPortAttribute"/>. <para/>
        /// If you want a full collection of all output ports on this node, use <see cref="OutputPorts"/> instead. <para/>
        /// It is recommended that you do not modify this list during runtime and only do it in the editor as this list is serialized.
        /// </summary>
        public List<NodePort> CustomOutputPorts => customOutputPorts;
        [HideInInspector] [SerializeField] private List<NodePort> customOutputPorts = new List<NodePort>();

        /// <summary>
        /// A <see cref="NodePortCollection"/> of all the input <see cref="NodePort"/> attached to this node. <para/>
        /// This is actually a collection of both attribute ports and <see cref="CustomInputPorts"/> on this node. <para/>
        /// It is recommended that you only read this collection during runtime. Modifying this should only be done in the editor as the <see cref="CustomInputPorts"/> list is serialized.
        /// </summary>
        public NodePortCollection InputPorts
        {
            get
            {
                // Check if the ports are null
                if (_inputPorts == null)
                {
                    // If so, then we will create a new collection

                    // Also create the attribute ports in case they are null
                    if (_attributeInputPorts == null)
                    {
                        CreateAttributePorts();
                    }

                    // Regenerate the collection
                    _inputPorts = new NodePortCollection(_attributeInputPorts, customInputPorts);
                }

                return _inputPorts;
            }
        }
        private NodePortCollection _inputPorts = null;

        /// <summary>
        /// A <see cref="NodePortCollection"/> of all the output <see cref="NodePort"/> attached to this node. <para/>
        /// This is actually a collection of both attribute ports and <see cref="CustomOutputPorts"/> on this node. <para/>
        /// It is recommended that you only read this collection during runtime. Modifying this should only be done in the editor as the <see cref="CustomOutputPorts"/> list is serialized.
        /// </summary>
        public NodePortCollection OutputPorts
        {
            get
            {
                // Check if the ports are null
                if (_outputPorts == null)
                {
                    // If so, then we will create a new collection

                    // Also create the attribute ports in case they are null
                    if (_attributeOutputPorts == null)
                    {
                        CreateAttributePorts();
                    }

                    // Regenerate the collection
                    _outputPorts = new NodePortCollection(_attributeOutputPorts, customOutputPorts);
                }

                return _outputPorts;
            }
        }
        private NodePortCollection _outputPorts = null;

        /// <summary>
        /// Sets both the <see cref="AttributeInputPorts"/> and <see cref="AttributeOutputPorts"/> lists to null so that they will regenerate the next time they are used.
        /// </summary>
        public void RefreshPorts()
        {
            _attributeInputPorts = null;
            _attributeOutputPorts = null;

            _inputPorts = null;
            _outputPorts = null;
        }

        /// <summary>
        /// Will create and populate the attribute port arrays stored in the <see cref="InputPorts"/> and <see cref="OutputPorts"/> collections. <para/>
        /// Is called whenever the <see cref="InputPorts"/> or <see cref="OutputPorts"/> collections are used when they are null, which happens at the start of the nodes existance or from calling the <see cref="RefreshPorts"/> method.
        /// </summary>
        private void CreateAttributePorts()
        {
            // Cache the node type
            Type type = GetType();

            // We will create no attribute ports at all if the attributes dicitonary doesn't contain this node type
            if (!GorkPortAttribute.Attributes.ContainsKey(type))
            {
                // Create empty arrays
                _attributeInputPorts = new NodePort[0];
                _attributeOutputPorts = new NodePort[0];

                return;
            }

            // Get a list of all the GorkPortAttributes that are on this GorkNode script
            IEnumerable<GorkPortAttribute> attributes = GorkPortAttribute.Attributes[type];

            // Create two lists which will later be turned into arrays for our attribute ports
            List<NodePort> inputPorts = new List<NodePort>();
            List<NodePort> outputPorts = new List<NodePort>();

            // Loop through every GorkPortAttribute
            foreach (GorkPortAttribute attribute in attributes)
            {
                // Check which type of attribute it is
                if (attribute is GorkInputPortAttribute)
                {
                    // If it's an input port, then add the attribute port to the input ports list
                    inputPorts.Add(new NodePort(attribute.DisplayName, attribute.PortType));
                }
                else if (attribute is GorkOutputPortAttribute)
                {
                    // If it's an output port, then add the attribute port to the output ports list
                    outputPorts.Add(new NodePort(attribute.DisplayName, attribute.PortType));
                }
            }

            // Convert the two port lists into arrays which we will set as our attribute ports
            _attributeInputPorts = inputPorts.ToArray();
            _attributeOutputPorts = outputPorts.ToArray();
        }
        #endregion

        #region GetValueFromPort
        /// <summary>
        /// Returns a raw <see cref="object"/> value from the input port with the given <paramref name="port"/> index. <para/>
        /// Will correctly convert different values with different types using a <see cref="GorkConverterAttribute"/>. <para/>
        /// Getting a <see cref="string"/> or <see cref="object"/> value will also NEVER fail as anything can be converted to <see cref="string"/> via <see cref="object.ToString"/> and also because everything in C# can be of type <see cref="object"/>. <para/>
        /// Use the generic <see cref="GetValueFromPort{T}(int)"/> if you are 100% sure what type of value you are going to receive.
        /// </summary>
        public virtual object GetValueFromPort(int port)
        {
            Type inputType;

            try
            {
                inputType = InputPorts[port].Type;
            }
            // Catch specifically out of range exceptions so that we can give a warning instead
            catch (IndexOutOfRangeException)
            {
#if UNITY_EDITOR
                Debug.LogWarning($"Input Port index {port} is out of range!");
#endif
                return default;
            }

            // Return default value if this input port type is a signal
            if (inputType.IsSignal())
            {
#if UNITY_EDITOR
                Debug.LogWarning($"Input Port {port + 1} is of type Signal! This means that it cannot get values!");
#endif
                return default;
            }

            // Get connections from the input port
            List<Connection> connectionList = GetInputConnections(port);

            // Nothing is connected
            if (connectionList.Count <= 0)
            {
#if UNITY_EDITOR
                // Give out warning
                Debug.LogWarning($"No connection was connected to Input Port {port + 1}!");
#endif
                return default;
            }

            Connection connection = connectionList[0];

            // Get the node on the end of the connection (as in the node that we will take value from)
            GorkNode node = connection.Node;

            // Get the nodes output port type
            int outputIndex = Mathf.Clamp(connection.PortIndex, 0, node.OutputPorts.Count - 1);
            Type outputType = node.OutputPorts[outputIndex].Type;

            // Return default value if this output port type is a signal
            if (outputType.IsSignal())
            {
#if UNITY_EDITOR
                Debug.LogWarning($"Output Port {outputIndex + 1} is of type Signal! This means that it cannot get values!");
#endif
                return default;
            }

            // Input & output type is the same?
            if (inputType == outputType)
            {
                // Just return the value, no GorkConverter is needed
                return node.ValueCall(connection.PortIndex, inputType);
            }

            // Is generic object type?
            if (inputType == typeof(object))
            {
                // Notice how we use the parameter type and not the return type
                return node.ValueCall(connection.PortIndex, outputType);
            }
            // Is returning a string?
            else if (inputType == typeof(string))
            {
                // Use ToString() because ANYTHING can be string
                return node.ValueCall(connection.PortIndex, outputType).ToString();
            }

            // Both types are different!
            // Usage of a GorkConverter is needed

            // Return if the GorkConversion Dictionary is missing a GorkConverter for these 2 types
            if (!GorkConverterAttribute.GorkConvertion.TryGetValue(outputType, out var other)
                ||
                !other.TryGetValue(inputType, out MethodInfo conversionMethod))
            {
#if UNITY_EDITOR
                Debug.LogWarning($"A GorkConverter that converts from: \"{outputType}\" to \"{inputType}\" does not exist!");
#endif
                return default;
            }

            // Get the value and use our GorkConverter
            object value = node.ValueCall(connection.PortIndex, outputType);

            return conversionMethod.Invoke(null, new object[] { value });
        }

        /// <summary>
        /// Returns a value of type <typeparamref name="T"/> from the input port with the given <paramref name="port"/> index. <para/>
        /// Will correctly convert different values with different types using a <see cref="GorkConverterAttribute"/>. <para/>
        /// Getting a <see cref="string"/> or <see cref="object"/> value will also NEVER fail as anything can be converted to <see cref="string"/> via <see cref="object.ToString"/> and also because everything in C# can be of type <see cref="object"/>. <para/>
        /// Use the regular <see cref="GetValueFromPort(int)"/> if you want a raw <see cref="object"/> value instead.
        /// </summary>
        public virtual T GetValueFromPort<T>(int port) => (T)GetValueFromPort(port);
#endregion

        #region Value Calls
        /// <summary>
        /// This method is called by another <see cref="GorkNode"/> when it needs to obtain a certain value from this <see cref="GorkNode"/> using <see cref="GetValueFromPort{T}(int)"/>. <para/>
        /// Override this to add your own custom value type that can be returned, otherwise <see cref="FloatCall(int)"/>, <see cref="IntCall(int)"/>, <see cref="BoolCall(int)"/> or <see cref="StringCall(int)"/> should be overwritten instead.
        /// </summary>
        /// <param name="port">The current INPUT port index of this <see cref="GorkNode"/> that's being called.</param>
        /// <param name="type">The type of value that is going to be given.</param>
        public virtual object ValueCall(int port, Type type)
        {
            if (type == typeof(float))
            {
                return FloatCall(port);
            }
            else if (type == typeof(int))
            {
                return IntCall(port);
            }
            else if (type == typeof(bool))
            {
                return BoolCall(port);
            }
            else if (type == typeof(string))
            {
                return StringCall(port);
            }

            return default;
        }

        /// <summary>
        /// Override this to add custom behaviour for when this node should return a <see cref="float"/> value. <para/>
        /// If you want to return a custom data type then override <see cref="ValueCall(int, Type)"/>.
        /// </summary>
        public virtual float FloatCall(int port)
        {
            return default;
        }

        /// <summary>
        /// Override this to add custom behaviour for when this node should return a <see cref="int"/> value. <para/>
        /// If you want to return a custom data type then override <see cref="ValueCall(int, Type)"/>.
        /// </summary>
        public virtual int IntCall(int port)
        {
            return default;
        }

        /// <summary>
        /// Override this to add custom behaviour for when this node should return a <see cref="bool"/> value. <para/>
        /// If you want to return a custom data type then override <see cref="ValueCall(int, Type)"/>.
        /// </summary>
        public virtual bool BoolCall(int port)
        {
            return default;
        }

        /// <summary>
        /// Override this to add custom behaviour for when this node should return a <see cref="string"/> value. <para/>
        /// If you want to return a custom data type then override <see cref="ValueCall(int, Type)"/>.
        /// </summary>
        public virtual string StringCall(int port)
        {
            return default;
        }
        #endregion

        #region Call Ports
        public void CallPort(int port)
        {
            List<Connection> connections = GetOutputConnections(port);

            // Call nodes
            foreach (Connection connection in connections)
            {
                Graph.OnNodeCalled(connection.Node, connection.PortIndex);
            }
        }
        #endregion

        /// <summary>
        /// Called whenever this node needs to process any input signals. <para/>
        /// Override this if you want to do some regular node behaviour. <para/>
        /// For node behaviour with coroutines, see: <see cref="ProcessIEnumerator(int)"/>.
        /// </summary>
        public virtual void Process(int port)
        {
            CallPort(0);
        }

        /// <summary>
        /// Called whenever this node needs to process any input signals. <para/>
        /// Override this method to add custom coroutine functionality to your node.
        /// </summary>
        public virtual IEnumerator ProcessIEnumerator(int port)
        {
            Process(port);

            yield return null;
        }

        /// <summary>
        /// Is called when this node is stopped.
        /// </summary>
        public virtual void OnStopped()
        {

        }

        #region Connections
        /// <summary>
        /// A list of every connection on every Input Port on this node.
        /// </summary>
        public List<ConnectionCollection> InputConnections => _inputConnections;
        [DontSaveInGorkGraph] [HideInInspector] [SerializeField] private List<ConnectionCollection> _inputConnections = new List<ConnectionCollection>();

        /// <summary>
        /// A list of every connection on every Output Port on this node.
        /// </summary>
        public List<ConnectionCollection> OutputConnections => _outputConnections;
        [DontSaveInGorkGraph] [HideInInspector] [SerializeField] private List<ConnectionCollection> _outputConnections = new List<ConnectionCollection>();

        /// <summary>
        /// Returns a list of connections from the input port at the given <paramref name="index"/>. <para/>
        /// If the index is out of range, then this method will automatically expand the list and return an empty connection list.
        /// </summary>
        public List<Connection> GetInputConnections(int index)
        {
            return GetConnections(ref _inputConnections, index);
        }

        /// <summary>
        /// Returns a list of connections from the output port at the given <paramref name="index"/>. <para/>
        /// If the index is out of range, then this method will automatically expand the list and return an empty connection list.
        /// </summary>
        public List<Connection> GetOutputConnections(int index)
        {
            return GetConnections(ref _outputConnections, index);
        }

        /// <summary>
        /// Returns a list of connections from the <paramref name="list"/> at the <paramref name="index"/> port. <para/>
        /// If the index is out of range, then this method will automatically expand the list and return an empty connection list.
        /// </summary>
        private List<Connection> GetConnections(ref List<ConnectionCollection> list, int index)
        {
            return list[index].Connections;
        }

        /// <summary>
        /// Will return if the input port with the given <paramref name="index"/> has any connections attached.
        /// </summary>
        public bool HasInputConnection(int index)
        {
            return HasConnections(_inputConnections, index);
        }

        /// <summary>
        /// Will return if the output port with the given <paramref name="index"/> has any connections attached.
        /// </summary>
        public bool HasOutputConnection(int index)
        {
            return HasConnections(_outputConnections, index);
        }

        /// <summary>
        /// Will return if the given <paramref name="list"/> at the port with the given <paramref name="index"/> has any connections attached.
        /// </summary>
        private bool HasConnections(List<ConnectionCollection> list, int index)
        {
            return index >= 0 && index <= list.Count - 1 && list[index].Connections.Count > 0;
        }

        /// <summary>
        /// A simple container class that contains a collection of <see cref="Connection"/> for a single <see cref="GorkNode"/> Port.
        /// </summary>
        [Serializable]
        public struct ConnectionCollection
        {
            public List<Connection> Connections;
        }

        /// <summary>
        /// A simple container class that contains a connection between this <see cref="GorkNode"/> and another <see cref="GorkNode"/>.
        /// </summary>
        [Serializable]
        public struct Connection
        {
            /// <summary>
            /// The port that the <see cref="Node"/> is connected to.
            /// </summary>
            public int PortIndex;
            /// <summary>
            /// The node we are connected to.
            /// </summary>
            public GorkNode Node;

            public Connection(int portIndex, GorkNode child)
            {
                PortIndex = portIndex;
                Node = child;
            }
        }
        #endregion
    }
}
