using System;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.Experimental.GraphView;

namespace Gork.Editor
{
    using Editor = UnityEditor.Editor;

    /// <summary>
    /// 
    /// </summary>
    public class GorkNodeEditor
    {
        /// <summary>
        /// The <see cref="GorkNode"/> that this editor is attached to.
        /// </summary>
        public GorkNode Node { get; set; }

        /// <summary>
        /// The <see cref="GorkGraph"/> that this editors node is attached to.
        /// </summary>
        public GorkGraph Graph { get; set; }

        /// <summary>
        /// The <see cref="GorkGraphView"/> that the <see cref="NodeView"/> is attached to.
        /// </summary>
        public GorkGraphView GraphView { get; set; }

        /// <summary>
        /// The <see cref="GorkNodeView"/> that this editors node is attached to.
        /// </summary>
        public GorkNodeView NodeView { get; set; }

        /// <summary>
        /// If the <see cref="GorkNodeView"/> is expanded or not.
        /// </summary>
        public bool Expanded => Node.Expanded;

        #region Title
        /// <summary>
        /// The title of this node. Use <see cref="SetTitle(string, bool)"/> to set this value.
        /// </summary>
        public string Title { get => _title; }
        private string _title;

        /// <summary>
        /// Sets the title of this node and updates the Node Views Title if <paramref name="updateNode"/> is true.
        /// </summary>
        public void SetTitle(string newTitle, bool updateNode = true)
        {
            _title = newTitle;

            if (updateNode)
            {
                NodeView.UpdateTitle();
            }
        }

        /// <summary>
        /// Resets this node back to the title that the node was given from it's <see cref="GorkMenuItemAttribute"/>. Updates the Node Views Title if <paramref name="updateNode"/> is true.
        /// </summary>
        public void ResetTitle(bool updateNode = true)
        {
            _title = NodeView.Attribute.DisplayName;

            if (updateNode)
            {
                NodeView.UpdateTitle();
            }
        }
        #endregion

        #region Custom Ports
        public NodePortCollection InputPorts => Node.InputPorts;
        public NodePortCollection OutputPorts => Node.OutputPorts;

        public List<NodePort> CustomInputPorts => Node.CustomInputPorts;
        public List<NodePort> CustomOutputPorts => Node.CustomOutputPorts;

        /*
        private NodePort GetPort(string defaultName, List<NodePort> ports, int index)
        {
            int count = ports.Count;

            if (index < count)
            {
                return ports[index];
            }

            NodePort port = null;

            for (int i = count; i < index + 1; i++)
            {
                port = new NodePort(defaultName, GorkUtility.SignalType);
                ports.Add(port);
            }

            return port;
        }

        protected NodePort GetInputPort(int index)
        {
            return GetPort("Input", CustomInputPorts, index);
        }

        protected NodePort GetOutputPort(int index)
        {
            return GetPort("Output", CustomOutputPorts, index);
        }

        protected void SetInputPort(int index, Action<NodePort> action)
        {
            NodePort port = GetInputPort(index);

            action?.Invoke(port);
        }

        protected void SetOutputPort(int index, Action<NodePort> action)
        {
            NodePort port = GetOutputPort(index);

            action?.Invoke(port);
        }

        protected void SetInputPort(int index, string name)
        {
            SetInputPort(index, port =>
            {
                port.Name = name;
            });
        }

        protected void SetInputPort(int index, Type type)
        {
            SetInputPort(index, port =>
            {
                port.Type = type;
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

        protected void DeleteInputPort(int index)
        {
            if (index < 0 || index > CustomInputPorts.Count - 1)
            {
                return;
            }

            CustomInputPorts.RemoveAt(index);
        }

        protected void DeleteInputPortRange(int index, int count)
        {
            if (index < 0 || index > CustomInputPorts.Count - 1)
            {
                return;
            }

            CustomInputPorts.RemoveRange(index, count);
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

        protected void DeleteOutputPort(int index)
        {
            if (index < 0 || index > CustomOutputPorts.Count - 1)
            {
                return;
            }

            CustomOutputPorts.RemoveAt(index);
        }

        protected void DeleteOutputPortRange(int index, int count)
        {
            if (index < 0 || index > CustomOutputPorts.Count - 1)
            {
                return;
            }

            CustomOutputPorts.RemoveRange(index, count);
        }
        */
        #endregion

