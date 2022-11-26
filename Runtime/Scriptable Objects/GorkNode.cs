using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System.Reflection;

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
#endif

namespace Gork
{
    /// <summary>
    /// A single node in a <see cref="GorkGraph"/>.
    /// </summary>
    public class GorkNode : ScriptableObject
    {
#if UNITY_EDITOR
        [DontSaveInGorkGraph] [HideInInspector] public string GUID;
        [DontSaveInGorkGraph] [HideInInspector] public Vector2 Position; // The position in the graph

        private string _title;
        /// <summary>
        /// The title of this node on the <see cref="GorkGraph"/>. NOTE: This is only available in the Unity Editor.
        /// </summary>
        public string Title { get => _title; set => _title = value; }

        #region Ports
        private List<Port> _inputPorts = new List<Port>();
        private List<Port> _outputPorts = new List<Port>();
        public List<Port> InputPorts => _inputPorts;
        public List<Port> OutputPorts => _outputPorts;

        public class Port
        {
            public string Name;
            public Type Type;
            public bool DisplayType  = true;

            public Port(string name, Type type)
            {
                Name = name;
                Type = type;
            }

            public Port(string name, Type type, bool displayType)
            {
                Name = name;
                Type = type;
                DisplayType = displayType;
            }
        }

        private Port GetPort(string defaultName, List<Port> ports, int index)
        {
            int count = ports.Count;

            if (index < count)
            {
                return ports[index];
            }

            Port port = null;

            for (int i = count; i < index + 1; i++)
            {
                port = new Port(defaultName, GorkPortAttribute.DefaultType);
                ports.Add(port);
            }

            return port;
        }

        protected Port GetInputPort(int index)
        {
            return GetPort("Input", InputPorts, index);
        }

        protected Port GetOutputPort(int index)
        {
            return GetPort("Output", OutputPorts, index);
        }

        protected void SetInputPort(int index, Action<Port> action)
        {
            Port port = GetInputPort(index);

            action?.Invoke(port);
        }

        protected void SetOutputPort(int index, Action<Port> action)
        {
            Port port = GetOutputPort(index);

            action?.Invoke(port);
        }

        protected void SetInputPort(int index, string name)
        {
            SetInputPort(index, port =>
            {
                port.Name = name;
            });
        }

        protected void SetInputPort(int index, string name, Type type)
        {
            SetInputPort(index, port =>
            {
                port.Name = name;
                port.Type = type;
            });
        }

        protected void SetInputPort(int index, string name, Type type, bool displayType)
        {
            SetInputPort(index, port =>
            {
                port.Name = name;
                port.Type = type;
                port.DisplayType = displayType;
            });
        }

        protected void DeleteInputPort(int index)
        {
            if (index < 0 || index > InputPorts.Count - 1)
            {
                return;
            }

            InputPorts.RemoveAt(index);
        }

        protected void SetOutputPort(int index, string name)
        {
            SetOutputPort(index, port =>
            {
                port.Name = name;
            });
        }

        protected void SetOutputPort(int index, string name, Type type)
        {
            SetOutputPort(index, port =>
            {
                port.Name = name;
                port.Type = type;
            });
        }

        protected void SetOutputPort(int index, string name, Type type, bool displayType)
        {
            SetOutputPort(index, port =>
            {
                port.Name = name;
                port.Type = type;
                port.DisplayType = displayType;
            });
        }

        protected void DeleteOutputPort(int index)
        {
            if (index < 0 || index > OutputPorts.Count - 1)
            {
                return;
            }

            OutputPorts.RemoveAt(index);
        }
        #endregion

        #region UpdateNodeView()
        [DontSaveInGorkGraph] public Action UpdateNodeViewCallback;
        protected virtual void UpdateNodeView() => UpdateNodeViewCallback?.Invoke();
        #endregion

        #region IMGUI Drawing
        private Editor _editor;
        protected Editor editor
        {
            get
            {
                if (_editor == null)
                {
                    _editor = Editor.CreateEditor(this);
                }

                return _editor;
            }
        }

        protected SerializedObject serializedObject => editor.serializedObject;
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
        }
        #endregion

