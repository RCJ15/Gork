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
    public class GorkNodeView : Node, IDeletionCallback
    {
        private static readonly Color InspectorBackgroundColor = new Color(0.159607f, 0.159607f, 0.159607f, 0.7f);

        public GorkNode Node { get; private set; }
        public GorkNodeEditor NodeEditor { get; private set; }
        public GorkGraphView GraphView { get; private set; }
        public GorkMenuItemAttribute Attribute { get; private set; }

        private Type _nodeType;

        //-- Ports
        public List<GorkPort> InputPorts = new List<GorkPort>();
        public List<GorkPort> OutputPorts = new List<GorkPort>();

        //-- Question Mark Button
        private static VisualTreeAsset _questionMarkButtonVisualTree = null;
        private VisualElement _questionMarkButton;
        //private bool _questionMarkButtonAdded = true;

        private static readonly Color _buttonSelectedColor = new Color(0.168627f, 0.168627f, 0.168627f);

        //-- Tag Display
        private VisualElement _tagDisplay;
        private ScrollView _scrollView;
        private static VisualTreeAsset _tagDisplayVisualTree = null;
        private static VisualTreeAsset _tagLabelTemplate = null;
        private static VisualTreeAsset _tagLabelArrowTemplate = null;
        
        private static readonly Color _tagArrowSelectedColor = new Color(0.4f, 0.4f, 0.4f, 1f);
        private static readonly Color _tagArrowUnselectedColor = new Color(0.254901f, 0.254901f, 0.254901f, 1);

        private readonly List<Label> _currentTags = new List<Label>();
        private int _tagCount;
        private readonly Dictionary<string, (VisualElement, Label)> _tagObjects = new Dictionary<string, (VisualElement, Label)>();
        private readonly Dictionary<Label, int> _tagIndices = new Dictionary<Label, int>();
        private readonly Queue<Label> _availableTags = new Queue<Label>();

        public GorkNodeView(GorkNode node, GorkGraphView graphView, List<FieldData> fieldData = null)
        {
            // Set data for this node
            Node = node;
            GraphView = graphView;
            viewDataKey = Node.GUID;

            // Cache the node type
            _nodeType = Node.GetType();

            Attribute = GorkMenuItemAttribute.TypeAttributes[_nodeType];

            Color? color = Attribute.GetColor();

            if (color.HasValue)
            {
                titleContainer.style.backgroundColor = color.Value;
            }

            // Create and setup the editor for the node
            NodeEditor = GorkNodeEditor.GetEditor(_nodeType);
            NodeEditor.Node = node;
            NodeEditor.Graph = graphView.Graph;
            NodeEditor.GraphView = graphView;
            NodeEditor.NodeView = this;
            NodeEditor.SetTitle(string.IsNullOrEmpty(Node.Title) ? Attribute.DisplayName : Node.Title, false);
            NodeEditor.SetupEditor();
            NodeEditor.Subscribe();

            UpdateTitle();

            #region Load Visual Tree Assets
            // Question Mark Button
            if (_questionMarkButtonVisualTree == null)
            {
                _questionMarkButtonVisualTree = GorkGraphEditor.GetVisualTree("QuestionMarkButton");
            }

            // Tags
            if (_tagDisplayVisualTree == null)
            {
                _tagDisplayVisualTree = GorkGraphEditor.GetVisualTree("TagDisplay");
            }
            if (_tagLabelTemplate == null)
            {
                _tagLabelTemplate = GorkGraphEditor.GetVisualTree("TagLabel");
            }
            if (_tagLabelArrowTemplate == null)
            {
                _tagLabelArrowTemplate = GorkGraphEditor.GetVisualTree("TagLabelArrows");
            }
            #endregion

            #region Question Mark Button
            // Create the question mark button which will open up the gork wiki page for this node type
            _questionMarkButtonVisualTree.CloneTree(titleButtonContainer);
            _questionMarkButton = titleButtonContainer.Q<VisualElement>("QuestionMarkButton");
            VisualElement icon = _questionMarkButton.Q<VisualElement>("Icon");

            icon.style.opacity = 0.5f;

            // Open up the gork wiki when the question mark button is pressed
            _questionMarkButton.AddManipulator(new Clickable(() =>
            {
                // TODO
                /*
                if (_attribute == null || !_attribute.HasWiki)
                {
                    return;
                }

                GorkWikiWindow.OpenNodePage(_attribute);
                */
            }));

            // Update the question mark button visually when the mouse enters/leaves the button
            _questionMarkButton.RegisterCallback<MouseEnterEvent>(evt =>
            {
                icon.style.backgroundColor = _buttonSelectedColor;
                icon.style.opacity = 1;
            });

            _questionMarkButton.RegisterCallback<MouseLeaveEvent>(evt =>
            {
                icon.style.backgroundColor = Color.clear;
                icon.style.opacity = 0.5f;
            });

            // Make the question mark button visible only when the mouse is on top of this node
            _questionMarkButton.visible = false;

            RegisterCallback<MouseEnterEvent>(evt =>
            {
                _questionMarkButton.visible = true;
            });

            RegisterCallback<MouseLeaveEvent>(evt =>
            {
                _questionMarkButton.visible = false;
            });
            #endregion

            #region Tag Display
            _tagDisplay = contentContainer.Q<VisualElement>("divider");

            // Clone the tag visual tree
            _tagDisplayVisualTree.CloneTree(_tagDisplay);

            _scrollView = _tagDisplay.Q<ScrollView>("TagsContainer");

            _scrollView.generateVisualContent += _ => UpdateTagSize();
            #endregion

            // Set our extension container background
            extensionContainer.style.backgroundColor = InspectorBackgroundColor;

            UpdatePosition(Node.Position);

            BuildNode();

            if (fieldData != null)
            {
                LoadFromFieldData(fieldData);
            }

            // Load tags
            foreach (string tag in node.Tags)
            {
                AddTag(tag, false, false);
            }

            UpdateTagSize();

            InitializeNode();
        }

        public void SetExpanded()
        {
            expanded = Node.Expanded;

            RefreshExpandedState();
        }

        private void BuildNode()
        {
            Node.RefreshPorts();

            // Create the needed ports
            InputPorts.Clear();

            foreach (NodePort inputPort in Node.InputPorts)
            {
                AddInputPort(inputPort.Name, inputPort.Type);
            }

            OutputPorts.Clear();

            foreach (NodePort outputPort in Node.OutputPorts)
            {
                AddOutputPort(outputPort.Name, outputPort.Type);
            }
        }

        private void InitializeNode()
        {
            // Initialize view
            NodeEditor.SetupDraw(this);
            NodeEditor.OnViewEnable();

            UpdateNodeView();
        }

        private void ClearNode()
        {
            inputContainer.Clear();
            outputContainer.Clear();
            extensionContainer.Clear();
        }

        /*
        private void ChangeAttribute(int attributeID, bool rebuildNode = true)
        {
            if (_attributeList == null)
            {
                _attributeID = 0;

                _attribute = null;
                return;
            }

            _attributeID = attributeID;

            _attribute = _attributeList[attributeID];

            // Save attribute ID
            Node.AttributeID = _attributeID;

            // Set background color
            Color? color = _attribute.GetColor();

            if (color.HasValue)
            {
                titleContainer.style.backgroundColor = color.Value;
            }

            // Set title
            title = _attribute.NodeName;

            // Update the question mark button
            if (!_questionMarkButtonAdded && _attribute.HasWiki)
            {
                titleButtonContainer.Add(_questionMarkButton);
                _questionMarkButtonAdded = true;
            }
            else if (_questionMarkButtonAdded && !_attribute.HasWiki)
            {
                titleButtonContainer.Remove(_questionMarkButton);
                _questionMarkButtonAdded = false;
            }

            // Also update the tooltip
            _questionMarkButton.tooltip = _attribute.WikiSummary;

            if (!rebuildNode)
            {
                return;
            }

            ClearNode();
            BuildNode();

            Node.Title = title;

            InitializeNode();
        }
        */

        protected override void ToggleCollapse()
        {
            base.ToggleCollapse();

            UpdateTagSize();

            Node.Expanded = expanded;

            if (expanded)
            {
                NodeEditor.OnExpand();
            }
            else
            {
                NodeEditor.OnCollapse();
            }

            RefreshExpandedState();
        }

        #region Tags
        private (VisualElement, Label) GetTagObject()
        {
            VisualElement newTag;

            Label tagLabel;

            if (_availableTags.TryDequeue(out tagLabel))
            {
                newTag = tagLabel.parent;
            }
            else
            {
                newTag = new VisualElement();
                _tagLabelTemplate.CloneTree(newTag);

                tagLabel = newTag.Q<Label>("TagLabel");

                _tagLabelArrowTemplate.CloneTree(newTag);

                VisualElement tagArrows = newTag.Q<VisualElement>("TagLabelArrows");

                VisualElement leftArrow = tagArrows.Q<Label>("LeftArrow");
                VisualElement rightArrow = tagArrows.Q<Label>("RightArrow");

                leftArrow.visible = false;
                rightArrow.visible = false;

                // Change visiblity of the right and left arrows when the mouse enters/leaves the button
                newTag.RegisterCallback<MouseEnterEvent>(evt =>
                {
                    if (_tagCount <= 1)
                    {
                        return;
                    }

                    int index = _tagIndices[tagLabel];

                    if (index > 0)
                    {
                        leftArrow.visible = true;
                    }
                    if (index < _tagCount - 1)
                    {
                        rightArrow.visible = true;
                    }
                });

                newTag.RegisterCallback<MouseLeaveEvent>(evt =>
                {
                    leftArrow.visible = false;
                    rightArrow.visible = false;
                });

                #region Arrow Clickable
                // Clickable arrow
                leftArrow.AddManipulator(new Clickable(() =>
                {
                    ChangeTagIndex(tagLabel, _tagIndices[tagLabel] - 1);
                }));

                rightArrow.AddManipulator(new Clickable(() =>
                {
                    ChangeTagIndex(tagLabel, _tagIndices[tagLabel] + 1);
                }));

                // Change the background color of the arrows when the mouse hovers over them
                // Left arrow
                leftArrow.RegisterCallback<MouseEnterEvent>(evt =>
                {
                    leftArrow.style.backgroundColor = _tagArrowSelectedColor;
                });

                leftArrow.RegisterCallback<MouseLeaveEvent>(evt =>
                {
                    leftArrow.style.backgroundColor = _tagArrowUnselectedColor;
                });

                // Right arrow
                rightArrow.RegisterCallback<MouseEnterEvent>(evt =>
                {
                    rightArrow.style.backgroundColor = _tagArrowSelectedColor;
                });

                rightArrow.RegisterCallback<MouseLeaveEvent>(evt =>
                {
                    rightArrow.style.backgroundColor = _tagArrowUnselectedColor;
                });
                #endregion
            }

            newTag.AddManipulator(new ContextualMenuManipulator(evt =>
            {
                DropdownMenu menu = evt.menu;

                menu.MenuItems()?.Clear(); // Clear

                menu.AppendAction("Delete", _ => RemoveTag(tagLabel.text));
            }));

            return (newTag, tagLabel);
        }

        public void AddTag(string tag, bool addToNode = true, bool updateTagSize = true)
        {
            if (_tagObjects.ContainsKey(tag))
            {
                return;
            }

            (VisualElement newTag, Label tagLabel) = GetTagObject();

            tagLabel.text = tag;

            _currentTags.Add(tagLabel);
            _tagIndices[tagLabel] = _currentTags.IndexOf(tagLabel);
            _tagCount++;

            _scrollView.Add(newTag);

            _tagObjects[tag] = (newTag, tagLabel);

            if (addToNode)
            {
                Node.AddTag(tag);
            }

            if (updateTagSize)
            {
                UpdateTagSize();
            }
        }

        public void RemoveTag(string tag)
        {
            if (!_tagObjects.TryGetValue(tag, out var objects))
            {
                return;
            }

            int index = _tagIndices[objects.Item2];

            _scrollView.Remove(objects.Item1);
            _currentTags.Remove(objects.Item2);
            _tagIndices.Remove(objects.Item2);
            _tagObjects.Remove(tag);

            _tagCount--;

            if (_tagCount > 0)
            {
                for (int i = index - 1; i < _tagCount; i++)
                {
                    _tagIndices[_currentTags[i]] = i;
                }
            }

            Node.RemoveTag(tag);

            _availableTags.Enqueue(objects.Item2);

            UpdateTagSize();
        }

        public void UpdateTagSize()
        {
            if (!expanded || !GorkEditorSaveData.DisplayTags || Node.Tags.Count <= 0)
            {
                _tagDisplay.style.height = 0;
                return;
            }

            _tagDisplay.style.height = _scrollView.horizontalScroller.style.display == DisplayStyle.Flex ? 35 : 20;
        }

        public void ChangeTagIndex(Label tagLabel, int newIndex)
        {
            int oldIndex = _tagIndices[tagLabel];

            Label newTagLabel = _currentTags[newIndex];

            string oldTag = Node.Tags[oldIndex];
            string newTag = Node.Tags[newIndex];

            newTagLabel.text = oldTag;
            tagLabel.text = newTag;

            Undo.RecordObject(Node, $"Changed location of tags \"{oldTag}\" and \"{newTag}\"");

            Node.Tags[oldIndex] = newTag;
            Node.Tags[newIndex] = oldTag;
        }
        #endregion

        public void UpdatePosition(Vector2 pos)
        {
            style.left = pos.x;
            style.top = pos.y;
        }

        public void UpdateTitle()
        {
            title = NodeEditor.Title;
        }

        public void UpdateNodeView()
        {
            int connectionCount = Node.OutputConnections.Count;

            int oldInputPortAmount = InputPorts.Count;
            int oldOutputPortAmount = OutputPorts.Count;

            int newInputPortAmount = Node.InputPorts.Count;
            int newOutputPortAmount = Node.OutputPorts.Count;

            void FillPorts(bool isInput, int newPortAmount, int oldPortAmount, NodePortCollection portInfos)
            {
                List<GorkPort> list = isInput ? InputPorts : OutputPorts;

                // Add new ports
                if (newPortAmount > oldPortAmount)
                {
                    for (int i = oldPortAmount; i < newPortAmount; i++)
                    {
                        var portInfo = portInfos[i];

                        GorkPort port;

                        if (isInput)
                        {
                            port = AddInputPort(portInfo.Name, portInfo.Type);
                        }
                        else
                        {
                            port = AddOutputPort(portInfo.Name, portInfo.Type);
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
                    }
                }
                // Remove ports
                else if (newPortAmount < oldPortAmount)
                {
                    VisualElement container = isInput ? inputContainer : outputContainer;

                    // Do in reverse order
                    for (int i = oldPortAmount - 1; i >= newPortAmount; i--)
                    {
                        GorkPort port = list[i];
                        list.RemoveAt(i);

                        if (port == null)
                        {
                            continue;
                        }

                        List<Edge> edgeList = new List<Edge>();
                        int edgeAmount = 0;

                        foreach (Edge edge in port.connections)
                        {
                            edgeList.Add(edge);
                            edgeAmount++;
                        }

                        for (int index = 0; index < edgeAmount; index++)
                        {
                            Edge edge = edgeList[index];

                            GorkPort inputPort = edge.input as GorkPort;
                            GorkPort outputPort = edge.output as GorkPort;

                            inputPort.Disconnect(edge);
                            outputPort.Disconnect(edge);

                            GraphView.RemoveElement(edge);
                        }

                        container.Remove(port);
                    }
                }

                // Update the types, names and color of all ports
                int listCount = list.Count;
                for (int i = 0; i < listCount; i++)
                {
                    var portInfo = portInfos[i];
                    GorkPort port = list[i];

                    // Default the type to the signal type if it's null
                    port.portType = portInfo.Type == null ? GorkUtility.SignalType : portInfo.Type;
                    port.portName = portInfo.Name;

                    port.UpdateColor();
                }

                //--
            }

            FillPorts(true, newInputPortAmount, oldInputPortAmount, Node.InputPorts);
            FillPorts(false, newOutputPortAmount, oldOutputPortAmount, Node.OutputPorts);

            RefreshExpandedState();
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

        /*
        public void AddAttributeContextMenu(DropdownMenu menu)
        {
            if (_attributeList == null || _attributeCount <= 1)
            {
                return;
            }

            // Add seperator
            menu.AppendSeparator();

            // Loop through attribute list
            for (int i = 0; i < _attributeCount; i++)
            {
                int cachedIndex = i;

                menu.AppendAction($"Change Variant/{_attributeList[i].NodeName}", _ =>
                {
                    DisconnectAll();
                    ChangeAttribute(cachedIndex);
                }, i == _attributeID ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
            }
        }
        */

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
                OnDeletion();
            }
        }

        public void OnDeletion()
        {
            NodeEditor.Unsubscribe();
        }

        /// <summary>
        /// Copied from: https://github.com/Unity-Technologies/UnityCsReference/blob/3fcad4bbbea0455588843799c67861f7b9eb3825/Modules/GraphViewEditor/Elements/Node.cs <para/>
        /// Will add all <see cref="Edge"/> connections on the <paramref name="container"/> to the <paramref name="hashSet"/>.
        /// </summary>
        private void AddConnectionsToHashSet(VisualElement container, ref HashSet<Edge> hashSet)
        {
            hashSet.UnionWith(GetConnections(container));
        }

        public List<Edge> GetConnections(VisualElement container)
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
        public GorkPort AddInputPort(string name, Type type)
        {
            // Create port
            GorkPort port = GorkPort.CreateInputPort(type, this);

            port.portName = name;

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
        public GorkPort AddOutputPort(string name, Type type)
        {
            // Create port
            GorkPort port = GorkPort.CreateOutputPort(type, this);

            port.portName = name;

            // Add to list
            port.PortIndex = OutputPorts.Count;
            OutputPorts.Add(port);

            // Add to container
            outputContainer.Add(port);

            // Return the port for further use
            return port;
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
            //AttributeID = _attributeID,
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

            NodeEditor.OnViewEnable();
            UpdateNodeView();
        }

        [Serializable]
        public class PasteData
        {
            public string GUID;
            public Vector2 Position;
            public string NodeType;
            //public int AttributeID;
            public List<FieldData> FieldData;
        }

        #region Field Loading
        private static Dictionary<Type, Dictionary<string, FieldInfo>> _gorkFields = new Dictionary<Type, Dictionary<string, FieldInfo>>();
        private static readonly BindingFlags _bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private static List<FieldInfo> _gorkNodePrivateFields = null;
        private static List<FieldInfo> GorkNodePrivateFields
        {
            get
            {
                if (_gorkNodePrivateFields == null)
                {
                    _gorkNodePrivateFields = new List<FieldInfo>();

                    foreach (FieldInfo field in typeof(GorkNode).GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
                    {
                        _gorkNodePrivateFields.Add(field);
                    }
                }

                return _gorkNodePrivateFields;
            }
        }

        /// <summary>
        /// Returns a dictionary of <see cref="FieldInfo"/> from the <paramref name="gorkType"/> using the <see cref="_gorkFields"/> dictionary.
        /// </summary>
        public static Dictionary<string, FieldInfo> GetFields(Type gorkType)
        {
            // Check if we have NOT cached the Fields into the dictionary yet
            if (!_gorkFields.ContainsKey(gorkType))
            {
                void AddField(FieldInfo field)
                {
                    // Ignore if the field is private and not serialized
                    if (field.IsPrivate && field.GetCustomAttribute<SerializeField>() == null)
                    {
                        return;
                    }

                    // Ignore if the fields type isn't serializable
                    if (!field.FieldType.IsSerializable)
                    {
                        return;
                    }

                    // Ignore if the field has the DontSaveInGorkGraphAttribute
                    if (field.GetCustomAttribute<DontSaveInGorkGraphAttribute>() != null)
                    {
                        return;
                    }

                    if (_gorkFields[gorkType].ContainsKey(field.Name))
                    {
                        return;
                    }

                    // If all of the checks above failed, then this field is ready to be added to the dictionary
                    _gorkFields[gorkType].Add(field.Name, field);
                }

                // If so, then we will create a new dicitonary and start populating it
                _gorkFields[gorkType] = new Dictionary<string, FieldInfo>();

                // Get the fields on the type
                FieldInfo[] fields = gorkType.GetFields(_bindingFlags);

                // Loop through all of our gotten fields
                foreach (FieldInfo field in fields)
                {
                    AddField(field);
                }

                // Also add all of the private fields from the GorkNode type as they cannot be gotten??? 
                foreach (FieldInfo field in GorkNodePrivateFields)
                {
                    AddField(field);
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

                // Turn the object data into string using GorkEditorUtility
                Value = GorkUtility.ToJson(obj, objType);
            }

            /// <summary>
            /// Returns the object value of this <see cref="FieldData"/>.
            /// </summary>
            public object GetValue(Type objType)
            {
                // Deserialize our json by using GorkEditorUtility
                return GorkUtility.FromJson(Value, objType);
            }
        }
        #endregion
        #endregion
    }
}