        #region UpdateNodeView()
        public delegate void UpdateNodeViewEvent();

        /// <summary>
        /// Updates this Node View and applies changes such as the title and any changed ports.
        /// </summary>
        protected virtual void UpdateNodeView() => NodeView.UpdateNodeView();
        #endregion

        #region IMGUI Drawing
        private IMGUIContainer _IMGUIContainer;
        protected IMGUIContainer IMGUIContainer
        {
            get
            {
                if (_IMGUIContainer == null)
                {
                    _IMGUIContainer = new IMGUIContainer(IMGUIOnGUIHandler);
                }

                return _IMGUIContainer;
            }
        }
        /// <summary>
        /// The method that <see cref="IMGUIContainer"/> will call everytime it needs to draw it's inspector. <para/>
        /// Override this to change which method is used 
        /// </summary>
        protected virtual Action IMGUIOnGUIHandler => OnInspectorGUI;

        private Editor _editor;
        protected Editor editor
        {
            get
            {
                if (_editor == null)
                {
                    _editor = Editor.CreateEditor(Node);
                }

                return _editor;
            }
        }
        #endregion

        #region Container Properties
        /// <summary>
        /// Empty container used to display custom elements. After adding elements, call
        /// RefreshExpandedState on the <see cref="NodeView"/> in order to toggle this container visibility.
        /// </summary>
        public VisualElement extensionContainer => NodeView.extensionContainer;

        /// <summary>
        /// Entire top area containing input and output containers.
        /// </summary>
        public VisualElement topContainer => NodeView.topContainer;

        /// <summary>
        /// Title bar button container. Contains the top right buttons.
        /// </summary>
        public VisualElement titleButtonContainer => NodeView.titleButtonContainer;

        /// <summary>
        /// Outputs container, used for output ports.
        /// </summary>
        public VisualElement outputContainer => NodeView.outputContainer;

        /// <summary>
        /// Input container used for input ports.
        /// </summary>
        public VisualElement inputContainer => NodeView.inputContainer;

        /// <summary>
        /// Title bar container.
        /// </summary>
        public VisualElement titleContainer => NodeView.titleContainer;

        /// <summary>
        /// Main container that includes all other containers.
        /// </summary>
        public VisualElement mainContainer => NodeView.mainContainer;
        #endregion

        #region Serialized Object fields & methods
        protected SerializedObject serializedObject => editor.serializedObject;

        protected SerializedProperty FindProperty(string name) => serializedObject.FindProperty(name);
        #endregion

        #region OnRename
        /// <summary>
        /// Called whenever a Parameter in the editor has been renamed.
        /// </summary>
        /// <param name="parameterType">The type of the Parameter that was renamed.</param>
        /// <param name="oldName">The old name that the Parameter had.</param>
        /// <param name="newName">The new name that the Parameter now has.</param>
        public virtual void OnRenameParameter(Type parameterType, string oldName, string newName)
        {

        }

        /// <summary>
        /// Called whenever a Tag in the editor has been renamed.
        /// </summary>
        /// <param name="oldName">The old name that the Tag had.</param>
        /// <param name="newName">The new name that the Tag now has.</param>
        public virtual void OnRenameTag(string oldName, string newName)
        {

        }

        /// <summary>
        /// Called whenever a Event in the editor has been renamed.
        /// </summary>
        /// <param name="eventType">The type of the Event that was renamed.</param>
        /// <param name="oldName">The old name that the Event had.</param>
        /// <param name="newName">The new name that the Event now has.</param>
        public virtual void OnRenameEvent(GorkGraph.Event.Type eventType, string oldName, string newName)
        {

        }
        #endregion

        /// <summary>
        /// Is called when this editor is created. <para/>
        /// Use this as your Start method for when you should setup some variables in your own custom editor.
        /// </summary>
        public virtual void SetupEditor()
        {

        }

        /// <summary>
        /// Override this method for total control over the way that this <see cref="GorkNode"/> is drawn in the editor.
        /// </summary>
        public virtual void SetupDraw(Node node)
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
        /// This should be more than enough for customizability, but if you want the ability to modify even more things, then override <see cref="SetupDraw(Node)"/>.
        /// </summary>
        protected virtual void Draw(List<VisualElement> elements)
        {
            elements.Add(IMGUIContainer);
        }