        /// <summary>
        /// Override this method for total control over the way that this <see cref="GorkNode"/> is drawn in the editor. <para/>
        /// Be weary however that this method only exists in the editor, so make sure to wrap your method in a #IF UNITY_EDITOR block. <para/>
        /// Same goes for the namespace importing of "UnityEditor.Experimental.GraphView", ensure it's wrapped in a #IF UNITY_EDITOR block as well.
        /// </summary>
        public virtual void Initialize(Node node)
        {
            // Create a list of visual elements which will be fed into the Draw method
            List<VisualElement> elements = new List<VisualElement>();

            // Draw the elements
            Draw(elements);

            // Loop through all elements
            foreach (VisualElement element in elements)
            {
                // Add the element to the extension container
                node.extensionContainer.Add(element);
            }

            // Refresh the node
            node.RefreshExpandedState();
        }

        /// <summary>
        /// Override this method to draw custom elements on the <see cref="GorkNode"/> using a list of <see cref="VisualElement"/>. <para/>
        /// This should be more than enough for customizability, but if you want the ability to modify even more things, then override <see cref="Initialize(Node)"/>. <para/>
        /// Be weary however that <see cref="Initialize(Node)"/> and this only exists in the editor, so make sure to wrap your method in a #IF UNITY_EDITOR block. <para/>
        /// Same goes for the namespace importing of "UnityEditor.Experimental.GraphView", ensure it's wrapped in a #IF UNITY_EDITOR block as well.
        /// </summary>
        protected virtual void Draw(List<VisualElement> elements)
        {
            elements.Add(CreateInspector());
        }

        protected IMGUIContainer CreateInspector()
        {
            return new IMGUIContainer(OnInspectorGUI);
        }

        /// <summary>
        /// Is called when the Node View is enabled. NOTE: This is called after <see cref="Initialize(Node)"/>
        /// </summary>
        public virtual void OnViewEnable()
        {

        }

        private const string SCRIPT_PROP = "m_Script";

        public virtual float InspectorLabelWidth => 150;
        public virtual float InspectorFieldWidth => 50;

        /// <summary>
        /// Override this method to draw custom inspector elements on the <see cref="GorkNode"/> using Unitys IMGUI system. <para/>
        /// Use this like you would in any other <see cref="Editor"/> script. However, if you want the ability to modify more things, then override <see cref="Draw"/>. <para/>
        /// Be weary however that <see cref="Draw"/> and this only exists in the editor, so make sure to wrap your method in a #IF UNITY_EDITOR block. <para/>
        /// Same goes for the namespace importing of "UnityEditor", ensure it's wrapped in a #IF UNITY_EDITOR block as well.
        /// </summary>
        protected virtual void OnInspectorGUI()
        {
            SetupInspector();

            DoDefaultInspector();
        }

        protected bool DoDefaultInspector()
        {
            // Get iterator
            SerializedProperty property = serializedObject.GetIterator();

            bool expanded = true;
            bool doneSpace = false;

            while (property.NextVisible(expanded))
            {
                if (property.propertyPath == SCRIPT_PROP)
                {
                    continue;
                }

                if (!doneSpace)
                {
                    EditorGUILayout.Space();
                    doneSpace = true;
                }

                EditorGUILayout.PropertyField(property, true);

                expanded = false;
            }

            if (doneSpace)
            {
                EditorGUILayout.Space();
            }

            return serializedObject.ApplyModifiedProperties();
        }

        protected void SetupInspector()
        {
            // Set label & field width
            EditorGUIUtility.labelWidth = InspectorLabelWidth;
            EditorGUIUtility.fieldWidth = InspectorFieldWidth;

            // Update editor if required or script
            serializedObject.UpdateIfRequiredOrScript();
        }
#endif

        public virtual void CallPort(int port)
        {

        }

