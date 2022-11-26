using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEditor;

namespace Gork.Editor
{
    /// <summary>
    /// The class that handles the drawing of every single <see cref="GorkNode"/>.
    /// </summary>
    public class GorkNodeView : Node
    {
        private static readonly Color InspectorBackgroundColor = new Color(0.159607f, 0.159607f, 0.159607f, 0.7f);

        public GorkNode Node;
        public GorkGraphView GraphView;

        private Type _nodeType;

        //-- Attributes
        private GorkNodeInfoAttribute _attribute = null;

        private GorkPortAttribute[] _inputAttributes = null;
        private GorkPortAttribute[] _outputAttributes = null;

        private bool _noInputPorts;
        private bool _noOutputPorts;

        //-- Ports
        public List<GorkPort> InputPorts = new List<GorkPort>();
        public List<GorkPort> OutputPorts = new List<GorkPort>();

        private Label _titleLabel;
        
        public GorkNodeView(GorkNode node, GorkGraphView graphView, List<FieldData> fieldData = null)
        {
            _titleLabel = titleContainer.Q<Label>(name: "title-label");

            // Set data for this node
            Node = node;
            GraphView = graphView;
            viewDataKey = Node.GUID;

            // Cache the node type
            _nodeType = Node.GetType();

            // Get the GorkNodeInfoAttribute using TryGetValue
            if (GorkNodeInfoAttribute.TypeAttributes.TryGetValue(_nodeType, out var value))
            {
                _attribute = value;

                // Set background color
                Color? color = _attribute.GetColor();

                if (color.HasValue)
                {
                    titleContainer.style.backgroundColor = color.Value;
                }
            }

            // Set our extension container background
            extensionContainer.style.backgroundColor = InspectorBackgroundColor;

            // Set title
            title = _attribute != null ? _attribute.NodeName : Node.name;

            UpdatePosition(Node.Position);

            // Get all of the different attributes
            if (GorkInputPortAttribute.Attributes.TryGetValue(_nodeType, out var inputArray))
            {
                _inputAttributes = inputArray;
            }
            else
            {
                _inputAttributes = GorkInputPortAttribute.TemplatePortArray;
            }

            if (GorkOutputPortAttribute.Attributes.TryGetValue(_nodeType, out var outputArray))
            {
                _outputAttributes = outputArray;
            }
            else
            {
                _outputAttributes = GorkOutputPortAttribute.TemplatePortArray;
            }

            _noInputPorts = NoInputPortsAttribute.Attributes.ContainsKey(_nodeType);
            _noOutputPorts = NoOutputPortsAttribute.Attributes.ContainsKey(_nodeType);

            // Create the needed ports
            if (!_noInputPorts)
            {
                Node.InputPorts.Clear();

                foreach (GorkInputPortAttribute attribute in _inputAttributes)
                {
                    AddInputPort(attribute.PortName, attribute.PortType, attribute.DisplayType);
                    Node.InputPorts.Add(new GorkNode.Port(attribute.PortName, attribute.PortType, attribute.DisplayType));
                }
            }

            if (!_noOutputPorts)
            {
                Node.OutputPorts.Clear();

                foreach (GorkOutputPortAttribute attribute in _outputAttributes)
                {
                    AddOutputPort(attribute.PortName, attribute.PortType, attribute.DisplayType);
                    Node.OutputPorts.Add(new GorkNode.Port(attribute.PortName, attribute.PortType, attribute.DisplayType));
                }
            }

            if (GraphView.SubscribedNodes.TryGetValue(Node, out GorkNodeView nodeView))
            {
                Node.UpdateNodeViewCallback -= nodeView.UpdateNodeView;
            }

            Node.UpdateNodeViewCallback += UpdateNodeView;
            GraphView.SubscribedNodes[Node] = this;

            Node.Title = _titleLabel.text;

            if (fieldData != null)
            {
                LoadFromFieldData(fieldData);
            }

            // Initialize view
            Node.Initialize(this);
            Node.OnViewEnable();

            UpdateNodeView();

            /*
            // Add pending nodes from the node view
            if (Node.InputPorts != null && Node.InputPorts.Count > 0)
            {
                Node.InputPorts.ForEach(n =>
                {
                    // Add input port
                    if (n.Direction == Direction.Input)
                    {
                        AddInputPort(n.Name, n.Type);
                    }
                    // Add output port
                    else
                    {
                        AddOutputPort(n.Name, n.Type);
                    }
                });
            }
            */
        }

