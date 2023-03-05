using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;

using Object = UnityEngine.Object;

namespace Gork.Editor
{
    /// <summary>
    /// The graph view of Gork Graph. <para/>
    /// Will display all of the nodes, edges and groups in the <see cref="GorkGraphEditor"/> window.
    /// </summary>
    public class GorkGraphView : GraphView
    {
        public new class UxmlFactory : UxmlFactory<GorkGraphView, UxmlTraits> { }

        public GorkGraph Graph;
        public string GraphPath => AssetDatabase.GetAssetPath(Graph);
        private GorkNodeSearchWindow _searchWindow;
        public GorkNodeSearchWindow GorkSearchWindow => _searchWindow;

        private MiniMap _miniMap;
        public MiniMap MiniMap => _miniMap;

        private Vector3 _cachedMousePos;
        public Vector3 MousePos => TransformScreenPos(_cachedMousePos);
        public Vector2 TransformScreenPos(Vector2 screenPos) => viewTransform.matrix.inverse.MultiplyPoint(screenPos);

        public Dictionary<GorkNode, GorkNodeView> SubscribedNodes = new Dictionary<GorkNode, GorkNodeView>();
        public Dictionary<GorkGraph.GroupData, GorkGroup> GorkGroups = new Dictionary<GorkGraph.GroupData, GorkGroup>();

        protected override bool canDeleteSelection => false;

        public Action<GorkGraph> OnOpenGraph;

        public GorkGraphView()
        {
            // Add the grid background
            Insert(0, new GridBackground());

            // Add manipulators
            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            // Add our style sheets
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(GorkGraphEditor.GetPath("uss"));
            styleSheets.Add(styleSheet);

            // Setup search window
            _searchWindow = ScriptableObject.CreateInstance<GorkNodeSearchWindow>();
            _searchWindow.GraphView = this;

            // Open search window when node creation is requested
            //nodeCreationRequest = context => OpenNodeCreationSearchWindow(context);

            viewTransformChanged += _ => SaveScrollAndZoomData();

            #region Create minimap
            _miniMap = new MiniMap()
            {
                anchored = GorkEditorSaveData.MinimapAnchored,
            };

            _miniMap.SetPosition(GorkEditorSaveData.MinimapRect);
            _miniMap.generateVisualContent += _ => SaveMinimapData();

            Add(_miniMap);

            _miniMap.visible = GorkEditorSaveData.DisplayMinimap;

            StyleColor backgroundColor = new StyleColor(new Color32(29, 29, 30, 255));
            StyleColor borderColor = new StyleColor(new Color32(51, 51, 51, 255));

            _miniMap.style.backgroundColor = backgroundColor;
            _miniMap.style.borderTopColor = borderColor;
            _miniMap.style.borderRightColor = borderColor;
            _miniMap.style.borderBottomColor = borderColor;
            _miniMap.style.borderLeftColor = borderColor;
            #endregion

            RegisterCallback<ValidateCommandEvent>(OnValidateCommand);
            RegisterCallback<MouseMoveEvent>(OnMouseMoveEvent);

            // Subscribe to the graphViewChanged callback
            graphViewChanged += OnGraphViewChanged;
        }