        public virtual T GetValueFromPort<T>(int port)
        {
            // Local function that will get the value from a Connection
            // Also uses a GorkConverter when neccessary
            T GetValueFromConnection(Connection connection)
            {
                GorkNode node = connection.Node;
                Type parameterType = node.OutputPortTypes[connection.PortIndex];
                Type returnType = typeof(T);

                // Parameter & return type is the same?
                if (parameterType == returnType)
                {
                    // Just return the value, no GorkConverter is needed
                    return (T)node.ValueCall(connection.PortIndex, returnType);
                }

                // Both types are different!
                // Usage of a GorkConverter is needed

                // Return if the GorkConversion Dictionary is missing a GorkConverter for these 2 types
                if (!GorkConverterAttribute.GorkConvertion.TryGetValue(parameterType, out var other)
                    ||
                    !other.TryGetValue(returnType, out MethodInfo conversionMethod))
                {
#if UNITY_EDITOR
                    Debug.LogWarning($"A GorkConverter that converts from: \"{parameterType}\" to \"{returnType}\" does not exist!");
#endif
                    return default;
                }

                // Get the value and use our GorkConverter
                object value = node.ValueCall(connection.PortIndex, parameterType);

                return (T)conversionMethod.Invoke(null, new object[] { value });
            }

            List<Connection> connections = GetInputConnections(port);

            if (connections.Count <= 1)
            {
                return GetValueFromConnection(connections[0]);
            }

            return default;
        }

        public virtual IEnumerator NodeIEnumerator(int port)
        {
            NodeCall(port);

            yield return null;

            CallPort(0);
        }

        public virtual void NodeCall(int port)
        {

        }

        private static readonly Type FloatType = typeof(float);
        private static readonly Type IntType = typeof(int);
        private static readonly Type BoolType = typeof(bool);
        private static readonly Type StringType = typeof(string);

        /// <summary>
        /// This method is called by another <see cref="GorkNode"/> when it needs to obtain a certain value from this <see cref="GorkNode"/> using <see cref="GetValueFromPort{T}(int)"/>. <para/>
        /// Override this to add your own custom value type that can be returned, otherwise <see cref="FloatCall(int)"/>, <see cref="IntCall(int)"/>, <see cref="BoolCall(int)"/> or <see cref="StringCall(int)"/> should be overwritten instead.
        /// </summary>
        /// <param name="port">The current OUTPUT port index of this <see cref="GorkNode"/> that's being called.</param>
        /// <param name="type">The type of value that is going to be given.</param>
        public virtual object ValueCall(int port, Type type)
        {
            if (type == FloatType)
            {
                return FloatCall(port);
            }
            else if (type == IntType)
            {
                return IntCall(port);
            }
            else if (type == BoolType)
            {
                return BoolCall(port);
            }
            else if (type == StringType)
            {
                return StringCall(port);
            }

            return default;
        }

        public virtual float FloatCall(int port)
        {
            return default;
        }

        public virtual int IntCall(int port)
        {
            return default;
        }

        public virtual bool BoolCall(int port)
        {
            return default;
        }

        public virtual string StringCall(int port)
        {
            return default;
        }

        #region Connections
        [DontSaveInGorkGraph] [HideInInspector] public List<ConnectPort> OutputConnections = new List<ConnectPort>();
        [DontSaveInGorkGraph] [HideInInspector] public List<ConnectPort> InputConnections = new List<ConnectPort>();

        public List<Connection> GetInputConnections(int index)
        {
            return GetConnections(ref InputConnections, index);
        }

        public List<Connection> GetOutputConnections(int index)
        {
            return GetConnections(ref OutputConnections, index);
        }

        private List<Connection> GetConnections(ref List<ConnectPort> list, int index)
        {
            int count = list.Count - 1;

            if (count < index)
            {
                for (int i = count; i < index; i++)
                {
                    ConnectPort port = new ConnectPort();
                    port.Connections = new List<Connection>();

                    list.Add(port);
                }
            }

            return list[index].Connections;
        }

        public void RemoveInputConnections(int index)
        {
            RemoveConnections(ref InputConnections, index);
        }

        public void RemoveOutputConnections(int index)
        {
            RemoveConnections(ref OutputConnections, index);
        }

        private void RemoveConnections(ref List<ConnectPort> list, int index)
        {
            if (index >= list.Count)
            {
                return;
            }

            list.RemoveAt(index);
        }

        /// <summary>
        /// A simple container class that contains a list of <see cref="Connection"/> for a single <see cref="GorkNode"/> Port.
        /// </summary>
        [Serializable]
        public struct ConnectPort
        {
            public List<Connection> Connections;
        }

