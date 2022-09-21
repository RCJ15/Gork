using System;
using System.Collections.Generic;
using UnityEngine;

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
        #endregion

        #region UpdateNodeView()
        [DontSaveInGorkGraph] public Action UpdateNodeViewCallback;
        protected void UpdateNodeView() => UpdateNodeViewCallback?.Invoke();
        #endregion

        #region IMGUI Drawing
        private Editor _editor;
        protected SerializedObject serializedObject => _editor.serializedObject;
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
            if (_editor == null)
            {
                _editor = Editor.CreateEditor(this);
            }

            IMGUIContainer container = new IMGUIContainer(OnInspectorGUI);

            elements.Add(container);
        }

        /// <summary>
        /// Is called when the Node View is enabled. NOTE: This is called after <see cref="Initialize(Node)"/>
        /// </summary>
        public virtual void OnViewEnable()
        {

        }

        private const string SCRIPT_PROP = "m_Script";

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

        public virtual object NodeCall<T>(T value)
        {
            return value;
        }

        public virtual float InspectorLabelWidth => 150;
        public virtual float InspectorFieldWidth => 50;

        #region Connections
        [DontSaveInGorkGraph] /*[HideInInspector]*/ public List<ConnectPort> AllConnections = new List<ConnectPort>();

        public List<Connection> GetConnections(int index)
        {
            int count = AllConnections.Count - 1;

            if (count < index)
            {
                for (int i = count; i < index; i++)
                {
                    ConnectPort port = new ConnectPort();
                    port.Connections = new List<Connection>();

                    AllConnections.Add(port);
                }
            }

            return AllConnections[index].Connections;
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
    }
}