        public void UpdatePosition(Vector2 pos)
        {
            style.left = pos.x;
            style.top = pos.y;
        }

        private void UpdateNodeView()
        {
            _titleLabel.text = Node.Title;

            int connectionCount = Node.OutputConnections.Count;

            int oldInputPortAmount = InputPorts.Count;
            int oldOutputPortAmount = OutputPorts.Count;

            int inputPortAmount = Node.InputPorts.Count;
            int outputPortAmount = Node.OutputPorts.Count;

            void FillPorts(bool isInput, int portAmount, int oldPortAmount, List<GorkNode.Port> portInfos)
            {
                int loopAmount = Mathf.Max(portAmount, oldPortAmount);
                List<GorkPort> list = isInput ? InputPorts : OutputPorts;
                VisualElement container = isInput ? inputContainer : outputContainer;

                for (int i = 0; i < loopAmount; i++)
                {
                    if (i >= portAmount)
                    {
                        GorkPort port = list[i];
                        list.RemoveAt(i);

                        List<Action> performLater = new List<Action>();

                        foreach (Edge edge in port.connections)
                        {
                            GorkPort inputPort = edge.input as GorkPort;
                            GorkPort outputPort = edge.output as GorkPort;

                            performLater.Add(() =>
                            {
                                inputPort.Disconnect(edge);
                                outputPort.Disconnect(edge);
                            });

                            GraphView.RemoveElement(edge);
                        }

                        foreach (Action action in performLater)
                        {
                            action?.Invoke();
                        }

                        container.Remove(port);

                        Node.RemoveOutputConnections(i);
                        continue;
                    }

                    GorkNode.Port portInfo = portInfos[i];

                    if (i < oldPortAmount)
                    {
                        GorkPort port = list[i];

                        port.portType = portInfo.Type;
                        SetPortName(port, portInfo.Name, portInfo.Type, portInfo.DisplayType);

                        port.UpdateColor();
                    }
                    else
                    {
                        GorkPort port;

                        if (isInput)
                        {
                            port = AddInputPort(portInfo.Name, portInfo.Type, portInfo.DisplayType);
                        }
                        else
                        {
                            port = AddOutputPort(portInfo.Name, portInfo.Type, portInfo.DisplayType);
                        }

                        if (i < connectionCount)
                        {
                            foreach (GorkNode.Connection connection in Node.OutputConnections[i].Connections)
                            {
                                GorkNodeView otherNodeView = GraphView.GetNodeView(connection.Node);

                                if (otherNodeView == null)
                                {
                                    continue;
                                }

                                if (connection.PortIndex >= otherNodeView.InputPorts.Count)
                                {
                                    continue;
                                }

                                GorkPort otherPort = otherNodeView.InputPorts[connection.PortIndex];

                                GorkEdge edge = port.GorkConnectTo(otherPort);
                                GraphView.AddElement(edge);
                            }

                        }

                        //--
                    }
                }
            }

            FillPorts(true, inputPortAmount, oldInputPortAmount, Node.InputPorts);
            FillPorts(false, outputPortAmount, oldOutputPortAmount, Node.OutputPorts);
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            //base.BuildContextualMenu(evt);

            // This is now all handled in GorkGraphView
            // It might sound dumb to do that, but doing so allows for editing of multiple nodes at once
            /*
            // Add commands for disconnecting connections
            evt.menu.AppendAction("Disconnect Input Ports", _ =>
            {
                DisconnectInputPorts();
            });

            evt.menu.AppendAction("Disconnect Output Ports", _ =>
            {
                DisconnectOutputPorts();
            });

            evt.menu.AppendAction("Disconnect All", _ => DisconnectAll());

            // Add seperator
            evt.menu.AppendSeparator();

            // Add delete command
            evt.menu.AppendAction("Delete", _ =>
            {
                Delete();
            });
            */
        }