        /// <summary>
        /// Is called when the Node View is enabled. NOTE: This is called after <see cref="SetupDraw(Node)"/>
        /// </summary>
        public virtual void OnViewEnable()
        {

        }

        #region Expanding & Collapsing Methods
        /// <summary>
        /// Is called when the Node View is collapsed.
        /// </summary>
        public virtual void OnCollapse()
        {

        }

        /// <summary>
        /// Is called when the Node View is expanded.
        /// </summary>
        public virtual void OnExpand()
        {

        }
        #endregion

        #region Connection Events

        #region Connection Properties
        /// <summary>
        /// The <see cref="SerializedProperty"/> for <see cref="GorkNode.InputConnections"/>.
        /// </summary>
        public SerializedProperty InputConnectionsProp
        {
            get
            {
                if (_inputConnectionsProp == null)
                {
                    _inputConnectionsProp = FindProperty("_inputConnections");
                }

                return _inputConnectionsProp;
            }
        }
        private SerializedProperty _inputConnectionsProp;

        /// <summary>
        /// The <see cref="SerializedProperty"/> for <see cref="GorkNode.OutputConnections"/>.
        /// </summary>
        public SerializedProperty OutputConnectionsProp
        {
            get
            {
                if (_outputConnectionsProp == null)
                {
                    _outputConnectionsProp = FindProperty("_outputConnections");
                }

                return _outputConnectionsProp;
            }
        }
        private SerializedProperty _outputConnectionsProp;
        #endregion

        #region Connection Methods
        /// <summary>
        /// Returns a list of <see cref="GorkNode.Connection"/> 
        /// </summary>
        /// <param name="connections"></param>
        /// <param name="index"></param>
        /// <param name="canFail"></param>
        /// <returns></returns>
        public SerializedProperty GetConnectionsProperty(SerializedProperty connections, int index, bool canFail = false)
        {
            // Store the count of the connections list
            int count = connections.arraySize - 1;

            // Check if the index is larger than the array size (meaning we are out of range)
            if (count < index)
            {
                // If so, then we check if the canFail boolean is set to true
                // This boolean determines if this method can fail (return null) in case the index is out of range
                if (canFail)
                {
                    return null;
                }

                // If canFail is false however, then we expand the list to fit our new index so the method will never fail
                for (int i = count; i < index; i++)
                {
                    // Add the new empty array element to the end of the list
                    int arrayElementIndex = connections.arraySize;
                    connections.InsertArrayElementAtIndex(arrayElementIndex);

                    connections.GetArrayElementAtIndex(arrayElementIndex).FindPropertyRelative("Connections").ClearArray();
                }
            }

            // Return the connections list "Connections" property which is a list inside of the connections property (WOAH! A 2D list!!)
            return connections.GetArrayElementAtIndex(index).FindPropertyRelative("Connections");
        }

        /// <summary>
        /// Will add a connection with the given parameters on the given <paramref name="connections"/> <see cref="SerializedProperty"/>.
        /// </summary>
        public void AddConnection(SerializedProperty connections, int portIndex, GorkNode otherNode, int otherNodePortIndex)
        {
            SerializedProperty prop = GetConnectionsProperty(connections, portIndex);

            // Add connection
            int index = prop.arraySize;
            prop.InsertArrayElementAtIndex(index);

            SerializedProperty arrayElement = prop.GetArrayElementAtIndex(index);

            arrayElement.FindPropertyRelative("PortIndex").intValue = otherNodePortIndex;
            arrayElement.FindPropertyRelative("Node").objectReferenceValue = otherNode;
        }