        public void OpenNodeCreationSearchWindow(NodeCreationContext context, GorkPort port = null)
        {
            if (Graph == null)
            {
                return;
            }

            _searchWindow.EdgePort = port;

            SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), _searchWindow);
        }

        public void ToggleMiniMap()
        {
            _miniMap.visible = !_miniMap.visible;

            GorkEditorSaveData.DisplayMinimap = _miniMap.visible;
        }
        
        private void OnMouseMoveEvent(MouseMoveEvent evt)
        {
            //_cachedMousePos = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
            _cachedMousePos = evt.localMousePosition;
        }

        private void OnValidateCommand(ValidateCommandEvent evt)
        {
            //Debug.Log(evt.commandName);

            PasteData pasteData = null;
            bool duplicated = false;

            string cmdName = evt.commandName;

            //-- Copy & Paste
            if (cmdName == "Copy")
            {
                CopySelection(ref pasteData);
            }
            else if (cmdName == "Paste")
            {
                //Paste(-contentViewContainer.WorldToLocal(_cachedMousePos));
                //Paste(-_cachedMousePos);
                Paste(MousePos);
            }
            else if (cmdName == "Cut")
            {
                Cut(ref pasteData);
            }
            else if (cmdName == "Duplicate")
            {
                Duplicate(ref pasteData);
                duplicated = true;
            }
            //-- Misc
            // Select All
            else if (cmdName == "SelectAll")
            {
                // Loop through all elements
                foreach (GraphElement element in graphElements)
                {
                    if (selection.Contains(element))
                    {
                        continue;
                    }

                    // Select them all
                    AddToSelection(element);
                }
            }
            // Delete selection
            else if (cmdName == "Delete" || cmdName == "SoftDelete")
            {
                List<GraphElement> elementsToRemove = new List<GraphElement>();

                foreach (GraphElement element in selection)
                {
                    if (element == null)
                    {
                        continue;
                    }

                    elementsToRemove.Add(element);
                }

                ClearSelection();

                graphViewChanged.Invoke(new GraphViewChange() { elementsToRemove = elementsToRemove });

                // Remove elements
                foreach (GraphElement element in elementsToRemove)
                {
                    RemoveElement(element);
                }
            }

            if (duplicated)
            {
                return;
            }

            TryAddPasteDataToClipboard(pasteData);
        }

        private void TryAddPasteDataToClipboard(PasteData data)
        {
            if (data != null)
            {
                // Set the copy buffer to the json of the string list
                EditorGUIUtility.systemCopyBuffer = JsonUtility.ToJson(data);
            }
        }

        // This is a destructor
        ~GorkGraphView()
        {
            // Destroy dead Search Window object
            Object.DestroyImmediate(_searchWindow);
        }

        public void SaveAsset(bool reimportAsset = true)
        {
            EditorUtility.SetDirty(Graph);

            string assetPath = GraphPath;

            AssetDatabase.SaveAssetIfDirty(AssetDatabase.GUIDFromAssetPath(assetPath));

            if (reimportAsset)
            {
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(Graph), ImportAssetOptions.ForceUpdate);
            }
        }

        public void OnUndoRedo()
        {
            // Do nothing if there is no graph
            if (Graph == null)
            {
                return;
            }

            // Save assets and reimport the graph
            SaveAsset();

            // Reopen the graph to refresh the graph view
            OpenGraph(Graph);
        }

        /// <summary>
        /// Returns the <see cref="GorkNodeView"/> for the <see cref="GorkNode"/>.
        /// </summary>
        public GorkNodeView GetNodeView(GorkNode node)
        {
            if (node == null)
            {
                return null;
            }

            return GetNodeByGuid(node.GUID) as GorkNodeView;
        }

        /// <summary>
        /// Removes all previous elements and opens a new <see cref="GorkGraph"/> to edit.
        /// </summary>
        public void OpenGraph(GorkGraph graph, bool frameAll = false)
        {
            OnOpenGraph?.Invoke(graph);

            // Set the graph variable
            Graph = graph;

            SaveCurrentlyEditingGraph();

            RemoveAllElements();

            // Remove all empty nodes from the graph to ensure no weird things
            Graph.Nodes.RemoveAll(node => node == null);

            // Create all nodes in the GraphView by looping through all of the nodes in the graph
            Graph.Nodes.ForEach(node =>
            {
                // Set graph value
                node.Graph = Graph;

                // Create the node view
                GorkNodeView nodeView = CreateNodeView(node, node.AttributeID);
            });

            // Create all the edges (connections) by looping through all of the nodes in the graph again
            // (we do this after so we already have all the nodes created in the graph)
            Graph.Nodes.ForEach(node =>
            {
                // Return if the connections are (somehow) null
                if (node.OutputConnections == null)
                {
                    return;
                }

                // Get the parent node view
                GorkNodeView parentNodeView = GetNodeView(node);

                // Create connections for the node by looping through all connections on the node
                int portLength = node.OutputConnections.Count;

                for (int portIndex = 0; portIndex < portLength; portIndex++)
                {
                    if (portIndex >= parentNodeView.OutputPorts.Count)
                    {
                        continue;
                    }

                    // Get the parent port
                    GorkPort parentPort = parentNodeView.OutputPorts[portIndex];

                    var port = node.OutputConnections[portIndex];
                    int connectionLength = port.Connections.Count;

                    // Loop through the connections on the port
                    for (int i = 0; i < connectionLength; i++)
                    {
                        var connection = port.Connections[i];

                        if (connection.Node == null)
                        {
                            continue;
                        }

                        // Get the child node view and the child port
                        GorkNodeView childNodeView = GetNodeView(connection.Node);

                        GorkPort childPort = childNodeView.InputPorts[connection.PortIndex];

                        // Connect the parent port to the child port
                        GorkEdge edge = parentPort.GorkConnectTo(childPort);

                        // Add the edge to the graph
                        AddElement(edge);
                    }
                }
            });

            Graph.Nodes.ForEach(node => GetNodeView(node).SetExpanded());

            // Create gork groups
            Graph.GorkGroups.ForEach(groupData =>
            {
                GorkGroup group = new GorkGroup(groupData, this);

                AddElement(group);

                group.Enabled = false;

                foreach (GorkNode node in groupData.Nodes)
                {
                    if (node == null)
                    {
                        continue;
                    }

                    // Add the GorkNodes view to the group
                    group.AddElement(GetNodeView(node));
                }

                group.Enabled = true;
            });

            // Update scroll position and zoom scale
            if (frameAll)
            {
                CustomFrameAll();
            }
            else
            {
                LoadScrollAndZoomData();
            }
        }

        /// <summary>
        /// A custom FrameAll() that's different from <see cref="GraphView.FrameAll"/> by recursively calling itself every frame until it succeeds. <para/>
        /// It also saves it's result to the <see cref="GorkEditorSaveData"/> class upon success.
        /// </summary>
        private void CustomFrameAll()
        {
            void Delay()
            {
                // Unsubscribe this method from the call
                EditorApplication.delayCall -= Delay;
                
                // Woah recursion
                CustomFrameAll();
            }

            Rect rectToFit = CalculateRectToFitAll(contentViewContainer);

            // Things haven't been loaded in yet
            if (float.IsNaN(rectToFit.x) || float.IsNaN(rectToFit.y) || float.IsNaN(rectToFit.width) || float.IsNaN(rectToFit.height))
            {
                // Mark for repaint and delay this entire method by 1 frame using the EditorApplication.delayCall callback
                MarkDirtyRepaint();
                EditorApplication.delayCall += Delay;
                return;
            }

            Vector3 frameTranslation;
            Vector3 frameScaling;
            CalculateFrameTransform(rectToFit, layout, 30, out frameTranslation, out frameScaling);

            viewTransform.position = frameTranslation;

            viewTransform.scale = frameScaling;

            SaveScrollAndZoomData();
        }

        /// <summary>
        /// Removes all elements on this graph
        /// </summary>
        public void RemoveAllElements()
        {
            // Cache the object which we need to remove
            IEnumerable<GraphElement> elementsToRemove = graphElements;

            // Disable all GorkGroup elements so they won't wrongly delete their own data
            foreach (GorkGroup group in elementsToRemove.OfType<GorkGroup>())
            {
                group.Enabled = false;
            }

            // Disconnect all connections on all edges
            foreach (GorkEdge edge in elementsToRemove.OfType<GorkEdge>())
            {
                if (edge.output != null)
                {
                    edge.output.Disconnect(edge);
                }

                if (edge.input != null)
                {
                    edge.input.Disconnect(edge);
                }

                edge.output = null;
                edge.input = null;
            }

            // Lasty, actually remove all of the elements
            foreach (GraphElement element in elementsToRemove)
            {
                RemoveElement(element);
            }
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange change)
        {
            // Check if we have any elements to remove
            if (change.elementsToRemove != null)
            {
                int count = change.elementsToRemove.Count;
                bool deletedNodes = false;

                // Loop through all the elements that are being removed
                for (int i = 0; i < count; i++)
                {
                    GraphElement element = change.elementsToRemove[i];

                    // Remove node
                    GorkNodeView nodeView = element as GorkNodeView;

                    if (nodeView != null)
                    {
                        nodeView.DisconnectAll(true);

                        if (nodeView.Node != null)
                        {
                            deletedNodes = true;

                            GorkNode node = nodeView.Node;

                            Undo.RecordObject(Graph, "Removed Node From List");

                            // Delete node from graph
                            Graph.DeleteNode(node);

                            // Remove the node from the asset
                            Undo.DestroyObjectImmediate(node);

                            Type nodeType = node.GetType();
                            string displayName = nodeView.Attribute == null ? nodeType.Name : nodeView.Attribute.NodeName;
                            Undo.SetCurrentGroupName($"Deleted \"{displayName}\"");
                        }
                    }

                    // Remove edge
                    GorkEdge edge = element as GorkEdge;

                    if (edge != null)
                    {
                        // Get both Gork Ports
                        GorkPort childPort = edge.input as GorkPort;
                        GorkPort parentPort = edge.output as GorkPort;

                        if (childPort != null && parentPort != null)
                        {
                            childPort.Disconnect(edge);
                            parentPort.Disconnect(edge);

                            // Remove the connection from the Graph
                            Graph.RemoveConnection(parentPort.Node, parentPort.PortIndex, childPort.Node, childPort.PortIndex);
                        }
                    }

                    // Remove group
                    GorkGroup group = element as GorkGroup;

                    if (group != null)
                    {
                        // Remove GroupData from graph
                        if (Graph.GorkGroups.Contains(group.GroupData))
                        {
                            Undo.RecordObject(Graph, $"Removed \"{group.Text.value}\" Group");
                            Graph.GorkGroups.Remove(group.GroupData);
                            GorkGroups.Remove(group.GroupData);
                        }
                    }
                }

                if (deletedNodes)
                {
                    SaveAsset();
                }
            }

            // Check if we have any edges to create
            if (change.edgesToCreate != null)
            {
                // Loop through all of the edges to create
                change.edgesToCreate.ForEach(edge =>
                {
                    // Get both Gork Ports
                    GorkPort childPort = edge.input as GorkPort;
                    GorkPort parentPort = edge.output as GorkPort;

                    // Add the connection to the Graph
                    Graph.AddConnection(parentPort.Node, parentPort.PortIndex, childPort.Node, childPort.PortIndex);
                });
            }

            return change;
        }

        #region Saving And Loading Data
        public void SaveScrollAndZoomData()
        {
            GorkEditorSaveData.ScrollPosition = viewTransform.position;
            GorkEditorSaveData.ZoomScale = (viewTransform.scale.x + viewTransform.scale.y) / 2;
        }

        public void SaveMinimapData()
        {
            GorkEditorSaveData.MinimapRect = _miniMap.GetPosition();
            GorkEditorSaveData.MinimapAnchored = _miniMap.anchored;
        }

        public void SaveCurrentlyEditingGraph()
        {
            if (Graph == null)
            {
                GorkEditorSaveData.CurrentlyEditingGraph = "";
                return;
            }

            GorkEditorSaveData.CurrentlyEditingGraph = AssetDatabase.AssetPathToGUID(GraphPath);
        }

        public void LoadScrollAndZoomData()
        {
            // Update scroll position and zoom scale to match save data
            viewTransform.position = GorkEditorSaveData.ScrollPosition;

            float zoomScale = GorkEditorSaveData.ZoomScale;
            viewTransform.scale = new Vector3(zoomScale, zoomScale, 1);
        }
        #endregion

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            DropdownMenu menu = evt.menu;

            // Return if there are already some items in the menu
            if (menu.MenuItems().Count > 0)
            {
                return;
            }

            //base.BuildContextualMenu(evt);

            Vector2 graphMousePos = viewTransform.matrix.inverse.MultiplyPoint(evt.localMousePosition);
            Vector2 guiMousePos = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);

            // Add a few list which we will populate later
            List<GorkNodeView> nodes = null;
            List<GorkEdge> edges = null;
            List<GorkGroup> groups = null;

            // Loop through all the selected elements
            foreach (GraphElement element in selection)
            {
                // Store the element as a GorkNodeView
                GorkNodeView node = element as GorkNodeView;

                // Check if the cast worked
                if (node != null)
                {
                    // Create the node list if it's not null
                    if (nodes == null)
                    {
                        nodes = new List<GorkNodeView>();
                    }

                    // Add the node to the nodes list
                    nodes.Add(node);
                }

                // Store the element as a GorkGroup
                GorkGroup group = element as GorkGroup;

                // Check if the cast worked
                if (group != null)
                {
                    // Create the group list if it's not null
                    if (groups == null)
                    {
                        groups = new List<GorkGroup>();
                    }

                    // Add the group to the groups list
                    groups.Add(group);
                }

                // Don't bother with edges if there are already some selected nodes
                if (nodes != null)
                {
                    continue;
                }

                // Store the element as a GorkEdge
                GorkEdge edge = element as GorkEdge;

                // Check if the cast worked
                if (edge != null)
                {
                    // Create the edge list if it's not null
                    if (edges == null)
                    {
                        edges = new List<GorkEdge>();
                    }

                    // Add the node to the nodes list
                    edges.Add(edge);
                }
            }

            // This will only add a seperator if any of the lists aren't null
            void AddNodeSeperator()
            {
                if (nodes != null || edges != null || groups != null)
                {
                    menu.AppendSeparator();
                }
            }

            // Add seperator
            AddNodeSeperator();

            // Add a toggle to display tags on nodes
            menu.AppendAction("Display Tags", _ =>
            {
                GorkEditorSaveData.DisplayTags = !GorkEditorSaveData.DisplayTags;

                // Update all the nodes in the graph
                foreach (GorkNodeView node in graphElements.OfType<GorkNodeView>())
                {
                    node.UpdateTagSize();
                }

            }, GorkEditorSaveData.DisplayTags ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);

            PasteData pasteData = null;

            // Add options for gork node view
            if (nodes != null)
            {
                bool singleNode = nodes.Count == 1;

                foreach (string tag in Graph.Tags)
                {
                    bool containsTag = true;

                    if (singleNode)
                    {
                        containsTag = nodes[0].Node.Tags.Contains(tag);
                    }
                    else
                    {
                        foreach (GorkNodeView node in nodes)
                        {
                            if (node.Node.Tags.Contains(tag))
                            {
                                continue;
                            }

                            containsTag = false;
                            break;
                        }
                    }

                    menu.AppendAction($"Tags/{tag}", item =>
                    {
                        if (singleNode)
                        {
                            if (containsTag)
                            {
                                nodes[0].RemoveTag(tag);
                            }
                            else
                            {
                                nodes[0].AddTag(tag);
                            }
                            return;
                        }

                        if (containsTag)
                        {
                            nodes.ForEach(n => n.RemoveTag(tag));
                        }
                        else
                        {
                            nodes.ForEach(n => n.AddTag(tag));
                        }
                    }, containsTag ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
                }

                // Add options to remove connection(s)
                menu.AppendAction("Ports/Disconnect Inputs", _ => nodes.ForEach(n => n.DisconnectInputPorts()));
                menu.AppendAction("Ports/Disconnect Outputs", _ => nodes.ForEach(n => n.DisconnectOutputPorts()));
                menu.AppendAction("Ports/Disconnect All", _ => nodes.ForEach(n => n.DisconnectAll()));
                
                // Add a seperator
                menu.AppendSeparator();

                // Add options to delete the gork node(s)
                menu.AppendAction("Delete", _ => nodes.ForEach(n => n.Delete()));

                bool insideGroup = false;

                foreach (GorkNodeView node in nodes)
                {
                    if (!Graph.NodesInGroups.Contains(node.Node))
                    {
                        continue;
                    }

                    insideGroup = true;
                    break;
                }

                // Add option to remove nodes from groups
                menu.AppendAction("Remove from group", _ => nodes.ForEach(n =>
                {
                    Undo.RecordObject(Graph, "Removed Nodes From Group");

                    foreach (GorkNodeView nodeView in nodes)
                    {
                        GorkNode node = nodeView.Node;

                        if (!Graph.NodesInGroups.Contains(node))
                        {
                            continue;
                        }

                        if (!Graph.GetNodeGroup.TryGetValue(node, out GorkGraph.GroupData groupData))
                        {
                            continue;
                        }

                        groupData.Nodes.Remove(node);

                        if (!GorkGroups.TryGetValue(groupData, out GorkGroup group))
                        {
                            continue;
                        }

                        if (!group.containedElements.Contains(nodeView))
                        {
                            continue;
                        }

                        group.RemoveElement(nodeView);
                    }

                }), !insideGroup ? DropdownMenuAction.Status.Disabled : DropdownMenuAction.Status.Normal);

                if (singleNode)
                {
                    nodes[0].AddAttributeContextMenu(menu);
                }
            }
            // Add options for gork edge
            else if (edges != null)
            {
                // Add option to delete edge(s)
                menu.AppendAction("Delete", _ => edges.ForEach(e => e.Delete()));
            }
            else if (groups != null)
            {
                // Add option to delete group(s)
                menu.AppendAction("Delete", _ =>
                {
                    GraphViewChange change = new GraphViewChange() { elementsToRemove = new List<GraphElement>() };

                    groups.ForEach(g => change.elementsToRemove.Add(g));

                    graphViewChanged.Invoke(change);

                    groups.ForEach(g => RemoveElement(g));
                });
            }

            #region Copy Paste Magic
            // Add a seperator
            menu.AppendSeparator();

            bool nothingSelected = nodes == null && groups == null && edges == null;

            // Duplicate
            menu.AppendAction("Duplicate", _ =>
            {
                Duplicate(ref pasteData, nodes, groups, edges);
            }, nothingSelected ? DropdownMenuAction.Status.Disabled : DropdownMenuAction.Status.Normal);

            // Copy
            menu.AppendAction("Copy", _ =>
            {
                CopySelection(ref pasteData, nodes);

                TryAddPasteDataToClipboard(pasteData);
            }, nothingSelected ? DropdownMenuAction.Status.Disabled : DropdownMenuAction.Status.Normal);

            // Cut
            menu.AppendAction("Cut", _ =>
            {
                Cut(ref pasteData, nodes);

                TryAddPasteDataToClipboard(pasteData);
            }, nothingSelected ? DropdownMenuAction.Status.Disabled : DropdownMenuAction.Status.Normal);

            PasteData clipboardData;

            // Try to convert clipboard data to json
            try
            {
                clipboardData = JsonUtility.FromJson<PasteData>(EditorGUIUtility.systemCopyBuffer);
            }
            catch (Exception)
            {
                // Parse failed
                clipboardData = null;
            }

            Vector2 cachedMousePos = MousePos;

            // Paste
            menu.AppendAction("Paste", _ =>
            {
                Paste(cachedMousePos, clipboardData);
            }, clipboardData == null ? DropdownMenuAction.Status.Disabled : DropdownMenuAction.Status.Normal);

            menu.AppendSeparator();
            #endregion

            // Add option to create node
            menu.AppendAction("Create Node", _ =>
            {
                NodeCreationContext context = new NodeCreationContext() { screenMousePosition = guiMousePos };

                _searchWindow.Position = graphMousePos;
                OpenNodeCreationSearchWindow(context);
            });

            // Add option to create a group
            menu.AppendAction("Create Group", _ =>
            {
                // Create new GorkGroup
                GorkGroup group = new GorkGroup("New Group", graphMousePos, this);

                // Add to graph view
                AddElement(group);

                ClearSelection();

                group.OnSelected();
                group.selected = true;
                selection.Add(group);

                group.FocusTitleTextField();

                // Add group data to graph
                Undo.RecordObject(Graph, $"Added New Group");
                Graph.GorkGroups.Add(group.GroupData);

                // Add selected nodes to group
                if (nodes != null)
                {
                    nodes.ForEach(n => group.AddElement(n));
                }
            });
        }

        public override List<Port> GetCompatiblePorts(Port thisPort, NodeAdapter nodeAdapter)
        {
            Node node = thisPort.node;
            Direction direction = thisPort.direction;
            List<Port> compatiblePorts = new List<Port>();
            Type thisType = thisPort.portType;

            foreach (Port port in ports)
            {
                if (port.node == node || port.direction == direction)
                {
                    continue;
                }

                Type type = port.portType;

                // Is not signal?
                if (!thisType.IsSignal())
                {
                    // String type
                    if (type == typeof(string))
                    {
                        compatiblePorts.Add(port);
                        continue;
                    }
                    // Generic c# system.object type
                    else if (type == typeof(object))
                    {
                        compatiblePorts.Add(port);
                        continue;
                    }
                }

                if (thisType != type)
                {
                    if (!GorkConverterAttribute.GorkConvertion.ContainsKey(thisType))
                    {
                        continue;
                    }

                    if (!GorkConverterAttribute.GorkConvertion[thisType].ContainsKey(type))
                    {
                        continue;
                    }
                }

                compatiblePorts.Add(port);
            }

            return compatiblePorts;
        }

        public GorkNodeView CreateNode(Type nodeType, Vector2 position, int attributeID = 0, List<GorkNodeView.FieldData> fieldData = null)
        {
            if (nodeType == null)
            {
                return null;
            }

            GorkNode node = Graph.CreateNode(nodeType);
            node.Position = position;

            return CreateNodeView(node, attributeID, fieldData);
        }

        private GorkNodeView CreateNodeView(GorkNode node, int attributeID = 0, List<GorkNodeView.FieldData> fieldData = null)
        {
            if (node == null)
            {
                return null;
            }

            GorkNodeView nodeView = new GorkNodeView(node, this, attributeID, fieldData);

            AddElement(nodeView);

            return nodeView;
        }

        #region Reading & Writing PasteData
        /// <summary>
        /// Copies all the currently selected <see cref="GorkNodeView"/>, <see cref="GorkGroup"/> and <see cref="GorkEdge"/> and writes them to the <paramref name="pasteData"/>. <para/>
        /// Will also return the currently selected <see cref="GraphElement"/> in case you want to use those.
        /// </summary>
        private IEnumerable<GraphElement> CopySelection(ref PasteData pasteData, IEnumerable<GorkNodeView> nodes = null, IEnumerable<GorkGroup> groups = null, IEnumerable<GorkEdge> edges = null)
        {
            if (pasteData == null)
            {
                pasteData = new PasteData();
            }

            // Create own list if one of provided list is nonexistent
            if (nodes == null)
            {
                nodes = selection.OfType<GorkNodeView>();

                /*
                selectedElements = new List<GraphElement>();

                // Populate the list with our selected objects
                // Also auto fail if we have nothing selected
                if (selection.Count <= 0)
                {
                    return selectedElements;
                }

                // Loop through all selected nodes
                foreach (GraphElement element in selection)
                {
                    GorkNodeView node = element as GorkNodeView;

                    if (node != null)
                    {
                        // Add the node to the list because the cast succeeded
                        selectedElements.Add(node);
                    }
                }
                */
            }

            if (groups == null)
            {
                groups = selection.OfType<GorkGroup>();
            }

            if (edges == null)
            {
                edges = selection.OfType<GorkEdge>();
            }

            // Copy nodes
            if (nodes.Count() > 0)
            {
                // Loop through all of our nodes
                foreach (GorkNodeView node in nodes)
                {
                    // Add paste data
                    pasteData.Nodes.Add(node.GetPasteData());
                }
            }

            // Copy groups
            if (groups.Count() > 0)
            {
                // Loop through all of our nodes
                foreach (GorkGroup group in groups)
                {
                    // Add paste data
                    pasteData.Groups.Add(group.GetPasteData());
                }
            }

            // Copy edges
            if (edges.Count() > 0)
            {
                // Loop through all of our nodes
                foreach (GorkEdge edge in edges)
                {
                    // Add paste data
                    pasteData.Edges.Add(edge.GetPasteData());
                }
            }

            // Join all elements together
            IEnumerable<GraphElement> graphElements = nodes;

            return graphElements.Concat(groups).Concat(edges);
        }

        /// <summary>
        /// Does the exact same thing as <see cref="CopySelection"/> but will also delete all the copied elements.
        /// </summary>
        private void Cut(ref PasteData pasteData, IEnumerable<GorkNodeView> nodes = null, IEnumerable<GorkGroup> groups = null, IEnumerable<GorkEdge> edges = null)
        {
            IEnumerable<GraphElement> elements = CopySelection(ref pasteData, nodes, groups, edges);

            List<GraphElement> elementsToRemove = elements.ToList();
            graphViewChanged.Invoke(new GraphViewChange() { elementsToRemove = elementsToRemove });

            // Remove elements
            foreach (GraphElement element in elementsToRemove)
            {
                RemoveElement(element);
            }

            // Set undo group name
            Undo.SetCurrentGroupName("Cut Objects");
        }

        /// <summary>
        /// Pastes the <see cref="PasteData"/> on the <see cref="EditorGUIUtility.systemCopyBuffer"/>. Will do nothing if the <see cref="PasteData"/> conversion failed.
        /// </summary>
        private void Paste(Vector2? position = null, PasteData data = null, Vector2? offset = null)
        {
            if (data == null)
            {
                // Try to convert to json
                try
                {
                    data = JsonUtility.FromJson<PasteData>(EditorGUIUtility.systemCopyBuffer);
                }
                catch (Exception)
                {
                    return;
                }
            }

            List<GorkNodeView> nodes = new List<GorkNodeView>();
            List<GorkGroup> groups = new List<GorkGroup>();
            List<GorkEdge> edges = new List<GorkEdge>();

            Dictionary<string, GorkNodeView> nodeFromGUID = new Dictionary<string, GorkNodeView>();

            Vector2 meanPosition = Vector2.zero;
            int meanPosCount = 0;

            ClearSelection();

            foreach (GorkNodeView.PasteData nodeData in data.Nodes)
            {
                // Get the type
                Type nodeType = Type.GetType(nodeData.NodeType);

                // Ignore this if getting the type was unsuccessful
                if (nodeType == null)
                {
                    continue;
                }

                // Create a new node
                GorkNodeView nodeView = CreateNode(nodeType, nodeData.Position, nodeData.AttributeID, nodeData.FieldData);
                //nodeView.LoadFromFieldData(nodeData.FieldData);

                nodes.Add(nodeView);
                nodeFromGUID.Add(nodeData.GUID, nodeView);

                // Add to the mean position
                if (position.HasValue)
                {
                    meanPosition += nodeData.Position;
                    meanPosCount++;
                }

                AddToSelection(nodeView);
            }

            SaveAsset(false);

            if (position.HasValue)
            {
                meanPosition /= meanPosCount;
            }

            foreach (GorkNodeView nodeView in nodes)
            {
                if (position.HasValue)
                {
                    nodeView.Node.Position -= meanPosition;
                    nodeView.Node.Position += position.Value;
                }

                if (offset.HasValue)
                {
                    nodeView.Node.Position += offset.Value;
                }

                nodeView.UpdatePosition(nodeView.Node.Position);
            }

            foreach (GorkGroup.PasteData groupData in data.Groups)
            {
                Undo.RecordObject(Graph, "Added Group");
                GorkGraph.GroupData graphGroupData = new GorkGraph.GroupData(groupData.Name, position.HasValue ? groupData.Position - meanPosition + position.Value : groupData.Position);
                Graph.GorkGroups.Add(graphGroupData);

                // Create new GorkGroup
                GorkGroup group = new GorkGroup(graphGroupData, this);

                // Add to graph view
                AddElement(group);

                // Loop through all of our nodes in the groupData
                foreach (string guid in groupData.Nodes)
                {
                    if (!nodeFromGUID.TryGetValue(guid, out GorkNodeView node))
                    {
                        continue;
                    }

                    // Add node to group
                    group.AddElement(node);
                }

                groups.Add(group);

                group.UpdateGeometryFromContent();

                // AddToSelection doesn't work so I have to do this thing instead
                selection.Add(group);
                group.selected = true;
                group.OnSelected();
            }

            bool TryGetPort(GorkNodeView node, int index, bool isInputPort, out GorkPort port)
            {
                if (index < 0)
                {
                    port = null;
                    return false;
                }

                List<GorkPort> ports;

                if (isInputPort)
                {
                    ports = node.InputPorts;
                }
                else
                {
                    ports = node.OutputPorts;
                }

                int length = ports.Count;

                if (index >= length)
                {
                    port = null;
                    return false;
                }

                port = ports[index];
                return true;
            }

            GraphViewChange change = new GraphViewChange();
            change.edgesToCreate = new List<Edge>();

            foreach (GorkEdge.PasteData edgeData in data.Edges)
            {
                if (!nodeFromGUID.TryGetValue(edgeData.Input, out GorkNodeView inputNode))
                {
                    continue;
                }

                if (!nodeFromGUID.TryGetValue(edgeData.Output, out GorkNodeView outputNode))
                {
                    continue;
                }

                if (!TryGetPort(inputNode, edgeData.InputPortIndex, true, out GorkPort inputPort))
                {
                    continue;
                }

                if (!TryGetPort(outputNode, edgeData.OutputPortIndex, false, out GorkPort outputPort))
                {
                    continue;
                }

                GorkEdge edge = inputPort.GorkConnectTo(outputPort);

                AddElement(edge);

                edges.Add(edge);

                change.edgesToCreate.Add(edge);

                // AddToSelection doesn't work so I have to do this thing instead
                selection.Add(edge);
                edge.selected = true;
                edge.OnSelected();
            }

            graphViewChanged.Invoke(change);
            
            Undo.SetCurrentGroupName("Pasted Nodes");
        }

        private void Duplicate(ref PasteData pasteData, IEnumerable<GorkNodeView> nodes = null, IEnumerable<GorkGroup> groups = null, IEnumerable<GorkEdge> edges = null)
        {
            CopySelection(ref pasteData, nodes, groups, edges);

            Paste(null, pasteData, new Vector2(30, 30));
        }

        [Serializable]
        public class PasteData
        {
            public List<GorkNodeView.PasteData> Nodes = new List<GorkNodeView.PasteData>();
            public List<GorkGroup.PasteData> Groups = new List<GorkGroup.PasteData>();
            public List<GorkEdge.PasteData> Edges = new List<GorkEdge.PasteData>();
        }
        #endregion
    }
}