        public void DisconnectInputPorts()
        {
            GraphView.DeleteElements(GetConnections(inputContainer));
        }

        public void DisconnectOutputPorts()
        {
            GraphView.DeleteElements(GetConnections(outputContainer));
        }

        public void Delete()
        {
            GraphViewChange change = new GraphViewChange();
            change.elementsToRemove = new List<GraphElement>();

            // Add all edges to the elements to remove 
            change.elementsToRemove.AddRange(GetConnections(inputContainer));
            change.elementsToRemove.AddRange(GetConnections(outputContainer));

            // Add this as an element to remove
            change.elementsToRemove.Add(this);

            // Invoke graphViewChanged to update the GraphView
            GraphView.graphViewChanged?.Invoke(change);

            // Remove all of the elements
            foreach (var element in change.elementsToRemove)
            {
                if (element == null)
                {
                    continue;
                }

                GraphView.RemoveElement(element);
            }
        }

        /// <summary>
        /// Copied from: https://github.com/Unity-Technologies/UnityCsReference/blob/3fcad4bbbea0455588843799c67861f7b9eb3825/Modules/GraphViewEditor/Elements/Node.cs <para/>
        /// Will add all <see cref="Edge"/> connections on the <paramref name="container"/> to the <paramref name="hashSet"/>.
        /// </summary>
        private void AddConnectionsToHashSet(VisualElement container, ref HashSet<Edge> hashSet)
        {
            hashSet.UnionWith(GetConnections(container));
        }

        private List<Edge> GetConnections(VisualElement container)
        {
            List<Edge> list = new List<Edge>();

            container.Query<Port>().ForEach(elem =>
            {
                if (elem.connected)
                {
                    foreach (Edge c in elem.connections)
                    {
                        if ((c.capabilities & Capabilities.Deletable) == 0)
                        {
                            continue;
                        }

                        list.Add(c);
                    }
                }
            });

            return list;
        }

        /// <summary>
        /// Copied from: https://github.com/Unity-Technologies/UnityCsReference/blob/3fcad4bbbea0455588843799c67861f7b9eb3825/Modules/GraphViewEditor/Elements/Node.cs <para/>
        /// Will remove all <see cref="Edge"/> connections from this node.
        /// </summary>
        public void DisconnectAll(bool recordUndo = false)
        {
            HashSet<Edge> toDelete = new HashSet<Edge>();

            AddConnectionsToHashSet(inputContainer, ref toDelete);
            AddConnectionsToHashSet(outputContainer, ref toDelete);

            toDelete.Remove(null);

            if (recordUndo)
            {
                foreach (Edge edge in toDelete)
                {
                    if (edge == null)
                    {
                        continue;
                    }

                    if (edge.input.node == this)
                    {
                        if (edge.output.node == null)
                        {
                            continue;
                        }

                        Undo.RecordObject((edge.output.node as GorkNodeView).Node, "Removed Connections");
                    }
                    else
                    {
                        if (edge.input.node == null)
                        {
                            continue;
                        }

                        Undo.RecordObject((edge.input.node as GorkNodeView).Node, "Removed Connections");
                    }
                }
            }

            GraphView.DeleteElements(toDelete);
        }

        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);
            