        /// <summary>
        /// Will remove a connection that matches the given parameters on the given <paramref name="connections"/> <see cref="SerializedProperty"/>.
        /// </summary>
        public void RemoveConnection(SerializedProperty connections, int portIndex, GorkNode otherNode, int otherNodePortIndex)
        {
            // Get the connections property
            // Notice how CanFail is set to true. This means that the result can be null which we don't mind in this case
            SerializedProperty prop = GetConnectionsProperty(connections, portIndex, true);

            // The connections property doesn't exist as we set CanFail to true
            if (prop == null)
            {
                return;
            }

            // Remove the connection with the correct values
            int arraySize = prop.arraySize;

            // There are no elements in the array
            if (arraySize <= 0)
            {
                return;
            }

            // Loop through the entire array until we find the matching connection that we want to remove
            for (int i = 0; i < arraySize; i++)
            {
                // Get the array element at the current index
                SerializedProperty arrayElement = prop.GetArrayElementAtIndex(i);

                // Continue if it's not the correct Node reference
                if (arrayElement.FindPropertyRelative("Node").objectReferenceValue != otherNode)
                {
                    continue;
                }

                // Continue if it's not the correct port index
                if (arrayElement.FindPropertyRelative("PortIndex").intValue != otherNodePortIndex)
                {
                    continue;
                }

                // We have found the correct connection!
                // Delete the connection and return
                prop.DeleteArrayElementAtIndex(i);
                return;
            }

            // No connection matched :(
        }
        #endregion

        /// <summary>
        /// Adds a new connection to the <see cref="InputConnectionsProp"/> and calls <see cref="SerializedObject.ApplyModifiedProperties"/> if <paramref name="applyProperties"/> is true.
        /// </summary>
        public void AddInputConnection(int portIndex, GorkNode otherNode, int otherNodePortIndex, bool applyProperties = true)
        {
            AddConnection(InputConnectionsProp, portIndex, otherNode, otherNodePortIndex);

            if (applyProperties && serializedObject.ApplyModifiedProperties())
            {
                Undo.SetCurrentGroupName("Added connection in Graph: \"" + Graph.name +"\"");
            }

            OnInputConnectionAdded(portIndex, otherNode, otherNodePortIndex);
        }

        /// <summary>
        /// Called when an input port connection is added to this node. <para/>
        /// Override to add custom behaviour when our node is connected to something. <para/>
        /// NOTE: This is called after the actual <see cref="InputPorts"/> collection is modified.
        /// </summary>
        public virtual void OnInputConnectionAdded(int portIndex, GorkNode otherNode, int otherNodePortIndex)
        {

        }

        /// <summary>
        /// Adds a new connection to the <see cref="OutputConnectionsProp"/> and calls <see cref="SerializedObject.ApplyModifiedProperties"/> if <paramref name="applyProperties"/> is true.
        /// </summary>
        public void AddOutputConnection(int portIndex, GorkNode otherNode, int otherNodePortIndex, bool applyProperties = true)
        {
            AddConnection(OutputConnectionsProp, portIndex, otherNode, otherNodePortIndex);

            if (applyProperties && serializedObject.ApplyModifiedProperties())
            {
                Undo.SetCurrentGroupName("Added connection in Graph: \"" + Graph.name + "\"");
            }

            OnOutputConnectionAdded(portIndex, otherNode, otherNodePortIndex);
        }

        /// <summary>
        /// Called when an output port connection is added to this node. <para/>
        /// Override to add custom behaviour when our node is connected to something. <para/>
        /// NOTE: This is called after the actual <see cref="OutputPorts"/> collection is modified.
        /// </summary>
        public virtual void OnOutputConnectionAdded(int portIndex, GorkNode otherNode, int otherNodePortIndex)
        {

        }

        /// <summary>
        /// Removes a connection from the <see cref="InputConnectionsProp"/> and calls <see cref="SerializedObject.ApplyModifiedProperties"/> if <paramref name="applyProperties"/> is true.
        /// </summary>
        public void RemoveInputConnection(int portIndex, GorkNode otherNode, int otherNodePortIndex, bool applyProperties = true)
        {
            RemoveConnection(InputConnectionsProp, portIndex, otherNode, otherNodePortIndex);

            if (applyProperties && serializedObject.ApplyModifiedProperties())
            {
                Undo.SetCurrentGroupName("Removed connection in Graph: \"" + Graph.name + "\"");
            }

            OnInputConnectionRemoved(portIndex, otherNode, otherNodePortIndex);
        }