        /// <summary>
        /// A simple container class contains a connection between this <see cref="GorkNode"/> and another <see cref="GorkNode"/>.
        /// </summary>
        [Serializable]
        public struct Connection
        {
            // Note: Port index is the child port
            public int PortIndex;
            public GorkNode Node;

            public Connection(int portIndex, GorkNode child)
            {
                PortIndex = portIndex;
                Node = child;
            }
        }
        #endregion

        #region Port Types
        #region Static Fields
        public static readonly Dictionary<Type, List<Type>> StaticInputPortTypes = new Dictionary<Type, List<Type>>();
        public static readonly Dictionary<Type, List<Type>> StaticOutputPortTypes = new Dictionary<Type, List<Type>>();

        public static readonly Dictionary<Type, int> StaticInputPortTypesLength = new Dictionary<Type, int>();
        public static readonly Dictionary<Type, int> StaticOutputPortTypesLength = new Dictionary<Type, int>();
        #endregion

        #region Public Instance Fields
        private List<Type> _cachedInputPortTypes;
        public List<Type> InputPortTypes
        {
            get
            {
                if (_cachedInputPortTypes == null)
                {
                    _cachedInputPortTypes = StaticInputPortTypes[GetType()];
                }

                return _cachedInputPortTypes;
            }
        }

        private List<Type> _cachedOutputPortTypes;
        public List<Type> OutputPortTypes
        {
            get
            {
                if (_cachedOutputPortTypes == null)
                {
                    _cachedOutputPortTypes = StaticOutputPortTypes[GetType()];
                }

                return _cachedOutputPortTypes;
            }
        }

        private int? _cachedInputPortTypeLength = null;
        public int InputPortTypesLength
        {
            get
            {
                if (_cachedInputPortTypeLength == null)
                {
                    _cachedInputPortTypeLength = StaticInputPortTypesLength[GetType()];
                }

                return _cachedInputPortTypeLength.Value;
            }
        }

        private int? _cachedOutputPortTypeLength = null;
        public int OutputPortTypesLength
        {
            get
            {
                if (_cachedOutputPortTypeLength == null)
                {
                    _cachedOutputPortTypeLength = StaticOutputPortTypesLength[GetType()];
                }

                return _cachedOutputPortTypeLength.Value;
            }
        }
        #endregion

#if UNITY_EDITOR
        [InitializeOnLoadMethod]
#else
        [RuntimeInitializeOnLoadMethod]
#endif
        public static void CachePortTypes()
        {
            Type gorkNodeType = typeof(GorkNode);

            // Loop through every assembly in the AppDomain
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                // Loop through every type in the assembly
                foreach (Type type in assembly.GetTypes())
                {
                    // Return if this type DOES NOT inherit from GorkNode
                    if (!type.IsSubclassOf(gorkNodeType))
                    {
                        continue;
                    }

                    // It does inherit from GorkNode

                    List<Type> inputPortTypes = new List<Type>();
                    List<Type> outputPortTypes = new List<Type>();

                    // Check first for if we have the NoOutput/NoInput ports attributes
                    // Leave the list empty if we have one of these attributes attached

                    void PortCheck(bool isOutputPorts)
                    {
                        List<Type> list = isOutputPorts ? outputPortTypes : inputPortTypes;
                        int count = 0;

                        foreach (GorkPortAttribute port in isOutputPorts ? type.GetCustomAttributes<GorkInputPortAttribute>() : type.GetCustomAttributes<GorkInputPortAttribute>())
                        {
                            count++;

                            list.Add(port.PortType);
                        }

                        if (count == 0)
                        {
                            list.Add(GorkPortAttribute.DefaultType);
                        }
                    }

                    if (type.GetCustomAttribute<NoInputPortsAttribute>() == null)
                    {
                        PortCheck(false);
                    }

                    if (type.GetCustomAttribute<NoOutputPortsAttribute>() == null)
                    {
                        PortCheck(true);
                    }

                    StaticInputPortTypes.Add(type, inputPortTypes);
                    StaticOutputPortTypes.Add(type, outputPortTypes);

                    StaticInputPortTypesLength.Add(type, inputPortTypes.Count);
                    StaticOutputPortTypesLength.Add(type, outputPortTypes.Count);
                }
            }
        }
        #endregion
    }
}