            Undo.RecordObject(Node, $"Moved \"{title}\" Node");
            Node.Position = newPos.min;
        }

        /// <summary>
        /// Adds an input <see cref="GorkPort"/> to this node and then returns it.
        /// </summary>
        public GorkPort AddInputPort(string name, Type type, bool displayType = true)
        {
            // Create port
            GorkPort port = GorkPort.CreateInputPort(type, this);

            SetPortName(port, name, type, displayType);

            // Add to list
            port.PortIndex = InputPorts.Count;
            InputPorts.Add(port);

            // Add to container
            inputContainer.Add(port);

            // Return the port for further use
            return port;
        }

        /// <summary>
        /// Adds an output <see cref="GorkPort"/> to this node and then returns it.
        /// </summary>
        public GorkPort AddOutputPort(string name, Type type, bool displayType = true)
        {
            // Create port
            GorkPort port = GorkPort.CreateOutputPort(type, this);

            SetPortName(port, name, type, displayType);

            // Add to list
            port.PortIndex = OutputPorts.Count;
            OutputPorts.Add(port);

            // Add to container
            outputContainer.Add(port);

            // Return the port for further use
            return port;
        }

        private static void SetPortName(GorkPort port, string name, Type type, bool displayType = true)
        {
            // Set the name
            port.portName = displayType && type != GorkPortAttribute.DefaultType ? $"{name} ({GetTypeName(type)})" : name;
        }

        private static string GetTypeName(Type type)
        {
            if (type == typeof(int))
            {
                return "Int";
            }
            else if (type == typeof(float))
            {
                return "Float";
            }
            else if (type == typeof(bool))
            {
                return "Bool";
            }
            else
            {
                return type.Name;
            }
        }

        /// <summary>
        /// Simply calls <see cref="List{T}.IndexOf(T)"/> and returns the result.
        /// </summary>
        public int GetInputPortIndex(GorkPort port) => InputPorts.IndexOf(port);

        /// <summary>
        /// Simply calls <see cref="List{T}.IndexOf(T)"/> and returns the result.
        /// </summary>
        public int GetOutputPortIndex(GorkPort port) => OutputPorts.IndexOf(port);

        #region PasteData
        public PasteData GetPasteData() => new PasteData()
        {
            GUID = Node.GUID,
            Position = Node.Position,
            NodeType = _nodeType.AssemblyQualifiedName,
            FieldData = GetFieldData(),
        };

        public void LoadFromFieldData(List<FieldData> fieldData)
        {
            Dictionary<string, FieldInfo> fields = GetFields(_nodeType);

            foreach (FieldData data in fieldData)
            {
                if (!fields.TryGetValue(data.Name, out FieldInfo field))
                {
                    continue;
                }

                field.SetValue(Node, data.GetValue(field.FieldType));
            }

            Node.OnViewEnable();
            UpdateNodeView();
        }

        [Serializable]
        public class PasteData
        {
            public string GUID;
            public Vector2 Position;
            public string NodeType;
            public List<FieldData> FieldData;
        }

        #region Field Loading
        private static Dictionary<Type, Dictionary<string, FieldInfo>> _gorkFields = new Dictionary<Type, Dictionary<string, FieldInfo>>();
        private static readonly BindingFlags _bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        
        /// <summary>
        /// Returns a dictionary of <see cref="FieldInfo"/> from the <paramref name="gorkType"/> using the <see cref="_gorkFields"/> dictionary.
        /// </summary>
        public static Dictionary<string, FieldInfo> GetFields(Type gorkType)
        {
            // Check if we have NOT cached the Fields into the dictionary yet
            if (!_gorkFields.ContainsKey(gorkType))
            {
                // If so, then we will create a new dicitonary and start populating it
                _gorkFields[gorkType] = new Dictionary<string, FieldInfo>();

                // Get the fields on the type
                FieldInfo[] fields = gorkType.GetFields(_bindingFlags);

                // Loop through all of our gotten fields
                foreach (FieldInfo field in fields)
                {
                    // Continue if the field is not serialized
                    if (field.IsNotSerialized)
                    {
                        continue;
                    }

                    // Continue if the field type isn't serializable
                    if (!field.FieldType.IsSerializable)
                    {
                        continue;
                    }

                    // Continue if the field has the DontSaveInGorkGraphAttribute
                    if (field.GetCustomAttribute<DontSaveInGorkGraphAttribute>() != null)
                    {
                        continue;
                    }

                    // If all of the checks above failed, then this field is ready to be added to the dictionary
                    _gorkFields[gorkType].Add(field.Name, field);
                }
            }

            // Return our GorkFields dictionary
            return _gorkFields[gorkType];
        }

        /// <summary>
        /// Returns a list of <see cref="FieldData"/> which can be used to deserialize this object later.
        /// </summary>
        public List<FieldData> GetFieldData()
        {
            // Create an empty list which we will later return
            List<FieldData> data = new List<FieldData>();

            // Get the fields dictionary using GetFields()
            Dictionary<string, FieldInfo> fields = GetFields(_nodeType);

            // Loop through our dictionary
            foreach (var pair in fields)
            {
                // Cache the FieldInfo
                FieldInfo field = pair.Value;

                // Get the value from our Node
                object obj = field.GetValue(Node);

                // Add a new FieldData to our list
                data.Add(new FieldData(pair.Key, obj, field.FieldType));
            }

            // Return the now populated list
            return data;
        }

        /// <summary>
        /// Class which contains serialized a <see cref="FieldInfo"/> on a <see cref="GorkNode"/>. <para/>
        /// This class effectively can convert ANYTHING into a <see cref="string"/> value and then turn it back to it's original value using Reflection and <see cref="JsonUtility"/>.
        /// </summary>
        [Serializable]
        public class FieldData
        {
            // These two static MethodInfos are for when we convert to and from json
            private static readonly MethodInfo _toJsonMethod = typeof(FieldData).GetMethod("ToJson", BindingFlags.Static | BindingFlags.NonPublic);
            private static readonly MethodInfo _fromJsonMethod = typeof(FieldData).GetMethod("FromJson", BindingFlags.Static | BindingFlags.NonPublic);

            // Actual serialized values
            public string Name;
            public string Value;

            /// <summary>
            /// Creates a new <see cref="FieldData"/> with all values set already
            /// </summary>
            public FieldData(string fieldname, object obj, Type objType)
            {
                // Set the name
                Name = fieldname;

                // Turn the object data into string using Reflection and JsonUtility
                MethodInfo genericMethod = _toJsonMethod.MakeGenericMethod(objType);
                Value = genericMethod.Invoke(null, new object[] { obj }) as string;
            }

            /// <summary>
            /// Returns the object value of this <see cref="FieldData"/>.
            /// </summary>
            public object GetValue(Type objType)
            {
                // Deserialize our json by using Reflection once again
                MethodInfo genericMethod = _fromJsonMethod.MakeGenericMethod(objType);

                return genericMethod.Invoke(null, new object[] { Value });
            }

            #region The methods that do all the magic
            /// <summary>
            /// Will convert a <paramref name="obj"/> into json and return the result. <para/>
            /// This is not meant to be used on it's own but is supposed to be used with Reflections <see cref="MethodInfo.MakeGenericMethod(Type[])"/> to turn a generic <see cref="object"/> into a <see cref="string"/>. (Given it's serializable)
            /// </summary>
            private static string ToJson<T>(T obj)
            {
                // Use ValueHolder<T> as just converting the object for some reason doesn't work
                // This is a strange workaround but it works
                string json = JsonUtility.ToJson(new ValueHolder<T>(obj));

                // Cut off the start and ending parts of the json conversion to keep the json clean
                int startLength = ValueHolder<T>.START.Length;
                json = json.Substring(startLength, json.Length - startLength - ValueHolder<T>.END.Length);

                // Return the json
                return json;
            }

            /// <summary>
            /// Will convert a json into a new <see cref="object"/> and return the result. <para/>
            /// This is not meant to be used on it's own but is supposed to be used with Reflections <see cref="MethodInfo.MakeGenericMethod(Type[])"/> to turn a <see cref="string"/> into a generic <see cref="object"/>. (Given it's serializable)
            /// </summary>
            private static T FromJson<T>(string json)
            {
                // Add the start and end parts of the json back. These parts were cut in ToJson<T>()
                json = $"{ValueHolder<T>.START}{json}{ValueHolder<T>.END}";

                // Return the json value
                return JsonUtility.FromJson<ValueHolder<T>>(json).V;
            }
            #endregion

            /// <summary>
            /// Class that a value for <see cref="FieldData"/>. In short, this is used to convert anything serializable into a <see cref="string"/>.
            /// </summary>
            [Serializable]
            public class ValueHolder<T>
            {
                public static readonly string START = $"{{\"{nameof(V)}\":";
                public const string END = "}";

                public T V;

                public ValueHolder(T obj)
                {
                    V = obj;
                }
            }
        }
        #endregion
        #endregion
    }
}