        /// <summary>
        /// Called when an input port connection is removed from this node. <para/>
        /// Override to add custom behaviour when our node is disconnected from something. <para/>
        /// NOTE: This is called after the actual <see cref="InputPorts"/> collection is modified.
        /// </summary>
        public virtual void OnInputConnectionRemoved(int portIndex, GorkNode otherNode, int otherNodePortIndex)
        {

        }

        /// <summary>
        /// Removes a connection from the <see cref="OutputConnectionsProp"/> and calls <see cref="SerializedObject.ApplyModifiedProperties"/> if <paramref name="applyProperties"/> is true.
        /// </summary>
        public void RemoveOutputConnection(int portIndex, GorkNode otherNode, int otherNodePortIndex, bool applyProperties = true)
        {
            RemoveConnection(OutputConnectionsProp, portIndex, otherNode, otherNodePortIndex);

            if (applyProperties && serializedObject.ApplyModifiedProperties())
            {
                Undo.SetCurrentGroupName("Removed connection in Graph: \"" + Graph.name + "\"");
            }

            OnOutputConnectionRemoved(portIndex, otherNode, otherNodePortIndex);
        }

        /// <summary>
        /// Called when an output port connection is removed from this node. <para/>
        /// Override to add custom behaviour when our node is disconnected from something. <para/>
        /// NOTE: This is called after the actual <see cref="OutputPorts"/> collection is modified.
        /// </summary>
        public virtual void OnOutputConnectionRemoved(int portIndex, GorkNode otherNode, int otherNodePortIndex)
        {

        }
        #endregion

        #region Connection Methods
        /// <summary>
        /// Will return if the input port with the given <paramref name="index"/> has any connections attached.
        /// </summary>
        public bool HasInputConnection(int index) => Node.HasInputConnection(index);

        /// <summary>
        /// Will return if the output port with the given <paramref name="index"/> has any connections attached.
        /// </summary>
        public bool HasOutputConnection(int index) => Node.HasOutputConnection(index);

        #endregion

        public virtual float InspectorLabelWidth => 50;
        public virtual float InspectorFieldWidth => 150;

        /// <summary>
        /// Override this method to draw custom inspector elements on the <see cref="GorkNode"/> using Unitys IMGUI system. <para/>
        /// Use this like you would in any other <see cref="Editor"/> script. However, if you want the ability to modify more things, then override <see cref="Draw"/>.
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
                if (property.propertyPath == "m_Script")
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

        #region Get Editor
        public static GorkNodeEditor GetEditor(GorkNode node)
        {
            return GetEditor(node.GetType());
        }

        public static GorkNodeEditor GetEditor<T>() where T : GorkNode
        {
            return GetEditor(typeof(T));
        }

        public static GorkNodeEditor GetEditor(Type nodeType)
        {
            //GorkNodeEditor editor;

            if (!CustomGorkNodeEditorAttribute.AssignedEditorTypes.ContainsKey(nodeType))
            {
                return new GorkNodeEditor();
                /*
                editor = new GorkNodeEditor();

                editor.Subscribe();

                return editor;
                */
            }

            Type editorType = CustomGorkNodeEditorAttribute.AssignedEditorTypes[nodeType];

            return Activator.CreateInstance(editorType) as GorkNodeEditor;
            /*
            editor = Activator.CreateInstance(editorType) as GorkNodeEditor;

            editor.Subscribe();

            return editor;
            */
        }
        #endregion

        /// <summary>
        /// Called whenever this node editor is created. Makes this editor subscribe to all events necessary.
        /// </summary>
        public void Subscribe()
        {
            GraphView.Inspector.OnRenameParameter += OnRenameParameter;
            GraphView.Inspector.OnRenameTag += OnRenameTag;
            GraphView.Inspector.OnRenameEvent += OnRenameEvent;
        }

        /// <summary>
        /// Called whenever this node editor is destroyed. Makes this editor unsubscribe to all events necessary.
        /// </summary>
        public void Unsubscribe()
        {
            GraphView.Inspector.OnRenameParameter -= OnRenameParameter;
            GraphView.Inspector.OnRenameTag -= OnRenameTag;
            GraphView.Inspector.OnRenameEvent -= OnRenameEvent;
        }

        public GorkNodeEditor()
        {

        }

        ~GorkNodeEditor()
        {
            Unsubscribe();
        }
    }
}
